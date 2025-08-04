using BankAccountsApi.Features.Account.Queries;
using BankAccountsApi.Infrastructure;
using BankAccountsApi.Storage.Interfaces;
using MediatR;

namespace BankAccountsApi.Features.Account.Handlers;

/// <summary>
/// Получение счетов пользователя
/// </summary>
public class GetAccountsByOwnerIdHandler(IInMemoryAccountStorage storage) : IRequestHandler<GetAccountsQuery, MbResult<List<BankAccountsApi.Models.Account>>>
{
    public Task<MbResult<List<BankAccountsApi.Models.Account>>> Handle(GetAccountsQuery request, CancellationToken cancellationToken)
    {
        var accounts = storage.GetByOwnerId(request.OwnerId);
        return Task.FromResult(MbResult<List<BankAccountsApi.Models.Account>>.Success(accounts));
    }
}