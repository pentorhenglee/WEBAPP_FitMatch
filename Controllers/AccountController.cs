using Microsoft.AspNetCore.Mvc;

namespace WEBAPP_FitMatch.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }
    }
}