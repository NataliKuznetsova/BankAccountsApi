using BankAccountsApi.Features.Account.Commands;
using BankAccountsApi.Infrastructure;
using BankAccountsApi.Storage.Interfaces;
using MediatR;

namespace BankAccountsApi.Features.Account.Handlers;

public class DeleteAccountHandle(IInMemoryAccountStorage storage) : IRequestHandler<DeleteAccountCommand, MbResult<Unit>>
{
    public Task<MbResult<Unit>> Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
    {
        var account = storage.GetById(request.Id);
        if (account == null)
        {
            return Task.FromResult(
                MbResult<Unit>.Failure(
                    MbError.NotFound("Данный счет не найден")
                ));
        }
        storage.Delete(request.Id);
        return Task.FromResult(MbResult.Success());
    }
}