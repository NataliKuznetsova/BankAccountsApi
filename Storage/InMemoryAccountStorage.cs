using System.Collections.Concurrent;
using BankAccountsApi.Models;
using BankAccountsApi.Storage.Interfaces;

namespace BankAccountsApi.Storage;

public class InMemoryAccountStorage : IInMemoryAccountStorage
{
    private readonly ConcurrentDictionary<Guid, Account> _storage = new();

    public void Create(Account account)
    {
        _storage[account.Id] = account;
    }

    public Account? GetById(Guid id)
    {
        _storage.TryGetValue(id, out var account);
        return account;
    }

    public List<Account> GetByOwnerId(Guid ownerId)
    {
        return _storage.Values.Where(a => a.OwnerId == ownerId).ToList();
    }

    public void Update(Account account)
    {
        _storage[account.Id] = account;
    }

    public void Delete(Guid id)
    {
        _storage.TryRemove(id, out _);
    }
}