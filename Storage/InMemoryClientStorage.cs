using BankAccountsApi.Models;
using BankAccountsApi.Storage.Interfaces;

namespace BankAccountsApi.Storage;

public class InMemoryClientStorage : IInMemoryClientStorage
{
    private readonly Dictionary<Guid, Client> _clients = new();

    public void Add(Client client)
    {
        _clients[client.Id] = client;
    }

    public bool Exists(Guid clientId)
    {
        return _clients.ContainsKey(clientId);
    }

    public Client? Get(Guid clientId)
    {
        _clients.TryGetValue(clientId, out var client);
        return client;
    }

    public void Update(Client client)
    {
        _clients[client.Id] = client;
    }
}