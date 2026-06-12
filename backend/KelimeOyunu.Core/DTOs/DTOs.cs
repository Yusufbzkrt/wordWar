namespace KelimeOyunu.Core.DTOs;

public record GameStateDto(
    Guid SessionId,
    Guid RoundId,
    string QuestionText,
    int RoundNumber,
    int Player1Score,
    int Player2Score,
    int Player1RoundWins,
    int Player2RoundWins,
    int RemainingSeconds,
    string Player1Name,
    string Player2Name,
    Guid? ActiveTurnPlayerId
);

public record AnswerResultDto(
    bool IsCorrect,
    bool IsPopular,
    string SubmittedAnswer,
    string? MatchedAnswer,
    int GoldEarned,
    int NewScore
);

public record UserProfileDto(
    Guid Id,
    string Username,
    int Gold,
    int Diamonds,
    int TotalWins,
    int TotalLosses,
    DateTime? LastAdRewardTime
);

public record LeaderboardEntryDto(
    int Rank,
    Guid UserId,
    string Username,
    int TotalWins
);

public record LoginRequestDto(string Username, string Password);
public record RegisterRequestDto(string Username, string Password);
public record AuthResponseDto(string Token, UserProfileDto User);

public record SendMessageDto(Guid ReceiverId, string Content);
public record MessageDto(
    Guid Id,
    Guid SenderId,
    string SenderName,
    Guid ReceiverId,
    string ReceiverName,
    string Content,
    bool IsRead,
    DateTime SentAt
);

public record FriendDto(
    Guid FriendshipId,
    Guid FriendId,
    string FriendName,
    int FriendWins
);

public record UseJokerDto(Guid SessionId, Guid RoundId, string JokerType);
public record SubmitAnswerDto(Guid SessionId, Guid RoundId, string Answer);
public record RematchRequestDto(Guid SessionId);

public record StoreItemDto(
    string Id,
    string Name,
    string Description,
    int GoldCost,
    int DiamondCost,
    string Type // "gold_pack", "diamond_pack"
);
