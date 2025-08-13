using BankAccountsApi.Features.Clients.Commands;
using BankAccountsApi.Infrastructure;
using BankAccountsApi.Models;
using BankAccountsApi.Storage.Interfaces;
using MediatR;

namespace BankAccountsApi.Features.Clients.Handlers;

public class UpdateClientCommandHandler(IClientsRepository storage) : IRequestHandler<UpdateClientCommand, MbResult<Unit>>
{
    public async Task<MbResult<Unit>> Handle(UpdateClientCommand request, CancellationToken cancellationToken)
    {
        var existingClient = await storage.GetByIdAsync(request.Id);
        if (existingClient == null)
            return MbResult.Failure(MbError.NotFound($"Клиент с Id {request.Id} не найден"));

        var updatedClient = new Client
        {
            Id = request.Id,
            Name = request.Name,
            LastName = request.LastName
        };

        await storage.UpdateAsync(updatedClient);

        return MbResult.Success();
    }
}