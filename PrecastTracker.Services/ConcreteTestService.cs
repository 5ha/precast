using Microsoft.Extensions.Logging;
using PrecastTracker.Data.Entities;
using PrecastTracker.Data.Repositories;

namespace PrecastTracker.Services;

public class ConcreteTestService : BaseService<ConcreteTestService>, IConcreteTestService
{
    private readonly IConcreteTestRepository _repository;

    public ConcreteTestService(IConcreteTestRepository repository, ILogger<ConcreteTestService> logger) : base(logger)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<ConcreteTest>> GetAllTestsWithRelatedDataAsync()
    {
        _logger.LogInformation("Retrieving all concrete tests with related data");
        return await _repository.GetAllWithRelatedDataAsync();
    }
}
