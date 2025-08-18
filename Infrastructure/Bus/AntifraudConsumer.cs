using System.Diagnostics;
using System.Text;
using System.Text.Json;
using BankAccountsApi.Storage.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
// ReSharper disable All

namespace BankAccountsApi.Infrastructure.Bus;

public class AntifraudConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConnectionFactory _connectionFactory;
    private readonly ILogger<AntifraudConsumer> _logger;

    private IConnection? _connection;
    private IModel? _channel;

    private const string QueueName = "account.antifraud";

    public AntifraudConsumer(
        IServiceScopeFactory scopeFactory,
        IConnectionFactory connectionFactory,
        ILogger<AntifraudConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _connection = _connectionFactory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.QueueDeclare(queue: QueueName, durable: true, exclusive: false, autoDelete: false);
        _channel.BasicQos(0, 1, false);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += async (sender, ea) =>
        {
            var sw = Stopwatch.StartNew();
            var body = ea.Body.ToArray();
            var messageJson = Encoding.UTF8.GetString(body);

            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var envelope = JsonSerializer.Deserialize<Envelope>(messageJson, options);

                using var scope = _scopeFactory.CreateScope();
                var inboxRepository = scope.ServiceProvider.GetRequiredService<IInboxRepository>();
                var inboxDeadLetterRepository = scope.ServiceProvider.GetRequiredService<IInboxDeadLetterRepository>();

                if (envelope?.Meta?.Version != "v1")
                {
                    await inboxDeadLetterRepository.MarkAsDeadLetterAsync(
                        envelope?.EventId ?? Guid.NewGuid(),
                        nameof(AntifraudConsumer),
                        messageJson,
                        "Неподдерживаемая версия"
                    );

                    _logger.LogWarning($"Неподдерживаемая версия сообщения {envelope?.Meta?.Version}");
                    _channel.BasicAck(ea.DeliveryTag, false);
                    return;
                }

                if (await inboxRepository.ExistsAsync(envelope.EventId))
                {
                    _logger.LogInformation($"Событие уже обработано: {envelope.EventId}");
                    _channel.BasicAck(ea.DeliveryTag, false);
                    return;
                }

                _logger.LogInformation($"Обработка события {envelope.EventId}, тип {envelope.Type}");

                await HandleEventAsync(envelope, scope);

                await inboxRepository.MarkAsConsumedAsync(envelope.EventId);

                _channel.BasicAck(ea.DeliveryTag, false);

                sw.Stop();
                _logger.LogInformation("Событие обработано {@EventId} за {@LatencyMs}ms", envelope.EventId, sw.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "Ошибка при обработке сообщения: {@Message}. Время: {@LatencyMs}ms", messageJson, sw.ElapsedMilliseconds);
                _channel.BasicNack(ea.DeliveryTag, false, false);
            }
        };

        _channel.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);

        return Task.CompletedTask;
    }
    public Task HandleEventAsync(Envelope envelope)
    {
        using var scope = _scopeFactory.CreateScope();
        return HandleEventAsync(envelope, scope);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _channel?.Close();
        _connection?.Close();
        return base.StopAsync(cancellationToken);
    }

    public async Task HandleEventAsync(Envelope envelope, IServiceScope scope)
    {
        var clientsRepository = scope.ServiceProvider.GetRequiredService<IClientsRepository>();
        var accountsRepository = scope.ServiceProvider.GetRequiredService<IAccountsRepository>();
        var inboxDeadLetterRepository = scope.ServiceProvider.GetRequiredService<IInboxDeadLetterRepository>();

        switch (envelope.Type)
        {
            case "ClientBlocked":
                await FreezeClientAsync(envelope.ClientId, clientsRepository, accountsRepository);
                break;
            case "ClientUnblocked":
                await UnfreezeClientAsync(envelope.ClientId, clientsRepository, accountsRepository);
                break;
            default:
                await inboxDeadLetterRepository.MarkAsDeadLetterAsync(
                    envelope.EventId,
                    nameof(AntifraudConsumer),
                    JsonSerializer.Serialize(envelope),
                    $"Неизвестный тип {envelope.Type}");

                _logger.LogWarning($"Неизвестный тип события {envelope}");
                break;
        }
    }

    private async Task FreezeClientAsync(Guid clientId, IClientsRepository clientsRepository, IAccountsRepository accountsRepository)
    {
        var client = await clientsRepository.GetByIdAsync(clientId);
        if (client != null && !client.IsFrozen)
        {
            client.IsFrozen = true;
            await clientsRepository.UpdateAsync(client);

            var accounts = await accountsRepository.GetByOwnerIdAsync(client.Id);
            foreach (var account in accounts)
            {
                account.IsFrozen = true;
                await accountsRepository.UpdateAsync(account);
            }
            _logger.LogInformation($"Клиент {@clientId} и счета заморожены");
        }
    }

    private async Task UnfreezeClientAsync(Guid clientId, IClientsRepository clientsRepository, IAccountsRepository accountsRepository)
    {
        var client = await clientsRepository.GetByIdAsync(clientId);
        if (client != null && client.IsFrozen)
        {
            client.IsFrozen = false;
            await clientsRepository.UpdateAsync(client);

            var accounts = await accountsRepository.GetByOwnerIdAsync(client.Id);
            foreach (var account in accounts)
            {
                account.IsFrozen = false;
                await accountsRepository.UpdateAsync(account);
            }

            _logger.LogInformation($"Клиент {@clientId} и счета разморожены");
        }
    }

    public class Envelope
    {
        public Guid EventId { get; set; }
        public DateTime OccurredAt { get; set; }
        public string Type { get; set; } = "";
        public Guid ClientId { get; set; }
        public MetaData? Meta { get; set; }
    }

    public class MetaData
    {
        public string Version { get; set; } = "";
        public string Source { get; set; } = "";
        public Guid CorrelationId { get; set; }
        public Guid CausationId { get; set; }
    }
}