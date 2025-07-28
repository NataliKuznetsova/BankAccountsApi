using BankAccountsApi.Features.Account.Commands;
using BankAccountsApi.Features.Account.Dto;
using BankAccountsApi.Features.Account.Enums;
using BankAccountsApi.Storage;
using MediatR;

namespace BankAccountsApi.Features.Account.Handlers;

/// <summary>
/// Хэндлер для создания счета
/// </summary>
public class CreateAccountHandler : IRequestHandler<CreateAccountCommand, Guid>
{
    public Task<Guid> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        var newAccount = new AccountDto()
        {
            Id = Guid.NewGuid(),
            OwnerId = request.OwnerId,
            Type = request.Type,
            Currency = request.Currency,
            InterestRate = request.InterestRate,
            Balance = 0,
            OpenDate = DateTime.UtcNow
        };

        InMemoryDatabase.Accounts.Add(newAccount);
        return Task.FromResult(newAccount.Id);
    }
}