using BankAccountsApi.Infrastructure;
using MediatR;

namespace BankAccountsApi.Features.Account.Queries;

/// <summary>
/// Проверка наличия счета по owner id
/// </summary>
public class HasAccountByOwnerIdQuery(Guid ownerId) : IRequest<MbResult<bool>>
{
    public Guid OwnerId { get; set; } = ownerId;
}