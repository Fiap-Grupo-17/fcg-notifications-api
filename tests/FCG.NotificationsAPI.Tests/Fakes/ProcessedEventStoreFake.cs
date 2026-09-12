using FCG.NotificationsAPI.Comum.Interfaces;

namespace FCG.NotificationsAPI.Tests.Fakes;

/// <summary>
/// Fake in-memory de <see cref="IProcessedEventStore"/> para testes unitários — reproduz
/// a semântica atômica do índice único (consumer, messageId) do MongoDB sem depender de
/// infraestrutura externa.
/// </summary>
public class ProcessedEventStoreFake : IProcessedEventStore
{
    private readonly HashSet<(Guid MessageId, string Consumer)> _registrados = new();

    public Task<bool> TryRegisterAsync(
        Guid messageId,
        string consumer,
        string? businessKey,
        string eventType,
        CancellationToken ct)
    {
        var primeiraVez = _registrados.Add((messageId, consumer));
        return Task.FromResult(primeiraVez);
    }
}
