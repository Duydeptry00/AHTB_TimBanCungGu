using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AHTB_TimBanCungGu_API.Data;
using AHTB_TimBanCungGu_API.Models;
using System.IO;
using Microsoft.AspNetCore.Http;

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

        // GET: Admin/QuanLyPhims
        public async Task<IActionResult> Index()
        {
            // Lấy token JWT và UserType từ session
            var token = HttpContext.Session.GetString("JwtToken");
            var userType = HttpContext.Session.GetString("UserType");

            if (userType == "Admin" && token != null)
            {
                var dBAHTBContext = _context.Phim.Include(p => p.TheLoai).Include(p => p.User).Where(p => p.TrangThai != "Ẩn");
                // Get all unique genres from the database
                var genres = await _context.TheLoai.Select(t => t.TenTheLoai).Distinct().ToListAsync();

                // Pass genres to ViewData
                ViewData["Genres"] = genres;
                return View(await dBAHTBContext.ToListAsync());
            }

            return NotFound();
          
        }

        // GET: Admin/QuanLyPhims/Details/5
        public async Task<IActionResult> Details(string id)
        {
            // Lấy token JWT và UserType từ session
            var token = HttpContext.Session.GetString("JwtToken");
            var userType = HttpContext.Session.GetString("UserType");

            if (userType == "Admin" && token != null)
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

            return NotFound();
           
        }

        // GET: Admin/QuanLyPhims/Create
        public IActionResult Create()
        {
            // Lấy token JWT và UserType từ session
            var token = HttpContext.Session.GetString("JwtToken");
            var userType = HttpContext.Session.GetString("UserType");

            if (userType == "Admin" && token != null)
            {
                ViewData["TheLoaiPhim"] = new SelectList(_context.TheLoai, "IdTheLoai", "TenTheLoai");
                ViewData["IDAdmin"] = new SelectList(_context.Users, "UsID", "UserName");
                ViewData["DangPhimOptions"] = new SelectList(new[] { "Phim lẻ", "Phim bộ" });
                return View();
            }

            return NotFound();
           
        }

        // POST: Admin/QuanLyPhims/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
      [Bind("IDPhim,TenPhim,MoTa,DienVien,TheLoaiPhim,NgayPhatHanh,DanhGia,TrailerURL,NoiDungPremium,SourcePhim,HinhAnh,DangPhim,NgayCapNhat,IDAdmin,TrangThai")] Phim phim,
      IFormFile ImageFile,
      int? soluongtap)
        {
            if (ModelState.IsValid)
            {
                if (ImageFile == null || ImageFile.Length == 0)
                {
                    ModelState.AddModelError(string.Empty, "Cần phải chọn tệp hình ảnh.");
                    PopulateViewData(phim);
                    return View(phim);
                }

                // Kiểm tra định dạng tệp hình ảnh
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(ImageFile.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError(string.Empty, "Chỉ cho phép các tệp hình ảnh (.jpg, .jpeg, .png, .gif).");
                    PopulateViewData(phim);
                    return View(phim);
                }

                // Tạo đường dẫn lưu trữ ảnh
                var fileName = Path.GetRandomFileName() + fileExtension;
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Uploads", fileName);

                try
                {
                    // Lưu tệp hình ảnh
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(fileStream);
                    }
                    phim.HinhAnh = fileName;
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"Đã xảy ra lỗi khi tải hình ảnh: {ex.Message}");
                    PopulateViewData(phim);
                    return View(phim);
                }

                // Lưu thông tin phim trước
                _context.Add(phim);
                await _context.SaveChangesAsync(); // Lưu để lấy IDPhim đã được tạo

                // Nếu là Phim Bộ, thêm Phan và các Tập
                if (phim.DangPhim == "Phim Bộ" && soluongtap.HasValue)
                {
                    // Tính toán IDPhan mới
                    string lastphanId = _context.Phan.OrderByDescending(kh => kh.IDPhan).Select(kh => kh.IDPhan).FirstOrDefault();
                    int nextphanId = (lastphanId == null) ? 1 : int.Parse(lastphanId.Substring(2)) + 1;

                    var phan = new Phan
                    {
                        IDPhan = "PH" + nextphanId.ToString(),
                        SoPhan = 1,
                        SoLuongTap = soluongtap.Value,
                        PhimID = phim.IDPhim // Đảm bảo IDPhim đã tồn tại
                    };

                    _context.Phan.Add(phan);
                    await _context.SaveChangesAsync(); // Lưu để tạo khóa chính cho phần

                    // Tính toán IDTap mới
                    string lasttapId = _context.Tap.OrderByDescending(t => t.IDTap).Select(t => t.IDTap).FirstOrDefault();
                    int nexttapId = (lasttapId == null) ? 1 : int.Parse(lasttapId.Substring(1)) + 1;

                    // Tạo các tập phim
                    for (int i = 1; i <= soluongtap.Value; i++)
                    {
                        var tap = new Tap
                        {
                            IDTap = "T" + nexttapId,
                            SoTap = i,
                            PhanPhim = phan.IDPhan // Liên kết với phần vừa tạo
                        };
                        _context.Tap.Add(tap);
                        nexttapId++;
                    }

                    await _context.SaveChangesAsync(); // Lưu các tập phim
                }

                return RedirectToAction(nameof(Index));
            }

            PopulateViewData(phim);
            return View(phim);
        }


        // Helper method to populate ViewData
        private void PopulateViewData(Phim phim)
        {

            ViewData["TheLoaiPhim"] = new SelectList(_context.TheLoai, "IdTheLoai", "TenTheLoai", phim.TheLoaiPhim);
            ViewData["IDAdmin"] = new SelectList(_context.Users, "UsID", "UserName", phim.IDAdmin);
            ViewData["DangPhimOptions"] = new SelectList(new[] { "Phim lẻ", "Phim bộ" }, phim.DangPhim);
        }


        // GET: Admin/QuanLyPhims/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            // Lấy token JWT và UserType từ session
            var token = HttpContext.Session.GetString("JwtToken");
            var userType = HttpContext.Session.GetString("UserType");

            if (userType == "Admin" && token != null)
            {
                if (string.IsNullOrEmpty(id))
                {
                    return NotFound();
                }

                var phim = await _context.Phim.FindAsync(id);
                if (phim == null)
                {
                    return NotFound();
                }

                // Populate dropdowns for the form
                ViewData["TheLoaiPhim"] = new SelectList(_context.TheLoai, "IdTheLoai", "TenTheLoai", phim.TheLoaiPhim);
                ViewData["IDAdmin"] = new SelectList(_context.Users, "UsID", "UserName", phim.IDAdmin);
                ViewData["DangPhimOptions"] = new SelectList(new[] { "Phim lẻ", "Phim bộ" }, phim?.DangPhim);
                return View(phim);
            }

            return NotFound();
          
        }

        // POST: Admin/QuanLyPhims/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("IDPhim,TenPhim,MoTa,DienVien,TheLoaiPhim,NgayPhatHanh,DanhGia,TrailerURL,NoiDungPremium,SourcePhim,HinhAnh,DangPhim,NgayCapNhat,IDAdmin,TrangThai")] Phim phim, IFormFile ImageFile)
        {
            if (id != phim.IDPhim)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Lấy thực thể hiện có từ ngữ cảnh
                    var existingPhim = await _context.Phim.FindAsync(id);
                    if (existingPhim == null)
                    {
                        return NotFound();
                    }

                    // Cập nhật chỉ những thuộc tính bạn muốn thay đổi
                    existingPhim.TenPhim = phim.TenPhim;
                    existingPhim.MoTa = phim.MoTa;
                    existingPhim.DienVien = phim.DienVien;
                    existingPhim.TheLoaiPhim = phim.TheLoaiPhim;
                    existingPhim.NgayPhatHanh = phim.NgayPhatHanh;
                    existingPhim.DanhGia = phim.DanhGia;
                    existingPhim.TrailerURL = phim.TrailerURL;
                    existingPhim.NoiDungPremium = phim.NoiDungPremium;
                    existingPhim.SourcePhim = phim.SourcePhim;
                    existingPhim.DangPhim = phim.DangPhim;
                    existingPhim.NgayCapNhat = DateTime.UtcNow; // Cập nhật thời gian
                    existingPhim.IDAdmin = phim.IDAdmin;
                    existingPhim.TrangThai = phim.TrangThai;

                    // Xử lý tải lên hình ảnh
                    await HandleImageUpload(existingPhim, ImageFile);

                    // Lưu thay đổi
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PhimExists(phim.IDPhim))
                    {
                        return NotFound();
                    }
                    throw; // Ném lại ngoại lệ nếu phim vẫn tồn tại
                }
            }

            PopulateViewData(phim);
            return View(phim);
        }


        private async Task HandleImageUpload(Phim phim, IFormFile ImageFile)
        {
            if (ImageFile != null && ImageFile.Length > 0)
            {
                // Validate file type and size
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(ImageFile.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError(string.Empty, "Only image files (.jpg, .jpeg, .png, .gif) are allowed.");
                    return;
                }

                // Generate a unique file name and path
                var fileName = Path.GetRandomFileName() + fileExtension;
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Uploads", fileName);

                // Save the new image
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(fileStream);
                }

                // Save the new image file name
                phim.HinhAnh = fileName;
            }
            else
            {
                // If no new image is uploaded, retain the existing image
                var existingPhim = await _context.Phim.FindAsync(phim.IDPhim);
                if (existingPhim != null)
                {
                    phim.HinhAnh = existingPhim.HinhAnh;
                }
            }
        }



        private bool PhimExists(string id)
        {
            return _context.Phim.Any(e => e.IDPhim == id);
        }



        [HttpPost]
        public async Task<IActionResult> HideFromIndex(string id)
        {
            var phim = await _context.Phim.FindAsync(id);
            if (phim != null)
            {
                // Thay đổi trạng thái phim thành "Ẩn"
                phim.TrangThai = "Ẩn"; // Đảm bảo bạn có cột "TrangThai" trong bảng Phim
                _context.Update(phim);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

    }
}
