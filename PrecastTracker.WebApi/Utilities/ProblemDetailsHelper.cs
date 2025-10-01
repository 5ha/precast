using Microsoft.AspNetCore.Mvc;
using System.Net;
using PrecastTracker.Business.Core;

namespace PrecastTracker.WebApi.Utilities;

public static class ProblemDetailsHelper
{
    public static ProblemDetails CreateBusinessErrorProblemDetails(HttpContext context, IReadOnlyList<BusinessError> errors)
    {
        var primaryError = DeterminePrimaryError(errors);
        var statusCode = MapErrorTypeToStatusCode(primaryError.Type);

        return new ProblemDetails
        {
            Type = GetErrorTypeUri(primaryError.Type),
            Title = GetErrorTitle(primaryError.Type),
            Status = statusCode,
            Instance = context.Request.Path,
            Detail = primaryError.Message
        };
    }

    private static BusinessError DeterminePrimaryError(IReadOnlyList<BusinessError> errors)
    {
        var priorityOrder = new[]
        {
            BusinessErrorType.InternalError,
            BusinessErrorType.Unauthorized,
            BusinessErrorType.Forbidden,
            BusinessErrorType.NotFound,
            BusinessErrorType.Conflict,
            BusinessErrorType.Validation
        };

        foreach (var errorType in priorityOrder)
        {
            var error = errors.FirstOrDefault(e => e.Type == errorType);
            if (error != null)
                return error;
        }

        return errors.First();
    }

    private static int MapErrorTypeToStatusCode(BusinessErrorType errorType) => errorType switch
    {
        BusinessErrorType.Validation => (int)HttpStatusCode.BadRequest,
        BusinessErrorType.NotFound => (int)HttpStatusCode.NotFound,
        BusinessErrorType.Unauthorized => (int)HttpStatusCode.Unauthorized,
        BusinessErrorType.Forbidden => (int)HttpStatusCode.Forbidden,
        BusinessErrorType.Conflict => (int)HttpStatusCode.Conflict,
        BusinessErrorType.InternalError => (int)HttpStatusCode.InternalServerError,
        _ => (int)HttpStatusCode.BadRequest
    };

    private static string GetErrorTypeUri(BusinessErrorType errorType) => errorType switch
    {
        BusinessErrorType.Validation => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
        BusinessErrorType.NotFound => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
        BusinessErrorType.Unauthorized => "https://tools.ietf.org/html/rfc7235#section-3.1",
        BusinessErrorType.Forbidden => "https://tools.ietf.org/html/rfc7231#section-6.5.3",
        BusinessErrorType.Conflict => "https://tools.ietf.org/html/rfc7231#section-6.5.8",
        BusinessErrorType.InternalError => "https://tools.ietf.org/html/rfc7231#section-6.6.1",
        _ => "https://tools.ietf.org/html/rfc7231#section-6.5.1"
    };

    private static string GetErrorTitle(BusinessErrorType errorType) => errorType switch
    {
        BusinessErrorType.Validation => "Validation Error",
        BusinessErrorType.NotFound => "Not Found",
        BusinessErrorType.Unauthorized => "Unauthorized",
        BusinessErrorType.Forbidden => "Forbidden",
        BusinessErrorType.Conflict => "Conflict",
        BusinessErrorType.InternalError => "Internal Server Error",
        _ => "Bad Request"
    };
}
