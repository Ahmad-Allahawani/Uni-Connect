using Microsoft.AspNetCore.Mvc;

namespace Uni_Connect.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Dashboard()
        {
            return View();
        }
        public IActionResult Profile()
        {
            return View();
        }public IActionResult Notifications()
        {
            return View();
        } 
        public IActionResult Leaderboard()
        {
            return View();
        }
        
    }
}
