using System.Security.Claims;
using KelimeOyunu.Core.DTOs;
using KelimeOyunu.Core.Interfaces;
using KelimeOyunu.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KelimeOyunu.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IEconomyManager _economyManager;
    private readonly AppDbContext _context;

    public ProfileController(IEconomyManager economyManager, AppDbContext context)
    {
        _economyManager = economyManager;
        _context = context;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpGet]
    public async Task<ActionResult<UserProfileDto>> GetProfile()
    {
        return Ok(await _economyManager.GetBalanceAsync(GetUserId()));
    }

    [HttpPut("username")]
    public async Task<IActionResult> UpdateUsername([FromBody] string newUsername)
    {
        var user = await _context.Users.FindAsync(GetUserId());
        if (user == null) return NotFound();
        user.Username = newUsername;
        await _context.SaveChangesAsync();
        return Ok(new { message = "Kullanıcı adı güncellendi." });
    }

    [HttpPost("ad-reward")]
    public async Task<IActionResult> ClaimAdReward()
    {
        try
        {
            await _economyManager.ClaimAdRewardAsync(GetUserId());
            return Ok(new { message = "3 Elmas kazandınız!" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("ad-reward/status")]
    public async Task<IActionResult> AdRewardStatus()
    {
        var canClaim = await _economyManager.CanClaimAdRewardAsync(GetUserId());
        return Ok(new { canClaim });
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteProfile()
    {
        var user = await _context.Users.FindAsync(GetUserId());
        if (user == null) return NotFound();

        // Anonymize user to preserve referential integrity (match history for other players)
        user.Username = "Deleted_" + Guid.NewGuid().ToString("N").Substring(0, 8);
        user.PasswordHash = Guid.NewGuid().ToString(); // Make it impossible to login
        user.Tokens = 0;
        user.Gold = 0;
        user.Diamonds = 0;
        
        await _context.SaveChangesAsync();
        return Ok(new { message = "Hesabınız başarıyla silindi." });
    }
}
