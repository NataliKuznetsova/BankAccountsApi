using BankAccountsApi.Features.Account.Commands;
using BankAccountsApi.Features.Account.Enums;
using BankAccountsApi.Features.Account.Handlers;
using BankAccountsApi.Models;
using BankAccountsApi.Storage.Interfaces;
using Moq;

namespace BankAccountsApi.Tests.Unit
{
    [TestFixture]
    public class CreateAccountHandlerTests
    {
        private Mock<IAccountsRepository>? _accountRepoMock;
        private Mock<IClientsRepository>? _clientRepoMock;
        private CreateAccountHandler? _handler;

        [SetUp]
        public void Setup()
        {
            _accountRepoMock = new Mock<IAccountsRepository>();
            _clientRepoMock = new Mock<IClientsRepository>();
            _handler = new CreateAccountHandler(_accountRepoMock.Object, _clientRepoMock.Object);
        }

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

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.EqualTo(Guid.Empty));
            _accountRepoMock?.Verify(a => a.CreateAsync(It.Is<Account>(acc =>
                acc.OwnerId == ownerId &&
                acc.Type == command.Type &&
                acc.Currency == command.Currency &&
                acc.InterestRate == command.InterestRate &&
                acc.Balance == 0)), Times.Once);
        }

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

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error?.Code, Is.EqualTo("not_found"));
            _accountRepoMock?.Verify(a => a.CreateAsync(It.IsAny<Account>()), Times.Never);
        }
    }
}
