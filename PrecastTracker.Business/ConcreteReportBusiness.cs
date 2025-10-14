using Microsoft.Extensions.Logging;
using PrecastTracker.Business.Core;
using PrecastTracker.Contracts.DTOs.RequestResponse;
using PrecastTracker.Data.Entities;
using PrecastTracker.Services;

namespace PrecastTracker.Business;

public class ConcreteReportBusiness : BaseBusiness<ConcreteReportBusiness>, IConcreteReportBusiness
{
    private readonly IConcreteTestService _service;
    private readonly IAgeCalculatorService _ageCalculatorService;

    public ConcreteReportBusiness(
        IConcreteTestService service,
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

            // Get all test sets with related data, already ordered by production date, start time, oven id, and test type
            var testSets = (await _service.GetAllTestSetsWithRelatedDataAsync()).ToList();

            // Get all concrete tests for these test sets
            var testSetIds = testSets.Select(ts => ts.TestSetId).ToList();
            var concreteTests = (await _service.GetConcreteTestsByTestSetIdsAsync(testSetIds))
                .GroupBy(ct => ct.TestSetId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var reportData = new List<ConcreteReportResponse>();

            // For calculating TestId suffixes: we need to track placements within each mix batch for 1-day tests
            // The suffix is based on the order of placements (ordered by StartTime, then OvenId) within a MixBatch
            var mixBatchPlacementIndexes = new Dictionary<(int MixBatchId, int PlacementId), int>();

            // Build the placement index mapping for 1-day tests
            var oneDayTests = testSets.Where(ts => ts.TestType == 1).ToList();
            foreach (var group in oneDayTests.GroupBy(ts => ts.Placement.MixBatchId))
            {
                var orderedPlacements = group
                    .Select(ts => ts.Placement)
                    .DistinctBy(p => p.PlacementId)
                    .OrderBy(p => p.StartTime)
                    .ThenBy(p => p.OvenId)
                    .ToList();

                for (int i = 0; i < orderedPlacements.Count; i++)
                {
                    mixBatchPlacementIndexes[(group.Key, orderedPlacements[i].PlacementId)] = i + 1; // 1-based suffix (.1, .2, .3, etc.)
                }
            }

            // Generate report lines
            foreach (var testSet in testSets)
            {
                var placement = testSet.Placement;
                var mixBatch = placement.MixBatch;
                var pour = placement.Pour;
                var productionDay = mixBatch.ProductionDay;

                // Calculate TestId with optional suffix for 1-day tests
                string testId;
                if (testSet.TestType == 1)
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

                // Get the required PSI from MixDesignRequirements based on test type
                var requiredPsi = mixBatch.MixDesign.MixDesignRequirements
                    .FirstOrDefault(r => r.TestType == testSet.TestType)?.RequiredPsi ?? 0;

                // Get truck numbers from deliveries - sort numerically if possible
                var truckNumbers = string.Join(", ", placement.Deliveries.Select(d => d.TruckId)
                    .OrderBy(t => int.TryParse(t, out var num) ? num : int.MaxValue)
                    .ThenBy(t => t));

                // Get breaks from concrete tests
                var breaks = concreteTests.ContainsKey(testSet.TestSetId)
                    ? concreteTests[testSet.TestSetId].Select(ct => ct.BreakPsi).ToList()
                    : new List<int>();

                var averagePsi = breaks.Count > 0
                    ? ((int)Math.Round((double)breaks.Sum() / breaks.Count, MidpointRounding.AwayFromZero)).ToString()
                    : string.Empty;

                // Convert TestType to Cylinder ID format (1 -> "1C", 7 -> "7C", 28 -> "28C")
                var cylinderId = $"{testSet.TestType}C";

                // Format StartTime as time only (h:mm)
                var startTimeStr = placement.StartTime.ToString(@"h\:mm");

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
                    AgeOfTest = _ageCalculatorService.CalculateAgeOfTest(productionDay.Date, placement.StartTime, testSet.TestingDate),
                    TestingDate = FormatTestingDate(testSet.TestingDate),
                    Required = requiredPsi.ToString(),
                    Break1 = breaks.Count > 0 ? breaks[0].ToString() : string.Empty,
                    Break2 = breaks.Count > 1 ? breaks[1].ToString() : string.Empty,
                    Break3 = breaks.Count > 2 ? breaks[2].ToString() : string.Empty,
                    AveragePsi = averagePsi,
                    Comments = testSet.Comments ?? string.Empty
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
