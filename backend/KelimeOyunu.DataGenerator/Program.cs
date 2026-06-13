using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace KelimeOyunu.DataGenerator
{
    class Program
    {
        private static readonly HttpClient _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromMinutes(5) // Zaman aşımını 5 dakikaya çıkardık
        };
        
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== AI Soru Üretme Motoru ===");

            var backendDir = @"c:\Users\yusuf\Desktop\kelime_oyunu\backend";
            string devSettingsPath = Path.Combine(backendDir, "KelimeOyunu.API", "appsettings.Development.json");
            string apiKey = "";

            if (File.Exists(devSettingsPath))
            {
                var json = File.ReadAllText(devSettingsPath);
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("AI", out var aiProp) && aiProp.TryGetProperty("GeminiApiKey", out var keyProp))
                {
                    apiKey = keyProp.GetString();
                }
            }

            if (string.IsNullOrEmpty(apiKey) || apiKey == "PLACEHOLDER_KEY")
            {
                Console.WriteLine("HATA: Geçerli bir Gemini API Key bulunamadı!");
                return;
            }

            int targetCount = 1000;
            if (args.Length > 0 && int.TryParse(args[0], out int parsedArg))
            {
                targetCount = parsedArg;
            }
            else
            {
                Console.WriteLine($"Parametre verilmediği için varsayılan hedef {targetCount} soru.");
            }

            string targetFile = Path.Combine(backendDir, "KelimeOyunu.Infrastructure", "Data", "questions.json");
            
            var existingQuestions = new List<GeneratedQuestion>();
            if (File.Exists(targetFile))
            {
                var json = await File.ReadAllTextAsync(targetFile);
                if (!string.IsNullOrWhiteSpace(json))
                {
                    try {
                        existingQuestions = JsonSerializer.Deserialize<List<GeneratedQuestion>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<GeneratedQuestion>();
                        Console.WriteLine($"Mevcut {existingQuestions.Count} soru dosyadan yüklendi. Üzerine eklenecek.");
                    } catch {
                        Console.WriteLine("Uyarı: Mevcut questions.json okunamadı, sıfırdan başlanacak.");
                    }
                }
            }

            Console.WriteLine($"Üretim başlıyor... Hedef: {targetCount} soru.");

            while (existingQuestions.Count < targetCount)
            {
                Console.WriteLine($"\n--- İlerleyiş: {existingQuestions.Count} / {targetCount} ---");
                try
                {
                    var newQuestions = await FetchQuestionsFromGemini(apiKey);
                    if (newQuestions != null && newQuestions.Count > 0)
                    {
                        existingQuestions.AddRange(newQuestions);
                        
                        // Her paket sonrası dosyayı kaydet
                        string outputJson = JsonSerializer.Serialize(existingQuestions, new JsonSerializerOptions { WriteIndented = true });
                        await File.WriteAllTextAsync(targetFile, outputJson);
                        
                        Console.WriteLine($"[BAŞARILI] {newQuestions.Count} soru eklendi. Toplam soru sayısı: {existingQuestions.Count}");
                    }
                    else
                    {
                        Console.WriteLine("[UYARI] Yapay zekadan boş veya geçersiz yanıt geldi.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[HATA] Paket alınırken hata oluştu: {ex.Message}");
                    if (ex.Message.Contains("429"))
                    {
                        Console.WriteLine("API Saatlik/Dakikalık limitine ulaşıldı. Kota sıfırlanması için 65 saniye bekleniyor...");
                        await Task.Delay(65000);
                    }
                }

                if (existingQuestions.Count < targetCount)
                {
                    Console.WriteLine("API limitlerine takılmamak için 10 saniye bekleniyor...");
                    await Task.Delay(10000);
                }
            }

            Console.WriteLine("\nÜRETİM TAMAMLANDI!");
            Console.WriteLine($"Toplam {existingQuestions.Count} soru {targetFile} dosyasına kaydedildi.");
        }

        private static async Task<List<GeneratedQuestion>> FetchQuestionsFromGemini(string apiKey)
        {
            string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}";

            string prompt = @"Sen bir kelime oyunu için yaratıcı ve çeşitli soru veri setleri üreten bir yapay zekasın.
Görev: Bana tamamen farklı ve rastgele alanlardan (bilim, tarih, günlük hayat, spor, yemek, sanat, doğa, teknoloji, kültür vb.) tam olarak 5 adet YENİ soru kategorisi üret.
Her kategori için aklına gelen *tüm* geçerli cevapları listele (kesinlikle bir sayı sınırı koyma, en az 10-20 tane cevap olsun).
Bu cevaplardan tam olarak en popüler (insanların ilk aklına gelen) 2 tanesini 'isPopular': true olarak işaretle, geri kalan tüm cevapları 'isPopular': false olarak bırak.

ÇIKTI FORMATI: 
Bana SADECE geçerli bir JSON dizisi (array) döndür. JSON haricinde hiçbir açıklama veya markdown (```json gibi) etiketi kullanma. 
Örnek format:
[
  {
    ""question"": ""Türkiye'deki Büyükşehirler"",
    ""answers"": [
      { ""text"": ""İstanbul"", ""isPopular"": true },
      { ""text"": ""Ankara"", ""isPopular"": true },
      { ""text"": ""İzmir"", ""isPopular"": false },
      { ""text"": ""Bursa"", ""isPopular"": false },
      { ""text"": ""Antalya"", ""isPopular"": false }
    ]
  }
]";

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.9,
                    topK = 40,
                    topP = 0.95,
                    maxOutputTokens = 8192
                }
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, jsonContent);
            
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(responseString);
            var root = document.RootElement;
            
            var textResult = root.GetProperty("candidates")[0]
                                .GetProperty("content")
                                .GetProperty("parts")[0]
                                .GetProperty("text").GetString();

            if (textResult != null)
            {
                // Remove Markdown code blocks if AI still adds them despite instructions
                textResult = textResult.Trim();
                if (textResult.StartsWith("```json"))
                {
                    textResult = textResult.Substring(7);
                }
                if (textResult.StartsWith("```"))
                {
                    textResult = textResult.Substring(3);
                }
                if (textResult.EndsWith("```"))
                {
                    textResult = textResult.Substring(0, textResult.Length - 3);
                }

                return JsonSerializer.Deserialize<List<GeneratedQuestion>>(textResult, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }

            return null;
        }
    }

    public class GeneratedQuestion
    {
        public string Question { get; set; }
        public List<GeneratedAnswer> Answers { get; set; }
    }

    public class GeneratedAnswer
    {
        public string Text { get; set; }
        public bool IsPopular { get; set; }
    }
}
