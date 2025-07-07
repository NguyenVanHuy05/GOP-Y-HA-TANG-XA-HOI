using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BTL.DNU.IdeaSpark.Web.Data;
using BTL.DNU.IdeaSpark.Web.Models;

namespace BTL.DNU.IdeaSpark.Web.Controllers;

public class UserController : Controller
{
    private readonly ApplicationDbContext _context;

    public UserController(ApplicationDbContext context)
    {
        _context = context;
    }
    [HttpGet]
    public IActionResult Create()
    {
        if (HttpContext.Session.GetString("UserEmail") == null)
            return RedirectToAction("Login", "Account");

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(Idea idea)
    {
        var email = HttpContext.Session.GetString("UserEmail");

        if (string.IsNullOrEmpty(idea.Title) || string.IsNullOrEmpty(idea.Description) || string.IsNullOrEmpty(idea.Category))
        {
            ViewBag.Error = "Vui lòng nhập đầy đủ thông tin.";
            return View();
        }

        idea.SubmitterEmail = email;
        idea.CreatedAt = DateTime.Now;
        idea.Status = "Received"; // Mặc định

        _context.Ideas.Add(idea);
        await _context.SaveChangesAsync();

        TempData["Success"] = "🎉 Gửi ý tưởng thành công!";
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Index()
    {
        var email = HttpContext.Session.GetString("UserEmail");
        if (string.IsNullOrEmpty(email))
        {
            return RedirectToAction("Login", "Account");
        }

        var ideas = await _context.Ideas
            .Where(i => i.SubmitterEmail == email) // ⚠️ Chỉ lấy ý tưởng của người dùng hiện tại
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();

        return View(ideas);
    }

} 