using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Uni_Connect.Models;
using Uni_Connect.ViewModels;

namespace Uni_Connect.Controllers
{
    public class LoginController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LoginController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Find user by email
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == model.Email.ToLower());

                // ===== Task #5: Check if account is locked =====
                if (user != null && user.AccountLockedUntil.HasValue && user.AccountLockedUntil > DateTime.Now)
                {
                    TimeSpan timeRemaining = user.AccountLockedUntil.Value - DateTime.Now;
                    ModelState.AddModelError("",
                        $"Account temporarily locked. Try again in {(int)timeRemaining.TotalMinutes + 1} minutes.");
                    return View(model);
                }

                // Check if user exists
                if (user == null)
                {
                    ModelState.AddModelError("", "Invalid email or password");
                    return View(model);
                }

                // Verify password
                bool isPasswordCorrect = BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash);

                if (!isPasswordCorrect)
                {
                    // ===== Task #5: Track failed attempts =====
                    user.FailedLoginAttempts++;

                    if (user.FailedLoginAttempts >= 5)
                    {
                        user.AccountLockedUntil = DateTime.Now.AddMinutes(15);
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

                // ===== Login successful — reset failed attempts =====
                user.FailedLoginAttempts = 0;
                user.AccountLockedUntil = null;
                user.LastLoginAt = DateTime.Now;
                await _context.SaveChangesAsync();

                // Create authentication cookie
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role)
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal);

                return RedirectToAction("Dashboard", "Dashboard");
            }
            catch (DbUpdateException)
            {
                // Task #3: Database error handling
                ModelState.AddModelError("", "Database error: Please try again later.");
                return View(model);
            }
            catch (Exception)
            {
                // Task #3: General error handling
                ModelState.AddModelError("", "An unexpected error occurred. Please try again.");
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Validate university email domain
            if (!model.Email.ToLower().EndsWith("@philadelphia.edu.jo"))
            {
                ModelState.AddModelError("Email", "Only Philadelphia University emails (@philadelphia.edu.jo) are allowed");
                return View(model);
            }

            try
            {
                // Check if email already exists
                bool emailExists = await _context.Users
                    .AnyAsync(u => u.Email.ToLower() == model.Email.ToLower());

                if (emailExists)
                {
                    ModelState.AddModelError("Email", "An account with this email already exists");
                    return View(model);
                }

                // Extract University ID from email
                string universityId = model.Email.Split('@')[0];

                // Hash the password
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);

                // Task #6: Input Sanitization — Trim spaces from name and email
                var newUser = new User
                {
                    UniversityID = universityId,
                    Name = model.Name?.Trim() ?? "",
                    Username = universityId,
                    Email = model.Email?.ToLower().Trim() ?? "",
                    PasswordHash = hashedPassword,
                    Role = "Student",
                    Points = 50,
                    IsDeleted = false,
                    CreatedAt = DateTime.Now,
                    ProfileImageUrl = null
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Account created successfully! You earned +50 welcome points 🎉 Please sign in.";
                return RedirectToAction("Login");
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

        // =====================================================================
        // LOGOUT
        // =====================================================================
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            // Step 1: Validate form is filled out correctly
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Step 2: Find user by email
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == model.Email.ToLower());

                // Step 3: Generate a random 6-digit reset token (100000-999999)
                string resetToken = new Random().Next(100000, 999999).ToString();
                
                // Step 4: If user exists, store the token in database
                if (user != null)
                {
                    // Store token + set expiry to 30 minutes from now
                    user.PasswordResetToken = resetToken;
                    user.PasswordResetTokenExpiry = DateTime.Now.AddMinutes(30);
                    
                    await _context.SaveChangesAsync();

                    // TODO: Send email with reset link here
                    // Example of what email should contain:
                    // "Click here to reset your password: 
                    //  https://yourwebsite.com/Login/ResetPassword?token=ABC123DEF456"
                    // For now, the 6-digit code is stored in DB for manual verification
                }

                // Step 5: Always show success page (security: don't reveal if email exists)
                ViewBag.EmailSent = true;
                ViewBag.SentToEmail = model.Email;
                return View(model);
            }
            catch (DbUpdateException)
            {
                // Database error
                ModelState.AddModelError("", "Database error: Please try again later.");
                return View(model);
            }
            catch (Exception)
            {
                // Unexpected error
                ModelState.AddModelError("", "An unexpected error occurred. Please try again.");
                return View(model);
            }
        }

        // =====================================================================
        // RESET PASSWORD PAGE — GET (shows reset form with token)
        // =====================================================================
        // This action is called when user visits: /Login/ResetPassword?token=XXXXX
        // It displays the form where they enter their new password
        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            // If no token provided, redirect back to forgot password page
            if (string.IsNullOrEmpty(token))
            {
                ModelState.AddModelError("", "Invalid or expired reset link");
                return RedirectToAction("ForgotPassword");
            }
            
            // Create a new ViewModel and put the token in it (hidden field in form)
            var model = new ResetPasswordViewModel { ResetToken = token };
            return View("ResetPassword", model);
        }

        // =====================================================================
        // RESET PASSWORD PAGE — POST (validates token and updates password)
        // =====================================================================
        // This action is called when user clicks "Reset Password" button on the form
        // It validates the token and updates the password
        [HttpPost]
        [ValidateAntiForgeryToken]  // ← CSRF protection
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            // Step 1: Validate the form (token, password, confirm match, etc.)
            if (!ModelState.IsValid)
            {
                return View("ResetPassword", model);
            }

            try
            {
                // Step 2: Find user with matching reset token
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.PasswordResetToken == model.ResetToken);

                // Step 3: Validate token exists in database
                if (user == null)
                {
                    ModelState.AddModelError("", "Invalid reset code. Please try again.");
                    return View("ResetPassword", model);
                }

                // Step 4: Validate token hasn't expired (30 minutes)
                if (user.PasswordResetTokenExpiry.HasValue && user.PasswordResetTokenExpiry < DateTime.Now)
                {
                    ModelState.AddModelError("", "Reset code has expired. Please request a new one.");
                    return View("ResetPassword", model);
                }

                // Step 5: Hash new password using BCrypt
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);

                // Step 6: Update user password and clear reset token
                user.PasswordHash = hashedPassword;
                user.PasswordResetToken = null;           // Clear the token
                user.PasswordResetTokenExpiry = null;     // Clear the expiry
                user.FailedLoginAttempts = 0;             // Reset failed attempts counter
                user.AccountLockedUntil = null;           // Unlock if locked

                // Step 7: Save changes to database
                await _context.SaveChangesAsync();

                // Step 8: Redirect to login with success message
                TempData["SuccessMessage"] = "✅ Password reset successfully! Please sign in with your new password.";
                return RedirectToAction("Login");
            }
            catch (DbUpdateException)
            {
                // Database error
                ModelState.AddModelError("", "Database error: Please try again later.");
                return View("ResetPassword", model);
            }
            catch (Exception)
            {
                // Unexpected error
                ModelState.AddModelError("", "An unexpected error occurred. Please try again.");
                return View("ResetPassword", model);
            }
        }
    }
}
