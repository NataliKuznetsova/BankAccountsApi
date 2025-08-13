using BankAccountsApi.Features.Account.Commands;
using BankAccountsApi.Infrastructure;
using BankAccountsApi.Storage.Interfaces;
using MediatR;

namespace BankAccountsApi.Features.Account.Handlers;

public class DeleteAccountHandle(IAccountsRepository storage) : IRequestHandler<DeleteAccountCommand, MbResult<Unit>>
{
    public async Task<MbResult<Unit>> Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
    {
        var account = await storage.GetByIdAsync(request.Id);
        if (account == null)
        {
            return MbResult<Unit>.Failure(
                    MbError.NotFound("Данный счет не найден"));
        }
        await storage.DeleteAsync(request.Id);
        return MbResult.Success();
    }
}