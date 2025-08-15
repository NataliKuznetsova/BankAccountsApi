// using Microsoft.AspNetCore.Mvc;
//
// namespace BankAccountsApi.Infrastructure;
//
// public static class MbResultExtensions
// {
//     public static IActionResult ToActionResult<T>(this MbResult<T> result)
//     {
//         if (result.IsSuccess)
//             return new OkObjectResult(result);
//
//         return result.Error!.Code switch
//         {
//             ErrorCodes.NotFound => new NotFoundObjectResult(result),
//             ErrorCodes.ValidationFailed => new BadRequestObjectResult(result),
//             ErrorCodes.Unauthorized => new UnauthorizedObjectResult(result),
//             _ => new ObjectResult(result) { StatusCode = 500 }
//         };
//     }
// }
