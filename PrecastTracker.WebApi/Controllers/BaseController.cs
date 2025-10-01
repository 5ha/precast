using Microsoft.AspNetCore.Mvc;
using PrecastTracker.Business.Core;
using PrecastTracker.WebApi.Utilities;

namespace PrecastTracker.WebApi.Controllers;

[ApiController]
public abstract class BaseController<T> : ControllerBase where T : BaseController<T>
{
    protected readonly ILogger<T> _logger;

    protected BaseController(ILogger<T> logger)
    {
        _logger = logger;
    }

    protected IActionResult HandleBusinessResult<TData>(BusinessResult<TData> result)
    {
        if (result.Succeeded)
        {
            return Ok(result.Data);
        }

        return CreateErrorResponse(result.Errors);
    }

    protected IActionResult HandleBusinessResult(BusinessResult result)
    {
        if (result.Succeeded)
        {
            return NoContent();
        }

        return CreateErrorResponse(result.Errors);
    }

    protected IActionResult CreateErrorResponse(IReadOnlyList<BusinessError> errors)
    {
        var problemDetails = ProblemDetailsHelper.CreateBusinessErrorProblemDetails(HttpContext, errors);
        return StatusCode(problemDetails.Status!.Value, problemDetails);
    }
}
