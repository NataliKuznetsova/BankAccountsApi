using BankAccountsApi.Features.Transactions.Enums;
using BankAccountsApi.Infrastructure.Errors;
using MediatR;

namespace BankAccountsApi.Features.Transactions.Commands;

/// <summary>
/// Регистрация транзакции
/// </summary>
public class RegisterTransactionCommand : IRequest<MbResult<Guid>>
{
    public Guid AccountId { get; set; }

    public decimal Amount { get; set; }

    public string Currency { get; set; } = string.Empty;

    public TransactionType Type { get; set; }

    public string Description { get; set; } = string.Empty;

    public DateTime Date { get; set; }
}