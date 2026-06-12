using System.Security.Claims;
using KelimeOyunu.Core.DTOs;
using KelimeOyunu.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace KelimeOyunu.API.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly ISocialManager _socialManager;
    private readonly ISessionManager _sessionManager;

    public ChatHub(ISocialManager socialManager, ISessionManager sessionManager)
    {
        _socialManager = socialManager;
        _sessionManager = sessionManager;
    }

    private Guid GetUserId() => Guid.Parse(Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    public override async Task OnConnectedAsync()
    {
        _sessionManager.SetConnectionId(GetUserId(), Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public async Task SendMessage(SendMessageDto dto)
    {
        var senderId = GetUserId();
        var message = await _socialManager.SendMessageAsync(senderId, dto.ReceiverId, dto.Content);

        await Clients.Caller.SendAsync("MessageSent", new MessageDto(
            message.Id, senderId, "", dto.ReceiverId, "", dto.Content, false, message.SentAt));

        var receiverConn = _sessionManager.GetConnectionId(dto.ReceiverId);
        if (receiverConn != null)
        {
            await Clients.Client(receiverConn).SendAsync("MessageReceived", new MessageDto(
                message.Id, senderId, "", dto.ReceiverId, "", dto.Content, false, message.SentAt));
        }
    }
}
