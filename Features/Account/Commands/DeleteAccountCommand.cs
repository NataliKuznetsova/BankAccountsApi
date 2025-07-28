using MediatR;

namespace BankAccountsApi.Features.Account.Commands;

public class DeleteAccountCommand(Guid id) : IRequest
{
    /// <summary>
    /// Идентификатор счета
    /// </summary>
    public Guid Id { get; set; } = id;
}