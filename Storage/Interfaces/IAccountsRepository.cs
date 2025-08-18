using BankAccountsApi.Features.Account.Enums;
using BankAccountsApi.Models;

namespace BankAccountsApi.Storage.Interfaces;

/// <summary>
/// Хранилище для работы со счетами
/// </summary>
public interface IAccountsRepository
{
    /// <summary>
    /// Добавить новый счёт
    /// </summary>
    Task CreateAsync(Account account);

    /// <summary>
    /// Получить счёт по идентификатору
    /// </summary>
    Task<Account?> GetByIdAsync(Guid id);

    /// <summary>
    /// Получить все счета пользователя
    /// </summary>
    Task<List<Account>> GetByOwnerIdAsync(Guid ownerId);

    /// <summary>
    /// Обновить существующий счёт
    /// </summary>
    Task UpdateAsync(Account account);

    /// <summary>
    /// Удалить счёт по идентификатору
    /// </summary>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Начисляет проценты по депозиту
    /// </summary>
    Task AccrueInterestAsync(Guid accountId);
    Task<IEnumerable<Account>> GetByTypesAsync(List<AccountType> types);
}
