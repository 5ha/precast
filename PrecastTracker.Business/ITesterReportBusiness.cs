using PrecastTracker.Business.Core;
using PrecastTracker.Contracts.DTOs.RequestResponse;

namespace PrecastTracker.Business;

public interface ITesterReportBusiness : IBusiness
{
    Task<BusinessResult<IEnumerable<TestCylinderQueueResponse>>> GetTestsDueTodayAsync();
    Task<BusinessResult<IEnumerable<TestCylinderQueueResponse>>> GetTestsOverdueAsync();
    Task<BusinessResult<IEnumerable<TestCylinderQueueResponse>>> GetTestsUpcomingAsync(int days);
}
