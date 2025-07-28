using BankAccountsApi.Features.Account.Dto;
using MediatR;

namespace BankAccountsApi.Features.Account.Queries;

/// <summary>
/// Запрос на получение списка всех счетов
/// </summary>
public class GetAccountsQuery : IRequest<List<AccountDto>>
{
}