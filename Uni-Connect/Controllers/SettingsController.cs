using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Uni_Connect.Models;
using Uni_Connect.Services;
using Uni_Connect.ViewModels;

namespace Uni_Connect.Controllers
{
    public class SettingsController : Controller
    {

        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly IPointService _pointService;
        private readonly IPostService _postService;
        private readonly INotificationService _notificationService;

        public SettingsController(ApplicationDbContext context, IWebHostEnvironment environment,
            IPointService pointService, IPostService postService, INotificationService notificationService)
        {
            _context = context;
            _environment = environment;
            _pointService = pointService;
            _postService = postService;
            _notificationService = notificationService;
        }
        [HttpGet]
        public async Task<IActionResult> Settings()
        {
            var user = await GetCurrentUser();
            if (user == null) return RedirectToAction("Login_Page", "Login");

            var model = new SettingsViewModel
            {
                UserID = user.UserID,
                Name = user.Name,
                Email = user.Email,
                Faculty = user.Faculty,
                YearOfStudy = user.YearOfStudy,
                ProfileImageUrl = user.ProfileImageUrl,
                NotifyOnAnswers = user.NotifyOnAnswers,
                NotifyOnSessionRequests = user.NotifyOnSessionRequests
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Settings(SettingsViewModel model, IFormFile? profileImage)
        {
            var user = await GetCurrentUser();
            if (user == null) return RedirectToAction("Login_Page", "Login");

            user.Name = model.Name?.Trim() ?? user.Name;
            user.Faculty = model.Faculty;
            user.YearOfStudy = model.YearOfStudy;


            // Handle profile image upload (file takes priority over URL)
            if (profileImage != null && profileImage.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var allowedMimeTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
                var ext = Path.GetExtension(profileImage.FileName).ToLowerInvariant();

                if (allowedExtensions.Contains(ext) &&
                    allowedMimeTypes.Contains(profileImage.ContentType.ToLowerInvariant()) &&
                    profileImage.Length <= 5 * 1024 * 1024)
                {
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "profiles");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                    var uniqueFileName = Guid.NewGuid().ToString() + ext;
                    using var fs = new FileStream(Path.Combine(uploadsFolder, uniqueFileName), FileMode.Create);
                    await profileImage.CopyToAsync(fs);
                    user.ProfileImageUrl = $"/uploads/profiles/{uniqueFileName}";
                }
                else
                {
                    TempData["ErrorMessage"] = "Invalid file. Use JPG, PNG, GIF or WebP under 5 MB.";
                    return RedirectToAction("Settings");
                }
            }
            else
            {
                user.ProfileImageUrl = model.ProfileImageUrl;
            }

            if (!string.IsNullOrWhiteSpace(model.NewPassword))
            {
                if (string.IsNullOrWhiteSpace(model.CurrentPassword))
                {
                    TempData["ErrorMessage"] = "Current password is required to change password.";
                    return View(model);
                }

                if (model.NewPassword != model.ConfirmPassword)
                {
                    TempData["ErrorMessage"] = "New password and confirmation do not match.";
                    return View(model);
                }

                var passwordError = ValidatePasswordStrength(model.NewPassword);

                if (passwordError != null)
                {
                    TempData["ErrorMessage"] = passwordError;
                    return View(model);
                }

                bool isValid = BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.PasswordHash);
                if (!isValid)
                {
                    TempData["ErrorMessage"] = "Current password is incorrect.";
                    return View(model);
                }

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
                TempData["SuccessMessage"] = "Password updated successfully.";
            }

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            // Re-issue auth cookie so sidebar name & avatar reflect changes immediately
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, User.FindFirst(ClaimTypes.Role)?.Value ?? "User")
            };
            if (!string.IsNullOrEmpty(user.ProfileImageUrl))
                claims.Add(new Claim("ProfileImageUrl", user.ProfileImageUrl));

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)));

            if (TempData["SuccessMessage"] == null)
                TempData["SuccessMessage"] = "Profile settings updated successfully.";

            return RedirectToAction("Settings");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveNotificationSettings(SettingsViewModel model)
        {
            var user = await GetCurrentUser();
            if (user == null) return RedirectToAction("Login_Page", "Login");

            user.NotifyOnAnswers = model.NotifyOnAnswers;
            user.NotifyOnSessionRequests = model.NotifyOnSessionRequests;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Notification preferences saved.";
            return RedirectToAction("Settings");
        }
       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAccount()
        {
            var user = await GetCurrentUser();
            if (user == null) return RedirectToAction("Login_Page", "Login");

            user.IsDeleted = true;
            user.DeletedByAdmin = false;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login_Page", "Login");
        }

        private async Task<User?> GetCurrentUser()
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return null;
            int userId = int.Parse(userIdStr);
            return await _context.Users.FirstOrDefaultAsync(u => u.UserID == userId);
        }


        private string? ValidatePasswordStrength(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return "Password is required.";

            if (password.Length < 8)
                return "Password must be at least 8 characters.";

            if (!password.Any(char.IsUpper))
                return "Password must contain at least one uppercase letter.";

            if (!password.Any(char.IsLower))
                return "Password must contain at least one lowercase letter.";

            if (!password.Any(char.IsDigit))
                return "Password must contain at least one number.";

            if (!password.Any(ch => !char.IsLetterOrDigit(ch)))
                return "Password must contain at least one special character.";

            return null;
        }
    }

}
