using AHTB_TimBanCungGu_API.Data;
using AHTB_TimBanCungGu_MVC.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace AHTB_TimBanCungGu_MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly DBAHTBContext _context;
        private readonly ILogger<HomeController> _logger;
        private readonly HttpClient _httpClient;

        public HomeController(DBAHTBContext context, ILogger<HomeController> logger, HttpClient httpClient)
        {
            _context = context;
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<IActionResult> Index()
        {
            // Lấy JWT token từ Session
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

                // Lấy danh sách 6 phim gần nhất từ cơ sở dữ liệu
                var phimList = await _context.Phim
                    .Include(p => p.TheLoai)
                    .Where(p => p.NgayPhatHanh <= DateTime.Now && p.TrangThai != "Ẩn") // Chỉ lấy phim đã phát hành
                    .OrderByDescending(p => p.NgayPhatHanh)    // Sắp xếp theo Ngày Phát Hành (mới nhất ở trên)
                    .Take(6)                                   // Giới hạn 6 phim
                    .ToListAsync();

                return View(phimList);
            }

            return View();
        }



        public async Task<IActionResult> GioiThieuAsync()
        {
            // Lấy JWT token từ Session
            var token = HttpContext.Session.GetString("JwtToken");
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

            if (!string.IsNullOrEmpty(token))
            {
                return View();
            }
            else
            {
                // Nếu không có token, có thể chuyển đến trang đăng nhập
                ViewBag.Message = "Bạn chưa đăng nhập.";
                return RedirectToAction("Login", "LoginvsRegister");
            }
        }
        public IActionResult Logout()
        {
            // Xóa JWT token khỏi Session
            HttpContext.Session.Remove("JwtToken");
            HttpContext.Session.Remove("UserType");
            HttpContext.Session.Remove("TempUserName");
            // Chuyển hướng về trang đăng nhập
            return RedirectToAction("Login", "LoginvsRegister");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
