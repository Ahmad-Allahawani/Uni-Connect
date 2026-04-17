using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Uni_Connect.Hubs;
using Uni_Connect.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
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

var app = builder.Build();

// ===== SEED DATABASE WITH FAKE DATA =====
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate();

    // Only seed if no posts exist
    if (!context.Posts.Any())
    {
        // Create test user if it doesn't exist
        var testUser = context.Users.FirstOrDefault(u => u.Email == "test@uni.ac.uk");
        if (testUser == null)
        {
            testUser = new User
            {
                UniversityID = "PU-2023-001",
                Name = "Ahmed Hassan",
                Username = "ahmedhassan",
                Email = "test@uni.ac.uk",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test@1234"),
                Role = "Student",
                Faculty = "IT Faculty",
                YearOfStudy = "3rd Year",
                Points = 250,
                CreatedAt = DateTime.Now
            };
            context.Users.Add(testUser);
            context.SaveChanges();
        }

        // Create other users for more realistic data
        var users = new List<User>();
        string[] faculties = { "IT Faculty", "Engineering", "Business", "Law", "Pharmacy" };
        string[] years = { "1st Year", "2nd Year", "3rd Year", "4th Year" };
        string[] names = { "Sarah Ahmed", "Omar Khan", "Fatima Ali", "Mohammed Saleh", "Lina Hassan", "Rania Abu" };

        for (int i = 0; i < 5; i++)
        {
            var user = new User
            {
                UniversityID = $"PU-2023-{100 + i}",
                Name = names[i],
                Username = names[i].ToLower().Replace(" ", ""),
                Email = $"user{i}@uni.ac.uk",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test@1234"),
                Role = "Student",
                Faculty = faculties[i % faculties.Length],
                YearOfStudy = years[i % years.Length],
                Points = new Random().Next(100, 500),
                CreatedAt = DateTime.Now.AddDays(-new Random().Next(30, 180))
            };
            users.Add(user);
        }
        context.Users.AddRange(users);
        context.SaveChanges();

        // Create categories
        var categories = new List<Category>
        {
            new Category { Name = "Data Structures", Faculty = "IT Faculty" },
            new Category { Name = "Algorithms", Faculty = "IT Faculty" },
            new Category { Name = "Database", Faculty = "IT Faculty" },
            new Category { Name = "Web Development", Faculty = "IT Faculty" },
            new Category { Name = "Thermodynamics", Faculty = "Engineering" },
            new Category { Name = "Finance", Faculty = "Business" },
            new Category { Name = "Constitutional Law", Faculty = "Law" },
            new Category { Name = "Pharmacology", Faculty = "Pharmacy" }
        };
        context.Categories.AddRange(categories);
        context.SaveChanges();

        // Create fake posts
        var posts = new List<Post>
        {
            new Post
            {
                UserID = testUser.UserID,
                CategoryID = categories.First().CategoryID,
                Title = "How does AVL tree LL rotation work exactly?",
                Content = "I understand the theory but I'm confused about the step-by-step LL rotation process. What happens to pointers during rotation? Can someone explain with a diagram or example?",
                ViewsCount = 234,
                Upvotes = 18,
                CreatedAt = DateTime.Now.AddDays(-5)
            },
            new Post
            {
                UserID = users[0].UserID,
                CategoryID = categories.First().CategoryID,
                Title = "BST vs Balanced BST - when should I use each?",
                Content = "In what scenarios is a regular BST better than AVL/Red-Black? Performance-wise, when does the overhead of balancing not pay off?",
                ViewsCount = 156,
                Upvotes = 12,
                CreatedAt = DateTime.Now.AddDays(-4)
            },
            new Post
            {
                UserID = users[1].UserID,
                CategoryID = categories[1].CategoryID,
                Title = "Dijkstra vs Bellman-Ford for shortest path",
                Content = "I'm confused about when to use which algorithm. Both seem to find shortest paths. What are the key differences and when is one better than the other?",
                ViewsCount = 89,
                Upvotes = 7,
                CreatedAt = DateTime.Now.AddDays(-3)
            },
            new Post
            {
                UserID = users[2].UserID,
                CategoryID = categories[2].CategoryID,
                Title = "SQL JOIN performance - LEFT vs INNER",
                Content = "Why does my LEFT JOIN query run slower than INNER JOIN? Are there optimization techniques? What about with multiple tables?",
                ViewsCount = 312,
                Upvotes = 25,
                CreatedAt = DateTime.Now.AddDays(-2)
            },
            new Post
            {
                UserID = users[3].UserID,
                CategoryID = categories[3].CategoryID,
                Title = "React hook dependencies array - when to include variables?",
                Content = "I'm getting warning to include variables in dependency arrays. But when I do, it causes infinite loops. How do I fix this properly?",
                ViewsCount = 178,
                Upvotes = 14,
                CreatedAt = DateTime.Now.AddDays(-1)
            },
            new Post
            {
                UserID = users[4].UserID,
                CategoryID = categories[4].CategoryID,
                Title = "Carnot cycle efficiency - does it ever reach 100%?",
                Content = "I learned that Carnot cycle is the most efficient. Can we ever achieve 100% efficiency in real machines? What are the practical limits?",
                ViewsCount = 92,
                Upvotes = 8,
                CreatedAt = DateTime.Now.AddHours(-12)
            },
            new Post
            {
                UserID = testUser.UserID,
                CategoryID = categories[5].CategoryID,
                Title = "Understanding compound interest vs simple interest",
                Content = "Can someone explain the practical difference? Which is more commonly used in real-world banking and investments?",
                ViewsCount = 145,
                Upvotes = 11,
                CreatedAt = DateTime.Now.AddHours(-6)
            },
            new Post
            {
                UserID = users[0].UserID,
                CategoryID = categories[6].CategoryID,
                Title = "What is the difference between civil and criminal law?",
                Content = "I'm struggling to understand the distinction between these two branches. What are the key differences in justice procedures?",
                ViewsCount = 201,
                Upvotes = 16,
                CreatedAt = DateTime.Now.AddHours(-4)
            }
        };
        context.Posts.AddRange(posts);
        context.SaveChanges();
    }
}

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


app.MapHub<ChatHub>("/chatHub");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();