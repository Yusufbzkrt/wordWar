namespace KelimeOyunu.Core.Interfaces;

public interface IBotManager
{
    /// <summary>
    /// Sisteme kayıtlı olan AI botunun Id'sini getirir. Yoksa oluşturur.
    /// </summary>
    Task<Guid> GetOrCreateBotAsync();

    /// <summary>
    /// Botun belirli bir turdaki hamlelerini (cevap verme) simüle eder.
    /// </summary>
    Task SimulateBotTurnAsync(Guid sessionId, Guid roundId, Guid botId);
}
