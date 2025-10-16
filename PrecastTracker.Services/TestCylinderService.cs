using Microsoft.Extensions.Logging;
using PrecastTracker.Data.Entities;
using PrecastTracker.Data.Repositories;

namespace PrecastTracker.Services;

public class ConcreteReportService : BaseService<ConcreteReportService>, IConcreteReportService
{
    private readonly ITestCylinderRepository _repository;

    public ConcreteReportService(ITestCylinderRepository repository, ILogger<ConcreteReportService> logger) : base(logger)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<TestSet>> GetAllTestSetsWithRelatedDataAsync()
    {
        _logger.LogInformation("Retrieving all test sets with related data");
        return await _repository.GetAllTestSetsWithRelatedDataAsync();
    }
}
