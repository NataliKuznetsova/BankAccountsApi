using BankAccountsApi.Features.Clients.Commands;
using BankAccountsApi.Infrastructure;
using BankAccountsApi.Models;
using BankAccountsApi.Storage.Interfaces;
using MediatR;

namespace BankAccountsApi.Features.Clients.Handlers;

public class UpdateClientCommandHandler(IInMemoryClientStorage storage) : IRequestHandler<UpdateClientCommand, MbResult<Unit>>
{
    public Task<MbResult<Unit>> Handle(UpdateClientCommand request, CancellationToken cancellationToken)
    {
        var existingClient = storage.Get(request.Id);
        if (existingClient == null)
            return Task.FromResult(MbResult.Failure(MbError.NotFound($"Клиент с Id {request.Id} не найден")));

        var updatedClient = new Client
        {
            Id = request.Id,
            Name = request.Name,
            LastName = request.LastName
        };

        storage.Update(updatedClient);

        return Task.FromResult(MbResult.Success());
    }
}