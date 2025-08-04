using BankAccountsApi.Features.Transactions.Queries;
using BankAccountsApi.Infrastructure;
using BankAccountsApi.Models;
using BankAccountsApi.Storage.Interfaces;
using MediatR;

namespace BankAccountsApi.Features.Transactions.Handlers;

public class GetStatementHandler(IInMemoryTransactionStorage memoryTransactionStorage) : IRequestHandler<GetStatementQuery, MbResult<List<Transaction>>>
{
    public Task<MbResult<List<Transaction>>> Handle(GetStatementQuery request, CancellationToken cancellationToken)
    {
        var statement = memoryTransactionStorage.GetByAccountId(request.AccountId);
        return Task.FromResult(MbResult<List<Transaction>>.Success(statement));
    }
}