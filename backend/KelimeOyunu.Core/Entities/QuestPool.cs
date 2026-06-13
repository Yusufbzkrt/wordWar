using KelimeOyunu.Core.Enums;

namespace KelimeOyunu.Core.Entities;

public class QuestDefinition
{
    public QuestType QuestType { get; set; }
    public QuestDifficulty Difficulty { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int TargetProgress { get; set; }
    public int RewardGold { get; set; }
    public int RewardDiamonds { get; set; }
}

public static class QuestPool
{
    public static readonly List<QuestDefinition> Quests = new()
    {
        // === KOLAY (25 Altın) ===
        new QuestDefinition { QuestType = QuestType.Login, Difficulty = QuestDifficulty.Easy, Title = "Hoş Geldin", Description = "Oyuna giriş yap.", TargetProgress = 1, RewardGold = 25 },
        new QuestDefinition { QuestType = QuestType.PlayMatches, Difficulty = QuestDifficulty.Easy, Title = "Isınma Turu", Description = "Kazanıp kaybetmene bakılmaksızın 3 maç tamamla.", TargetProgress = 3, RewardGold = 25 },
        new QuestDefinition { QuestType = QuestType.FindValidWords, Difficulty = QuestDifficulty.Easy, Title = "Kelime Avcısı", Description = "Rakiplerine karşı toplam 10 geçerli kelime yaz.", TargetProgress = 10, RewardGold = 25 },
        new QuestDefinition { QuestType = QuestType.FindPopularAnswers, Difficulty = QuestDifficulty.Easy, Title = "Halkın Sesi", Description = "Oynadığın maçlarda en az 1 adet 'Popüler Cevap' bul.", TargetProgress = 1, RewardGold = 25 },
        new QuestDefinition { QuestType = QuestType.FirstBlood, Difficulty = QuestDifficulty.Easy, Title = "İlk Kan", Description = "Herhangi bir maçta, rakibinden önce ilk kelimeyi yazan sen ol.", TargetProgress = 1, RewardGold = 25 },
        new QuestDefinition { QuestType = QuestType.PlayInCategory, Difficulty = QuestDifficulty.Easy, Title = "Kategori Gezgini", Description = "'Günlük Yaşam' veya 'Coğrafya' kategorisinde 1 maç oyna.", TargetProgress = 1, RewardGold = 25 },
        
        // === ORTA (50 Altın) ===
        new QuestDefinition { QuestType = QuestType.WinMatches, Difficulty = QuestDifficulty.Medium, Title = "Seri Katil", Description = "Rakiplerine karşı toplam 3 maç kazan.", TargetProgress = 3, RewardGold = 50 },
        new QuestDefinition { QuestType = QuestType.WordsInSingleRound, Difficulty = QuestDifficulty.Medium, Title = "Parmak Egzersizi", Description = "Tek bir turda en az 5 geçerli kelime yaz.", TargetProgress = 5, RewardGold = 50 },
        new QuestDefinition { QuestType = QuestType.FindPopularAnswers, Difficulty = QuestDifficulty.Medium, Title = "Popüler Kültür Uzmanı", Description = "Rakiplerine karşı toplam 5 adet 'Popüler Cevap' bul.", TargetProgress = 5, RewardGold = 50 },
        new QuestDefinition { QuestType = QuestType.UseJokers, Difficulty = QuestDifficulty.Medium, Title = "Taktiksel Zeka", Description = "Oyun içinde toplam 2 kez joker kullan.", TargetProgress = 2, RewardGold = 50 },
        new QuestDefinition { QuestType = QuestType.WinInCategory, Difficulty = QuestDifficulty.Medium, Title = "Alan Koruması", Description = "'Geleneksel Türk Yemekleri' veya 'Yapay Zeka ve Robotik' kategorilerinden herhangi birinde 2 maç kazan.", TargetProgress = 2, RewardGold = 50 },
        new QuestDefinition { QuestType = QuestType.FindLongWords, Difficulty = QuestDifficulty.Medium, Title = "Uzun Soluklu", Description = "İçinde en az 7 harf bulunan 3 farklı geçerli kelime bul.", TargetProgress = 3, RewardGold = 50 },
        new QuestDefinition { QuestType = QuestType.WinBySmallMargin, Difficulty = QuestDifficulty.Medium, Title = "Kıl Payı", Description = "Bir maçı rakibinden sadece 1 veya 2 kelime farkla kazan.", TargetProgress = 1, RewardGold = 50 },

        // === ZOR (100 Altın) ===
        new QuestDefinition { QuestType = QuestType.WinStreak, Difficulty = QuestDifficulty.Hard, Title = "Yenilmez Armada", Description = "Art arda 5 maç kazan (Galibiyet serisi yap).", TargetProgress = 5, RewardGold = 100 },
        new QuestDefinition { QuestType = QuestType.PopularAnswersInSingleRound, Difficulty = QuestDifficulty.Hard, Title = "Zihin Okuyucu", Description = "Tek bir turda, o sorunun 2 'Popüler Cevabının' İKİSİNİ BİRDEN bul.", TargetProgress = 2, RewardGold = 100 },
        new QuestDefinition { QuestType = QuestType.WordsInSingleRound, Difficulty = QuestDifficulty.Hard, Title = "Klavye Canavarı", Description = "Tek bir turda en az 10 geçerli kelime yaz.", TargetProgress = 10, RewardGold = 100 },
        new QuestDefinition { QuestType = QuestType.TotalWordsInDay, Difficulty = QuestDifficulty.Hard, Title = "Kelime Dağarcığı", Description = "Gün içinde toplam 100 geçerli kelime yaz.", TargetProgress = 100, RewardGold = 100 },
        new QuestDefinition { QuestType = QuestType.DefeatDifferentOpponents, Difficulty = QuestDifficulty.Hard, Title = "Büyük Kıyım", Description = "Eşleştirme sisteminde 5 farklı rakibi mağlup et.", TargetProgress = 5, RewardGold = 100 },
        new QuestDefinition { QuestType = QuestType.WinByLargeMargin, Difficulty = QuestDifficulty.Hard, Title = "Kusursuz Fırtına", Description = "Bir maçı, rakibine en az 5 kelime fark atarak (ezici üstünlükle) kazan.", TargetProgress = 1, RewardGold = 100 },
        new QuestDefinition { QuestType = QuestType.WinWithoutJokers, Difficulty = QuestDifficulty.Hard, Title = "Jokersiz Şampiyon", Description = "Hiç joker kullanmadan art arda 3 maç kazan.", TargetProgress = 3, RewardGold = 100 },
    };
}
