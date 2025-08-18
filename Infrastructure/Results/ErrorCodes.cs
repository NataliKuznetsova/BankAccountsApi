namespace BankAccountsApi.Infrastructure.Results;

public static class ErrorCodes
{
    public const string NotFound = "not_found";
    public const string ValidationFailed = "validationfailed";
    public const string Unauthorized = "unauthorized";
    public const string Internal = "internal_error";
    public const string Conflict = "conflict";
}