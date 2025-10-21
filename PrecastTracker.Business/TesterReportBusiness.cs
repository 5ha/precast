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
    private readonly IUnitOfWork _unitOfWork;

    public TesterReportBusiness(
        ITesterReportRepository testerReportRepository,
        ITestSetDayRepository testSetDayRepository,
        ITestResultService testResultService,
        IUnitOfWork unitOfWork,
        ILogger<TesterReportBusiness> logger) : base(logger)
    {
        _testerReportRepository = testerReportRepository;
        _testSetDayRepository = testSetDayRepository;
        _testResultService = testResultService;
        _unitOfWork = unitOfWork;
    }

    public async Task<BusinessResult<IEnumerable<TestCylinderQueueResponse>>> GetTestQueueAsync(DateTime endDate)
    {
        try
        {
            _logger.LogInformation("Retrieving test queue up to {EndDate}", endDate);

            var projections = await _testerReportRepository.GetTestQueueAsync(endDate);
            var response = projections.Select(MapToResponse);

            return BusinessResult<IEnumerable<TestCylinderQueueResponse>>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving test queue up to {EndDate}", endDate);
            return BusinessResult<IEnumerable<TestCylinderQueueResponse>>.Failure(
                BusinessError.InternalError("Failed to retrieve test queue"));
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
            DateDue = projection.DateDue,
            DateTested = projection.DateTested
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

    public async Task<BusinessResult<TestCylinderQueueResponse>> SaveTestSetDayDataAsync(SaveTestSetDayDataRequest request)
    {
        try
        {
            _logger.LogInformation("Saving test set day data for TestSetDayId: {TestSetDayId}", request.TestSetDayId);

            // Validate: DateTested must not be before CastDate
            var castDate = await _testSetDayRepository.GetCastDateAsync(request.TestSetDayId);

            if (castDate == null)
            {
                _logger.LogWarning("Test set day not found for TestSetDayId: {TestSetDayId}", request.TestSetDayId);
                return BusinessResult<TestCylinderQueueResponse>.Failure(
                    BusinessError.NotFound($"Test set day with ID {request.TestSetDayId} not found"));
            }

            if (request.DateTested < castDate.Value)
            {
                _logger.LogWarning(
                    "DateTested ({DateTested}) is before CastDate ({CastDate}) for TestSetDayId: {TestSetDayId}",
                    request.DateTested, castDate.Value, request.TestSetDayId);
                return BusinessResult<TestCylinderQueueResponse>.Failure(
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
                return BusinessResult<TestCylinderQueueResponse>.Failure(BusinessError.Validation(ex.Message));
            }

            // Save changes
            await _unitOfWork.SaveChangesAsync();

            // Fetch updated projection as source of truth
            var updatedProjection = await _testerReportRepository.GetTestQueueItemAsync(request.TestSetDayId);

            if (updatedProjection == null)
            {
                _logger.LogWarning("Could not retrieve updated test queue item for TestSetDayId: {TestSetDayId}", request.TestSetDayId);
                return BusinessResult<TestCylinderQueueResponse>.Failure(
                    BusinessError.InternalError("Test data was saved but could not be retrieved"));
            }

            var response = MapToResponse(updatedProjection);

            _logger.LogInformation("Successfully saved test set day data for TestSetDayId: {TestSetDayId}", request.TestSetDayId);
            return BusinessResult<TestCylinderQueueResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving test set day data for TestSetDayId: {TestSetDayId}", request.TestSetDayId);
            return BusinessResult<TestCylinderQueueResponse>.Failure(
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
