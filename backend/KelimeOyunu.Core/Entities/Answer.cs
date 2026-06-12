namespace KelimeOyunu.Core.Entities;

public class Answer
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid QuestionId { get; set; }
    public string Text { get; set; } = string.Empty; // Cevap metni
    public bool IsPopular { get; set; } = false; // Popüler cevap mı? (Her soruda max 2)

    // Navigation Property
    public Question Question { get; set; } = null!;
}
