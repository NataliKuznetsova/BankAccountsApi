using BankAccountsApi.Features.Currency.Query;
using BankAccountsApi.Infrastructure;
using BankAccountsApi.Storage.Interfaces;
using MediatR;

namespace BankAccountsApi.Features.Currency.Handlers;

public class CheckCurrencyExistsHandler(IInMemoryCurrencyStorage storage)
    : IRequestHandler<CheckCurrencyExistsQuery, MbResult<bool>>
{
    public Task<MbResult<bool>> Handle(CheckCurrencyExistsQuery request, CancellationToken cancellationToken)
    {
        var exists = storage.Exists(request.Code);
        return Task.FromResult(MbResult<bool>.Success(exists));
    }
}