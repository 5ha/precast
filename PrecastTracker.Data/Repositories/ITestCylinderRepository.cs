using PrecastTracker.Data.Entities;

namespace PrecastTracker.Data.Repositories;

public interface ITestCylinderRepository : IRepository
{
    Task<IEnumerable<TestCylinder>> GetAllWithRelatedDataAsync();
    Task<IEnumerable<TestSet>> GetAllTestSetsWithRelatedDataAsync();
}
