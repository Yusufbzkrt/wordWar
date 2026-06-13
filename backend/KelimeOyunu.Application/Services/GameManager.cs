using KelimeOyunu.Core.DTOs;
using KelimeOyunu.Core.Entities;
using KelimeOyunu.Core.Enums;
using KelimeOyunu.Core.Interfaces;
using KelimeOyunu.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KelimeOyunu.Application.Services;

/// <summary>
/// Oyun akışını yönetir: tur başlatma, cevap değerlendirme, Best-of-3, maç sonu.
/// SRP: Sadece oyun mantığından sorumlu. Ekonomi işlemleri EconomyManager'a delege edilir.
/// </summary>
public class GameManager : IGameManager
{
    private readonly AppDbContext _context;
    private readonly IValidationEngine _validationEngine;
    private readonly IEconomyManager _economyManager;
    private readonly IQuestManager _questManager;
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<Guid, System.Collections.Concurrent.ConcurrentDictionary<Guid, bool>> _changeRequests = new();

    public GameManager(
        AppDbContext context,
        IValidationEngine validationEngine,
        IEconomyManager economyManager,
        IQuestManager questManager)
    {
        _context = context;
        _validationEngine = validationEngine;
        _economyManager = economyManager;
        _questManager = questManager;
    }

    public async Task<GameSession> CreateSessionAsync(Guid player1Id, Guid player2Id)
    {
        var session = new GameSession
        {
            Player1Id = player1Id,
            Player2Id = player2Id,
            Status = GameStatus.InProgress
        };

        _context.GameSessions.Add(session);
        await _context.SaveChangesAsync();

        return session;
    }

    public async Task<GameRound> StartNewRoundAsync(Guid sessionId)
    {
        var session = await _context.GameSessions
            .Include(s => s.Rounds)
            .FirstOrDefaultAsync(s => s.Id == sessionId)
            ?? throw new InvalidOperationException("Oturum bulunamadı.");

        int roundNumber = session.Rounds.Count + 1;

        // Daha önce kullanılmamış bir soru seç
        var usedQuestionIds = session.Rounds.Select(r => r.QuestionId).ToList();
        var question = await _context.Questions
            .Where(q => !usedQuestionIds.Contains(q.Id))
            .OrderBy(_ => Guid.NewGuid()) // Rastgele soru
            .FirstOrDefaultAsync()
            ?? throw new InvalidOperationException("Yeterli soru bulunamadı.");

        var round = new GameRound
        {
            GameSessionId = sessionId,
            QuestionId = question.Id,
            RoundNumber = roundNumber,
            ActiveTurnPlayerId = session.Player1Id, // İlk sırayı Player1'e verelim
            Result = RoundResult.InProgress
        };

        _context.GameRounds.Add(round);
        session.Status = GameStatus.RoundInProgress;
        await _context.SaveChangesAsync();

        return round;
    }

    public async Task<AnswerResultDto> SubmitAnswerAsync(
        Guid sessionId, Guid roundId, Guid playerId, string answer)
    {
        var round = await _context.GameRounds
            .Include(r => r.Question)
                .ThenInclude(q => q.Answers)
            .Include(r => r.GameSession)
            .FirstOrDefaultAsync(r => r.Id == roundId && r.GameSessionId == sessionId)
            ?? throw new InvalidOperationException("Tur bulunamadı.");

        if (round.Result != RoundResult.InProgress)
            return new AnswerResultDto(false, false, answer, null, 0, 0);

        if (round.ActiveTurnPlayerId.HasValue && round.ActiveTurnPlayerId.Value != playerId)
        {
            return new AnswerResultDto(false, false, answer, null, 0, 0);
        }

        // Bu oyuncunun daha önce bulduğu cevapları Redis/in-memory'den al
        // Şimdilik basit bir kontrol - aynı session içinde
        var validAnswers = round.Question.Answers
            .Select(a => (a.Text, a.IsPopular))
            .ToList();

        // Daha önce bulunmuş cevapları listeye al
        var alreadyFound = new HashSet<string>(round.FoundAnswers, StringComparer.OrdinalIgnoreCase);

        var result = await _validationEngine.ValidateAnswerAsync(round.Question.Text, answer, validAnswers, alreadyFound);

        int goldEarned = 0;
        bool isPlayer1 = round.GameSession.Player1Id == playerId;

        if (result.IsMatch)
        {
            if (isPlayer1) round.Player1Score++;
            else round.Player2Score++;

            if (result.IsPopular)
            {
                await _economyManager.AddPopularAnswerBonusAsync(playerId);
                goldEarned = 10;
            }

            // Bulunanlar listesine ekle
            round.FoundAnswers.Add(result.MatchedAnswer ?? answer);

            // Yapay zeka tarafından öğrenilen yepyeni bir kelimeyse veritabanına kaydet
            if (result.IsAIValidated)
            {
                var newAnswer = new KelimeOyunu.Core.Entities.Answer
                {
                    Text = result.MatchedAnswer ?? answer.Trim().ToLowerInvariant(),
                    IsPopular = false,
                    QuestionId = round.QuestionId
                };
                round.Question.Answers.Add(newAnswer);
                await _context.SaveChangesAsync();
            }

            // Doğru bildiği için sıra karşı tarafa geçer
            round.ActiveTurnPlayerId = isPlayer1 ? round.GameSession.Player2Id : round.GameSession.Player1Id;
        }

        int myWordsInRound = isPlayer1 ? round.Player1Score : round.Player2Score;
        bool isFirstBlood = round.FoundAnswers.Count == 1 && result.IsMatch;

        await _questManager.TrackEventAsync(playerId, QuestEventType.WordSubmitted, new WordSubmissionData(
            Word: answer,
            IsValid: result.IsMatch,
            IsPopular: result.IsPopular,
            IsFirstBlood: isFirstBlood,
            WordsInThisRound: myWordsInRound,
            PopularAnswersInThisRound: result.IsPopular ? 1 : 0
        ));

        await _context.SaveChangesAsync();

        int currentScore = isPlayer1 ? round.Player1Score : round.Player2Score;

        return new AnswerResultDto(
            result.IsMatch,
            result.IsPopular,
            answer,
            result.MatchedAnswer,
            goldEarned,
            currentScore
        );
    }

    public async Task EndRoundAsync(Guid sessionId, Guid roundId)
    {
        var round = await _context.GameRounds
            .Include(r => r.GameSession)
            .FirstOrDefaultAsync(r => r.Id == roundId && r.GameSessionId == sessionId)
            ?? throw new InvalidOperationException("Tur bulunamadı.");

        // Eğer tur zaten bitmişse işlemi tekrar etme (double-trigger koruması)
        if (round.Result != RoundResult.InProgress) return;

        var session = round.GameSession;

        // Tur kazananını belirle (Turn-based kurallarına göre kaybeden 'ActiveTurnPlayerId' dir çünkü süresi dolmuştur)
        // İstisna: Oyundan kopma veya cevapların tükenmesi.
        // Biz burada basitçe TimeUp dendiğinde, sırası gelenin kaybettiğini sayacağız.
        var loserId = round.ActiveTurnPlayerId;
        var winnerId = loserId == session.Player1Id ? session.Player2Id : session.Player1Id;

        round.Result = winnerId == session.Player1Id ? RoundResult.Player1Won : RoundResult.Player2Won;
        round.RoundWinnerId = winnerId;
        
        if (winnerId == session.Player1Id) session.Player1RoundWins++;
        else session.Player2RoundWins++;

        round.EndedAt = DateTime.UtcNow;

        // Best-of-3: 2 tur kazanan maçı kazanır
        if (session.Player1RoundWins >= 2)
        {
            session.WinnerId = session.Player1Id;
            session.Status = GameStatus.Completed;
            session.CompletedAt = DateTime.UtcNow;
            await _economyManager.AddMatchWinRewardAsync(session.Player1Id);

            // İstatistik güncelle
            var winner = await _context.Users.FindAsync(session.Player1Id);
            var loser = await _context.Users.FindAsync(session.Player2Id);
            if (winner != null) winner.TotalWins++;
            if (loser != null) loser.TotalLosses++;

            var category = round.Question?.Category ?? "";
            int scoreDiff = Math.Abs(session.Player1RoundWins - session.Player2RoundWins);
            
            await _questManager.TrackEventAsync(session.Player1Id, QuestEventType.MatchCompleted, new MatchEndData(true, scoreDiff, category, false));
            await _questManager.TrackEventAsync(session.Player2Id, QuestEventType.MatchCompleted, new MatchEndData(false, scoreDiff, category, false));
        }
        else if (session.Player2RoundWins >= 2)
        {
            session.WinnerId = session.Player2Id;
            session.Status = GameStatus.Completed;
            session.CompletedAt = DateTime.UtcNow;
            await _economyManager.AddMatchWinRewardAsync(session.Player2Id);

            var winner = await _context.Users.FindAsync(session.Player2Id);
            var loser = await _context.Users.FindAsync(session.Player1Id);
            if (winner != null) winner.TotalWins++;
            if (loser != null) loser.TotalLosses++;

            var category = round.Question?.Category ?? "";
            int scoreDiff = Math.Abs(session.Player1RoundWins - session.Player2RoundWins);
            
            await _questManager.TrackEventAsync(session.Player2Id, QuestEventType.MatchCompleted, new MatchEndData(true, scoreDiff, category, false));
            await _questManager.TrackEventAsync(session.Player1Id, QuestEventType.MatchCompleted, new MatchEndData(false, scoreDiff, category, false));
        }
        else
        {
            session.Status = GameStatus.BetweenRounds;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<GameSession?> GetSessionAsync(Guid sessionId)
    {
        return await _context.GameSessions
            .Include(s => s.Player1)
            .Include(s => s.Player2)
            .Include(s => s.Rounds)
                .ThenInclude(r => r.Question)
            .FirstOrDefaultAsync(s => s.Id == sessionId);
    }

    public async Task<bool> UseJokerAsync(
        Guid sessionId, Guid roundId, Guid playerId, string jokerType)
    {
        if (!Enum.TryParse<JokerType>(jokerType, true, out var joker))
            return false;

        bool canAfford = await _economyManager.CanAffordJokerAsync(playerId, joker);
        if (!canAfford) return false;

        await _economyManager.DeductJokerCostAsync(playerId, joker);
        await _questManager.TrackEventAsync(playerId, QuestEventType.JokerUsed);
        
        return true;
    }

    public async Task SurrenderAsync(Guid sessionId, Guid playerId)
    {
        var session = await _context.GameSessions
            .Include(s => s.Rounds)
            .FirstOrDefaultAsync(s => s.Id == sessionId);

        if (session == null || session.Status == GameStatus.Completed) return;

        var winnerId = session.Player1Id == playerId ? session.Player2Id : session.Player1Id;

        session.Status = GameStatus.Completed;
        session.WinnerId = winnerId;
        session.CompletedAt = DateTime.UtcNow;

        var currentRound = session.Rounds.FirstOrDefault(r => r.Result == RoundResult.InProgress);
        if (currentRound != null)
        {
            currentRound.Result = winnerId == session.Player1Id ? RoundResult.Player1Won : RoundResult.Player2Won;
            currentRound.RoundWinnerId = winnerId;
            currentRound.EndedAt = DateTime.UtcNow;
        }

        var winner = await _context.Users.FindAsync(winnerId);
        var loser = await _context.Users.FindAsync(playerId);

        if (winner != null) winner.TotalWins++;
        if (loser != null) loser.TotalLosses++;

        await _economyManager.AddMatchWinRewardAsync(winnerId);
        
        var category = session.Rounds.FirstOrDefault()?.Question?.Category ?? "";
        int scoreDiff = Math.Abs(session.Player1RoundWins - session.Player2RoundWins);
        
        await _questManager.TrackEventAsync(winnerId, QuestEventType.MatchCompleted, new MatchEndData(true, scoreDiff, category, false));
        await _questManager.TrackEventAsync(playerId, QuestEventType.MatchCompleted, new MatchEndData(false, scoreDiff, category, false));

        await _context.SaveChangesAsync();
    }

    public async Task<(bool Changed, string NewQuestionText)> RequestChangeQuestionAsync(Guid sessionId, Guid roundId, Guid playerId)
    {
        var round = await _context.GameRounds
            .Include(r => r.GameSession)
            .FirstOrDefaultAsync(r => r.Id == roundId && r.GameSessionId == sessionId);

        if (round == null || round.Result != RoundResult.InProgress) 
            return (false, null);

        var roundRequests = _changeRequests.GetOrAdd(roundId, _ => new System.Collections.Concurrent.ConcurrentDictionary<Guid, bool>());
        roundRequests.TryAdd(playerId, true);

        if (roundRequests.Count >= 2)
        {
            var usedQuestionIds = await _context.GameRounds
                .Where(r => r.GameSessionId == sessionId)
                .Select(r => r.QuestionId)
                .ToListAsync();

            var newQuestion = await _context.Questions
                .Where(q => !usedQuestionIds.Contains(q.Id))
                .OrderBy(_ => Guid.NewGuid())
                .FirstOrDefaultAsync();

            if (newQuestion == null) return (false, null);

            round.QuestionId = newQuestion.Id;
            round.FoundAnswers.Clear();
            round.Player1Score = 0;
            round.Player2Score = 0;
            round.ActiveTurnPlayerId = round.GameSession.Player1Id;

            await _context.SaveChangesAsync();
            
            _changeRequests.TryRemove(roundId, out _);

            return (true, newQuestion.Text);
        }

        return (false, null);
    }
}
