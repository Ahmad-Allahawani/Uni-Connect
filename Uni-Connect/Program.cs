using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Uni_Connect.Hubs;
using Uni_Connect.Models;
using Uni_Connect.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/Login_Page";       
        options.LogoutPath = "/Login/Logout";            
        options.ExpireTimeSpan = TimeSpan.FromHours(24); 
        options.SlidingExpiration = true;              
    });

builder.Services.AddSignalR();
builder.Services.AddScoped<IPointService, PointService>();
builder.Services.AddScoped<IPostService, PostService>();

var app = builder.Build();

// ===== SEED DATABASE WITH FAKE DATA (Development only) =====


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

// ===== ADDED: Authentication must come BEFORE Authorization =====
// UseAuthentication = "read the cookie and figure out who this user is"
// UseAuthorization  = "check if this user is ALLOWED to access this page"
// Order matters! You can't check permissions before you know who they are.
app.UseAuthentication();
app.UseAuthorization();



app.MapHub<Uni_Connect.Hubs.ChatHub>("/chatHub");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();