using BankAccountsApi.Infrastructure.Bus;
using BankAccountsApi.Models;
using BankAccountsApi.Storage.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using Testcontainers.RabbitMq;

namespace BankAccountsApi.Tests.Integration
{
    [TestFixture]
    public class OutboxIntegrationTests
    {
        private RabbitMqContainer _rabbitMqContainer;
        private ServiceProvider _serviceProvider;
        private MessageBus _messageBus;
        private IOutboxRepository _outboxRepository;

        [OneTimeSetUp]
        public async Task Setup()
        {
            _rabbitMqContainer = new RabbitMqBuilder()
                .WithImage("rabbitmq:3.12")
                .WithUsername("guest")
                .WithPassword("guest")
                .Build();

            await _rabbitMqContainer.StartAsync();

            var connectionFactory = new ConnectionFactory
            {
                Uri = new Uri(_rabbitMqContainer.GetConnectionString()),
                AutomaticRecoveryEnabled = true
            };

            var services = new ServiceCollection();
            services.AddScoped<IOutboxRepository, InMemoryOutboxRepository>();

            var eventRouting = new Dictionary<string, string>
            {
                { "AccountOpened", "account.created" }
            };

            services.AddSingleton(connectionFactory);
            services.AddSingleton(eventRouting);
            services.AddSingleton<MessageBus>();

            _serviceProvider = services.BuildServiceProvider();

            _messageBus = new MessageBus(
                _serviceProvider.GetRequiredService<IServiceScopeFactory>(),
                eventRouting,
                connectionFactory
            );

            _outboxRepository = _serviceProvider.GetRequiredService<IOutboxRepository>();
        }

        [OneTimeTearDown]
        public async Task GlobalTeardown()
        {
            if (_rabbitMqContainer != null)
            {
                await _rabbitMqContainer.StopAsync();
                await _rabbitMqContainer.DisposeAsync();
            }

            if (_serviceProvider != null)
            {
                await _serviceProvider.DisposeAsync();
            }
        }
        [Test]
        public async Task OutboxPublishesAfterFailure()
        {
            var accountId = Guid.NewGuid();
            var evt = new OutboxMessage
            {
                Id = Guid.NewGuid(),
                Type = "AccountOpened",
                Payload = JsonSerializer.Serialize(new { AccountId = accountId }),
                CreatedAt = DateTime.UtcNow
            };
            await _outboxRepository.AddEventAsync(evt.Type, evt);

            // симулируем недоступность RabbitMQ
            var badConnectionFactory = new ConnectionFactory
            {
                HostName = "localhost",
                Port = 12345,
                UserName = "guest",
                Password = "guest",
                AutomaticRecoveryEnabled = true
            };

            var messageBusWithFailure = new MessageBus(
                _serviceProvider.GetRequiredService<IServiceScopeFactory>(),
                new Dictionary<string, string> { { "AccountOpened", "account.created" } },
                badConnectionFactory
            );

            // попытка публикации при недоступности
            var publishTask = messageBusWithFailure.PublishPendingEventsAsync();
            await Task.Delay(500);

            var pendingAfterFailure = await _outboxRepository.GetPendingEventsAsync();
            Assert.That(pendingAfterFailure.Count, Is.EqualTo(1));

            // создаём очередь и подписчика
            var factory = new ConnectionFactory
            {
                Uri = new Uri(_rabbitMqContainer.GetConnectionString()),
                AutomaticRecoveryEnabled = true
            };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            channel.ExchangeDeclare("account.events", ExchangeType.Topic, durable: true);

            var queueName = channel.QueueDeclare().QueueName;
            channel.QueueBind(queueName, "account.events", "account.created");

            var consumer = new EventingBasicConsumer(channel);
            var tcs = new TaskCompletionSource<JsonElement>();

            consumer.Received += (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var envelope = JsonSerializer.Deserialize<JsonElement>(message);
                    var payloadElement = envelope.GetProperty("payload");
                    var innerPayloadString = payloadElement.GetProperty("Payload").GetString();
                    var innerPayload = JsonSerializer.Deserialize<JsonElement>(innerPayloadString!);

                    tcs.TrySetResult(innerPayload);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            };

            channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);

            // пробуем снова — с нормальным соединением
            await _messageBus.PublishPendingEventsAsync();

            // ждём получения сообщения с таймаутом
            var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(5000));
            if (completedTask != tcs.Task)
                Assert.Fail("Сообщение не было получено за 5 секунд");

            // берём результат
            var payloadValue = await tcs.Task;
            Assert.That(payloadValue.GetProperty("AccountId").GetGuid(), Is.EqualTo(accountId));

            // проверяем, что больше нет ожидающих событий
            var pendingEvents = await _outboxRepository.GetPendingEventsAsync();
            Assert.That(pendingEvents.Count, Is.EqualTo(0));
        }
    }

    public class InMemoryOutboxRepository : IOutboxRepository
    {
        private readonly ConcurrentBag<OutboxMessage> _messages = new();

        public Task AddEventAsync(string type, object payload)
        {
            var message = new OutboxMessage
            {
                Id = Guid.NewGuid(),
                Type = type,
                Payload = JsonSerializer.Serialize(payload),
                CreatedAt = DateTime.UtcNow,
                IsPublished = false,
                IsFailed = false
            };
            _messages.Add(message);
            return Task.CompletedTask;
        }

        public Task<List<OutboxMessage>> GetPendingEventsAsync()
        {
            var pending = _messages.Where(m => !m.IsPublished && !m.IsFailed).ToList();
            return Task.FromResult(pending);
        }

        public Task MarkAsPublishedAsync(Guid id)
        {
            var message = _messages.FirstOrDefault(m => m.Id == id);
            if (message != null) message.IsPublished = true;
            return Task.CompletedTask;
        }

        public Task MarkAsFailedAsync(Guid id, string reason)
        {
            var message = _messages.FirstOrDefault(m => m.Id == id);
            if (message != null)
            {
                message.IsFailed = true;
                message.FailureReason = reason;
                message.FailedAt = DateTime.UtcNow;
            }
            return Task.CompletedTask;
        }
    }
}
