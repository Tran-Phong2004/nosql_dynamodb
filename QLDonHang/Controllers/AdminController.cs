using Microsoft.AspNetCore.Mvc;

namespace QLDonHang.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
