using System.ComponentModel.DataAnnotations;

namespace BankAccountsApi.Models;

public class Currency
{
    [StringLength(3)]
    public required string Code { get; set; }
    [StringLength(150)]
    public required string Name { get; set; }
}