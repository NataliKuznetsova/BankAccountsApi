using BankAccountsApi.Features.Account.Dto;
using BankAccountsApi.Features.Account.Enums;
using BankAccountsApi.Features.Account.Queries;
using MediatR;

namespace BankAccountsApi.Features.Account.Handlers;

/// <summary>
/// Получение всех счетов
/// </summary>
public class GetAccountsHandler : IRequestHandler<GetAccountsQuery, List<AccountDto>>
{
    public Task<List<AccountDto>> Handle(GetAccountsQuery request, CancellationToken cancellationToken)
    {
        // Заглушка: возвращаем статический список счетов
        var accounts = new List<AccountDto>
        {
            new()
            {
                Id = Guid.NewGuid(),
                OwnerId = Guid.NewGuid(),
                Type = AccountType.Checking,
                Currency = "RUB",
                Balance = 1000,
                InterestRate = null,
                OpenDate = DateTime.UtcNow,
                CloseDate = null
            },
            new()
            {
                Id = Guid.NewGuid(),
                OwnerId = Guid.NewGuid(),
                Type = AccountType.Credit,
                Currency = "RUB",
                Balance = 5000,
                InterestRate = 3.0m,
                OpenDate = DateTime.UtcNow.AddMonths(-2),
                CloseDate = null
            }
        };

        return Task.FromResult(accounts);
    }
}