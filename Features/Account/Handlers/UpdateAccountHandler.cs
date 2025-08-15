using BankAccountsApi.Features.Account.Commands;
using BankAccountsApi.Infrastructure;
using BankAccountsApi.Storage.Interfaces;
using MediatR;

namespace BankAccountsApi.Features.Account.Handlers;

public class UpdateAccountHandler : IRequestHandler<UpdateAccountCommand, MbResult<Unit>>
{
    private readonly IAccountsRepository _storage;

    public UpdateAccountHandler(IAccountsRepository storage)
    {
        _storage = storage;
    }

    public async Task<MbResult<Unit>> Handle(UpdateAccountCommand request, CancellationToken cancellationToken)
    {
        var existingAccount = await _storage.GetByIdAsync(request.Id);
        if (existingAccount == null)
        {
            return MbResult.Failure(MbError.NotFound("Счет не найден"));
        }

        // Обновляем поля
        existingAccount.Currency = request.Currency;
        existingAccount.InterestRate = request.InterestRate;

        await _storage.UpdateAsync(existingAccount);

        return MbResult.Success();
    }
}
