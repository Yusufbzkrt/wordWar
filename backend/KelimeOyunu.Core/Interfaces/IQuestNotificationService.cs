namespace KelimeOyunu.Core.Interfaces;

public interface IQuestNotificationService
{
    Task NotifyQuestCompletedAsync(Guid userId, string questTitle);
}
