using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PrecastTracker.Business;
using PrecastTracker.Data;
using PrecastTracker.Data.Repositories;
using PrecastTracker.Services;
using PrecastTracker.Tests.Helpers;

namespace PrecastTracker.Tests.Business;

public class ConcreteReportBusinessTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly IConcreteReportBusiness _business;
    private readonly string _csvPath;

    public ConcreteReportBusinessTests()
    {
        // Setup in-memory SQLite database
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        _context = new ApplicationDbContext(options);
        _context.Database.OpenConnection();
        _context.Database.EnsureCreated();

        // Setup business layer dependencies
        var unitOfWork = new UnitOfWork(_context);
        var repository = new TestCylinderRepository(_context);
        var loggerService = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<TestCylinderService>();
        var service = new TestCylinderService(repository, loggerService);
        var loggerAgeCalculator = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<AgeCalculatorService>();
        var ageCalculatorService = new AgeCalculatorService(loggerAgeCalculator);
        var loggerBusiness = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<ConcreteReportBusiness>();
        _business = new ConcreteReportBusiness(service, ageCalculatorService, loggerBusiness);

        // Get CSV path
        _csvPath = Path.Combine(AppContext.BaseDirectory, "Resources", "ConcreteReport.csv");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetConcreteReportAsync_GeneratedCsvMatchesOriginal()
    {
        // Arrange - Seed data from original CSV
        await TestDataSeeder.SeedFromCsvAsync(_context, _csvPath);

        // Act - Generate report using business layer
        var result = await _business.GetConcreteReportAsync();

        // Assert - Business result is successful
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);

        // Generate CSV from business result
        var generatedCsv = CsvGenerator.GenerateCsv(result.Data);

        // Read original CSV
        var originalCsv = await File.ReadAllTextAsync(_csvPath);

        // Normalize line endings for comparison
        var normalizedGenerated = NormalizeLineEndings(generatedCsv);
        var normalizedOriginal = NormalizeLineEndings(originalCsv);

        // Write to files for debugging if test fails
        if (normalizedOriginal != normalizedGenerated)
        {
            await File.WriteAllTextAsync("/tmp/expected.csv", normalizedOriginal);
            await File.WriteAllTextAsync("/tmp/actual.csv", normalizedGenerated);
        }

        // Compare
        Assert.Equal(normalizedOriginal, normalizedGenerated);
    }

    private static string NormalizeLineEndings(string text)
    {
        return text.Replace("\r\n", "\n").Replace("\r", "\n");
    }

    public void Dispose()
    {
        _context.Database.CloseConnection();
        _context.Dispose();
    }
}
