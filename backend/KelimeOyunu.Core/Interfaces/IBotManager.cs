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

    /// <summary>
    /// Bekleyen bir mesajı işlenmek üzere ekler.
    /// </summary>
    Task AddPendingMessageAsync(Guid sessionId, string message);

    /// <summary>
    /// Rastgele bir bot ismi döndürür.
    /// </summary>
    string GetRandomBotName();
}
