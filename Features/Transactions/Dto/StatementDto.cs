namespace BankAccountsApi.Features.Transactions.Dto;

/// <summary>
/// Выписка по счету
/// </summary>
public class StatementDto
{
    /// <summary>
    /// Идентификатор счета
    /// </summary>
    public Guid AccountId { get; set; }

    /// <summary>
    /// Список транзакций
    /// </summary>
    public List<TransactionDto> Transactions { get; set; } = [];
    
}