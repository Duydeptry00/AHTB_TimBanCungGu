using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AHTB_TimBanCungGu_API.Data;
using AHTB_TimBanCungGu_API.Models;

namespace HeThongChieuPhimAHTB_TimBanCungGu_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PhimsController : ControllerBase
    {
        private readonly DBAHTBContext _context;

        public PhimsController(DBAHTBContext context)
        {
            _context = context;
        }
      
        // GET: api/Phims
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Phim>>> GetPhim()
        {
            return await _context.Phim.ToListAsync();
        }

        // GET: api/Phims/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Phim>> GetPhim(string id)
        {
            var phim = await _context.Phim.FindAsync(id);

            if (phim == null)
            {
                return NotFound();
            }

            return phim;
        }

        // PUT: api/Phims/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPhim(string id, Phim phim)
        {
            if (id != phim.IDPhim)
            {
                return BadRequest();
            }

            _context.Entry(phim).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PhimExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Phims
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Phim>> PostPhim(Phim phim)
        {
            _context.Phim.Add(phim);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (PhimExists(phim.IDPhim))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetPhim", new { id = phim.IDPhim }, phim);
        }

        // DELETE: api/Phims/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePhim(string id)
        {
            var phim = await _context.Phim.FindAsync(id);
            if (phim == null)
            {
                return NotFound();
            }

            _context.Phim.Remove(phim);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PhimExists(string id)
        {
            return _context.Phim.Any(e => e.IDPhim == id);
        }
    }
}
