using KelimeOyunu.Core.DTOs;
using KelimeOyunu.Core.Entities;
using KelimeOyunu.Core.Enums;
using KelimeOyunu.Core.Interfaces;
using KelimeOyunu.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KelimeOyunu.Application.Services;

public class SocialManager : ISocialManager
{
    private readonly AppDbContext _context;
    public SocialManager(AppDbContext context) => _context = context;

    public async Task<Friendship> SendFriendRequestAsync(Guid requesterId, Guid addresseeId)
    {
        var existing = await _context.Friendships.FirstOrDefaultAsync(f =>
            (f.RequesterId == requesterId && f.AddresseeId == addresseeId) ||
            (f.RequesterId == addresseeId && f.AddresseeId == requesterId));
        if (existing != null && existing.Status == FriendshipStatus.Accepted)
            throw new InvalidOperationException("Zaten arkadaşsınız.");
        if (existing != null && existing.Status == FriendshipStatus.Pending)
            throw new InvalidOperationException("Bekleyen istek mevcut.");

        var friendship = new Friendship { RequesterId = requesterId, AddresseeId = addresseeId };
        _context.Friendships.Add(friendship);
        await _context.SaveChangesAsync();
        return friendship;
    }

    public async Task<Friendship> AcceptFriendRequestAsync(Guid friendshipId, Guid userId)
    {
        var f = await _context.Friendships.FindAsync(friendshipId) ?? throw new InvalidOperationException("İstek bulunamadı.");
        if (f.AddresseeId != userId) throw new UnauthorizedAccessException("Bu isteği kabul edemezsiniz.");
        f.Status = FriendshipStatus.Accepted;
        f.AcceptedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return f;
    }

    public async Task<Friendship> RejectFriendRequestAsync(Guid friendshipId, Guid userId)
    {
        var f = await _context.Friendships.FindAsync(friendshipId) ?? throw new InvalidOperationException("İstek bulunamadı.");
        if (f.AddresseeId != userId) throw new UnauthorizedAccessException("Bu isteği reddedemezsiniz.");
        f.Status = FriendshipStatus.Rejected;
        await _context.SaveChangesAsync();
        return f;
    }

    public async Task RemoveFriendAsync(Guid friendshipId, Guid userId)
    {
        var f = await _context.Friendships.FindAsync(friendshipId) ?? throw new InvalidOperationException("Arkadaşlık bulunamadı.");
        if (f.RequesterId != userId && f.AddresseeId != userId) throw new UnauthorizedAccessException();
        f.Status = FriendshipStatus.Removed;
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<FriendDto>> GetFriendsAsync(Guid userId)
    {
        return await _context.Friendships
            .Where(f => (f.RequesterId == userId || f.AddresseeId == userId) && f.Status == FriendshipStatus.Accepted)
            .Select(f => new FriendDto(
                f.Id,
                f.RequesterId == userId ? f.AddresseeId : f.RequesterId,
                f.RequesterId == userId ? f.Addressee.Username : f.Requester.Username,
                f.RequesterId == userId ? f.Addressee.TotalWins : f.Requester.TotalWins))
            .ToListAsync();
    }

    public async Task<IEnumerable<FriendDto>> GetPendingRequestsAsync(Guid userId)
    {
        return await _context.Friendships
            .Where(f => f.AddresseeId == userId && f.Status == FriendshipStatus.Pending)
            .Select(f => new FriendDto(f.Id, f.RequesterId, f.Requester.Username, f.Requester.TotalWins))
            .ToListAsync();
    }

    public async Task<bool> AreFriendsAsync(Guid userId1, Guid userId2)
    {
        return await _context.Friendships.AnyAsync(f =>
            ((f.RequesterId == userId1 && f.AddresseeId == userId2) ||
             (f.RequesterId == userId2 && f.AddresseeId == userId1)) &&
            f.Status == FriendshipStatus.Accepted);
    }

    public async Task<Message> SendMessageAsync(Guid senderId, Guid receiverId, string content)
    {
        if (!await AreFriendsAsync(senderId, receiverId))
            throw new UnauthorizedAccessException("Sadece arkadaşlarınıza mesaj gönderebilirsiniz.");
        var msg = new Message { SenderId = senderId, ReceiverId = receiverId, Content = content };
        _context.Messages.Add(msg);
        await _context.SaveChangesAsync();
        return msg;
    }

    public async Task<IEnumerable<MessageDto>> GetMessagesAsync(Guid userId, Guid friendId, int page = 1, int pageSize = 50)
    {
        return await _context.Messages
            .Where(m => (m.SenderId == userId && m.ReceiverId == friendId) || (m.SenderId == friendId && m.ReceiverId == userId))
            .OrderByDescending(m => m.SentAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(m => new MessageDto(m.Id, m.SenderId, m.Sender.Username, m.ReceiverId, m.Receiver.Username, m.Content, m.IsRead, m.SentAt))
            .ToListAsync();
    }
}
