using BankAccountsApi.Features.Account.Commands;
using MediatR;

namespace BankAccountsApi.Features.Account.Handlers;

public class DeleteAccountHandler : IRequestHandler<DeleteAccountCommand>
{
    public Task Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
    {
        // Тут будет логика удаления
        return Task.CompletedTask;
    }
}