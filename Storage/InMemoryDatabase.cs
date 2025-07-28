using BankAccountsApi.Features.Account.Dto;
using BankAccountsApi.Features.Transactions.Dto;

namespace BankAccountsApi.Storage;

public class InMemoryDatabase
{
    public static List<AccountDto> Accounts { get; set; } = [];
    public static List<TransactionDto> Transactions { get; set;} = [];
}