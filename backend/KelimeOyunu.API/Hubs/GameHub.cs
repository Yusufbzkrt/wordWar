using System.Security.Claims;
using KelimeOyunu.Core.DTOs;
using KelimeOyunu.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using KelimeOyunu.Infrastructure.Data;

namespace KelimeOyunu.API.Hubs;

[Authorize]
public class GameHub : Hub
{
    private readonly IGameManager _gameManager;
    private readonly ISessionManager _sessionManager;
    private readonly IValidationEngine _validationEngine;
    private readonly AppDbContext _context;
    private readonly IBotManager _botManager;
    private readonly IServiceScopeFactory _scopeFactory;

    public GameHub(
        IGameManager gameManager, 
        ISessionManager sessionManager, 
        IValidationEngine validationEngine, 
        AppDbContext context, 
        IBotManager botManager, 
        IServiceScopeFactory scopeFactory)
    {
        _gameManager = gameManager;
        _sessionManager = sessionManager;
        _validationEngine = validationEngine;
        _context = context;
        _botManager = botManager;
        _scopeFactory = scopeFactory;
    }

    private Guid GetUserId() => Guid.Parse(Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        _sessionManager.SetConnectionId(userId, Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        _sessionManager.RemoveConnection(userId);
        await _sessionManager.RemoveFromQueueAsync(userId);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SearchMatch()
    {
        var userId = GetUserId();
        await _sessionManager.AddToQueueAsync(userId, Context.ConnectionId);

        var opponentId = await _sessionManager.FindMatchAsync(userId);
        if (opponentId.HasValue)
        {
            var session = await _gameManager.CreateSessionAsync(userId, opponentId.Value);
            var round = await _gameManager.StartNewRoundAsync(session.Id);
            var question = await _context.Questions.FindAsync(round.QuestionId);

            var gameState = new GameStateDto(
                session.Id, round.Id, question!.Text, round.RoundNumber,
                0, 0, 0, 0, 30,
                (await _context.Users.FindAsync(userId))!.Username,
                (await _context.Users.FindAsync(opponentId.Value))!.Username,
                round.ActiveTurnPlayerId
            );

            await Clients.Client(Context.ConnectionId).SendAsync("MatchFound", gameState);
            var opponentConnId = _sessionManager.GetConnectionId(opponentId.Value);
            if (opponentConnId != null)
                await Clients.Client(opponentConnId).SendAsync("MatchFound", gameState);
        }
        else
        {
            await Clients.Caller.SendAsync("SearchingMatch", "Rakip aranıyor...");

            // 15 saniye bekle (Cold Start Bot Sistemi)
            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(15000);
                    
                    // Oyuncu hala kuyruktaysa (başka biriyle eşleşmediyse veya iptal etmediyse)
                    bool stillInQueue = await _sessionManager.RemoveFromQueueAsync(userId);
                    if (stillInQueue)
                    {
                        using var scope = _scopeFactory.CreateScope();
                        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                        var gm = scope.ServiceProvider.GetRequiredService<IGameManager>();
                        var botMgr = scope.ServiceProvider.GetRequiredService<IBotManager>();
                        var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<GameHub>>();

                        var botId = await botMgr.GetOrCreateBotAsync();
                        var session = await gm.CreateSessionAsync(userId, botId);
                        var round = await gm.StartNewRoundAsync(session.Id);
                        var question = await db.Questions.FindAsync(round.QuestionId);

                        var p1 = await db.Users.FindAsync(userId);
                        var p2 = await db.Users.FindAsync(botId);

                        var gameState = new GameStateDto(
                            session.Id, round.Id, question!.Text, round.RoundNumber,
                            0, 0, 0, 0, 30,
                            p1!.Username,
                            p2!.Username,
                            round.ActiveTurnPlayerId
                        );

                        var playerConnId = _sessionManager.GetConnectionId(userId);
                        if (playerConnId != null)
                        {
                            await hubContext.Clients.Client(playerConnId).SendAsync("MatchFound", gameState);
                            
                            _ = Task.Run(async () => {
                                await Task.Delay(10000);
                                await botMgr.SimulateBotTurnAsync(session.Id, round.Id, botId);
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[BOT MATCHMAKING ERROR] {ex.Message}\n{ex.StackTrace}");
                    var connId = _sessionManager.GetConnectionId(userId);
                    if (connId != null)
                    {
                        using var scope = _scopeFactory.CreateScope();
                        var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<GameHub>>();
                        await hubContext.Clients.Client(connId).SendAsync("SearchCancelled");
                    }
                }
            });
        }
    }

    public async Task CancelSearch()
    {
        var userId = GetUserId();
        await _sessionManager.RemoveFromQueueAsync(userId);
        await Clients.Caller.SendAsync("SearchCancelled");
    }

    public async Task SubmitAnswer(SubmitAnswerDto dto)
    {
        var userId = GetUserId();
        var result = await _gameManager.SubmitAnswerAsync(dto.SessionId, dto.RoundId, userId, dto.Answer);
        await Clients.Caller.SendAsync("AnswerResult", result);

        // Rakibe skor ve cevap güncellemesi gönder
        var session = await _gameManager.GetSessionAsync(dto.SessionId);
        if (session != null)
        {
            var opponentId = session.Player1Id == userId ? session.Player2Id : session.Player1Id;
            var opponentConnId = _sessionManager.GetConnectionId(opponentId);
            if (opponentConnId != null)
            {
                await Clients.Client(opponentConnId).SendAsync("OpponentScoreUpdate", new { result.NewScore, PlayerId = userId });
                await Clients.Client(opponentConnId).SendAsync("OpponentAnswer", new { 
                    answer = dto.Answer, 
                    isCorrect = result.IsCorrect, 
                    matchedAnswer = result.MatchedAnswer,
                    isPopular = result.IsPopular
                });
            }

            if (result.IsCorrect)
            {
                var round = session.Rounds.FirstOrDefault(r => r.Id == dto.RoundId);
                if (round != null)
                {
                    var p1Conn = _sessionManager.GetConnectionId(session.Player1Id);
                    var p2Conn = _sessionManager.GetConnectionId(session.Player2Id);
                    if (p1Conn != null) await Clients.Client(p1Conn).SendAsync("TurnChanged", round.ActiveTurnPlayerId);
                    if (p2Conn != null) await Clients.Client(p2Conn).SendAsync("TurnChanged", round.ActiveTurnPlayerId);
                }
            }
        }
    }

    public async Task UseJoker(UseJokerDto dto)
    {
        var userId = GetUserId();
        var success = await _gameManager.UseJokerAsync(dto.SessionId, dto.RoundId, userId, dto.JokerType);

        if (success && dto.JokerType.Equals("Hint", StringComparison.OrdinalIgnoreCase))
        {
            var round = await _context.GameRounds.Include(r => r.Question).ThenInclude(q => q.Answers)
                .FirstOrDefaultAsync(r => r.Id == dto.RoundId);
            var popularAnswer = round?.Question.Answers.FirstOrDefault(a => a.IsPopular);
            if (popularAnswer != null)
                await Clients.Caller.SendAsync("HintResult", new { FirstLetter = popularAnswer.Text[0].ToString(), Success = true });
        }
        else
        {
            await Clients.Caller.SendAsync("JokerResult", new { JokerType = dto.JokerType, Success = success });
        }
    }

    public async Task TimeUp(Guid sessionId, Guid roundId)
    {
        await _gameManager.EndRoundAsync(sessionId, roundId);
        var session = await _gameManager.GetSessionAsync(sessionId);
        if (session == null) return;

        var round = session.Rounds.FirstOrDefault(r => r.Id == roundId);
        var roundResult = new { 
            Result = round?.Result.ToString(), 
            Player1Score = round?.Player1Score, 
            Player2Score = round?.Player2Score, 
            RoundWinnerId = round?.RoundWinnerId, 
            Player1Wins = session.Player1RoundWins, 
            Player2Wins = session.Player2RoundWins, 
            Status = session.Status.ToString(), 
            WinnerId = session.WinnerId 
        };

        var p1Conn = _sessionManager.GetConnectionId(session.Player1Id);
        var p2Conn = _sessionManager.GetConnectionId(session.Player2Id);
        if (p1Conn != null) await Clients.Client(p1Conn).SendAsync("RoundEnded", roundResult);
        if (p2Conn != null) await Clients.Client(p2Conn).SendAsync("RoundEnded", roundResult);

        if (session.Status == Core.Enums.GameStatus.BetweenRounds)
        {
            var newRound = await _gameManager.StartNewRoundAsync(sessionId);
            var question = await _context.Questions.FindAsync(newRound.QuestionId);
            var newState = new { RoundId = newRound.Id, QuestionText = question!.Text, RoundNumber = newRound.RoundNumber, ActiveTurnPlayerId = newRound.ActiveTurnPlayerId };
            if (p1Conn != null) await Clients.Client(p1Conn).SendAsync("NewRound", newState);
            if (p2Conn != null) await Clients.Client(p2Conn).SendAsync("NewRound", newState);

            var botId = await _botManager.GetOrCreateBotAsync();
            if (session.Player1Id == botId || session.Player2Id == botId)
            {
                _ = Task.Run(async () => {
                    await Task.Delay(10000);
                    await _botManager.SimulateBotTurnAsync(sessionId, newRound.Id, botId);
                });
            }
        }
    }

    public async Task RequestRematch(Guid sessionId)
    {
        var userId = GetUserId();
        await _sessionManager.RequestRematchAsync(sessionId, userId);
        var session = await _gameManager.GetSessionAsync(sessionId);
        if (session == null) return;
        var opponentId = session.Player1Id == userId ? session.Player2Id : session.Player1Id;
        var opponentConn = _sessionManager.GetConnectionId(opponentId);
        if (opponentConn != null)
            await Clients.Client(opponentConn).SendAsync("RematchRequested", userId);

        bool bothAccepted = await _sessionManager.AcceptRematchAsync(sessionId, userId);
        if (bothAccepted)
        {
            var newSession = await _gameManager.CreateSessionAsync(session.Player1Id, session.Player2Id);
            var round = await _gameManager.StartNewRoundAsync(newSession.Id);
            var question = await _context.Questions.FindAsync(round.QuestionId);
            var state = new GameStateDto(newSession.Id, round.Id, question!.Text, 1, 0, 0, 0, 0, 30, session.Player1.Username, session.Player2.Username, round.ActiveTurnPlayerId);
            var p1Conn = _sessionManager.GetConnectionId(session.Player1Id);
            var p2Conn = _sessionManager.GetConnectionId(session.Player2Id);
            if (p1Conn != null) await Clients.Client(p1Conn).SendAsync("RematchStarted", state);
            if (p2Conn != null) await Clients.Client(p2Conn).SendAsync("RematchStarted", state);
        }
    }
}
