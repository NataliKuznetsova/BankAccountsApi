using BankAccountsApi.Features.Account.Commands;
using BankAccountsApi.Infrastructure;
using BankAccountsApi.Storage.Interfaces;
using MediatR;

namespace BankAccountsApi.Features.Account.Handlers;

/// <summary>
/// Хэндлер для создания счета
/// </summary>
public class CreateAccountHandler(IInMemoryAccountStorage storage, IInMemoryClientStorage clientStorage)
    : IRequestHandler<CreateAccountCommand, MbResult<Guid>>
{
    public Task<MbResult<Guid>> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        if (!clientStorage.Exists(request.OwnerId))
        {
            return Task.FromResult(MbResult<Guid>.Failure(MbError.NotFound("Клиент не найден")));
        }

        var newAccount = new BankAccountsApi.Models.Account()
        {
            Id = Guid.NewGuid(),
            OwnerId = request.OwnerId,
            Type = request.Type,
            Currency = request.Currency,
            InterestRate = request.InterestRate,
            Balance = 0,
            OpenDate = DateTime.UtcNow
        };

        storage.Create(newAccount);
        return Task.FromResult(MbResult<Guid>.Success(newAccount.Id));
    }
}