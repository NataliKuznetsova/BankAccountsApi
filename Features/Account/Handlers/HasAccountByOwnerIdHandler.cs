using BankAccountsApi.Features.Account.Queries;
using BankAccountsApi.Infrastructure;
using BankAccountsApi.Storage.Interfaces;
using MediatR;

namespace BankAccountsApi.Features.Account.Handlers;

public class HasAccountByOwnerIdHandler : IRequestHandler<HasAccountByOwnerIdQuery, MbResult<bool>>
{
    private readonly IAccountsRepository _storage;

    public HasAccountByOwnerIdHandler(IAccountsRepository storage)
    {
        _storage = storage;
    }

    public async Task<MbResult<bool>> Handle(HasAccountByOwnerIdQuery request, CancellationToken cancellationToken)
    {
        var accounts = await _storage.GetByOwnerIdAsync(request.OwnerId);
        bool hasAccounts = accounts.Count != 0;
        return MbResult<bool>.Success(hasAccounts);
    }
}
