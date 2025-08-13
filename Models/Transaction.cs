using System.ComponentModel.DataAnnotations;
using BankAccountsApi.Features.Transactions.Enums;

namespace BankAccountsApi.Models;

public class Transaction
{
    public Guid Id { get; set; }
    
    public Guid AccountId { get; set; }

    public decimal Amount { get; set; }

    [StringLength(3)]
    public required string Currency { get; set; }

    public TransactionType Type { get; set; }

    [StringLength(150)]
    public string? Description { get; set; }

    public DateTime Date { get; set; }
}