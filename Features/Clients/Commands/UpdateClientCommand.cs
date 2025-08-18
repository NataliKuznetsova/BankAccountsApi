using BankAccountsApi.Infrastructure.Results;
using MediatR;

namespace BankAccountsApi.Features.Clients.Commands;

public class UpdateClientCommand: IRequest<MbResult<Unit>>
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// Имя
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Фамилия
    /// </summary>
    public string LastName { get; set; } = string.Empty;
}