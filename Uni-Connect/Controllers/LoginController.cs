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
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == model.Email.ToLower());


                if (user == null)
                {
                    ModelState.AddModelError("", "Invalid email or password");
                    return View(model);
                }
                if (!user.IsEmailVerified)
                {
                   
                    string newToken = System.Security.Cryptography.RandomNumberGenerator.GetHexString(32);
                    user.EmailVerificationToken = newToken;
                    user.EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24);
                    await _context.SaveChangesAsync();

                    var verifyUrl = Url.Action("VerifyEmail", "Login",
                        new { token = newToken }, Request.Scheme);

                    string emailBody = $@"
                        <!DOCTYPE html>
                        <html>
                        <body style='font-family:Arial,sans-serif;background:#f4f4f4;padding:20px;'>
                          <div style='max-width:500px;margin:auto;background:#fff;border-radius:8px;padding:30px;'>
                            <h2 style='color:#333;'>Verify Your Uni-Connect Account</h2>
                            <p>Hi {user.Name},</p>
                            <p>Your account hasn't been verified yet. Click the button below to verify your email and access your account.</p>
                            <div style='text-align:center;margin:30px 0;'>
                              <a href='{verifyUrl}' style='background:#4CAF50;color:#fff;padding:12px 28px;
                                 text-decoration:none;border-radius:6px;font-size:16px;display:inline-block;'>
                                Verify My Email
                              </a>
                            </div>
                            <p style='color:#666;font-size:13px;'>This link expires in 24 hours.</p>
                            <hr style='border:none;border-top:1px solid #eee;margin:20px 0;'/>
                            <p style='color:#999;font-size:12px;'>© {DateTime.UtcNow.Year} Uni-Connect · Philadelphia University</p>
                          </div>
                        </body>
                        </html>";

                    await _emailService.SendEmailAsync(user.Email, "Verify Your Uni-Connect Account", emailBody);

                    ModelState.AddModelError("", "Your email isn't verified yet. We've sent you a new verification link — please check your inbox.");
                    return View(model);
                }

                // Block login if email not verified — resend verification link
                if (!user.IsEmailVerified)
                {
                    string newToken = System.Security.Cryptography.RandomNumberGenerator.GetHexString(32);
                    user.EmailVerificationToken = newToken;
                    user.EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24);
                    await _context.SaveChangesAsync();

                    try
                    {
                        var verifyUrl = Url.Action("VerifyEmail", "Login", new { token = newToken }, Request.Scheme);
                        string emailBody = $@"
                            <!DOCTYPE html><html><body style='font-family:Arial,sans-serif;background:#f4f4f4;padding:20px;'>
                              <div style='max-width:500px;margin:auto;background:#fff;border-radius:8px;padding:30px;'>
                                <h2 style='color:#333;'>Verify Your Uni-Connect Account</h2>
                                <p>Hi {user.Name},</p>
                                <p>Your account hasn't been verified yet. Click the button below to verify your email and access your account.</p>
                                <div style='text-align:center;margin:30px 0;'>
                                  <a href='{verifyUrl}' style='background:#4CAF50;color:#fff;padding:12px 28px;text-decoration:none;border-radius:6px;font-size:16px;display:inline-block;'>Verify My Email</a>
                                </div>
                                <p style='color:#666;font-size:13px;'>This link expires in 24 hours.</p>
                              </div>
                            </body></html>";
                        await _emailService.SendEmailAsync(user.Email, "Verify Your Uni-Connect Account", emailBody);
                    }
                    catch { /* Email send failure must not block the login UI response */ }

                    ModelState.AddModelError("", "Your email isn't verified yet. We've sent you a new verification link — please check your inbox.");
                    return View(model);
                }

                // Soft-deleted account — redirect to reactivation page
                if (user.IsDeleted)
                {
                    TempData["ReactivateEmail"] = user.Email;
                    TempData["ReactivateName"] = user.Name;
                    return RedirectToAction("Reactivate_Page");
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
                if (user.IsDeleted)
                {
                    TempData["ReactivateEmail"] = user.Email;
                    TempData["ReactivateName"] = user.Name;
                    return RedirectToAction("Reactivate_Page");
                }

                user.FailedLoginAttempts = 0;
                user.AccountLockedUntil = null;

                // Award daily login bonus (once per day)
                bool awardDailyBonus = !user.LastLoginAt.HasValue ||
                    user.LastLoginAt.Value.Date < DateTime.UtcNow.Date;

                user.LastLoginAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                if (awardDailyBonus)
                    await _pointService.AwardPoints(user.UserID, 2, "Daily Login", "You logged in today!", "📅");

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
            catch (DbUpdateException ex)
            {
<<<<<<< HEAD
                ModelState.AddModelError("", $"Database error: {ex.Message}");
=======
                ModelState.AddModelError("", "Database error. Please try again.");
>>>>>>> 71ee2e5bea89dc860abe7029520086067f2b1b71
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
            string verificationToken = System.Security.Cryptography.RandomNumberGenerator.GetHexString(32);

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
                string verificationToken = System.Security.Cryptography.RandomNumberGenerator.GetHexString(32);

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
<<<<<<< HEAD
                    IsEmailVerified = false,                          
                    EmailVerificationToken = verificationToken,       
                    EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24), 
=======
                    IsEmailVerified = false,
                    EmailVerificationToken = verificationToken,
                    EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24),
>>>>>>> 71ee2e5bea89dc860abe7029520086067f2b1b71
                    CreatedAt = DateTime.UtcNow,
                    ProfileImageUrl = null
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

<<<<<<< HEAD
                var verifyUrl = Url.Action("VerifyEmail", "Login", new { token = verificationToken }, Request.Scheme);
                string emailBody = $@"
                    <!DOCTYPE html>
                    <html>
                    <body style='font-family:Arial,sans-serif;background:#f4f4f4;padding:20px;'>
                      <div style='max-width:500px;margin:auto;background:#fff;border-radius:8px;padding:30px;'>
                        <h2 style='color:#333;'>Welcome to Uni-Connect! 🎉</h2>
                        <p>Hi {newUser.Name},</p>
                        <p>Thanks for registering. Please verify your email address to activate your account.</p>
                        <div style='text-align:center;margin:30px 0;'>
                          <a href='{verifyUrl}' style='background:#4CAF50;color:#fff;padding:12px 28px;
                             text-decoration:none;border-radius:6px;font-size:16px;display:inline-block;'>
                            Verify My Email
                          </a>
                        </div>
                        <p style='color:#666;font-size:13px;'>This link expires in 24 hours. If you didn't register, ignore this email.</p>
                        <hr style='border:none;border-top:1px solid #eee;margin:20px 0;'/>
                        <p style='color:#999;font-size:12px;'>© {DateTime.UtcNow.Year} Uni-Connect · Philadelphia University</p>
                      </div>
                    </body>
                    </html>";

                await _emailService.SendEmailAsync(newUser.Email, "Verify Your Uni-Connect Account", emailBody);
                


=======
                // Send verification email — points awarded AFTER email is confirmed
                var verifyUrl = Url.Action("VerifyEmail", "Login", new { token = verificationToken }, Request.Scheme);
                string emailBody = $@"
                    <!DOCTYPE html><html><body style='font-family:Arial,sans-serif;background:#f4f4f4;padding:20px;'>
                      <div style='max-width:500px;margin:auto;background:#fff;border-radius:8px;padding:30px;'>
                        <h2 style='color:#333;'>Welcome to Uni-Connect! 🎉</h2>
                        <p>Hi {newUser.Name},</p>
                        <p>Thanks for registering. Please verify your email address to activate your account and receive your +50 welcome points.</p>
                        <div style='text-align:center;margin:30px 0;'>
                          <a href='{verifyUrl}' style='background:#4CAF50;color:#fff;padding:12px 28px;text-decoration:none;border-radius:6px;font-size:16px;display:inline-block;'>Verify My Email</a>
                        </div>
                        <p style='color:#666;font-size:13px;'>This link expires in 24 hours. If you didn't register, ignore this email.</p>
                      </div>
                    </body></html>";
>>>>>>> 71ee2e5bea89dc860abe7029520086067f2b1b71

                await _emailService.SendEmailAsync(newUser.Email, "Verify Your Uni-Connect Account", emailBody);

#if DEBUG
                if (_environment.IsDevelopment())
                    TempData["DebugVerifyUrl"] = verifyUrl;
#endif

                TempData["SuccessMessage"] = "Account created! Please check your email to verify your account before signing in.";
                return RedirectToAction("Login_Page");
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError("", $"Database error: {ex.Message}");
                return View(model);
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "An unexpected error occurred. Please try again.");
                return View(model);
            }
        }
        [HttpGet]
        public async Task<IActionResult> VerifyEmail(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "Invalid verification link.";
                return RedirectToAction("Login_Page");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.EmailVerificationToken == token);

            if (user == null)
            {
                TempData["ErrorMessage"] = "Invalid or already used verification link.";
                return RedirectToAction("Login_Page");
            }

            if (user.EmailVerificationTokenExpiry < DateTime.UtcNow)
            {
                TempData["ErrorMessage"] = "Verification link has expired. Please register again or contact support.";
                return RedirectToAction("Login_Page");
            }

            user.IsEmailVerified = true;
            user.EmailVerificationToken = null;
            user.EmailVerificationTokenExpiry = null;
            await _context.SaveChangesAsync();

            
            await _pointService.AwardPoints(user.UserID, 50,
                "Welcome Bonus", "Welcome to UniConnect!", "🎉");

            TempData["SuccessMessage"] = "✅ Email verified! You earned +50 welcome points 🎉 Please sign in.";
            return RedirectToAction("Login_Page");
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
                        <!DOCTYPE html>
                        <html>
                        <body style='font-family:Arial,sans-serif;background:#f4f4f4;padding:20px;'>
                          <div style='max-width:500px;margin:auto;background:#fff;border-radius:8px;padding:30px;'>
                            <h2 style='color:#333;'>Password Reset Request</h2>
                            <p>Hi {user.Name},</p>
                            <p>We received a request to reset your Uni-Connect password. Click the button below to choose a new password. This link expires in <strong>30 minutes</strong>.</p>
                            <div style='text-align:center;margin:30px 0;'>
                              <a href='{resetUrl}' style='background:#4CAF50;color:#fff;padding:12px 28px;
                                 text-decoration:none;border-radius:6px;font-size:16px;display:inline-block;'>
                                Reset My Password
                              </a>
                            </div>
                            <p style='color:#666;font-size:13px;'>If you didn't request a password reset, you can safely ignore this email.</p>
                            <hr style='border:none;border-top:1px solid #eee;margin:20px 0;'/>
                            <p style='color:#999;font-size:12px;'>© {DateTime.UtcNow.Year} Uni-Connect · Philadelphia University</p>
                          </div>
                        </body>
                        </html>";

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
            catch (DbUpdateException ex )
            {
                ModelState.AddModelError("", $"Database error: {ex.Message}");
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
            catch (DbUpdateException ex )
            {
                ModelState.AddModelError("", $"Database error: {ex.Message}");
                return View(model);
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "An unexpected error occurred. Please try again.");
                return View("ResetPass_Page", model);
            }
        }
<<<<<<< HEAD
        [HttpGet]
        public IActionResult Reactivate_Page()
        {
            
            if (TempData["ReactivateEmail"] == null)
                return RedirectToAction("Login_Page");

            
            TempData.Keep("ReactivateEmail");
            TempData.Keep("ReactivateName");

=======

        [HttpGet]
        public async Task<IActionResult> VerifyEmail(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "Invalid verification link.";
                return RedirectToAction("Login_Page");
            }

            var user = await _context.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.EmailVerificationToken == token);

            if (user == null)
            {
                TempData["ErrorMessage"] = "Invalid or already used verification link.";
                return RedirectToAction("Login_Page");
            }

            if (user.EmailVerificationTokenExpiry < DateTime.UtcNow)
            {
                TempData["ErrorMessage"] = "Verification link has expired. Please try to log in and we'll send a new link.";
                return RedirectToAction("Login_Page");
            }

            user.IsEmailVerified = true;
            user.EmailVerificationToken = null;
            user.EmailVerificationTokenExpiry = null;
            await _context.SaveChangesAsync();

            // Award welcome bonus now that email is confirmed
            await _pointService.AwardPoints(user.UserID, 50, "Welcome Bonus", "Welcome to UniConnect!", "🎉");

            TempData["SuccessMessage"] = "✅ Email verified! You earned +50 welcome points 🎉 Please sign in.";
            return RedirectToAction("Login_Page");
        }

        [HttpGet]
        public IActionResult Reactivate_Page()
        {
            if (TempData["ReactivateEmail"] == null)
                return RedirectToAction("Login_Page");

            TempData.Keep("ReactivateEmail");
            TempData.Keep("ReactivateName");
>>>>>>> 71ee2e5bea89dc860abe7029520086067f2b1b71
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reactivate_Page(string confirm)
        {
            var email = TempData["ReactivateEmail"]?.ToString();
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Login_Page");

            var user = await _context.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

            if (user == null || !user.IsDeleted)
                return RedirectToAction("Login_Page");

<<<<<<< HEAD
            // Reactivate
=======
>>>>>>> 71ee2e5bea89dc860abe7029520086067f2b1b71
            user.IsDeleted = false;
            user.FailedLoginAttempts = 0;
            user.AccountLockedUntil = null;
            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

<<<<<<< HEAD
            // Sign them in
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
        new Claim(ClaimTypes.Name, user.Name),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Role, user.Role ?? "User")
    };

=======
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role ?? "User")
            };
>>>>>>> 71ee2e5bea89dc860abe7029520086067f2b1b71
            if (!string.IsNullOrEmpty(user.ProfileImageUrl))
                claims.Add(new Claim("ProfileImageUrl", user.ProfileImageUrl));

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

            TempData["SuccessMessage"] = "Welcome back! Your account has been reactivated.";
            return RedirectToAction("Dashboard", "Dashboard");
        }
    }
}
