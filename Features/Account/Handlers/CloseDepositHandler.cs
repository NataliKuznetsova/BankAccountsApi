using BankAccountsApi.Features.Account.Commands;
using BankAccountsApi.Features.Account.Enums;
using BankAccountsApi.Infrastructure.Results;
using BankAccountsApi.Storage.Interfaces;
using MediatR;

namespace BankAccountsApi.Features.Account.Handlers;

/// <summary>
/// Хэндлер для закрытия депозита с начислением процентов
/// </summary>
public class CloseDepositHandler(IAccountsRepository storage) : IRequestHandler<CloseDepositCommand, MbResult<Unit>>
{
    public async Task<MbResult<Unit>> Handle(CloseDepositCommand request, CancellationToken cancellationToken)
    {
        var deposit = await storage.GetByIdAsync(request.DepositId);
        if (deposit == null || deposit.Type != AccountType.Deposit)
        {
            return MbResult<Unit>.Failure(MbError.NotFound("Депозит не найден"));
        }

        await storage.AccrueInterestAsync(deposit.Id);

        deposit.CloseDate = DateTime.UtcNow;
        await storage.UpdateAsync(deposit);

        return MbResult.Success();
    }
}
