using MassTransit;
using Serilog;
using FCG.NotificationsAPI.Consumers;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, _, lc) =>
    lc.ReadFrom.Configuration(ctx.Configuration)
      .Enrich.FromLogContext()
      .WriteTo.Console(outputTemplate:
          "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"));

// MassTransit + RabbitMQ — apenas consumers neste serviço
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<UserCreatedConsumer>();
    x.AddConsumer<PaymentProcessedConsumer>();

    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(
            builder.Configuration["RabbitMQ:Host"] ?? "localhost",
            builder.Configuration["RabbitMQ:VirtualHost"] ?? "/",
            h =>
            {
                h.Username(builder.Configuration["RabbitMQ:Username"] ?? "guest");
                h.Password(builder.Configuration["RabbitMQ:Password"] ?? "guest");
            });

        cfg.ConfigureEndpoints(ctx);
    });
});

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Health check — obrigatório para Kubernetes liveness/readiness probes
app.MapGet("/health", () => Results.Ok(new
{
    status = "ok",
    service = "notifications-api",
    timestamp = DateTime.UtcNow
}));

Log.Information("FCG Notifications API iniciando — aguardando eventos do RabbitMQ...");
await app.RunAsync();
