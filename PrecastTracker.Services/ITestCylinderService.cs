using PrecastTracker.Data.Entities;

namespace PrecastTracker.Services;

public interface ITestCylinderService : IService
{
    Task<IEnumerable<TestCylinder>> GetAllTestsWithRelatedDataAsync();
    Task<IEnumerable<TestSet>> GetAllTestSetsWithRelatedDataAsync();
    Task<IEnumerable<TestCylinder>> GetTestCylindersByTestSetIdsAsync(IEnumerable<int> testSetIds);
}
