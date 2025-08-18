using BankAccountsApi.Features.Transactions.Commands;
using BankAccountsApi.Features.Transactions.Enums;
using BankAccountsApi.Infrastructure.Errors;
using BankAccountsApi.Infrastructure.Results;
using BankAccountsApi.Models;
using BankAccountsApi.Storage;
using BankAccountsApi.Storage.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BankAccountsApi.Features.Transactions.Handlers;

public class RegisterTransactionHandler(ITransactionsRepository transactionStorage, 
    AppDbContext context,
    IOutboxRepository outboxRepository,
    IAccountsRepository accountStorage) 
    : IRequestHandler<RegisterTransactionCommand, MbResult<Guid>>
{
    public async Task<MbResult<Guid>> Handle(RegisterTransactionCommand request, CancellationToken cancellationToken)
    {
        var sourceAccount = await accountStorage.GetByIdAsync(request.AccountId);
        if (sourceAccount == null)
            return MbResult.Failure<Guid>(MbError.NotFound($"Счёт отправителя не найден: {request.AccountId}"));

        if (sourceAccount.IsFrozen)
            return MbResult.Failure<Guid>(MbError.Conflict($"Дебетовые операции запрещены для этого счета"));

        await using var transaction = await context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);
        try
        {
            var transactionModel = new Transaction
            {
                Id = Guid.NewGuid(),
                AccountId = request.AccountId,
                Amount = request.Amount,
                Currency = request.Currency,
                Description = request.Description,
                Date = DateTime.UtcNow
            };

            await transactionStorage.AddAsync(transactionModel);
            await AddEventAsync(request.Type, transactionModel);
            return MbResult<Guid>.Success(transactionModel.Id);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            return MbResult.Failure<Guid>(MbError.Internal($"Ошибка при выполнении транзацкии: {ex.Message}"));
        }
    }
    private async Task AddEventAsync(TransactionType type, Transaction request)
    {
        var eventType = type == TransactionType.Debit ? "MoneyDebited" : "MoneyCredited";
        await outboxRepository.AddEventAsync(eventType, new
        {
            EventId = Guid.NewGuid(),
            OccurredAt = DateTime.UtcNow,
            request.AccountId,
            request.Currency,
            request.Amount,
            OperationId = request.Id,
            Reason = request.Description
        });
    }
}
