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
using System.Threading.Tasks;

namespace AHTB_TimBanCungGu_MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly DBAHTBContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(DBAHTBContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string username)
        {

            var phimList = await _context.Phim
                .Include(p => p.TheLoai)  // Bao gồm thông tin của bảng TheLoai
                .OrderByDescending(p => p.NgayCapNhat) // Sắp xếp phim theo ngày cập nhật mới nhất
                .Take(6) // Lấy 6 phim mới nhất
                .ToListAsync(); // Trả về danh sách

            var PhimLe = _context.Phim.Include(p => p.TheLoai).Where(p =>p.DangPhim == "Phim lẻ").ToList();
            ViewBag.PhimLe = PhimLe;
            var PhimBo = _context.Phim.Include(p => p.TheLoai).Where(p => p.DangPhim == "Phim bộ").ToList();
            ViewBag.PhimBo = PhimBo;
            return View(phimList);
        }
        public IActionResult GioiThieu()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
