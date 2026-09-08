using FCG.NotificationsAPI.Tests.Fakes;
using FluentAssertions;

namespace FCG.NotificationsAPI.Tests.Mensageria;

public class ProcessedEventStoreFakeTests
{
    [Fact]
    public async Task TryRegisterAsync_PrimeiraChamada_RetornaTrue()
    {
        var store = new ProcessedEventStoreFake();
        var messageId = Guid.NewGuid();

        var resultado = await store.TryRegisterAsync(
            messageId, "ConsumerTeste", businessKey: null, eventType: "EventoTeste", CancellationToken.None);

        resultado.Should().BeTrue();
    }

    [Fact]
    public async Task TryRegisterAsync_SegundaChamadaComMesmoMessageId_RetornaFalse()
    {
        var store = new ProcessedEventStoreFake();
        var messageId = Guid.NewGuid();

        await store.TryRegisterAsync(
            messageId, "ConsumerTeste", businessKey: null, eventType: "EventoTeste", CancellationToken.None);
        var resultado = await store.TryRegisterAsync(
            messageId, "ConsumerTeste", businessKey: null, eventType: "EventoTeste", CancellationToken.None);

        resultado.Should().BeFalse();
    }
}
