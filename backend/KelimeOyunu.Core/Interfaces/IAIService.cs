namespace KelimeOyunu.Core.Interfaces;

public interface IAIService
{
    /// <summary>
    /// Yapay zeka kullanarak verilen cevabın soru için geçerli olup olmadığını kontrol eder.
    /// </summary>
    /// <param name="question">Örn: "Bir hayvan söyleyin"</param>
    /// <param name="answer">Örn: "Ornitorenk"</param>
    /// <returns>Geçerli ise true, değilse false</returns>
    Task<bool> ValidateAnswerWithAIAsync(string question, string answer);
}
