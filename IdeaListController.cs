using BTL.DNU.IdeaSpark.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BTL.DNU.IdeaSpark.Web.Controllers
{
    public class IdeaListController : Controller
    {
        private readonly ApplicationDbContext _context;

        public IdeaListController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? search, string? category, string? status, string? sort)
        {
            var ideas = _context.Ideas.AsQueryable();

            // Tìm kiếm theo tiêu đề hoặc mô tả
            if (!string.IsNullOrEmpty(search))
            {
                ideas = ideas.Where(i => i.Title.Contains(search) || i.Description.Contains(search));
            }

            // Lọc theo danh mục
            if (!string.IsNullOrEmpty(category) && category != "-- Tất cả --")
            {
                ideas = ideas.Where(i => i.Category == category);
            }

            // Lọc theo trạng thái
            if (!string.IsNullOrEmpty(status) && status != "-- Tất cả --")
            {
                ideas = ideas.Where(i => i.Status == status);
            }

            // Sắp xếp theo ngày tạo
            ideas = sort == "oldest"
                ? ideas.OrderBy(i => i.CreatedAt)
                : ideas.OrderByDescending(i => i.CreatedAt);

            // Gửi lại dữ liệu lọc để hiển thị đúng trong View
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentCategory = category;
            ViewBag.CurrentStatus = status;
            ViewBag.CurrentSort = sort;

            return View(await ideas.ToListAsync());
        }

        public async Task<IActionResult> FeaturedIdeas()
        {
            var ideas = await _context.Ideas
                .Include(i => i.Comments)
                .OrderByDescending(i => i.VoteCount + i.Comments.Count)
                .ToListAsync();

            return View(ideas);
        }

        public async Task<IActionResult> Details(int id)
        {
            var idea = await _context.Ideas
                .Include(i => i.Comments)
                .FirstOrDefaultAsync(i => i.IdeaId == id);

            if (idea == null)
            {
                return NotFound();
            }

            return View(idea);
        }
    }
}
