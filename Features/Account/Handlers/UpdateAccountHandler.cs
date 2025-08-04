using BankAccountsApi.Features.Account.Commands;
using BankAccountsApi.Infrastructure;
using BankAccountsApi.Storage.Interfaces;
using MediatR;

namespace BankAccountsApi.Features.Account.Handlers;

public class UpdateAccountHandler(IInMemoryAccountStorage storage) : IRequestHandler<UpdateAccountCommand, MbResult<Unit>>
{
    public Task<MbResult<Unit>> Handle(UpdateAccountCommand request, CancellationToken cancellationToken)
    {
        var existingAccount = storage.GetById(request.Id);
        if (existingAccount == null)
        {
            return Task.FromResult(
                MbResult.Failure(MbError.NotFound("Счет не найден"))
            );
        }

        // Обновляем поля
        existingAccount.Currency = request.Currency;
        existingAccount.InterestRate = request.InterestRate;
        storage.Update(existingAccount);

        return Task.FromResult(MbResult.Success());
    }
}