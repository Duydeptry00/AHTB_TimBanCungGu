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
using AHTB_TimBanCungGu_MVC.Models;

namespace HeThongChieuPhimAHTB_TimBanCungGu_MVC.Controllers
{
    public class PhimController : Controller
    {
        private readonly DBAHTBContext _context;

        public PhimController(DBAHTBContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> XemPhim([FromQuery] string idPhim, [FromQuery] string Phan)
        {
            var token = HttpContext.Session.GetString("JwtToken");
            var username = HttpContext.Session.GetString("TempUserName");

            if (string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để xem nội dung này.";
                return RedirectToAction("Login", "LoginvsRegister");
            }

            var user = await _context.Users
                .Include(u => u.ThongTinCN)
                .FirstOrDefaultAsync(u => u.UserName == username);

            if (user == null)
            {
                TempData["ErrorMessage"] = "Người dùng không tồn tại.";
                return RedirectToAction("Index", "Home");
            }

            var phim = await _context.Phim
                .Include(p => p.Phan)
                .Include(p => p.TheLoai)
                .FirstOrDefaultAsync(p => p.IDPhim == idPhim);

            if (phim == null)
            {
                return NotFound();
            }

            if (string.Equals(phim.DangPhim, "Phim Lẻ", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("PhimLe", "Phim", new { idPhim });
            }

            var phan = await _context.Phan
                .FirstOrDefaultAsync(p => p.IDPhan == Phan && p.PhimID == idPhim && p.NgayCongChieu <= DateTime.Now);

            if (phan == null)
            {
                TempData["ErrorMessage"] = "Phân đoạn này chưa được phát hành hoặc không tồn tại.";
                return RedirectToAction("ChiTietPhim", "Phim", new { id = idPhim });
            }

            if (phim.NoiDungPremium && !user.ThongTinCN.IsPremium)
            {
                TempData["ErrorMessage"] = "Bạn cần tài khoản Premium để xem nội dung này.";
                return RedirectToAction("ChiTietPhim", "Phim", new { id = idPhim });
            }

            return RedirectToAction("PhimBo", "Phim", new { idPhim, Phan });
        }
        public async Task<IActionResult> PhimLe(string idPhim)
        {
            // Lấy JWT token từ Session
            var token = HttpContext.Session.GetString("JwtToken");
            var userName = HttpContext.Session.GetString("TempUserName");

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userName))
            {
                // Nếu chưa đăng nhập, chuyển đến trang đăng nhập
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để xem nội dung này.";
                return RedirectToAction("Login", "LoginvsRegister");
            }

            // Lấy thông tin cá nhân người dùng
            var userInfo = await _context.ThongTinCN
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.User.UserName == userName);

            if (userInfo == null)
            {
                TempData["ErrorMessage"] = "Thông tin người dùng không hợp lệ.";
                return RedirectToAction("Index", "Home");
            }

            // Truyền thông tin người dùng vào ViewBag
            ViewBag.HoTen = userInfo.HoTen;
            ViewBag.GioiTinh = userInfo.GioiTinh;
            ViewBag.IdThongTinCaNhan = userInfo.IDProfile;

            // Lấy thông tin phim lẻ
            var phimLe = await _context.Phim
                .Include(p => p.TheLoai)
                .FirstOrDefaultAsync(p => p.IDPhim == idPhim && p.DangPhim.ToLower() == "phim lẻ".ToLower());

            if (phimLe == null)
            {
                return NotFound();
            }

            // Kiểm tra nếu phim là Premium và người dùng không phải Premium
            if (phimLe.NoiDungPremium && !userInfo.IsPremium)
            {
                TempData["ErrorMessage"] = "Nội dung này chỉ dành cho người dùng Premium. Vui lòng nâng cấp tài khoản.";
            }


            // Lấy danh sách phim đề cử
            var phimDeCu = await _context.Phim
                .Where(p => p.TheLoai.TenTheLoai == phimLe.TheLoai.TenTheLoai)
                .Where(p => p.IDPhim != phimLe.IDPhim && p.NgayPhatHanh <= DateTime.Now)
                .ToListAsync();

            // Truyền dữ liệu vào ViewBag
            ViewBag.PhimLe = phimLe;
            ViewBag.PhimBo = null;
            ViewBag.Phan = null;
            ViewBag.PhimDeCu = phimDeCu;

            return View("XemPhim", phimLe);
        }

        public async Task<IActionResult> PhimBo(string idPhim, string Phan)
        {
            // Lấy thông tin người dùng từ Session
            var userName = HttpContext.Session.GetString("TempUserName");
            var token = HttpContext.Session.GetString("JwtToken");

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userName))
            {
                // Nếu chưa đăng nhập, chuyển đến trang đăng nhập
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để xem nội dung này.";
                return RedirectToAction("Login", "LoginvsRegister");
            }

            // Lấy thông tin cá nhân người dùng
            var userInfo = await _context.ThongTinCN
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.User.UserName == userName);

            if (userInfo == null)
            {
                TempData["ErrorMessage"] = "Thông tin người dùng không hợp lệ.";
                return RedirectToAction("Index", "Home");
            }

            // Truyền thông tin người dùng vào ViewBag
            ViewBag.HoTen = userInfo.HoTen;
            ViewBag.GioiTinh = userInfo.GioiTinh;
            ViewBag.IdThongTinCaNhan = userInfo.IDProfile;

            // Lấy thông tin phần của phim
            var phan = await _context.Phan
                .Include(p => p.Phim)
                .ThenInclude(p => p.TheLoai)
                .FirstOrDefaultAsync(p => p.Phim.IDPhim == idPhim && p.IDPhan == Phan);

            if (phan == null)
            {
                return NotFound();
            }

            // Lấy thông tin phim bộ
            var phimBo = await _context.Phim
                .Include(p => p.Phan)
                .Include(p => p.TheLoai)
                .FirstOrDefaultAsync(p => p.IDPhim == idPhim && p.DangPhim.ToLower() == "Phim Bộ".ToLower());

            if (phimBo == null)
            {
                TempData["ErrorMessage"] = "Phim không tồn tại.";
                return RedirectToAction("Index", "Home");
            }

            // Kiểm tra nếu phim là Premium và người dùng không phải Premium
            if (phimBo.NoiDungPremium && !userInfo.IsPremium)
            {
                TempData["ErrorMessage"] = "Nội dung này chỉ dành cho người dùng Premium. Vui lòng nâng cấp tài khoản.";
              
            }

            // Lấy danh sách phim đề cử
            var phimDeCu = await _context.Phim
                .Where(p => p.TheLoai.TenTheLoai == phan.Phim.TheLoai.TenTheLoai)
                .Where(p => p.IDPhim != phan.Phim.IDPhim && p.NgayPhatHanh <= DateTime.Now)
                .ToListAsync();

            // Truyền dữ liệu vào ViewBag
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
                var newFavorite = new AHTB_TimBanCungGu_API.Models.PhimYeuThich
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


        public async Task<IActionResult> ChiTietPhimAsync(string id)
        {
            var token = HttpContext.Session.GetString("JwtToken");

            if (string.IsNullOrEmpty(token))
            {
                // Chuyển hướng đến trang đăng nhập nếu chưa đăng nhập
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để xem nội dung này.";
                return RedirectToAction("Login", "LoginvsRegister");
            }

            var username = HttpContext.Session.GetString("TempUserName");
            var userInfo = await _context.ThongTinCN
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.User.UserName == username);

            if (userInfo != null)
            {
                ViewBag.HoTen = userInfo.HoTen;
                ViewBag.GioiTinh = userInfo.GioiTinh;
                ViewBag.IdThongTinCaNhan = userInfo.IDProfile;
            }

            var movie = await _context.Phim
                .Include(p => p.Phan)
                .Include(p => p.TheLoai)
                .FirstOrDefaultAsync(p => p.IDPhim == id);

            if (movie == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.ThongTinCN)
                .FirstOrDefaultAsync(u => u.UserName == username);

            if (user == null)
            {
                TempData["ErrorMessage"] = "Người dùng không tồn tại.";
                return RedirectToAction("Index", "Home");
            }
            var danhSachPhan = _context.Phan
                .Where(p => p.PhimID == id && p.NgayCongChieu <= DateTime.Now)
                .OrderBy(p => p.SoPhan)
                .ToList();

            var phimDeCu = _context.Phim
                .Include(p => p.TheLoai)
                .Include(p => p.Phan)
                .Where(p => p.TheLoai.TenTheLoai == movie.TheLoai.TenTheLoai && p.IDPhim != movie.IDPhim && p.NgayPhatHanh <= DateTime.Now)
                .ToList();

            var isFavorite = _context.PhimYeuThich
                .Any(py => py.NguoiDungYT == user.UsID && py.PhimYT == id);

            ViewBag.PhimDeCu = phimDeCu;
            ViewBag.PhimLe = movie;
            ViewBag.DanhSachPhan = danhSachPhan;
            ViewBag.IsFavorite = isFavorite;
            ViewBag.IsPremium = user.ThongTinCN.IsPremium;
            ViewBag.Username = username;
            ViewBag.IsSingleMovie = movie.DangPhim.Trim().Equals("Phim Lẻ", StringComparison.OrdinalIgnoreCase);

            return View(movie);
        }



        // GET: Phim
        public async Task<IActionResult> Index()
        {
            // Lấy JWT token từ Session
            var token = HttpContext.Session.GetString("JwtToken");

            if (!string.IsNullOrEmpty(token))
            {
                var dBAHTBContext = _context.Phim.Include(p => p.TheLoai).Include(p => p.User);
                return View(await dBAHTBContext.ToListAsync());
            }
            else
            {
                // Nếu không có token, có thể chuyển đến trang đăng nhập
                ViewBag.Message = "Bạn chưa đăng nhập.";
                return RedirectToAction("Login", "LoginvsRegister");
            }
        }

        // GET: Phim/Details/5
        public async Task<IActionResult> Details(string id)
        {
            // Lấy JWT token từ Session
            var token = HttpContext.Session.GetString("JwtToken");

            if (!string.IsNullOrEmpty(token))
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
            else
            {
                // Nếu không có token, có thể chuyển đến trang đăng nhập
                ViewBag.Message = "Bạn chưa đăng nhập.";
                return RedirectToAction("Login", "LoginvsRegister");
            }
        }
    }
   
}
