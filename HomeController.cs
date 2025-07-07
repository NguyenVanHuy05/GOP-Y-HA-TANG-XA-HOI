using BTL.DNU.IdeaSpark.Models;
using BTL.DNU.IdeaSpark.Web.Data;
using BTL.DNU.IdeaSpark.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace BTL.DNU.IdeaSpark.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ Trang mặc định: tự redirect nếu là User
        public IActionResult Index()
        {
            var role = HttpContext.Session.GetString("UserRole");

            if (role == "User")
            {
                // Chuyển sang trang PublicIdeas nếu là User
                return RedirectToAction("PublicIdeas", "Home");
            }

            // Nếu không phải User (Admin hoặc chưa đăng nhập), hiển thị View Index mặc định (nếu có)
            return RedirectToAction("Login", "Account");
        }

        public async Task<IActionResult> PublicIdeas()
        {
            var ideas = await _context.Ideas
                .Include(i => i.Comments)
                    .ThenInclude(c => c.Replies)
                .OrderByDescending(i => i.VoteCount)
                .ToListAsync();
            return View(ideas);
        }

        public async Task<IActionResult> IdeaList()
        {
            var ideas = await _context.Ideas
                .Include(i => i.Comments)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();
            return View(ideas);
        }

        public async Task<IActionResult> IdeaDetails(int id)
        {
            var idea = await _context.Ideas
                .Include(i => i.Comments)
                    .ThenInclude(c => c.Replies)
                .FirstOrDefaultAsync(i => i.IdeaId == id);

            if (idea == null)
                return NotFound();

            return View(idea);
        }

        [HttpPost]
        public async Task<IActionResult> Vote(int ideaId)
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login", "Account");
            }

            var idea = await _context.Ideas.FindAsync(ideaId);
            if (idea == null) return NotFound();

            var existingVote = await _context.Votes
                .FirstOrDefaultAsync(v => v.IdeaId == ideaId && v.UserEmail == email);

            if (existingVote == null)
            {
                idea.VoteCount++;
                _context.Votes.Add(new Vote
                {
                    IdeaId = ideaId,
                    UserEmail = email,
                    VotedAt = DateTime.Now
                });
            }
            else
            {
                idea.VoteCount = Math.Max(0, idea.VoteCount - 1);
                _context.Votes.Remove(existingVote);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("PublicIdeas");
        }

        [HttpPost]
        public async Task<IActionResult> AddComment(int ideaId, string content)
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "Bạn cần đăng nhập để bình luận.";
                return RedirectToAction("Login", "Account");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin người dùng.";
                return RedirectToAction("Login", "Account");
            }

            var comment = new Comment
            {
                IdeaId = ideaId,
                Content = content,
                CommenterEmail = user.Email,
                CommentedAt = DateTime.Now,
                UserId = user.UserId
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction("IdeaDetails", new { id = ideaId });
        }

        [HttpPost]
        public async Task<IActionResult> ReplyComment(int ideaId, int parentId, string content)
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "Bạn cần đăng nhập để trả lời.";
                return RedirectToAction("Login", "Account");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin người dùng.";
                return RedirectToAction("Login", "Account");
            }

            var reply = new Comment
            {
                IdeaId = ideaId,
                ParentCommentId = parentId,
                Content = content,
                CommenterEmail = email,
                CommentedAt = DateTime.Now,
                UserId = user.UserId
            };

            _context.Comments.Add(reply);
            await _context.SaveChangesAsync();

            return RedirectToAction("IdeaDetails", new { id = ideaId });
        }

        [HttpPost]
        public async Task<IActionResult> LikeComment(int commentId)
        {
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment != null)
            {
                comment.LikeCount++;
                await _context.SaveChangesAsync();
            }

            var ideaId = comment?.IdeaId ?? 0;
            return RedirectToAction("IdeaDetails", new { id = ideaId });
        }
    }
}
