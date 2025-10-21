using PrecastTracker.Contracts.DTOs.RequestResponse;

namespace PrecastTracker.Services;

public interface ITestResultService : IService
{
    Task UpdateTestSetDayResultsAsync(
        int testSetDayId,
        DateTime dateTested,
        string? comments,
        List<TestCylinderBreakInput> cylinderBreaks);
}
