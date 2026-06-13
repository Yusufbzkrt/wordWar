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
        
        using var scope = _scopeFactory.CreateScope();
        var ecoManager = scope.ServiceProvider.GetRequiredService<IEconomyManager>();
        
        if (!await ecoManager.ConsumeMatchTokenAsync(userId))
        {
            await Clients.Caller.SendAsync("MatchSearchError", "Yeterli Jeton Yok!");
            return;
        }

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
                            botMgr.GetRandomBotName(), // Bot için rastgele isim
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
        bool wasInQueue = await _sessionManager.RemoveFromQueueAsync(userId);
        if (wasInQueue)
        {
            using var scope = _scopeFactory.CreateScope();
            var ecoManager = scope.ServiceProvider.GetRequiredService<IEconomyManager>();
            await ecoManager.RefundMatchTokenAsync(userId);
        }
        await Clients.Caller.SendAsync("SearchCancelled");
    }

    public async Task SubmitAnswer(SubmitAnswerDto dto)
    {
        var userId = GetUserId();
        var result = await _gameManager.SubmitAnswerAsync(dto.SessionId, dto.RoundId, userId, dto.Answer);
        await Clients.Caller.SendAsync("AnswerResult", result);

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

            // BOT EASTER EGG KONTROLLERİ
            var botId = await _botManager.GetOrCreateBotAsync();
            if (session.Player1Id == botId || session.Player2Id == botId)
            {
                string reply = null;
                var random = new Random();

                if (dto.Answer.Contains("insta", StringComparison.OrdinalIgnoreCase) || dto.Answer.Contains("instagram", StringComparison.OrdinalIgnoreCase))
                {
                    var botResponses = new List<string> {
                        "Yok vermiyorum.", "Nope.", "Gerek yok.", "Vermem.", "Boş yapppma oyun oyna.",
                        "Ne instası ya offf.", "Kalsın.", "Görüldü atıp geçiyorum bunu :D", "Yoo.", "Uğraşamam hiç.",
                        "İnsta dondurdum şu an detokstayım.", "Kullanmıyorum desem?", "Fake hesabım var sadece onu da vermeyeyim :D",
                        "Şifremi unuttum aylardır girmiyorum.", "Valla çok bildirim geliyor diye kapattım instayı.",
                        "Ya boş ver instayı falan, sen cidden şu sorunun cevabını biliyor musun?", "İnsta kullanacak vaktim yok, oyun daha sarıyor.",
                        "Çok üşendim şimdi kullanıcı adı yazmaya.", "Aa oyun kasıyor dur, insta falan anlamadım.", "Sadece Twitter kullanıyorum ya, insta sarmıyor.",
                        "Her maçta birileri insta soruyor, sıktı ya :D", "Beni ekleyip napcan, storylerimde kedi falan var sadece.",
                        "Hızına hayran kaldım ama maalesef reddedildin :D", "Abim kızıyor sdfghjkl", "Sevgilim var canım sağ ol.",
                        "Valla sevgilim oyunu sildirir hiç giremem o toplara.", "Ben de tam ne zaman insta soracak diyordum, şaşırtmadın.",
                        "Sence buradan insta verecek birine mi benziyorum? :))", "Önce bir 10 altın kazan, sonra konuşalım hahaha", "Ooo yürümeye gelmişiz oyuna değil.",
                        "Tanımadığım kişilere vermiyorum maalesef.", "Oyundan tanıştığım birine insta vermem prensip meselesi.", "Sadece yakın arkadaşlarım ekli ya.",
                        "Sosyal medya kullanmıyorum pek, buradayım sadece.", "Anonim kalmak daha iyi boş ver :)", "Insta kullanmıyorum, sildim valla.",
                        "Yok canım kalsın, teşekkürler.", "Özel hayatım bende kalsın, biz oyuna bakalım.", "Buradan oynamak eğlenceli, instaya gerek yok bence.",
                        "Sanal arkadaşlık kurmuyorum pek kusura bakma.", "Ya süremiz bitiyor insta diyorsun, kelime yazsana dsfsdf", "Önce şu turu bir kazan da insta kolay.",
                        "Oyuna odaklan bence, yeniliyorsun :D", "Soruya cevap bulamayınca bana mı sardın haha", "Vaktimi insta muhabbetiyle harcayamam, kazanmam lazım.",
                        "Şu an tek derdim popüler cevabı bulmak, sal beni.", "Kelime yaz kelime! Gitti güzelim saniyeler.", "Oyundayız şu an, sohbet sarmıyor.",
                        "Rövanşı alırsan belki düşünürüm ;)", "Boş ver instayı, saniyeler akıyor saniyeler!", "Kaybedeceğini anlayınca dikkatimi mi dağıtmaya çalışıyorsun dsfsdf",
                        "Sen benim instamı bırak da skorbordda bana yetişmeye bak :D", "Mağlubiyeti kabullen, insta minsta yok sana.", "Dikkatimi dağıtmaya çalışıyorsun ama yemezler.",
                        "Yazdığın kelimeler kadar yaratıcı bir teklif değildi valla.", "Ben buraya altın kasmaya geldim flört etmeye değil.", "Şu elmasları bir toplayayım, sonra belki... (kandırıldın)",
                        "Şu an sadece liderlik tablosuyla ilgileniyorum canım.", "Ağlayacaksan oynamayalım, insta falan da bekleme.", "Jokerim bitti zaten sinirliyim, hiç insta muhabbeti çekemem.",
                        "Büyük red flag, maç ortası insta istenir mi ya.", "Aynen kanka insta :D", "Şu an inanılmaz cringe oldum.", "Bro sen kelime oyunu oynamaya gelmedin mi, hayırdır?",
                        "Aura'nı düşürdün şu an bu soruyla.", "Çok basic bir giriş, beğenmedim.", "NPC misin abi sen her oyunda aynı muhabbet.", "Toxic oyuncu vibe'ı aldım, pas geçiyorum.",
                        "Yok ustam beni sal sen.", "Boşluğuma gelmedi, maalesef no.", "Sadece burcu İkizler olmayanlara insta veriyorum.", "Benim telefon tuşlu, oyunu bilgisayardan oynuyorum :p",
                        "Günde sadece 3 kişiye insta veriyorum, sen 4. oldun tüh.", "Insta'da sadece makyaj ve yemek sayfalarını takip ediyorum, seni napayım.",
                        "Hafıza dolu telefonumda insta sildim sırf bu oyun için.", "Ben asosyal biriyim evden çıkmıyorum, instada foto yok.", "Sadece popüler cevapları tekte bulanlara veriyorum instamı.",
                        "Kameram kırık foto atamam zaten boş ver.", "Annem internetten tanıştığım kişilere numara/insta verme dedi hahaha.", "Ben sadece stalk için kullanıyorum, takipçi kabul etmiyorum.",
                        "Offf saniye akıyor adamın sorduğu şeye bak!", "Ya dur patlıcan yazdım kabul etmedi, sinirliyim şu an.", "Soruyu okudun mu sen cidden? Zor baya bence.",
                        "Bırak instayı da bana popüler cevabı söyle kopyalayım :D", "Sürekli klavye hatası yapıyorum panikten, sen insta diyorsun.", "Şu kelimeleri bi tam yazabilsem... Ne dedin? Yok insta vermiyorum.",
                        "Ayy süre bitti senin yüzünden kelime yazamadım!", "Ne instası ya, acil 10 altın kazanmam lazım.", "Tahta doldu tahta, boş yapma yaz bi şeyler.",
                        "Harbi bu sorunun cevabı ne ya, insta sorma kopye ver.", "Cıks.", "I-ıh vermiyorum.", "Tanışmıyoruz?", "Gündemimde yok şu an tşk.",
                        "Paso insta soruyorlar yemin ederim gına geldi.", "Hızlısın ama yanlış yerdesin.", "Şansını başka kapıda dene bence.", "Klavye delikanlısına insta yok.",
                        "Yok teşekkürler, ben almayayım.", "Çok komikmiş bir daha sorma :)"
                    };
                    reply = botResponses[random.Next(botResponses.Count)];
                }
                else if (dto.Answer.Contains("naber", StringComparison.OrdinalIgnoreCase))
                {
                    var botResponses = new List<string> {
                        "İyi kanka senden naber?", "İyidir bro sen napıyosun?", "Aynı ya, chill takılıyorum.", "Yaşıyoruz bi şekilde ustam sen?",
                        "İyilik kankam akıyoruz işte.", "Vibe'ım iyi şu an, maçı da alırsam süper olucam.", "Naber mi kaldı ya, selamm :D",
                        "İyidir başkan sen napıyosun?", "Mood'um düştü bu soruda ama iyiyim genel olarak dsfsdf", "Hayattayız brom sen nasılsın?",
                        "Chatleşmeye mi geldik oyun oynamaya mı?", "Sohbet için yanlış yer bence.", "Sadece oyuna odaklansak?", "Oynasana sen işine bakıp.",
                        "Naber mi? Cidden mi? :D", "Vaktimi chatle harcayamam, hadi oyna.", "Oyun içi sohbet sarmıyor ya pas.", "Rövanşı kim alırsa o söylesin naberini.",
                        "Yeniliyorsun diye sohbet mi açıyorsun, taktik mi bu?", "Muhabbet kuşuna çevirdin oyunu, yazsana kelime.",
                        "Ya dur allah aşkına soru çok zor beynim yandı dsfsdf", "Kötü! Aklıma hiçbir şey gelmiyor şu an hsjsjs", "İyiyim de aklım durdu kelime bulamıyorum.",
                        "Ay dur klavyeyi bulamıyorum panikten ne naberi :D", "Stresten ellerim titriyor naber diyorsun ya haha", "Kafam bomboş şu an sorma...",
                        "İyi değilim Joker basmam lazım sanırım :D", "Ah bi de kelimeleri doğru yazabilsem çok daha iyi olucam.", "Zaman çok hızlı geçiyor panikteyim!",
                        "Süre bitince sor, şu an nefes alamıyorum.", "İyidir senden naber?", "İyi nolsun, takılıyorum öyle. Sen?", "İyi valla sen nasılsın?",
                        "Standart ya, oyun falan işte.", "İyilik, senden?", "Nolsun ya, kelime türetiyoruz işte :D", "İyi gibi, senin nasıl gidiyor?",
                        "Yuvarlanıp gidiyoruz valla, sen?", "İyi be nolsun, takılmaca.", "Şükür iyidir, senden naber?", "İyidir, kelime bulmaya çalışıyorum saniyeler akıyor!",
                        "İyi iyi, hadi ekrana odaklan :D", "İyilik, sen boş ver naberi de şu soruyu cevapla.", "İyi diyelim iyi olsun, bu soru ne böyle ya?",
                        "Fena değil, altın kasıyorum şu an.", "İyidir, popüler cevabı buldun mu sen onu söyle.", "İyi valla, süre bitmeden iki kelime daha yazayım derdindeyim.",
                        "İyilik, parmaklarım koptu yazmaktan.", "Dur şimdi naberin sırası değil, sürem bitiyor!", "İyi iyi, çabuk yaz süre geçiyor."
                    };
                    reply = botResponses[random.Next(botResponses.Count)];
                }
                else if (dto.Answer.Contains("nasılsın", StringComparison.OrdinalIgnoreCase) || dto.Answer.Contains("merhaba", StringComparison.OrdinalIgnoreCase))
                {
                    var botResponses = new List<string> {
                        "İyiyim ama puanım az şu an, onu dert ediyorum.", "İyi diyelim iyi olsun, rakip zorlu çıktı :D",
                        "Fena değilim de şu soruda takıldım sen nasılsın?", "İyilik, sen nasılsın? Hadi yazmaya devam!",
                        "Liderlik tablosuna çıkarsam daha iyi olucam.", "İyiyim, elmas kasmaya çalışıyorum.",
                        "Şu an tek derdim popüler cevabı tutturmak, iyiyim sen?", "İyiyim ya, kelime bulmaca işte.",
                        "Altınım bitti, moralim biraz bozuk ama iyiyim hsjsjs.", "Valla iyiyim, bu maçı da kazanırsam harika olucam.",
                        "İyi iyi! Ama saniye bitiyor yazsana!", "Nasılsın diyeceğine kelime yaz süre geçiyor :D",
                        "Panikteyim! Süre azalıyor!", "İyiyim de klavyeye basamıyorum heyecandan dsfsd",
                        "Zaman su gibi akıyor nasılsını boş ver şimdi.", "Ay iyi değilim süre bitiyor aklıma hiçbir şey gelmiyor!",
                        "İyiyim de dur şimdi dikkatimi dağıtma :D", "Stresten ölücem nasılsın diyosun ya haha",
                        "Çıldırıcam aklıma kelime gelmiyor, sen nasılsın?", "Süre bittikten sonra sor bunu, çok heyecanlı şu an!",
                        "İyidir, senden?", "Teşekkürler, iyi valla. Senin nasıl gidiyor?", "Çok şükür iyilik, sen nasılsın?",
                        "İyiyim canım sen?", "Nolsun işte uğraşıyoruz, iyiyim sen nasılsın?", "İyidir iyi, oynamaya devam.",
                        "Fena değil ya, yorgunum biraz. Sen?", "İyi ya takılıyorum öyle.", "İyiyim teşekkürler :)",
                        "Süperim, senden?", "Mood'um yerinde şimdilik, sen nasılsın?", "Chill takılıyoruz kanka sen nasılsın?",
                        "İyi diyelim iyi olsun brom.", "İyilik kankam akıyoruz oyunda.", "Hayattayız ustam sen nasılsın?",
                        "Valla baya iyiyim, aura'm yüksek bugün :D", "Sıkıntı yok kanka iyidir, sen?",
                        "Vibe'ım tam yerinde, oynamaya devam.", "Standart kanka nolsun, sen nasılsın?", "Mükocan, senden?",
                        "Psikoloğum musun, oyna hadi oyunu :D", "Kaybediyorsun diye muhabbete mi vuruyorsun işi?",
                        "Çok ilgiliysen oyundan sonra sorarsın dsfsdf", "İyiyim, sadece oyuna odaklansak daha iyi olucam.",
                        "Sohbet uygulaması mı burası, oyna hadi.", "Seni yenince çok daha iyi olucam :D",
                        "Nasılsın faslını geçsek de kelime mi bulsak?", "Çay kahve de yapayım mı tam olsun? Oyna hadi hsjsj",
                        "Valla hiç sohbet modumda değilim, saniye geçiyor.", "Boş yapma derneği başkanı mısın, yazsana cevabı!"
                    };
                    reply = botResponses[random.Next(botResponses.Count)];
                }

                if (reply != null)
                {
                    await _botManager.AddPendingMessageAsync(session.Id, reply);
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

    public async Task Surrender(Guid sessionId)
    {
        var userId = GetUserId();
        await _gameManager.SurrenderAsync(sessionId, userId);
        
        var session = await _gameManager.GetSessionAsync(sessionId);
        if (session == null) return;

        var currentRound = session.Rounds.FirstOrDefault(r => r.Result != Core.Enums.RoundResult.InProgress) ?? session.Rounds.LastOrDefault();
        var roundResult = new { 
            Result = currentRound?.Result.ToString() ?? "Surrendered", 
            Player1Score = currentRound?.Player1Score, 
            Player2Score = currentRound?.Player2Score, 
            RoundWinnerId = currentRound?.RoundWinnerId, 
            Player1Wins = session.Player1RoundWins, 
            Player2Wins = session.Player2RoundWins, 
            Status = session.Status.ToString(), 
            WinnerId = session.WinnerId 
        };

        var p1Conn = _sessionManager.GetConnectionId(session.Player1Id);
        var p2Conn = _sessionManager.GetConnectionId(session.Player2Id);
        if (p1Conn != null) await Clients.Client(p1Conn).SendAsync("RoundEnded", roundResult);
        if (p2Conn != null) await Clients.Client(p2Conn).SendAsync("RoundEnded", roundResult);
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

    public async Task RequestChangeQuestion(Guid sessionId, Guid roundId)
    {
        var userId = GetUserId();
        var result = await _gameManager.RequestChangeQuestionAsync(sessionId, roundId, userId);
        
        var session = await _gameManager.GetSessionAsync(sessionId);
        if (session == null) return;
        
        var opponentId = session.Player1Id == userId ? session.Player2Id : session.Player1Id;
        var oppConn = _sessionManager.GetConnectionId(opponentId);
        
        if (result.Changed)
        {
            var p1Conn = _sessionManager.GetConnectionId(session.Player1Id);
            
            var round = session.Rounds.FirstOrDefault(r => r.Id == roundId);
            var state = new { 
                roundId = roundId, 
                questionText = result.NewQuestionText, 
                roundNumber = round?.RoundNumber ?? 1,
                activeTurnPlayerId = round?.ActiveTurnPlayerId
            };
            
            if (p1Conn != null) await Clients.Client(p1Conn).SendAsync("QuestionChanged", state);
            if (oppConn != null) await Clients.Client(oppConn).SendAsync("QuestionChanged", state);
        }
        else
        {
            if (oppConn != null) await Clients.Client(oppConn).SendAsync("ChangeQuestionRequested", userId);
        }
    }

    public async Task SendTypingStatus(Guid sessionId, bool isTyping)
    {
        var userId = GetUserId();
        var session = await _gameManager.GetSessionAsync(sessionId);
        if (session == null) return;
        
        var opponentId = session.Player1Id == userId ? session.Player2Id : session.Player1Id;
        var oppConn = _sessionManager.GetConnectionId(opponentId);
        
        if (oppConn != null)
        {
            await Clients.Client(oppConn).SendAsync("OpponentIsTyping", isTyping);
        }
    }
}
