using BankAccountsApi.Models;

namespace BankAccountsApi.Storage.Interfaces;

public interface IInMemoryTransactionStorage
{
    /// <summary>
    /// Получить все транзакции по счёту
    /// </summary>
    List<Transaction> GetByAccountId(Guid accountId);

    /// <summary>
    /// Добавить новую транзакцию
    /// </summary>
    void Add(Transaction transaction);
}