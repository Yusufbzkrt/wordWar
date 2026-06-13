namespace KelimeOyunu.Core.Entities;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public int Gold { get; set; } = 100; // Başlangıç altını
    public int Diamonds { get; set; } = 10; // Başlangıç elması
    public int TotalWins { get; set; } = 0;
    public int TotalLosses { get; set; } = 0;
    public DateTime? LastAdRewardTime { get; set; }
    public int Tokens { get; set; } = 10; // Başlangıç jetonu
    public DateTime LastTokenUpdateTime { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public ICollection<GameSession> GameSessionsAsPlayer1 { get; set; } = new List<GameSession>();
    public ICollection<GameSession> GameSessionsAsPlayer2 { get; set; } = new List<GameSession>();
    public ICollection<Friendship> SentFriendRequests { get; set; } = new List<Friendship>();
    public ICollection<Friendship> ReceivedFriendRequests { get; set; } = new List<Friendship>();
    public ICollection<Message> SentMessages { get; set; } = new List<Message>();
    public ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public ICollection<UserDailyQuest> DailyQuests { get; set; } = new List<UserDailyQuest>();
}
