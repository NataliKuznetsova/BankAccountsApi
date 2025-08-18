using BankAccountsApi.Features.Transactions.Commands;
using BankAccountsApi.Features.Transactions.Enums;
using BankAccountsApi.Features.Transactions.Handlers;
using BankAccountsApi.Infrastructure.Bus;
using BankAccountsApi.Infrastructure.Results;
using BankAccountsApi.Models;
using BankAccountsApi.Storage;
using BankAccountsApi.Storage.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace BankAccountsApi.Tests.Integration
{
    /// <summary>
    /// 
    /// </summary>
    [TestFixture]
    public class AntifraudConsumerTests
    {
        private AppDbContext? _context;
        private IClientsRepository? _clientsRepository;
        private IAccountsRepository? _accountsRepository;
        private IInboxRepository? _inboxRepository;
        private IInboxDeadLetterRepository? _inboxDeadLetterRepository;
        private AntifraudConsumer? _consumer;

        /// <summary>
        /// 
        /// </summary>
        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);

            _clientsRepository = new ClientsRepository(_context);
            _accountsRepository = new AccountsRepository(_context);
            _inboxRepository = new InboxRepository(_context);
            _inboxDeadLetterRepository = new InboxDeadLetterRepository(_context);

            var serviceProvider = new ServiceCollection()
                .AddScoped(_ => _clientsRepository)
                .AddScoped(_ => _accountsRepository)
                .AddScoped(_ => _inboxRepository)
                .AddScoped(_ => _inboxDeadLetterRepository)
                .BuildServiceProvider();

            var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

            _consumer = new AntifraudConsumer(
                scopeFactory,
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                null,
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                NullLogger<AntifraudConsumer>.Instance
            );
        }

        /// <summary>
        /// Блокируем клиента
        /// </summary>
        [Test]
        public async Task ClientBlockedPreventsDebit()
        {
            // Arrange
            var client = new Client { Id = Guid.NewGuid(), IsFrozen = false };
            await _clientsRepository.AddAsync(client);

            var account = new Account { Id = Guid.NewGuid(), Currency = "RUB", OwnerId = client.Id, IsFrozen = false };
            await _accountsRepository.CreateAsync(account);

            var envelope = new AntifraudConsumer.Envelope
            {
                EventId = Guid.NewGuid(),
                ClientId = client.Id,
                Type = "ClientBlocked",
                OccurredAt = DateTime.UtcNow,
                Meta = new AntifraudConsumer.MetaData { Version = "v1" }
            };

            await _consumer.HandleEventAsync(envelope);

            var updatedClient = await _clientsRepository.GetByIdAsync(client.Id);
            var updatedAccounts = await _accountsRepository.GetByOwnerIdAsync(client.Id);

            Assert.That(updatedClient?.IsFrozen, Is.True);
            Assert.That(updatedAccounts.All(a => a.IsFrozen), Is.True);

            // Проверяем, что дебет запрещён
            var handler = new RegisterTransactionHandler(
                new TransactionsRepository(_context),
                _context,
                new OutboxRepository(_context),
                _accountsRepository
            );

            var command = new RegisterTransactionCommand
            {
                AccountId = account.Id,
                Amount = 100,
                Currency = "RUB",
                Type = TransactionType.Debit
            };

            var result = await handler.Handle(command, default);

            Assert.That(result.Error!.Code, Is.EqualTo(ErrorCodes.Conflict));
            Assert.That(result.Error.Message, Does.Contain("Дебетовые операции запрещены"));
        }
    }
}
