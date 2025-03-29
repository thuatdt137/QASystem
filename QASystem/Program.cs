// Program.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QASystem.Models;

var builder = WebApplication.CreateBuilder(args);

// Thêm dịch vụ MVC
builder.Services.AddControllersWithViews();

// Thêm DbContext
builder.Services.AddDbContext<QasystemContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("QASystem")));

builder.Services.AddIdentity<User, IdentityRole<int>>()
    .AddEntityFrameworkStores<QasystemContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// Thêm session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Cấu hình middleware
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession(); // Thêm middleware session
app.UseAuthentication(); // Thêm xác thực
app.UseAuthorization();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 404;
        context.Response.Redirect("/Home/NotFound");
    });
});

app.UseStatusCodePagesWithReExecute("/Home/NotFound", "?statusCode={0}");
app.UseStatusCodePagesWithReExecute("/Home/AccessDenied", "?statusCode={0}");

// Xử lý lỗi quyền hạn
app.Use(async (context, next) =>
{
    await next();
    if (context.Response.StatusCode == 403)
    {
        context.Response.Redirect("/Home/AccessDenied");
    }
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();