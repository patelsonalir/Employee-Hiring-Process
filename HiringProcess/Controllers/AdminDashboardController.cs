using Microsoft.AspNetCore.Mvc;

namespace HiringProcess.Controllers
{
    public class AdminDashboardController : Controller
    {
        public IActionResult Index()
        {
            ViewBag.Name = HttpContext.Session.GetString("username");

            return View();
        }
    }
}
