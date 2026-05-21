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
    public class LoginController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;
        private readonly IPointService _pointService;
        private readonly IWebHostEnvironment _environment;

        public LoginController(ApplicationDbContext context, EmailService emailService, IPointService pointService, IWebHostEnvironment environment)
        {
            _context = context;
            _emailService = emailService;
            _pointService = pointService;
            _environment = environment;
        }

        [HttpGet]
        public IActionResult Login_Page()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login_Page(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == model.Email.ToLower());

                // Check null first — do NOT reveal whether email exists via lockout message
                if (user == null)
                {
                    ModelState.AddModelError("", "Invalid email or password");
                    return View(model);
                }

                if (user.AccountLockedUntil.HasValue && user.AccountLockedUntil > DateTime.UtcNow)
                {
                    var timeRemaining = user.AccountLockedUntil.Value - DateTime.UtcNow;
                    ModelState.AddModelError("",
                        $"Account temporarily locked. Try again in {(int)timeRemaining.TotalMinutes + 1} minutes.");
                    return View(model);
                }

                bool isPasswordCorrect = BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash);

                if (!isPasswordCorrect)
                {
                    user.FailedLoginAttempts++;

                    if (user.FailedLoginAttempts >= 5)
                    {
                        user.AccountLockedUntil = DateTime.UtcNow.AddMinutes(15);
                        await _context.SaveChangesAsync();
                        ModelState.AddModelError("",
                            "Too many failed login attempts. Account locked for 15 minutes.");
                        return View(model);
                    }

                    await _context.SaveChangesAsync();
                    ModelState.AddModelError("",
                        $"Invalid email or password. ({user.FailedLoginAttempts}/5 attempts)");
                    return View(model);
                }

                user.FailedLoginAttempts = 0;
                user.AccountLockedUntil = null;
                user.LastLoginAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role ?? "User")
                };
                if (!string.IsNullOrEmpty(user.ProfileImageUrl))
                    claims.Add(new Claim("ProfileImageUrl", user.ProfileImageUrl));

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                return RedirectToAction("Dashboard", "Dashboard");
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Database error: Please try again later.");
                return View(model);
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "An unexpected error occurred. Please try again.");
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Register_Page()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register_Page(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (!model.Email.ToLower().EndsWith("@philadelphia.edu.jo"))
            {
                ModelState.AddModelError("Email", "Only Philadelphia University emails (@philadelphia.edu.jo) are allowed");
                return View(model);
            }

            try
            {
                bool emailExists = await _context.Users
                    .AnyAsync(u => u.Email.ToLower() == model.Email.ToLower());

                if (emailExists)
                {
                    ModelState.AddModelError("Email", "An account with this email already exists");
                    return View(model);
                }

                string universityId = model.Email.Split('@')[0];
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);

                var newUser = new User
                {
                    UniversityID = universityId,
                    Name = model.Name?.Trim() ?? "",
                    Username = universityId,
                    Email = model.Email?.ToLower().Trim() ?? "",
                    PasswordHash = hashedPassword,
                    Role = "Student",
                    Faculty = model.Faculty,
                    YearOfStudy = model.YearOfStudy,
                    Points = 0,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    ProfileImageUrl = null
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                // Award welcome bonus via PointService so transaction record is created
                await _pointService.AwardPoints(newUser.UserID, 50,
                    "Welcome Bonus", "Welcome to UniConnect!", "🎉");

                TempData["SuccessMessage"] = "Account created successfully! You earned +50 welcome points 🎉 Please sign in.";
                return RedirectToAction("Login_Page");
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Database error: Please try again later.");
                return View(model);
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "An unexpected error occurred. Please try again.");
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login_Page");
        }

        [HttpGet]
        public IActionResult ForgotPass_Page()
        {
            return View(new ForgotPasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPass_Page(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == model.Email.ToLower());

                // Use cryptographically secure RNG
                string resetToken = System.Security.Cryptography.RandomNumberGenerator
                    .GetInt32(100000, 1000000).ToString();

                if (user != null)
                {
                    user.PasswordResetToken = resetToken;
                    user.PasswordResetTokenExpiry = DateTime.UtcNow.AddMinutes(30);
                    await _context.SaveChangesAsync();

                    var resetUrl = Url.Action("ResetPass_Page", "Login",
                        new { token = resetToken }, Request.Scheme);

                    string emailBody = $@"
                        <h2>Password Reset - Uni-Connect</h2>
                        <p>Hi {user.Name},</p>
                        <p>Click the link below to reset your password. This link expires in 30 minutes.</p>
                        <a href='{resetUrl}' style='background:#4CAF50;color:white;padding:10px 20px;
                           text-decoration:none;border-radius:5px;'>Reset My Password</a>
                        <p>If you didn't request this, ignore this email.</p>";

                    await _emailService.SendEmailAsync(user.Email, "Reset Your Password", emailBody);

#if DEBUG
                    if (_environment.IsDevelopment())
                        ViewBag.DebugResetUrl = resetUrl;
#endif
                }

                ViewBag.EmailSent = true;
                ViewBag.SentToEmail = model.Email;
                return View(model);
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Database error: Please try again later.");
                return View(model);
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "An unexpected error occurred. Please try again.");
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult ResetPass_Page(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                ModelState.AddModelError("", "Invalid or expired reset link");
                return RedirectToAction("ForgotPass_Page");
            }

            var model = new ResetPasswordViewModel { ResetToken = token };
            return View("ResetPass_Page", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPass_Page(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View("ResetPass_Page", model);

            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.PasswordResetToken == model.ResetToken);

                if (user == null)
                {
                    ModelState.AddModelError("", "Invalid reset code. Please try again.");
                    return View("ResetPass_Page", model);
                }

                if (user.PasswordResetTokenExpiry.HasValue && user.PasswordResetTokenExpiry < DateTime.UtcNow)
                {
                    ModelState.AddModelError("", "Reset code has expired. Please request a new one.");
                    return View("ResetPass_Page", model);
                }

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
                user.PasswordResetToken = null;
                user.PasswordResetTokenExpiry = null;
                user.FailedLoginAttempts = 0;
                user.AccountLockedUntil = null;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "✅ Password reset successfully! Please sign in with your new password.";
                return RedirectToAction("Login_Page");
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Database error: Please try again later.");
                return View("ResetPass_Page", model);
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "An unexpected error occurred. Please try again.");
                return View("ResetPass_Page", model);
            }
        }
    }
}
