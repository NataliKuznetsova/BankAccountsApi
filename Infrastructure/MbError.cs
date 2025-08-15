using MediatR;

namespace BankAccountsApi.Infrastructure;

/// <summary>
/// Ошибка
/// </summary>
public class MbError(string code, string message, string? field = null)
{
    /// <summary>
    /// Код ошибки
    /// </summary>
    public string Code { get; } = code;

    /// <summary>
    /// Поле с ошибкой
    /// </summary>
    public string? Field { get; set; } = field;

    /// <summary>
    /// Сообщение
    /// </summary>
    public string Message { get; } = message;

    public static MbError Validation(string message, string field) =>
        new(ErrorCodes.ValidationFailed, message, field);

    public static MbError NotFound(string message) =>
        new(ErrorCodes.NotFound, message);

    public static MbError Unauthorized(string message) =>
        new(ErrorCodes.Unauthorized, message);

    public static MbError Internal(string message) =>
        new(ErrorCodes.Internal, message);
    public static MbError Conflict(string message) =>
        new(ErrorCodes.Conflict, message);
}

public static class MbResult
{
    public static MbResult<Unit> Success() => MbResult<Unit>.Success(Unit.Value);

    public static MbResult<Unit> Failure(MbError error) => MbResult<Unit>.Failure(error);

    public static bool IsSuccess<T>(MbResult<T> result) => result.IsSuccess;
}