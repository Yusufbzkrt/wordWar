using System.Text.Json;
using KelimeOyunu.Core.Entities;
using KelimeOyunu.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KelimeOyunu.API.SeedData;

public class QuestionSeedDto
{
    public string Kategori { get; set; } = string.Empty;
    public string Soru { get; set; } = string.Empty;
    public List<string> GecerliCevaplar { get; set; } = new();
    public List<string> PopulerCevaplar { get; set; } = new();
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
            var seedFilePath = Path.Combine(app.Environment.ContentRootPath, "SeedData", "sorular.json");
            
            if (!File.Exists(seedFilePath))
            {
                logger.LogWarning("Seed data dosyası bulunamadı: {Path}", seedFilePath);
                return;
            }

            var jsonData = await File.ReadAllTextAsync(seedFilePath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var questionsDto = JsonSerializer.Deserialize<List<QuestionSeedDto>>(jsonData, options);

            if (questionsDto == null || !questionsDto.Any())
                return;

            int addedCount = 0;

            foreach (var dto in questionsDto)
            {
                // Aynı soru veritabanında var mı kontrol et
                bool exists = await context.Questions.AnyAsync(q => q.Text == dto.Soru);
                if (!exists)
                {
                    var question = new Question
                    {
                        Id = Guid.NewGuid(),
                        Text = dto.Soru,
                        Category = dto.Kategori,
                        CreatedAt = DateTime.UtcNow
                    };

                    foreach (var cevap in dto.GecerliCevaplar)
                    {
                        var answer = new Answer
                        {
                            Id = Guid.NewGuid(),
                            QuestionId = question.Id,
                            Text = cevap,
                            IsPopular = dto.PopulerCevaplar.Contains(cevap)
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
