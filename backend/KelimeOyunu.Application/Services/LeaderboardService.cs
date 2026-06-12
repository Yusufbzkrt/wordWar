using KelimeOyunu.Core.DTOs;
using KelimeOyunu.Core.Interfaces;
using KelimeOyunu.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using KelimeOyunu.Core.Enums;

namespace KelimeOyunu.Application.Services;

public class LeaderboardService : ILeaderboardService
{
    private readonly AppDbContext _context;
    public LeaderboardService(AppDbContext context) => _context = context;

    public async Task<IEnumerable<LeaderboardEntryDto>> GetGlobalLeaderboardAsync(int top = 50)
    {
        var users = await _context.Users
            .OrderByDescending(u => u.TotalWins)
            .Take(top)
            .Select(u => new { u.Id, u.Username, u.TotalWins })
            .ToListAsync();

        return users.Select((u, i) => new LeaderboardEntryDto(i + 1, u.Id, u.Username, u.TotalWins));
    }

    public async Task<IEnumerable<LeaderboardEntryDto>> GetFriendsLeaderboardAsync(Guid userId)
    {
        var friendIds = await _context.Friendships
            .Where(f => (f.RequesterId == userId || f.AddresseeId == userId) && f.Status == FriendshipStatus.Accepted)
            .Select(f => f.RequesterId == userId ? f.AddresseeId : f.RequesterId)
            .ToListAsync();

        friendIds.Add(userId);

        var users = await _context.Users
            .Where(u => friendIds.Contains(u.Id))
            .OrderByDescending(u => u.TotalWins)
            .Select(u => new { u.Id, u.Username, u.TotalWins })
            .ToListAsync();

        return users.Select((u, i) => new LeaderboardEntryDto(i + 1, u.Id, u.Username, u.TotalWins));
    }
}
