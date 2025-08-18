using BankAccountsApi.Features.Transactions.Queries;
using BankAccountsApi.Infrastructure.Errors;
using BankAccountsApi.Models;
using BankAccountsApi.Storage.Interfaces;
using MediatR;

namespace BankAccountsApi.Features.Transactions.Handlers;

public class GetStatementHandler : IRequestHandler<GetStatementQuery, MbResult<List<Transaction>>>
{
    private readonly ITransactionsRepository _transactionStorage;

    public GetStatementHandler(ITransactionsRepository transactionStorage)
    {
        _transactionStorage = transactionStorage;
    }

    public async Task<MbResult<List<Transaction>>> Handle(GetStatementQuery request, CancellationToken cancellationToken)
    {
        var statement = await _transactionStorage.GetByAccountIdAsync(request.AccountId);
        return MbResult<List<Transaction>>.Success(statement);
    }
}
