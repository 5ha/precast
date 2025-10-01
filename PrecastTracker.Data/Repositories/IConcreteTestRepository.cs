using PrecastTracker.Data.Entities;

namespace PrecastTracker.Data.Repositories;

public interface IConcreteTestRepository : IRepository
{
    Task<IEnumerable<ConcreteTest>> GetAllWithRelatedDataAsync();
}
