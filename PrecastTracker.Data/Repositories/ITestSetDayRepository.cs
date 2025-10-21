using PrecastTracker.Data.Entities;

namespace PrecastTracker.Data.Repositories;

public interface ITestSetDayRepository : IRepository
{
    Task<TestSetDay?> GetByIdWithRelatedDataAsync(int testSetDayId);
    Task<DateTime?> GetCastDateAsync(int testSetDayId);
}
