﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AHTB_TimBanCungGu_API.Models;
using AHTB_TimBanCungGu_API.Data;
using Microsoft.AspNetCore.Http;

namespace HeThongChieuPhimAHTB_TimBanCungGu_MVC.Controllers
{
    public class PhimController : Controller
    {
        private readonly DBAHTBContext _context;

        public PhimController(DBAHTBContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> XemPhim([FromQuery] string idPhim)
        {
           
            // Truy vấn thông tin phim từ bảng Phim
            var phim = await _context.Phim
                .Include(p => p.TheLoai) // Bao gồm thông tin thể loại của phim
                .FirstOrDefaultAsync(p => p.IDPhim == idPhim);

            if (phim == null)
            {
                return NotFound();
            }

            // Nếu là Phim Lẻ, không cần truy vấn thêm Phan, chỉ cần chuyển hướng tới action PhimLe
            if (string.Equals(phim.DangPhim, "Phim Lẻ", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("PhimLe", "Phim", new { idPhim });
            }
            else
            {
                // Nếu là Phim Bộ, cần truy vấn Phan để lấy thông tin phân đoạn
                var phan = await _context.Phan
                    .Include(p => p.Phim) // Bao gồm thông tin phim
                    .ThenInclude(p => p.TheLoai) // Bao gồm thông tin thể loại của phim
                    .FirstOrDefaultAsync(p => p.Phim.IDPhim == idPhim);
                if (phan == null)
                {
                    return NotFound();
                }

                return RedirectToAction("PhimBo", "Phim", new { idPhim });
            }
        }

        public async Task<IActionResult> PhimLe(string idPhim)
        {
            var phimLe = await _context.Phim
                .Include(p => p.TheLoai)
                .FirstOrDefaultAsync(p => p.IDPhim == idPhim && p.DangPhim.ToLower() == "phim lẻ".ToLower());

            if (phimLe == null)
            {
                return NotFound();
            }

            var phimDeCu = await _context.Phim
                .Where(p => p.TheLoai.TenTheLoai == phimLe.TheLoai.TenTheLoai)
                .Where(p => p.IDPhim != phimLe.IDPhim)
                .ToListAsync();

            ViewBag.PhimLe = phimLe;
            ViewBag.PhimBo = null;
            ViewBag.Phan = null;
            ViewBag.PhimDeCu = phimDeCu;

            return View("XemPhim", phimLe);
        }
        public async Task<IActionResult> PhimBo(string idPhim)
        {
            var phan = await _context.Phan
                .Include(p => p.Phim)
                .ThenInclude(p => p.TheLoai)
                .FirstOrDefaultAsync(p => p.Phim.IDPhim == idPhim);

            if (phan == null)
            {
                return NotFound();
            }

            var phimBo = await _context.Phim
                .Include(p => p.TheLoai)
                .FirstOrDefaultAsync(p => p.IDPhim == idPhim && p.DangPhim == "Phim Bộ");

            var phimDeCu = await _context.Phim
                .Where(p => p.TheLoai.TenTheLoai == phan.Phim.TheLoai.TenTheLoai)
                .Where(p => p.IDPhim != phan.Phim.IDPhim)
                .ToListAsync();

            ViewBag.Phan = phan;
            ViewBag.PhimBo = phimBo;
            ViewBag.PhimLe = null;
            ViewBag.PhimDeCu = phimDeCu;

            return View("XemPhim", phimBo);
        }

        [HttpPost]
        public IActionResult ToggleFavorite(string movieId)
        {
            var username = HttpContext.Session.GetString("TempUserName");

            // Kiểm tra nếu người dùng chưa đăng nhập
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "LoginvsRegister");
            }

            // Kiểm tra xem người dùng có tồn tại trong bảng Users
            var user = _context.Users.FirstOrDefault(u => u.UserName == username);
            if (user == null)
            {
                TempData["Message"] = "Người dùng không tồn tại!";
                return RedirectToAction("Login", "LoginvsRegister");
            }

            // Kiểm tra nếu phim đã có trong danh sách yêu thích
            var favorite = _context.PhimYeuThich
                .FirstOrDefault(py => py.NguoiDungYT == user.UsID && py.PhimYT == movieId);  // So sánh với UsID

            if (favorite != null)
            {
                // Nếu có trong danh sách yêu thích, xóa
                _context.PhimYeuThich.Remove(favorite);
                _context.SaveChanges();
                TempData["Message"] = "Phim đã được xóa khỏi danh sách yêu thích!";
            }
            else
            {
                // Nếu chưa có, thêm vào danh sách yêu thích
                var newFavorite = new PhimYeuThich
                {
                    NguoiDungYT = user.UsID,  // Sử dụng UsID thay vì username
                    PhimYT = movieId
                };
                _context.PhimYeuThich.Add(newFavorite);
                _context.SaveChanges();
                TempData["Message"] = "Phim đã được thêm vào yêu thích!";
            }

            // Truyền thông tin trạng thái yêu thích về View
            var isFavorite = _context.PhimYeuThich.Any(py => py.NguoiDungYT == user.UsID && py.PhimYT == movieId);
            ViewBag.IsFavorite = isFavorite;

            // Chuyển hướng về chi tiết phim
            return RedirectToAction("ChiTietPhim", new { id = movieId });
        }





        public IActionResult ChiTietPhim(string id)
        {
            var username = HttpContext.Session.GetString("TempUserName");

            var movie = _context.Phim
                .Include(p => p.Phan)
                .Include(p => p.TheLoai)
                .FirstOrDefault(p => p.IDPhim == id);

            if (movie == null)
            {
                return NotFound();
            }

            var isSingleMovie = movie.DangPhim.Trim().Equals("Phim Lẻ", StringComparison.OrdinalIgnoreCase);

            var danhSachPhan = _context.Phan
                                .Where(p => p.PhimID == id)
                                .OrderBy(p => p.SoPhan)
                                .ToList();

            var phimDeCu = _context.Phim
                .Include(p => p.TheLoai)
                .Include(p => p.Phan)
                .Where(p => p.TheLoai.TenTheLoai == movie.TheLoai.TenTheLoai && p.IDPhim != movie.IDPhim)
                .ToList();
            var user = _context.Users.FirstOrDefault(u => u.UserName == username);
            if (user == null)
            {
                return RedirectToAction("Login", "LoginvsRegister");
            }

            var isFavorite = _context.PhimYeuThich
                .Any(py => py.NguoiDungYT == user.UsID && py.PhimYT == id);

            ViewBag.PhimDeCu = phimDeCu;
            ViewBag.PhimLe = movie;
            ViewBag.DanhSachPhan = danhSachPhan;
            ViewBag.Username = username;
            ViewBag.IsSingleMovie = isSingleMovie;
            ViewBag.IsFavorite = isFavorite;

            return View(movie);
        }

        // GET: Phim
        public async Task<IActionResult> Index()
        {
            var dBAHTBContext = _context.Phim.Include(p => p.TheLoai).Include(p => p.User);
            return View(await dBAHTBContext.ToListAsync());
        }

        // GET: Phim/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var phim = await _context.Phim
                .Include(p => p.TheLoai)
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.IDPhim == id);
            if (phim == null)
            {
                return NotFound();
            }

            return View(phim);
        }
    }
   
}
