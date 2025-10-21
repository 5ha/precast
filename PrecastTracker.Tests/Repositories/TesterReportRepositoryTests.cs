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

    #region GetUntestedPlacementsAsync Tests

    [Fact]
    public async Task GetUntestedPlacementsAsync_ReturnsOnlyPlacementsWithStartTimeAndNoTestSets()
    {
        // Verify that GetUntestedPlacementsAsync returns placements that:
        // 1. Have StartTime set (not null)
        // 2. Have no TestSets

        // Arrange
        using var context = CreateContext();
        var repository = new TesterReportRepository(context);
        var today = DateTime.Today;
        var testData = CreateUntestedPlacementsTestData(today);
        await SeedUntestedPlacementsTestDataAsync(context, testData);

        // Act
        var result = await repository.GetUntestedPlacementsAsync(7);
        var resultList = result.ToList();

        // Assert
        Assert.Single(resultList); // Only one placement with StartTime and no TestSets
        Assert.Equal(testData.PlacementUntested.PlacementId, resultList[0].PlacementId);
        Assert.Equal("UNTESTED", resultList[0].JobCode);
    }

    [Fact]
    public async Task GetUntestedPlacementsAsync_ExcludesPlacementsWithoutStartTime()
    {
        // Verify that placements without StartTime (null) are excluded,
        // even if they have no TestSets

        // Arrange
        using var context = CreateContext();
        var repository = new TesterReportRepository(context);
        var today = DateTime.Today;
        var testData = CreateUntestedPlacementsTestData(today);
        await SeedUntestedPlacementsTestDataAsync(context, testData);

        // Act
        var result = await repository.GetUntestedPlacementsAsync(7);

        // Assert - Should not include placement without StartTime
        Assert.DoesNotContain(result, p => p.PlacementId == testData.PlacementNoStartTime.PlacementId);
    }

    [Fact]
    public async Task GetUntestedPlacementsAsync_ExcludesPlacementsWithTestSets()
    {
        // Verify that placements with TestSets are excluded,
        // even if they have StartTime set

        // Arrange
        using var context = CreateContext();
        var repository = new TesterReportRepository(context);
        var today = DateTime.Today;
        var testData = CreateUntestedPlacementsTestData(today);
        await SeedUntestedPlacementsTestDataAsync(context, testData);

        // Act
        var result = await repository.GetUntestedPlacementsAsync(7);

        // Assert - Should not include placements with TestSets
        Assert.DoesNotContain(result, p => p.PlacementId == testData.PlacementWithTestSet.PlacementId);
    }

    [Fact]
    public async Task GetUntestedPlacementsAsync_ExcludesPlacementsOlderThanDaysBack()
    {
        // Verify that placements cast before the cutoff date are excluded

        // Arrange
        using var context = CreateContext();
        var repository = new TesterReportRepository(context);
        var today = DateTime.Today;
        var testData = CreateUntestedPlacementsTestData(today);
        await SeedUntestedPlacementsTestDataAsync(context, testData);

        // Act - Only look back 5 days
        var result = await repository.GetUntestedPlacementsAsync(5);

        // Assert - Should not include placement from 10 days ago
        Assert.DoesNotContain(result, p => p.PlacementId == testData.PlacementOld.PlacementId);
    }

    [Fact]
    public async Task GetUntestedPlacementsAsync_IncludesPlacementsOnCutoffDate()
    {
        // Verify that placements cast exactly on the cutoff date are included

        // Arrange
        using var context = CreateContext();
        var repository = new TesterReportRepository(context);
        var today = DateTime.Today;
        var testData = CreateUntestedPlacementsTestData(today);
        await SeedUntestedPlacementsTestDataAsync(context, testData);

        // Act - Look back exactly 3 days (should include placement from 3 days ago)
        var result = await repository.GetUntestedPlacementsAsync(3);

        // Assert - Should include placement from exactly 3 days ago
        Assert.Contains(result, p => p.PlacementId == testData.PlacementUntested.PlacementId);
    }

    [Fact]
    public async Task GetUntestedPlacementsAsync_ReturnsCorrectProjectionData()
    {
        // Verify that all fields in UntestedPlacementProjection are correctly populated
        // from the joined entities (Placement, Pour, Job, MixBatch, etc.)

        // Arrange
        using var context = CreateContext();
        var repository = new TesterReportRepository(context);
        var today = DateTime.Today;
        var testData = CreateUntestedPlacementsTestData(today);
        await SeedUntestedPlacementsTestDataAsync(context, testData);

        // Act
        var result = await repository.GetUntestedPlacementsAsync(7);
        var projection = result.First();

        // Assert - Verify all projection fields are correctly populated
        Assert.Equal(testData.PourUntested.PourId, projection.PourId);
        Assert.Equal(testData.PlacementUntested.PlacementId, projection.PlacementId);
        Assert.Equal(today.AddDays(-3), projection.CastDate);
        Assert.Equal(new TimeSpan(14, 30, 0), projection.CastTime);
        Assert.Equal("UNTESTED", projection.JobCode);
        Assert.Equal("Untested Job", projection.JobName);
        Assert.Equal("MIX-UNTESTED", projection.MixDesignCode);
        Assert.Equal("Walls", projection.PieceType);
        Assert.Equal(12.5m, projection.Volume);
    }

    [Fact]
    public async Task GetUntestedPlacementsAsync_ReturnsEmptyWhenNoUntestedPlacements()
    {
        // Verify that an empty list is returned when all placements either:
        // - Have no StartTime, or
        // - Have TestSets, or
        // - Are older than the cutoff

        // Arrange
        using var context = CreateContext();
        var repository = new TesterReportRepository(context);
        var today = DateTime.Today;
        var testData = CreateUntestedPlacementsTestData(today);
        await SeedUntestedPlacementsTestDataAsync(context, testData);

        // Act - Look back only 1 day (untested placement is 3 days ago)
        var result = await repository.GetUntestedPlacementsAsync(1);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetUntestedPlacementsAsync_ReturnsMultipleUntestedPlacements()
    {
        // Verify that multiple untested placements are returned when they exist

        // Arrange
        using var context = CreateContext();
        var repository = new TesterReportRepository(context);
        var today = DateTime.Today;

        // Create two untested placements
        var prodDay = new ProductionDay { Date = today.AddDays(-2) };
        var job = new Job { Code = "TEST", Name = "Test Job" };
        var bed = new Bed();
        var mixDesign = new MixDesign { Code = "MIX-1" };
        var pour = new Pour { Job = job, Bed = bed };
        var mixBatch = new MixBatch { ProductionDay = prodDay, MixDesign = mixDesign };

        var placement1 = new Placement
        {
            Pour = pour,
            MixBatch = mixBatch,
            StartTime = new TimeSpan(8, 0, 0),
            PieceType = "Walls",
            Volume = 10m
        };

        var placement2 = new Placement
        {
            Pour = pour,
            MixBatch = mixBatch,
            StartTime = new TimeSpan(10, 0, 0),
            PieceType = "Slabs",
            Volume = 15m
        };

        context.ProductionDays.Add(prodDay);
        context.Jobs.Add(job);
        context.Beds.Add(bed);
        context.MixDesigns.Add(mixDesign);
        await context.SaveChangesAsync();

        context.Pours.Add(pour);
        context.MixBatches.Add(mixBatch);
        await context.SaveChangesAsync();

        context.Placements.AddRange(placement1, placement2);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetUntestedPlacementsAsync(7);

        // Assert - Should return both untested placements
        Assert.Equal(2, result.Count);
        Assert.Contains(result, p => p.PieceType == "Walls");
        Assert.Contains(result, p => p.PieceType == "Slabs");
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

    private class UntestedPlacementsTestDataSet
    {
        public ProductionDay ProductionDayRecent { get; set; } = null!;
        public ProductionDay ProductionDayOld { get; set; } = null!;
        public ProductionDay ProductionDayPlanned { get; set; } = null!;

        public Job JobUntested { get; set; } = null!;
        public Job JobWithTests { get; set; } = null!;
        public Job JobNoStartTime { get; set; } = null!;
        public Job JobOld { get; set; } = null!;

        public Bed Bed1 { get; set; } = null!;

        public MixDesign MixDesignUntested { get; set; } = null!;
        public MixDesign MixDesignWithTests { get; set; } = null!;
        public MixDesign MixDesignNoStartTime { get; set; } = null!;
        public MixDesign MixDesignOld { get; set; } = null!;

        public Pour PourUntested { get; set; } = null!;
        public Pour PourWithTests { get; set; } = null!;
        public Pour PourNoStartTime { get; set; } = null!;
        public Pour PourOld { get; set; } = null!;

        public MixBatch MixBatchUntested { get; set; } = null!;
        public MixBatch MixBatchWithTests { get; set; } = null!;
        public MixBatch MixBatchNoStartTime { get; set; } = null!;
        public MixBatch MixBatchOld { get; set; } = null!;

        public Placement PlacementUntested { get; set; } = null!;
        public Placement PlacementWithTestSet { get; set; } = null!;
        public Placement PlacementNoStartTime { get; set; } = null!;
        public Placement PlacementOld { get; set; } = null!;

        public TestSet TestSet { get; set; } = null!;
    }

    private UntestedPlacementsTestDataSet CreateUntestedPlacementsTestData(DateTime today)
    {
        var data = new UntestedPlacementsTestDataSet();

        // Create Production Days
        data.ProductionDayRecent = new ProductionDay { Date = today.AddDays(-3) };
        data.ProductionDayOld = new ProductionDay { Date = today.AddDays(-10) };
        data.ProductionDayPlanned = new ProductionDay { Date = today.AddDays(-1) };

        // Create Jobs
        data.JobUntested = new Job { Code = "UNTESTED", Name = "Untested Job" };
        data.JobWithTests = new Job { Code = "TESTED", Name = "Tested Job" };
        data.JobNoStartTime = new Job { Code = "PLANNED", Name = "Planned Job" };
        data.JobOld = new Job { Code = "OLD", Name = "Old Job" };

        // Create Bed
        data.Bed1 = new Bed();

        // Create Mix Designs
        data.MixDesignUntested = new MixDesign { Code = "MIX-UNTESTED" };
        data.MixDesignWithTests = new MixDesign { Code = "MIX-TESTED" };
        data.MixDesignNoStartTime = new MixDesign { Code = "MIX-PLANNED" };
        data.MixDesignOld = new MixDesign { Code = "MIX-OLD" };

        // Create Pours
        data.PourUntested = new Pour { Job = data.JobUntested, Bed = data.Bed1 };
        data.PourWithTests = new Pour { Job = data.JobWithTests, Bed = data.Bed1 };
        data.PourNoStartTime = new Pour { Job = data.JobNoStartTime, Bed = data.Bed1 };
        data.PourOld = new Pour { Job = data.JobOld, Bed = data.Bed1 };

        // Create Mix Batches
        data.MixBatchUntested = new MixBatch { ProductionDay = data.ProductionDayRecent, MixDesign = data.MixDesignUntested };
        data.MixBatchWithTests = new MixBatch { ProductionDay = data.ProductionDayRecent, MixDesign = data.MixDesignWithTests };
        data.MixBatchNoStartTime = new MixBatch { ProductionDay = data.ProductionDayPlanned, MixDesign = data.MixDesignNoStartTime };
        data.MixBatchOld = new MixBatch { ProductionDay = data.ProductionDayOld, MixDesign = data.MixDesignOld };

        // Create Placements
        // 1. Untested placement: has StartTime, no TestSets, within date range
        data.PlacementUntested = new Placement
        {
            Pour = data.PourUntested,
            MixBatch = data.MixBatchUntested,
            StartTime = new TimeSpan(14, 30, 0),
            OvenId = "Oven1",
            PieceType = "Walls",
            Volume = 12.5m
        };

        // 2. Placement with TestSet: has StartTime and TestSets (should be excluded)
        data.PlacementWithTestSet = new Placement
        {
            Pour = data.PourWithTests,
            MixBatch = data.MixBatchWithTests,
            StartTime = new TimeSpan(9, 0, 0),
            OvenId = "Oven2",
            PieceType = "Tees",
            Volume = 15.0m
        };

        // 3. Placement without StartTime: no StartTime, no TestSets (should be excluded)
        data.PlacementNoStartTime = new Placement
        {
            Pour = data.PourNoStartTime,
            MixBatch = data.MixBatchNoStartTime,
            StartTime = null, // Null StartTime
            OvenId = "Oven3",
            PieceType = "Slabs",
            Volume = 20.0m
        };

        // 4. Old placement: has StartTime, no TestSets, but too old (should be excluded when daysBack < 10)
        data.PlacementOld = new Placement
        {
            Pour = data.PourOld,
            MixBatch = data.MixBatchOld,
            StartTime = new TimeSpan(11, 0, 0),
            OvenId = "Oven4",
            PieceType = "Beams",
            Volume = 8.5m
        };

        // Create TestSet for PlacementWithTestSet
        data.TestSet = new TestSet { Placement = data.PlacementWithTestSet };

        return data;
    }

    private async Task SeedUntestedPlacementsTestDataAsync(ApplicationDbContext context, UntestedPlacementsTestDataSet data)
    {
        // Add in proper order to satisfy foreign key constraints
        context.ProductionDays.AddRange(data.ProductionDayRecent, data.ProductionDayOld, data.ProductionDayPlanned);
        context.Jobs.AddRange(data.JobUntested, data.JobWithTests, data.JobNoStartTime, data.JobOld);
        context.Beds.Add(data.Bed1);
        context.MixDesigns.AddRange(data.MixDesignUntested, data.MixDesignWithTests, data.MixDesignNoStartTime, data.MixDesignOld);
        await context.SaveChangesAsync();

        context.Pours.AddRange(data.PourUntested, data.PourWithTests, data.PourNoStartTime, data.PourOld);
        context.MixBatches.AddRange(data.MixBatchUntested, data.MixBatchWithTests, data.MixBatchNoStartTime, data.MixBatchOld);
        await context.SaveChangesAsync();

        context.Placements.AddRange(data.PlacementUntested, data.PlacementWithTestSet, data.PlacementNoStartTime, data.PlacementOld);
        await context.SaveChangesAsync();

        context.TestSets.Add(data.TestSet);
        await context.SaveChangesAsync();
    }

    #endregion
}
