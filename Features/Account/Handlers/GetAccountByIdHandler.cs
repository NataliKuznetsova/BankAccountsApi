using BankAccountsApi.Features.Account.Queries;
using BankAccountsApi.Infrastructure.Results;
using BankAccountsApi.Storage.Interfaces;
using MediatR;

namespace BankAccountsApi.Features.Account.Handlers;

/// <summary>
/// Хэндлер для получения информации о счете
/// </summary>
public class GetAccountByIdHandler(IAccountsRepository storage) : IRequestHandler<GetAccountByIdQuery, MbResult<Models.Account>>
{
    public async Task<MbResult<Models.Account>> Handle(GetAccountByIdQuery request, CancellationToken cancellationToken)
    {
        var account = await storage.GetByIdAsync(request.Id);
        if (account == null)
        {
            return MbResult<Models.Account>.Failure(MbError.NotFound("Клиент не найден"));
        }
        return MbResult<Models.Account>.Success(account);
    }
}