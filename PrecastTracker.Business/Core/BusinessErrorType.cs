namespace PrecastTracker.Business.Core;

public enum BusinessErrorType
{
    Validation,      // 400 Bad Request
    NotFound,        // 404 Not Found
    Unauthorized,    // 401 Unauthorized
    Forbidden,       // 403 Forbidden
    Conflict,        // 409 Conflict
    InternalError    // 500 Internal Server Error
}
