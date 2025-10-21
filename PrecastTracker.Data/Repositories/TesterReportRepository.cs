using Microsoft.EntityFrameworkCore;
using PrecastTracker.Data.Projections;

namespace PrecastTracker.Data.Repositories;

public class TesterReportRepository : ITesterReportRepository
{
    private readonly ApplicationDbContext _context;

    public TesterReportRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<List<TestCylinderQueueProjection>> GetTestsDuePastAsync()
    {
        var today = DateTime.Today;
        return _context.TestCylinders
            .AsNoTracking()
            .Where(tc => tc.TestSetDay.DateDue < today && tc.TestSetDay.DateTested == null)
            .Join(
                _context.MixDesignRequirements,
                tc => new {
                    MixDesignId = tc.TestSetDay.TestSet.Placement.MixBatch.MixDesignId,
                    TestType = tc.TestSetDay.DayNum
                },
                mdr => new {
                    MixDesignId = mdr.MixDesignId,
                    TestType = mdr.TestType
                },
                (tc, mdr) => new TestCylinderQueueProjection
                {
                    TestCylinderCode = tc.Code,
                    OvenId = tc.TestSetDay.TestSet.Placement.OvenId,
                    DayNum = tc.TestSetDay.DayNum,
                    CastDate = tc.TestSetDay.TestSet.Placement.MixBatch.ProductionDay.Date,
                    CastTime = tc.TestSetDay.TestSet.Placement.StartTime,
                    JobCode = tc.TestSetDay.TestSet.Placement.Pour.Job.Code,
                    JobName = tc.TestSetDay.TestSet.Placement.Pour.Job.Name,
                    MixDesignCode = tc.TestSetDay.TestSet.Placement.MixBatch.MixDesign.Code,
                    RequiredPsi = mdr.RequiredPsi,
                    PieceType = tc.TestSetDay.TestSet.Placement.PieceType,
                    TestSetId = tc.TestSetDay.TestSetId,
                    TestSetDayId = tc.TestSetDay.TestSetDayId,
                    DateDue = tc.TestSetDay.DateDue
                })
            .ToListAsync();
    }

    public Task<List<TestCylinderQueueProjection>> GetTestsDueTodayAsync()
    {
        var start = DateTime.Today;
        var end = start.AddDays(1);
        return _context.TestCylinders
            .AsNoTracking()
            .Where(tc => tc.TestSetDay.DateDue >= start && tc.TestSetDay.DateDue < end)
            .Join(
                _context.MixDesignRequirements,
                tc => new {
                    MixDesignId = tc.TestSetDay.TestSet.Placement.MixBatch.MixDesignId,
                    TestType = tc.TestSetDay.DayNum
                },
                mdr => new {
                    MixDesignId = mdr.MixDesignId,
                    TestType = mdr.TestType
                },
                (tc, mdr) => new TestCylinderQueueProjection
                {
                    TestCylinderCode = tc.Code,
                    OvenId = tc.TestSetDay.TestSet.Placement.OvenId,
                    DayNum = tc.TestSetDay.DayNum,
                    CastDate = tc.TestSetDay.TestSet.Placement.MixBatch.ProductionDay.Date,
                    CastTime = tc.TestSetDay.TestSet.Placement.StartTime,
                    JobCode = tc.TestSetDay.TestSet.Placement.Pour.Job.Code,
                    JobName = tc.TestSetDay.TestSet.Placement.Pour.Job.Name,
                    MixDesignCode = tc.TestSetDay.TestSet.Placement.MixBatch.MixDesign.Code,
                    RequiredPsi = mdr.RequiredPsi,
                    PieceType = tc.TestSetDay.TestSet.Placement.PieceType,
                    TestSetId = tc.TestSetDay.TestSetId,
                    TestSetDayId = tc.TestSetDay.TestSetDayId,
                    DateDue = tc.TestSetDay.DateDue
                })
            .ToListAsync();
    }

    public Task<List<TestCylinderQueueProjection>> GetTestsDueBetweenDatesAsync(DateTime startDate, DateTime endDate)
    {
        return _context.TestCylinders
            .AsNoTracking()
            .Where(tc => tc.TestSetDay.DateDue >= startDate && tc.TestSetDay.DateDue <= endDate)
            .Join(
                _context.MixDesignRequirements,
                tc => new {
                    MixDesignId = tc.TestSetDay.TestSet.Placement.MixBatch.MixDesignId,
                    TestType = tc.TestSetDay.DayNum
                },
                mdr => new {
                    MixDesignId = mdr.MixDesignId,
                    TestType = mdr.TestType
                },
                (tc, mdr) => new TestCylinderQueueProjection
                {
                    TestCylinderCode = tc.Code,
                    OvenId = tc.TestSetDay.TestSet.Placement.OvenId,
                    DayNum = tc.TestSetDay.DayNum,
                    CastDate = tc.TestSetDay.TestSet.Placement.MixBatch.ProductionDay.Date,
                    CastTime = tc.TestSetDay.TestSet.Placement.StartTime,
                    JobCode = tc.TestSetDay.TestSet.Placement.Pour.Job.Code,
                    JobName = tc.TestSetDay.TestSet.Placement.Pour.Job.Name,
                    MixDesignCode = tc.TestSetDay.TestSet.Placement.MixBatch.MixDesign.Code,
                    RequiredPsi = mdr.RequiredPsi,
                    PieceType = tc.TestSetDay.TestSet.Placement.PieceType,
                    TestSetId = tc.TestSetDay.TestSetId,
                    TestSetDayId = tc.TestSetDay.TestSetDayId,
                    DateDue = tc.TestSetDay.DateDue
                })
            .ToListAsync();
    }

    public Task<List<UntestedPlacementProjection>> GetUntestedPlacementsAsync(int daysBack)
    {
        var cutoffDate = DateTime.Today.AddDays(-daysBack);

        return _context.Placements
            .AsNoTracking()
            .Where(p => p.StartTime != null)
            .Where(p => !p.TestSets.Any())
            .Where(p => p.MixBatch.ProductionDay.Date >= cutoffDate)
            .Select(p => new UntestedPlacementProjection
            {
                PourId = p.PourId,
                PlacementId = p.PlacementId,
                CastDate = p.MixBatch.ProductionDay.Date,
                CastTime = p.StartTime,
                JobCode = p.Pour.Job.Code,
                JobName = p.Pour.Job.Name,
                MixDesignCode = p.MixBatch.MixDesign.Code,
                PieceType = p.PieceType,
                Volume = p.Volume
            })
            .ToListAsync();
    }

    public async Task<TestSetDayDetailsProjection?> GetTestSetDayDetailsProjectionAsync(int testSetDayId)
    {
        var query = from tsd in _context.TestSetDays
            where tsd.TestSetDayId == testSetDayId
            join mdr in _context.MixDesignRequirements
                on new { MixDesignId = tsd.TestSet.Placement.MixBatch.MixDesignId, TestType = tsd.DayNum }
                equals new { MixDesignId = mdr.MixDesignId, TestType = mdr.TestType }
            select new TestSetDayDetailsProjection
            {
                TestSetDayId = tsd.TestSetDayId,
                DayNum = tsd.DayNum,
                Comments = tsd.Comments,
                DateDue = tsd.DateDue,
                DateTested = tsd.DateTested,
                JobCode = tsd.TestSet.Placement.Pour.Job.Code,
                JobName = tsd.TestSet.Placement.Pour.Job.Name,
                MixDesignCode = tsd.TestSet.Placement.MixBatch.MixDesign.Code,
                RequiredPsi = mdr.RequiredPsi,
                PieceType = tsd.TestSet.Placement.PieceType,
                CastDate = tsd.TestSet.Placement.MixBatch.ProductionDay.Date,
                CastTime = tsd.TestSet.Placement.StartTime,
                TestCylinders = tsd.TestCylinders
                    .Select(tc => new TestCylinderBreakProjection
                    {
                        TestCylinderId = tc.TestCylinderId,
                        Code = tc.Code,
                        BreakPsi = tc.BreakPsi
                    })
                    .ToList()
            };

        return await query.AsNoTracking().FirstOrDefaultAsync();
    }
}
