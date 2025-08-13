using BankAccountsApi.Features.Transactions.Commands;
using BankAccountsApi.Features.Transactions.Handlers;
using BankAccountsApi.Models;
using BankAccountsApi.Storage;
using BankAccountsApi.Storage.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;

namespace BankAccountsApi.Tests.Unit
{
    [TestFixture]
    public class ExecuteTransferHandlerTests
    {
        private Mock<ITransactionsRepository>? _transactionRepoMock;
        private Mock<IAccountsRepository>? _accountRepoMock;
        private AppDbContext? _context;
        private ExecuteTransferHandler? _handler;

        [SetUp]
        public void Setup()
        {
            _transactionRepoMock = new Mock<ITransactionsRepository>();
            _accountRepoMock = new Mock<IAccountsRepository>();

            // Создаем опции InMemory базы с подавлением предупреждения о транзакциях
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) // уникальная база на каждый тест
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning)) // Подавляем ошибку о транзакциях
                .Options;

            // Реальный контекст EF Core с InMemory
            _context = new AppDbContext(options);

            _handler = new ExecuteTransferHandler(
                _transactionRepoMock.Object,
                _accountRepoMock.Object,
                _context);
        }

        [Test]
        public async Task Handle_SuccessfulTransfer_ReturnsSuccess()
        {
            var sourceAccountId = Guid.NewGuid();
            var destinationAccountId = Guid.NewGuid();
            var amount = 100m;

            var sourceAccount = new Account
            {
                Id = sourceAccountId,
                Balance = 200m,
                Currency = "RUB"
            };
            var destinationAccount = new Account
            {
                Id = destinationAccountId,
                Balance = 50m,
                Currency = "RUB"
            };

            _accountRepoMock!.Setup(r => r.GetByIdAsync(sourceAccountId)).ReturnsAsync(sourceAccount);
            _accountRepoMock!.Setup(r => r.GetByIdAsync(destinationAccountId)).ReturnsAsync(destinationAccount);
            _accountRepoMock!.Setup(r => r.UpdateAsync(It.IsAny<Account>())).Returns(Task.CompletedTask);

            _transactionRepoMock!.Setup(r => r.AddAsync(It.IsAny<Transaction>())).Returns(Task.CompletedTask);

            var command = new ExecuteTransferCommand
            {
                FromAccountId = sourceAccountId,
                ToAccountId = destinationAccountId,
                Amount = amount,
                Currency = "RUB"
            };

            var result = await _handler!.Handle(command, CancellationToken.None);

            Assert.That(result.IsSuccess, Is.True);
            _accountRepoMock!.Verify(
                r => r.UpdateAsync(It.Is<Account>(a => a.Id == sourceAccountId && a.Balance == 100m)), Times.Once);
            _accountRepoMock!.Verify(
                r => r.UpdateAsync(It.Is<Account>(a => a.Id == destinationAccountId && a.Balance == 150m)), Times.Once);
            _transactionRepoMock!.Verify(r => r.AddAsync(It.IsAny<Transaction>()), Times.Exactly(2));
        }

        [Test]
        public async Task Handle_SourceAccountNotFound_ReturnsNotFound()
        {
            var command = new ExecuteTransferCommand
            {
                FromAccountId = Guid.NewGuid(),
                ToAccountId = Guid.NewGuid(),
                Amount = 100,
                Currency = "RUB"
            };

            _accountRepoMock!.Setup(r => r.GetByIdAsync(command.FromAccountId)).ReturnsAsync((Account?)null);

            var result = await _handler!.Handle(command, CancellationToken.None);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error?.Code, Is.EqualTo("not_found"));
        }

        [Test]
        public async Task Handle_DestinationAccountNotFound_ReturnsNotFound()
        {
            var sourceAccount = new Account
            {
                Id = Guid.NewGuid(),
                Balance = 200m,
                Currency = "RUB"
            };
            var command = new ExecuteTransferCommand
            {
                FromAccountId = sourceAccount.Id,
                ToAccountId = Guid.NewGuid(),
                Amount = 100,
                Currency = "RUB"
            };

            _accountRepoMock!.Setup(r => r.GetByIdAsync(command.FromAccountId)).ReturnsAsync(sourceAccount);
            _accountRepoMock!.Setup(r => r.GetByIdAsync(command.ToAccountId)).ReturnsAsync((Account?)null);

            var result = await _handler!.Handle(command, CancellationToken.None);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error?.Code, Is.EqualTo("not_found"));
        }

        [Test]
        public async Task Handle_InsufficientFunds_ReturnsValidationError()
        {
            var sourceAccount = new Account
            {
                Id = Guid.NewGuid(),
                Balance = 50m,
                Currency = "RUB"
            };
            var destinationAccount = new Account
            {
                Id = Guid.NewGuid(),
                Balance = 100m,
                Currency = "RUB"
            };
            var command = new ExecuteTransferCommand
            {
                FromAccountId = sourceAccount.Id,
                ToAccountId = destinationAccount.Id,
                Amount = 100,
                Currency = "RUB"
            };

            _accountRepoMock!.Setup(r => r.GetByIdAsync(command.FromAccountId)).ReturnsAsync(sourceAccount);
            _accountRepoMock!.Setup(r => r.GetByIdAsync(command.ToAccountId)).ReturnsAsync(destinationAccount);

            var result = await _handler!.Handle(command, CancellationToken.None);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error?.Code, Is.EqualTo("validationfailed"));
        }

        [Test]
        public async Task Handle_DbUpdateConcurrencyException_ReturnsConflictError()
        {
            var sourceAccount = new Account
            {
                Id = Guid.NewGuid(),
                Balance = 200m,
                Currency = "RUB"
            };
            var destinationAccount = new Account
            {
                Id = Guid.NewGuid(),
                Balance = 100m,
                Currency = "RUB"
            };
            var command = new ExecuteTransferCommand
            {
                FromAccountId = sourceAccount.Id,
                ToAccountId = destinationAccount.Id,
                Amount = 50,
                Currency = "RUB"
            };

            _accountRepoMock!.Setup(r => r.GetByIdAsync(command.FromAccountId)).ReturnsAsync(sourceAccount);
            _accountRepoMock!.Setup(r => r.GetByIdAsync(command.ToAccountId)).ReturnsAsync(destinationAccount);

            // Здесь делаем так, чтобы UpdateAsync бросал исключение DbUpdateConcurrencyException
            _accountRepoMock!.Setup(r => r.UpdateAsync(It.IsAny<Account>()))
                .ThrowsAsync(new DbUpdateConcurrencyException());

            _transactionRepoMock!.Setup(r => r.AddAsync(It.IsAny<Transaction>())).Returns(Task.CompletedTask);

            var result = await _handler!.Handle(command, CancellationToken.None);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error?.Code, Is.EqualTo("conflict"));
        }
    }
}
