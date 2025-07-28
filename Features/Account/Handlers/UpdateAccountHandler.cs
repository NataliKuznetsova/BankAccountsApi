using BankAccountsApi.Features.Account.Commands;
using MediatR;

namespace BankAccountsApi.Features.Account.Handlers;

public class UpdateAccountHandler : IRequestHandler<UpdateAccountCommand>
{
    public Task Handle(UpdateAccountCommand request, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}