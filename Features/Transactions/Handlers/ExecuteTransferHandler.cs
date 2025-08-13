using BankAccountsApi.Features.Transactions.Commands;
using BankAccountsApi.Infrastructure;
using BankAccountsApi.Models;
using BankAccountsApi.Storage;
using BankAccountsApi.Storage.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace BankAccountsApi.Features.Transactions.Handlers;

public class ExecuteTransferHandler : IRequestHandler<ExecuteTransferCommand, MbResult<Unit>>
{
    private readonly ITransactionsRepository _transactionStorage;
    private readonly IAccountsRepository _accountStorage;
    private readonly AppDbContext _context;

    public ExecuteTransferHandler(
        ITransactionsRepository transactionStorage,
        IAccountsRepository accountStorage,
        AppDbContext context)
    {
        _transactionStorage = transactionStorage;
        _accountStorage = accountStorage;
        _context = context;
    }

    public async Task<MbResult<Unit>> Handle(ExecuteTransferCommand request, CancellationToken cancellationToken)
    {
        // Запускаем транзакцию с уровнем изоляции Serializable
        await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);

        try
        {
            var sourceAccount = await _accountStorage.GetByIdAsync(request.FromAccountId);
            var destinationAccount = await _accountStorage.GetByIdAsync(request.ToAccountId);

            if (sourceAccount == null)
                return MbResult.Failure(MbError.NotFound($"Счёт отправителя не найден: {request.FromAccountId}"));

            if (destinationAccount == null)
                return MbResult.Failure(MbError.NotFound($"Счёт получателя не найден: {request.ToAccountId}"));

            if (sourceAccount.Balance < request.Amount)
                return MbResult.Failure(MbError.Validation("Недостаточно средств на счёте отправителя", request.Amount.ToString(CultureInfo.InvariantCulture)));

            var totalBalanceBefore = sourceAccount.Balance + destinationAccount.Balance;

            sourceAccount.Balance -= request.Amount;
            destinationAccount.Balance += request.Amount;

            await _accountStorage.UpdateAsync(sourceAccount);
            await _accountStorage.UpdateAsync(destinationAccount);

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

            await _transactionStorage.AddAsync(debitTransaction);
            await _transactionStorage.AddAsync(creditTransaction);

            var totalBalanceAfter = sourceAccount.Balance + destinationAccount.Balance;

            if (totalBalanceBefore != totalBalanceAfter)
            {
                throw new InvalidOperationException("Несоответствие итоговых балансов после перевода. Транзакция отменена");
            }

            await transaction.CommitAsync(cancellationToken);

            return MbResult.Success();
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(cancellationToken); 
            return MbResult.Failure(MbError.Conflict("Конфликт обновления данных"));
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            return MbResult.Failure(MbError.Internal($"Ошибка при выполнении перевода: {ex.Message}"));
        }
    }
}
