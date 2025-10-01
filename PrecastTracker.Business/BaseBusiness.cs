using Microsoft.Extensions.Logging;
using PrecastTracker.Business.Core;

namespace PrecastTracker.Business;

public abstract class BaseBusiness<T> : IBusiness where T : BaseBusiness<T>
{
    protected readonly ILogger<T> _logger;

    protected BaseBusiness(ILogger<T> logger)
    {
        _logger = logger;
    }
}
