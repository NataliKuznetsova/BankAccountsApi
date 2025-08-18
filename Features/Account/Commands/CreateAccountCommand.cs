using BankAccountsApi.Features.Account.Enums;
using BankAccountsApi.Infrastructure.Results;
using MediatR;

namespace BankAccountsApi.Features.Account.Commands;

/// <summary>
/// Команда для создания нового счета
/// </summary>
public class CreateAccountCommand : IRequest<MbResult<Guid>>
{
    public Guid OwnerId { get; set; }
    
    public AccountType Type { get; set; }
    
    public string Currency { get; set; } = string.Empty;
    
    public decimal? InterestRate { get; set; }
}