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

        public IActionResult Movies(int page = 1, string genre = "Tất cả", string search = "")
        {
            // Lấy JWT token từ Session
            var token = HttpContext.Session.GetString("JwtToken");

            if (!string.IsNullOrEmpty(token))
            {
                const int itemsPerPage = 8;

                IQueryable<Phim> moviesQuery = _context.Phim
        .Include(m => m.TheLoai)
        .AsNoTracking(); // Giúp cải thiện hiệu suất


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
