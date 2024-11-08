using System;
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





        public IActionResult ChiTietPhim(string id, string searchTerm = null)
        {
            // Lấy thông tin username từ Session
            var username = HttpContext.Session.GetString("TempUserName");

            // Lấy thông tin chi tiết của phim dựa theo IDPhim
            var movie = _context.Phim
                .Include(p => p.Phan)      // Bao gồm các phần của phim
                .Include(p => p.TheLoai)   // Bao gồm thể loại
                .FirstOrDefault(p => p.IDPhim == id);

            if (movie == null)
            {
                return NotFound();
            }

            // Lấy danh sách các phần của bộ phim
            var danhSachPhan = _context.Phan
                                .Where(p => p.PhimID == id)
                                .OrderBy(p => p.SoPhan)
                                .ToList();

            // Lấy danh sách phim đề cử cùng thể loại và loại trừ phim hiện tại
            var phimDeCu = _context.Phim
                .Include(p => p.TheLoai)
                .Include(p => p.Phan)
                .Where(p => p.TheLoai.TenTheLoai == movie.TheLoai.TenTheLoai && p.IDPhim != movie.IDPhim)
                .ToList();

            // Lọc theo từ khóa tìm kiếm nếu từ khóa được cung cấp
            if (!string.IsNullOrEmpty(searchTerm))
            {
                phimDeCu = phimDeCu
                    .Where(p => p.TenPhim.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            var user = _context.Users.FirstOrDefault(u => u.UserName == username);
            if (user == null)
            {
                return RedirectToAction("Login", "LoginvsRegister");
            }

            // Kiểm tra xem phim này đã được người dùng yêu thích chưa
            var isFavorite = _context.PhimYeuThich
                .Any(py => py.NguoiDungYT == user.UsID && py.PhimYT == id);
            ViewBag.IsFavorite = isFavorite;


            // Truyền thông tin cần thiết qua ViewBag
            ViewBag.PhimDeCu = phimDeCu;
            ViewBag.DanhSachPhan = danhSachPhan;
            ViewBag.Username = username;
            ViewBag.IsFavorite = isFavorite; // Trạng thái yêu thích

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
