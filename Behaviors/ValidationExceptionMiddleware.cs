using System.Net;
using System.Text.Json;
using FluentValidation;

namespace BankAccountsApi.Behaviors;

public class ValidationExceptionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            await HandleValidationExceptionAsync(context, ex);
        }
    }

    private static Task HandleValidationExceptionAsync(HttpContext context, ValidationException exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

        var errors = new
        {
            Message = "Ошибка валидации",
            Details = exception.Errors
        };

        var json = JsonSerializer.Serialize(errors);
        return context.Response.WriteAsync(json);
    }
}