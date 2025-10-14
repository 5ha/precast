using PrecastTracker.Data.Entities;

namespace PrecastTracker.Data.Repositories;

public interface IConcreteTestRepository : IRepository
{
    Task<IEnumerable<ConcreteTest>> GetAllWithRelatedDataAsync();
    Task<IEnumerable<TestSet>> GetAllTestSetsWithRelatedDataAsync();
    Task<IEnumerable<ConcreteTest>> GetConcreteTestsByTestSetIdsAsync(IEnumerable<int> testSetIds);
}
