using BankAccountsApi.Features.Transactions.Enums;

namespace BankAccountsApi.Features.Transactions.Dto;

public class TransactionDto
{
    public Guid Id { get; set; }

    public decimal Amount { get; set; }

    public string Currency { get; set; } = string.Empty;

    public TransactionType Type { get; set; }

    public string? Description { get; set; }

    public DateTime Date { get; set; }
}