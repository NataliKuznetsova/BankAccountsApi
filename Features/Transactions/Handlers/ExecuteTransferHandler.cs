using BankAccountsApi.Features.Transactions.Commands;
using BankAccountsApi.Features.Transactions.Dto;
using BankAccountsApi.Storage;
using MediatR;

namespace BankAccountsApi.Features.Transactions.Handlers;

public class ExecuteTransferHandler : IRequestHandler<ExecuteTransferCommand, Guid>
{
    public Task<Guid> Handle(ExecuteTransferCommand request, CancellationToken cancellationToken)
    {
        var transaction = new TransactionDto
        {
            Id = Guid.NewGuid(),
            Amount = request.Amount,
            Currency = request.Currency,
            Description = request.Description,
            Date = DateTime.UtcNow
        };
        InMemoryDatabase.Transactions.Add(transaction);
        return Task.FromResult(transaction.Id);
    }
}