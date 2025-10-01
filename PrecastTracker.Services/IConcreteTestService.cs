using PrecastTracker.Data.Entities;

namespace PrecastTracker.Services;

public interface IConcreteTestService : IService
{
    Task<IEnumerable<ConcreteTest>> GetAllTestsWithRelatedDataAsync();
}
