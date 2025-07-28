using BankAccountsApi.Features.Account.Dto;
using MediatR;

namespace BankAccountsApi.Features.Account.Queries;

/// <summary>
/// Запрос на получение инфы о счёте
/// </summary>
public class GetAccountByIdQuery(Guid id) : IRequest<AccountDto>
{
    public Guid Id { get; set; } = id;
}