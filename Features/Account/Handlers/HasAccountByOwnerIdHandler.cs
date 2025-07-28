using BankAccountsApi.Features.Account.Dto;
using BankAccountsApi.Features.Account.Queries;
using BankAccountsApi.Storage;
using MediatR;

namespace BankAccountsApi.Features.Account.Handlers;

public class HasAccountByOwnerIdHandler: IRequestHandler<HasAccountByOwnerIdQuery, bool>
{
    public Task<bool> Handle(HasAccountByOwnerIdQuery request, CancellationToken cancellationToken)
    {
        var account = InMemoryDatabase.Accounts
            .Any(x => x.OwnerId  == request.OwnerId);

        return Task.FromResult(account);
    }
}