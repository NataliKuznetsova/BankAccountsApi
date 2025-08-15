namespace BankAccountsApi.Storage.Interfaces;

public interface IInMemoryCurrencyStorage
{
    /// <summary>
    /// Проверяет, поддерживается ли валюта
    /// </summary>
    bool Exists(string currencyCode);
}