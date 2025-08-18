namespace BankAccountsApi.Infrastructure.Results;

/// <summary>
/// Результат выполнения операции
/// </summary>
/// <typeparam name="T">Тип возвращаемого значения</typeparam>
public class MbResult<T>
{
    public T? Value { get; }
    public MbError? Error { get; }
    public bool IsSuccess => Error is null;

    private MbResult(T value)
    {
        Value = value;
    }

    private MbResult(MbError error)
    {
        Error = error;
    }

    public static MbResult<T> Success(T value) => new(value);
    public static MbResult<T> Failure(MbError error) => new(error);
}