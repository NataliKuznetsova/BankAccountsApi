using BankAccountsApi.Infrastructure.Errors;
using BankAccountsApi.Models;
using MediatR;

namespace BankAccountsApi.Features.Transactions.Queries;

public class GetStatementQuery(Guid accountId) : IRequest<MbResult<List<Transaction>>>
{
    public Guid AccountId { get; set; } = accountId;
}