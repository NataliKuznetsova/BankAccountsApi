using BankAccountsApi.Features.Transactions.Commands;
using BankAccountsApi.Infrastructure;
using BankAccountsApi.Models;
using BankAccountsApi.Storage.Interfaces;
using MediatR;

namespace BankAccountsApi.Features.Transactions.Handlers;

public class RegisterTransactionHandler(IInMemoryTransactionStorage memoryTransactionStorage) : IRequestHandler<RegisterTransactionCommand, MbResult<Guid>>
{
    public Task<MbResult<Guid>> Handle(RegisterTransactionCommand request, CancellationToken cancellationToken)
    {
        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            AccountId = request.AccountId,
            Amount = request.Amount,
            Currency = request.Currency,
            Description = request.Description,
            Date = DateTime.UtcNow
        };

        memoryTransactionStorage.Add(transaction);

        return Task.FromResult(MbResult<Guid>.Success(transaction.Id));
    }
}