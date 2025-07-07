using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using BTL.DNU.IdeaSpark.Web.Data;
using BTL.DNU.IdeaSpark.Web.Models;
using BTL.DNU.IdeaSpark.Web.Hubs;

namespace BTL.DNU.IdeaSpark.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IdeasController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<IdeaHub> _hub;

    public IdeasController(ApplicationDbContext context, IHubContext<IdeaHub> hub)
    {
        _context = context;
        _hub = hub;
    }

    [HttpGet]
    public async Task<IActionResult> GetIdeas()
    {
        var ideas = await _context.Ideas.OrderByDescending(i => i.CreatedAt).ToListAsync();
        return Ok(ideas);
    }

    [HttpPost]
    public async Task<IActionResult> PostIdea([FromBody] Idea idea)
    {
        idea.CreatedAt = DateTime.Now;
        _context.Ideas.Add(idea);
        await _context.SaveChangesAsync();

        await _hub.Clients.All.SendAsync("ReceiveUpdate", $"Ý tưởng mới: {idea.Title}");

        return Ok(idea);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] string newStatus)
    {
        var idea = await _context.Ideas.FindAsync(id);
        if (idea == null) return NotFound();

        idea.Status = newStatus;
        await _context.SaveChangesAsync();

        await _hub.Clients.All.SendAsync("ReceiveUpdate", $"Trạng thái cập nhật: {idea.Title} → {newStatus}");

        return Ok(idea);
    }
    [HttpPut("approve/{id}")]
    public async Task<IActionResult> ApproveIdea(int id, [FromQuery] bool approve)
    {
        var idea = await _context.Ideas.FindAsync(id);
        if (idea == null) return NotFound();

        idea.IsApproved = approve;
        await _context.SaveChangesAsync();

        return Ok(new { success = true, status = approve ? "Approved" : "Rejected" });
    }
    [HttpPut("set-label/{id}")]
    public async Task<IActionResult> SetStatusLabel(int id, [FromQuery] string label)
    {
        var idea = await _context.Ideas.FindAsync(id);
        if (idea == null) return NotFound();

        idea.StatusLabel = label;
        await _context.SaveChangesAsync();

        return Ok(new { success = true, label });
    }


}