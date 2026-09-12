# FCG Notifications API — Microsserviço de Notificações

Microsserviço responsável por **simular o envio de e-mails** da plataforma FIAP Cloud Games (FCG), consumindo eventos do RabbitMQ e registrando as notificações no console (log estruturado).

## Responsabilidades

| Evento consumido | Ação simulada |
|---|---|
| `UserCreatedEvent` (do UsersAPI) | Log de e-mail de boas-vindas ao novo usuário |
| `PaymentProcessedEvent` — `Approved` (do PaymentsAPI) | Log de e-mail de confirmação de compra |
| `PaymentProcessedEvent` — `Rejected` (do PaymentsAPI) | Log de e-mail de pagamento recusado |

> **Simulação:** em produção, os métodos `Consume` seriam trocados por chamadas reais a SMTP / SendGrid / Amazon SES sem alterar nenhum contrato.

## Stack

| Componente | Tecnologia |
|---|---|
| Framework | .NET 8 Minimal API |
| Mensageria | MassTransit + RabbitMQ |
| Logs | Serilog (Console) |

## NoSQL (MongoDB)

O serviço utiliza **MongoDB** (banco `fcg_notifications`) para registrar o histórico de
notificações enviadas e para controle de idempotência no consumo de eventos (evita
reenvio de e-mail duplicado em caso de redelivery pelo RabbitMQ/MassTransit).

## Endpoints

| Método | Rota | Descrição |
|---|---|---|
| GET | `/health` | Health check para Kubernetes |

## Variáveis de Ambiente

| Variável | Descrição | Padrão |
|---|---|---|
| `RabbitMQ__Host` | Host do RabbitMQ | `localhost` |
| `RabbitMQ__VirtualHost` | Virtual host | `/` |
| `RabbitMQ__Username` | Usuário RabbitMQ | `guest` |
| `RabbitMQ__Password` | Senha RabbitMQ | `guest` |
| `Mongo__ConnectionString` | String de conexão MongoDB | `mongodb://mongo:27017` |
| `Mongo__Database` | Banco de dados MongoDB | `fcg_notifications` |

## Executar com Docker Compose

```bash
docker compose up -d --build

# Validar o MongoDB
mongosh mongodb://localhost:27017/fcg_notifications --eval "db.getCollectionNames()"
```

## Executar localmente

```bash
# Pré-requisito: RabbitMQ rodando (porta 5672)
dotnet run --project src/FCG.NotificationsAPI
```

## Deploy Kubernetes

```bash
kubectl apply -f k8s/namespace.yaml
kubectl apply -f k8s/mongo.yaml
kubectl apply -f k8s/
kubectl get pods -n fcg
# Ver logs das notificações em tempo real:
kubectl logs -f deployment/notifications-api -n fcg
```

## Exemplo de log gerado

```
[10:30:01 INF] [NOTIFICAÇÃO] ✉ E-mail de boas-vindas ENVIADO
               | Para: joao@email.com | Nome: João Silva | UserId: abc-123

[10:31:05 INF] [NOTIFICAÇÃO] ✉ E-mail de confirmação de compra ENVIADO
               | UserId: abc-123 | Jogo: Cyberpunk 2077 | Preço: R$ 149,90
               | Transação: txn-xyz | OrderId: order-456
```

## Eventos consumidos (FCG.Contracts)

```csharp
// UserCreatedEvent
record UserCreatedEvent(Guid UserId, string Nome, string Email, DateTime DataCadastro);

// PaymentProcessedEvent
record PaymentProcessedEvent(Guid OrderId, Guid UserId, Guid GameId, string GameName,
    decimal Price, string Status, string? TransactionId, string? MotivoRejeicao, DateTime ProcessedAt);
```

## Grupo 17 — Pos-Tech FIAP
- Letícia Lopes Ribeiro Vasconcelos
- Marcelo Henrique Cornelis Rei
- Vinícius Calixto Real
