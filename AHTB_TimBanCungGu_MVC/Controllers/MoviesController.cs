using AHTB_TimBanCungGu_API.Data;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Linq;
using System;
using AHTB_TimBanCungGu_API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

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
            const int itemsPerPage = 8;

            IQueryable<Phim> moviesQuery = _context.Phim;

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

            if (!movies.Any())
            {
                ViewBag.NoResults = true;
            }
            else
            {
                ViewBag.NoResults = false;
            }

            return PartialView("Movies", movies);
        }
    }
}
