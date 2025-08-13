using BankAccountsApi.Models;

namespace BankAccountsApi.Storage.Interfaces;

/// <summary>
/// Хранилище для работы с клиентами
/// </summary>
public interface IClientsRepository
{
    /// <summary>
    /// Добавить клиента
    /// </summary>
    Task AddAsync(Client client);

    /// <summary>
    /// Проверить существование клиента по идентификатору
    /// </summary>
    Task<bool> ExistsAsync(Guid clientId);

    /// <summary>
    /// Получить клиента по идентификатору
    /// </summary>
    Task<Client?> GetByIdAsync(Guid clientId);

    /// <summary>
    /// Обновить информацию о клиенте
    /// </summary>
    Task UpdateAsync(Client client);
}
