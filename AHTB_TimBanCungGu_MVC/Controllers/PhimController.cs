using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AHTB_TimBanCungGu_API.Models;
using AHTB_TimBanCungGu_API.Data;

namespace HeThongChieuPhimAHTB_TimBanCungGu_MVC.Controllers
{
    public class PhimController : Controller
    {
        private readonly DBAHTBContext _context;

        public PhimController(DBAHTBContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> XemPhim(string idPhim, int soPhan)
        {
            var phan = await _context.Phan
                .Include(p => p.Phim) // Bao gồm thông tin phim
                .ThenInclude(p => p.TheLoai) // Bao gồm thông tin thể loại của phim
                .FirstOrDefaultAsync(p => p.Phim.IDPhim == idPhim && p.SoPhan == soPhan);

            if (phan == null)
            {
                return NotFound();
            }

            var tenTheLoai = phan.Phim.TheLoai.TenTheLoai;

            // Lấy danh sách phim đề cử
            var phimDeCu = await _context.Phim
                .Where(p => p.TheLoai.TenTheLoai == tenTheLoai) // Lọc phim theo thể loại của phim hiện tại
                .Where(p => p.IDPhim != phan.Phim.IDPhim) // Loại trừ phim hiện tại
                .ToListAsync();

            // Gán dữ liệu vào ViewBag
            ViewBag.Phan = phan;
            ViewBag.PhimDeCu = phimDeCu;

            return View(phan);
        }
        public IActionResult ChiTietPhim(string id, string searchTerm = null)
        {
            // Lấy thông tin chi tiết của phim dựa theo IDPhim
            var movie = _context.Phim
                .Include(p => p.Phan)      // Bao gồm các phần của phim
                .Include(p => p.TheLoai)   // Bao gồm thể loại
                .FirstOrDefault(p => p.IDPhim == id);

            if (movie == null)
            {
                return NotFound();
            }

            // Lấy tên thể loại của phim hiện tại
            var tenTheLoai = movie.TheLoai.TenTheLoai;

            // Lấy danh sách các phần của bộ phim
            var danhSachPhan = movie.Phan.ToList();

            // Lấy danh sách phim đề cử cùng thể loại và loại trừ phim hiện tại
            var phimDeCu = _context.Phim
                .Include(p => p.TheLoai)
                .Include(p => p.Phan)
                .Where(p => p.TheLoai.TenTheLoai == tenTheLoai && p.IDPhim != movie.IDPhim)
                .ToList();

            // Lọc theo từ khóa tìm kiếm nếu từ khóa được cung cấp
            if (!string.IsNullOrEmpty(searchTerm))
            {
                phimDeCu = phimDeCu
                    .Where(p => p.TenPhim.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            // Truyền danh sách phim đề cử và danh sách các phần qua ViewBag
            ViewBag.PhimDeCu = phimDeCu;
            ViewBag.DanhSachPhan = danhSachPhan;

            return View(movie); // Truyền phim hiện tại sang view
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
