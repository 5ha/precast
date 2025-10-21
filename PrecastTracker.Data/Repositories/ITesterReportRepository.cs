using PrecastTracker.Data.Projections;

namespace PrecastTracker.Data.Repositories;

public interface ITesterReportRepository : IRepository
{
    Task<List<TestCylinderQueueProjection>> GetTestsDuePastAsync();
    Task<List<TestCylinderQueueProjection>> GetTestsDueTodayAsync();
    Task<List<TestCylinderQueueProjection>> GetTestsDueBetweenDatesAsync(DateTime startDate, DateTime endDate);
    Task<List<UntestedPlacementProjection>> GetUntestedPlacementsAsync(int daysBack);
}
