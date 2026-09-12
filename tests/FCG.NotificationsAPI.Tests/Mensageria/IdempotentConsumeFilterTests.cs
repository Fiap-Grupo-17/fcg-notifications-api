using FCG.Contracts.Events;
using FCG.NotificationsAPI.Mensageria;
using FCG.NotificationsAPI.Tests.Fakes;
using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;

namespace FCG.NotificationsAPI.Tests.Mensageria;

public class IdempotentConsumeFilterTests
{
    private static UserCreatedEvent CriarEvento() =>
        new(Guid.NewGuid(), "Usuário Teste", "teste@fcg.com", DateTime.UtcNow);

    private static ConsumeContext<UserCreatedEvent> CriarContexto(UserCreatedEvent evento, Guid messageId)
    {
        var mock = new Mock<ConsumeContext<UserCreatedEvent>>();
        mock.Setup(c => c.Message).Returns(evento);
        mock.Setup(c => c.MessageId).Returns(messageId);
        mock.Setup(c => c.CancellationToken).Returns(CancellationToken.None);
        return mock.Object;
    }

    [Fact]
    public async Task Send_PrimeiraMensagem_ChamaNext()
    {
        var filtro = new IdempotentConsumeFilter<UserCreatedEvent>(
            new ProcessedEventStoreFake(), Mock.Of<ILogger<IdempotentConsumeFilter<UserCreatedEvent>>>());

        var contexto = CriarContexto(CriarEvento(), Guid.NewGuid());

        var nextChamado = false;
        var pipeMock = new Mock<IPipe<ConsumeContext<UserCreatedEvent>>>();
        pipeMock.Setup(p => p.Send(It.IsAny<ConsumeContext<UserCreatedEvent>>()))
            .Callback(() => nextChamado = true)
            .Returns(Task.CompletedTask);

        await filtro.Send(contexto, pipeMock.Object);

        nextChamado.Should().BeTrue();
    }

    [Fact]
    public async Task Send_SegundaMensagemComMesmoMessageId_NaoChamaNext()
    {
        var store = new ProcessedEventStoreFake();
        var filtro = new IdempotentConsumeFilter<UserCreatedEvent>(
            store, Mock.Of<ILogger<IdempotentConsumeFilter<UserCreatedEvent>>>());

        var evento = CriarEvento();
        var messageId = Guid.NewGuid();

        var chamadasAoNext = 0;
        var pipeMock = new Mock<IPipe<ConsumeContext<UserCreatedEvent>>>();
        pipeMock.Setup(p => p.Send(It.IsAny<ConsumeContext<UserCreatedEvent>>()))
            .Callback(() => chamadasAoNext++)
            .Returns(Task.CompletedTask);

        await filtro.Send(CriarContexto(evento, messageId), pipeMock.Object);
        await filtro.Send(CriarContexto(evento, messageId), pipeMock.Object);

        chamadasAoNext.Should().Be(1);
    }
}
