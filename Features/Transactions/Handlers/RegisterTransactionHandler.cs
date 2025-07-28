using BankAccountsApi.Features.Transactions.Commands;
using MediatR;

namespace BankAccountsApi.Features.Transactions.Handlers;

public class RegisterTransactionHandler : IRequestHandler<RegisterTransactionCommand, Guid>
{
    public Task<Guid> Handle(RegisterTransactionCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Guid.NewGuid());
    }
}