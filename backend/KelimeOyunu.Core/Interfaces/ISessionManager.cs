namespace KelimeOyunu.Core.Interfaces;

/// <summary>
/// Oyuncu eşleştirme, oda yönetimi ve rövanş sistemi.
/// </summary>
public interface ISessionManager
{
    Task<Guid?> FindMatchAsync(Guid playerId);
    Task AddToQueueAsync(Guid playerId, string connectionId);
    Task<bool> RemoveFromQueueAsync(Guid playerId);
    Task<bool> RequestRematchAsync(Guid sessionId, Guid playerId);
    Task<bool> AcceptRematchAsync(Guid sessionId, Guid playerId);
    string? GetConnectionId(Guid playerId);
    void SetConnectionId(Guid playerId, string connectionId);
    void RemoveConnection(Guid playerId);
}
