using Microsoft.Extensions.Logging;
using PrecastTracker.Business.Core;
using PrecastTracker.Contracts.DTOs.RequestResponse;
using PrecastTracker.Services;

namespace PrecastTracker.Business;

public class ConcreteReportBusiness : BaseBusiness<ConcreteReportBusiness>, IConcreteReportBusiness
{
    private readonly IConcreteTestService _service;

    public ConcreteReportBusiness(IConcreteTestService service, ILogger<ConcreteReportBusiness> logger) : base(logger)
    {
        _service = service;
    }

    public async Task<BusinessResult<IEnumerable<ConcreteReportResponse>>> GetConcreteReportAsync()
    {
        try
        {
            _logger.LogInformation("Generating concrete report");
            var tests = await _service.GetAllTestsWithRelatedDataAsync();

            var reportData = tests.Select(MapToReportResponse);

            return BusinessResult<IEnumerable<ConcreteReportResponse>>.Success(reportData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating concrete report");
            return BusinessResult<IEnumerable<ConcreteReportResponse>>.Failure(
                BusinessError.InternalError("Failed to generate concrete report"));
        }
    }

    private static ConcreteReportResponse MapToReportResponse(Data.Entities.ConcreteTest test)
    {
        return new ConcreteReportResponse
        {
            TestId = test.TestCode,
            CylinderId = test.CylinderId,
            CastingDate = test.Placement.Pour.CastingDate.ToString("MM/dd/yyyy"),
            MixDesign = test.Placement.MixDesign.Code,
            YardsPerBed = test.Placement.YardsPerBed.ToString("0.##"),
            BedId = test.Placement.Pour.Bed.Code,
            BatchingStartTime = test.Placement.BatchingStartTime?.ToString(@"h\:mm") ?? string.Empty,
            JobId = test.Placement.Pour.Job.Code,
            JobName = test.Placement.Pour.Job.Name,
            TruckNo = test.Placement.TruckNumbers ?? string.Empty,
            PourId = test.Placement.Pour.Code,
            PieceType = test.Placement.PieceType ?? string.Empty,
            OvenId = test.Placement.OvenId ?? string.Empty,
            AgeOfTest = CalculateAgeOfTest(test.Placement.Pour.CastingDate, test.Placement.BatchingStartTime, test.TestingDate),
            TestingDate = FormatTestingDate(test.TestingDate),
            Required = $"{test.RequiredPsi}",
            Break1 = test.Break1?.ToString() ?? string.Empty,
            Break2 = test.Break2?.ToString() ?? string.Empty,
            Break3 = test.Break3?.ToString() ?? string.Empty,
            AveragePsi = CalculateAveragePsi(test.Break1, test.Break2, test.Break3),
            Comments = test.Comments ?? string.Empty
        };
    }

    private static string CalculateAgeOfTest(DateTime castingDate, TimeSpan? batchingStartTime, DateTime? testingDate)
    {
        if (!testingDate.HasValue)
            return string.Empty;

        // Combine casting date with batching start time to get the actual start time
        var startDateTime = batchingStartTime.HasValue
            ? castingDate.Add(batchingStartTime.Value)
            : castingDate;

        var timeSpan = testingDate.Value - startDateTime;
        var days = (int)timeSpan.TotalDays;
        var hours = timeSpan.Hours;
        var minutes = timeSpan.Minutes;

        // Format: "7" for whole days, "0d 12:53" for fractional days
        if (days > 0 && hours == 0 && minutes == 0)
            return days.ToString();

        return $"{days}d {hours}:{minutes:00}";
    }

    private static string CalculateAveragePsi(int? break1, int? break2, int? break3)
    {
        var breaks = new[] { break1, break2, break3 }.Where(b => b.HasValue).Select(b => b!.Value).ToList();

        if (breaks.Count == 0)
            return string.Empty;

        var average = breaks.Sum() / breaks.Count;
        return average.ToString();
    }

    private static string FormatTestingDate(DateTime? testingDate)
    {
        if (!testingDate.HasValue)
            return string.Empty;

        var dt = testingDate.Value;

        // If time component exists (not midnight), include it in format
        if (dt.TimeOfDay != TimeSpan.Zero)
        {
            return dt.ToString("M/d/yy H:mm");
        }

        // Otherwise just return month/day
        return dt.ToString("M/d");
    }
}
