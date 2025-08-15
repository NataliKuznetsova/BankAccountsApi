using BankAccountsApi.Models;
using BankAccountsApi.Storage.Interfaces;

namespace BankAccountsApi.Storage;

public class InMemoryTransactionStorage : IInMemoryTransactionStorage
{
    private readonly List<Transaction> _transactions = [];

    public List<Transaction> GetByAccountId(Guid accountId)
    {
        return _transactions.Where(t => t.AccountId == accountId).ToList();
    }

    public void Add(Transaction transaction)
    {
        _transactions.Add(transaction);
    }
}