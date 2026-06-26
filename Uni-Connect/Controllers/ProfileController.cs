using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Uni_Connect.Models;
using Uni_Connect.Services;

namespace Uni_Connect.Controllers
{
    public class ProfileController : Controller
    {

        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly IPointService _pointService;
        private readonly IPostService _postService;
        private readonly INotificationService _notificationService;

        public ProfileController(ApplicationDbContext context, IWebHostEnvironment environment,
            IPointService pointService, IPostService postService, INotificationService notificationService)
        {
            _context = context;
            _environment = environment;
            _pointService = pointService;
            _postService = postService;
            _notificationService = notificationService;
        }


        public async Task<IActionResult> Profile()
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login_Page", "Login");
            int userId = int.Parse(userIdStr);

            var user = await _context.Users
                .Include(u => u.Posts.Where(p => !p.IsDeleted))
                    .ThenInclude(p => p.Category)
                .Include(u => u.Posts.Where(p => !p.IsDeleted))
                    .ThenInclude(p => p.Answers.Where(a => !a.IsDeleted))
                .Include(u => u.Answers.Where(a => !a.IsDeleted))
                    .ThenInclude(a => a.Post)
                .FirstOrDefaultAsync(u => u.UserID == userId);

            if (user == null) return RedirectToAction("Login_Page", "Login");

            var sessionCount = await _context.PrivateSessions
                .CountAsync(s => (s.StudentID == userId || s.HelperID == userId) && !s.IsDeleted);

            ViewBag.SessionCount = sessionCount;
            return View(user);
        }

        public async Task<IActionResult> ViewProfile(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) return RedirectToAction("Dashboard");

            var userProfile = await _context.Users
                .Include(u => u.Posts.Where(p => !p.IsDeleted))
                    .ThenInclude(p => p.Category)
                .Include(u => u.Posts.Where(p => !p.IsDeleted))
                    .ThenInclude(p => p.Answers.Where(a => !a.IsDeleted))
                .Include(u => u.Answers.Where(a => !a.IsDeleted))
                    .ThenInclude(a => a.Post)
                        .ThenInclude(p => p!.Category)
                .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower() && !u.IsDeleted);

            if (userProfile == null) return RedirectToAction("Dashboard");

            var sessionCount = await _context.PrivateSessions
                .CountAsync(s => (s.StudentID == userProfile.UserID || s.HelperID == userProfile.UserID) && !s.IsDeleted);
            ViewBag.SessionCount = sessionCount;

            return View("Profile", userProfile);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuickUpdatePhoto(IFormFile profileImage)
        {
            var user = await GetCurrentUser();
            if (user == null) return Unauthorized();
            if (profileImage == null || profileImage.Length == 0)
                return Json(new { success = false, message = "No file provided." });

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var allowedMimeTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
            var ext = Path.GetExtension(profileImage.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(ext) ||
                !allowedMimeTypes.Contains(profileImage.ContentType.ToLowerInvariant()) ||
                profileImage.Length > 5 * 1024 * 1024)
                return Json(new { success = false, message = "Invalid file. Use JPG, PNG, GIF or WebP under 5MB." });

            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "profiles");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
            var uniqueFileName = Guid.NewGuid().ToString() + ext;
            using var fs = new FileStream(Path.Combine(uploadsFolder, uniqueFileName), FileMode.Create);
            await profileImage.CopyToAsync(fs);
            user.ProfileImageUrl = $"/uploads/profiles/{uniqueFileName}";

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            var existingClaims = User.Claims.Where(c => c.Type != "ProfileImageUrl").ToList();
            existingClaims.Add(new Claim("ProfileImageUrl", user.ProfileImageUrl));
            var identity = new ClaimsIdentity(existingClaims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

            return Json(new { success = true, imageUrl = user.ProfileImageUrl });
        }

        private async Task<User?> GetCurrentUser()
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return null;
            int userId = int.Parse(userIdStr);
            return await _context.Users.FirstOrDefaultAsync(u => u.UserID == userId);
        }
    }
}
