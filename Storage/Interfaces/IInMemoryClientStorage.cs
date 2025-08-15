using BankAccountsApi.Models;

namespace BankAccountsApi.Storage.Interfaces;

/// <summary>
/// Интерфейс для хранилища клиентов
/// </summary>
public interface IInMemoryClientStorage
{
    /// <summary>
    /// Добавить клиента
    /// </summary>
    void Add(Client client);

    /// <summary>
    /// Проверить существование клиента по идентификатору
    /// </summary>
    bool Exists(Guid clientId);

    /// <summary>
    /// Получить клиента по идентификатору
    /// </summary>
    Client? Get(Guid clientId);
    
    /// <summary>
    /// Обновление инфы клиента
    /// </summary>
    /// <param name="client"></param>
    void Update(Client client);
}