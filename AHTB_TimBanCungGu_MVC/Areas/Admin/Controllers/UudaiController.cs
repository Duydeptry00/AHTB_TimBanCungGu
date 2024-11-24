using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AHTB_TimBanCungGu_API.Data;
using AHTB_TimBanCungGu_API.Models;

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
        public async Task<IActionResult> Create(UuDai uudai)
        {
            if (ModelState.IsValid)
            {
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
            return View(uudai);
        }

        // POST: Uudai/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, UuDai uudai)
        {
            if (id != uudai.IdUuDai)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
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
