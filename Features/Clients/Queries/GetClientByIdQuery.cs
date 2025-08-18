using BankAccountsApi.Infrastructure.Results;
using BankAccountsApi.Models;
using MediatR;

namespace BankAccountsApi.Features.Clients.Queries;

/// <summary>
/// Запрос на получение клиента по идентификатору
/// </summary>
public class GetClientByIdQuery(Guid id) : IRequest<MbResult<Client>>
{
    /// <summary>
    /// Идентификатор клиента
    /// </summary>
    public Guid Id { get; } = id;
}