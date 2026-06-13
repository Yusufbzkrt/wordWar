using KelimeOyunu.Core.DTOs;
using KelimeOyunu.Core.Entities;

namespace KelimeOyunu.Core.Interfaces;

/// <summary>
/// Oyun akışını ve tur kontrolünü yöneten servis.
/// Best-of-3 mantığı, tur başlatma/bitirme, maç sonu ödül hesaplamaları.
/// </summary>
public interface IGameManager
{
    Task<GameSession> CreateSessionAsync(Guid player1Id, Guid player2Id);
    Task<GameRound> StartNewRoundAsync(Guid sessionId);
    Task<AnswerResultDto> SubmitAnswerAsync(Guid sessionId, Guid roundId, Guid playerId, string answer);
    Task EndRoundAsync(Guid sessionId, Guid roundId);
    Task<GameSession?> GetSessionAsync(Guid sessionId);
    Task<bool> UseJokerAsync(Guid sessionId, Guid roundId, Guid playerId, string jokerType);
    Task SurrenderAsync(Guid sessionId, Guid playerId);
    Task<(bool Changed, string NewQuestionText)> RequestChangeQuestionAsync(Guid sessionId, Guid roundId, Guid playerId);
}
