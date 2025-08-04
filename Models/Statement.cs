namespace BankAccountsApi.Models;

/// <summary>
/// Выписка по счету
/// </summary>
public class Statement
{
    /// <summary>
    /// Идентификатор счета
    /// </summary>
    public Guid AccountId { get; set; }

    /// <summary>
    /// Список транзакций
    /// </summary>
    public List<Transaction> Transactions { get; set; } = [];
    
}