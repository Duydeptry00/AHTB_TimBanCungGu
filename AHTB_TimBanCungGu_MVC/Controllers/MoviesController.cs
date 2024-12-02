using AHTB_TimBanCungGu_API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AHTB_TimBanCungGu_API.Models;
using Microsoft.AspNetCore.Http;

namespace AHTB_TimBanCungGu_MVC.Controllers
{
    public class MoviesController : Controller
    {
        private readonly DBAHTBContext _context;

        public MoviesController(DBAHTBContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> MoviesAsync(int page = 1, string genre = "Tất cả", string search = "")
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
                const int itemsPerPage = 8;

                IQueryable<Phim> moviesQuery = _context.Phim
            .Include(m => m.TheLoai)
            .AsNoTracking()
            .Where(m => m.NgayPhatHanh <= DateTime.Now && m.TrangThai != "Ẩn"); // Lọc bỏ phim có trạng thái ẩn


                if (genre != "Tất cả")
                {
                    moviesQuery = moviesQuery.Where(m => m.TheLoai.TenTheLoai.Contains(genre));
                }

                if (!string.IsNullOrEmpty(search))
                {
                    moviesQuery = moviesQuery.Where(m => m.TenPhim.Contains(search));
                }

                var totalItems = moviesQuery.Count();
                var movies = moviesQuery
                    .Skip((page - 1) * itemsPerPage)
                    .Take(itemsPerPage)
                    .ToList();

                ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)itemsPerPage);
                ViewBag.CurrentPage = page;
                ViewBag.Genre = genre;
                ViewBag.Search = search;
                ViewBag.NoResults = !movies.Any();

                return PartialView("Movies", movies);
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
