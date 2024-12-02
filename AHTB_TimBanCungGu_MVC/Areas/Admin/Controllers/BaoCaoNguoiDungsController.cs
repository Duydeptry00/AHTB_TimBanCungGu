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
    public class BaoCaoNguoiDungsController : Controller
    {
        private readonly DBAHTBContext _context;

        public BaoCaoNguoiDungsController(DBAHTBContext context)
        {
            _context = context;
        }

        // GET: Admin/BaoCaoNguoiDungs
        public async Task<IActionResult> Index()
        {
            // Lấy token JWT và UserType từ session
            var token = HttpContext.Session.GetString("JwtToken");
            var userType = HttpContext.Session.GetString("UserType");

            if (userType == "Admin" && token != null)
            {
                var dBAHTBContext = _context.BaoCaoNguoiDung.Include(b => b.DoiTuongBaoCaoUser).Include(b => b.NguoiBaoCaoUser);
                return View(await dBAHTBContext.ToListAsync());
            }

            return NotFound();

        }
        [HttpPost]
        public async Task<IActionResult> XacNhan(int id)
        {
            // Kiểm tra quyền
            var token = HttpContext.Session.GetString("JwtToken");
            var userType = HttpContext.Session.GetString("UserType");

            if (userType != "Admin" || token == null)
            {
                return Unauthorized();
            }

            // Tìm báo cáo dựa trên ID
            var baoCao = await _context.BaoCaoNguoiDung.FindAsync(id);
            if (baoCao == null)
            {
                return NotFound();
            }

            // Cập nhật trạng thái
            baoCao.TrangThai = "Đã xử lý";
            _context.Update(baoCao);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

    }
}