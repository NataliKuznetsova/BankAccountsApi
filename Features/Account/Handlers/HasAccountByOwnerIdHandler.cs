using BankAccountsApi.Features.Account.Queries;
using BankAccountsApi.Infrastructure;
using BankAccountsApi.Storage.Interfaces;
using MediatR;

namespace BankAccountsApi.Features.Account.Handlers;

public class HasAccountByOwnerIdHandler(IInMemoryAccountStorage storage): IRequestHandler<HasAccountByOwnerIdQuery, MbResult<bool>>
{
    public Task<MbResult<bool>> Handle(HasAccountByOwnerIdQuery request, CancellationToken cancellationToken)
    {
        var accounts = storage.GetByOwnerId(request.OwnerId);
        return Task.FromResult(MbResult<bool>.Success(accounts.Count != 0));
    }
}