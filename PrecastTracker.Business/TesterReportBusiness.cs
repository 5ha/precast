using Microsoft.Extensions.Logging;
using PrecastTracker.Business.Core;
using PrecastTracker.Contracts.DTOs.RequestResponse;
using PrecastTracker.Data.Repositories;

namespace PrecastTracker.Business;

public class TesterReportBusiness : BaseBusiness<TesterReportBusiness>, ITesterReportBusiness
{
    private readonly ITesterReportRepository _repository;

    public TesterReportBusiness(
        ITesterReportRepository repository,
        ILogger<TesterReportBusiness> logger) : base(logger)
    {
        _repository = repository;
    }

    public async Task<BusinessResult<IEnumerable<TestCylinderQueueResponse>>> GetTestsDueTodayAsync()
    {
        try
        {
            _logger.LogInformation("Retrieving tests due today");

            var projections = await _repository.GetTestsDueTodayAsync();
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

            var projections = await _repository.GetTestsDuePastAsync();
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

            var projections = await _repository.GetTestsDueBetweenDatesAsync(startDate, endDate);
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
            IsComplete = projection.IsComplete
        };
    }
}
