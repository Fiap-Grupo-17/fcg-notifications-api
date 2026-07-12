using MassTransit;
using FCG.Contracts.Events;

namespace FCG.NotificationsAPI.Consumers;

/// <summary>
/// Consome UserCreatedEvent publicado pelo UsersAPI.
/// Simula o envio de e-mail de boas-vindas logando no console.
/// </summary>
public class UserCreatedConsumer : IConsumer<UserCreatedEvent>
{
    private readonly ILogger<UserCreatedConsumer> _logger;

    public UserCreatedConsumer(ILogger<UserCreatedConsumer> logger)
        => _logger = logger;

    public Task Consume(ConsumeContext<UserCreatedEvent> context)
    {
        var evt = context.Message;

        _logger.LogInformation(
            "[NOTIFICAÇÃO] ✉ E-mail de boas-vindas ENVIADO" +
            " | Para: {Email}" +
            " | Nome: {Nome}" +
            " | UserId: {UserId}" +
            " | Cadastrado em: {DataCadastro:dd/MM/yyyy HH:mm}",
            evt.Email, evt.Nome, evt.UserId, evt.DataCadastro);

        // Ponto de extensão: substituir por chamada real a SMTP / SendGrid / SES
        // await _emailService.EnviarBoasVindasAsync(evt.Email, evt.Nome);

        return Task.CompletedTask;
    }
}
