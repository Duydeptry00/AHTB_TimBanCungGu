    
using AHTB_TimBanCungGu_API.Data;
using AHTB_TimBanCungGu_API.Models; // Adjust the namespace as necessary
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Required for Include extension method
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AHTB_TimBanCungGu_MVC.Controllers
{
    public class LichSuXemController : Controller
    {
        private readonly DBAHTBContext _context;

        public LichSuXemController(DBAHTBContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Lấy các bản ghi Lịch Sử Xem mới nhất, bao gồm thông tin Phim, sắp xếp theo thời gian xem giảm dần
            var latestItems = _context.LichSuXem
                .Include(l => l.Phim) // Bao gồm thông tin Phim
                .OrderByDescending(x => x.ThoiGianXem) // Sắp xếp giảm dần theo Thời Gian Xem
                .Take(8) // Lấy 8 phần tử gần nhất
                .ToList(); // Chuyển thành danh sách

            // Trả về view với dữ liệu
            return View(latestItems);
        }
        [HttpPost]
        public IActionResult LuuLichSuXem(string phimId)
        {
            if (string.IsNullOrEmpty(phimId))
            {
                return BadRequest("ID phim không hợp lệ.");
            }

            var phim = _context.Phim.FirstOrDefault(p => p.IDPhim == phimId);
            if (phim == null)
            {
                return NotFound("Phim không tồn tại.");
            }

            var lichSuXem = new LichSuXem
            {

                PhimDaXem = phimId,
                ThoiGianXem = DateTime.Now
            };

            try
            {
                _context.LichSuXem.Add(lichSuXem);
                _context.SaveChanges();
                return Ok();  // Không trả về thông báo nào
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Lỗi khi lưu lịch sử xem: " + ex.Message);
            }
        }

    }
}