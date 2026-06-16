using System.Security.Claims;
using KelimeOyunu.Core.DTOs;
using KelimeOyunu.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.SignalR;
using KelimeOyunu.API.Hubs;
using KelimeOyunu.Infrastructure.Data;

namespace KelimeOyunu.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FriendController : ControllerBase
{
    private readonly ISocialManager _socialManager;
    private readonly IHubContext<GameHub> _hubContext;
    private readonly ISessionManager _sessionManager;
    private readonly AppDbContext _dbContext;

    public FriendController(
        ISocialManager socialManager, 
        IHubContext<GameHub> hubContext,
        ISessionManager sessionManager,
        AppDbContext dbContext)
    {
        _socialManager = socialManager;
        _hubContext = hubContext;
        _sessionManager = sessionManager;
        _dbContext = dbContext;
    }
    
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
        try 
        { 
            var userId = GetUserId();
            await _socialManager.SendFriendRequestAsync(userId, addresseeId); 
            
            // Send real-time notification
            var connId = _sessionManager.GetConnectionId(addresseeId);
            if (connId != null)
            {
                var requester = await _dbContext.Users.FindAsync(userId);
                if (requester != null)
                {
                    await _hubContext.Clients.Client(connId).SendAsync("FriendRequestReceived", requester.Username);
                }
            }
            
            return Ok(); 
        }
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
