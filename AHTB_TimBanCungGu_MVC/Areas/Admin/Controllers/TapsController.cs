using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AHTB_TimBanCungGu_API.Data;
using AHTB_TimBanCungGu_API.Models;

namespace AHTB_TimBanCungGu_MVC.Controllers
{
    [Area("Admin")]
    public class TapsController : Controller
    {
        private readonly DBAHTBContext _context;

        public TapsController(DBAHTBContext context)
        {
            _context = context;
        }

        // GET: Taps
        public async Task<IActionResult> Index()
        {
            var dBAHTBContext = _context.Tap.Include(t => t.Phan);
            return View(await dBAHTBContext.ToListAsync());
        }

        // GET: Taps/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tap = await _context.Tap
                .Include(t => t.Phan)
                .FirstOrDefaultAsync(m => m.IDTap == id);
            if (tap == null)
            {
                return NotFound();
            }

            return View(tap);
        }

        // GET: Taps/Create
        public IActionResult Create()
        {
            ViewData["PhanPhim"] = new SelectList(_context.Phan, "IDPhan", "IDPhan");
            return View();
        }

        // POST: Taps/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IDTap,SoTap,PhanPhim")] Tap tap)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tap);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["PhanPhim"] = new SelectList(_context.Phan, "IDPhan", "IDPhan", tap.PhanPhim);
            return View(tap);
        }

        // GET: Taps/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tap = await _context.Tap.FindAsync(id);
            if (tap == null)
            {
                return NotFound();
            }
            ViewData["PhanPhim"] = new SelectList(_context.Phan, "IDPhan", "IDPhan", tap.PhanPhim);
            return View(tap);
        }

        // POST: Taps/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("IDTap,SoTap,PhanPhim")] Tap tap)
        {
            if (id != tap.IDTap)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tap);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TapExists(tap.IDTap))
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
            ViewData["PhanPhim"] = new SelectList(_context.Phan, "IDPhan", "IDPhan", tap.PhanPhim);
            return View(tap);
        }

        // GET: Taps/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tap = await _context.Tap
                .Include(t => t.Phan)
                .FirstOrDefaultAsync(m => m.IDTap == id);
            if (tap == null)
            {
                return NotFound();
            }

            return View(tap);
        }

        // POST: Taps/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var tap = await _context.Tap.FindAsync(id);
            _context.Tap.Remove(tap);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TapExists(string id)
        {
            return _context.Tap.Any(e => e.IDTap == id);
        }
    }
}
