using PrecastTracker.Data.Entities;

namespace PrecastTracker.Services;

public interface IConcreteTestService : IService
{
    Task<IEnumerable<ConcreteTest>> GetAllTestsWithRelatedDataAsync();
    Task<IEnumerable<TestSet>> GetAllTestSetsWithRelatedDataAsync();
    Task<IEnumerable<ConcreteTest>> GetConcreteTestsByTestSetIdsAsync(IEnumerable<int> testSetIds);
}
