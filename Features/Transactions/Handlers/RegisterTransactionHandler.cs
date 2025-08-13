using BankAccountsApi.Features.Transactions.Commands;
using BankAccountsApi.Infrastructure;
using BankAccountsApi.Models;
using BankAccountsApi.Storage.Interfaces;
using MediatR;

namespace BankAccountsApi.Features.Transactions.Handlers;

public class RegisterTransactionHandler : IRequestHandler<RegisterTransactionCommand, MbResult<Guid>>
{
    private readonly ITransactionsRepository _transactionStorage;

    public RegisterTransactionHandler(ITransactionsRepository transactionStorage)
    {
        _transactionStorage = transactionStorage;
    }

    public async Task<MbResult<Guid>> Handle(RegisterTransactionCommand request, CancellationToken cancellationToken)
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

        await _transactionStorage.AddAsync(transaction);

        return MbResult<Guid>.Success(transaction.Id);
    }
}
