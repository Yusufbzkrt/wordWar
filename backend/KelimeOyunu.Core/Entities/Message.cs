namespace KelimeOyunu.Core.Entities;

public class Message
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SenderId { get; set; }
    public Guid ReceiverId { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsRead { get; set; } = false;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public User Sender { get; set; } = null!;
    public User Receiver { get; set; } = null!;
}
