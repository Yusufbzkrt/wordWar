using KelimeOyunu.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace KelimeOyunu.Infrastructure.Data;

/// <summary>
/// 20+ soru ve cevaplarla veritabanını başlangıç verileriyle doldurur.
/// Her sorunun birden çok doğru cevabı ve 2 popüler cevabı vardır.
/// </summary>
public static class SeedData
{
    public static void Seed(ModelBuilder modelBuilder)
    {
        // Sorular ve Cevaplar
        var q1 = Guid.Parse("a1111111-1111-1111-1111-111111111111");
        var q2 = Guid.Parse("a2222222-2222-2222-2222-222222222222");
        var q3 = Guid.Parse("a3333333-3333-3333-3333-333333333333");
        var q4 = Guid.Parse("a4444444-4444-4444-4444-444444444444");
        var q5 = Guid.Parse("a5555555-5555-5555-5555-555555555555");
        var q6 = Guid.Parse("a6666666-6666-6666-6666-666666666666");
        var q7 = Guid.Parse("a7777777-7777-7777-7777-777777777777");
        var q8 = Guid.Parse("a8888888-8888-8888-8888-888888888888");
        var q9 = Guid.Parse("a9999999-9999-9999-9999-999999999999");
        var q10 = Guid.Parse("b1111111-1111-1111-1111-111111111111");
        var q11 = Guid.Parse("b2222222-2222-2222-2222-222222222222");
        var q12 = Guid.Parse("b3333333-3333-3333-3333-333333333333");
        var q13 = Guid.Parse("b4444444-4444-4444-4444-444444444444");
        var q14 = Guid.Parse("b5555555-5555-5555-5555-555555555555");
        var q15 = Guid.Parse("b6666666-6666-6666-6666-666666666666");
        var q16 = Guid.Parse("b7777777-7777-7777-7777-777777777777");
        var q17 = Guid.Parse("b8888888-8888-8888-8888-888888888888");
        var q18 = Guid.Parse("b9999999-9999-9999-9999-999999999999");
        var q19 = Guid.Parse("c1111111-1111-1111-1111-111111111111");
        var q20 = Guid.Parse("c2222222-2222-2222-2222-222222222222");

        var fixedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        modelBuilder.Entity<Question>().HasData(
            new Question { Id = q1, Text = "Bir meyve adı söyleyin", Category = "Yiyecek", CreatedAt = fixedDate },
            new Question { Id = q2, Text = "Bir renk adı söyleyin", Category = "Genel", CreatedAt = fixedDate },
            new Question { Id = q3, Text = "Bir hayvan adı söyleyin", Category = "Hayvanlar", CreatedAt = fixedDate },
            new Question { Id = q4, Text = "Bir ülke adı söyleyin", Category = "Coğrafya", CreatedAt = fixedDate },
            new Question { Id = q5, Text = "Bir spor dalı söyleyin", Category = "Spor", CreatedAt = fixedDate },
            new Question { Id = q6, Text = "Bir meslek adı söyleyin", Category = "Meslekler", CreatedAt = fixedDate },
            new Question { Id = q7, Text = "Bir müzik aleti söyleyin", Category = "Müzik", CreatedAt = fixedDate },
            new Question { Id = q8, Text = "Bir sebze adı söyleyin", Category = "Yiyecek", CreatedAt = fixedDate },
            new Question { Id = q9, Text = "Bir şehir adı söyleyin (Türkiye)", Category = "Coğrafya", CreatedAt = fixedDate },
            new Question { Id = q10, Text = "Bir araç markası söyleyin", Category = "Otomobil", CreatedAt = fixedDate },
            new Question { Id = q11, Text = "Bir içecek adı söyleyin", Category = "Yiyecek", CreatedAt = fixedDate },
            new Question { Id = q12, Text = "Bir çiçek adı söyleyin", Category = "Doğa", CreatedAt = fixedDate },
            new Question { Id = q13, Text = "Bir film türü söyleyin", Category = "Eğlence", CreatedAt = fixedDate },
            new Question { Id = q14, Text = "Bir okul dersi söyleyin", Category = "Eğitim", CreatedAt = fixedDate },
            new Question { Id = q15, Text = "Bir giysi türü söyleyin", Category = "Moda", CreatedAt = fixedDate },
            new Question { Id = q16, Text = "Mutfakta bulunan bir eşya söyleyin", Category = "Ev", CreatedAt = fixedDate },
            new Question { Id = q17, Text = "Bir gezegen adı söyleyin", Category = "Bilim", CreatedAt = fixedDate },
            new Question { Id = q18, Text = "Bir deniz canlısı söyleyin", Category = "Hayvanlar", CreatedAt = fixedDate },
            new Question { Id = q19, Text = "Bir Türk yemeği söyleyin", Category = "Yiyecek", CreatedAt = fixedDate },
            new Question { Id = q20, Text = "Bir yazılım dili söyleyin", Category = "Teknoloji", CreatedAt = fixedDate }
        );

        int id = 1;
        Guid AnswerId() => Guid.Parse($"d{id++:D7}-0000-0000-0000-000000000000");

        // Q1: Meyve
        modelBuilder.Entity<Answer>().HasData(
            new Answer { Id = AnswerId(), QuestionId = q1, Text = "Elma", IsPopular = true },
            new Answer { Id = AnswerId(), QuestionId = q1, Text = "Muz", IsPopular = true },
            new Answer { Id = AnswerId(), QuestionId = q1, Text = "Portakal", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q1, Text = "Çilek", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q1, Text = "Karpuz", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q1, Text = "Üzüm", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q1, Text = "Kiraz", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q1, Text = "Şeftali", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q1, Text = "Armut", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q1, Text = "Kavun", IsPopular = false }
        );

        // Q2: Renk
        modelBuilder.Entity<Answer>().HasData(
            new Answer { Id = AnswerId(), QuestionId = q2, Text = "Kırmızı", IsPopular = true },
            new Answer { Id = AnswerId(), QuestionId = q2, Text = "Mavi", IsPopular = true },
            new Answer { Id = AnswerId(), QuestionId = q2, Text = "Yeşil", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q2, Text = "Sarı", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q2, Text = "Siyah", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q2, Text = "Beyaz", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q2, Text = "Turuncu", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q2, Text = "Mor", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q2, Text = "Pembe", IsPopular = false }
        );

        // Q3: Hayvan
        modelBuilder.Entity<Answer>().HasData(
            new Answer { Id = AnswerId(), QuestionId = q3, Text = "Kedi", IsPopular = true },
            new Answer { Id = AnswerId(), QuestionId = q3, Text = "Köpek", IsPopular = true },
            new Answer { Id = AnswerId(), QuestionId = q3, Text = "At", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q3, Text = "Aslan", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q3, Text = "Kaplan", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q3, Text = "Fil", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q3, Text = "Tavuk", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q3, Text = "Kuş", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q3, Text = "Balık", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q3, Text = "Tavşan", IsPopular = false }
        );

        // Q4: Ülke
        modelBuilder.Entity<Answer>().HasData(
            new Answer { Id = AnswerId(), QuestionId = q4, Text = "Türkiye", IsPopular = true },
            new Answer { Id = AnswerId(), QuestionId = q4, Text = "Almanya", IsPopular = true },
            new Answer { Id = AnswerId(), QuestionId = q4, Text = "Fransa", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q4, Text = "İngiltere", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q4, Text = "İtalya", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q4, Text = "İspanya", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q4, Text = "ABD", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q4, Text = "Japonya", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q4, Text = "Brezilya", IsPopular = false }
        );

        // Q5: Spor
        modelBuilder.Entity<Answer>().HasData(
            new Answer { Id = AnswerId(), QuestionId = q5, Text = "Futbol", IsPopular = true },
            new Answer { Id = AnswerId(), QuestionId = q5, Text = "Basketbol", IsPopular = true },
            new Answer { Id = AnswerId(), QuestionId = q5, Text = "Voleybol", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q5, Text = "Tenis", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q5, Text = "Yüzme", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q5, Text = "Boks", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q5, Text = "Atletizm", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q5, Text = "Güreş", IsPopular = false }
        );

        // Q6: Meslek
        modelBuilder.Entity<Answer>().HasData(
            new Answer { Id = AnswerId(), QuestionId = q6, Text = "Doktor", IsPopular = true },
            new Answer { Id = AnswerId(), QuestionId = q6, Text = "Öğretmen", IsPopular = true },
            new Answer { Id = AnswerId(), QuestionId = q6, Text = "Mühendis", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q6, Text = "Avukat", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q6, Text = "Polis", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q6, Text = "Hemşire", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q6, Text = "Pilot", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q6, Text = "Aşçı", IsPopular = false }
        );

        // Q7: Müzik Aleti
        modelBuilder.Entity<Answer>().HasData(
            new Answer { Id = AnswerId(), QuestionId = q7, Text = "Gitar", IsPopular = true },
            new Answer { Id = AnswerId(), QuestionId = q7, Text = "Piyano", IsPopular = true },
            new Answer { Id = AnswerId(), QuestionId = q7, Text = "Keman", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q7, Text = "Davul", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q7, Text = "Flüt", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q7, Text = "Saz", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q7, Text = "Klarnet", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q7, Text = "Ney", IsPopular = false }
        );

        // Q8: Sebze
        modelBuilder.Entity<Answer>().HasData(
            new Answer { Id = AnswerId(), QuestionId = q8, Text = "Domates", IsPopular = true },
            new Answer { Id = AnswerId(), QuestionId = q8, Text = "Biber", IsPopular = true },
            new Answer { Id = AnswerId(), QuestionId = q8, Text = "Salatalık", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q8, Text = "Patates", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q8, Text = "Soğan", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q8, Text = "Havuç", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q8, Text = "Patlıcan", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q8, Text = "Kabak", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q8, Text = "Marul", IsPopular = false }
        );

        // Q9: Türkiye Şehirleri
        modelBuilder.Entity<Answer>().HasData(
            new Answer { Id = AnswerId(), QuestionId = q9, Text = "İstanbul", IsPopular = true },
            new Answer { Id = AnswerId(), QuestionId = q9, Text = "Ankara", IsPopular = true },
            new Answer { Id = AnswerId(), QuestionId = q9, Text = "İzmir", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q9, Text = "Antalya", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q9, Text = "Bursa", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q9, Text = "Trabzon", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q9, Text = "Adana", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q9, Text = "Konya", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q9, Text = "Gaziantep", IsPopular = false }
        );

        // Q10: Araba Markası
        modelBuilder.Entity<Answer>().HasData(
            new Answer { Id = AnswerId(), QuestionId = q10, Text = "BMW", IsPopular = true },
            new Answer { Id = AnswerId(), QuestionId = q10, Text = "Mercedes", IsPopular = true },
            new Answer { Id = AnswerId(), QuestionId = q10, Text = "Audi", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q10, Text = "Toyota", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q10, Text = "Volkswagen", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q10, Text = "Ford", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q10, Text = "Honda", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q10, Text = "Renault", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q10, Text = "Fiat", IsPopular = false }
        );

        // Q11: İçecek
        modelBuilder.Entity<Answer>().HasData(
            new Answer { Id = AnswerId(), QuestionId = q11, Text = "Çay", IsPopular = true },
            new Answer { Id = AnswerId(), QuestionId = q11, Text = "Kahve", IsPopular = true },
            new Answer { Id = AnswerId(), QuestionId = q11, Text = "Su", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q11, Text = "Ayran", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q11, Text = "Kola", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q11, Text = "Limonata", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q11, Text = "Süt", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q11, Text = "Meyve Suyu", IsPopular = false }
        );

        // Q12: Çiçek
        modelBuilder.Entity<Answer>().HasData(
            new Answer { Id = AnswerId(), QuestionId = q12, Text = "Gül", IsPopular = true },
            new Answer { Id = AnswerId(), QuestionId = q12, Text = "Papatya", IsPopular = true },
            new Answer { Id = AnswerId(), QuestionId = q12, Text = "Lale", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q12, Text = "Orkide", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q12, Text = "Karanfil", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q12, Text = "Menekşe", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q12, Text = "Ayçiçeği", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q12, Text = "Zambak", IsPopular = false }
        );

        // Q13: Film Türü
        modelBuilder.Entity<Answer>().HasData(
            new Answer { Id = AnswerId(), QuestionId = q13, Text = "Aksiyon", IsPopular = true },
            new Answer { Id = AnswerId(), QuestionId = q13, Text = "Komedi", IsPopular = true },
            new Answer { Id = AnswerId(), QuestionId = q13, Text = "Korku", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q13, Text = "Dram", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q13, Text = "Bilim Kurgu", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q13, Text = "Romantik", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q13, Text = "Animasyon", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q13, Text = "Belgesel", IsPopular = false }
        );

        // Q14: Okul Dersi
        modelBuilder.Entity<Answer>().HasData(
            new Answer { Id = AnswerId(), QuestionId = q14, Text = "Matematik", IsPopular = true },
            new Answer { Id = AnswerId(), QuestionId = q14, Text = "Türkçe", IsPopular = true },
            new Answer { Id = AnswerId(), QuestionId = q14, Text = "Fen Bilgisi", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q14, Text = "Tarih", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q14, Text = "Coğrafya", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q14, Text = "İngilizce", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q14, Text = "Müzik", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q14, Text = "Beden Eğitimi", IsPopular = false }
        );

        // Q15: Giysi
        modelBuilder.Entity<Answer>().HasData(
            new Answer { Id = AnswerId(), QuestionId = q15, Text = "Tişört", IsPopular = true },
            new Answer { Id = AnswerId(), QuestionId = q15, Text = "Pantolon", IsPopular = true },
            new Answer { Id = AnswerId(), QuestionId = q15, Text = "Gömlek", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q15, Text = "Etek", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q15, Text = "Ceket", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q15, Text = "Kazak", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q15, Text = "Şort", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q15, Text = "Mont", IsPopular = false }
        );

        // Q16: Mutfak Eşyası
        modelBuilder.Entity<Answer>().HasData(
            new Answer { Id = AnswerId(), QuestionId = q16, Text = "Tencere", IsPopular = true },
            new Answer { Id = AnswerId(), QuestionId = q16, Text = "Bıçak", IsPopular = true },
            new Answer { Id = AnswerId(), QuestionId = q16, Text = "Tabak", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q16, Text = "Bardak", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q16, Text = "Çatal", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q16, Text = "Kaşık", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q16, Text = "Tava", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q16, Text = "Süzgeç", IsPopular = false }
        );

        // Q17: Gezegen
        modelBuilder.Entity<Answer>().HasData(
            new Answer { Id = AnswerId(), QuestionId = q17, Text = "Mars", IsPopular = true },
            new Answer { Id = AnswerId(), QuestionId = q17, Text = "Jüpiter", IsPopular = true },
            new Answer { Id = AnswerId(), QuestionId = q17, Text = "Venüs", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q17, Text = "Satürn", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q17, Text = "Merkür", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q17, Text = "Neptün", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q17, Text = "Uranüs", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q17, Text = "Dünya", IsPopular = false }
        );

        // Q18: Deniz Canlısı
        modelBuilder.Entity<Answer>().HasData(
            new Answer { Id = AnswerId(), QuestionId = q18, Text = "Yunus", IsPopular = true },
            new Answer { Id = AnswerId(), QuestionId = q18, Text = "Köpekbalığı", IsPopular = true },
            new Answer { Id = AnswerId(), QuestionId = q18, Text = "Balina", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q18, Text = "Ahtapot", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q18, Text = "Denizanası", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q18, Text = "Yengeç", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q18, Text = "Karides", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q18, Text = "Deniz Kaplumbağası", IsPopular = false }
        );

        // Q19: Türk Yemeği
        modelBuilder.Entity<Answer>().HasData(
            new Answer { Id = AnswerId(), QuestionId = q19, Text = "Kebap", IsPopular = true },
            new Answer { Id = AnswerId(), QuestionId = q19, Text = "Lahmacun", IsPopular = true },
            new Answer { Id = AnswerId(), QuestionId = q19, Text = "Pide", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q19, Text = "Döner", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q19, Text = "Mantı", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q19, Text = "İskender", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q19, Text = "Köfte", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q19, Text = "Çorba", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q19, Text = "Dolma", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q19, Text = "Baklava", IsPopular = false }
        );

        // Q20: Yazılım Dili
        modelBuilder.Entity<Answer>().HasData(
            new Answer { Id = AnswerId(), QuestionId = q20, Text = "Python", IsPopular = true },
            new Answer { Id = AnswerId(), QuestionId = q20, Text = "JavaScript", IsPopular = true },
            new Answer { Id = AnswerId(), QuestionId = q20, Text = "Java", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q20, Text = "C#", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q20, Text = "C++", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q20, Text = "TypeScript", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q20, Text = "Go", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q20, Text = "Rust", IsPopular = false },
            new Answer { Id = AnswerId(), QuestionId = q20, Text = "PHP", IsPopular = false }
        );
    }
}
