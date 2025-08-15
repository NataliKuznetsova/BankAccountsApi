using BankAccountsApi.Features.Clients.Commands;
using BankAccountsApi.Infrastructure;
using BankAccountsApi.Models;
using BankAccountsApi.Storage.Interfaces;
using MediatR;

namespace BankAccountsApi.Features.Clients.Handlers;

public class CreateClientHandler(IInMemoryClientStorage clientStorage) : IRequestHandler<CreateClientCommand, MbResult<Guid>>
{
    public Task<MbResult<Guid>> Handle(CreateClientCommand request, CancellationToken cancellationToken)
    {
        var client = new Client
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            LastName = request.LastName
        };

        clientStorage.Add(client);
        return Task.FromResult(MbResult<Guid>.Success(client.Id));
    }
}