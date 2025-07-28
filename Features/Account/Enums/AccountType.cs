using System.Text.Json.Serialization;

namespace BankAccountsApi.Features.Account.Enums;

/// <summary>
/// Тип счёта
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AccountType
{
    Deposit = 1,
    Checking = 2,
    Credit = 3
}