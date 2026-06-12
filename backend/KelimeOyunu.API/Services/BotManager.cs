using KelimeOyunu.API.Hubs;
using KelimeOyunu.Core.Entities;
using KelimeOyunu.Core.Enums;
using KelimeOyunu.Core.Interfaces;
using KelimeOyunu.Infrastructure.Data;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace KelimeOyunu.API.Services;

public class BotManager : IBotManager
{
    private readonly IServiceScopeFactory _scopeFactory;
    private static Guid? _botId;
    private readonly Random _random = new Random();

    // Sahte/Yanlış cevaplar havuzu
    private readonly List<string> _fakeAnswers = new List<string>
    {
        "elma", "armut", "masa", "kalem", "bilgisayar", "telefon", "araba",
        "ev", "kedi", "köpek", "kitap", "defter", "bardak", "çatal", "kaşık",
        "deniz", "güneş", "ay", "yıldız", "bulut", "ağaç", "çiçek", "kuş"
    };

    public BotManager(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task<Guid> GetOrCreateBotAsync()
    {
        if (_botId.HasValue) return _botId.Value;

        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var bot = await context.Users.FirstOrDefaultAsync(u => u.Username == "KelimeBotu_AI");
        if (bot == null)
        {
            bot = new User
            {
                Id = Guid.NewGuid(),
                Username = "KelimeBotu_AI",
                PasswordHash = "BOT_NO_PASSWORD",
                Gold = 99999,
                Diamonds = 99999
            };
            context.Users.Add(bot);
            await context.SaveChangesAsync();
        }

        _botId = bot.Id;
        return _botId.Value;
    }

    public async Task SimulateBotTurnAsync(Guid sessionId, Guid roundId, Guid botId)
    {
        using var initScope = _scopeFactory.CreateScope();
        var db = initScope.ServiceProvider.GetRequiredService<AppDbContext>();

        var round = await db.GameRounds
            .Include(r => r.Question).ThenInclude(q => q.Answers)
            .FirstOrDefaultAsync(r => r.Id == roundId);

        if (round == null) return;

        var session = await db.GameSessions.FindAsync(sessionId);
        if (session == null) return;

        // Geçerli cevapların bir kopyasını al
        var validAnswers = round.Question.Answers.Select(a => a.Text).ToList();
        
        var startTime = DateTime.UtcNow;

        while (true)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var currentDb = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var currentRound = await currentDb.GameRounds.AsNoTracking().FirstOrDefaultAsync(r => r.Id == roundId);
                
                if (currentRound == null || currentRound.Result != RoundResult.InProgress) break;

                // Sıra botta değilse bekle
                if (currentRound.ActiveTurnPlayerId != botId)
                {
                    await Task.Delay(1000);
                    continue;
                }

                // Sıra botta, düşünme süresi
                int delayMin = 4000;
                int delayMax = 8000;
                await Task.Delay(_random.Next(delayMin, delayMax));

                using var executionScope = _scopeFactory.CreateScope();
                var executionDb = executionScope.ServiceProvider.GetRequiredService<AppDbContext>();
                var executionRound = await executionDb.GameRounds.AsNoTracking().FirstOrDefaultAsync(r => r.Id == roundId);
                
                if (executionRound == null || executionRound.Result != RoundResult.InProgress) break;

                string chosenAnswer = "";
                bool makeTypo = _random.NextDouble() < 0.20; 
                bool giveWrongAnswer = _random.NextDouble() < 0.15; 

                if (giveWrongAnswer || validAnswers.Count == 0)
                {
                    chosenAnswer = _fakeAnswers[_random.Next(_fakeAnswers.Count)];
                }
                else
                {
                    int index = _random.Next(validAnswers.Count);
                    chosenAnswer = validAnswers[index];
                    validAnswers.RemoveAt(index);

                    if (makeTypo && chosenAnswer.Length > 2)
                    {
                        char[] chars = chosenAnswer.ToCharArray();
                        int charIndex = _random.Next(1, chars.Length - 1);
                        chars[charIndex] = _random.NextDouble() > 0.5 ? 'a' : 'e';
                        chosenAnswer = new string(chars);
                    }
                }

                var gm = executionScope.ServiceProvider.GetRequiredService<IGameManager>();
                var result = await gm.SubmitAnswerAsync(sessionId, roundId, botId, chosenAnswer);

                var sessionManager = executionScope.ServiceProvider.GetRequiredService<ISessionManager>();
                var opponentId = session.Player1Id == botId ? session.Player2Id : session.Player1Id;
                var oppConn = sessionManager.GetConnectionId(opponentId);
                
                if (oppConn != null)
                {
                    var hubContext = executionScope.ServiceProvider.GetRequiredService<IHubContext<GameHub>>();
                    await hubContext.Clients.Client(oppConn).SendAsync("OpponentScoreUpdate", new { NewScore = result.NewScore, PlayerId = botId });
                    
                    await hubContext.Clients.Client(oppConn).SendAsync("OpponentAnswer", new { 
                        answer = chosenAnswer, 
                        isCorrect = result.IsCorrect, 
                        matchedAnswer = result.MatchedAnswer,
                        isPopular = result.IsPopular
                    });
                }

                if (result.IsCorrect)
                {
                    var updatedSession = await gm.GetSessionAsync(sessionId);
                    var updatedRound = updatedSession?.Rounds.FirstOrDefault(r => r.Id == roundId);
                    
                    if (updatedRound != null && oppConn != null)
                    {
                        var hubContext = executionScope.ServiceProvider.GetRequiredService<IHubContext<GameHub>>();
                        await hubContext.Clients.Client(oppConn).SendAsync("TurnChanged", updatedRound.ActiveTurnPlayerId);
                    }
                    // Döngü kırılmaz, bir sonraki tur için sıra beklemeye geçer
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BOT TURN ERROR] {ex.Message}\n{ex.StackTrace}");
                break;
            }
        }
    }
}
