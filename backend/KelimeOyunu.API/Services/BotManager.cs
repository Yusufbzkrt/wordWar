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
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<Guid, System.Collections.Concurrent.ConcurrentQueue<string>> _pendingMessages = new();

    // Sahte/Yanlış cevaplar havuzu
    private readonly List<string> _fakeAnswers = new List<string>
    {
        "elma", "armut", "masa", "kalem", "bilgisayar", "telefon", "araba",
        "ev", "kedi", "köpek", "kitap", "defter", "bardak", "çatal", "kaşık",
        "deniz", "güneş", "ay", "yıldız", "bulut", "ağaç", "çiçek", "kuş"
    };

    private readonly List<string> _botNames = new List<string>
    {
        // 150 Kız İsmi
        "Ayşe", "Fatma", "Zeynep", "Elif", "Merve", "Büşra", "Kübra", "Aslı", "Eda", "Gizem", 
        "Esra", "Selin", "Pelin", "İrem", "Ceren", "Tuğçe", "Burcu", "Ece", "Özge", "Cansu", 
        "Şeyma", "Melis", "Aleyna", "Ebru", "Beyza", "İlayda", "Buse", "Sena", "Deniz", "Derya", 
        "Ceyda", "Sinem", "Pınar", "Gamze", "Yasemin", "Damla", "Özlem", "Ceylan", "Şevval", "Berrin", 
        "Nisa", "Sude", "Yağmur", "Zehra", "Sümeyye", "Sibel", "Aylin", "Nur", "Başak", "Tuğba", 
        "Dilan", "Gözde", "Rabia", "Hande", "Handan", "Asuman", "Ayten", "Aysel", "Zeliha", "Ayşegül", 
        "Nermin", "Nevin", "Nilgün", "Serpil", "Seda", "Sevgi", "Seval", "Sevil", "Sevda", "Songül", 
        "Şengül", "Hülya", "Hatice", "Halime", "Emine", "Havva", "Melek", "Meryem", "Cemile", "Huriye", 
        "Saliha", "Gülsüm", "Ayfer", "Aynur", "İlknur", "Öznur", "Güllü", "Leyla", "Hayriye", "Kadriye", 
        "Leman", "Lütfiye", "Şükran", "Necla", "Nesrin", "Gülay", "Tülay", "Nilay", "Türkan", "Şermin", 
        "Zuhal", "Zerrin", "Yeşim", "Yonca", "Yıldız", "Ülkü", "Seçil", "Nazlı", "Nalan", "Müge", 
        "Mine", "Meltem", "Lale", "İpek", "İncilay", "Işıl", "Işık", "Güzin", "Gülçin", "Gülcan", 
        "Füsun", "Funda", "Filiz", "Feride", "Esin", "Esen", "Ender", "Emel", "Duygu", "Dilek", 
        "Didem", "Demet", "Defne", "Çiğdem", "Buket", "Bilge", "Binnur", "Birsen", "Bedia", "Bahar", 
        "Ayşenur", "Aycan", "Arzu", "Aysun", "Ayla", "Belgin", "Banu", "Berna", "Canan", "Şule", 
        // 50 Erkek İsmi
        "Ahmet", "Mehmet", "Ali", "Mustafa", "Can", "Cem", "Burak", "Kaan", "Emre", "Enes", 
        "Yasin", "Yusuf", "Furkan", "Onur", "Umut", "Uğur", "Hakan", "Serkan", "Gökhan", "Volkan", 
        "Murat", "Fatih", "Osman", "Ömer", "Bekir", "Hasan", "Hüseyin", "Efe", "Ege", "Arda", 
        "Mert", "Cenk", "Berk", "Barış", "Savaş", "Ufuk", "Şafak", "Aydın", "Doğan", "Şahin", 
        "Kartal", "Aslan", "Poyraz", "Rüzgar", "Çınar", "Özgür", "Engin", "Erdem", "Eren", "Batuhan"
    };

    public BotManager(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public string GetRandomBotName()
    {
        string name = _botNames[_random.Next(_botNames.Count)];
        // Rastgele 1 veya 4 uzunluğunda sayı ekleme
        bool isLengthOne = _random.Next(2) == 0;
        string numberSuffix = isLengthOne 
            ? _random.Next(0, 10).ToString() 
            : _random.Next(1000, 10000).ToString();
            
        return name + numberSuffix;
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

    public Task AddPendingMessageAsync(Guid sessionId, string message)
    {
        var queue = _pendingMessages.GetOrAdd(sessionId, _ => new System.Collections.Concurrent.ConcurrentQueue<string>());
        queue.Enqueue(message);
        return Task.CompletedTask;
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

                // Easter egg mesajı var mı kontrol et
                if (_pendingMessages.TryGetValue(sessionId, out var queue) && queue.TryDequeue(out var pendingMsg))
                {
                    await Task.Delay(_random.Next(2000, 4000));

                    using var tempScope = _scopeFactory.CreateScope();
                    var tempGm = tempScope.ServiceProvider.GetRequiredService<IGameManager>();
                    var tempResult = await tempGm.SubmitAnswerAsync(sessionId, roundId, botId, pendingMsg);

                    var tempOppConn = tempScope.ServiceProvider.GetRequiredService<ISessionManager>().GetConnectionId(session.Player1Id == botId ? session.Player2Id : session.Player1Id);
                    if (tempOppConn != null)
                    {
                        var hubContext = tempScope.ServiceProvider.GetRequiredService<IHubContext<GameHub>>();
                        await hubContext.Clients.Client(tempOppConn).SendAsync("OpponentAnswer", new { 
                            answer = pendingMsg, 
                            isCorrect = false, 
                            matchedAnswer = (string)null,
                            isPopular = false
                        });
                    }
                    continue; // Mesajı yolladıktan sonra başa dönüp asıl kelimeyi bulmaya/yanlış yapmaya çalışsın.
                }

                // Sıra botta, düşünme süresi (1-3 saniye)
                await Task.Delay(_random.Next(1000, 3000));

                var sessionManager = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ISessionManager>();
                var opponentId = session.Player1Id == botId ? session.Player2Id : session.Player1Id;
                var oppConn = sessionManager.GetConnectionId(opponentId);

                // Yazıyor... durumunu gönder
                if (oppConn != null)
                {
                    using var typingScope = _scopeFactory.CreateScope();
                    var hubContext = typingScope.ServiceProvider.GetRequiredService<IHubContext<GameHub>>();
                    await hubContext.Clients.Client(oppConn).SendAsync("OpponentIsTyping", true);
                }

                // Yazma süresi (kelime uzunluğuna göre simülasyon, ortalama 1-4 saniye)
                await Task.Delay(_random.Next(1500, 4000));

                // Yazmayı bitir
                if (oppConn != null)
                {
                    using var typingScope = _scopeFactory.CreateScope();
                    var hubContext = typingScope.ServiceProvider.GetRequiredService<IHubContext<GameHub>>();
                    await hubContext.Clients.Client(oppConn).SendAsync("OpponentIsTyping", false);
                }

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
                    if (oppConn != null)
                    {
                        var hubContext = executionScope.ServiceProvider.GetRequiredService<IHubContext<GameHub>>();
                        // Bot doğru cevapladığında sıra kesinlikle rakibe (user) geçer.
                        await hubContext.Clients.Client(oppConn).SendAsync("TurnChanged", opponentId);
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
