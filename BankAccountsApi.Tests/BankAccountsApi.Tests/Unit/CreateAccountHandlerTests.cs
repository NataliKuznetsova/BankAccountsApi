using BankAccountsApi.Features.Account.Commands;
using BankAccountsApi.Features.Account.Enums;
using BankAccountsApi.Features.Account.Handlers;
using BankAccountsApi.Models;
using BankAccountsApi.Storage;
using BankAccountsApi.Storage.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using NUnit.Framework;

namespace BankAccountsApi.Tests.Unit
{
    /// <summary>
    /// 
    /// </summary>
    [TestFixture]
    public class CreateAccountHandlerTests
    {
        private Mock<IAccountsRepository>? _accountRepoMock;
        private Mock<IClientsRepository>? _clientRepoMock;
        private Mock<IOutboxRepository>? _outboxRepositoryMock;
        private CreateAccountHandler? _handler;

        /// <summary>
        /// 
        /// </summary>
        [SetUp]
        public void Setup()
        {
            _accountRepoMock = new Mock<IAccountsRepository>();
            _clientRepoMock = new Mock<IClientsRepository>();
            _outboxRepositoryMock = new Mock<IOutboxRepository>();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            var context = new AppDbContext(options);

            _handler = new CreateAccountHandler(_accountRepoMock.Object, _clientRepoMock.Object, _outboxRepositoryMock.Object, context);
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public async Task Handle_ClientExists_CreatesAccountAndReturnsSuccess()
        {
            var ownerId = Guid.NewGuid();
            var command = new CreateAccountCommand
            {
                OwnerId = ownerId,
                Type = AccountType.Checking,
                Currency = "USD",
                InterestRate = 0.05m
            };

            _clientRepoMock?.Setup(c => c.ExistsAsync(ownerId)).ReturnsAsync(true);
            _accountRepoMock?.Setup(a => a.CreateAsync(It.IsAny<Account>())).Returns(Task.CompletedTask);

            var result = await _handler?.Handle(command, CancellationToken.None)!;

            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Value, Is.Not.EqualTo(Guid.Empty));
            });
            _accountRepoMock?.Verify(a => a.CreateAsync(It.Is<Account>(acc =>
                acc.OwnerId == ownerId &&
                acc.Type == command.Type &&
                acc.Currency == command.Currency &&
                acc.InterestRate == command.InterestRate &&
                acc.Balance == 0)), Times.Once);
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public async Task Handle_ClientDoesNotExist_ReturnsNotFound()
        {
            var ownerId = Guid.NewGuid();
            var command = new CreateAccountCommand
            {
                OwnerId = ownerId,
                Type = AccountType.Checking,
                Currency = "USD",
                InterestRate = 0.05m
            };

            _clientRepoMock?.Setup(c => c.ExistsAsync(ownerId)).ReturnsAsync(false);

            var result = await _handler?.Handle(command, CancellationToken.None)!;

            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Error?.Code, Is.EqualTo("not_found"));
            });
            _accountRepoMock?.Verify(a => a.CreateAsync(It.IsAny<Account>()), Times.Never);
        }
    }
}
