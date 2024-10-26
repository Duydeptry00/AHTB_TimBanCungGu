using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AHTB_TimBanCungGu_API.Data;
using AHTB_TimBanCungGu_API.Models;

namespace AHTB_TimBanCungGu_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PhansController : Controller
    {
        private readonly DBAHTBContext _context;

        public PhansController(DBAHTBContext context)
        {
            _context = context;
        }

        // GET: api/Phans
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Phan>>> GetPhans()
        {
            var phans = await _context.Phan.ToListAsync();
            return Ok(phans);
        }

        // GET: api/Phans/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Phan>> GetPhanById(string id)
        {
            var phan = await _context.Phan.FindAsync(id);

            if (phan == null)
            {
                return NotFound();
            }

            return Ok(phan);
        }
        // POST: api/Phans
        [HttpPost]
        public async Task<ActionResult<Phan>> PostPhan(PhanVM phanVM)
        {
            var phan = new Phan
            {
                IDPhan = phanVM.IDPhan,
                SoPhan = phanVM.SoPhan,
                NgayCongChieu = phanVM.NgayCongChieu,
                SoLuongTap = phanVM.SoLuongTap,
                PhimID = phanVM.PhimID,
            };

            _context.Phan.Add(phan);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPhans), new { id = phan.IDPhan }, phan);
        }

        // PUT: api/Phans/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePhan(string id, Phan phanUpdate)
        {
            if (id != phanUpdate.IDPhan)
            {
                return BadRequest();
            }

            var existingPhan = await _context.Phan.FindAsync(id);
            if (existingPhan == null)
            {
                return NotFound();
            }

            existingPhan.SoPhan = phanUpdate.SoPhan;
            existingPhan.NgayCongChieu = phanUpdate.NgayCongChieu;
            existingPhan.SoLuongTap = phanUpdate.SoLuongTap;
            existingPhan.PhimID = phanUpdate.PhimID; // Nếu cần cập nhật cả trường này

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Phan.Any(e => e.IDPhan == id))
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

        // DELETE: api/Phans/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePhan(string id)
        {
            // Tìm phần phim cần xóa
            var phan = await _context.Phan.FindAsync(id);
            if (phan == null)
            {
                return NotFound();
            }

            // Tìm các tập phim liên quan đến phần phim
            var relatedTaps = _context.Tap.Where(t => t.PhanPhim == id).ToList();

            // Xóa các tập phim trước
            if (relatedTaps.Any())
            {
                _context.Tap.RemoveRange(relatedTaps);
            }

            // Xóa phần phim
            _context.Phan.Remove(phan);

            // Lưu thay đổi vào cơ sở dữ liệu
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
