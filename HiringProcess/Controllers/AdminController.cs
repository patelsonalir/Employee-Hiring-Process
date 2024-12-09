using HiringProcess.Data;
using HiringProcess.Models;
using Microsoft.AspNetCore.Mvc;

namespace HiringProcess.Controllers
{
    public class AdminController : Controller
    {
        public ApplicationDbContext _dbContext { get; set; }
        public AdminController(ApplicationDbContext applicationDbContext)
        {
            this._dbContext = applicationDbContext;
        }
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(Admin login)
        {
            var model = (from m in _dbContext.Admin
                         where m.UserName == login.UserName && m.Password == login.Password
                         select m).Any();

            if (model)
            {
                var loginInfo = _dbContext.Admin.Where(x => x.UserName == login.UserName && x.Password == login.Password).FirstOrDefault();
                HttpContext.Session.SetString("username", loginInfo.UserName);
                HttpContext.Session.SetString("Id", loginInfo.Id.ToString());

                return RedirectToAction("Index", "AdminDashboard");
            }
            return View();
        }
    }
}
