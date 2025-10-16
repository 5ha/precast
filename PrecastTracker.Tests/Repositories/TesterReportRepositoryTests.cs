using Microsoft.EntityFrameworkCore;
using PrecastTracker.Data;
using PrecastTracker.Data.Entities;
using PrecastTracker.Data.Repositories;

namespace PrecastTracker.Tests.Repositories;

[Collection("Sequential")]
public class TesterReportRepositoryTests
{
    private ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        var context = new ApplicationDbContext(options);
        context.Database.OpenConnection();
        context.Database.EnsureCreated();

        return context;
    }

    #region GetTestsDuePastAsync Tests

    [Fact]
    public async Task GetTestsDuePastAsync_ReturnsOnlyIncompleteTestsDuePast()
    {
        // Verify that GetTestsDuePastAsync returns only tests that are:
        // 1. Due before today (DateDue < today)
        // 2. Marked as incomplete (IsComplete = false)

        // Arrange
        using var context = CreateContext();
        var repository = new TesterReportRepository(context);
        var today = DateTime.Today;
        var testData = CreateTestDataSet(today);
        await SeedTestDataAsync(context, testData);

        // Act
        var result = await repository.GetTestsDuePastAsync();
        var resultList = result.ToList();

        // Assert
        Assert.Single(resultList); // Only one test due in the past and incomplete
        Assert.Equal("PAST-1-7", resultList[0].TestCylinderCode);
        Assert.Equal(7, resultList[0].DayNum);
        Assert.False(resultList[0].IsComplete);
    }

    [Fact]
    public async Task GetTestsDuePastAsync_ExcludesCompletedTests()
    {
        // Verify that tests marked as complete (IsComplete = true) are excluded,
        // even if they are due in the past

        // Arrange
        using var context = CreateContext();
        var repository = new TesterReportRepository(context);
        var today = DateTime.Today;
        var testData = CreateTestDataSet(today);

        // Mark the past test as complete
        testData.TestSetDayPast.IsComplete = true;

        await SeedTestDataAsync(context, testData);

        // Act
        var result = await repository.GetTestsDuePastAsync();

        // Assert
        Assert.Empty(result); // Should return no results since the past test is complete
    }

    [Fact]
    public async Task GetTestsDuePastAsync_ExcludesTestsDueToday()
    {
        // Verify that tests due today (DateDue == today) are not included in past tests

        // Arrange
        using var context = CreateContext();
        var repository = new TesterReportRepository(context);
        var today = DateTime.Today;
        var testData = CreateTestDataSet(today);
        await SeedTestDataAsync(context, testData);

        // Act
        var result = await repository.GetTestsDuePastAsync();

        // Assert - Should not include today's tests
        Assert.DoesNotContain(result, t => t.TestCylinderCode == "TODAY-1-1");
        Assert.DoesNotContain(result, t => t.TestCylinderCode == "TODAY-2-1");
    }

    [Fact]
    public async Task GetTestsDuePastAsync_ExcludesTestsDueFuture()
    {
        // Verify that tests due in the future (DateDue > today) are not included in past tests

        // Arrange
        using var context = CreateContext();
        var repository = new TesterReportRepository(context);
        var today = DateTime.Today;
        var testData = CreateTestDataSet(today);
        await SeedTestDataAsync(context, testData);

        // Act
        var result = await repository.GetTestsDuePastAsync();

        // Assert - Should not include future tests
        Assert.DoesNotContain(result, t => t.TestCylinderCode == "FUTURE-1-28");
    }

    [Fact]
    public async Task GetTestsDuePastAsync_ReturnsCorrectProjectionData()
    {
        // Verify that all fields in TestCylinderQueueProjection are correctly populated
        // from the joined entities (TestCylinder, TestSetDay, TestSet, Placement, etc.)

        // Arrange
        using var context = CreateContext();
        var repository = new TesterReportRepository(context);
        var today = DateTime.Today;
        var testData = CreateTestDataSet(today);
        await SeedTestDataAsync(context, testData);

        // Act
        var result = await repository.GetTestsDuePastAsync();
        var projection = result.First();

        // Assert - Verify all projection fields are correctly populated
        Assert.Equal("PAST-1-7", projection.TestCylinderCode);
        Assert.Equal("Oven1", projection.OvenId);
        Assert.Equal(7, projection.DayNum);
        Assert.Equal(today.AddDays(-14), projection.CastDate);
        Assert.Equal(new TimeSpan(8, 0, 0), projection.CastTime);
        Assert.Equal("25-001", projection.JobCode);
        Assert.Equal("Test Job 1", projection.JobName);
        Assert.Equal("MIX-1", projection.MixDesignCode);
        Assert.Equal(3500, projection.RequiredPsi);
        Assert.Equal("Walls", projection.PieceType);
        Assert.False(projection.IsComplete);
    }

    #endregion

    #region GetTestsDueTodayAsync Tests

    [Fact]
    public async Task GetTestsDueTodayAsync_ReturnsOnlyTestsDueToday()
    {
        // Verify that GetTestsDueTodayAsync returns all tests due today,
        // regardless of completion status

        // Arrange
        using var context = CreateContext();
        var repository = new TesterReportRepository(context);
        var today = DateTime.Today;
        var testData = CreateTestDataSet(today);
        await SeedTestDataAsync(context, testData);

        // Act
        var result = await repository.GetTestsDueTodayAsync();
        var resultList = result.ToList();

        // Assert
        Assert.Equal(2, resultList.Count); // Two tests due today
        Assert.Contains(resultList, t => t.TestCylinderCode == "TODAY-1-1");
        Assert.Contains(resultList, t => t.TestCylinderCode == "TODAY-2-1");
    }

    [Fact]
    public async Task GetTestsDueTodayAsync_IncludesCompletedTests()
    {
        // Verify that GetTestsDueTodayAsync includes both completed and incomplete tests.
        // Unlike GetTestsDuePastAsync, today's report should show all tests regardless of status.

        // Arrange
        using var context = CreateContext();
        var repository = new TesterReportRepository(context);
        var today = DateTime.Today;
        var testData = CreateTestDataSet(today);

        // Mark one of today's tests as complete
        testData.TestSetDayToday1.IsComplete = true;

        await SeedTestDataAsync(context, testData);

        // Act
        var result = await repository.GetTestsDueTodayAsync();
        var resultList = result.ToList();

        // Assert - Both complete and incomplete tests should be included
        Assert.Equal(2, resultList.Count);
        Assert.Contains(resultList, t => t.TestCylinderCode == "TODAY-1-1" && t.IsComplete);
        Assert.Contains(resultList, t => t.TestCylinderCode == "TODAY-2-1" && !t.IsComplete);
    }

    [Fact]
    public async Task GetTestsDueTodayAsync_ExcludesTestsDuePast()
    {
        // Verify that tests due before today (DateDue < today) are not included

        // Arrange
        using var context = CreateContext();
        var repository = new TesterReportRepository(context);
        var today = DateTime.Today;
        var testData = CreateTestDataSet(today);
        await SeedTestDataAsync(context, testData);

        // Act
        var result = await repository.GetTestsDueTodayAsync();

        // Assert
        Assert.DoesNotContain(result, t => t.TestCylinderCode == "PAST-1-7");
    }

    [Fact]
    public async Task GetTestsDueTodayAsync_ExcludesTestsDueFuture()
    {
        // Verify that tests due after today (DateDue > today) are not included

        // Arrange
        using var context = CreateContext();
        var repository = new TesterReportRepository(context);
        var today = DateTime.Today;
        var testData = CreateTestDataSet(today);
        await SeedTestDataAsync(context, testData);

        // Act
        var result = await repository.GetTestsDueTodayAsync();

        // Assert
        Assert.DoesNotContain(result, t => t.TestCylinderCode == "FUTURE-1-28");
    }

    [Fact]
    public async Task GetTestsDueTodayAsync_ReturnsCorrectProjectionData()
    {
        // Verify that all fields in TestCylinderQueueProjection are correctly populated
        // from the joined entities for tests due today

        // Arrange
        using var context = CreateContext();
        var repository = new TesterReportRepository(context);
        var today = DateTime.Today;
        var testData = CreateTestDataSet(today);
        await SeedTestDataAsync(context, testData);

        // Act
        var result = await repository.GetTestsDueTodayAsync();
        var projection = result.First(p => p.TestCylinderCode == "TODAY-1-1");

        // Assert - Verify all projection fields are correctly populated
        Assert.Equal("TODAY-1-1", projection.TestCylinderCode);
        Assert.Equal("Oven2", projection.OvenId);
        Assert.Equal(1, projection.DayNum);
        Assert.Equal(today.AddDays(-1), projection.CastDate);
        Assert.Equal(new TimeSpan(9, 30, 0), projection.CastTime);
        Assert.Equal("25-002", projection.JobCode);
        Assert.Equal("Test Job 2", projection.JobName);
        Assert.Equal("MIX-2", projection.MixDesignCode);
        Assert.Equal(4000, projection.RequiredPsi);
        Assert.Equal("Tees", projection.PieceType);
    }

    #endregion

    #region GetTestsDueBetweenDatesAsync Tests

    [Fact]
    public async Task GetTestsDueBetweenDatesAsync_ReturnsTestsWithinDateRange()
    {
        // Verify that GetTestsDueBetweenDatesAsync returns only tests where
        // DateDue is within the specified date range (inclusive)

        // Arrange
        using var context = CreateContext();
        var repository = new TesterReportRepository(context);
        var today = DateTime.Today;
        var testData = CreateTestDataSet(today);
        await SeedTestDataAsync(context, testData);

        var startDate = today.AddDays(5);
        var endDate = today.AddDays(35);

        // Act
        var result = await repository.GetTestsDueBetweenDatesAsync(startDate, endDate);
        var resultList = result.ToList();

        // Assert
        Assert.Single(resultList); // Only the future test (28 days out)
        Assert.Equal("FUTURE-1-28", resultList[0].TestCylinderCode);
    }

    [Fact]
    public async Task GetTestsDueBetweenDatesAsync_IncludesCompletedTests()
    {
        // Verify that GetTestsDueBetweenDatesAsync includes both completed and incomplete tests.
        // This method returns all tests in range regardless of completion status.

        // Arrange
        using var context = CreateContext();
        var repository = new TesterReportRepository(context);
        var today = DateTime.Today;
        var testData = CreateTestDataSet(today);

        // Mark future test as complete
        testData.TestSetDayFuture.IsComplete = true;

        await SeedTestDataAsync(context, testData);

        var startDate = today.AddDays(5);
        var endDate = today.AddDays(35);

        // Act
        var result = await repository.GetTestsDueBetweenDatesAsync(startDate, endDate);
        var resultList = result.ToList();

        // Assert - Should include completed test
        Assert.Single(resultList);
        Assert.True(resultList[0].IsComplete);
    }

    [Fact]
    public async Task GetTestsDueBetweenDatesAsync_ExcludesTestsOutsideRange()
    {
        // Verify that tests with DateDue outside the specified range are not included

        // Arrange
        using var context = CreateContext();
        var repository = new TesterReportRepository(context);
        var today = DateTime.Today;
        var testData = CreateTestDataSet(today);
        await SeedTestDataAsync(context, testData);

        var startDate = today.AddDays(5);
        var endDate = today.AddDays(35);

        // Act
        var result = await repository.GetTestsDueBetweenDatesAsync(startDate, endDate);

        // Assert - Should not include today's tests or past tests
        Assert.DoesNotContain(result, t => t.TestCylinderCode == "TODAY-1-1");
        Assert.DoesNotContain(result, t => t.TestCylinderCode == "TODAY-2-1");
        Assert.DoesNotContain(result, t => t.TestCylinderCode == "PAST-1-7");
    }

    [Fact]
    public async Task GetTestsDueBetweenDatesAsync_IncludesTestsOnBoundaryDates()
    {
        // Verify that the date range is inclusive on both boundaries.
        // Tests with DateDue exactly equal to startDate or endDate should be included.

        // Arrange
        using var context = CreateContext();
        var repository = new TesterReportRepository(context);
        var today = DateTime.Today;
        var testData = CreateTestDataSet(today);
        await SeedTestDataAsync(context, testData);

        // Test boundary: include test due exactly on start date and end date
        var startDate = today; // Today's tests should be included
        var endDate = today; // Only today's tests

        // Act
        var result = await repository.GetTestsDueBetweenDatesAsync(startDate, endDate);
        var resultList = result.ToList();

        // Assert
        Assert.Equal(2, resultList.Count);
        Assert.Contains(resultList, t => t.TestCylinderCode == "TODAY-1-1");
        Assert.Contains(resultList, t => t.TestCylinderCode == "TODAY-2-1");
    }

    [Fact]
    public async Task GetTestsDueBetweenDatesAsync_ReturnsCorrectProjectionData()
    {
        // Verify that all fields in TestCylinderQueueProjection are correctly populated
        // from the joined entities for tests in the date range

        // Arrange
        using var context = CreateContext();
        var repository = new TesterReportRepository(context);
        var today = DateTime.Today;
        var testData = CreateTestDataSet(today);
        await SeedTestDataAsync(context, testData);

        var startDate = today.AddDays(5);
        var endDate = today.AddDays(35);

        // Act
        var result = await repository.GetTestsDueBetweenDatesAsync(startDate, endDate);
        var projection = result.First();

        // Assert - Verify all projection fields are correctly populated
        Assert.Equal("FUTURE-1-28", projection.TestCylinderCode);
        Assert.Equal("Oven3", projection.OvenId);
        Assert.Equal(28, projection.DayNum);
        Assert.Equal(today, projection.CastDate);
        Assert.Equal(new TimeSpan(10, 15, 0), projection.CastTime);
        Assert.Equal("25-003", projection.JobCode);
        Assert.Equal("Test Job 3", projection.JobName);
        Assert.Equal("MIX-3", projection.MixDesignCode);
        Assert.Equal(5000, projection.RequiredPsi);
        Assert.Equal("Slabs", projection.PieceType);
    }

    [Fact]
    public async Task GetTestsDueBetweenDatesAsync_ReturnsEmptyWhenNoTestsInRange()
    {
        // Verify that an empty list is returned when no tests have DateDue
        // within the specified date range

        // Arrange
        using var context = CreateContext();
        var repository = new TesterReportRepository(context);
        var today = DateTime.Today;
        var testData = CreateTestDataSet(today);
        await SeedTestDataAsync(context, testData);

        var startDate = today.AddDays(50);
        var endDate = today.AddDays(60);

        // Act
        var result = await repository.GetTestsDueBetweenDatesAsync(startDate, endDate);

        // Assert
        Assert.Empty(result);
    }

    #endregion

    #region Helper Methods

    private class TestDataSet
    {
        public ProductionDay ProductionDayPast { get; set; } = null!;
        public ProductionDay ProductionDayToday { get; set; } = null!;
        public ProductionDay ProductionDayFuture { get; set; } = null!;

        public Job Job1 { get; set; } = null!;
        public Job Job2 { get; set; } = null!;
        public Job Job3 { get; set; } = null!;

        public Bed Bed1 { get; set; } = null!;

        public MixDesign MixDesign1 { get; set; } = null!;
        public MixDesign MixDesign2 { get; set; } = null!;
        public MixDesign MixDesign3 { get; set; } = null!;

        public MixDesignRequirement MixDesignReq1Day1 { get; set; } = null!;
        public MixDesignRequirement MixDesignReq1Day7 { get; set; } = null!;
        public MixDesignRequirement MixDesignReq1Day28 { get; set; } = null!;
        public MixDesignRequirement MixDesignReq2Day1 { get; set; } = null!;
        public MixDesignRequirement MixDesignReq2Day7 { get; set; } = null!;
        public MixDesignRequirement MixDesignReq2Day28 { get; set; } = null!;
        public MixDesignRequirement MixDesignReq3Day1 { get; set; } = null!;
        public MixDesignRequirement MixDesignReq3Day7 { get; set; } = null!;
        public MixDesignRequirement MixDesignReq3Day28 { get; set; } = null!;

        public Pour Pour1 { get; set; } = null!;
        public Pour Pour2 { get; set; } = null!;
        public Pour Pour3 { get; set; } = null!;

        public MixBatch MixBatch1 { get; set; } = null!;
        public MixBatch MixBatch2 { get; set; } = null!;
        public MixBatch MixBatch3 { get; set; } = null!;

        public Placement Placement1 { get; set; } = null!;
        public Placement Placement2 { get; set; } = null!;
        public Placement Placement3 { get; set; } = null!;

        public TestSet TestSet1 { get; set; } = null!;
        public TestSet TestSet2 { get; set; } = null!;
        public TestSet TestSet3 { get; set; } = null!;

        public TestSetDay TestSetDayPast { get; set; } = null!;
        public TestSetDay TestSetDayToday1 { get; set; } = null!;
        public TestSetDay TestSetDayToday2 { get; set; } = null!;
        public TestSetDay TestSetDayFuture { get; set; } = null!;

        public TestCylinder TestCylinderPast { get; set; } = null!;
        public TestCylinder TestCylinderToday1 { get; set; } = null!;
        public TestCylinder TestCylinderToday2 { get; set; } = null!;
        public TestCylinder TestCylinderFuture { get; set; } = null!;
    }

    private TestDataSet CreateTestDataSet(DateTime today)
    {
        var data = new TestDataSet();

        // Create Production Days
        data.ProductionDayPast = new ProductionDay { Date = today.AddDays(-14) };
        data.ProductionDayToday = new ProductionDay { Date = today.AddDays(-1) };
        data.ProductionDayFuture = new ProductionDay { Date = today };

        // Create Jobs
        data.Job1 = new Job { Code = "25-001", Name = "Test Job 1" };
        data.Job2 = new Job { Code = "25-002", Name = "Test Job 2" };
        data.Job3 = new Job { Code = "25-003", Name = "Test Job 3" };

        // Create Bed
        data.Bed1 = new Bed();

        // Create Mix Designs
        data.MixDesign1 = new MixDesign { Code = "MIX-1" };
        data.MixDesign2 = new MixDesign { Code = "MIX-2" };
        data.MixDesign3 = new MixDesign { Code = "MIX-3" };

        // Create Mix Design Requirements
        data.MixDesignReq1Day1 = new MixDesignRequirement { TestType = 1, RequiredPsi = 2500 };
        data.MixDesignReq1Day7 = new MixDesignRequirement { TestType = 7, RequiredPsi = 3500 };
        data.MixDesignReq1Day28 = new MixDesignRequirement { TestType = 28, RequiredPsi = 4500 };

        data.MixDesignReq2Day1 = new MixDesignRequirement { TestType = 1, RequiredPsi = 4000 };
        data.MixDesignReq2Day7 = new MixDesignRequirement { TestType = 7, RequiredPsi = 4500 };
        data.MixDesignReq2Day28 = new MixDesignRequirement { TestType = 28, RequiredPsi = 5000 };

        data.MixDesignReq3Day1 = new MixDesignRequirement { TestType = 1, RequiredPsi = 3000 };
        data.MixDesignReq3Day7 = new MixDesignRequirement { TestType = 7, RequiredPsi = 4000 };
        data.MixDesignReq3Day28 = new MixDesignRequirement { TestType = 28, RequiredPsi = 5000 };

        data.MixDesign1.MixDesignRequirements = new List<MixDesignRequirement>
        {
            data.MixDesignReq1Day1,
            data.MixDesignReq1Day7,
            data.MixDesignReq1Day28
        };
        data.MixDesign2.MixDesignRequirements = new List<MixDesignRequirement>
        {
            data.MixDesignReq2Day1,
            data.MixDesignReq2Day7,
            data.MixDesignReq2Day28
        };
        data.MixDesign3.MixDesignRequirements = new List<MixDesignRequirement>
        {
            data.MixDesignReq3Day1,
            data.MixDesignReq3Day7,
            data.MixDesignReq3Day28
        };

        // Create Pours
        data.Pour1 = new Pour { Job = data.Job1, Bed = data.Bed1 };
        data.Pour2 = new Pour { Job = data.Job2, Bed = data.Bed1 };
        data.Pour3 = new Pour { Job = data.Job3, Bed = data.Bed1 };

        // Create Mix Batches
        data.MixBatch1 = new MixBatch { ProductionDay = data.ProductionDayPast, MixDesign = data.MixDesign1 };
        data.MixBatch2 = new MixBatch { ProductionDay = data.ProductionDayToday, MixDesign = data.MixDesign2 };
        data.MixBatch3 = new MixBatch { ProductionDay = data.ProductionDayFuture, MixDesign = data.MixDesign3 };

        // Create Placements
        data.Placement1 = new Placement
        {
            Pour = data.Pour1,
            MixBatch = data.MixBatch1,
            StartTime = new TimeSpan(8, 0, 0),
            OvenId = "Oven1",
            PieceType = "Walls",
            Volume = 10.5m
        };
        data.Placement2 = new Placement
        {
            Pour = data.Pour2,
            MixBatch = data.MixBatch2,
            StartTime = new TimeSpan(9, 30, 0),
            OvenId = "Oven2",
            PieceType = "Tees",
            Volume = 15.2m
        };
        data.Placement3 = new Placement
        {
            Pour = data.Pour3,
            MixBatch = data.MixBatch3,
            StartTime = new TimeSpan(10, 15, 0),
            OvenId = "Oven3",
            PieceType = "Slabs",
            Volume = 20.8m
        };

        // Create Test Sets
        data.TestSet1 = new TestSet { Placement = data.Placement1 };
        data.TestSet2 = new TestSet { Placement = data.Placement2 };
        data.TestSet3 = new TestSet { Placement = data.Placement3 };

        // Create Test Set Days
        // Past test: 7-day test from 14 days ago, due 7 days ago
        data.TestSetDayPast = new TestSetDay
        {
            TestSet = data.TestSet1,
            DayNum = 7,
            DateDue = today.AddDays(-7),
            IsComplete = false
        };

        // Today test 1: 1-day test from yesterday, due today
        data.TestSetDayToday1 = new TestSetDay
        {
            TestSet = data.TestSet2,
            DayNum = 1,
            DateDue = today,
            IsComplete = false
        };

        // Today test 2: Another 1-day test from yesterday, due today (different placement)
        data.TestSetDayToday2 = new TestSetDay
        {
            TestSet = data.TestSet2,
            DayNum = 1,
            DateDue = today,
            IsComplete = false
        };

        // Future test: 28-day test from today, due in 28 days
        data.TestSetDayFuture = new TestSetDay
        {
            TestSet = data.TestSet3,
            DayNum = 28,
            DateDue = today.AddDays(28),
            IsComplete = false
        };

        // Create Test Cylinders
        data.TestCylinderPast = new TestCylinder
        {
            Code = "PAST-1-7",
            TestSetDay = data.TestSetDayPast
        };

        data.TestCylinderToday1 = new TestCylinder
        {
            Code = "TODAY-1-1",
            TestSetDay = data.TestSetDayToday1
        };

        data.TestCylinderToday2 = new TestCylinder
        {
            Code = "TODAY-2-1",
            TestSetDay = data.TestSetDayToday2
        };

        data.TestCylinderFuture = new TestCylinder
        {
            Code = "FUTURE-1-28",
            TestSetDay = data.TestSetDayFuture
        };

        return data;
    }

    private async Task SeedTestDataAsync(ApplicationDbContext context, TestDataSet data)
    {
        // Add in proper order to satisfy foreign key constraints
        context.ProductionDays.AddRange(data.ProductionDayPast, data.ProductionDayToday, data.ProductionDayFuture);
        context.Jobs.AddRange(data.Job1, data.Job2, data.Job3);
        context.Beds.Add(data.Bed1);
        context.MixDesigns.AddRange(data.MixDesign1, data.MixDesign2, data.MixDesign3);
        await context.SaveChangesAsync();

        // MixDesignRequirements are already tracked through MixDesign navigation properties
        // No need to add them explicitly
        context.Pours.AddRange(data.Pour1, data.Pour2, data.Pour3);
        context.MixBatches.AddRange(data.MixBatch1, data.MixBatch2, data.MixBatch3);
        await context.SaveChangesAsync();

        context.Placements.AddRange(data.Placement1, data.Placement2, data.Placement3);
        await context.SaveChangesAsync();

        context.TestSets.AddRange(data.TestSet1, data.TestSet2, data.TestSet3);
        await context.SaveChangesAsync();

        context.TestSetDays.AddRange(
            data.TestSetDayPast,
            data.TestSetDayToday1,
            data.TestSetDayToday2,
            data.TestSetDayFuture
        );
        await context.SaveChangesAsync();

        context.TestCylinders.AddRange(
            data.TestCylinderPast,
            data.TestCylinderToday1,
            data.TestCylinderToday2,
            data.TestCylinderFuture
        );
        await context.SaveChangesAsync();
    }

    #endregion
}
