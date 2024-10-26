using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AHTB_TimBanCungGu_API.Data;
using AHTB_TimBanCungGu_API.Models;

namespace AHTB_TimBanCungGu_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TapsController : ControllerBase
    {
        private readonly DBAHTBContext _context;

        public TapsController(DBAHTBContext context)
        {
            _context = context;
        }

        // GET: api/Taps
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tap>>> GetTap()
        {
            return await _context.Tap.ToListAsync();
        }
        // GET: api/Taps/CountByPhan/{phanID}
        [HttpGet("CountByPhan/{phanID}")]
        public async Task<ActionResult<int>> CountTapsByPhan(string phanID)
        {
            if (string.IsNullOrEmpty(phanID))
            {
                return BadRequest("PhanID không hợp lệ.");
            }

            // Đếm số tập thuộc phần phim có ID là phanID
            var count = await _context.Tap.CountAsync(t => t.PhanPhim == phanID);

            // Nếu không tìm thấy tập nào, trả về 0
            return Ok(count);
        }
        // GET: api/Taps/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Tap>> GetTap(string id)
        {
            var tap = await _context.Tap.FindAsync(id);

            if (tap == null)
            {
                return NotFound();
            }

            return tap;
        }

        // PUT: api/Taps/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTap(string id, Tap tap)
        {
            if (id != tap.IDTap)
            {
                return BadRequest();
            }

            _context.Entry(tap).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TapExists(id))
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

        // POST: api/Taps
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Tap>> PostTap(Tap tap)
        {
            _context.Tap.Add(tap);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (TapExists(tap.IDTap))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetTap", new { id = tap.IDTap }, tap);
        }

        [HttpPost("DeleteExcessTaps")]
        public async Task<IActionResult> DeleteExcessTaps([FromBody] UpdateTapsDto updateDto)
        {
            if (updateDto == null || updateDto.SoLuongTap < 0 || string.IsNullOrEmpty(updateDto.PhanID))
            {
                return BadRequest("Dữ liệu không hợp lệ.");
            }

            // Lấy danh sách tập hiện có của phần phim
            var existingTaps = await _context.Tap
                .Where(t => t.PhanPhim == updateDto.PhanID)
                .OrderBy(t => t.SoTap)
                .ToListAsync();

            if (existingTaps.Count <= updateDto.SoLuongTap)
            {
                return Ok("Không có tập nào cần xóa.");
            }

            // Lấy danh sách các tập cần xóa (từ tập lớn hơn số lượng yêu cầu)
            var tapsToRemove = existingTaps
                .Where(t => t.SoTap > updateDto.SoLuongTap)
                .ToList();

            _context.Tap.RemoveRange(tapsToRemove);
            await _context.SaveChangesAsync();

            return NoContent();
        }

       

        [HttpPost("CreateMultiple")]
        public async Task<IActionResult> CreateMultipleTaps([FromBody] CreateMultipleTapsDto createDto)
        {
            if (createDto == null || createDto.SoLuongTap <= 0 || string.IsNullOrEmpty(createDto.PhanID))
            {
                return BadRequest("Dữ liệu không hợp lệ.");
            }

            // Kiểm tra xem phần có tồn tại không
            var phan = await _context.Phan.FindAsync(createDto.PhanID);
            if (phan == null)
            {
                return NotFound("Phần không tồn tại.");
            }

            // Lấy số tập lớn nhất hiện tại của phần phim
            var maxSoTap = await _context.Tap
                .Where(t => t.PhanPhim == createDto.PhanID)
                .MaxAsync(t => (int?)t.SoTap) ?? 0;

            // Tạo danh sách các tập phim
            List<Tap> taps = new List<Tap>();
            for (int i = 1; i <= createDto.SoLuongTap; i++)
            {
                var tap = new Tap
                {
                    IDTap = Guid.NewGuid().ToString(), // Tạo ID duy nhất cho mỗi tập
                    SoTap = i + maxSoTap, // Gán số tập
                    PhanPhim = createDto.PhanID // Gán ID phần
                };
                taps.Add(tap);
            }

            // Thêm các tập vào DbContext
            _context.Tap.AddRange(taps);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTap", new { id = taps.First().IDTap }, taps);
        }

        private bool TapExists(string id)
        {
            return _context.Tap.Any(e => e.IDTap == id);
        }
    }
}
