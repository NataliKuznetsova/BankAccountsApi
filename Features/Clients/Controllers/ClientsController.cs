using BankAccountsApi.Features.Clients.Commands;
using BankAccountsApi.Features.Clients.Queries;
using BankAccountsApi.Infrastructure;
using BankAccountsApi.Infrastructure.Results;
using BankAccountsApi.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankAccountsApi.Features.Clients.Controllers;

/// <summary>
/// Контроллер для работы с клиентами
/// </summary>
[ApiController]
[Route("clients")]
[Authorize]
public class ClientsController(IMediator mediator) : MbControllerBase
{
    /// <summary>
    /// Проверить, существует ли клиент с указанным Id
    /// </summary>
    /// <param name="id">Идентификатор клиента</param>
    /// <returns>True, если клиент существует, иначе False</returns>
    [HttpGet("{id:guid}/exists")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<MbResult<bool>> CheckClientExists(Guid id)
    {
        return await mediator.Send(new CheckClientExistsQuery(id));
    }

    /// <summary>
    /// Создать нового клиента
    /// </summary>
    /// <param name="command">Данные для создания клиента</param>
    /// <returns>Идентификатор нового клиента</returns>
    [HttpPost]
    [ProducesResponseType(typeof(MbResult<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MbResult<Guid>), StatusCodes.Status400BadRequest)]
    public async Task<MbResult<Guid>> CreateClient([FromBody] CreateClientCommand command)
    {
        return await mediator.Send(command);
    }

    /// <summary>
    /// Получить клиента по Id
    /// </summary>
    /// <param name="id">Идентификатор клиента</param>
    /// <returns>Данные клиента</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(MbResult<Client>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MbResult<Client>), StatusCodes.Status404NotFound)]
    public async Task<MbResult<Client>> GetClientById(Guid id)
    {
        return await mediator.Send(new GetClientByIdQuery(id));
    }

    /// <summary>
    /// Обновить клиента
    /// </summary>
    /// <param name="id">Идентификатор клиента</param>
    /// <param name="command">Данные для обновления</param>
    /// <returns>204 NoContent или ошибка</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(MbError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(MbError), StatusCodes.Status404NotFound)]
    public async Task<MbResult<Unit>> UpdateClient(Guid id, [FromBody] UpdateClientCommand command)
    {
        command.Id = id;
        return await mediator.Send(command);
    }
}
