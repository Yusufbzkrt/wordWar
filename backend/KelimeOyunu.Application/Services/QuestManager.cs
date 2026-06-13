using KelimeOyunu.Core.Entities;
using KelimeOyunu.Core.Enums;
using KelimeOyunu.Core.Interfaces;
using KelimeOyunu.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KelimeOyunu.Application.Services;

public class QuestManager : IQuestManager
{
    private readonly AppDbContext _context;
    private readonly IQuestNotificationService _notificationService;

    public QuestManager(AppDbContext context, IQuestNotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<List<UserDailyQuest>> GetOrAssignDailyQuestsAsync(Guid userId)
    {
        var today = DateTime.UtcNow.Date;
        
        var quests = await _context.DailyQuests
            .Where(q => q.UserId == userId && q.AssignedDate == today)
            .ToListAsync();

        if (quests.Any()) return quests;

        // Görev havuzundan rastgele 2 kolay, 2 orta, 1 zor seç
        var random = new Random();
        var easyQuests = QuestPool.Quests.Where(q => q.Difficulty == QuestDifficulty.Easy).OrderBy(x => random.Next()).Take(2).ToList();
        var mediumQuests = QuestPool.Quests.Where(q => q.Difficulty == QuestDifficulty.Medium).OrderBy(x => random.Next()).Take(2).ToList();
        var hardQuests = QuestPool.Quests.Where(q => q.Difficulty == QuestDifficulty.Hard).OrderBy(x => random.Next()).Take(1).ToList();

        var selectedQuests = easyQuests.Concat(mediumQuests).Concat(hardQuests).ToList();

        foreach (var def in selectedQuests)
        {
            var userQuest = new UserDailyQuest
            {
                UserId = userId,
                QuestType = def.QuestType,
                Difficulty = def.Difficulty,
                Title = def.Title,
                Description = def.Description,
                TargetProgress = def.TargetProgress,
                CurrentProgress = 0,
                RewardGold = def.RewardGold,
                RewardDiamonds = def.RewardDiamonds,
                IsCompleted = false,
                IsClaimed = false,
                AssignedDate = today
            };
            _context.DailyQuests.Add(userQuest);
            quests.Add(userQuest);
        }

        await _context.SaveChangesAsync();
        return quests;
    }

    public async Task TrackEventAsync(Guid userId, QuestEventType type, object? data = null)
    {
        var today = DateTime.UtcNow.Date;
        var activeQuests = await _context.DailyQuests
            .Where(q => q.UserId == userId && q.AssignedDate == today && !q.IsCompleted)
            .ToListAsync();

        if (!activeQuests.Any()) return;

        bool updated = false;

        foreach (var quest in activeQuests)
        {
            switch (type)
            {
                case QuestEventType.Login:
                    if (quest.QuestType == QuestType.Login)
                    {
                        quest.CurrentProgress++;
                        updated = true;
                    }
                    break;

                case QuestEventType.MatchCompleted:
                    if (data is MatchEndData m)
                    {
                        if (quest.QuestType == QuestType.PlayMatches) { quest.CurrentProgress++; updated = true; }
                        else if (quest.QuestType == QuestType.WinMatches && m.IsWin) { quest.CurrentProgress++; updated = true; }
                        else if (quest.QuestType == QuestType.PlayInCategory && (m.Category == "Günlük Yaşam" || m.Category == "Coğrafya")) { quest.CurrentProgress++; updated = true; }
                        else if (quest.QuestType == QuestType.WinInCategory && m.IsWin && (m.Category == "Geleneksel Türk Yemekleri" || m.Category == "Yapay Zeka ve Robotik")) { quest.CurrentProgress++; updated = true; }
                        else if (quest.QuestType == QuestType.WinBySmallMargin && m.IsWin && m.ScoreDifference <= 2) { quest.CurrentProgress++; updated = true; }
                        else if (quest.QuestType == QuestType.WinByLargeMargin && m.IsWin && m.ScoreDifference >= 5) { quest.CurrentProgress++; updated = true; }
                        else if (quest.QuestType == QuestType.WinWithoutJokers && m.IsWin && !m.UsedJokerInMatch) { quest.CurrentProgress++; updated = true; }
                        else if (quest.QuestType == QuestType.WinWithoutJokers && !m.IsWin) { quest.CurrentProgress = 0; updated = true; } // Reset streak if lost
                        else if (quest.QuestType == QuestType.WinStreak)
                        {
                            if (m.IsWin) quest.CurrentProgress++;
                            else quest.CurrentProgress = 0; // Reset streak
                            updated = true;
                        }
                    }
                    break;

                case QuestEventType.WordSubmitted:
                    if (data is WordSubmissionData w && w.IsValid)
                    {
                        if (quest.QuestType == QuestType.FindValidWords) { quest.CurrentProgress++; updated = true; }
                        else if (quest.QuestType == QuestType.TotalWordsInDay) { quest.CurrentProgress++; updated = true; }
                        else if (quest.QuestType == QuestType.FindPopularAnswers && w.IsPopular) { quest.CurrentProgress++; updated = true; }
                        else if (quest.QuestType == QuestType.FindLongWords && w.Word.Length >= 7) { quest.CurrentProgress++; updated = true; }
                        else if (quest.QuestType == QuestType.FirstBlood && w.IsFirstBlood) { quest.CurrentProgress++; updated = true; }
                        else if (quest.QuestType == QuestType.WordsInSingleRound && w.WordsInThisRound >= quest.TargetProgress) { quest.CurrentProgress = quest.TargetProgress; updated = true; }
                        else if (quest.QuestType == QuestType.PopularAnswersInSingleRound && w.IsPopular && w.PopularAnswersInThisRound >= quest.TargetProgress) { quest.CurrentProgress = quest.TargetProgress; updated = true; }
                    }
                    break;

                case QuestEventType.JokerUsed:
                    if (quest.QuestType == QuestType.UseJokers) { quest.CurrentProgress++; updated = true; }
                    break;
            }

            if (quest.CurrentProgress >= quest.TargetProgress)
            {
                if (!quest.IsCompleted)
                {
                    quest.CurrentProgress = quest.TargetProgress;
                    quest.IsCompleted = true;
                    updated = true;
                    await _notificationService.NotifyQuestCompletedAsync(userId, quest.Title);
                }
            }
        }

        if (updated)
        {
            await _context.SaveChangesAsync();
        }
    }
}
