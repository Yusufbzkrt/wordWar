using KelimeOyunu.API.Hubs;
using KelimeOyunu.Core.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace KelimeOyunu.API.Services;

public class QuestNotificationService : IQuestNotificationService
{
    private readonly IHubContext<GameHub> _hubContext;

    public QuestNotificationService(IHubContext<GameHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyQuestCompletedAsync(Guid userId, string questTitle)
    {
        await _hubContext.Clients.User(userId.ToString()).SendAsync("QuestCompleted", questTitle);
    }
}
