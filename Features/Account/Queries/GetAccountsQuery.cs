using BankAccountsApi.Infrastructure.Results;
using MediatR;

namespace BankAccountsApi.Features.Account.Queries;

/// <summary>
/// Запрос на получение списка всех счетов
/// </summary>
public class GetAccountsQuery : IRequest<MbResult<List<BankAccountsApi.Models.Account>>>
{
    public Guid OwnerId { get; set; }
}