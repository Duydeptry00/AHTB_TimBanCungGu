using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Net.WebSockets;
using AHTB_TimBanCungGu_API.Data;
using AHTB_TimBanCungGu_API.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using AHTB_TimBanCungGu_MVC.Service;

namespace AHTB_TimBanCungGu_MVC.Controllers
{
    public class TimBanCungGuController : Controller
    {
        private readonly DBAHTBContext _context;
        private readonly IMongoCollection<BsonDocument> _MatchNguoiDung;
        private readonly IMongoCollection<BsonDocument> _ThongBao;
        private readonly IMongoCollection<BsonDocument> _SoLuotVuot;
        private static Dictionary<string, WebSocket> _userWebSockets = new Dictionary<string, WebSocket>();

        public TimBanCungGuController(DBAHTBContext context)
        {
            _context = context;

            // Khởi tạo MongoDB
            var connectionString = "mongodb://localhost:27017";
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase("AHTBdb");
            _MatchNguoiDung = database.GetCollection<BsonDocument>("MatchNguoiDung");
            _ThongBao = database.GetCollection<BsonDocument>("Thongbao");
            _SoLuotVuot = database.GetCollection<BsonDocument>("SoLuotVuot");
        }

        public async Task<IActionResult> TrangChu()
        {
            var token = HttpContext.Session.GetString("JwtToken");

            if (!string.IsNullOrEmpty(token))
            {
                var userName = HttpContext.Session.GetString("TempUserName");

                var userInfo = await _context.ThongTinCN
                    .Include(t => t.User)
                    .FirstOrDefaultAsync(t => t.User.UserName == userName);

                if (userInfo != null)
                {
                    // Truyền thông tin người dùng vào ViewBag
                    ViewBag.HoTen = userInfo.HoTen;
                    ViewBag.GioiTinh = userInfo.GioiTinh;
                    ViewBag.IdThongTinCaNhan = userInfo.IDProfile;
                }

                if (string.IsNullOrEmpty(userName))
                {
                    return Unauthorized(new { success = false, message = "Người dùng chưa đăng nhập." });
                }

                var nguoitimdoituong = await _context.Users.FirstOrDefaultAsync(x => x.UserName == userName);
                if (nguoitimdoituong == null)
                {
                    return NotFound(new { success = false, message = "Người dùng không tồn tại." });
                }

                var userSwipeInfo = await _SoLuotVuot.Find(x => x["Uservuot"] == userName).FirstOrDefaultAsync();
                if (userSwipeInfo != null)
                {
                    ViewBag.SwipeCount = userSwipeInfo["SwipesRemaining"].ToInt32(); // Hoặc ToInt32() để chuyển đổi kiểu nếu cần
                }
                else
                {
                    ViewBag.SwipeCount = 0; // Giá trị mặc định nếu không tìm thấy
                }
                // Retrieve swiped users
                // Lọc những người dùng đã swipe (bao gồm cả Like và Dislike)
                var swipedUsers = await _MatchNguoiDung
                    .Find(x => x["User1"] == userName)
                    .ToListAsync();

                var likedOrDislikedUsernames = swipedUsers
                    .Where(x => x["SwipeAction"] == "Like" || x["SwipeAction"] == "Dislike")
                    .Select(x => x["User2"].ToString())
                    .ToList();

                // Loại bỏ những người dùng đã bị "Dislike"
                var dislikedUsernames = swipedUsers
                    .Where(x => x["SwipeAction"] == "Dislike")
                    .Select(x => x["User2"].ToString())
                    .ToList();

                var dBAHTBContext = _context.ThongTinCN
                    .Include(t => t.User)
                    .Include(t => t.AnhCaNhan)
                    .Where(t =>
                        (t.TrangThai == "Hoạt Động" || t.TrangThai == "Không Hoạt Động") // Lọc theo trạng thái
                        && !likedOrDislikedUsernames.Contains(t.User.UserName) // Loại bỏ người đã swipe
                        && !dislikedUsernames.Contains(t.User.UserName)) // Loại bỏ người đã "Dislike"
                    .AsQueryable();

                // Map user profiles to view model


                var thongTinCaNhanViewModels = await dBAHTBContext.Select(t => new InfoNguoiDung
                {
                    IDProfile = t.IDProfile,
                    UsID = t.UsID,
                    HoTen = t.HoTen,
                    Email = t.Email,
                    GioiTinh = t.GioiTinh,
                    NgaySinh = (DateTime)t.NgaySinh,
                    SoDienThoai = t.SoDienThoai,
                    IsPremium = t.IsPremium,
                    MoTa = t.MoTa,
                    NgayTao = t.NgayTao,
                    TrangThai = t.TrangThai,
                    HinhAnh = t.AnhCaNhan.Select(a => a.HinhAnh).ToList() ?? new List<string>()
                }).Where(x => x.UsID != nguoitimdoituong.UsID).ToListAsync();


                return View(thongTinCaNhanViewModels);
            }
            else
            {
                ViewBag.Message = "Bạn chưa đăng nhập.";
                return RedirectToAction("Login", "LoginvsRegister");
            }
        }



        // GET: TimBanCungGu/Create
        public IActionResult Create()
        {
            // Lấy JWT token từ Session
            var token = HttpContext.Session.GetString("JwtToken");

            if (!string.IsNullOrEmpty(token))
            {
                ViewData["UsID"] = new SelectList(_context.Users, "UsID", "UsID");
                return View();
            }
            else
            {
                // Nếu không có token, có thể chuyển đến trang đăng nhập
                ViewBag.Message = "Bạn chưa đăng nhập.";
                return RedirectToAction("Login", "LoginvsRegister");
            }

        }


        // Kết nối WebSocket
        [HttpGet]
        public async Task<IActionResult> ConnectWebSocket()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                var userName = HttpContext.Session.GetString("TempUserName");
                if (string.IsNullOrEmpty(userName))
                {
                    return Unauthorized();
                }

                var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                _userWebSockets[userName] = webSocket;

                await HandleWebSocketConnection(userName, webSocket);

                return Ok();
            }
            else
            {
                return BadRequest(new { message = "Yêu cầu không phải WebSocket." });
            }
        }

        // Xử lý WebSocket
        private async Task HandleWebSocketConnection(string userName, WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            while (!result.CloseStatus.HasValue)
            {
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                Console.WriteLine($"Received from {userName}: {message}");

                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }

            _userWebSockets.Remove(userName);
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }

        [HttpPost]
        public async Task<IActionResult> LuuSwipeAction([FromBody] SwipeRequest request)
        {
            if (string.IsNullOrEmpty(request.UserId2) || string.IsNullOrEmpty(request.Action))
            {
                return BadRequest(new { success = false, message = "Dữ liệu đầu vào không hợp lệ." });
            }

            try
            { 
                int idUser2 = int.Parse(request.UserId2);
                var idDoiTuong = await _context.ThongTinCN.FirstOrDefaultAsync(x => x.IDProfile == idUser2);
                if (idDoiTuong == null)
                {
                    return NotFound(new { success = false, message = "Người dùng không tồn tại." });
                }

                var doiTuong = await _context.Users.FirstOrDefaultAsync(x => x.UsID == idDoiTuong.UsID);
                var userName = HttpContext.Session.GetString("TempUserName");
          
                if (string.IsNullOrEmpty(userName))
                {
                    return Unauthorized(new { success = false, message = "Người dùng chưa đăng nhập." });
                }

                var nguoiTimDoiTuong = await _context.Users.FirstOrDefaultAsync(x => x.UserName == userName);
                if (nguoiTimDoiTuong == null)
                {
                    return NotFound(new { success = false, message = "Người dùng không tồn tại." });
                }
                var premium = _context.ThongTinCN.FirstOrDefault(x => x.UsID == nguoiTimDoiTuong.UsID);
                if (premium.IsPremium == false) 
                {
                    var countSwipService = new CountSwipService();
                    await countSwipService.GiamLuotVuot(userName);
                }



                // Lưu thông tin swipe vào MongoDB từ người A
                var swipeAction = new BsonDocument
        {
            { "IDSwipe", Guid.NewGuid().ToString() },
            { "User1", nguoiTimDoiTuong.UserName },
            { "User2", doiTuong.UserName },
            { "SwipeAction", request.Action },
            { "SwipedAt", DateTime.UtcNow }
        };

                await _MatchNguoiDung.InsertOneAsync(swipeAction);
                if (request.Action == "Dislike")
                {
                    var filter = Builders<BsonDocument>.Filter.And(
                        Builders<BsonDocument>.Filter.Eq("User1", nguoiTimDoiTuong.UserName),
                        Builders<BsonDocument>.Filter.Eq("User2", doiTuong.UserName)
                    );
                    var update = Builders<BsonDocument>.Update.Set("SwipeAction", "Dislike");
                    await _MatchNguoiDung.UpdateOneAsync(filter, update);
                }


                // Kiểm tra xem người A đã swipe "Like" với người B chưa
                var matchNguoiA = await _MatchNguoiDung
                    .Find(x => x["User1"] == nguoiTimDoiTuong.UserName && x["User2"] == doiTuong.UserName && x["SwipeAction"] == "Like")
                    .FirstOrDefaultAsync();

                // Kiểm tra xem người B đã swipe "Like" với người A chưa
                var matchNguoiB = await _MatchNguoiDung
                    .Find(x => x["User1"] == doiTuong.UserName && x["User2"] == nguoiTimDoiTuong.UserName && x["SwipeAction"] == "Like")
                    .FirstOrDefaultAsync();

                if (matchNguoiA != null && matchNguoiB != null)
                {
                    // Nếu cả hai người đều đã swipe "Like", tạo thông báo cho cả hai người
                    var hoTenNguoiGui = await _context.ThongTinCN
                        .Where(x => x.UsID == nguoiTimDoiTuong.UsID)
                        .Select(x => x.HoTen)
                        .FirstOrDefaultAsync();

                    var hoTenNguoiNhan = await _context.ThongTinCN
                        .Where(x => x.UsID == doiTuong.UsID)
                        .Select(x => x.HoTen)
                        .FirstOrDefaultAsync();

                    // Thông báo cho người A rằng họ đã matched với người B
                    var thongBaoA = new BsonDocument
            {
                { "NguoiGui", nguoiTimDoiTuong.UserName },
                { "NguoiNhan", doiTuong.UserName },
                { "NoiDung", $" đã matched với bạn!" },
                { "ThoiGian", DateTime.UtcNow },
                { "Read", false }
            };

                    await _ThongBao.InsertOneAsync(thongBaoA);

                    // Thông báo cho người B rằng họ đã matched với người A
                    var thongBaoB = new BsonDocument
            {
                { "NguoiGui", doiTuong.UserName },
                { "NguoiNhan", nguoiTimDoiTuong.UserName },
                { "NoiDung", $" đã matched với bạn!" },
                { "ThoiGian", DateTime.UtcNow },
                { "Read", false }
            };

                    await _ThongBao.InsertOneAsync(thongBaoB);

                    // Gửi thông báo qua WebSocket cho người A (nếu người A đang online)
                    if (_userWebSockets.TryGetValue(nguoiTimDoiTuong.UserName, out var webSocketA))
                    {
                        var messageA = new
                        {
                            type = "match",
                            hoTen = hoTenNguoiNhan,  // Gửi tên đầy đủ của người A
                        };
                        var jsonMessage = JsonConvert.SerializeObject(messageA);
                        var bufferA = Encoding.UTF8.GetBytes(jsonMessage);
                        await webSocketA.SendAsync(new ArraySegment<byte>(bufferA), WebSocketMessageType.Text, true, CancellationToken.None);
                    }

                    // Gửi thông báo qua WebSocket cho người B (nếu người B đang online)
                    if (_userWebSockets.TryGetValue(doiTuong.UserName, out var webSocketB))
                    {
                        var messageB = new
                        {
                            type = "match",
                            hoTen = hoTenNguoiGui,  // Gửi tên đầy đủ của người B
                        };
                        var jsonMessage = JsonConvert.SerializeObject(messageB);
                        var bufferB = Encoding.UTF8.GetBytes(jsonMessage);
                        await webSocketB.SendAsync(new ArraySegment<byte>(bufferB), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                }

                return Ok(new { success = true, message = "Đã lưu hành động swipe." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Lỗi hệ thống: {ex.Message}" });
            }
        }


        public class SwipeRequest
        {
            public string UserId2 { get; set; }
            public string Action { get; set; }
        }
        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            var userName = HttpContext.Session.GetString("TempUserName");
            if (string.IsNullOrEmpty(userName))
            {
                return Unauthorized(new { success = false, message = "Người dùng chưa đăng nhập." });
            }

            // Lấy thông tin UsID của người dùng hiện tại
            var currentUser = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == userName);

            if (currentUser == null)
            {
                return NotFound(new { success = false, message = "Người dùng không tồn tại." });
            }

            var userId = currentUser.UsID;

            // Lấy thông báo
            var notifications = await _ThongBao
      .Find(x => x["NguoiNhan"] == userName)
      .Sort(Builders<BsonDocument>.Sort.Ascending("ThoiGian"))
      .ToListAsync();


            // Lấy danh sách UserName của NguoiGui từ thông báo
            var senderUserNames = notifications.Select(x => x["NguoiGui"].ToString()).ToList();

            // Lấy ThongTinCN của những người gửi có UserName trong danh sách
            var senders = await _context.Users
                .Where(t => senderUserNames.Contains(t.UserName))
                .Select(t => new { t.UserName, t.ThongTinCN })
                .ToListAsync();

            // Đếm thông báo chưa đọc
            var unreadCount = notifications.Count(x => x["Read"].ToBoolean() == false);

            // Chuẩn bị kết quả trả về
            var result = notifications.Select(x => new
            {
                id = x["_id"].ToString(),
                sender = senders
           .FirstOrDefault(t => t.UserName == x["NguoiGui"].ToString())?.ThongTinCN?.HoTen, // Lấy tên người gửi từ ThongTinCN
                text = x["NoiDung"].ToString(),
                time = x["ThoiGian"].ToUniversalTime().ToString("o"), // ISO 8601 format (UTC)
                read = x["Read"]?.ToBoolean() ?? false
            }).ToList();



            return Json(new { success = true, notifications = result, unreadCount });
        }


        [HttpPost]
        public async Task<IActionResult> MarkAllNotificationsAsRead()
        {
            var userName = HttpContext.Session.GetString("TempUserName");
            if (string.IsNullOrEmpty(userName))
            {
                return Unauthorized(new { success = false, message = "Người dùng chưa đăng nhập." });
            }

            var filter = Builders<BsonDocument>.Filter.Eq("NguoiNhan", userName);
            var update = Builders<BsonDocument>.Update.Set("Read", true);

            var result = await _ThongBao.UpdateManyAsync(filter, update);

            if (result.ModifiedCount > 0)
            {
                return Ok(new { success = true, message = "Tất cả thông báo đã được đánh dấu là đã đọc." });
            }
            else
            {
                return BadRequest(new { success = false, message = "Không có thông báo nào cần cập nhật." });
            }
        }
        public async Task<IActionResult> DanhSachNguoiThich()
        {
            // Lấy JWT token từ Session
            var token = HttpContext.Session.GetString("JwtToken");

            if (string.IsNullOrEmpty(token))
            {
                ViewBag.Message = "Bạn chưa đăng nhập.";
                return RedirectToAction("Login", "LoginvsRegister");
            }

            // Lấy tên người dùng đang đăng nhập từ Session
            var userNameLogged = HttpContext.Session.GetString("TempUserName");
            if (string.IsNullOrEmpty(userNameLogged))
            {
                return Unauthorized(new { success = false, message = "Người dùng chưa đăng nhập." });
            }

            // Kiểm tra xem người dùng có phải là Premium không
            var userInfo = await _context.ThongTinCN
                .Include(t => t.User)
                .Where(t => t.User.UserName == userNameLogged)
                .FirstOrDefaultAsync();

            if (userInfo == null || !userInfo.IsPremium)
            {
                // Lưu thông báo vào TempData và chuyển hướng về trang chủ
                TempData["Message"] = "Bạn phải là người dùng Premium để vào trang danh sách người thích.";
                return RedirectToAction("TrangChu", "TimBanCungGu");
            }

            // Truy vấn MongoDB: Tìm tất cả những ai đã "Like" người dùng hiện tại
            var likedUsers = await _MatchNguoiDung
                .Find(x => x["User2"] == userNameLogged && x["SwipeAction"] == "Like")
                .ToListAsync();

            if (likedUsers.Count == 0)
            {
                ViewBag.Message = "Không có người nào thích bạn.";
                return View(new List<InfoNguoiDung>()); // Trả về view trống
            }

            // Lấy danh sách tên người dùng đã thích
            var userNames = likedUsers.Select(x => x["User1"].ToString()).ToList();

            // Lấy danh sách người dùng mà tài khoản hiện tại đã "Like" hoặc "Dislike"
            var actionsByCurrentUser = await _MatchNguoiDung
                .Find(x => x["User1"] == userNameLogged)
                .ToListAsync();

            var excludedUserNames = actionsByCurrentUser
                .Where(x => x["SwipeAction"] == "Like" || x["SwipeAction"] == "Dislike")
                .Select(x => x["User2"].ToString())
                .ToHashSet(); // Sử dụng HashSet để loại bỏ trùng lặp và tăng hiệu suất tìm kiếm

            // Loại bỏ những người dùng đã được "Like" hoặc "Dislike" bởi người dùng hiện tại
            userNames = userNames.Except(excludedUserNames).ToList();

            if (userNames.Count == 0)
            {
                ViewBag.Message = "Không có người nào thích bạn.";
                return View(new List<InfoNguoiDung>()); // Trả về view trống
            }

            // Truy vấn danh sách thông tin người dùng từ SQL Server
            var likedUserDetails = await _context.ThongTinCN
                .Include(t => t.User)
                .Include(t => t.AnhCaNhan)
                .Where(t => userNames.Contains(t.User.UserName))
                .Select(t => new InfoNguoiDung
                {
                    IDProfile = t.IDProfile,
                    UsID = t.UsID,
                    HoTen = t.HoTen,
                    Email = t.Email,
                    GioiTinh = t.GioiTinh,
                    NgaySinh = (DateTime)t.NgaySinh,
                    SoDienThoai = t.SoDienThoai,
                    IsPremium = t.IsPremium,
                    MoTa = t.MoTa,
                    NgayTao = t.NgayTao,
                    TrangThai = t.TrangThai,
                    HinhAnh = t.AnhCaNhan.Select(a => a.HinhAnh).ToList() ?? new List<string>()
                })
                .ToListAsync();

            return View(likedUserDetails); // Trả về view với danh sách thông tin người đã thích bạn
        }


    }
}
