using BankAccountsApi.Features.Clients.Queries;
using BankAccountsApi.Infrastructure.Results;
using BankAccountsApi.Models;
using BankAccountsApi.Storage.Interfaces;
using MediatR;

namespace BankAccountsApi.Features.Clients.Handlers;

public class GetClientByIdHandler(IClientsRepository clientStorage) : IRequestHandler<GetClientByIdQuery, MbResult<Client>>
{
    public async Task<MbResult<Client>> Handle(GetClientByIdQuery request, CancellationToken cancellationToken)
    {
        var client = await clientStorage.GetByIdAsync(request.Id);
        if (client == null)
            return MbResult<Client>.Failure(MbError.NotFound($"Клиент с Id {request.Id} не найден"));

        return MbResult<Client>.Success(client);
    }
}