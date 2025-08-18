using BankAccountsApi.Features.Currency.Query;
using BankAccountsApi.Infrastructure;
using BankAccountsApi.Infrastructure.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankAccountsApi.Features.Currency.Controllers;

/// <summary>
/// Контроллер для работы с валютами
/// </summary>
[ApiController]
[Authorize]
[Route("currency")]
public class CurrencyController(IMediator mediator) : MbControllerBase
{
    /// <summary>
    /// Проверить, поддерживается ли валюта по коду ISO 4217
    /// </summary>
    /// <param name="code">Код валюты в формате ISO 4217</param>
    /// <returns>Возвращает true, если валюта поддерживается, иначе false</returns>
    [HttpGet("exists/{code}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<MbResult<bool>> CheckExists(string code)
    {
        return await mediator.Send(new CheckCurrencyExistsQuery(code));
    }
}