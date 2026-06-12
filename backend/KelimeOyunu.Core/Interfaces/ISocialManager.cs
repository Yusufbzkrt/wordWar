using KelimeOyunu.Core.DTOs;
using KelimeOyunu.Core.Entities;

namespace KelimeOyunu.Core.Interfaces;

/// <summary>
/// Arkadaşlık yönetimi ve mesajlaşma yetkilendirmesi.
/// Sadece aktif arkadaşlar DM atabilir.
/// </summary>
public interface ISocialManager
{
    Task<Friendship> SendFriendRequestAsync(Guid requesterId, Guid addresseeId);
    Task<Friendship> AcceptFriendRequestAsync(Guid friendshipId, Guid userId);
    Task<Friendship> RejectFriendRequestAsync(Guid friendshipId, Guid userId);
    Task RemoveFriendAsync(Guid friendshipId, Guid userId);
    Task<IEnumerable<FriendDto>> GetFriendsAsync(Guid userId);
    Task<IEnumerable<FriendDto>> GetPendingRequestsAsync(Guid userId);
    Task<bool> AreFriendsAsync(Guid userId1, Guid userId2);
    Task<Message> SendMessageAsync(Guid senderId, Guid receiverId, string content);
    Task<IEnumerable<MessageDto>> GetMessagesAsync(Guid userId, Guid friendId, int page = 1, int pageSize = 50);
}
