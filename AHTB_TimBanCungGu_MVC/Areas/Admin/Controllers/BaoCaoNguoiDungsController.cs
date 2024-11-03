using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AHTB_TimBanCungGu_API.Data;
using AHTB_TimBanCungGu_API.Models;

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
            var dBAHTBContext = _context.BaoCaoNguoiDung.Include(b => b.DoiTuongBaoCaoUser).Include(b => b.NguoiBaoCaoUser);
            return View(await dBAHTBContext.ToListAsync());
        }
    } 
}
