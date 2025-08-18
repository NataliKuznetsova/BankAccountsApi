using BankAccountsApi.Infrastructure.Errors;
using MediatR;

namespace BankAccountsApi.Features.Clients.Queries;

/// <summary>
/// Запрос на проверку существования клиента
/// </summary>
public class CheckClientExistsQuery(Guid id) : IRequest<MbResult<bool>>
{
    public Guid Id { get; set; } = id;
}