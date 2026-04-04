using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Uni_Connect.Models;

namespace Uni_Connect.Controllers
{
    /// <summary>
    /// LoginController handles everything related to user authentication:
    /// - Showing the Login page
    /// - Showing the Register page
    /// - Processing the Login form (checking credentials)
    /// - Processing the Register form (creating new users)
    /// - Logging out
    /// 
    /// HOW MVC WORKS (simple explanation):
    /// 1. User visits a URL like /Login/Login_Page
    /// 2. ASP.NET looks at the URL: "Login" = Controller name, "Login_Page" = Action (method) name
    /// 3. It calls the Login_Page() method in this controller
    /// 4. The method returns View() which renders the .cshtml file with the same name
    /// </summary>
    public class LoginController : Controller
    {
        // _context is our "connection" to the database.
        // We use it to read/write Users, Posts, etc.
        // ASP.NET automatically gives us this through "Dependency Injection" (DI) —
        // we just ask for it in the constructor and ASP.NET provides it.
        private readonly ApplicationDbContext _context;

        // Constructor — runs when ASP.NET creates this controller
        // ASP.NET sees we need ApplicationDbContext and automatically provides it
        public LoginController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =====================================================================
        // LOGIN PAGE — GET (when user VISITS the page)
        // =====================================================================
        // [HttpGet] means: "this method runs when the user VISITS the page" (types URL or clicks link)
        // As opposed to [HttpPost] which runs when a FORM IS SUBMITTED
        [HttpGet]
        public IActionResult Login_Page()
        {
            // Simply show the login page with an empty form
            return View(new LoginViewModel());
        }

        // =====================================================================
        // LOGIN PAGE — POST (when user SUBMITS the form)
        // =====================================================================
        // [HttpPost] means: "this method runs when the user clicks the Sign In button"
        // The form data (email, password) is automatically put into the LoginViewModel object
        [HttpPost]
        public async Task<IActionResult> Login_Page(LoginViewModel model)
        {
            // ---- Step 1: Check if the form is valid ----
            // ModelState.IsValid checks ALL the [Required], [EmailAddress] rules we defined
            // in LoginViewModel. If any rule fails, it returns false.
            if (!ModelState.IsValid)
            {
                // Something is wrong (empty email, invalid format, etc.)
                // Return to the same page — the validation errors will show automatically
                return View(model);
            }

            // ---- Step 2: Find the user in the database by email ----
            // We search the Users table for a user with a matching email.
            // .FirstOrDefaultAsync() returns the first match, or null if no match found.
            // We also convert both to lowercase so "Ahmad@PHI.edu.jo" matches "ahmad@phi.edu.jo"
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == model.Email.ToLower());

            // ---- Step 3: Check if user exists ----
            if (user == null)
            {
                // No user found with this email
                // ModelState.AddModelError adds a custom error message to show on the page
                // The first parameter "" means it's a general error (not tied to a specific field)
                ModelState.AddModelError("", "Invalid email or password");
                return View(model);
            }

            // ---- Step 4: Verify the password ----
            // BCrypt.Net.BCrypt.Verify() takes the plain-text password the user typed
            // and compares it against the hashed password stored in the database.
            // It does NOT decrypt the hash — it hashes the input and compares the two hashes.
            bool isPasswordCorrect = BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash);

            if (!isPasswordCorrect)
            {
                // Password doesn't match
                ModelState.AddModelError("", "Invalid email or password");
                return View(model);
                // NOTE: We say "Invalid email or password" for BOTH wrong email and wrong password.
                // This is a security practice — we don't tell hackers WHICH one was wrong.
            }

            // ---- Step 5: Create the authentication cookie (log the user in) ----
            // "Claims" are pieces of information about the logged-in user that we store in the cookie.
            // Think of them as the user's "ID card" that the browser carries on every request.
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()), // User's ID number
                new Claim(ClaimTypes.Name, user.Name),                        // User's display name
                new Claim(ClaimTypes.Email, user.Email),                      // User's email
                new Claim(ClaimTypes.Role, user.Role)                         // "Student" or "Admin"
            };

            // ClaimsIdentity wraps all claims into one "identity document"
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            // ClaimsPrincipal = the actual "person" holding the identity
            var principal = new ClaimsPrincipal(identity);

            // HttpContext.SignInAsync() creates the cookie and sends it to the browser
            // From now on, every request from this browser will include this cookie
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal);

            // ---- Step 6: Update last login time ----
            user.LastLoginAt = DateTime.Now;
            await _context.SaveChangesAsync();

            // ---- Step 7: Redirect to Dashboard ----
            // After successful login, take the user to the Dashboard page
            return RedirectToAction("Dashboard", "Dashboard");
        }

        // =====================================================================
        // REGISTER PAGE — GET (when user VISITS the page)
        // =====================================================================
        [HttpGet]
        public IActionResult Register_Page()
        {
            return View(new RegisterViewModel());
        }

        // =====================================================================
        // REGISTER PAGE — POST (when user SUBMITS the form)
        // =====================================================================
        [HttpPost]
        public async Task<IActionResult> Register_Page(RegisterViewModel model)
        {
            // ---- Step 1: Check if the form is valid ----
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // ---- Step 2: Validate university email domain ----
            // FR1 says: "Only @philadelphia.edu.jo email addresses accepted at registration"
            // We extract the part after @ and check it
            if (!model.Email.ToLower().EndsWith("@philadelphia.edu.jo"))
            {
                ModelState.AddModelError("Email", "Only Philadelphia University emails (@philadelphia.edu.jo) are allowed");
                return View(model);
            }

            // ---- Step 3: Check if email already exists ----
            // We don't want two accounts with the same email
            bool emailExists = await _context.Users
                .AnyAsync(u => u.Email.ToLower() == model.Email.ToLower());

            if (emailExists)
            {
                ModelState.AddModelError("Email", "An account with this email already exists");
                return View(model);
            }

            // ---- Step 4: Extract University ID from email ----
            // Email format is: 202210882@philadelphia.edu.jo
            // We take everything before the @ to get the student ID: "202210882"
            string universityId = model.Email.Split('@')[0];

            // ---- Step 5: Hash the password ----
            // BCrypt.Net.BCrypt.HashPassword() takes "MyPassword123" and turns it into
            // something like "$2a$11$xKz3Qw..." — this is what we store in the database.
            // Even if someone steals the database, they can't figure out the original password.
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);

            // ---- Step 6: Create the new User object ----
            // We fill in all the fields that the User model needs
            var newUser = new User
            {
                UniversityID = universityId,               // Extracted from email
                Name = model.Name,                          // From the form
                Username = universityId,                    // Default username = student ID (can change later)
                Email = model.Email.ToLower(),              // Store in lowercase for consistency
                PasswordHash = hashedPassword,              // The BCrypt hash, NOT the plain password
                Role = "Student",                           // All new registrations are Students (FR1)
                Points = 50,                                // +50 welcome bonus (Section 5.7.1 of your doc)
                IsDeleted = false,                          // Account is active
                CreatedAt = DateTime.Now,                   // When the account was created
                ProfileImageUrl = null                      // No profile picture yet
            };

            // ---- Step 7: Save to database ----
            // _context.Users.Add() stages the new user (prepares it)
            // SaveChangesAsync() actually sends the SQL INSERT command to the database
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            // ---- Step 8: Redirect to Login with success message ----
            // TempData is a way to send a one-time message to the NEXT page.
            // It survives one redirect and then disappears.
            TempData["SuccessMessage"] = "Account created successfully! You earned +50 welcome points 🎉 Please sign in.";
            return RedirectToAction("Login_Page");
        }

        // =====================================================================
        // LOGOUT
        // =====================================================================
        public async Task<IActionResult> Logout()
        {
            // Delete the authentication cookie — user is no longer logged in
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Redirect back to login page
            return RedirectToAction("Login_Page");
        }

        // =====================================================================
        // FORGOT PASSWORD PAGE — GET (shows the email form)
        // =====================================================================
        [HttpGet]
        public IActionResult ForgotPass_Page()
        {
            return View(new ForgotPasswordViewModel());
        }

        // =====================================================================
        // FORGOT PASSWORD PAGE — POST (checks if email exists, shows confirmation)
        // =====================================================================
        [HttpPost]
        public async Task<IActionResult> ForgotPass_Page(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check if a user with this email exists
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == model.Email.ToLower());

            // Always show success message even if email not found (security: don't reveal which emails exist)
            ViewBag.EmailSent = true;
            ViewBag.SentToEmail = model.Email;

            // In a real system, you would send an actual email here.
            // For now, we just show the confirmation screen.
            return View(model);
        }
    }
}
