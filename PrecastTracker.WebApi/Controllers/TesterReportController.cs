using Microsoft.AspNetCore.Mvc;
using PrecastTracker.Business;

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
    public async Task<IActionResult> GetTestsDueToday()
    {
        var result = await _business.GetTestsDueTodayAsync();
        return HandleBusinessResult(result);
    }

    [HttpGet("tests-overdue")]
    public async Task<IActionResult> GetTestsOverdue()
    {
        var result = await _business.GetTestsOverdueAsync();
        return HandleBusinessResult(result);
    }

    [HttpGet("tests-upcoming")]
    public async Task<IActionResult> GetTestsUpcoming([FromQuery] int days = 7)
    {
        var result = await _business.GetTestsUpcomingAsync(days);
        return HandleBusinessResult(result);
    }
}
