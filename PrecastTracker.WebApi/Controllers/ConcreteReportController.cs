using System.Text;
using Microsoft.AspNetCore.Mvc;
using PrecastTracker.Business;

namespace PrecastTracker.WebApi.Controllers;

[Route("api/[controller]")]
public class ConcreteReportController : BaseController<ConcreteReportController>
{
    private readonly IConcreteReportBusiness _business;

    public ConcreteReportController(IConcreteReportBusiness business, ILogger<ConcreteReportController> logger) : base(logger)
    {
        _business = business;
    }

    [HttpGet("csv")]
    public async Task<IActionResult> GetCsvReport()
    {
        var result = await _business.GetConcreteReportAsync();

        if (!result.Succeeded)
        {
            return CreateErrorResponse(result.Errors);
        }

        var csv = GenerateCsv(result.Data!);
        var bytes = Encoding.UTF8.GetBytes(csv);

        return File(bytes, "text/csv", "ConcreteReport.csv");
    }

    [HttpGet]
    public async Task<IActionResult> GetReport()
    {
        var result = await _business.GetConcreteReportAsync();
        return HandleBusinessResult(result);
    }

    private static string GenerateCsv(IEnumerable<Contracts.DTOs.RequestResponse.ConcreteReportResponse> data)
    {
        var sb = new StringBuilder();

        // Add BOM for proper Excel UTF-8 handling
        sb.Append('\uFEFF');

        // Header
        sb.AppendLine("Test ID,Cylinder ID,Casting Date,Mix Design,Yards/Bed,Bed ID,Batching Start Time,Job ID,Job Name,Truck No.,Pour ID,Piece Type,Oven ID,Age of Test,Testing Date,Required (PSI),Break #1,Break #2,Break #3,Average PSI,Comments");

        // Data rows
        foreach (var row in data)
        {
            sb.AppendLine($"{CsvEscape(row.TestId)},{CsvEscape(row.CylinderId)},{CsvEscape(row.CastingDate)},{CsvEscape(row.MixDesign)},{CsvEscape(row.YardsPerBed)},{CsvEscape(row.BedId)},{CsvEscape(row.BatchingStartTime)},{CsvEscape(row.JobId)},{CsvEscape(row.JobName)},{CsvEscape(row.TruckNo)},{CsvEscape(row.PourId)},{CsvEscape(row.PieceType)},{CsvEscape(row.OvenId)},{CsvEscape(row.AgeOfTest)},{CsvEscape(row.TestingDate)},{CsvEscape(row.Required)},{CsvEscape(row.Break1)},{CsvEscape(row.Break2)},{CsvEscape(row.Break3)},{CsvEscape(row.AveragePsi)},{CsvEscape(row.Comments)}");
        }

        return sb.ToString();
    }

    private static string CsvEscape(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }
}
