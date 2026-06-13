namespace KelimeOyunu.Core.Enums;

public enum QuestDifficulty
{
    Easy,
    Medium,
    Hard
}

public enum QuestType
{
    Login,
    PlayMatches,
    FindValidWords,
    FindPopularAnswers,
    FirstBlood,
    PlayInCategory,
    WinMatches,
    WordsInSingleRound,
    UseJokers,
    WinInCategory,
    FindLongWords,
    WinBySmallMargin,
    WinStreak,
    PopularAnswersInSingleRound,
    TotalWordsInDay,
    DefeatDifferentOpponents,
    WinByLargeMargin,
    WinWithoutJokers
}

public enum QuestEventType
{
    Login,
    MatchCompleted,
    WordSubmitted,
    JokerUsed
}
