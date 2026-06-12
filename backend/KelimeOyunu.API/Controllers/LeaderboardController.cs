using System.Security.Claims;
using KelimeOyunu.Core.DTOs;
using KelimeOyunu.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KelimeOyunu.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LeaderboardController : ControllerBase
{
    private readonly ILeaderboardService _leaderboardService;
    public LeaderboardController(ILeaderboardService leaderboardService) => _leaderboardService = leaderboardService;
    private Guid GetUserId() => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpGet("global")]
    public async Task<ActionResult<IEnumerable<LeaderboardEntryDto>>> GetGlobal()
        => Ok(await _leaderboardService.GetGlobalLeaderboardAsync());

    [HttpGet("friends")]
    public async Task<ActionResult<IEnumerable<LeaderboardEntryDto>>> GetFriends()
        => Ok(await _leaderboardService.GetFriendsLeaderboardAsync(GetUserId()));
}
