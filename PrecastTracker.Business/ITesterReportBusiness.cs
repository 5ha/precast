using PrecastTracker.Business.Core;
using PrecastTracker.Contracts.DTOs.RequestResponse;

namespace PrecastTracker.Business;

public interface ITesterReportBusiness : IBusiness
{
    Task<BusinessResult<IEnumerable<TestCylinderQueueResponse>>> GetTestQueueAsync(DateTime endDate);
    Task<BusinessResult<IEnumerable<UntestedPlacementResponse>>> GetUntestedPlacementsAsync(int daysBack = 7);
    Task<BusinessResult<GetTestSetDayDetailsResponse>> GetTestSetDayDetailsAsync(int testSetDayId);
    Task<BusinessResult> SaveTestSetDayDataAsync(SaveTestSetDayDataRequest request);
}
