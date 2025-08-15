using BankAccountsApi.Models;
using BankAccountsApi.Storage.Interfaces;

namespace BankAccountsApi.Storage;

public class InMemoryCurrencyStorage : IInMemoryCurrencyStorage
{
    private readonly List<Currency> _currencies = [];

    public bool Exists(string currencyCode)
    {
        return _currencies.Any(c => c.Code.Equals(currencyCode, StringComparison.OrdinalIgnoreCase));
    }
    
    public void Add(Currency currencyController)
    {
        if (!Exists(currencyController.Code))
            _currencies.Add(currencyController);
    }
}