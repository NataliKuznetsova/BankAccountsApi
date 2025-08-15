namespace BankAccountsApi.Models;

public class Client
{
    /// <summary>
    /// Идентификатор клиента
    /// </summary>
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;
}