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

namespace AHTB_TimBanCungGu_MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class PhansController : Controller
    {
        private readonly DBAHTBContext _context;

        public PhansController(DBAHTBContext context)
        {
            _context = context;
        }

        // GET: Admin/Phans
        public async Task<IActionResult> Index()
        {
            // Lấy token JWT và UserType từ session
            var token = HttpContext.Session.GetString("JwtToken");
            var userType = HttpContext.Session.GetString("UserType");

            if (userType == "Admin" && token != null)
            {
                var dBAHTBContext = _context.Phan.Include(p => p.Phim)  .OrderBy(p => p.SoPhan);
                return View(await dBAHTBContext.ToListAsync());
            }

            return NotFound();
          
        }

        // GET: Admin/Phans/Details/5
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

                var phan = await _context.Phan
                    .Include(p => p.Phim)
                    .FirstOrDefaultAsync(m => m.IDPhan == id);
                if (phan == null)
                {
                    return NotFound();
                }

                return View(phan);
            }

            return NotFound();
        }

        // GET: Admin/Phans/Create
        public IActionResult Create()
        {
            // Lấy token JWT và UserType từ session
            var token = HttpContext.Session.GetString("JwtToken");
            var userType = HttpContext.Session.GetString("UserType");

            if (userType == "Admin" && token != null)
            {
                ViewData["PhimID"] = new SelectList(_context.Phim, "IDPhim", "TenPhim");
                return View();
            }

            return NotFound();
           
        }

        // POST: Admin/Phans/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SoPhan,NgayCongChieu,SoLuongTap,PhimId")] Phan phan)
        {

            if (ModelState.IsValid)
            {
                // Kiểm tra xem phim đã có phần với số phần này chưa
                var existingPhan = await _context.Phan
                    .Where(p => p.PhimID == phan.PhimID && p.SoPhan == phan.SoPhan)
                    .FirstOrDefaultAsync();

                if (existingPhan != null)
                {
                    // Phim đã có phần với số phần này, không cho phép thêm phần mới
                    ModelState.AddModelError(string.Empty, "Phim đã có phần này, không thể thêm phần mới.");
                }
                else
                {
                    // Tính toán IDPhan tiếp theo và đảm bảo không bị trùng lặp
                    string lastphanId = await _context.Phan
                        .OrderByDescending(kh => kh.IDPhan)
                        .Select(kh => kh.IDPhan)
                        .FirstOrDefaultAsync();

                    int nextphanId = (lastphanId == null) ? 1 : int.Parse(lastphanId.Substring(2)) + 1;
                    string newIDPhan = "PH" + nextphanId.ToString();

                    // Đảm bảo IDPhan không bị trùng lặp
                    while (await _context.Phan.AnyAsync(p => p.IDPhan == newIDPhan))
                    {
                        nextphanId++;
                        newIDPhan = "PH" + nextphanId.ToString();
                    }

                    // Tạo phần mới
                    phan.IDPhan = newIDPhan;

                    _context.Phan.Add(phan);
                    await _context.SaveChangesAsync();

                    // Tính toán IDTap tiếp theo và đảm bảo không bị trùng lặp
                    string lasttapId = _context.Tap.OrderByDescending(t => t.IDTap).Select(t => t.IDTap).FirstOrDefault();
                    int nexttapId = (lasttapId == null) ? 1 : int.Parse(lasttapId.Substring(1)) + 1;
                    string newIDTap = "T" + nexttapId.ToString();

                    // Đảm bảo IDTap không bị trùng lặp
                    while (await _context.Tap.AnyAsync(t => t.IDTap == newIDTap))
                    {
                        nexttapId++;
                        newIDTap = "T" + nexttapId.ToString();
                    }

                    // Tạo các tập phim
                    for (int i = 1; i <= phan.SoLuongTap; i++)
                    {
                        var tap = new Tap
                        {
                            IDTap = newIDTap,
                            SoTap = i,
                            PhanPhim = phan.IDPhan
                        };
                        _context.Tap.Add(tap);
                        nexttapId++; // Tăng IDTap tiếp theo
                        newIDTap = "T" + nexttapId.ToString();

                        // Đảm bảo IDTap không bị trùng lặp
                        while (await _context.Tap.AnyAsync(t => t.IDTap == newIDTap))
                        {
                            nexttapId++;
                            newIDTap = "T" + nexttapId.ToString();
                        }
                    }

                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }

            ViewData["PhimId"] = new SelectList(_context.Phim, "IDPhim", "TenPhim", phan.Phim);
            return View(phan);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Lấy dữ liệu Phan từ cơ sở dữ liệu
            var phan = await _context.Phan
                .Include(p => p.Phim).
                Include(p => p.Tap)
                 .OrderBy(p => p.SoPhan)// Nếu cần thông tin liên kết
                .FirstOrDefaultAsync(m => m.IDPhan == id);

            if (phan == null)
            {
                return NotFound();
            }

            // Truyền danh sách Phim vào ViewBag
            ViewData["PhimID"] = new SelectList(_context.Phim, "IDPhim", "TenPhim", phan.PhimID);

            // Trả dữ liệu về View
            return View(phan);
        }

        // POST: Admin/Phans/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("IDPhan,SoPhan,NgayCongChieu,SoLuongTap,PhimID")] Phan phan)
        {
            if (id != phan.IDPhan)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingPhan = await _context.Phan.Include(p => p.Tap).FirstOrDefaultAsync(p => p.IDPhan == id);
                    if (existingPhan == null)
                    {
                        return NotFound();
                    }

                    // Cập nhật thông tin phần phim
                    existingPhan.SoPhan = phan.SoPhan;
                    existingPhan.NgayCongChieu = phan.NgayCongChieu;
                    existingPhan.PhimID = phan.PhimID;
                    existingPhan.SoLuongTap = phan.SoLuongTap;

                    // Lấy danh sách tất cả IDTap hiện có trong database để đảm bảo không bị trùng
                    var existingTapIds = _context.Tap.Select(t => t.IDTap).ToList();

                    // Chỉ thêm tập mới nếu SoLuongTap tăng
                    int currentEpisodeCount = existingPhan.Tap.Count;
                    if (phan.SoLuongTap > currentEpisodeCount)
                    {
                        for (int i = currentEpisodeCount + 1; i <= phan.SoLuongTap; i++)
                        {
                            string newIDTap;
                            int nextTapId = i;

                            // Tạo IDTap duy nhất
                            do
                            {
                                newIDTap = "T" + nextTapId;
                                nextTapId++;
                            } while (existingTapIds.Contains(newIDTap));  // Kiểm tra trong danh sách IDTap đã tồn tại

                            // Thêm tập mới vào DbContext
                            var newTap = new Tap
                            {
                                IDTap = newIDTap,
                                SoTap = i,
                                PhanPhim = phan.IDPhan
                            };

                            _context.Tap.Add(newTap);
                            existingTapIds.Add(newIDTap);  // Thêm vào danh sách IDTap đã tạo
                        }
                    }

                    // Lưu thay đổi
                    _context.Update(existingPhan);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PhanExists(phan.IDPhan))
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

            ViewData["PhimID"] = new SelectList(_context.Phim, "IDPhim", "TenPhim", phan.PhimID);
            return View(phan);
        }



        // GET: Admin/Phans/Delete/5
        public async Task<IActionResult> Delete(string id)
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

                var phan = await _context.Phan
                    .Include(p => p.Phim)
                    .FirstOrDefaultAsync(m => m.IDPhan == id);
                if (phan == null)
                {
                    return NotFound();
                }

                return View(phan);
            }

            return NotFound();
        
        }

        // POST: Admin/Phans/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var phan = await _context.Phan.FindAsync(id);
            _context.Phan.Remove(phan);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PhanExists(string id)
        {
            return _context.Phan.Any(e => e.IDPhan == id);
        }
    }
}
