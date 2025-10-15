using Microsoft.EntityFrameworkCore;
using PrecastTracker.Data.Entities;

namespace PrecastTracker.Data.Repositories;

public class TestCylinderRepository : ITestCylinderRepository
{
    private readonly ApplicationDbContext _context;

    public TestCylinderRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TestCylinder>> GetAllWithRelatedDataAsync()
    {
        return await _context.TestCylinders
            .Include(tc => tc.TestSet)
                .ThenInclude(ts => ts.Placement)
                    .ThenInclude(p => p.MixBatch)
                        .ThenInclude(mb => mb.MixDesign)
                            .ThenInclude(md => md.MixDesignRequirements)
            .Include(tc => tc.TestSet)
                .ThenInclude(ts => ts.Placement)
                    .ThenInclude(p => p.MixBatch)
                        .ThenInclude(mb => mb.ProductionDay)
            .Include(tc => tc.TestSet)
                .ThenInclude(ts => ts.Placement)
                    .ThenInclude(p => p.Deliveries)
            .Include(tc => tc.TestSet)
                .ThenInclude(ts => ts.Placement)
                    .ThenInclude(p => p.Pour)
                        .ThenInclude(pour => pour.Job)
            .Include(tc => tc.TestSet)
                .ThenInclude(ts => ts.Placement)
                    .ThenInclude(p => p.Pour)
                        .ThenInclude(pour => pour.Bed)
            .AsSplitQuery()  // Prevents cartesian explosion with multiple includes
            .OrderBy(tc => tc.TestCylinderId)
            .ToListAsync();
    }

    public async Task<IEnumerable<TestSet>> GetAllTestSetsWithRelatedDataAsync()
    {
        // Load all test sets with navigation properties
        var testSets = await _context.TestSets
            .Include(ts => ts.Placement)
                .ThenInclude(p => p.MixBatch)
                    .ThenInclude(mb => mb.MixDesign)
                        .ThenInclude(md => md.MixDesignRequirements)
            .Include(ts => ts.Placement)
                .ThenInclude(p => p.MixBatch)
                    .ThenInclude(mb => mb.ProductionDay)
            .Include(ts => ts.Placement)
                .ThenInclude(p => p.Deliveries)
            .Include(ts => ts.Placement)
                .ThenInclude(p => p.Pour)
                    .ThenInclude(pour => pour.Job)
            .Include(ts => ts.Placement)
                .ThenInclude(p => p.Pour)
                    .ThenInclude(pour => pour.Bed)
            .Include(ts => ts.TestCylinders)
            .AsSplitQuery()  // Prevents cartesian explosion with multiple includes
            .ToListAsync();

        // Order in memory after loading all data
        return testSets
            .OrderBy(ts => ts.Placement.MixBatch.ProductionDay.Date)
            .ThenBy(ts => ts.Placement.MixBatch.MixBatchId)
            .ThenBy(ts => ts.Placement.StartTime)
            .ThenBy(ts => ts.Placement.OvenId)
            .ToList();
    }

    public async Task<IEnumerable<TestCylinder>> GetTestCylindersByTestSetIdsAsync(IEnumerable<int> testSetIds)
    {
        return await _context.TestCylinders
            .Where(tc => testSetIds.Contains(tc.TestSetId))
            .OrderBy(tc => tc.TestCylinderId)
            .ToListAsync();
    }
}
