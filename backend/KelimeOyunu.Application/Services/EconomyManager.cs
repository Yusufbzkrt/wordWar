using KelimeOyunu.Core.DTOs;
using KelimeOyunu.Core.Entities;
using KelimeOyunu.Core.Enums;
using KelimeOyunu.Core.Interfaces;
using KelimeOyunu.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KelimeOyunu.Application.Services;

public class EconomyManager : IEconomyManager
{
    private readonly AppDbContext _context;
    private static readonly Dictionary<JokerType, int> JokerCosts = new()
    {
        { JokerType.ExtraTime, 10 },
        { JokerType.Hint, 15 }
    };

    public EconomyManager(AppDbContext context) => _context = context;

    public async Task<bool> CanAffordJokerAsync(Guid userId, JokerType jokerType)
    {
        var user = await _context.Users.FindAsync(userId);
        return user != null && user.Gold >= JokerCosts.GetValueOrDefault(jokerType, int.MaxValue);
    }

    public async Task DeductJokerCostAsync(Guid userId, JokerType jokerType)
    {
        var user = await _context.Users.FindAsync(userId) ?? throw new InvalidOperationException("Kullanıcı bulunamadı.");
        int cost = JokerCosts[jokerType];
        user.Gold -= cost;
        _context.Transactions.Add(new Transaction { UserId = userId, Type = TransactionType.JokerPurchase, GoldAmount = -cost, Description = $"Joker: {jokerType}" });
        await _context.SaveChangesAsync();
    }

    public async Task AddPopularAnswerBonusAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId) ?? throw new InvalidOperationException("Kullanıcı bulunamadı.");
        user.Gold += 10;
        _context.Transactions.Add(new Transaction { UserId = userId, Type = TransactionType.PopularAnswerBonus, GoldAmount = 10, Description = "Popüler cevap bonusu" });
        await _context.SaveChangesAsync();
    }

    public async Task AddMatchWinRewardAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId) ?? throw new InvalidOperationException("Kullanıcı bulunamadı.");
        user.Gold += 10;
        user.Diamonds += 5;
        _context.Transactions.Add(new Transaction { UserId = userId, Type = TransactionType.MatchWinReward, GoldAmount = 10, DiamondAmount = 5, Description = "Maç kazanma ödülü" });
        await _context.SaveChangesAsync();
    }

    public async Task<bool> CanClaimAdRewardAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user?.LastAdRewardTime == null) return true;
        return (DateTime.UtcNow - user.LastAdRewardTime.Value).TotalHours >= 2;
    }

    public async Task ClaimAdRewardAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId) ?? throw new InvalidOperationException("Kullanıcı bulunamadı.");
        if (!await CanClaimAdRewardAsync(userId)) throw new InvalidOperationException("2 saat beklemelisiniz.");
        user.Diamonds += 3;
        user.LastAdRewardTime = DateTime.UtcNow;
        _context.Transactions.Add(new Transaction { UserId = userId, Type = TransactionType.AdReward, DiamondAmount = 3, Description = "Reklam ödülü" });
        await _context.SaveChangesAsync();
    }

    public async Task<UserProfileDto> GetBalanceAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId) ?? throw new InvalidOperationException("Kullanıcı bulunamadı.");
        return new UserProfileDto(user.Id, user.Username, user.Gold, user.Diamonds, user.TotalWins, user.TotalLosses, user.LastAdRewardTime);
    }
}
