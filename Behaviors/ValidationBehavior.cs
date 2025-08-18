using BankAccountsApi.Infrastructure.Results;
using FluentValidation;
using MediatR;

namespace BankAccountsApi.Behaviors;

public class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);

        var errors = (await Task.WhenAll(
                validators.Select(x => x.ValidateAsync(context, cancellationToken))))
            .SelectMany(x => x.Errors)
            .Where(x => x != null)
            .GroupBy(x => x.PropertyName)
            .Select(x => x.First()) // Только первая ошибка на поле
            .ToList();

        if (errors.Count == 0)
            return await next();

        var message = string.Join("; ",
            errors.Select(x => $"{x.PropertyName}: {x.ErrorMessage}"));

        var error = new MbError(ErrorCodes.ValidationFailed, message);

        if (typeof(TResponse).IsGenericType &&
            typeof(TResponse).GetGenericTypeDefinition() == typeof(MbResult<>))
        {
            var resultType = typeof(TResponse).GetGenericArguments()[0];
            var method = typeof(MbResult<>)
                .MakeGenericType(resultType)
                .GetMethod(nameof(MbResult<object>.Failure),
                    [typeof(string), typeof(string)]);

            if (method != null)
                return (TResponse)method.Invoke(null, [error.Code, error.Message])!;
        }

        throw new ValidationException(errors);
    }
}