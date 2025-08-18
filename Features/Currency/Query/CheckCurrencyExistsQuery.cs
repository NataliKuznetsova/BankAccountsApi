using BankAccountsApi.Infrastructure.Errors;
using MediatR;

namespace BankAccountsApi.Features.Currency.Query;

public class CheckCurrencyExistsQuery(string code) : IRequest<MbResult<bool>>
{
    public string Code { get; set; } = code;
}