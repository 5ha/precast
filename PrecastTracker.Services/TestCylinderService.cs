using Microsoft.Extensions.Logging;
using PrecastTracker.Data.Entities;
using PrecastTracker.Data.Repositories;

namespace PrecastTracker.Services;

public class TestCylinderService : BaseService<TestCylinderService>, ITestCylinderService
{
    private readonly ITestCylinderRepository _repository;

    public TestCylinderService(ITestCylinderRepository repository, ILogger<TestCylinderService> logger) : base(logger)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<TestCylinder>> GetAllTestsWithRelatedDataAsync()
    {
        _logger.LogInformation("Retrieving all test cylinders with related data");
        return await _repository.GetAllWithRelatedDataAsync();
    }

    public async Task<IEnumerable<TestSet>> GetAllTestSetsWithRelatedDataAsync()
    {
        _logger.LogInformation("Retrieving all test sets with related data");
        return await _repository.GetAllTestSetsWithRelatedDataAsync();
    }
}
