using KelimeOyunu.Core.Enums;

namespace KelimeOyunu.Core.Entities;

public class Transaction
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public TransactionType Type { get; set; }
    public int GoldAmount { get; set; } = 0;
    public int DiamondAmount { get; set; } = 0;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Property
    public User User { get; set; } = null!;
}
