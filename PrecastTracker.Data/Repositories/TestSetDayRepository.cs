using Microsoft.EntityFrameworkCore;
using PrecastTracker.Data.Entities;

namespace PrecastTracker.Data.Repositories;

public class TestSetDayRepository : ITestSetDayRepository
{
    private readonly ApplicationDbContext _context;

    public TestSetDayRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TestSetDay?> GetByIdWithRelatedDataAsync(int testSetDayId)
    {
        // Only load TestCylinders for service to validate and update BreakPsi values
        return await _context.TestSetDays
            .Include(tsd => tsd.TestCylinders)
            .FirstOrDefaultAsync(tsd => tsd.TestSetDayId == testSetDayId);
    }

    public async Task<DateTime?> GetCastDateAsync(int testSetDayId)
    {
        // Return cast date without loading entities - used for validation
        return await _context.TestSetDays
            .Where(tsd => tsd.TestSetDayId == testSetDayId)
            .Select(tsd => (DateTime?)tsd.TestSet.Placement.MixBatch.ProductionDay.Date)
            .FirstOrDefaultAsync();
    }
}
