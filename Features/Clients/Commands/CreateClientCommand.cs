using BankAccountsApi.Infrastructure.Results;
using MediatR;

namespace BankAccountsApi.Features.Clients.Commands;

/// <summary>
/// Команда для создания нового клиента
/// </summary>
public class CreateClientCommand : IRequest<MbResult<Guid>>
{
    /// <summary>
    /// Имя
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Фамилия
    /// </summary>
    public string LastName { get; set; } = string.Empty;
}