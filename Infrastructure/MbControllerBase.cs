using BankAccountsApi.Infrastructure.Errors;
using Microsoft.AspNetCore.Mvc;

namespace BankAccountsApi.Infrastructure;

[ApiController]
public abstract class MbControllerBase : ControllerBase
{
    protected IActionResult HandleResult<T>(MbResult<T> result, int successStatusCode = StatusCodes.Status200OK)
    {
        if (!result.IsSuccess)
            return result.Error?.Code switch
            {
                ErrorCodes.ValidationFailed => BadRequest(result.Error),
                ErrorCodes.NotFound => NotFound(result.Error),
                ErrorCodes.Unauthorized => Unauthorized(result.Error),
                ErrorCodes.Conflict => Conflict(result.Error),
                ErrorCodes.Internal => StatusCode(StatusCodes.Status500InternalServerError, result.Error),
                _ => BadRequest(result.Error)
            };
        
        if (result.Value is null && successStatusCode == StatusCodes.Status200OK)
            return NoContent();

        return StatusCode(successStatusCode, result.Value);

    }
}