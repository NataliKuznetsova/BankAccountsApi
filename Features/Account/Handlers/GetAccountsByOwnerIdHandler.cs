using BankAccountsApi.Features.Account.Queries;
using BankAccountsApi.Infrastructure.Results;
using BankAccountsApi.Storage.Interfaces;
using MediatR;

namespace BankAccountsApi.Features.Account.Handlers;

/// <summary>
/// Получение счетов пользователя
/// </summary>
public class GetAccountsByOwnerIdHandler : IRequestHandler<GetAccountsQuery, MbResult<List<Models.Account>>>
{
    private readonly IAccountsRepository _storage;

    public GetAccountsByOwnerIdHandler(IAccountsRepository storage)
    {
        _storage = storage;
    }

    public async Task<MbResult<List<BankAccountsApi.Models.Account>>> Handle(GetAccountsQuery request, CancellationToken cancellationToken)
    {
        var accounts = await _storage.GetByOwnerIdAsync(request.OwnerId);
        return MbResult<List<Models.Account>>.Success(accounts);
    }
}
