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
using Testcontainers.PostgreSql;

namespace BankAccountsApi.Tests.Integration
{
    /// <summary>
    /// OutboxIntegrationTests
    /// </summary>
    [TestFixture]
    public class OutboxIntegrationTests
    {
        private RabbitMqContainer? _rabbitMqContainer;
        private PostgreSqlContainer? _postgresContainer;
        private ServiceProvider? _serviceProvider;
        private MessageBus? _messageBus;
        private IOutboxRepository? _outboxRepository;

        /// <summary>
        /// Setup
        /// </summary>
        /// <returns></returns>
        [OneTimeSetUp]
        public async Task Setup()
        {
            _postgresContainer = new PostgreSqlBuilder()
                .WithDatabase("mydb")
                .WithUsername("postgres")
                .WithPassword("mypassword")
                .Build();

            await _postgresContainer.StartAsync();

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

            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(_postgresContainer.GetConnectionString()));

            services.AddScoped<IOutboxRepository, OutboxRepository>();

            var eventRouting = new Dictionary<string, string>
            {
                { "AccountOpened", "account.created" }
            };

            services.AddSingleton(connectionFactory);
            services.AddSingleton(eventRouting);
            services.AddSingleton<MessageBus>();

            _serviceProvider = services.BuildServiceProvider();

            // Применяем миграции к БД перед тестами
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await dbContext.Database.MigrateAsync();
            }

            _outboxRepository = _serviceProvider.GetRequiredService<IOutboxRepository>();

            _messageBus = new MessageBus(
                _serviceProvider.GetRequiredService<IServiceScopeFactory>(),
                eventRouting,
                connectionFactory
            );
        }

        [OneTimeTearDown]
        public async Task Teardown()
        {
            if (_rabbitMqContainer != null)
            {
                await _rabbitMqContainer.StopAsync();
                await _rabbitMqContainer.DisposeAsync();
            }

            if (_postgresContainer != null)
            {
                await _postgresContainer.StopAsync();
                await _postgresContainer.DisposeAsync();
            }

            if (_serviceProvider != null)
            {
                await _serviceProvider.DisposeAsync();
            }
        }

        [Test]
        public async Task OutboxPublishes50Events()
        {
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
                await _outboxRepository?.AddEventAsync(evt.Type, evt)!;
            }

            var factory = new ConnectionFactory
            {
                Uri = new Uri(_rabbitMqContainer?.GetConnectionString()!),
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

            await _messageBus.PublishPendingEventsAsync();

            var timeout = Task.Delay(5000);
            while (receivedAccountIds.Count < 50 && !timeout.IsCompleted)
            {
                await Task.Delay(50);
            }

            Assert.That(receivedAccountIds.Count, Is.EqualTo(50));
            var pendingEvents = await _outboxRepository.GetPendingEventsAsync();
            Assert.That(pendingEvents.Count, Is.EqualTo(0));
        }
    }
}
