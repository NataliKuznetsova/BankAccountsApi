using BankAccountsApi.Features.Account.Commands;
using BankAccountsApi.Infrastructure.Errors;
using BankAccountsApi.Infrastructure.Results;
using BankAccountsApi.Storage.Interfaces;
using MediatR;

namespace BankAccountsApi.Features.Account.Handlers;

public class UpdateAccountHandler(IAccountsRepository storage) : IRequestHandler<UpdateAccountCommand, MbResult<Unit>>
{
    public async Task<MbResult<Unit>> Handle(UpdateAccountCommand request, CancellationToken cancellationToken)
    {
        var existingAccount = await storage.GetByIdAsync(request.Id);
        if (existingAccount == null)
        {
            return MbResult.Failure<Unit>(MbError.NotFound("Счет не найден"));
        }

        // Обновляем поля
        existingAccount.Currency = request.Currency;
        existingAccount.InterestRate = request.InterestRate;

        await storage.UpdateAsync(existingAccount);

        return MbResult.Success();
    }
}
