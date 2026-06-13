using System.Text.Json;
using KelimeOyunu.Core.Entities;
using KelimeOyunu.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KelimeOyunu.API.SeedData;

public class GeneratedQuestion
{
    public string Question { get; set; } = string.Empty;
    public List<GeneratedAnswer> Answers { get; set; } = new();
}

public class GeneratedAnswer
{
    public string Text { get; set; } = string.Empty;
    public bool IsPopular { get; set; }
}

public static class DbInitializer
{
    public static async Task SeedQuestionsFromJsonAsync(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        try
        {
            var seedFilePath = Path.Combine(app.Environment.ContentRootPath, "..", "KelimeOyunu.Infrastructure", "Data", "questions.json");
            
            if (!File.Exists(seedFilePath))
            {
                logger.LogWarning("Seed data dosyası bulunamadı: {Path}", seedFilePath);
                return;
            }

            var jsonData = await File.ReadAllTextAsync(seedFilePath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var questionsDto = JsonSerializer.Deserialize<List<GeneratedQuestion>>(jsonData, options);

            if (questionsDto == null || !questionsDto.Any())
                return;

            int addedCount = 0;

            foreach (var dto in questionsDto)
            {
                // Aynı soru veritabanında var mı kontrol et
                bool exists = await context.Questions.AnyAsync(q => q.Text == dto.Question);
                if (!exists)
                {
                    var question = new Question
                    {
                        Id = Guid.NewGuid(),
                        Text = dto.Question,
                        Category = "Genel", // Kategori üretilmediyse genel diyelim
                        CreatedAt = DateTime.UtcNow
                    };

                    foreach (var cevap in dto.Answers)
                    {
                        var answer = new Answer
                        {
                            Id = Guid.NewGuid(),
                            QuestionId = question.Id,
                            Text = cevap.Text,
                            IsPopular = cevap.IsPopular
                        };
                        question.Answers.Add(answer);
                    }

                    context.Questions.Add(question);
                    addedCount++;
                }
            }

            if (addedCount > 0)
            {
                await context.SaveChangesAsync();
                logger.LogInformation("{Count} yeni soru JSON dosyasından veritabanına başarıyla eklendi.", addedCount);
            }
            else
            {
                logger.LogInformation("JSON dosyasındaki tüm sorular zaten veritabanında mevcut.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "JSON Seed verileri eklenirken bir hata oluştu.");
        }
    }
}
