using BankAccountsApi.Features.Account.Enums;

namespace BankAccountsApi.Features.Account.Dto;

/// <summary>
/// DTO для счёта
/// </summary>
public class AccountDto
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public AccountType Type { get; set; }
    public string? Currency { get; set; }
    public decimal Balance { get; set; }
    public decimal? InterestRate { get; set; }
    public DateTime OpenDate { get; set; }
    public DateTime? CloseDate { get; set; }
}