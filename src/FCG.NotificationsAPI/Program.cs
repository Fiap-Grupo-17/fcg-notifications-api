using MassTransit;
using Serilog;
using FCG.NotificationsAPI.Comum.Interfaces;
using FCG.NotificationsAPI.Consumers;
using FCG.NotificationsAPI.Mensageria;
using FCG.NotificationsAPI.Persistencia.Mongo;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, _, lc) =>
    lc.ReadFrom.Configuration(ctx.Configuration)
      .Enrich.FromLogContext()
      .WriteTo.Console(outputTemplate:
          "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"));

// ── Idempotência de consumers (MongoDB) ─────────────────────────
var mongoOptions = new MongoOptions();
builder.Configuration.GetSection(MongoOptions.SectionName).Bind(mongoOptions);
builder.Services.AddSingleton(mongoOptions);
builder.Services.AddSingleton<MongoContext>();
builder.Services.AddSingleton<IProcessedEventStore, MongoProcessedEventStore>();
builder.Services.AddHostedService<MongoIndexInitializer>();

// MassTransit + RabbitMQ — apenas consumers neste serviço
builder.Services.AddMassTransit(x =>
{
    // Prefixo de fila por serviço: evita colisão de nome de fila com outros
    // serviços que também consomem PaymentProcessedEvent (ex.: Catalog),
    // garantindo fan-out (uma fila por serviço) em vez de competing consumers.
    x.SetEndpointNameFormatter(new DefaultEndpointNameFormatter("Notifications", false));

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

        cfg.UseConsumeFilter(typeof(IdempotentConsumeFilter<>), ctx);

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
