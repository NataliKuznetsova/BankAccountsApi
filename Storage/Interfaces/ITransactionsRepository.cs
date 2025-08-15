using BankAccountsApi.Models;

namespace BankAccountsApi.Storage.Interfaces;

public interface ITransactionsRepository
{
    /// <summary>
    /// Получить все транзакции по счёту
    /// </summary>
    Task<List<Transaction>> GetByAccountIdAsync(Guid accountId);

    /// <summary>
    /// Добавить новую транзакцию
    /// </summary>
    Task AddAsync(Transaction transaction);
}