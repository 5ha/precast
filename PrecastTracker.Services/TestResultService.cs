using Microsoft.Extensions.Logging;
using PrecastTracker.Contracts.DTOs.RequestResponse;
using PrecastTracker.Data.Repositories;

namespace PrecastTracker.Services;

public class TestResultService : BaseService<TestResultService>, ITestResultService
{
    private readonly ITestSetDayRepository _testSetDayRepository;

    public TestResultService(
        ITestSetDayRepository testSetDayRepository,
        ILogger<TestResultService> logger) : base(logger)
    {
        _testSetDayRepository = testSetDayRepository;
    }

    public async Task UpdateTestSetDayResultsAsync(
        int testSetDayId,
        DateTime dateTested,
        string? comments,
        List<TestCylinderBreakInput> cylinderBreaks)
    {
        _logger.LogInformation("Updating test results for TestSetDayId: {TestSetDayId}", testSetDayId);

        // Load entity with test cylinders
        var testSetDay = await _testSetDayRepository.GetByIdWithRelatedDataAsync(testSetDayId);

        if (testSetDay == null)
        {
            _logger.LogWarning("Test set day not found for TestSetDayId: {TestSetDayId}", testSetDayId);
            throw new InvalidOperationException($"Test set day with ID {testSetDayId} not found");
        }

        // Validate: all cylinder IDs must belong to this TestSetDay
        var validCylinderIds = testSetDay.TestCylinders
            .Select(tc => tc.TestCylinderId)
            .ToHashSet();

        var invalidCylinders = cylinderBreaks
            .Where(cb => !validCylinderIds.Contains(cb.TestCylinderId))
            .ToList();

        if (invalidCylinders.Any())
        {
            _logger.LogWarning("Invalid cylinder IDs provided for TestSetDayId: {TestSetDayId}", testSetDayId);
            throw new InvalidOperationException("One or more cylinder IDs do not belong to this test set");
        }

        // Update TestSetDay properties
        testSetDay.DateTested = dateTested;
        testSetDay.Comments = comments;

        // Update BreakPsi for each cylinder
        foreach (var cylinderBreak in cylinderBreaks)
        {
            var cylinder = testSetDay.TestCylinders
                .First(tc => tc.TestCylinderId == cylinderBreak.TestCylinderId);

            cylinder.BreakPsi = cylinderBreak.BreakPsi;
        }

        _logger.LogInformation("Successfully updated test results for TestSetDayId: {TestSetDayId}", testSetDayId);
    }
}
