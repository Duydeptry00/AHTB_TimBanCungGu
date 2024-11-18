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
    public class HoaDonsController : Controller
    {
        private readonly DBAHTBContext _context;

        public HoaDonsController(DBAHTBContext context)
        {
            _context = context;
        }

        // GET: Admin/HoaDons
        public async Task<IActionResult> Index()
        {
            // Lấy token JWT và UserType từ session
            var token = HttpContext.Session.GetString("JwtToken");
            var userType = HttpContext.Session.GetString("UserType");

            if (userType == "Admin" && token != null)
            {
                var dBAHTBContext = _context.HoaDon.Include(h => h.User);
                return View(await dBAHTBContext.ToListAsync());
            }

            return NotFound();
          
        }

    }
}
