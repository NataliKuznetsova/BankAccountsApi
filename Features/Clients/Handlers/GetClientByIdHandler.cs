using BankAccountsApi.Features.Clients.Queries;
using BankAccountsApi.Infrastructure;
using BankAccountsApi.Models;
using BankAccountsApi.Storage.Interfaces;
using MediatR;

namespace BankAccountsApi.Features.Clients.Handlers;

public class GetClientByIdHandler(IInMemoryClientStorage clientStorage) : IRequestHandler<GetClientByIdQuery, MbResult<Client>>
{
    public Task<MbResult<Client>> Handle(GetClientByIdQuery request, CancellationToken cancellationToken)
    {
        var client = clientStorage.Get(request.Id);
        if (client == null)
            return Task.FromResult(
                MbResult<Client>.Failure(MbError.NotFound($"Клиент с Id {request.Id} не найден"))
            );

        return Task.FromResult(MbResult<Client>.Success(client));
    }
}