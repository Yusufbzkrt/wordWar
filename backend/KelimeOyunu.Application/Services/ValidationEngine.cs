using KelimeOyunu.Application.Helpers;
using KelimeOyunu.Core.Interfaces;

namespace KelimeOyunu.Application.Services;

/// <summary>
/// Levenshtein Distance kullanarak hata toleranslı cevap doğrulaması.
/// SRP: Sadece cevap doğrulama mantığından sorumlu.
/// </summary>
public class ValidationEngine : IValidationEngine
{
    private const int MaxAllowedDistance = 2;
    private readonly IAIService _aiService;

    public ValidationEngine(IAIService aiService)
    {
        _aiService = aiService;
    }

    public async Task<(bool IsMatch, string? MatchedAnswer, bool IsPopular, bool IsAIValidated)> ValidateAnswerAsync(
        string questionText,
        string userInput,
        IEnumerable<(string Text, bool IsPopular)> validAnswers,
        ISet<string> alreadyFoundAnswers)
    {
        if (string.IsNullOrWhiteSpace(userInput))
            return (false, null, false, false);

        string normalizedInput = NormalizeText(userInput);

        // Önce tam eşleşme kontrolü (performans optimizasyonu)
        foreach (var answer in validAnswers)
        {
            string normalizedAnswer = NormalizeText(answer.Text);

            // Zaten bulunmuş cevapları atla
            if (alreadyFoundAnswers.Contains(normalizedAnswer))
                continue;

            if (normalizedInput == normalizedAnswer)
                return (true, answer.Text, answer.IsPopular, false);
        }

        // Fuzzy matching — Levenshtein Distance ile
        string? bestMatch = null;
        bool bestIsPopular = false;
        int bestDistance = int.MaxValue;

        foreach (var answer in validAnswers)
        {
            string normalizedAnswer = NormalizeText(answer.Text);

            if (alreadyFoundAnswers.Contains(normalizedAnswer))
                continue;

            int distance = LevenshteinDistance.Calculate(normalizedInput, normalizedAnswer);

            // Dinamik eşik: kısa kelimeler için daha az tolerans
            int threshold = normalizedAnswer.Length <= 3 ? 1 : MaxAllowedDistance;

            if (distance <= threshold && distance < bestDistance)
            {
                bestDistance = distance;
                bestMatch = answer.Text;
                bestIsPopular = answer.IsPopular;
            }
        }

        if (bestMatch != null)
            return (true, bestMatch, bestIsPopular, false);

        // --- AI Validation ---
        // Sadece tek kelimelik (boşluk içermeyen) cevaplar AI'a gönderilir.
        if (!userInput.Trim().Contains(' '))
        {
            bool isValidatedByAI = await _aiService.ValidateAnswerWithAIAsync(questionText, userInput);
            if (isValidatedByAI)
            {
                return (true, userInput.Trim().ToLowerInvariant(), false, true);
            }
        }

        return (false, null, false, false);
    }

    private static string NormalizeText(string text)
    {
        return text.Trim().ToLowerInvariant()
            .Replace("ı", "i")
            .Replace("İ", "i");
    }
}
