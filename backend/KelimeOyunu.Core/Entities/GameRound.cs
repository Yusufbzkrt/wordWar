using KelimeOyunu.Core.Enums;

namespace KelimeOyunu.Core.Entities;

public class GameRound
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid GameSessionId { get; set; }
    public Guid QuestionId { get; set; }
    public int RoundNumber { get; set; } // 1, 2 veya 3
    public int Player1Score { get; set; } = 0; // Doğru cevap sayısı
    public int Player2Score { get; set; } = 0;
    public Guid? ActiveTurnPlayerId { get; set; } // O turda sıranın kimde olduğunu tutar
    public List<string> FoundAnswers { get; set; } = new List<string>(); // Turda bulunan cevaplar
    public Guid? RoundWinnerId { get; set; }
    public RoundResult Result { get; set; } = RoundResult.InProgress;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EndedAt { get; set; }

    // Navigation Properties
    public GameSession GameSession { get; set; } = null!;
    public Question Question { get; set; } = null!;
}
