using BankAccountsApi.Infrastructure.Results;
using MediatR;

namespace BankAccountsApi.Features.Account.Commands;

public class DeleteAccountCommand(Guid id) : IRequest<MbResult<Unit>>
{
    /// <summary>
    /// Идентификатор счета
    /// </summary>
    public Guid Id { get; set; } = id;
}