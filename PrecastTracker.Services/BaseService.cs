using Microsoft.Extensions.Logging;

namespace PrecastTracker.Services;

public abstract class BaseService<T> : IService where T : BaseService<T>
{
    protected readonly ILogger<T> _logger;

    protected BaseService(ILogger<T> logger)
    {
        _logger = logger;
    }
}
