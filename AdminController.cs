using BTL.DNU.IdeaSpark.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BTL.DNU.IdeaSpark.Web.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ Trang chính của Admin – có lọc, tìm kiếm, sắp xếp
        public async Task<IActionResult> Index(string statusFilter, string sortOrder, string searchTitle)
        {
            var email = HttpContext.Session.GetString("UserEmail");
            var role = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(role) || role != "Admin")
                return RedirectToAction("AccessDenied", "Account");

            var query = _context.Ideas.AsQueryable();

            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "-- Tất cả --")
            {
                query = query.Where(i => i.Status == statusFilter);
            }

            if (!string.IsNullOrEmpty(searchTitle))
            {
                query = query.Where(i => i.Title.Contains(searchTitle));
            }

            query = sortOrder == "oldest"
                ? query.OrderBy(i => i.CreatedAt)
                : query.OrderByDescending(i => i.CreatedAt);

            var ideas = await query.ToListAsync();

            return View(ideas);
        }

        // ✅ Trang thống kê (dành cho biểu đồ hoặc phân tích)
        public async Task<IActionResult> Statistics()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            var role = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(role) || role != "Admin")
                return RedirectToAction("AccessDenied", "Account");

            var stats = await _context.Ideas
                .GroupBy(i => i.Status)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            ViewBag.Labels = stats.Select(s => s.Status).ToList();
            ViewBag.Values = stats.Select(s => s.Count).ToList();

            return View(); // Views/Admin/Statistics.cshtml
        }

        // ✅ Trang chủ Dashboard dành cho Admin
        public async Task<IActionResult> Dashboard()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            var role = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(role) || role != "Admin")
                return RedirectToAction("AccessDenied", "Account");

            var totalIdeas = await _context.Ideas.CountAsync();
            var totalUsers = await _context.Users.CountAsync();
            var totalCategories = await _context.Categories.CountAsync();

            var ideaStatuses = await _context.Ideas
                .GroupBy(i => i.Status)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count()
                }).ToListAsync();

            ViewBag.TotalIdeas = totalIdeas;
            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalCategories = totalCategories;

            ViewBag.StatusCounts = ideaStatuses;

            return View(); // Views/Admin/Dashboard.cshtml
        }
        public async Task<IActionResult> ManageUsers()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            var role = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(role) || role != "Admin")
                return RedirectToAction("AccessDenied", "Account");

            // LẤY TOÀN BỘ USER (KHÔNG CHỈ NHỮNG NGƯỜI ĐÃ COMMENT)
            var allUsers = await _context.Users.ToListAsync();

            return View(allUsers);
        }



    }
}
