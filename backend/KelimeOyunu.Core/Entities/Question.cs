namespace KelimeOyunu.Core.Entities;

public class Question
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Text { get; set; } = string.Empty; // Soru metni (ör: "Bir meyve adı söyleyin")
    public string Category { get; set; } = string.Empty; // Kategori
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public ICollection<Answer> Answers { get; set; } = new List<Answer>();
}
