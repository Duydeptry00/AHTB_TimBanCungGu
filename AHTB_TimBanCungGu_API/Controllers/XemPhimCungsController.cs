using AHTB_TimBanCungGu_API.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace AHTB_TimBanCungGu_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class XemPhimCungsController : ControllerBase
    {
        private readonly DBAHTBContext _context;
        public XemPhimCungsController(DBAHTBContext context)
        {
            _context = context;
        }
        // GET: api/XemPhimCungs/GetAllPhim
        [HttpGet("GetAllPhim")]
        public async Task<IActionResult> GetAllPhim()
        {
            // Lấy tất cả phim từ cơ sở dữ liệu
            var danhSachPhim = await _context.Phim
                .Select(p => new
                {
                    p.IDPhim,
                    p.TenPhim,
                    p.DienVien,
                    p.NgayPhatHanh,
                    p.DanhGia,
                    p.HinhAnh,
                    p.NoiDungPremium
                })
                .ToListAsync();

            if (!danhSachPhim.Any())
            {
                return NotFound("Không có phim nào trong cơ sở dữ liệu.");
            }

            // Trả về danh sách phim
            return Ok(danhSachPhim);
        }
        // GET: api/XemPhimCungs/GetPhim?idPhim=12345
        [HttpGet("GetPhim")]
        public async Task<IActionResult> GetPhim([FromQuery] string idPhim)
        {
            if (string.IsNullOrEmpty(idPhim))
            {
                return BadRequest("ID phim không được để trống.");
            }

            // Tìm phim trong cơ sở dữ liệu dựa trên ID
            var phim = await _context.Phim
                .Where(p => p.IDPhim == idPhim)
                .Select(p => new
                {
                    p.TenPhim,
                    p.SourcePhim,
                    p.NoiDungPremium
                })
                .FirstOrDefaultAsync();

            if (phim == null)
            {
                return NotFound("Không tìm thấy phim phù hợp.");
            }

            // Trả về dữ liệu phim
            return Ok(new
            {
                TenPhim = phim.TenPhim,
                SourcePhim = phim.SourcePhim,
                Premium = phim.NoiDungPremium
            });
        }
    }
}
