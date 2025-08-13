using BankAccountsApi.Models;

namespace BankAccountsApi.Storage.Interfaces;

public interface ICurrencyRepository
{
    /// <summary>
    /// Проверяет, поддерживается ли валюта
    /// </summary>
    Task<bool> ExistsAsync(string currencyCode);
    Task<bool> AddAsync(Currency currency);
}