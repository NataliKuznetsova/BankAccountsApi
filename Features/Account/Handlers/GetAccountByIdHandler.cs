using BankAccountsApi.Features.Account.Dto;
using BankAccountsApi.Features.Account.Queries;
using BankAccountsApi.Storage;
using MediatR;

namespace BankAccountsApi.Features.Account.Handlers;

/// <summary>
/// Хэндлер для получения информации о счете
/// </summary>
public class GetAccountByIdHandler : IRequestHandler<GetAccountByIdQuery, AccountDto>
{
    public Task<AccountDto> Handle(GetAccountByIdQuery request, CancellationToken cancellationToken)
    {
        var account = InMemoryDatabase.Accounts
            .FirstOrDefault(x => x.Id == request.Id);

        return Task.FromResult(account ?? new AccountDto());
    }
}