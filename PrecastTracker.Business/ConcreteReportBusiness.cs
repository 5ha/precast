using Microsoft.Extensions.Logging;
using PrecastTracker.Business.Core;
using PrecastTracker.Contracts.DTOs.RequestResponse;
using PrecastTracker.Data.Entities;
using PrecastTracker.Services;

namespace PrecastTracker.Business;

public class ConcreteReportBusiness : BaseBusiness<ConcreteReportBusiness>, IConcreteReportBusiness
{
    private readonly IConcreteReportService _service;
    private readonly IAgeCalculatorService _ageCalculatorService;

    public ConcreteReportBusiness(
        IConcreteReportService service,
        IAgeCalculatorService ageCalculatorService,
        ILogger<ConcreteReportBusiness> logger) : base(logger)
    {
        _service = service;
        _ageCalculatorService = ageCalculatorService;
    }

    public async Task<BusinessResult<IEnumerable<ConcreteReportResponse>>> GetConcreteReportAsync()
    {
        try
        {
            _logger.LogInformation("Generating concrete report");

            // Get all test sets with related data
            var testSets = (await _service.GetAllTestSetsWithRelatedDataAsync()).ToList();

            var reportData = new List<ConcreteReportResponse>();

            // For calculating TestId suffixes: we need to track placements within each mix batch for 1-day tests
            // The suffix is based on the order of placements (ordered by StartTime, then OvenId) within a MixBatch
            var mixBatchPlacementIndexes = new Dictionary<(int MixBatchId, int PlacementId), int>();

            // Build the placement index mapping for 1-day tests
            // Flatten all TestSetDays from all TestSets where DayNum == 1
            var oneDayTestSetDays = testSets
                .SelectMany(ts => ts.TestSetDays.Where(tsd => tsd.DayNum == 1)
                    .Select(tsd => new { TestSet = ts, TestSetDay = tsd }))
                .ToList();

            foreach (var group in oneDayTestSetDays.GroupBy(x => x.TestSet.Placement.MixBatchId))
            {
                var orderedPlacements = group
                    .Select(x => x.TestSet.Placement)
                    .DistinctBy(p => p.PlacementId)
                    .OrderBy(p => p.StartTime)
                    .ThenBy(p => p.OvenId)
                    .ToList();

                for (int i = 0; i < orderedPlacements.Count; i++)
                {
                    mixBatchPlacementIndexes[(group.Key, orderedPlacements[i].PlacementId)] = i + 1; // 1-based suffix (.1, .2, .3, etc.)
                }
            }

            // Generate report lines - one line per TestSetDay (grouping cylinders into Break #1, Break #2, Break #3)
            // Flatten TestSets to TestSetDays
            // Ordering: batch-level tests (7, 28-day) come before placement-level tests (1-day)
            var allTestSetDays = testSets
                .SelectMany(ts => ts.TestSetDays.Select(tsd => new { TestSet = ts, TestSetDay = tsd }))
                .OrderBy(x => x.TestSet.Placement.MixBatch.ProductionDay.Date)
                .ThenBy(x => x.TestSet.Placement.MixBatch.MixBatchId)
                .ThenBy(x => x.TestSetDay.DayNum == 1) // false (7/28-day) before true (1-day)
                .ThenBy(x => x.TestSet.Placement.StartTime)
                .ThenBy(x => x.TestSet.Placement.OvenId)
                .ThenBy(x => x.TestSetDay.DayNum)
                .ToList();

            foreach (var item in allTestSetDays)
            {
                var testSet = item.TestSet;
                var testSetDay = item.TestSetDay;
                var placement = testSet.Placement;
                var mixBatch = placement.MixBatch;
                var pour = placement.Pour;
                var productionDay = mixBatch.ProductionDay;

                // Get test cylinders for this test set day
                var cylinders = testSetDay.TestCylinders.ToList();

                // Calculate TestId with optional suffix for 1-day tests
                string testId;
                if (testSetDay.DayNum == 1)
                {
                    // For 1-day tests, add suffix based on placement order within the mix batch
                    var key = (mixBatch.MixBatchId, placement.PlacementId);
                    if (mixBatchPlacementIndexes.TryGetValue(key, out int placementSuffix))
                    {
                        testId = $"{mixBatch.MixBatchId}.{placementSuffix}";
                    }
                    else
                    {
                        testId = $"{mixBatch.MixBatchId}.1";
                    }
                }
                else
                {
                    // For 7-day and 28-day tests, use MixBatchId without suffix
                    testId = mixBatch.MixBatchId.ToString();
                }

                // Get the required PSI from MixDesignRequirements based on day num
                var requiredPsi = mixBatch.MixDesign.MixDesignRequirements
                    .FirstOrDefault(r => r.TestType == testSetDay.DayNum)?.RequiredPsi ?? 0;

                // Get truck numbers from deliveries - sort numerically if possible
                var truckNumbers = string.Join(", ", placement.Deliveries.Select(d => d.TruckId)
                    .OrderBy(t => int.TryParse(t, out var num) ? num : int.MaxValue)
                    .ThenBy(t => t));

                // Convert DayNum to Cylinder ID format (1 -> "1C", 7 -> "7C", 28 -> "28C")
                var cylinderId = $"{testSetDay.DayNum}C";

                // Format StartTime as time only (h:mm)
                var startTimeStr = placement.StartTime.ToString(@"h\:mm");

                // Get break PSI values for up to 3 cylinders
                var break1 = cylinders.ElementAtOrDefault(0)?.BreakPsi?.ToString() ?? string.Empty;
                var break2 = cylinders.ElementAtOrDefault(1)?.BreakPsi?.ToString() ?? string.Empty;
                var break3 = cylinders.ElementAtOrDefault(2)?.BreakPsi?.ToString() ?? string.Empty;

                // Calculate average PSI from all cylinders with non-null BreakPsi
                var cylindersWithBreaks = cylinders.Where(c => c.BreakPsi.HasValue).ToList();
                var averagePsi = cylindersWithBreaks.Any()
                    ? Math.Round(cylindersWithBreaks.Average(c => c.BreakPsi!.Value), MidpointRounding.AwayFromZero).ToString()
                    : string.Empty;

                // Use actual DateTested from TestSetDay
                var testingDate = testSetDay.DateTested;
                var comments = testSetDay.Comments ?? string.Empty;

                // Calculate Age of Test and Testing Date
                string ageOfTest;
                string testingDateStr;

                if (testingDate.HasValue)
                {
                    // Cylinders have been tested - use actual values
                    ageOfTest = _ageCalculatorService.CalculateAgeOfTest(productionDay.Date, placement.StartTime, testingDate.Value);
                    testingDateStr = FormatTestingDate(testingDate);
                }
                else if (testSetDay.DayNum == 1)
                {
                    // 1-day tests not yet tested - leave fields empty
                    ageOfTest = string.Empty;
                    testingDateStr = string.Empty;
                }
                else
                {
                    // 7-day and 28-day tests not yet tested - show scheduled date
                    ageOfTest = testSetDay.DayNum.ToString();
                    testingDateStr = FormatTestingDate(testSetDay.DateDue);
                }

                reportData.Add(new ConcreteReportResponse
                {
                    TestId = testId,
                    CylinderId = cylinderId,
                    CastingDate = productionDay.Date.ToString("MM/dd/yyyy"),
                    MixDesign = mixBatch.MixDesign.Code,
                    YardsPerBed = placement.Volume.ToString("0.##"),
                    BedId = pour.Bed.BedId.ToString(),
                    BatchingStartTime = startTimeStr,
                    JobId = pour.Job.Code,
                    JobName = pour.Job.Name,
                    TruckNo = truckNumbers,
                    PourId = pour.PourId.ToString(),
                    PieceType = placement.PieceType,
                    OvenId = placement.OvenId ?? string.Empty,
                    AgeOfTest = ageOfTest,
                    TestingDate = testingDateStr,
                    Required = requiredPsi.ToString(),
                    Break1 = break1,
                    Break2 = break2,
                    Break3 = break3,
                    AveragePsi = averagePsi,
                    Comments = comments
                });
            }

            return BusinessResult<IEnumerable<ConcreteReportResponse>>.Success(reportData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating concrete report");
            return BusinessResult<IEnumerable<ConcreteReportResponse>>.Failure(
                BusinessError.InternalError("Failed to generate concrete report"));
        }
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
