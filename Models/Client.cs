using System.ComponentModel.DataAnnotations;

namespace BankAccountsApi.Models;

public class Client
{
    /// <summary>
    /// Идентификатор клиента
    /// </summary>
    public Guid Id { get; set; }

    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [StringLength(150)]
    public string LastName { get; set; } = string.Empty;
}