using KelimeOyunu.Core.Enums;

namespace KelimeOyunu.Core.Entities;

public class UserDailyQuest
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public QuestType QuestType { get; set; }
    public QuestDifficulty Difficulty { get; set; }
    
    // UI için başlık ve açıklama
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    public int TargetProgress { get; set; }
    public int CurrentProgress { get; set; }
    
    public int RewardGold { get; set; }
    public int RewardDiamonds { get; set; }
    
    public bool IsCompleted { get; set; }
    public bool IsClaimed { get; set; }
    public DateTime AssignedDate { get; set; }

    public User User { get; set; } = null!;
}
