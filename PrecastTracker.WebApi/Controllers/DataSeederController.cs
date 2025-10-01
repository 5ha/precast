using Microsoft.AspNetCore.Mvc;
using PrecastTracker.Services;

namespace PrecastTracker.WebApi.Controllers;

[Route("api/[controller]")]
public class DataSeederController : BaseController<DataSeederController>
{
    private readonly IDataSeederService _seederService;

    public DataSeederController(IDataSeederService seederService, ILogger<DataSeederController> logger) : base(logger)
    {
        _seederService = seederService;
    }

    [HttpPost("seed")]
    public async Task<IActionResult> SeedData()
    {
        try
        {
            var csvPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "Specs", "original", "ConcreteReport.csv");

            if (!System.IO.File.Exists(csvPath))
            {
                return BadRequest($"CSV file not found at: {csvPath}");
            }

            await _seederService.SeedFromCsvAsync(csvPath);
            return Ok("Data seeded successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding data");
            return StatusCode(500, $"Error seeding data: {ex.Message}");
        }
    }
}
