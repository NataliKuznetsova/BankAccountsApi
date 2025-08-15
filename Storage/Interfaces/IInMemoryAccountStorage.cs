using BankAccountsApi.Models;

namespace BankAccountsApi.Storage.Interfaces;

/// <summary>
/// Хранилище для работы со счетами
/// </summary>
public interface IInMemoryAccountStorage
{
    /// <summary>
    /// Добавить новый счёт
    /// </summary>
    void Create(Account account);

    /// <summary>
    /// Получить счёт по идентификатору
    /// </summary>
    Account? GetById(Guid id);

    /// <summary>
    /// Получить все счета пользователя
    /// </summary>
    List<Account> GetByOwnerId(Guid ownerId);

    /// <summary>
    /// Обновить существующий счёт
    /// </summary>
    void Update(Account account);

    /// <summary>
    /// Удалить счёт по идентификатору
    /// </summary>
    void Delete(Guid id);
}