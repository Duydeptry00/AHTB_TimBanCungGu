using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AHTB_TimBanCungGu_API.Data;
using AHTB_TimBanCungGu_API.Models;

namespace AHTB_TimBanCungGu_MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ThongTinCaNhansController : Controller
    {
        private readonly DBAHTBContext _context;

        public ThongTinCaNhansController(DBAHTBContext context)
        {
            _context = context;
        }

        // GET: Admin/ThongTinCaNhans
        public async Task<IActionResult> Index()
        {
            var dBAHTBContext = _context.ThongTinCN.Include(t => t.User);
            return View(await dBAHTBContext.ToListAsync());
        }

        // GET: Admin/ThongTinCaNhans/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var thongTinCaNhan = await _context.ThongTinCN
                .Include(t => t.User)
                .FirstOrDefaultAsync(m => m.IDProfile == id);
            if (thongTinCaNhan == null)
            {
                return NotFound();
            }

            return View(thongTinCaNhan);
        }

        // GET: Admin/ThongTinCaNhans/Create
        public IActionResult Create()
        {
            ViewData["UsID"] = new SelectList(_context.Users, "UsID", "UsID");
            return View();
        }

        // POST: Admin/ThongTinCaNhans/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IDProfile,UsID,HoTen,GioiTinh,NgaySinh,SoDienThoai,IsPremium,MoTa,NgayTao,TrangThai")] ThongTinCaNhan thongTinCaNhan)
        {
            if (ModelState.IsValid)
            {
                _context.Add(thongTinCaNhan);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UsID"] = new SelectList(_context.Users, "UsID", "UsID", thongTinCaNhan.UsID);
            return View(thongTinCaNhan);
        }

        // GET: Admin/ThongTinCaNhans/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var thongTinCaNhan = await _context.ThongTinCN.FindAsync(id);
            if (thongTinCaNhan == null)
            {
                return NotFound();
            }
            ViewData["UsID"] = new SelectList(_context.Users, "UsID", "UsID", thongTinCaNhan.UsID);
            return View(thongTinCaNhan);
        }

        // POST: Admin/ThongTinCaNhans/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IDProfile,UsID,HoTen,GioiTinh,NgaySinh,SoDienThoai,IsPremium,MoTa,NgayTao,TrangThai")] ThongTinCaNhan thongTinCaNhan)
        {
            if (id != thongTinCaNhan.IDProfile)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(thongTinCaNhan);
                    await _context.SaveChangesAsync();
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
                return RedirectToAction(nameof(Index));
            }
            ViewData["UsID"] = new SelectList(_context.Users, "UsID", "UsID", thongTinCaNhan.UsID);
            return View(thongTinCaNhan);
        }

        // GET: Admin/ThongTinCaNhans/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var thongTinCaNhan = await _context.ThongTinCN
                .Include(t => t.User)
                .FirstOrDefaultAsync(m => m.IDProfile == id);
            if (thongTinCaNhan == null)
            {
                return NotFound();
            }

            return View(thongTinCaNhan);
        }

        // POST: Admin/ThongTinCaNhans/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var thongTinCaNhan = await _context.ThongTinCN.FindAsync(id);
            _context.ThongTinCN.Remove(thongTinCaNhan);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ThongTinCaNhanExists(int id)
        {
            return _context.ThongTinCN.Any(e => e.IDProfile == id);
        }
        [HttpGet]
        public async Task<IActionResult> Search(string searchString)
        {
            ViewData["CurrentFilter"] = searchString;

            // Truy vấn dữ liệu từ bảng ThongTinCN và include User để lấy thông tin liên quan
            var profileInfos = from p in _context.ThongTinCN.Include(p => p.User)
                               select p;

            // Nếu searchString không trống, tìm kiếm theo các trường hợp
            if (!String.IsNullOrEmpty(searchString))
            {
                profileInfos = profileInfos.Where(p =>
                    p.User.UserName.Contains(searchString) || // Tìm kiếm theo UserName
                    p.HoTen.Contains(searchString) ||         // Tìm kiếm theo Họ Tên
                    p.SoDienThoai.Contains(searchString));    // Tìm kiếm theo Số Điện Thoại
            }

            // Trả về view Index với kết quả tìm kiếm
            return View("Index", await profileInfos.ToListAsync());
        }
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            // Ghi log ID nhận được để kiểm tra
            Console.WriteLine($"ID nhận được: {id}");

            // Tìm bản ghi trong bảng ThongTinCN
            var thongTinCN = await _context.ThongTinCN.FindAsync(id);
            if (thongTinCN == null)
            {
                // Ghi log khi không tìm thấy bản ghi
                Console.WriteLine("Người dùng trong ThongTinCN không tồn tại.");
                return Json(new { success = false, message = "Người dùng không tồn tại trong ThongTinCN." });
            }

            // Tìm bản ghi tương ứng trong bảng User
            var user = await _context.Users.FindAsync(thongTinCN.UsID);
            if (user == null)
            {
                // Ghi log khi không tìm thấy bản ghi
                Console.WriteLine("Người dùng trong bảng User không tồn tại.");
                return Json(new { success = false, message = "Người dùng không tồn tại trong bảng User." });
            }

            // Đảo ngược trạng thái khóa/mở
            var newStatus = thongTinCN.TrangThai == "Hoạt động" ? "Không hoạt động" : "Hoạt động";
            thongTinCN.TrangThai = newStatus;
            user.TrangThai = newStatus;

            // Cập nhật cả hai bảng
            _context.ThongTinCN.Update(thongTinCN);
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Json(new { success = true, status = newStatus });
        }
    }
}