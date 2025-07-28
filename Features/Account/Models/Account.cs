using BankAccountsApi.Features.Account.Enums;
using Transaction = BankAccountsApi.Features.Transactions.Models.Transaction;

namespace BankAccountsApi.Features.Account.Models;

/// <summary>
/// Модель счёта
/// </summary>
public class Account
{
    public Guid Id { get; set; }
    
    public Guid OwnerId { get; set; }
    
    public AccountType Type { get; set; }
    
    public string? Currency { get; set; }
    
    public decimal Balance { get; set; }
    
    public decimal? InterestRate { get; set; }
    
    public DateTime OpenDate { get; set; }
    
    public DateTime? CloseDate { get; set; }

    public List<Transaction> Transactions { get; set; } = [];
}