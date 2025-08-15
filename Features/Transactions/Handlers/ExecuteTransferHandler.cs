using System.Globalization;
using BankAccountsApi.Features.Transactions.Commands;
using BankAccountsApi.Infrastructure;
using BankAccountsApi.Models;
using BankAccountsApi.Storage.Interfaces;
using MediatR;

namespace BankAccountsApi.Features.Transactions.Handlers;

public class ExecuteTransferHandler(IInMemoryTransactionStorage memoryTransactionStorage, IInMemoryAccountStorage accountStorage)
    : IRequestHandler<ExecuteTransferCommand, MbResult<Unit>>
{
    public Task<MbResult<Unit>> Handle(ExecuteTransferCommand request, CancellationToken cancellationToken)
    {
        // Получение счетов
        var sourceAccount = accountStorage.GetById(request.FromAccountId);
        var destinationAccount = accountStorage.GetById(request.ToAccountId);

        // Проверка наличия счетов
        if (sourceAccount is null)
            return Task.FromResult(MbResult.Failure(MbError.NotFound($"Счёт отправителя не найден: {request.FromAccountId}")));

        if (destinationAccount is null)
            return Task.FromResult(MbResult.Failure(MbError.NotFound($"Счёт получателя не найден: {request.ToAccountId}")));

        // Проверка баланса
        if (sourceAccount.Balance < request.Amount)
            return Task.FromResult(MbResult.Failure(MbError.Validation("Недостаточно средств на счёте отправителя", request.Amount.ToString(CultureInfo.InvariantCulture))));

        sourceAccount.Balance -= request.Amount;
        destinationAccount.Balance += request.Amount;

        accountStorage.Update(sourceAccount);
        accountStorage.Update(destinationAccount);

        var debitTransaction = new Transaction
        {
            Id = Guid.NewGuid(),
            AccountId = request.FromAccountId,
            Amount = -request.Amount,
            Currency = request.Currency,
            Description = $"Перевод средств на счёт {request.ToAccountId}",
            Date = DateTime.UtcNow
        };

        var creditTransaction = new Transaction
        {
            Id = Guid.NewGuid(),
            AccountId = request.ToAccountId,
            Amount = request.Amount,
            Currency = request.Currency,
            Description = $"Получение средств от счёта {request.FromAccountId}",
            Date = DateTime.UtcNow
        };

        memoryTransactionStorage.Add(debitTransaction);
        memoryTransactionStorage.Add(creditTransaction);

        return Task.FromResult(MbResult.Success());
    }
}