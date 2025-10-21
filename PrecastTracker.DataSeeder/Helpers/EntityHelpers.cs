using PrecastTracker.Data;
using PrecastTracker.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace PrecastTracker.DataSeeder.Helpers;

/// <summary>
/// Helper methods for ensuring base entities exist in the database.
/// </summary>
public static class BaseEntityHelpers
{
    /// <summary>
    /// Ensures a ProductionDay exists for the given date. Creates if not found.
    /// </summary>
    public static ProductionDay EnsureProductionDay(ApplicationDbContext context, DateTime date)
    {
        var normalizedDate = date.Date;
        var productionDay = context.ProductionDays.FirstOrDefault(pd => pd.Date == normalizedDate);
        if (productionDay == null)
        {
            productionDay = new ProductionDay { Date = normalizedDate };
            context.ProductionDays.Add(productionDay);
            context.SaveChanges();
        }
        return productionDay;
    }

    /// <summary>
    /// Ensures a Job exists with the given code. Creates if not found.
    /// </summary>
    public static Job EnsureJob(ApplicationDbContext context, string code, string name)
    {
        var job = context.Jobs.FirstOrDefault(j => j.Code == code);
        if (job == null)
        {
            job = new Job { Code = code, Name = name };
            context.Jobs.Add(job);
            context.SaveChanges();
        }
        return job;
    }

    /// <summary>
    /// Ensures a Bed exists with the given ID. Creates if not found.
    /// </summary>
    public static Bed EnsureBed(ApplicationDbContext context, int bedId)
    {
        var bed = context.Beds.Find(bedId);
        if (bed == null)
        {
            bed = new Bed { BedId = bedId };
            context.Beds.Add(bed);
            context.SaveChanges();
        }
        return bed;
    }

    /// <summary>
    /// Ensures a MixDesign exists with the given code. Creates if not found.
    /// Also ensures standard test requirements (1, 7, 28 day) exist for the mix design.
    /// </summary>
    public static MixDesign EnsureMixDesign(ApplicationDbContext context, string code)
    {
        var mixDesign = context.MixDesigns.FirstOrDefault(md => md.Code == code);
        if (mixDesign == null)
        {
            mixDesign = new MixDesign { Code = code };
            context.MixDesigns.Add(mixDesign);
            context.SaveChanges();

            // Create standard test requirements for 1, 7, and 28 day tests
            var requirements = new[]
            {
                new MixDesignRequirement { MixDesignId = mixDesign.MixDesignId, TestType = 1, RequiredPsi = 3000 },
                new MixDesignRequirement { MixDesignId = mixDesign.MixDesignId, TestType = 7, RequiredPsi = 5000 },
                new MixDesignRequirement { MixDesignId = mixDesign.MixDesignId, TestType = 28, RequiredPsi = 6000 }
            };
            context.MixDesignRequirements.AddRange(requirements);
            context.SaveChanges();
        }
        return mixDesign;
    }
}

/// <summary>
/// Helper methods for ensuring mid-level entities exist in the database.
/// </summary>
public static class MidLevelEntityHelpers
{
    /// <summary>
    /// Ensures a Pour exists for the given job and bed combination. Creates if not found.
    /// </summary>
    public static Pour EnsurePour(ApplicationDbContext context, int jobId, int bedId)
    {
        var pour = context.Pours.FirstOrDefault(p => p.JobId == jobId && p.BedId == bedId);
        if (pour == null)
        {
            pour = new Pour { JobId = jobId, BedId = bedId };
            context.Pours.Add(pour);
            context.SaveChanges();
        }
        return pour;
    }

    /// <summary>
    /// Ensures a MixBatch exists for the given production day and mix design. Creates if not found.
    /// </summary>
    public static MixBatch EnsureMixBatch(ApplicationDbContext context, int productionDayId, int mixDesignId)
    {
        var mixBatch = context.MixBatches.FirstOrDefault(mb =>
            mb.ProductionDayId == productionDayId && mb.MixDesignId == mixDesignId);
        if (mixBatch == null)
        {
            mixBatch = new MixBatch
            {
                ProductionDayId = productionDayId,
                MixDesignId = mixDesignId
            };
            context.MixBatches.Add(mixBatch);
            context.SaveChanges();
        }
        return mixBatch;
    }
}

/// <summary>
/// Helper methods for creating high-level entities in the database.
/// </summary>
public static class HighLevelEntityHelpers
{
    /// <summary>
    /// Creates a Placement with all required dependencies. Dependencies are created if they don't exist.
    /// </summary>
    public static Placement CreatePlacement(
        ApplicationDbContext context,
        string jobCode,
        string jobName,
        int bedId,
        string mixDesignCode,
        DateTime productionDate,
        string pieceType,
        TimeSpan? startTime,
        decimal volume,
        string? ovenId = null)
    {
        var productionDay = BaseEntityHelpers.EnsureProductionDay(context, productionDate);
        var job = BaseEntityHelpers.EnsureJob(context, jobCode, jobName);
        var bed = BaseEntityHelpers.EnsureBed(context, bedId);
        var mixDesign = BaseEntityHelpers.EnsureMixDesign(context, mixDesignCode);
        var pour = MidLevelEntityHelpers.EnsurePour(context, job.JobId, bed.BedId);
        var mixBatch = MidLevelEntityHelpers.EnsureMixBatch(context, productionDay.ProductionDayId, mixDesign.MixDesignId);

        var placement = new Placement
        {
            PourId = pour.PourId,
            MixBatchId = mixBatch.MixBatchId,
            PieceType = pieceType,
            StartTime = startTime,
            Volume = volume,
            OvenId = ovenId
        };
        context.Placements.Add(placement);
        context.SaveChanges();

        return placement;
    }

    /// <summary>
    /// Creates a TestSet for the given placement.
    /// </summary>
    public static TestSet CreateTestSet(ApplicationDbContext context, int placementId)
    {
        var testSet = new TestSet { PlacementId = placementId };
        context.TestSets.Add(testSet);
        context.SaveChanges();
        return testSet;
    }

    /// <summary>
    /// Creates a TestSetDay for the given test set.
    /// </summary>
    public static TestSetDay CreateTestSetDay(
        ApplicationDbContext context,
        int testSetId,
        int dayNum,
        DateTime dateDue,
        DateTime? dateTested = null,
        string? comments = null)
    {
        var testSetDay = new TestSetDay
        {
            TestSetId = testSetId,
            DayNum = dayNum,
            DateDue = dateDue,
            DateTested = dateTested,
            Comments = comments
        };
        context.TestSetDays.Add(testSetDay);
        context.SaveChanges();
        return testSetDay;
    }

    /// <summary>
    /// Creates a TestCylinder for the given test set day.
    /// </summary>
    public static TestCylinder CreateTestCylinder(
        ApplicationDbContext context,
        int testSetDayId,
        string code,
        int? breakPsi = null)
    {
        var testCylinder = new TestCylinder
        {
            TestSetDayId = testSetDayId,
            Code = code,
            BreakPsi = breakPsi
        };
        context.TestCylinders.Add(testCylinder);
        context.SaveChanges();
        return testCylinder;
    }
}
