using KelimeOyunu.Core.DTOs;
using KelimeOyunu.Core.Enums;

namespace KelimeOyunu.Core.Interfaces;

/// <summary>
/// Altın/Elmas bakiye yönetimi, joker satın alma, reklam cooldown kontrolü.
/// Tüm bakiye değişiklikleri Transaction olarak loglanır.
/// </summary>
public interface IEconomyManager
{
    Task<bool> CanAffordJokerAsync(Guid userId, JokerType jokerType);
    Task DeductJokerCostAsync(Guid userId, JokerType jokerType);
    Task AddPopularAnswerBonusAsync(Guid userId);
    Task AddMatchWinRewardAsync(Guid userId);
    Task<bool> CanClaimAdRewardAsync(Guid userId);
    Task ClaimAdRewardAsync(Guid userId);
    Task<UserProfileDto> GetBalanceAsync(Guid userId);
    Task<bool> ConsumeMatchTokenAsync(Guid userId);
    Task RefundMatchTokenAsync(Guid userId);
}
