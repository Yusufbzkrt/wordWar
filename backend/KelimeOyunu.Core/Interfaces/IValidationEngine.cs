namespace KelimeOyunu.Core.Interfaces;

/// <summary>
/// Levenshtein Distance ile hata toleranslı cevap doğrulama.
/// </summary>
public interface IValidationEngine
{
    /// <summary>
    /// Kullanıcının girdiği cevabı, doğru cevaplar listesiyle karşılaştırır.
    /// <param name="questionText">AI sorgusu için sorunun metni (Örn: Bir hayvan söyleyin)</param>
    /// <returns>IsMatch, MatchedAnswer, IsPopular, IsAIValidated</returns>
    Task<(bool IsMatch, string? MatchedAnswer, bool IsPopular, bool IsAIValidated)> ValidateAnswerAsync(
        string questionText,
        string userInput,
        IEnumerable<(string Text, bool IsPopular)> validAnswers,
        ISet<string> alreadyFoundAnswers);
}
