using BTL.DNU.IdeaSpark.Web.Data;
using BTL.DNU.IdeaSpark.Web.Helpers;
using BTL.DNU.IdeaSpark.Web.Hubs;
using BTL.DNU.IdeaSpark.Web.Models;
using BTL.DNU.IdeaSpark.Web.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ➕ Cấu hình dịch vụ
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();
builder.Services.AddSession(); // ⚠️ Không cần gọi 2 lần
builder.Services.AddTransient<EmailService>();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// ❗ Xử lý lỗi
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// ➕ Middleware
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();
app.UseSession();

// ✅ Route mặc định: Danh sách ý tưởng nổi bật
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=IdeaList}/{action=FeaturedIdeas}/{id?}");

// ✅ Route riêng cho Account/Login
app.MapControllerRoute(
    name: "account",
    pattern: "{controller=Account}/{action=Login}/{id?}");

// ✅ Route riêng cho Admin Dashboard
app.MapControllerRoute(
    name: "admindashboard",
    pattern: "Admin/Dashboard",
    defaults: new { controller = "Admin", action = "Dashboard" }
);

// ✅ Đăng ký SignalR hub cho real-time
app.MapHub<IdeaHub>("/ideahub");

// ✅ Tạo admin mặc định nếu chưa có
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    if (!db.Users.Any(u => u.Email == "admin@gmail.com"))
    {
        db.Users.Add(new User
        {
            Email = "admin@gmail.com",
            Role = "Admin",
            PasswordHash = PasswordHelper.HashPassword("admin123")
        });
        db.SaveChanges();
    }
}

app.Run();