using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AHTB_TimBanCungGu_MVC.Areas.Admin.Controllers
{
    public class HomeController : Controller
    {
        [Area("Admin")]
        public IActionResult Index()
        {
            // Lấy token JWT và UserType từ session
            var token = HttpContext.Session.GetString("JwtToken");
            var userType = HttpContext.Session.GetString("UserType");

            if (userType != "khach" && token != null )
            {
                return View();
            }

            return NotFound();
        }
    }
}
