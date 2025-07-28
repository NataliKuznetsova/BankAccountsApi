using BankAccountsApi.Features.Transactions.Dto;
using MediatR;

namespace BankAccountsApi.Features.Transactions.Queries;

public class GetStatementQuery(Guid accountId) : IRequest<StatementDto>
{
    public Guid AccountId { get; set; } = accountId;
}