using BankAccountsApi.Infrastructure.Bus;
using BankAccountsApi.Models;
using BankAccountsApi.Storage;
using BankAccountsApi.Storage.Interfaces;
using Microsoft.EntityFrameworkCore;
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
    /// <summary>
    /// Интеграционный тест 
    /// </summary>
    [TestFixture]
    public class OutboxIntegrationTests
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        private RabbitMqContainer _rabbitMqContainer;
        private ServiceProvider _serviceProvider;
        private MessageBus _messageBus;
        private IOutboxRepository _outboxRepository;

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        /// <summary>
        /// 
        /// </summary>
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

            // Используем реальную БД PostgreSQL
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql("Host=localhost;Port=5432;Database=mydb;Username=postgres;Password=mypassword"));

            services.AddScoped<IOutboxRepository, OutboxRepository>();

            var eventRouting = new Dictionary<string, string>
            {
                { "AccountOpened", "account.created" }
            };

            services.AddSingleton(connectionFactory);
            services.AddSingleton(eventRouting);
            services.AddSingleton<MessageBus>();

            _serviceProvider = services.BuildServiceProvider();

            _outboxRepository = _serviceProvider.GetRequiredService<IOutboxRepository>();

            _messageBus = new MessageBus(
                _serviceProvider.GetRequiredService<IServiceScopeFactory>(),
                eventRouting,
                connectionFactory
            );
        }

        /// <summary>
        /// 
        /// </summary>
        [OneTimeTearDown]
        public async Task Teardown()
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

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public async Task OutboxPublishes50Events()
        {
            // Генерация 50 событий
            var events = new List<OutboxMessage>();
            for (int i = 0; i < 50; i++)
            {
                var accountId = Guid.NewGuid();
                var evt = new OutboxMessage
                {
                    Id = Guid.NewGuid(),
                    Type = "AccountOpened",
                    Payload = JsonSerializer.Serialize(new { AccountId = accountId }),
                    CreatedAt = DateTime.UtcNow
                };
                events.Add(evt);
                await _outboxRepository.AddEventAsync(evt.Type, evt);
            }

            // Настраиваем подписчика на очередь
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

            var receivedAccountIds = new ConcurrentBag<Guid>();
            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var envelope = JsonSerializer.Deserialize<JsonElement>(message);
                var payloadElement = envelope.GetProperty("payload");
                var innerPayloadString = payloadElement.GetProperty("Payload").GetString();
                var innerPayload = JsonSerializer.Deserialize<JsonElement>(innerPayloadString!);

                var accountId = innerPayload.GetProperty("AccountId").GetGuid();
                receivedAccountIds.Add(accountId);
            };

            channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);

            // Публикация всех событий
            await _messageBus.PublishPendingEventsAsync();

            // Ждём, пока придут все 50 сообщений
            var timeout = Task.Delay(5000);
            while (receivedAccountIds.Count < 50 && !timeout.IsCompleted)
            {
                await Task.Delay(50);
            }

            Assert.That(receivedAccountIds.Count, Is.EqualTo(50), "Не все события были получены");

            // Проверка, что в Outbox больше нет ожидающих событий
            var pendingEvents = await _outboxRepository.GetPendingEventsAsync();
            Assert.That(pendingEvents.Count, Is.EqualTo(0));
        }
    }
}
