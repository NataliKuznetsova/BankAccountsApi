using BankAccountsApi.Features.Currency.Query;
using BankAccountsApi.Infrastructure.Results;
using BankAccountsApi.Storage.Interfaces;
using MediatR;

namespace BankAccountsApi.Features.Currency.Handlers;

public class CheckCurrencyExistsHandler(ICurrencyRepository storage)
    : IRequestHandler<CheckCurrencyExistsQuery, MbResult<bool>>
{
    public async Task<MbResult<bool>> Handle(CheckCurrencyExistsQuery request, CancellationToken cancellationToken)
    {
        var exists = await storage.ExistsAsync(request.Code);
        return MbResult<bool>.Success(exists);
    }
}