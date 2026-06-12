using KelimeOyunu.Core.Enums;

namespace KelimeOyunu.Core.Entities;

public class GameSession
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid Player1Id { get; set; }
    public Guid Player2Id { get; set; }
    public Guid? WinnerId { get; set; }
    public int Player1RoundWins { get; set; } = 0;
    public int Player2RoundWins { get; set; } = 0;
    public GameStatus Status { get; set; } = GameStatus.WaitingForPlayers;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    // Navigation Properties
    public User Player1 { get; set; } = null!;
    public User Player2 { get; set; } = null!;
    public User? Winner { get; set; }
    public ICollection<GameRound> Rounds { get; set; } = new List<GameRound>();
}
