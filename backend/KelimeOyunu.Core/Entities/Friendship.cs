using KelimeOyunu.Core.Enums;

namespace KelimeOyunu.Core.Entities;

public class Friendship
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid RequesterId { get; set; } // İsteği gönderen
    public Guid AddresseeId { get; set; } // İsteği alan
    public FriendshipStatus Status { get; set; } = FriendshipStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? AcceptedAt { get; set; }

    // Navigation Properties
    public User Requester { get; set; } = null!;
    public User Addressee { get; set; } = null!;
}
