using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Uni_Connect.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ===== ADDED: Cookie Authentication =====
// This tells ASP.NET: "Use cookies to track who is logged in"
// When a user logs in, we create a cookie (small piece of data stored in the browser).
// On every page request, ASP.NET reads this cookie to know WHO the user is.
// If they're not logged in and try to access a protected page, redirect to Login page.
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/Login";        // Where to redirect if not logged in
        options.LogoutPath = "/Login/Logout";            // Where to go when logging out
        options.ExpireTimeSpan = TimeSpan.FromHours(24); // Cookie expires after 24 hours
        options.SlidingExpiration = true;                // Resets the 24h timer on each request
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ===== ADDED: Authentication must come BEFORE Authorization =====
// UseAuthentication = "read the cookie and figure out who this user is"
// UseAuthorization  = "check if this user is ALLOWED to access this page"
// Order matters! You can't check permissions before you know who they are.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
