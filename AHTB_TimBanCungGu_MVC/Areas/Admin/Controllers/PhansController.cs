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
                var phimData = await _context.Phan
                    .Include(p => p.Phim)  // Bao gồm thông tin về phim
                    .OrderBy(p => p.PhimID).ThenBy(p => p.SoPhan)  // Sắp xếp theo IDPhim và SoPhan
                    .ToListAsync(); // Lấy danh sách tất cả các phần phim

                return View(phimData); // Trả về View với dữ liệu Phan
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
        public async Task<IActionResult> Create([Bind("SoPhan,NgayCongChieu,SoLuongTap,PhimID")] Phan phan)
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
                    ModelState.AddModelError("SoPhan", "Số phần này đã tồn tại. Vui lòng chọn số khác.");
                }
                else
                {
                    // Tính toán IDPhan và đảm bảo không bị trùng lặp
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

                    // Đặt IDPhan cho phần phim mới
                    phan.IDPhan = newIDPhan;

                    // Lưu phần phim vào cơ sở dữ liệu
                    _context.Phan.Add(phan);
                    await _context.SaveChangesAsync();

                    // Tạo các tập phim
                    string lasttapId = _context.Tap.OrderByDescending(t => t.IDTap).Select(t => t.IDTap).FirstOrDefault();
                    int nexttapId = (lasttapId == null) ? 1 : int.Parse(lasttapId.Substring(1)) + 1;
                    string newIDTap = "T" + nexttapId.ToString();

                    // Đảm bảo IDTap không bị trùng lặp
                    while (await _context.Tap.AnyAsync(t => t.IDTap == newIDTap))
                    {
                        nexttapId++;
                        newIDTap = "T" + nexttapId.ToString();
                    }

                    // Tạo các tập phim liên quan đến phần phim này
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
                    }

                    // Lưu tất cả vào cơ sở dữ liệu
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }

            // Nếu có lỗi, trả lại view và gắn lại danh sách phim cho dropdown
            ViewData["PhimID"] = new SelectList(_context.Phim, "IDPhim", "TenPhim", phan.PhimID);
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
                .Include(p => p.Phim)
                .Include(p => p.Tap)
                .OrderBy(p => p.SoPhan)
                .FirstOrDefaultAsync(m => m.IDPhan == id);

            if (phan == null)
            {
                return NotFound();
            }

            // Kiểm tra xem phim có liên kết hay không
            if (phan.Phim == null)
            {
                ModelState.AddModelError("", "Phim không có liên kết với Phan này.");
                return View(phan);
            }
            ViewData["PhimName"] = phan.Phim.TenPhim;  // Truyền tên phim vào ViewData
            ViewData["PhimID"] = phan.PhimID;
            return View(phan);
        }


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
                    var existingPhan = await _context.Phan
                        .Include(p => p.Phim)
                        .Include(p => p.Tap)
                        .FirstOrDefaultAsync(p => p.IDPhan == id);

                    if (existingPhan == null)
                    {
                        return NotFound();
                    }

                    // Cập nhật thông tin phần phim
                    existingPhan.SoPhan = phan.SoPhan;
                    existingPhan.NgayCongChieu = phan.NgayCongChieu;
                    existingPhan.PhimID = phan.PhimID;
                    existingPhan.SoLuongTap = phan.SoLuongTap;

                    // Lấy danh sách tất cả IDTap của phần phim hiện tại (dựa vào IDPhan)
                    var existingTapIds = _context.Tap
                        .Where(t => t.PhanPhim == phan.IDPhan)
                        .Select(t => t.IDTap)
                        .ToList();

                    // Chỉ thêm tập mới nếu SoLuongTap tăng
                    int currentEpisodeCount = existingPhan.Tap.Count;
                    if (phan.SoLuongTap > currentEpisodeCount)
                    {
                        // Thêm tập mới nếu số lượng tăng
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
                    else if (phan.SoLuongTap < currentEpisodeCount)
                    {
                        // Xóa các tập dư thừa nếu số lượng giảm
                        var tapsToDelete = existingPhan.Tap
                            .Where(t => t.SoTap > phan.SoLuongTap)
                            .ToList();

                        _context.Tap.RemoveRange(tapsToDelete);  // Xóa các tập thừa
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

            // Truyền lại tên phim vào ViewData
            var phimName = await _context.Phim
                .Where(p => p.IDPhim == phan.PhimID)
                .Select(p => p.TenPhim)
                .FirstOrDefaultAsync();

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
