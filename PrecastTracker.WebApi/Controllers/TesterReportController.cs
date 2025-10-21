using Microsoft.AspNetCore.Mvc;
using PrecastTracker.Business;
using PrecastTracker.Contracts.DTOs.RequestResponse;

namespace PrecastTracker.WebApi.Controllers;

[Route("api/tester-report")]
public class TesterReportController : BaseController<TesterReportController>
{
    private readonly ITesterReportBusiness _business;

    public TesterReportController(
        ITesterReportBusiness business,
        ILogger<TesterReportController> logger) : base(logger)
    {
        _business = business;
    }

    [HttpGet("tests-due-today")]
    [ProducesResponseType(typeof(IEnumerable<TestCylinderQueueResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTestsDueToday()
    {
        var result = await _business.GetTestsDueTodayAsync();
        return HandleBusinessResult(result);
    }

    [HttpGet("tests-overdue")]
    [ProducesResponseType(typeof(IEnumerable<TestCylinderQueueResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTestsOverdue()
    {
        var result = await _business.GetTestsOverdueAsync();
        return HandleBusinessResult(result);
    }

    [HttpGet("tests-upcoming")]
    [ProducesResponseType(typeof(IEnumerable<TestCylinderQueueResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTestsUpcoming([FromQuery] int days = 7)
    {
        var result = await _business.GetTestsUpcomingAsync(days);
        return HandleBusinessResult(result);
    }

    [HttpGet("untested-placements")]
    [ProducesResponseType(typeof(IEnumerable<UntestedPlacementResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUntestedPlacements([FromQuery] int daysBack = 7)
    {
        var result = await _business.GetUntestedPlacementsAsync(daysBack);
        return HandleBusinessResult(result);
    }

    [HttpGet("test-set-day/{testSetDayId}")]
    [ProducesResponseType(typeof(GetTestSetDayDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTestSetDayDetails(int testSetDayId)
    {
        var result = await _business.GetTestSetDayDetailsAsync(testSetDayId);
        return HandleBusinessResult(result);
    }

    [HttpPost("test-set-day")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SaveTestSetDayData([FromBody] SaveTestSetDayDataRequest request)
    {
        var result = await _business.SaveTestSetDayDataAsync(request);
        return HandleBusinessResult(result);
    }
}
