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

    #region GetTestQueueAsync Tests

    [Fact]
    public async Task GetTestQueueAsync_ReturnsOverdueUntestedAndTodayAndFutureAllTests()
    {
        // Verify that GetTestQueueAsync returns:
        // 1. Overdue tests that are NOT tested (DateDue < today AND DateTested == null)
        // 2. All tests due today (DateDue == today, regardless of DateTested)
        // 3. All tests due in future up to endDate (DateDue > today AND DateDue <= endDate, regardless of DateTested)

        // Arrange
        using var context = CreateContext();
        var repository = new TesterReportRepository(context);
        var today = DateTime.Today;
        var testData = CreateTestDataSet(today);
        await SeedTestDataAsync(context, testData);

        var endDate = today.AddDays(30);

        // Act
        var result = await repository.GetTestQueueAsync(endDate);
        var resultList = result.ToList();

        // Assert
        // Should include: 1 overdue untested + 2 today + 1 future = 4 total
        Assert.Equal(4, resultList.Count);
        Assert.Contains(resultList, t => t.TestCylinderCode == "PAST-1-7"); // Overdue, untested
        Assert.Contains(resultList, t => t.TestCylinderCode == "TODAY-1-1"); // Today
        Assert.Contains(resultList, t => t.TestCylinderCode == "TODAY-2-1"); // Today
        Assert.Contains(resultList, t => t.TestCylinderCode == "FUTURE-1-28"); // Future within range
    }

    [Fact]
    public async Task GetTestQueueAsync_ExcludesOverdueTestedTests()
    {
        // Verify that overdue tests that have been tested (DateTested is not null) are excluded

        // Arrange
        using var context = CreateContext();
        var repository = new TesterReportRepository(context);
        var today = DateTime.Today;
        var testData = CreateTestDataSet(today);

        // Mark the past test as tested
        testData.TestSetDayPast.DateTested = today.AddDays(-2);

        await SeedTestDataAsync(context, testData);

        var endDate = today.AddDays(30);

        // Act
        var result = await repository.GetTestQueueAsync(endDate);

        // Assert
        // Should NOT include the tested overdue test
        Assert.DoesNotContain(result, t => t.TestCylinderCode == "PAST-1-7");
        // Should still include today's tests and future tests
        Assert.Equal(3, result.Count); // 2 today + 1 future
    }

    [Fact]
    public async Task GetTestQueueAsync_IncludesTodayTestedTests()
    {
        // Verify that tests due today are included even if they have been tested

        // Arrange
        using var context = CreateContext();
        var repository = new TesterReportRepository(context);
        var today = DateTime.Today;
        var testData = CreateTestDataSet(today);

        // Mark one of today's tests as tested
        testData.TestSetDayToday1.DateTested = today;

        await SeedTestDataAsync(context, testData);

        var endDate = today.AddDays(30);

        // Act
        var result = await repository.GetTestQueueAsync(endDate);
        var resultList = result.ToList();

        // Assert - Should include both tested and untested tests for today
        Assert.Contains(resultList, t => t.TestCylinderCode == "TODAY-1-1" && t.DateTested == today);
        Assert.Contains(resultList, t => t.TestCylinderCode == "TODAY-2-1" && t.DateTested == null);
    }

    [Fact]
    public async Task GetTestQueueAsync_IncludesFutureTestedTests()
    {
        // Verify that future tests are included even if they have been tested

        // Arrange
        using var context = CreateContext();
        var repository = new TesterReportRepository(context);
        var today = DateTime.Today;
        var testData = CreateTestDataSet(today);

        // Mark future test as tested
        testData.TestSetDayFuture.DateTested = today.AddDays(29);

        await SeedTestDataAsync(context, testData);

        var endDate = today.AddDays(30);

        // Act
        var result = await repository.GetTestQueueAsync(endDate);

        // Assert - Should include tested future test
        Assert.Contains(result, t => t.TestCylinderCode == "FUTURE-1-28" && t.DateTested != null);
    }

    [Fact]
    public async Task GetTestQueueAsync_ExcludesTestsBeyondEndDate()
    {
        // Verify that tests with DateDue after endDate are not included

        // Arrange
        using var context = CreateContext();
        var repository = new TesterReportRepository(context);
        var today = DateTime.Today;
        var testData = CreateTestDataSet(today);
        await SeedTestDataAsync(context, testData);

        // Set endDate before the future test
        var endDate = today.AddDays(20);

        // Act
        var result = await repository.GetTestQueueAsync(endDate);

        // Assert - Should not include future test that's 28 days out
        Assert.DoesNotContain(result, t => t.TestCylinderCode == "FUTURE-1-28");
        // Should include overdue + today's tests
        Assert.Equal(3, result.Count); // 1 overdue + 2 today
    }

    [Fact]
    public async Task GetTestQueueAsync_IncludesTestsOnEndDateBoundary()
    {
        // Verify that tests with DateDue exactly equal to endDate are included

        // Arrange
        using var context = CreateContext();
        var repository = new TesterReportRepository(context);
        var today = DateTime.Today;
        var testData = CreateTestDataSet(today);
        await SeedTestDataAsync(context, testData);

        // Set endDate to exactly when future test is due
        var endDate = today.AddDays(28);

        // Act
        var result = await repository.GetTestQueueAsync(endDate);

        // Assert - Should include test due on boundary
        Assert.Contains(result, t => t.TestCylinderCode == "FUTURE-1-28");
    }

    [Fact]
    public async Task GetTestQueueAsync_ReturnsSortedByDateDue()
    {
        // Verify that results are sorted by DateDue ascending (most urgent first)

        // Arrange
        using var context = CreateContext();
        var repository = new TesterReportRepository(context);
        var today = DateTime.Today;
        var testData = CreateTestDataSet(today);
        await SeedTestDataAsync(context, testData);

        var endDate = today.AddDays(30);

        // Act
        var result = await repository.GetTestQueueAsync(endDate);
        var resultList = result.ToList();

        // Assert - Verify sort order
        Assert.Equal("PAST-1-7", resultList[0].TestCylinderCode); // Overdue (today -7)
        Assert.True(resultList[1].TestCylinderCode.StartsWith("TODAY")); // Today
        Assert.True(resultList[2].TestCylinderCode.StartsWith("TODAY")); // Today
        Assert.Equal("FUTURE-1-28", resultList[3].TestCylinderCode); // Future (today +28)
    }

    [Fact]
    public async Task GetTestQueueAsync_ReturnsCorrectProjectionDataIncludingDateTested()
    {
        // Verify that all fields in TestCylinderQueueProjection are correctly populated,
        // including the new DateTested field

        // Arrange
        using var context = CreateContext();
        var repository = new TesterReportRepository(context);
        var today = DateTime.Today;
        var testData = CreateTestDataSet(today);

        // Mark one test as tested
        var testedDate = today.AddDays(-1);
        testData.TestSetDayToday1.DateTested = testedDate;

        await SeedTestDataAsync(context, testData);

        var endDate = today.AddDays(30);

        // Act
        var result = await repository.GetTestQueueAsync(endDate);
        var testedProjection = result.First(p => p.TestCylinderCode == "TODAY-1-1");
        var untestedProjection = result.First(p => p.TestCylinderCode == "PAST-1-7");

        // Assert - Verify tested projection includes DateTested
        Assert.Equal("TODAY-1-1", testedProjection.TestCylinderCode);
        Assert.Equal(testedDate, testedProjection.DateTested);
        Assert.Equal("Oven2", testedProjection.OvenId);
        Assert.Equal(1, testedProjection.DayNum);
        Assert.Equal(today.AddDays(-1), testedProjection.CastDate);
        Assert.Equal(new TimeSpan(9, 30, 0), testedProjection.CastTime);
        Assert.Equal("25-002", testedProjection.JobCode);
        Assert.Equal("Test Job 2", testedProjection.JobName);
        Assert.Equal("MIX-2", testedProjection.MixDesignCode);
        Assert.Equal(4000, testedProjection.RequiredPsi);
        Assert.Equal("Tees", testedProjection.PieceType);
        Assert.Equal(today, testedProjection.DateDue);

        // Assert - Verify untested projection has null DateTested
        Assert.Equal("PAST-1-7", untestedProjection.TestCylinderCode);
        Assert.Null(untestedProjection.DateTested);
    }

    [Fact]
    public async Task GetTestQueueAsync_ReturnsEmptyWhenNoTestsInRange()
    {
        // Verify that an empty list is returned when no tests match the criteria

        // Arrange
        using var context = CreateContext();
        var repository = new TesterReportRepository(context);
        var today = DateTime.Today;
        var testData = CreateTestDataSet(today);

        // Mark all tests as tested
        testData.TestSetDayPast.DateTested = today.AddDays(-6);
        testData.TestSetDayToday1.DateTested = today;
        testData.TestSetDayToday2.DateTested = today;
        testData.TestSetDayFuture.DateTested = today.AddDays(29);

        await SeedTestDataAsync(context, testData);

        // Set endDate before all future tests
        var endDate = today.AddDays(-1);

        // Act
        var result = await repository.GetTestQueueAsync(endDate);

        // Assert
        Assert.Empty(result); // All tests either tested (past) or beyond endDate
    }

    [Fact]
    public async Task GetTestQueueItemAsync_ReturnsSingleItemByTestSetDayId()
    {
        // Verify that GetTestQueueItemAsync returns the correct projection for a specific TestSetDayId

        // Arrange
        using var context = CreateContext();
        var repository = new TesterReportRepository(context);
        var today = DateTime.Today;
        var testData = CreateTestDataSet(today);
        await SeedTestDataAsync(context, testData);

        // Act
        var result = await repository.GetTestQueueItemAsync(testData.TestSetDayToday1.TestSetDayId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("TODAY-1-1", result.TestCylinderCode);
        Assert.Equal(testData.TestSetDayToday1.TestSetDayId, result.TestSetDayId);
        Assert.Equal(1, result.DayNum);
        Assert.Equal("25-002", result.JobCode);
        Assert.Equal("Test Job 2", result.JobName);
        Assert.Equal("MIX-2", result.MixDesignCode);
        Assert.Equal(4000, result.RequiredPsi);
        Assert.Equal("Tees", result.PieceType);
        Assert.Equal(today, result.DateDue);
        Assert.Null(result.DateTested);
    }

    [Fact]
    public async Task GetTestQueueItemAsync_ReturnsNullWhenTestSetDayIdNotFound()
    {
        // Verify that GetTestQueueItemAsync returns null for non-existent TestSetDayId

        // Arrange
        using var context = CreateContext();
        var repository = new TesterReportRepository(context);
        var today = DateTime.Today;
        var testData = CreateTestDataSet(today);
        await SeedTestDataAsync(context, testData);

        // Act
        var result = await repository.GetTestQueueItemAsync(99999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetTestQueueItemAsync_ReturnsUpdatedDateTested()
    {
        // Verify that GetTestQueueItemAsync returns the updated DateTested value after save

        // Arrange
        using var context = CreateContext();
        var repository = new TesterReportRepository(context);
        var today = DateTime.Today;
        var testData = CreateTestDataSet(today);
        await SeedTestDataAsync(context, testData);

        // Mark the test as tested
        var testedDate = today.AddHours(10);
        testData.TestSetDayToday1.DateTested = testedDate;
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetTestQueueItemAsync(testData.TestSetDayToday1.TestSetDayId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(testedDate, result.DateTested);
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
        Assert.Equal(testData.PourUntested.BedId, projection.BedId);
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
            DateDue = today.AddDays(-7)
        };

        // Today test 1: 1-day test from yesterday, due today
        data.TestSetDayToday1 = new TestSetDay
        {
            TestSet = data.TestSet2,
            DayNum = 1,
            DateDue = today
        };

        // Today test 2: Another 1-day test from yesterday, due today (different placement)
        data.TestSetDayToday2 = new TestSetDay
        {
            TestSet = data.TestSet2,
            DayNum = 1,
            DateDue = today
        };

        // Future test: 28-day test from today, due in 28 days
        data.TestSetDayFuture = new TestSetDay
        {
            TestSet = data.TestSet3,
            DayNum = 28,
            DateDue = today.AddDays(28)
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
