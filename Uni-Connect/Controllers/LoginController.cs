using Microsoft.AspNetCore.Mvc;

namespace Uni_Connect.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Login_Page()
        {
            return View();
        }
        public IActionResult Register_Page()
        {
            return View();
        }
        public IActionResult ForgotPass_Page()
        {
            return View();
        }
    }
}
