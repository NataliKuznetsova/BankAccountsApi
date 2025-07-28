using System.Text.Json.Serialization;

namespace BankAccountsApi.Features.Transactions.Enums;

/// <summary>
/// Тип транзакции
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TransactionType
{
    Credit,
    Debit
}