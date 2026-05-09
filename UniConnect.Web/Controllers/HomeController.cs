using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniConnect.Web.Data;

namespace UniConnect.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly UniConnectDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, UniConnectDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var recentPosts = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Category)
                .OrderByDescending(p => p.CreatedAt)
                .Take(10)
                .ToListAsync();

            var recentRequests = await _context.AcademicRequests
                .Include(r => r.User)
                .Include(r => r.Category)
                .OrderByDescending(r => r.CreatedAt)
                .Take(5)
                .ToListAsync();

            ViewBag.RecentPosts = recentPosts;
            ViewBag.RecentRequests = recentRequests;

            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}
