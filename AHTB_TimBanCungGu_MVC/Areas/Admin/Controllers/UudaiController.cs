using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AHTB_TimBanCungGu_API.Data;
using AHTB_TimBanCungGu_API.Models;
using Microsoft.AspNetCore.Http;
using AHTB_TimBanCungGu_MVC.Models;
using DocumentFormat.OpenXml.Vml;
using System.IO;

namespace Uudaipro.Controllers
{
    [Area("Admin")]
    public class UudaiController : Controller
    {

        private readonly DBAHTBContext _context;

        public UudaiController(DBAHTBContext context)
        {
            _context = context;
        }

        // GET: Uudai
        public async Task<IActionResult> Index()
        {
            return View(await _context.UuDai.ToListAsync());
        }

        // GET: Uudai/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var uudai = await _context.UuDai
                .FirstOrDefaultAsync(m => m.IdUuDai == id);
            if (uudai == null)
            {
                return NotFound();
            }

            return View(uudai);
        }

        // GET: Uudai/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Uudai/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UuDai uudai, IFormFile hinh)
        {
            if (ModelState.IsValid)
            {
                if (hinh == null || hinh.Length == 0)
                {
                    ModelState.AddModelError(string.Empty, "Cần phải chọn tệp hình ảnh.");
                   
                    return View(uudai);
                }
                // Kiểm tra định dạng tệp hình ảnh
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = System.IO.Path.GetExtension(hinh.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError(string.Empty, "Chỉ cho phép các tệp hình ảnh (.jpg, .jpeg, .png, .gif).");
                   
                    return View(uudai);
                }

                // Tạo đường dẫn lưu trữ ảnh
                var fileName = System.IO.Path.GetRandomFileName() + fileExtension;
                var filePath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Uploads", fileName);

                try
                {
                    // Lưu tệp hình ảnh
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await hinh.CopyToAsync(fileStream);
                    }
                    uudai.Hinh = fileName;
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"Đã xảy ra lỗi khi tải hình ảnh: {ex.Message}");
                    return View(uudai);
                }

                // Lưu đối tượng UuDai vào cơ sở dữ liệu
                _context.Add(uudai);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(uudai);
        }


        // GET: Uudai/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var uudai = await _context.UuDai.FindAsync(id);
            if (uudai == null)
            {
                return NotFound();
            }

            ViewBag.CurrentImage = uudai.Hinh; // Lưu đường dẫn hình ảnh hiện tại vào ViewBag
            return View(uudai);
        }


        // POST: Uudai/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, UuDai uudai, IFormFile hinh)
        {
            if (id != uudai.IdUuDai)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (hinh != null && hinh.Length > 0)
                    {
                        // Xử lý tệp hình ảnh mới
                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                        var fileExtension = System.IO.Path.GetExtension(hinh.FileName).ToLowerInvariant();
                        if (!allowedExtensions.Contains(fileExtension))
                        {
                            ModelState.AddModelError(string.Empty, "Chỉ cho phép các tệp hình ảnh (.jpg, .jpeg, .png, .gif).");
                            return View(uudai);
                        }

                        var fileName = System.IO.Path.GetRandomFileName() + fileExtension;
                        var filePath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Uploads", fileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await hinh.CopyToAsync(fileStream);
                        }

                        // Cập nhật hình ảnh mới
                        uudai.Hinh = fileName;
                    }
                    else
                    {
                        // Giữ nguyên hình ảnh hiện tại
                        var existingUuDai = await _context.UuDai.AsNoTracking().FirstOrDefaultAsync(u => u.IdUuDai == id);
                        uudai.Hinh = existingUuDai?.Hinh;
                    }

                    _context.Update(uudai);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UudaiExists(uudai.IdUuDai))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(uudai);
        }


        // GET: Uudai/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var uudai = await _context.UuDai
                .FirstOrDefaultAsync(m => m.IdUuDai == id);
            if (uudai == null)
            {
                return NotFound();
            }

            return View(uudai);
        }

        // POST: Uudai/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var uudai = await _context.UuDai.FindAsync(id);
            _context.UuDai.Remove(uudai);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UudaiExists(string id)
        {
            return _context.UuDai.Any(e => e.IdUuDai == id);
        }
    }
}
