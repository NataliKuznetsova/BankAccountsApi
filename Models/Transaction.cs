using BankAccountsApi.Features.Transactions.Enums;

namespace BankAccountsApi.Models;

public class Transaction
{
    public Guid Id { get; set; }
    
    public Guid AccountId { get; set; }

    public decimal Amount { get; set; }

    public string Currency { get; set; } = string.Empty;

    public TransactionType Type { get; set; }

    public string? Description { get; set; }

    public DateTime Date { get; set; }
}