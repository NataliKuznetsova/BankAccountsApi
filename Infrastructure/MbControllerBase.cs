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
                "validation_error" => BadRequest(result.Error),
                "not_found" => NotFound(result.Error),
                "unauthorized" => Unauthorized(result.Error),
                "internal_error" => StatusCode(StatusCodes.Status500InternalServerError, result.Error),
                _ => BadRequest(result.Error)
            };
        
        if (result.Value is null && successStatusCode == StatusCodes.Status200OK)
            return NoContent();

        return StatusCode(successStatusCode, result.Value);

    }
}