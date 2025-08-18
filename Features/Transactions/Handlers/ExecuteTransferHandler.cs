using BankAccountsApi.Features.Account.Enums;
using BankAccountsApi.Features.Transactions.Commands;
using BankAccountsApi.Infrastructure.Results;
using BankAccountsApi.Models;
using BankAccountsApi.Storage;
using BankAccountsApi.Storage.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace BankAccountsApi.Features.Transactions.Handlers;

public class ExecuteTransferHandler(
    ITransactionsRepository transactionStorage,
    IAccountsRepository accountStorage,
    AppDbContext context,
    IOutboxRepository outboxRepository) : IRequestHandler<ExecuteTransferCommand, MbResult<Unit>>
{
    public async Task<MbResult<Unit>> Handle(ExecuteTransferCommand request, CancellationToken cancellationToken)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);

        try
        {
            var sourceAccount = await accountStorage.GetByIdAsync(request.FromAccountId);
            var destinationAccount = await accountStorage.GetByIdAsync(request.ToAccountId);
            (bool flowControl, MbResult<Unit> value) = validateAccount(request, sourceAccount, destinationAccount);
            if (!flowControl)
            {
                return value;
            }

            var totalBalanceBefore = sourceAccount!.Balance + destinationAccount!.Balance;

            // Списываем и зачисляем средства
            sourceAccount.Balance -= request.Amount;
            destinationAccount.Balance += request.Amount;

            await accountStorage.UpdateAsync(sourceAccount);
            await accountStorage.UpdateAsync(destinationAccount);

            var transferId = Guid.NewGuid();
            // Создаём транзакции
            var debitTransaction = new Transaction
            {
                Id = Guid.NewGuid(),
                Type = Enums.TransactionType.Debit,
                AccountId = request.FromAccountId,
                Amount = -request.Amount,
                Currency = request.Currency,
                Description = $"Перевод средств на счёт {request.ToAccountId}",
                Date = DateTime.UtcNow,
                TransferId = transferId
            };

            var creditTransaction = new Transaction
            {
                Id = Guid.NewGuid(),
                AccountId = request.ToAccountId,
                Type = Enums.TransactionType.Credit,
                Amount = request.Amount,
                Currency = request.Currency,
                Description = $"Получение средств от счёта {request.FromAccountId}",
                Date = DateTime.UtcNow,
                TransferId = transferId
            };

            await transactionStorage.AddAsync(debitTransaction);
            await transactionStorage.AddAsync(creditTransaction);

            // Если получатель — депозит, начисляем проценты через процедуру
            if (destinationAccount.Type == AccountType.Deposit)
            {
                await accountStorage.AccrueInterestAsync(destinationAccount.Id);
            }

            var totalBalanceAfter = sourceAccount.Balance + destinationAccount.Balance;
            if (totalBalanceBefore != totalBalanceAfter)
            {
                throw new InvalidOperationException("Несоответствие итоговых балансов после перевода. Транзакция отменена");
            }

            await outboxRepository.AddEventAsync("TransferCompleted", new
            {
                EventId = Guid.NewGuid(),
                OccurredAt = DateTime.UtcNow,
                SourceAccountId = sourceAccount.Id,
                DestinationAccountId = destinationAccount.Id,
                request.Amount,
                request.Currency,
                TransferId = transferId
            });

            await transaction.CommitAsync(cancellationToken);

            return MbResult.Success();
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(cancellationToken);
            return MbResult.Failure<Unit>(MbError.Conflict("Конфликт обновления данных"));
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            return MbResult.Failure<Unit>(MbError.Internal($"Ошибка при выполнении перевода: {ex.Message}"));
        }
    }

    private static (bool flowControl, MbResult<Unit> value) validateAccount(ExecuteTransferCommand request, Models.Account? sourceAccount, Models.Account? destinationAccount)
    {
        if (sourceAccount == null)
            return (flowControl: false, value: MbResult.Failure<Unit>(MbError.NotFound($"Счёт отправителя не найден: {request.FromAccountId}")));

        if (sourceAccount.IsFrozen)
            return (flowControl: false, value: MbResult.Failure<Unit>(MbError.Conflict($"Дебетовые операции запрещены для этого счета")));

        if (destinationAccount == null)
            return (flowControl: false, value: MbResult.Failure<Unit>(MbError.NotFound($"Счёт получателя не найден: {request.ToAccountId}")));

        if (sourceAccount.Balance < request.Amount)
            return (flowControl: false, value: MbResult.Failure<Unit>(MbError.Validation(
                "Недостаточно средств на счёте отправителя",
                request.Amount.ToString(CultureInfo.InvariantCulture))));
        return (flowControl: true, value: MbResult.Success());
    }
}
