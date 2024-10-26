using AHTB_TimBanCungGu_API.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System;
using AHTB_TimBanCungGu_API.Models;

namespace AHTB_TimBanCungGu_MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class QuanLyPhimsController : Controller
    {
        private readonly DBAHTBContext _context;

        public QuanLyPhimsController(DBAHTBContext context)
        {
            _context = context;
        }
        // GET: Phims
        public async Task<IActionResult> Index(string searchString)
        {
            ViewData["CurrentFilter"] = searchString;

            var phimList = from p in _context.Phim.Include(p => p.TheLoai)
                           select p;

            if (!String.IsNullOrEmpty(searchString))
            {
                phimList = phimList.Where(p =>
                    p.TenPhim.Contains(searchString) ||
                    p.DienVien.Contains(searchString) ||
                    p.TheLoai.TenTheLoai.Contains(searchString));
            }

            // Populate ViewBag.Categories
            ViewBag.Categories = await _context.TheLoai.ToListAsync();

            return View(await phimList.ToListAsync());
        }

        // GET: Phims/Details/5
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

        // GET: Phims/Create
        public IActionResult Create()
        {
            ViewBag.TheLoai = new SelectList(_context.TheLoai, "IdTheLoai", "TenTheLoai");
            ViewBag.IDAdmin = new SelectList(_context.Users, "UsID", "UsID");

            var movies = _context.Phim.Include(m => m.TheLoai).ToList();
            return View(movies);
        }

        // POST: Phims/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Phim phim, IFormFile hinhAnhFile)
        {
            if (ModelState.IsValid)
            {
                if (hinhAnhFile != null && hinhAnhFile.Length > 0)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var fileExtension = Path.GetExtension(hinhAnhFile.FileName).ToLowerInvariant();

                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        ModelState.AddModelError(string.Empty, "Only image files (.jpg, .jpeg, .png, .gif) are allowed.");
                        ViewBag.TheLoai = new SelectList(_context.TheLoai, "IdTheLoai", "TenTheLoai", phim.TheLoaiPhim);
                        return View(phim);
                    }

                    var fileName = Guid.NewGuid().ToString() + fileExtension; // Keep original extension
                    var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                    if (!Directory.Exists(uploadsDir))
                    {
                        Directory.CreateDirectory(uploadsDir); // Ensure the directory exists
                    }

                    var filePath = Path.Combine(uploadsDir, fileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await hinhAnhFile.CopyToAsync(fileStream);
                    }

                    phim.HinhAnh = fileName; // Save the file name in the database
                }

                phim.IDPhim = Guid.NewGuid().ToString();
                _context.Add(phim);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.TheLoai = new SelectList(_context.TheLoai, "IdTheLoai", "TenTheLoai", phim.TheLoaiPhim);
            return View(phim);
        }



        // GET: Phims/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var phim = await _context.Phim.FindAsync(id);
            if (phim == null)
            {
                return NotFound();
            }
            var movie = _context.Phim.Include(m => m.TheLoai) // Đảm bảo đã lấy thể loại phim nếu cần
                  .FirstOrDefault(m => m.IDPhim == id);
            if (movie == null)
            {
                return NotFound();
            }

            ViewBag.Categories = new SelectList(_context.TheLoai, "IdTheLoai", "TenTheLoai", phim.TheLoaiPhim);
            ViewBag.IDAdmin = new SelectList(_context.Users, "UsID", "UsID", phim.IDAdmin);

            return View(phim);
        }

        // POST: Phims/Edit/5
        [HttpPost]
        public async Task<IActionResult> Edit(Phim model, IFormFile hinhAnhFile)
        {
            if (ModelState.IsValid)
            {
                var existingMovie = await _context.Phim.FindAsync(model.IDPhim);
                if (existingMovie == null)
                {
                    return NotFound();
                }

                // Xử lý file upload nếu có
                if (hinhAnhFile != null && hinhAnhFile.Length > 0)
                {
                    var fileName = Path.GetFileName(hinhAnhFile.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await hinhAnhFile.CopyToAsync(stream);
                    }

                    existingMovie.HinhAnh = fileName; // Cập nhật tên file
                }

                // Cập nhật các thuộc tính khác
                existingMovie.TenPhim = model.TenPhim;
                existingMovie.DienVien = model.DienVien;
                existingMovie.SourcePhim = model.SourcePhim;
                existingMovie.TheLoaiPhim = model.TheLoaiPhim;

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index)); // Chuyển hướng về trang danh sách
            }

            ViewBag.Categories = _context.TheLoai.ToList(); // Lấy danh sách thể loại nếu có lỗi
            return View(model);
        }




        // GET: Phims/Delete/5
        public async Task<IActionResult> Delete(string id)
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

        // POST: Phims/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var phim = await _context.Phim.FindAsync(id);
            if (phim != null)
            {
                _context.Phim.Remove(phim);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool PhimExists(string id)
        {
            return _context.Phim.Any(e => e.IDPhim == id);
        }

        public IActionResult Search(string query)
        {
            // Your logic to search the movies
            var results = _context.Phim
                .Where(p => p.TenPhim.Contains(query) || p.DienVien.Contains(query)) // Example criteria
                .ToList();

            return View(results); // Return the view with search results
        }

    }
}
