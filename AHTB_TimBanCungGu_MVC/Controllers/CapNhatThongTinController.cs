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
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.User.UserName == userName);  // Thay ToList() bằng FirstOrDefaultAsync()

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

            // Tìm thông tin cá nhân của người dùng theo ID và UserName từ session
            var thongTinCaNhan = await _context.ThongTinCN
                .FirstOrDefaultAsync(t => t.IDProfile == id && t.User.UserName == userName);

            if (thongTinCaNhan == null)
            {
                return Unauthorized(); // Trả về Unauthorized nếu không tìm thấy thông tin của người dùng
            }

            return View(thongTinCaNhan); // Truyền thông tin đến view để hiển thị
        }

        // POST: ThongTinCaNhans/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IDProfile,HoTen,GioiTinh,NgaySinh,SoDienThoai,MoTa")] ThongTinCaNhan thongTinCaNhan)
        {
            if (id != thongTinCaNhan.IDProfile)
            {
                return NotFound(); // Kiểm tra ID có hợp lệ không
            }

            var userName = HttpContext.Session.GetString("TempUserName");
            if (string.IsNullOrEmpty(userName))
            {
                return Unauthorized(); // Trả về Unauthorized nếu session không có giá trị
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Tìm thông tin cá nhân của người dùng theo ID và UserName từ session
                    var existingProfile = await _context.ThongTinCN
                        .FirstOrDefaultAsync(t => t.IDProfile == id && t.User.UserName == userName);

                    if (existingProfile == null)
                    {
                        return NotFound(); // Không tìm thấy hồ sơ
                    }

                    // Cập nhật thông tin
                    existingProfile.HoTen = thongTinCaNhan.HoTen;
                    existingProfile.GioiTinh = thongTinCaNhan.GioiTinh;
                    existingProfile.NgaySinh = thongTinCaNhan.NgaySinh;
                    existingProfile.SoDienThoai = thongTinCaNhan.SoDienThoai;
                    existingProfile.MoTa = thongTinCaNhan.MoTa;

                    // Cập nhật vào cơ sở dữ liệu
                    _context.Update(existingProfile);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.ThongTinCN.Any(e => e.IDProfile == thongTinCaNhan.IDProfile))
                    {
                        return NotFound(); // Nếu không tìm thấy bản ghi trong cơ sở dữ liệu
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index)); // Sau khi cập nhật thành công, chuyển hướng về trang Index
            }

            return View(thongTinCaNhan); // Nếu ModelState không hợp lệ, trả về view với dữ liệu đã nhập
        }

        private bool ThongTinCaNhanExists(int id)
        {
            return _context.ThongTinCN.Any(e => e.IDProfile == id);
        }
    }
}
