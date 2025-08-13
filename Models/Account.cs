using System.ComponentModel.DataAnnotations;
using BankAccountsApi.Features.Account.Enums;

namespace BankAccountsApi.Models;

/// <summary>
/// Модель счёта
/// </summary>
public class Account
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public AccountType Type { get; set; }
    
    [StringLength(150)]
    public required string Currency { get; set; }
    public decimal Balance { get; set; }
    public decimal? InterestRate { get; set; }
    public DateTime OpenDate { get; set; }
    public DateTime? CloseDate { get; set; }
}