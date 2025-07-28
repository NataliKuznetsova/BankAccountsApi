using BankAccountsApi.Features.Transactions.Dto;
using BankAccountsApi.Features.Transactions.Enums;
using BankAccountsApi.Features.Transactions.Queries;
using MediatR;

namespace BankAccountsApi.Features.Transactions.Handlers;

public class GetStatementHandler : IRequestHandler<GetStatementQuery, StatementDto>
{
    public Task<StatementDto> Handle(GetStatementQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new StatementDto
        {
            AccountId = request.AccountId,
            Transactions =
            [
                new TransactionDto
                {
                    Id = Guid.NewGuid(),
                    Amount = 1000,
                    Currency = "RUB",
                    Type = TransactionType.Credit,
                    Description = "Зачисление",
                    Date = DateTime.UtcNow
                }
            ]
        });
    }
}