using KelimeOyunu.Core.Entities;
using KelimeOyunu.Core.Enums;

namespace KelimeOyunu.Core.Interfaces;

public interface IQuestManager
{
    Task<List<UserDailyQuest>> GetOrAssignDailyQuestsAsync(Guid userId);
    Task TrackEventAsync(Guid userId, QuestEventType type, object? data = null);
}

public record MatchEndData(bool IsWin, int ScoreDifference, string Category, bool UsedJokerInMatch);
public record WordSubmissionData(string Word, bool IsValid, bool IsPopular, bool IsFirstBlood, int WordsInThisRound, int PopularAnswersInThisRound);
