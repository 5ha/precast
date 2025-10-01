using PrecastTracker.Business.Core;
using PrecastTracker.Contracts.DTOs.RequestResponse;

namespace PrecastTracker.Business;

public interface IConcreteReportBusiness : IBusiness
{
    Task<BusinessResult<IEnumerable<ConcreteReportResponse>>> GetConcreteReportAsync();
}
