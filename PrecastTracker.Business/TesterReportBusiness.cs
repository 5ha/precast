using Microsoft.Extensions.Logging;
using PrecastTracker.Business.Core;
using PrecastTracker.Contracts.DTOs.RequestResponse;
using PrecastTracker.Data;
using PrecastTracker.Data.Repositories;
using PrecastTracker.Services;

namespace PrecastTracker.Business;

public class TesterReportBusiness : BaseBusiness<TesterReportBusiness>, ITesterReportBusiness
{
    private readonly ITesterReportRepository _testerReportRepository;
    private readonly ITestSetDayRepository _testSetDayRepository;
    private readonly ITestResultService _testResultService;
    private readonly ApplicationDbContext _context;

    public TesterReportBusiness(
        ITesterReportRepository testerReportRepository,
        ITestSetDayRepository testSetDayRepository,
        ITestResultService testResultService,
        ApplicationDbContext context,
        ILogger<TesterReportBusiness> logger) : base(logger)
    {
        _testerReportRepository = testerReportRepository;
        _testSetDayRepository = testSetDayRepository;
        _testResultService = testResultService;
        _context = context;
    }

    public async Task<BusinessResult<IEnumerable<TestCylinderQueueResponse>>> GetTestsDueTodayAsync()
    {
        try
        {
            _logger.LogInformation("Retrieving tests due today");

            var projections = await _testerReportRepository.GetTestsDueTodayAsync();
            var response = projections.Select(MapToResponse);

            return BusinessResult<IEnumerable<TestCylinderQueueResponse>>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tests due today");
            return BusinessResult<IEnumerable<TestCylinderQueueResponse>>.Failure(
                BusinessError.InternalError("Failed to retrieve tests due today"));
        }
    }

    public async Task<BusinessResult<IEnumerable<TestCylinderQueueResponse>>> GetTestsOverdueAsync()
    {
        try
        {
            _logger.LogInformation("Retrieving overdue tests");

            var projections = await _testerReportRepository.GetTestsDuePastAsync();
            var response = projections.Select(MapToResponse);

            return BusinessResult<IEnumerable<TestCylinderQueueResponse>>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving overdue tests");
            return BusinessResult<IEnumerable<TestCylinderQueueResponse>>.Failure(
                BusinessError.InternalError("Failed to retrieve overdue tests"));
        }
    }

    public async Task<BusinessResult<IEnumerable<TestCylinderQueueResponse>>> GetTestsUpcomingAsync(int days)
    {
        try
        {
            _logger.LogInformation("Retrieving upcoming tests for next {Days} days", days);

            var startDate = DateTime.Today.AddDays(1);
            var endDate = DateTime.Today.AddDays(days).AddDays(1).AddTicks(-1);

            var projections = await _testerReportRepository.GetTestsDueBetweenDatesAsync(startDate, endDate);
            var response = projections.Select(MapToResponse);

            return BusinessResult<IEnumerable<TestCylinderQueueResponse>>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving upcoming tests for next {Days} days", days);
            return BusinessResult<IEnumerable<TestCylinderQueueResponse>>.Failure(
                BusinessError.InternalError($"Failed to retrieve upcoming tests for next {days} days"));
        }
    }

    public async Task<BusinessResult<IEnumerable<UntestedPlacementResponse>>> GetUntestedPlacementsAsync(int daysBack = 7)
    {
        try
        {
            _logger.LogInformation("Retrieving untested placements from the last {DaysBack} days", daysBack);

            var projections = await _testerReportRepository.GetUntestedPlacementsAsync(daysBack);
            var response = projections.Select(MapToUntestedPlacementResponse);

            return BusinessResult<IEnumerable<UntestedPlacementResponse>>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving untested placements from the last {DaysBack} days", daysBack);
            return BusinessResult<IEnumerable<UntestedPlacementResponse>>.Failure(
                BusinessError.InternalError($"Failed to retrieve untested placements from the last {daysBack} days"));
        }
    }

    private static TestCylinderQueueResponse MapToResponse(Data.Projections.TestCylinderQueueProjection projection)
    {
        return new TestCylinderQueueResponse
        {
            TestCylinderCode = projection.TestCylinderCode,
            OvenId = projection.OvenId,
            DayNum = projection.DayNum,
            CastDate = projection.CastDate,
            CastTime = projection.CastTime,
            JobCode = projection.JobCode,
            JobName = projection.JobName,
            MixDesignCode = projection.MixDesignCode,
            RequiredPsi = projection.RequiredPsi,
            PieceType = projection.PieceType,
            TestSetId = projection.TestSetId,
            TestSetDayId = projection.TestSetDayId,
            DateDue = projection.DateDue
        };
    }

    private static UntestedPlacementResponse MapToUntestedPlacementResponse(Data.Projections.UntestedPlacementProjection projection)
    {
        return new UntestedPlacementResponse
        {
            PourId = projection.PourId,
            PlacementId = projection.PlacementId,
            CastDate = projection.CastDate,
            CastTime = projection.CastTime,
            JobCode = projection.JobCode,
            JobName = projection.JobName,
            MixDesignCode = projection.MixDesignCode,
            PieceType = projection.PieceType,
            Volume = projection.Volume
        };
    }

    public async Task<BusinessResult<GetTestSetDayDetailsResponse>> GetTestSetDayDetailsAsync(int testSetDayId)
    {
        try
        {
            _logger.LogInformation("Retrieving test set day details for TestSetDayId: {TestSetDayId}", testSetDayId);

            var projection = await _testerReportRepository.GetTestSetDayDetailsProjectionAsync(testSetDayId);

            if (projection == null)
            {
                _logger.LogWarning("Test set day not found for TestSetDayId: {TestSetDayId}", testSetDayId);
                return BusinessResult<GetTestSetDayDetailsResponse>.Failure(
                    BusinessError.NotFound($"Test set day with ID {testSetDayId} not found"));
            }

            var response = MapToDetailsResponse(projection);
            return BusinessResult<GetTestSetDayDetailsResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving test set day details for TestSetDayId: {TestSetDayId}", testSetDayId);
            return BusinessResult<GetTestSetDayDetailsResponse>.Failure(
                BusinessError.InternalError("Failed to retrieve test set day details"));
        }
    }

    public async Task<BusinessResult> SaveTestSetDayDataAsync(SaveTestSetDayDataRequest request)
    {
        try
        {
            _logger.LogInformation("Saving test set day data for TestSetDayId: {TestSetDayId}", request.TestSetDayId);

            // Validate: DateTested must not be before CastDate
            var castDate = await _testSetDayRepository.GetCastDateAsync(request.TestSetDayId);

            if (castDate == null)
            {
                _logger.LogWarning("Test set day not found for TestSetDayId: {TestSetDayId}", request.TestSetDayId);
                return BusinessResult.Failure(
                    BusinessError.NotFound($"Test set day with ID {request.TestSetDayId} not found"));
            }

            if (request.DateTested < castDate.Value)
            {
                _logger.LogWarning(
                    "DateTested ({DateTested}) is before CastDate ({CastDate}) for TestSetDayId: {TestSetDayId}",
                    request.DateTested, castDate.Value, request.TestSetDayId);
                return BusinessResult.Failure(
                    BusinessError.Validation($"Test date cannot be before cast date ({castDate.Value:yyyy-MM-dd})"));
            }

            // Call service to update entities (service loads entity itself)
            try
            {
                await _testResultService.UpdateTestSetDayResultsAsync(
                    request.TestSetDayId,
                    request.DateTested,
                    request.Comments,
                    request.CylinderBreaks);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Validation error while updating test results for TestSetDayId: {TestSetDayId}", request.TestSetDayId);
                return BusinessResult.Failure(BusinessError.Validation(ex.Message));
            }

            // Save changes
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully saved test set day data for TestSetDayId: {TestSetDayId}", request.TestSetDayId);
            return BusinessResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving test set day data for TestSetDayId: {TestSetDayId}", request.TestSetDayId);
            return BusinessResult.Failure(
                BusinessError.InternalError("Failed to save test set day data"));
        }
    }

    private static GetTestSetDayDetailsResponse MapToDetailsResponse(Data.Projections.TestSetDayDetailsProjection projection)
    {
        return new GetTestSetDayDetailsResponse
        {
            TestSetDayId = projection.TestSetDayId,
            DayNum = projection.DayNum,
            Comments = projection.Comments,
            DateDue = projection.DateDue,
            DateTested = projection.DateTested,
            JobCode = projection.JobCode,
            JobName = projection.JobName,
            MixDesignCode = projection.MixDesignCode,
            RequiredPsi = projection.RequiredPsi,
            PieceType = projection.PieceType,
            CastDate = projection.CastDate,
            CastTime = projection.CastTime,
            TestCylinders = projection.TestCylinders
                .Select(tc => new TestCylinderBreakDto
                {
                    TestCylinderId = tc.TestCylinderId,
                    Code = tc.Code,
                    BreakPsi = tc.BreakPsi
                })
                .ToList()
        };
    }
}
