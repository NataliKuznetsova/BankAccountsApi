using BankAccountsApi.Features.Account.Queries;
using BankAccountsApi.Infrastructure;
using BankAccountsApi.Storage.Interfaces;
using MediatR;

namespace BankAccountsApi.Features.Account.Handlers;

/// <summary>
/// Хэндлер для получения информации о счете
/// </summary>
public class GetAccountByIdHandler(IInMemoryAccountStorage storage) : IRequestHandler<GetAccountByIdQuery, MbResult<Models.Account>>
{
    public Task<MbResult<Models.Account>> Handle(GetAccountByIdQuery request, CancellationToken cancellationToken)
    {
        var account = storage.GetById(request.Id);
        if (account == null)
        {
            return Task.FromResult(MbResult<Models.Account>.Failure(MbError.NotFound("Клиент не найден")));
        }
        return Task.FromResult(MbResult<Models.Account>.Success(account));
    }
}