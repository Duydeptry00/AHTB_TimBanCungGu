using AHTB_TimBanCungGu_API.Data;
using AHTB_TimBanCungGu_API.Models; // Adjust the namespace as necessary
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Required for Include extension method
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AHTB_TimBanCungGu_MVC.Controllers
{
    public class LichSuXemController : Controller
    {
        private readonly DBAHTBContext _context;

        public LichSuXemController(DBAHTBContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var username = HttpContext.Session.GetString("TempUserName");

            // Kiểm tra nếu người dùng chưa đăng nhập
            if (string.IsNullOrEmpty(username))
            {
                // Chuyển hướng đến trang đăng nhập nếu chưa đăng nhập
                return RedirectToAction("Login", "LoginvsRegister");
            }
            // Lấy các bản ghi Lịch Sử Xem mới nhất, bao gồm thông tin Phim, sắp xếp theo thời gian xem giảm dần
            var latestItems = _context.LichSuXem
                .Include(l => l.Phim) // Bao gồm thông tin Phim
                .OrderByDescending(x => x.ThoiGianXem) // Sắp xếp giảm dần theo Thời Gian Xem
                .Take(8) // Lấy 8 phần tử gần nhất
                .ToList(); // Chuyển thành danh sách

            // Trả về view với dữ liệu
            return View(latestItems);
        }
        [HttpPost]
        public IActionResult LuuLichSuXem(string phimId)
        {
            if (string.IsNullOrEmpty(phimId))
            {
                return BadRequest("ID phim không hợp lệ.");
            }

            var username = HttpContext.Session.GetString("TempUserName");

            // Kiểm tra nếu người dùng chưa đăng nhập
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "LoginvsRegister"); // Chuyển hướng đến trang đăng nhập
            }

            // Lấy thông tin người dùng từ username
            var user = _context.Users.FirstOrDefault(u => u.UserName == username);
            if (user == null)
            {
                return NotFound("Người dùng không tồn tại.");
            }

            // Lấy thông tin phim từ ID
            var phim = _context.Phim.FirstOrDefault(p => p.IDPhim == phimId);
            if (phim == null)
            {
                return NotFound("Phim không tồn tại.");
            }

            // Tạo bản ghi lịch sử xem
            var lichSuXem = new LichSuXem
            {
                NguoiDungXem = user.UsID, // Gán UsID của người dùng
                PhimDaXem = phimId,
                ThoiGianXem = DateTime.Now
            };

            try
            {
                _context.LichSuXem.Add(lichSuXem);
                _context.SaveChanges();
                return Ok();  // Trả về phản hồi thành công
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Lỗi khi lưu lịch sử xem: " + ex.Message);
            }
        }

    }
}