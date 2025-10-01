namespace PrecastTracker.Business.Core;

public class BusinessError
{
    public BusinessErrorType Type { get; }
    public string Message { get; }
    public string? Field { get; }
    public string? Code { get; }

    public BusinessError(BusinessErrorType type, string message, string? field = null, string? code = null)
    {
        Type = type;
        Message = message;
        Field = field;
        Code = code;
    }

    public static BusinessError Validation(string message, string? field = null, string? code = null)
        => new(BusinessErrorType.Validation, message, field, code);

    public static BusinessError NotFound(string message = "Resource not found", string? code = null)
        => new(BusinessErrorType.NotFound, message, null, code);

    public static BusinessError Unauthorized(string message = "Unauthorized", string? code = null)
        => new(BusinessErrorType.Unauthorized, message, null, code);

    public static BusinessError Forbidden(string message = "Forbidden", string? code = null)
        => new(BusinessErrorType.Forbidden, message, null, code);

    public static BusinessError Conflict(string message = "Conflict", string? code = null)
        => new(BusinessErrorType.Conflict, message, null, code);

    public static BusinessError InternalError(string message = "Internal server error", string? code = null)
        => new(BusinessErrorType.InternalError, message, null, code);
}
