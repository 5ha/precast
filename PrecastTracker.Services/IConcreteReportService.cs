using PrecastTracker.Data.Entities;

namespace PrecastTracker.Services;

public interface IConcreteReportService : IService
{
    Task<IEnumerable<TestSet>> GetAllTestSetsWithRelatedDataAsync();
}
