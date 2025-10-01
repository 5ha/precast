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
            .Include(ct => ct.Placement)
                .ThenInclude(p => p.MixDesign)
            .Include(ct => ct.Placement)
                .ThenInclude(p => p.Pour)
                    .ThenInclude(pour => pour.Job)
            .Include(ct => ct.Placement)
                .ThenInclude(p => p.Pour)
                    .ThenInclude(pour => pour.Bed)
            .OrderBy(ct => ct.ConcreteTestId)
            .ToListAsync();
    }
}
