using System.Security.Claims;
using KelimeOyunu.Core.DTOs;
using KelimeOyunu.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KelimeOyunu.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MessageController : ControllerBase
{
    private readonly ISocialManager _socialManager;
    public MessageController(ISocialManager socialManager) => _socialManager = socialManager;
    private Guid GetUserId() => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpGet("{friendId}")]
    public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessages(Guid friendId, [FromQuery] int page = 1)
        => Ok(await _socialManager.GetMessagesAsync(GetUserId(), friendId, page));
}
