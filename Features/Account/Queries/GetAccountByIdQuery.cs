using BankAccountsApi.Infrastructure.Errors;
using MediatR;

namespace BankAccountsApi.Features.Account.Queries;

/// <summary>
/// Запрос на получение инфы о счёте
/// </summary>
public class GetAccountByIdQuery(Guid id) : IRequest<MbResult<Models.Account>>
{
    public Guid Id { get; set; } = id;
}