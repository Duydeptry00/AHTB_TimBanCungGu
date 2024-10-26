using AHTB_TimBanCungGu_API.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System;
using AHTB_TimBanCungGu_API.Models;

namespace AHTB_TimBanCungGu_MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class QuanLyDanhMucController : Controller

    {
        private readonly DBAHTBContext _context;

        public QuanLyDanhMucController(DBAHTBContext context)
        {
            _context = context;
        }
        // GET: QuanLyDanhMuc
        public async Task<IActionResult> Index(string searchString)
        {
            ViewData["CurrentFilter"] = searchString;

            // Only select TenPhim and MoTa for search
            var phimList = from p in _context.Phim
                           select p;

            if (!String.IsNullOrEmpty(searchString))
            {
                phimList = phimList.Where(p =>
                    p.TenPhim.Contains(searchString) ||  // Search by TenPhim
                    p.MoTa.Contains(searchString));      // Search by MoTa
            }

            return View(await phimList.ToListAsync());
        }

        // GET: QuanLyDanhMuc/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var phim = await _context.Phim
                .FirstOrDefaultAsync(m => m.IDPhim == id);
            if (phim == null)
            {
                return NotFound();
            }

            return View(phim);
        }

        // GET: QuanLyDanhMuc/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: QuanLyDanhMuc/Create
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
                        return View(phim);
                    }

                    var fileName = Guid.NewGuid().ToString() + fileExtension;
                    var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                    if (!Directory.Exists(uploadsDir))
                    {
                        Directory.CreateDirectory(uploadsDir);
                    }

                    var filePath = Path.Combine(uploadsDir, fileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await hinhAnhFile.CopyToAsync(fileStream);
                    }

                    phim.HinhAnh = fileName;
                }

                phim.IDPhim = Guid.NewGuid().ToString();
                _context.Add(phim);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(phim);
        }

        // GET: QuanLyDanhMuc/Edit/5
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

            return View(phim);
        }

        // POST: QuanLyDanhMuc/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Phim model, IFormFile hinhAnhFile)
        {
            if (ModelState.IsValid)
            {
                var existingMovie = await _context.Phim.FindAsync(model.IDPhim);
                if (existingMovie == null)
                {
                    return NotFound();
                }

                if (hinhAnhFile != null && hinhAnhFile.Length > 0)
                {
                    var fileName = Path.GetFileName(hinhAnhFile.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await hinhAnhFile.CopyToAsync(stream);
                    }

                    existingMovie.HinhAnh = fileName;
                }

                existingMovie.TenPhim = model.TenPhim;
                existingMovie.MoTa = model.MoTa;

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // GET: QuanLyDanhMuc/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var phim = await _context.Phim
                .FirstOrDefaultAsync(m => m.IDPhim == id);

            if (phim == null)
            {
                return NotFound();
            }

            return View(phim);
        }

        // POST: QuanLyDanhMuc/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var phim = await _context.Phim.FindAsync(id);
            if (phim != null)
            {
                _context.Phim.Remove(phim);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // Search method only by TenPhim and MoTa
        public IActionResult Search(string query)
        {
            var results = _context.Phim
                .Where(p => p.TenPhim.Contains(query) || p.MoTa.Contains(query)) // Search by TenPhim and MoTa
                .ToList();

            return View(results);
        }
    }
    
}

