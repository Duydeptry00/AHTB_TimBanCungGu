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
using System.IO;

namespace AHTB_TimBanCungGu_MVC.Controllers
{
    public class ThongTinCaNhansController : Controller
    {
        private readonly DBAHTBContext _context;

        public ThongTinCaNhansController(DBAHTBContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var userName = HttpContext.Session.GetString("TempUserName");

            var thongTinCaNhan = await _context.ThongTinCN
          .Include(t => t.User)              // Nạp thông tin người dùng
          .Include(t => t.AnhCaNhan)         // Nạp ảnh cá nhân
          .FirstOrDefaultAsync(t => t.User.UserName == userName);

            if (thongTinCaNhan == null)
            {
                return NotFound();
            }

            return View(thongTinCaNhan); // Truyền một đối tượng duy nhất
        }

        // GET: ThongTinCaNhans/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userName = HttpContext.Session.GetString("TempUserName");

            // Tìm thông tin cá nhân của người dùng theo ID và UserName từ session,
            // bao gồm cả ảnh cá nhân (AnhCaNhan)
            var thongTinCaNhan = await _context.ThongTinCN
                .Include(t => t.AnhCaNhan) // Nạp dữ liệu từ bảng AnhCaNhan
                .FirstOrDefaultAsync(t => t.IDProfile == id && t.User.UserName == userName);

            if (thongTinCaNhan == null)
            {
                return Unauthorized(); // Trả về Unauthorized nếu không tìm thấy thông tin của người dùng
            }

            return View(thongTinCaNhan); // Truyền thông tin đến view để hiển thị
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, IFormFile AnhCaNhanFile, [Bind("IDProfile,HoTen,GioiTinh,NgaySinh,SoDienThoai,DiaChi,MoTa")] ThongTinCaNhan thongTinCaNhan)
        {
            if (id != thongTinCaNhan.IDProfile)
            {
                return NotFound();
            }

            var userName = HttpContext.Session.GetString("TempUserName");
            if (string.IsNullOrEmpty(userName))
            {
                return Unauthorized();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingProfile = await _context.ThongTinCN
                        .Include(t => t.AnhCaNhan) // Nạp danh sách ảnh liên kết với ThongTinCaNhan
                        .FirstOrDefaultAsync(t => t.IDProfile == id && t.User.UserName == userName);

                    if (existingProfile == null)
                    {
                        return NotFound();
                    }

                    // Cập nhật thông tin cá nhân
                    existingProfile.HoTen = thongTinCaNhan.HoTen;
                    existingProfile.GioiTinh = thongTinCaNhan.GioiTinh;
                    existingProfile.NgaySinh = thongTinCaNhan.NgaySinh;
                    existingProfile.SoDienThoai = thongTinCaNhan.SoDienThoai;
                    existingProfile.MoTa = thongTinCaNhan.MoTa;
                    existingProfile.DiaChi = thongTinCaNhan.DiaChi;

                    if (existingProfile.AnhCaNhan.Count >= 7)
                    {
                        ViewBag.ErrorMessage = "Bạn chỉ có thể tải tối đa 7 ảnh cá nhân.";
                        return View(existingProfile); // Trả về lại trang Edit với thông báo lỗi
                    }

                    // Lưu ảnh nếu có file được chọn và nếu chưa đạt số lượng ảnh tối đa
                    if (AnhCaNhanFile != null && AnhCaNhanFile.Length > 0 && existingProfile.AnhCaNhan.Count < 7)
                    {
                        // Tính toán tên file mới theo thứ tự
                        var newFileName = (existingProfile.AnhCaNhan.Count + 1) + Path.GetExtension(AnhCaNhanFile.FileName);

                        // Đường dẫn lưu trữ ảnh
                        var filePath = Path.Combine("wwwroot/uploads", newFileName);

                        // Lưu ảnh vào thư mục
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await AnhCaNhanFile.CopyToAsync(stream);
                        }

                        // Thêm ảnh vào danh sách AnhCaNhan của profile
                        var newAnhCaNhan = new AnhCaNhan
                        {
                            HinhAnh = newFileName,
                            IDProfile = existingProfile.IDProfile
                        };

                        existingProfile.AnhCaNhan.Add(newAnhCaNhan);
                        await _context.SaveChangesAsync();
                    }

                    // Lưu thay đổi vào cơ sở dữ liệu
                    _context.Update(existingProfile);
                    await _context.SaveChangesAsync();

                    // Trả về lại cùng trang Edit sau khi lưu thay đổi
                    return View(existingProfile);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ThongTinCaNhanExists(thongTinCaNhan.IDProfile))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return View(thongTinCaNhan);
        }

        // POST: ThongTinCaNhans/DeleteImage/5
        [HttpPost]
        public async Task<IActionResult> DeleteImage(int id)
        {
            var anh = await _context.AnhCaNhan.FindAsync(id);
            if (anh == null)
            {
                return NotFound();
            }

            // Xóa file ảnh khỏi thư mục lưu trữ
            var filePath = Path.Combine("wwwroot/uploads", anh.HinhAnh);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            // Xóa ảnh khỏi cơ sở dữ liệu
            _context.AnhCaNhan.Remove(anh);
            await _context.SaveChangesAsync();

            return Ok();
        }
        public IActionResult LoadImages(int id)
        {
            var profile = _context.ThongTinCN.Include(p => p.AnhCaNhan).FirstOrDefault(p => p.IDProfile == id);
            return PartialView("_ImageSection", profile);  // Ensure _ImageSection is your partial view for images.
        }

        private bool ThongTinCaNhanExists(int id)
        {
            return _context.ThongTinCN.Any(e => e.IDProfile == id);
        }
    }
}
