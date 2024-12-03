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
                // Lấy tên người dùng từ Session
                var userName = HttpContext.Session.GetString("TempUserName");

                // Lấy thông tin cá nhân của người dùng
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

                // Lấy danh sách phim mới nhất (trừ phim ẩn)
                var phimMoiNhat = await _context.Phim
                    .Include(p => p.TheLoai)
                    .AsNoTracking()
                    .Where(p => p.NgayPhatHanh <= DateTime.Now && p.TrangThai != "Ẩn")
                    .OrderByDescending(p => p.NgayPhatHanh)
                    .Take(5)
                    .ToListAsync();

                // Lấy danh sách phim thịnh hành (trừ phim ẩn)
                var phimThinhHanh = await _context.LichSuXem
                    .Where(lsx => lsx.Phim.TrangThai != "Ẩn")
                    .GroupBy(lsx => lsx.PhimDaXem)
                    .Select(group => new
                    {
                        PhimId = group.Key,
                        LuotXem = group.Count()
                    })
                    .OrderByDescending(g => g.LuotXem)
                    .Take(5)
                    .Join(_context.Phim, g => g.PhimId, p => p.IDPhim, (g, p) => p)
                    .ToListAsync();

                // Lấy danh sách phim Premium (trừ phim ẩn)
                var phimPremium = await _context.Phim
                    .Include(p => p.TheLoai)
                    .AsNoTracking()
                    .Where(p => p.NoiDungPremium == true && p.TrangThai != "Ẩn")
                    .Take(5)
                    .ToListAsync();

                // Truyền dữ liệu vào ViewBag
                ViewBag.PhimMoiNhat = phimMoiNhat;
                ViewBag.PhimThinhHanh = phimThinhHanh;
                ViewBag.PhimPremium = phimPremium;

                return View();
            }

            return RedirectToAction("Login", "LoginvsRegister");
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
