using System.Collections.Concurrent;
using KelimeOyunu.Core.Interfaces;

namespace KelimeOyunu.Application.Services;

/// <summary>
/// Oyuncu eşleştirme kuyruğu ve bağlantı yönetimi.
/// SRP: Sadece lobi/kuyruk yönetiminden sorumlu.
/// In-memory ConcurrentDictionary kullanır (Redis'e taşınabilir).
/// </summary>
public class SessionManager : ISessionManager
{
    // Eşleşme kuyruğu: PlayerId -> ConnectionId
    private static readonly ConcurrentDictionary<Guid, string> _matchmakingQueue = new();
    
    // Aktif bağlantılar: PlayerId -> ConnectionId
    private static readonly ConcurrentDictionary<Guid, string> _connections = new();
    
    // Rövanş istekleri: SessionId -> İsteği gönderen PlayerId set
    private static readonly ConcurrentDictionary<Guid, ConcurrentDictionary<Guid, bool>> _rematchRequests = new();

    public Task AddToQueueAsync(Guid playerId, string connectionId)
    {
        _matchmakingQueue[playerId] = connectionId;
        _connections[playerId] = connectionId;
        return Task.CompletedTask;
    }

    public Task<bool> RemoveFromQueueAsync(Guid playerId)
    {
        bool removed = _matchmakingQueue.TryRemove(playerId, out _);
        return Task.FromResult(removed);
    }

    public Task<Guid?> FindMatchAsync(Guid playerId)
    {
        // Kuyrukta bekleyen başka bir oyuncu bul
        foreach (var kvp in _matchmakingQueue)
        {
            if (kvp.Key != playerId)
            {
                // Her iki oyuncuyu da kuyruktan çıkar
                _matchmakingQueue.TryRemove(kvp.Key, out _);
                _matchmakingQueue.TryRemove(playerId, out _);
                return Task.FromResult<Guid?>(kvp.Key);
            }
        }

        return Task.FromResult<Guid?>(null);
    }

    public Task<bool> RequestRematchAsync(Guid sessionId, Guid playerId)
    {
        var requests = _rematchRequests.GetOrAdd(sessionId, _ => new ConcurrentDictionary<Guid, bool>());
        requests[playerId] = true;
        return Task.FromResult(true);
    }

    public Task<bool> AcceptRematchAsync(Guid sessionId, Guid playerId)
    {
        if (_rematchRequests.TryGetValue(sessionId, out var requests))
        {
            requests[playerId] = true;
            // İki oyuncu da kabul ettiyse
            return Task.FromResult(requests.Count >= 2);
        }
        return Task.FromResult(false);
    }

    public string? GetConnectionId(Guid playerId)
    {
        _connections.TryGetValue(playerId, out var connectionId);
        return connectionId;
    }

    public void SetConnectionId(Guid playerId, string connectionId)
    {
        _connections[playerId] = connectionId;
    }

    public void RemoveConnection(Guid playerId)
    {
        _connections.TryRemove(playerId, out _);
        _matchmakingQueue.TryRemove(playerId, out _);
    }
}
