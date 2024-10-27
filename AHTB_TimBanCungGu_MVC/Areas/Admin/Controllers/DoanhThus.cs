using Microsoft.AspNetCore.Mvc;

namespace AHTB_TimBanCungGu_MVC.Areas.Admin.Controllers
{
    public class DoanhThus : Controller
    {
        [Area("Admin")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
