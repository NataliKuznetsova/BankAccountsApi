using BankAccountsApi.Infrastructure.Errors;
using MediatR;

namespace BankAccountsApi.Features.Transactions.Commands;

/// <summary>
/// Перевод между счетами
/// </summary>
public class ExecuteTransferCommand : IRequest<MbResult<Unit>>
{
    /// <summary>
    /// Счёт отправителя
    /// </summary>
    public Guid FromAccountId { get; set; }

    /// <summary>
    /// Счёт получателя
    /// </summary>
    public Guid ToAccountId { get; set; }

    /// <summary>
    /// Сумма
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Валюта
    /// </summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Описание
    /// </summary>
    public string? Description { get; set; }
}