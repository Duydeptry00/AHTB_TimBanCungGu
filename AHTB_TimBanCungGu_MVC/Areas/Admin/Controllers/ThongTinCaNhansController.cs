using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AHTB_TimBanCungGu_API.Data;
using AHTB_TimBanCungGu_API.Models;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Net.WebSockets;
using System.Text;
using System.Threading;


namespace AHTB_TimBanCungGu_MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ThongTinCaNhansController : Controller
    {
        private readonly DBAHTBContext _context;
        private readonly IMongoCollection<BsonDocument> _notifications;
        private static Dictionary<string, WebSocket> _userWebSockets = new Dictionary<string, WebSocket>();


        public ThongTinCaNhansController(DBAHTBContext context)
        {
            _context = context;
            var connectionString = "mongodb://localhost:27017"; // URI của MongoDB
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase("AHTBdb"); // Tên database của bạn
            _notifications = database.GetCollection<BsonDocument>("AHTBCollection"); // Tên collection của bạn
        }

        // GET: Admin/ThongTinCaNhans
        public async Task<IActionResult> Index()
        {
            // Lấy token JWT và UserType từ session
            var token = HttpContext.Session.GetString("JwtToken");
            var userType = HttpContext.Session.GetString("UserType");

            if (userType == "Admin" && token != null)
            {
                var dBAHTBContext = _context.ThongTinCN.Include(t => t.User);

                // Lấy danh sách tất cả thông tin cá nhân
                var thongTinCaNhanList = await dBAHTBContext.ToListAsync();

                // Lấy danh sách người dùng có ngày mở khóa đã đến hoặc đã qua
                var usersToUpdate = thongTinCaNhanList
                    .Select(t => t.User)
                    .Where(u => u.NgayMoKhoa != null && u.NgayMoKhoa <= DateTime.Now)
                    .ToList();

                // Cập nhật trạng thái cho từng người dùng và thông tin cá nhân
                foreach (var user in usersToUpdate)
                {
                    user.TrangThai = "Hoạt động"; // Đặt trạng thái thành "Hoạt động"
                    user.NgayMoKhoa = DateTime.Now; // Cập nhật ngày mở khóa thành ngày hiện tại

                    // Cập nhật trạng thái của người dùng
                    _context.Users.Update(user);

                    // Cập nhật trạng thái tương ứng trong bảng ThongTinCN
                    var thongTinCN = thongTinCaNhanList.FirstOrDefault(t => t.UsID == user.UsID);
                    if (thongTinCN != null)
                    {
                        thongTinCN.TrangThai = "Hoạt động"; // Đặt lại trạng thái thành "Hoạt động"
                        _context.ThongTinCN.Update(thongTinCN);
                    }
                }

                await _context.SaveChangesAsync(); // Lưu các thay đổi vào cơ sở dữ liệu

                return View(thongTinCaNhanList);
            }

            return NotFound();
       
        }


        [HttpGet]
        public async Task<IActionResult> Search(string searchString)
        {
            // Lấy token JWT và UserType từ session
            var token = HttpContext.Session.GetString("JwtToken");
            var userType = HttpContext.Session.GetString("UserType");

            if (userType == "Admin" && token != null)
            {
                ViewData["CurrentFilter"] = searchString;

                // Truy vấn dữ liệu từ bảng ThongTinCN và include User để lấy thông tin liên quan
                var profileInfos = from p in _context.ThongTinCN.Include(p => p.User)
                                   select p;

                // Nếu searchString không trống, tìm kiếm theo các trường hợp
                if (!String.IsNullOrEmpty(searchString))
                {
                    profileInfos = profileInfos.Where(p =>
                        p.User.UserName.Contains(searchString) || // Tìm kiếm theo UserName
                        p.HoTen.Contains(searchString) ||         // Tìm kiếm theo Họ Tên
                        p.SoDienThoai.Contains(searchString));    // Tìm kiếm theo Số Điện Thoại
                }

                // Trả về view Index với kết quả tìm kiếm
                return View("Index", await profileInfos.ToListAsync());
            }

            return NotFound();
           
        }
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id, int days = 0, int months = 0, int years = 0, string lyDoKhoa = "")
        {
            var thongTinCN = await _context.ThongTinCN.FindAsync(id);
            if (thongTinCN == null)
            {
                return Json(new { success = false, message = "Người dùng không tồn tại." });
            }

            var user = await _context.Users.FindAsync(thongTinCN.UsID);
            if (user == null)
            {
                return Json(new { success = false, message = "Người dùng không tồn tại trong bảng User." });
            }

            var newStatus = thongTinCN.TrangThai == "Hoạt động" ? "Không hoạt động" : "Hoạt động";
            thongTinCN.TrangThai = newStatus;

            string notificationMessage = ""; 

            if (newStatus == "Không hoạt động")
            {
                var mocThoiGian = DateTime.Now.AddDays(days).AddMonths(months).AddYears(years);
                user.NgayMoKhoa = mocThoiGian;
                user.LyDoKhoa = lyDoKhoa;

                // Tạo thông báo
                notificationMessage = $"Tài khoản {user.UserName} đã bị khóa. Ngày mở khóa: {mocThoiGian:dd/MM/yyyy}. Lý do: {lyDoKhoa}";

                if (_userWebSockets.ContainsKey(user.UserName))
                {
                    await SendNotificationToWebSocket(user.UserName, notificationMessage);
                }

                // Lưu thông báo vào MongoDB
                var notification = new BsonDocument
                {
                    { "UserId", user.UsID },
                    { "Message", notificationMessage },
                    { "Timestamp", DateTime.Now },
                    { "LyDoKhoa", lyDoKhoa }, // Thêm lý do khóa vào document
                    { "NgayMoKhoa", mocThoiGian } // Thêm ngày mở khóa vào document
                };
                await _notifications.InsertOneAsync(notification);

                // Tạo một đối tượng quản lý người dùng
                var quanLyNguoiDung = new QuanLyNguoiDung
                {
                    AdminID = HttpContext.Session.GetString("AdminId"),
                    NguoiDungID = thongTinCN.UsID,
                    ThaoTac = "Khóa tài khoản",
                    MocThoiGian = DateTime.Now,
                    LichSuMoKhoa = mocThoiGian,
                    LichSuLyDoKhoa = lyDoKhoa
                };

                _context.QuanLyNguoiDung.Add(quanLyNguoiDung);
            }
            else
            {
                user.NgayMoKhoa = DateTime.Now;
                user.LyDoKhoa = null;

                var quanLyNguoiDung = new QuanLyNguoiDung
                {
                    AdminID = HttpContext.Session.GetString("AdminId"),
                    NguoiDungID = thongTinCN.UsID,
                    ThaoTac = "Mở tài khoản",
                    LichSuMoKhoa = DateTime.Now,
                    LichSuLyDoKhoa = null
                };

                _context.QuanLyNguoiDung.Add(quanLyNguoiDung);
            }

            _context.ThongTinCN.Update(thongTinCN);
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            // Trả về thông báo thành công chỉ khi đã cập nhật
            HttpContext.Session.Clear();

            return Json(new { success = true, status = newStatus, redirectToLogin = "/LoginvsRegister/Login" });
        }

        public IActionResult TestLogin()
        {
            // Tạo thông tin người dùng tạm thời cho admin
            var userId = "1"; // ID của admin trong cơ sở dữ liệu
            var userName = "admin"; // Tên người dùng
            var password = "123"; // Mật khẩu

            // Kiểm tra xem người dùng có tồn tại trong cơ sở dữ liệu không
            var user = _context.Users.FirstOrDefault(u => u.UsID == userId && u.UserName == userName && u.Password == password);
            if (user != null)
            {
                // Lưu thông tin người dùng vào session (hoặc cookie) để giả lập việc đăng nhập
                HttpContext.Session.SetString("UserId", user.UsID);
                HttpContext.Session.SetString("UserName", user.UserName);

                // Lưu ID admin vào session
                HttpContext.Session.SetString("AdminId", user.UsID); // Sử dụng user.UsID của admin

                // Chuyển hướng đến trang quản trị
                return RedirectToAction("Index", "ThongTinCaNhans", new { area = "Admin" });
            }

            return Content("Đăng nhập không thành công. Vui lòng kiểm tra thông tin tài khoản.");
        }
        [HttpGet]
        public async Task<IActionResult> ConnectWebSocket()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                var userName = HttpContext.Session.GetString("TempUserName"); // Lấy TempUserName từ session

                if (!string.IsNullOrEmpty(userName))
                {
                    // Sử dụng TempUserName làm key để lưu kết nối WebSocket
                    _userWebSockets[userName] = webSocket;
                }
                else
                {
                    // Trả về lỗi nếu TempUserName không có trong session
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "TempUserName không có trong session", CancellationToken.None);
                    return BadRequest("TempUserName không có trong session.");
                }

                var buffer = new byte[1024 * 4];
                while (webSocket.State == WebSocketState.Open)
                {
                    await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                }

                // Xóa kết nối khi người dùng ngắt kết nối
                if (!string.IsNullOrEmpty(userName))
                {
                    _userWebSockets.Remove(userName);
                }
            }
            return BadRequest();
        }

        private async Task SendNotificationToWebSocket(string userName, string message)
        {
            if (_userWebSockets.TryGetValue(userName, out var webSocket) && webSocket.State == WebSocketState.Open)
            {
                var buffer = Encoding.UTF8.GetBytes(message);
                var segment = new ArraySegment<byte>(buffer);

                try
                {
                    await webSocket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
                    Console.WriteLine($"Sent message to user {userName}: {message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending message to user {userName}: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine($"WebSocket is not open for user {userName}");
            }
        }
    }
}