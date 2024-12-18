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
                .Where(p => p.DangPhim == "Phim Lẻ")
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
        public async Task<IActionResult> GetPhim([FromQuery] string idPhim, [FromQuery] string senderUsername, [FromQuery] string receiverUserName)
        {
            if (string.IsNullOrEmpty(idPhim))
            {
                return BadRequest("ID phim không được để trống.");
            }

            // Lấy thông tin của Sender và Receiver
            var Name1 = await _context.ThongTinCN
                .Where(N1 => N1.User.UserName == senderUsername)
                .Select(N1 => N1.HoTen)  // Only get 'HoTen' (Full name)
                .FirstOrDefaultAsync();

            var Name2 = await _context.ThongTinCN
                .Where(N1 => N1.User.UserName == receiverUserName)
                .Select(N1 => N1.HoTen)  // Only get 'HoTen' (Full name)
                .FirstOrDefaultAsync();

            // Kiểm tra nếu không tìm thấy thông tin người dùng
            if (Name1 == null || Name2 == null)
            {
                return NotFound("Không tìm thấy thông tin người dùng.");
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

            // Kiểm tra nếu không tìm thấy phim
            if (phim == null)
            {
                return NotFound("Không tìm thấy phim phù hợp.");
            }

            // Trả về dữ liệu phim và thông tin người dùng
            return Ok(new
            {
                Movie = new
                {
                    TenPhim = phim.TenPhim,
                    SourcePhim = phim.SourcePhim,
                    Premium = phim.NoiDungPremium
                },
                SenderFullName = Name1,
                ReceiverFullName = Name2
            });
        }
        // GET: api/XemPhimCungs/GetSourcePhim?idPhim=12345
        [HttpGet("GetSourcePhim")]
        public async Task<IActionResult> GetSourcePhim([FromQuery] string idPhim)
        {
            if (string.IsNullOrEmpty(idPhim))
            {
                return BadRequest("ID phim không được để trống.");
            }

            // Tìm phim dựa trên ID
            var phim = await _context.Phim
                .Where(p => p.IDPhim == idPhim)
                .Select(p => new
                {
                    p.SourcePhim
                })
                .FirstOrDefaultAsync();

            // Kiểm tra nếu không tìm thấy phim
            if (phim == null)
            {
                return NotFound("Không tìm thấy phim phù hợp.");
            }

            // Trả về chỉ SourcePhim
            return Ok(new
            {
                SourcePhim = phim.SourcePhim
            });
        }

    }
}
