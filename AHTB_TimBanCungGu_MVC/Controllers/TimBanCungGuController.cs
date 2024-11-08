using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AHTB_TimBanCungGu_API.Data;
using AHTB_TimBanCungGu_API.Models;
using AHTB_TimBanCungGu_API.ViewModels;

namespace AHTB_TimBanCungGu_MVC.Controllers
{
    public class TimBanCungGuController : Controller
    {
        private readonly DBAHTBContext _context;

        public TimBanCungGuController(DBAHTBContext context)
        {
            _context = context;
        }

        // GET: TimBanCungGu
        public async Task<IActionResult> TrangChu()
        {
            var dBAHTBContext = _context.ThongTinCN
      .Include(t => t.User)
      .Include(t => t.AnhCaNhan);


            // Chuyển dữ liệu thành danh sách ViewModel
            var thongTinCaNhanViewModels = await dBAHTBContext.Select(t => new InfoNguoiDung
            {
                IDProfile = t.IDProfile,
                UsID = t.UsID,
                HoTen = t.HoTen,
                Email = t.Email,
                GioiTinh = t.GioiTinh,
                NgaySinh = t.NgaySinh,
                SoDienThoai = t.SoDienThoai,
                IsPremium = t.IsPremium,
                MoTa = t.MoTa,
                NgayTao = t.NgayTao,
                TrangThai = t.TrangThai,
                HinhAnh = t.AnhCaNhan.Select(a => a.HinhAnh).ToList() ?? new List<string>()
            }).ToListAsync();

            // Trả về view và truyền dữ liệu qua ViewModel
            return View(thongTinCaNhanViewModels);
        }

        // GET: TimBanCungGu/Details/5
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

        // GET: TimBanCungGu/Create
        public IActionResult Create()
        {
            ViewData["UsID"] = new SelectList(_context.Users, "UsID", "UsID");
            return View();
        }

        // POST: TimBanCungGu/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IDProfile,UsID,Email,HoTen,GioiTinh,NgaySinh,SoDienThoai,IsPremium,MoTa,NgayTao,TrangThai")] ThongTinCaNhan thongTinCaNhan)
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

        // GET: TimBanCungGu/Edit/5
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

        // POST: TimBanCungGu/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IDProfile,UsID,Email,HoTen,GioiTinh,NgaySinh,SoDienThoai,IsPremium,MoTa,NgayTao,TrangThai")] ThongTinCaNhan thongTinCaNhan)
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

        // GET: TimBanCungGu/Delete/5
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

        // POST: TimBanCungGu/Delete/5
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

    }
}
