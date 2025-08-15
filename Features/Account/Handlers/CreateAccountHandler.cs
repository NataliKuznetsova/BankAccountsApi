using BankAccountsApi.Features.Account.Commands;
using BankAccountsApi.Infrastructure;
using BankAccountsApi.Storage.Interfaces;
using MediatR;

namespace BankAccountsApi.Features.Account.Handlers;

/// <summary>
/// Хэндлер для создания счета
/// </summary>
public class CreateAccountHandler(IAccountsRepository storage, IClientsRepository clientStorage)
    : IRequestHandler<CreateAccountCommand, MbResult<Guid>>
{
    public async Task<MbResult<Guid>> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        if (!await clientStorage.ExistsAsync(request.OwnerId))
        {
            return MbResult<Guid>.Failure(MbError.NotFound("Клиент не найден"));
        }

        var newAccount = new Models.Account()
        {
            Id = Guid.NewGuid(),
            OwnerId = request.OwnerId,
            Type = request.Type,
            Currency = request.Currency,
            InterestRate = request.InterestRate,
            Balance = 0,
            OpenDate = DateTime.UtcNow
        };

        await storage.CreateAsync(newAccount);
        return MbResult<Guid>.Success(newAccount.Id);
    }
}