using BankAccountsApi.Models;
using BankAccountsApi.Storage.Interfaces;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace BankAccountsApi.Infrastructure.Bus
{
    public class MessageBus
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConnectionFactory _connectionFactory;
        private readonly Dictionary<string, string> _eventRouting;
        private readonly int _maxRetries = 5;

        public MessageBus(
            IServiceScopeFactory scopeFactory,
            Dictionary<string, string> eventRouting,
            IConnectionFactory connectionFactory)
        {
            _scopeFactory = scopeFactory;
            _eventRouting = eventRouting;
            _connectionFactory = connectionFactory;
        }

        public async Task PublishPendingEventsAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var _outboxRepository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();

            var events = await _outboxRepository.GetPendingEventsAsync();

            using var connection = _connectionFactory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.ExchangeDeclare("account.events", ExchangeType.Topic, durable: true);

            foreach (var evt in events)
            {
                if (!_eventRouting.TryGetValue(evt.Type, out var routingKey))
                {
                    Console.WriteLine($"[OutboxPublisher] Неизвестный EventType {evt.Type}, пропускаем");
                    continue;
                }

                var eventMessage = CreateEventEnvelope(evt);
                var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(eventMessage));

                int attempt = 0;
                bool published = false;

                while (!published && attempt < _maxRetries)
                {
                    try
                    {
                        attempt++;
                        channel.BasicPublish(
                            exchange: "account.events",
                            routingKey: routingKey,
                            basicProperties: null,
                            body: body
                        );

                        await _outboxRepository.MarkAsPublishedAsync(evt.Id);
                        published = true;
                        Console.WriteLine($"[OutboxPublisher] Событие {evt.Id} успешно опубликовано на {routingKey}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[OutboxPublisher] Ошибка публикации {evt.Id} (попытка {attempt}): {ex.Message}");

                        if (attempt >= _maxRetries)
                        {
                            Console.WriteLine($"[OutboxPublisher] MAX_RETRIES достигнут для {evt.Id}, отправляем в dead-letter");
                            await _outboxRepository.MarkAsFailedAsync(evt.Id, ex.Message);
                            break;
                        }

                        var delayMs = (int)(Math.Pow(2, attempt) * 1000 + new Random().Next(0, 500));
                        await Task.Delay(delayMs);
                    }
                }
            }
        }

        private static object CreateEventEnvelope(OutboxMessage evt)
        {
            return new
            {
                eventId = evt.Id,
                occurredAt = DateTime.UtcNow.ToString("o"),
                meta = new
                {
                    version = "v1",
                    source = "account-service",
                    correlationId = Guid.NewGuid(),
                    causationId = Guid.NewGuid()
                },
                payload = JsonSerializer.Deserialize<JsonElement>(evt.Payload)
            };
        }
    }
}