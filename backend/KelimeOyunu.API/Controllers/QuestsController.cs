using System.Security.Claims;
using KelimeOyunu.Core.Entities;
using KelimeOyunu.Core.Interfaces;
using KelimeOyunu.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KelimeOyunu.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class QuestsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IQuestManager _questManager;

    public QuestsController(AppDbContext context, IQuestManager questManager)
    {
        _context = context;
        _questManager = questManager;
    }

    [HttpGet]
    public async Task<ActionResult<List<UserDailyQuest>>> GetDailyQuests()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();

        var quests = await _questManager.GetOrAssignDailyQuestsAsync(userId);
        
        // Return without User relation to avoid loop
        foreach(var q in quests) { q.User = null!; }
        
        return Ok(quests);
    }

    [HttpPost("{questId}/claim")]
    public async Task<IActionResult> ClaimReward(Guid questId)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();

        var quest = await _context.DailyQuests
            .FirstOrDefaultAsync(q => q.Id == questId && q.UserId == userId);

        if (quest == null) return NotFound("Görev bulunamadı.");
        if (!quest.IsCompleted) return BadRequest("Görev henüz tamamlanmadı.");
        if (quest.IsClaimed) return BadRequest("Bu görevin ödülü zaten alındı.");

        var user = await _context.Users.FindAsync(userId);
        if (user == null) return NotFound();

        quest.IsClaimed = true;
        user.Gold += quest.RewardGold;
        user.Diamonds += quest.RewardDiamonds;

        await _context.SaveChangesAsync();

        return Ok(new { success = true, gold = user.Gold, diamonds = user.Diamonds });
    }
}
