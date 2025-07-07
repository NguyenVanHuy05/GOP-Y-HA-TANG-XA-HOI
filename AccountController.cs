using BTL.DNU.IdeaSpark.Web.Data;
using BTL.DNU.IdeaSpark.Web.Helpers;
using BTL.DNU.IdeaSpark.Web.Models;
using BTL.DNU.IdeaSpark.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace BTL.DNU.IdeaSpark.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;

        public AccountController(ApplicationDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password, string role)
        {
            email = email?.Trim();
            password = password?.Trim();
            role = role?.Trim();

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(role))
            {
                ViewBag.Error = "Vui lòng điền đầy đủ thông tin.";
                return View();
            }

            if (role == "Người dùng") role = "User";
            if (role == "Quản trị viên") role = "Admin";

            string hashed = PasswordHelper.HashPassword(password);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email && u.Role == role);

            if (user != null)
            {
                if (user.PasswordHash != hashed)
                {
                    ViewBag.Error = "Mật khẩu không đúng.";
                    return View();
                }
            }
            else
            {
                if (role == "Admin")
                {
                    ViewBag.Error = "Tài khoản Admin không tồn tại.";
                    return View();
                }

                user = new User
                {
                    Email = email,
                    Role = "User",
                    PasswordHash = hashed,
                    AvatarUrl = null
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }

            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("UserRole", user.Role);

            return role == "Admin"
                ? RedirectToAction("Dashboard", "Admin")
                : RedirectToAction("PublicIdeas", "Home");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (existingUser != null)
            {
                ViewBag.Error = "Email đã được sử dụng.";
                return View(model);
            }

            string otp = new Random().Next(100000, 999999).ToString();
            HttpContext.Session.SetString("OTP", otp);
            HttpContext.Session.SetString("PendingEmail", model.Email);
            HttpContext.Session.SetString("PendingPassword", model.Password);
            HttpContext.Session.SetString("PendingFullName", model.FullName);
            HttpContext.Session.SetString("PendingDepartment", model.Department);

            await _emailService.SendEmailAsync(model.Email, "Xác thực đăng ký", $"Mã OTP của bạn là: {otp}");

            return RedirectToAction("VerifyOtp");
        }

        [HttpGet]
        public IActionResult VerifyOtp()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> VerifyOtp(string enteredOtp)
        {
            var otp = HttpContext.Session.GetString("OTP");
            var email = HttpContext.Session.GetString("PendingEmail");
            var password = HttpContext.Session.GetString("PendingPassword");
            var fullName = HttpContext.Session.GetString("PendingFullName");
            var department = HttpContext.Session.GetString("PendingDepartment");

            if (enteredOtp == otp)
            {
                var newUser = new User
                {
                    Email = email,
                    Role = "User",
                    PasswordHash = PasswordHelper.HashPassword(password),
                    FullName = fullName,
                    Department = department
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                ViewBag.Success = "Đăng ký thành công! Vui lòng đăng nhập.";
                return RedirectToAction("Login");
            }

            ViewBag.Error = "Mã OTP không đúng.";
            return View();
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        public IActionResult HashAdminPassword()
        {
            string raw = "123789";
            string hashed = PasswordHelper.HashPassword(raw);
            return Content($"Hash: {hashed}");
        }

        // ✅ BƯỚC 2 – Giao diện nhập email để đặt lại mật khẩu
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                ViewBag.Message = "Email không tồn tại trong hệ thống.";
                return View();
            }

            string token = Guid.NewGuid().ToString();
            user.ResetToken = token;
            user.ResetTokenExpiry = DateTime.Now.AddHours(1);
            await _context.SaveChangesAsync();

            var resetLink = Url.Action("ResetPassword", "Account", new { token }, Request.Scheme);
            await _emailService.SendEmailAsync(email, "Đặt lại mật khẩu", $"Bấm vào liên kết để đặt lại mật khẩu: {resetLink}");

            ViewBag.Message = "Đã gửi email đặt lại mật khẩu.";
            return View();
        }

        // ✅ BƯỚC 4 – Hiển thị form đặt lại mật khẩu
        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            var user = _context.Users.FirstOrDefault(u => u.ResetToken == token && u.ResetTokenExpiry > DateTime.Now);
            if (user == null)
            {
                return Content("Liên kết không hợp lệ hoặc đã hết hạn.");
            }

            ViewBag.Token = token;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(string token, string newPassword)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.ResetToken == token && u.ResetTokenExpiry > DateTime.Now);
            if (user == null)
            {
                return Content("Liên kết không hợp lệ hoặc đã hết hạn.");
            }

            user.PasswordHash = PasswordHelper.HashPassword(newPassword);
            user.ResetToken = null;
            user.ResetTokenExpiry = null;
            await _context.SaveChangesAsync();

            ViewBag.Success = "Mật khẩu đã được đặt lại thành công. Vui lòng đăng nhập.";
            return RedirectToAction("Login");
        }
    }
}
