using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using AHTB_TimBanCungGu_API.Models;
using System.Threading.Tasks;
using AHTB_TimBanCungGu_API.Data;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace AHTB_TimBanCungGu_MVC.Controllers
{
    public class PhimYeuThichController : Controller
    {
        private readonly DBAHTBContext _context;

        public PhimYeuThichController(DBAHTBContext context)
        {
            _context = context;
        }

        // Action để hiển thị danh sách phim yêu thích
        public async Task<IActionResult> Index()
        {
            // Lấy JWT token từ Session
            var token = HttpContext.Session.GetString("JwtToken");

            if (!string.IsNullOrEmpty(token))
            {
                // Lấy tên tài khoản từ Session
                var username = HttpContext.Session.GetString("TempUserName");
                // Kiểm tra nếu người dùng chưa đăng nhập
                if (string.IsNullOrEmpty(username))
                {
                    return RedirectToAction("Login", "LoginvsRegister"); // Chuyển hướng đến trang đăng nhập
                }
               

                var userInfo = await _context.ThongTinCN
                    .Include(t => t.User)
                    .FirstOrDefaultAsync(t => t.User.UserName == username);

                if (userInfo != null)
                {
                    // Truyền thông tin người dùng vào ViewBag
                    ViewBag.HoTen = userInfo.HoTen;
                    ViewBag.GioiTinh = userInfo.GioiTinh;
                    ViewBag.IdThongTinCaNhan = userInfo.IDProfile;
                }
                // Lấy thông tin người dùng từ username
                var user = _context.Users.FirstOrDefault(u => u.UserName == username);
                // Lấy danh sách phim yêu thích của người dùng
                var phimYeuThichList = await _context.PhimYeuThich
                    .Where(p => p.NguoiDungYT == user.UsID) // Lọc theo UsID của người dùng
                    .Include(p => p.Phim)
                     .Where(p => p.Phim.TrangThai != "Ẩn")
                    .Select(p => new PhimYeuThichViewModel
                    {
                        IdPhim = p.Phim.IDPhim,
                        TenPhim = p.Phim.TenPhim,
                        HinhAnh = p.Phim.HinhAnh
                    })
                    .ToListAsync();

                return View(phimYeuThichList);
            }
            else
            {
                // Nếu không có token, có thể chuyển đến trang đăng nhập
                ViewBag.Message = "Bạn chưa đăng nhập.";
                return RedirectToAction("Login", "LoginvsRegister");
            }

        }
        // ViewModel để hiển thị thông tin phim yêu thích
        public class PhimYeuThichViewModel
        {
            public string IdPhim { get; set; }
            public string TenPhim { get; set; }
            public string HinhAnh { get; set; }
        }
    }
}
