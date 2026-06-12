namespace KelimeOyunu.Core.Interfaces;

/// <summary>
/// Levenshtein Distance ile hata toleranslı cevap doğrulama.
/// </summary>
public interface IValidationEngine
{
    /// <summary>
    /// Kullanıcının girdiği cevabı, doğru cevaplar listesiyle karşılaştırır.
    /// 1-2 harflik typo'ları tolere eder.
    /// </summary>
    (bool IsMatch, string? MatchedAnswer, bool IsPopular) ValidateAnswer(
        string userInput,
        IEnumerable<(string Text, bool IsPopular)> validAnswers,
        ISet<string> alreadyFoundAnswers);
}
