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
            .Where(tc => tc.TestSetDay.DateDue < today && !tc.TestSetDay.IsComplete)
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
                    IsComplete = tc.TestSetDay.IsComplete
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
                    IsComplete = tc.TestSetDay.IsComplete
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
                    IsComplete = tc.TestSetDay.IsComplete
                })
            .ToListAsync();
    }
}
