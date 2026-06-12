using System.Security.Claims;
using KelimeOyunu.Core.DTOs;
using KelimeOyunu.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KelimeOyunu.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FriendController : ControllerBase
{
    private readonly ISocialManager _socialManager;
    public FriendController(ISocialManager socialManager) => _socialManager = socialManager;
    private Guid GetUserId() => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpGet]
    public async Task<ActionResult<IEnumerable<FriendDto>>> GetFriends()
        => Ok(await _socialManager.GetFriendsAsync(GetUserId()));

    [HttpGet("pending")]
    public async Task<ActionResult<IEnumerable<FriendDto>>> GetPending()
        => Ok(await _socialManager.GetPendingRequestsAsync(GetUserId()));

    [HttpPost("request/{addresseeId}")]
    public async Task<IActionResult> SendRequest(Guid addresseeId)
    {
        try { await _socialManager.SendFriendRequestAsync(GetUserId(), addresseeId); return Ok(); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPost("accept/{friendshipId}")]
    public async Task<IActionResult> Accept(Guid friendshipId)
    {
        await _socialManager.AcceptFriendRequestAsync(friendshipId, GetUserId());
        return Ok();
    }

    [HttpPost("reject/{friendshipId}")]
    public async Task<IActionResult> Reject(Guid friendshipId)
    {
        await _socialManager.RejectFriendRequestAsync(friendshipId, GetUserId());
        return Ok();
    }

    [HttpDelete("{friendshipId}")]
    public async Task<IActionResult> Remove(Guid friendshipId)
    {
        await _socialManager.RemoveFriendAsync(friendshipId, GetUserId());
        return Ok();
    }
}
