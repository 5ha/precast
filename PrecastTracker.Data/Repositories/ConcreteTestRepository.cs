using Microsoft.EntityFrameworkCore;
using PrecastTracker.Data.Entities;

namespace PrecastTracker.Data.Repositories;

public class ConcreteTestRepository : IConcreteTestRepository
{
    private readonly ApplicationDbContext _context;

    public ConcreteTestRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ConcreteTest>> GetAllWithRelatedDataAsync()
    {
        return await _context.ConcreteTests
            .Include(ct => ct.TestSet)
                .ThenInclude(ts => ts.Placement)
                    .ThenInclude(p => p.MixBatch)
                        .ThenInclude(mb => mb.MixDesign)
                            .ThenInclude(md => md.MixDesignRequirements)
            .Include(ct => ct.TestSet)
                .ThenInclude(ts => ts.Placement)
                    .ThenInclude(p => p.MixBatch)
                        .ThenInclude(mb => mb.ProductionDay)
            .Include(ct => ct.TestSet)
                .ThenInclude(ts => ts.Placement)
                    .ThenInclude(p => p.Deliveries)
            .Include(ct => ct.TestSet)
                .ThenInclude(ts => ts.Placement)
                    .ThenInclude(p => p.Pour)
                        .ThenInclude(pour => pour.Job)
            .Include(ct => ct.TestSet)
                .ThenInclude(ts => ts.Placement)
                    .ThenInclude(p => p.Pour)
                        .ThenInclude(pour => pour.Bed)
            .AsSplitQuery()  // Prevents cartesian explosion with multiple includes
            .OrderBy(ct => ct.ConcreteTestId)
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
            .AsSplitQuery()  // Prevents cartesian explosion with multiple includes
            .ToListAsync();

        // Order in memory after loading all data
        // Test type order: 7 (7-day), 28 (28-day), 1 (1-day)
        var testTypeOrder = new Dictionary<int, int> { { 7, 0 }, { 28, 1 }, { 1, 2 } };

        return testSets
            .OrderBy(ts => ts.Placement.MixBatch.ProductionDay.Date)
            .ThenBy(ts => ts.Placement.MixBatch.MixBatchId)
            .ThenBy(ts => testTypeOrder.GetValueOrDefault(ts.TestType, 99))
            .ThenBy(ts => ts.Placement.StartTime)
            .ThenBy(ts => ts.Placement.OvenId)
            .ToList();
    }

    public async Task<IEnumerable<ConcreteTest>> GetConcreteTestsByTestSetIdsAsync(IEnumerable<int> testSetIds)
    {
        return await _context.ConcreteTests
            .Where(ct => testSetIds.Contains(ct.TestSetId))
            .OrderBy(ct => ct.ConcreteTestId)
            .ToListAsync();
    }
}
