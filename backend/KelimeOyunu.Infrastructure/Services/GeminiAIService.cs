using System.Net.Http;
using System.Text;
using System.Text.Json;
using KelimeOyunu.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace KelimeOyunu.Infrastructure.Services;

public class GeminiAIService : IAIService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly ILogger<GeminiAIService> _logger;

    public GeminiAIService(HttpClient httpClient, IConfiguration configuration, ILogger<GeminiAIService> logger)
    {
        _httpClient = httpClient;
        _apiKey = configuration["AI:GeminiApiKey"] ?? string.Empty;
        _logger = logger;
    }

    public async Task<bool> ValidateAnswerWithAIAsync(string question, string answer)
    {
        if (string.IsNullOrWhiteSpace(_apiKey) || _apiKey == "PLACEHOLDER_KEY")
        {
            _logger.LogWarning("Gemini API key is not configured or is a placeholder.");
            return false;
        }

        try
        {
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={_apiKey}";
            
            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = $"Kelime oyunu oynuyoruz. Soru: '{question}'. Oyuncunun verdiği cevap: '{answer}'. Bu cevap bu soru kategorisine teknik ve anlamsal olarak kesinlikle dahil edilebilir geçerli bir kelime mi? Lütfen sadece 'EVET' veya 'HAYIR' yaz. Nokta bile koyma." }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.0,
                    maxOutputTokens = 5
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync(url, content);
            
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Gemini API error: {response.StatusCode} - {error}");
                return false;
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(jsonResponse);
            
            var root = doc.RootElement;
            if (root.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
            {
                var candidate = candidates[0];
                if (candidate.TryGetProperty("content", out var resContent) && resContent.TryGetProperty("parts", out var parts) && parts.GetArrayLength() > 0)
                {
                    var part = parts[0];
                    if (part.TryGetProperty("text", out var textEl))
                    {
                        string resultText = textEl.GetString()?.Trim().ToUpperInvariant() ?? "";
                        if (resultText.Contains("EVET"))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while calling Gemini AI API");
            return false;
        }
    }
}
