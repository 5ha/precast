using PrecastTracker.Data.Projections;

namespace PrecastTracker.Data.Repositories;

public interface ITesterReportRepository : IRepository
{
    Task<List<TestCylinderQueueProjection>> GetTestQueueAsync(DateTime endDate);
    Task<List<UntestedPlacementProjection>> GetUntestedPlacementsAsync(int daysBack);
    Task<TestSetDayDetailsProjection?> GetTestSetDayDetailsProjectionAsync(int testSetDayId);
}
