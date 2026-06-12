using KelimeOyunu.Core.DTOs;

namespace KelimeOyunu.Core.Interfaces;

/// <summary>
/// Global ve arkadaşlar arası liderlik tablosu hesaplaması.
/// Haftalık sıfırlanan kazanma sayısına göre sıralama.
/// </summary>
public interface ILeaderboardService
{
    Task<IEnumerable<LeaderboardEntryDto>> GetGlobalLeaderboardAsync(int top = 50);
    Task<IEnumerable<LeaderboardEntryDto>> GetFriendsLeaderboardAsync(Guid userId);
}
