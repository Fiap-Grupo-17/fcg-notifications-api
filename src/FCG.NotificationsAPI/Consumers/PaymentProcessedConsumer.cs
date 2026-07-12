using MassTransit;
using FCG.Contracts.Events;

namespace FCG.NotificationsAPI.Consumers;

/// <summary>
/// Consome PaymentProcessedEvent publicado pelo PaymentsAPI.
/// Se Approved: simula e-mail de confirmação de compra.
/// Se Rejected: simula e-mail de pagamento recusado.
/// </summary>
public class PaymentProcessedConsumer : IConsumer<PaymentProcessedEvent>
{
    private readonly ILogger<PaymentProcessedConsumer> _logger;

    public PaymentProcessedConsumer(ILogger<PaymentProcessedConsumer> logger)
        => _logger = logger;

    public Task Consume(ConsumeContext<PaymentProcessedEvent> context)
    {
        var evt = context.Message;

        if (evt.Status == "Approved")
        {
            _logger.LogInformation(
                "[NOTIFICAÇÃO] ✉ E-mail de confirmação de compra ENVIADO" +
                " | UserId: {UserId}" +
                " | Jogo: {GameName}" +
                " | Preço: R$ {Price:F2}" +
                " | Transação: {TransactionId}" +
                " | OrderId: {OrderId}",
                evt.UserId, evt.GameName, evt.Price, evt.TransactionId, evt.OrderId);
        }
        else
        {
            _logger.LogWarning(
                "[NOTIFICAÇÃO] ✉ E-mail de pagamento recusado ENVIADO" +
                " | UserId: {UserId}" +
                " | Jogo: {GameName}" +
                " | Motivo: {Motivo}" +
                " | OrderId: {OrderId}",
                evt.UserId, evt.GameName, evt.MotivoRejeicao, evt.OrderId);
        }

        return Task.CompletedTask;
    }
}
