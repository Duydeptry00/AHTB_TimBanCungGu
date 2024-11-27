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
using DocumentFormat.OpenXml.Spreadsheet;
using AHTB_TimBanCungGu_MVC.Models;
using ThongTinCaNhan = AHTB_TimBanCungGu_API.Models.ThongTinCaNhan;
using AnhCaNhan = AHTB_TimBanCungGu_API.Models.AnhCaNhan;

namespace AHTB_TimBanCungGu_MVC.Controllers
{
    public class ThongTinCaNhansController : Controller
    {
        private readonly DBAHTBContext _context;

        public ThongTinCaNhansController(DBAHTBContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(int id)
        {
            // Kiểm tra xem ID có được truyền đúng không
            Console.WriteLine($"ID từ URL: {id}");

            // Lấy JWT token từ Session
            var token = HttpContext.Session.GetString("JwtToken");

            if (!string.IsNullOrEmpty(token))
            {
                var userName = HttpContext.Session.GetString("TempUserName");

                var thongTinCaNhan = await _context.ThongTinCN
                    .Include(t => t.User)              // Nạp thông tin người dùng
                    .Include(t => t.AnhCaNhan)         // Nạp ảnh cá nhân
                    .FirstOrDefaultAsync(t => t.IDProfile == id && t.User.UserName == userName);

                if (thongTinCaNhan != null)
                {
                    // Truyền IDProfile vào ViewBag để có thể sử dụng trong View
                    ViewBag.IdThongTinCaNhan = thongTinCaNhan.IDProfile;
                }
                else
                {
                    // Xử lý trường hợp không tìm thấy thông tin người dùng
                    ViewBag.Message = "Không tìm thấy thông tin người dùng!";
                }
                return View(thongTinCaNhan);
            }
            else
            {
                // Nếu không có token trong session, bạn có thể yêu cầu đăng nhập lại
                return RedirectToAction("Login", "LoginvsRegister");
            }
        }


        public async Task<IActionResult> Edit(int id)
        {
            // Lấy JWT token từ Session
            var token = HttpContext.Session.GetString("JwtToken");

            if (!string.IsNullOrEmpty(token))
            {
                // Kiểm tra ID có hợp lệ không
                if (id <= 0)
                {
                    return NotFound(); // Nếu ID không hợp lệ, trả về lỗi 404
                }

                var userName = HttpContext.Session.GetString("TempUserName");

                // Tìm thông tin cá nhân của người dùng theo ID và UserName từ session
                var thongTinCaNhan = await _context.ThongTinCN
                    .Include(t => t.AnhCaNhan) // Nạp dữ liệu từ bảng AnhCaNhan
                    .FirstOrDefaultAsync(t => t.IDProfile == id && t.User.UserName == userName); // Tìm theo ID và Username

                if (thongTinCaNhan == null)
                {
                    // Nếu không tìm thấy dữ liệu, trả về lỗi 404
                    return NotFound();
                }

                return View(thongTinCaNhan); // Truyền thông tin đến view để hiển thị
            }
            else
            {
                // Nếu không có token, có thể chuyển đến trang đăng nhập
                TempData["Message"] = "Bạn chưa đăng nhập.";
                return RedirectToAction("Login", "LoginvsRegister");
            }
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

            // Kiểm tra lỗi validation ngày sinh và số điện thoại trước khi tiếp tục
            if (thongTinCaNhan.NgaySinh >= DateTime.Now)
            {
                ModelState.AddModelError("NgaySinh", "Ngày sinh phải nhỏ hơn ngày hiện tại.");
            }

            if (!string.IsNullOrEmpty(thongTinCaNhan.SoDienThoai) &&
                (thongTinCaNhan.SoDienThoai.Length < 10 || thongTinCaNhan.SoDienThoai.Length > 11))
            {
                ModelState.AddModelError("SoDienThoai", "Số điện thoại phải có độ dài từ 10 đến 11 ký tự.");
            }

            if (string.IsNullOrEmpty(thongTinCaNhan.HoTen))
            {
                ModelState.AddModelError("HoTen", "Vui lòng nhập họ tên");
            }

            // Nếu có lỗi validation, trả lại form với các lỗi
            if (!ModelState.IsValid)
            {
                var existingProfile = await _context.ThongTinCN
                    .Include(t => t.AnhCaNhan) // Nạp danh sách ảnh liên kết với ThongTinCaNhan
                    .FirstOrDefaultAsync(t => t.IDProfile == id && t.User.UserName == userName);

                return View(existingProfile);  // Trả lại trang Edit với các lỗi và giữ lại hình ảnh
            }

            try
            {
                var existingProfile = await _context.ThongTinCN
                    .Include(t => t.AnhCaNhan)
                    .FirstOrDefaultAsync(t => t.IDProfile == id && t.User.UserName == userName);

                if (existingProfile == null)
                {
                    return NotFound();
                }

                if (existingProfile.AnhCaNhan.Count >= 7)
                {
                    ViewBag.ErrorMessage = "Bạn chỉ có thể tải tối đa 7 ảnh cá nhân.";
                    return View(existingProfile);  // Trả về lại trang Edit với thông báo lỗi
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
                        // Định nghĩa số thứ tự dựa trên số lượng ảnh hiện tại
                        var imageIndex = existingProfile.AnhCaNhan.Count + 1;

                        // Tạo tên file mới theo cú pháp username + số thứ tự
                        var newFileName = $"{userName}_{imageIndex}{Path.GetExtension(AnhCaNhanFile.FileName)}";

                        // Đường dẫn lưu trữ ảnh
                        var filePath = Path.Combine("wwwroot/uploads", newFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await AnhCaNhanFile.CopyToAsync(stream);
                    }

                    var newAnhCaNhan = new AnhCaNhan
                    {
                        HinhAnh = newFileName,
                        IDProfile = existingProfile.IDProfile
                    };

                    existingProfile.AnhCaNhan.Add(newAnhCaNhan);
                }

                // Lưu thay đổi vào cơ sở dữ liệu
                _context.Update(existingProfile);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", new { id = thongTinCaNhan.IDProfile });
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
