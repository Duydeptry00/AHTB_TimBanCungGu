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

namespace AHTB_TimBanCungGu_MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ThongTinCaNhansController : Controller
    {
        private readonly DBAHTBContext _context;

        public ThongTinCaNhansController(DBAHTBContext context)
        {
            _context = context;
        }

        // GET: Admin/ThongTinCaNhans
        public async Task<IActionResult> Index()
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


        [HttpGet]
        public async Task<IActionResult> Search(string searchString)
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
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id, int days = 0, int months = 0, int years = 0, string lyDoKhoa = "")
        {
            Console.WriteLine($"ID nhận được: {id}");

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

            // Thêm logic lưu lịch sử mở khóa và lý do khóa
            if (newStatus == "Không hoạt động")
            {
                var mocThoiGian = DateTime.Now.AddDays(days).AddMonths(months).AddYears(years);
                user.NgayMoKhoa = mocThoiGian;
                user.LyDoKhoa = lyDoKhoa; // Lưu lý do khóa

                var quanLyNguoiDung = new QuanLyNguoiDung
                {
                    AdminID = HttpContext.Session.GetString("AdminId"),
                    NguoiDungID = thongTinCN.UsID,
                    ThaoTac = "Khóa tài khoản",
                    MocThoiGian = DateTime.Now,
                    LichSuMoKhoa = mocThoiGian, // Lưu lịch sử mở khóa
                    LichSuLyDoKhoa = lyDoKhoa // Lưu lý do khóa
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
                    LichSuMoKhoa = DateTime.Now, // Lưu lịch sử mở khóa
                    LichSuLyDoKhoa = null // Không cần lý do mở khóa
                };

                _context.QuanLyNguoiDung.Add(quanLyNguoiDung);
            }

            _context.ThongTinCN.Update(thongTinCN);
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Json(new { success = true, status = newStatus });
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

    }
}