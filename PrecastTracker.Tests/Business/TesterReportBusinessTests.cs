using Microsoft.Extensions.Logging;
using Moq;
using PrecastTracker.Business;
using PrecastTracker.Data;
using PrecastTracker.Data.Projections;
using PrecastTracker.Data.Repositories;
using PrecastTracker.Services;

namespace PrecastTracker.Tests.Business;

public class TesterReportBusinessTests
{
    [Fact]
    public async Task GetTestsUpcomingAsync_UsesCorrectDateBoundaries()
    {
        // Verify that GetTestsUpcomingAsync uses:
        // - Start date: beginning of tomorrow (00:00:00)
        // - End date: end of the last day (23:59:59.999)

        // Arrange
        var mockRepository = new Mock<ITesterReportRepository>();
        var mockTestSetDayRepository = new Mock<ITestSetDayRepository>();
        var mockTestResultService = new Mock<ITestResultService>();
        var mockLogger = new Mock<ILogger<TesterReportBusiness>>();
        var business = new TesterReportBusiness(
            mockRepository.Object,
            mockTestSetDayRepository.Object,
            mockTestResultService.Object,
            null!, // DbContext not used in this test
            mockLogger.Object);

        var days = 7;
        var today = DateTime.Today;
        var expectedStartDate = today.AddDays(1); // Tomorrow at 00:00:00
        var expectedEndDate = today.AddDays(days).AddDays(1).AddTicks(-1); // End of day 7 (23:59:59.999...)

        mockRepository
            .Setup(r => r.GetTestsDueBetweenDatesAsync(
                expectedStartDate,
                expectedEndDate))
            .ReturnsAsync(new List<TestCylinderQueueProjection>());

        // Act
        await business.GetTestsUpcomingAsync(days);

        // Assert
        mockRepository.Verify(
            r => r.GetTestsDueBetweenDatesAsync(
                expectedStartDate,
                expectedEndDate),
            Times.Once);
    }

    [Fact]
    public async Task GetTestsDueTodayAsync_MapsProjectionToDtoCorrectly()
    {
        // Verify that all properties are correctly mapped from
        // TestCylinderQueueProjection to TestCylinderQueueResponse

        // Arrange
        var mockRepository = new Mock<ITesterReportRepository>();
        var mockTestSetDayRepository = new Mock<ITestSetDayRepository>();
        var mockTestResultService = new Mock<ITestResultService>();
        var mockLogger = new Mock<ILogger<TesterReportBusiness>>();
        var business = new TesterReportBusiness(
            mockRepository.Object,
            mockTestSetDayRepository.Object,
            mockTestResultService.Object,
            null!, // DbContext not used in this test
            mockLogger.Object);

        var today = DateTime.Today;
        var projections = new List<TestCylinderQueueProjection>
        {
            new TestCylinderQueueProjection
            {
                TestCylinderCode = "TEST-123",
                OvenId = "Oven1",
                DayNum = 7,
                CastDate = today.AddDays(-7),
                CastTime = new TimeSpan(14, 30, 45),
                JobCode = "25-100",
                JobName = "Test Job Name",
                MixDesignCode = "MIX-500",
                RequiredPsi = 5500,
                PieceType = "Beams",
                TestSetId = 42,
                DateDue = today
            }
        };

        mockRepository
            .Setup(r => r.GetTestsDueTodayAsync())
            .ReturnsAsync(projections);

        // Act
        var result = await business.GetTestsDueTodayAsync();

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);

        var response = result.Data.First();
        Assert.Equal("TEST-123", response.TestCylinderCode);
        Assert.Equal("Oven1", response.OvenId);
        Assert.Equal(7, response.DayNum);
        Assert.Equal(today.AddDays(-7), response.CastDate);
        Assert.Equal(new TimeSpan(14, 30, 45), response.CastTime);
        Assert.Equal("25-100", response.JobCode);
        Assert.Equal("Test Job Name", response.JobName);
        Assert.Equal("MIX-500", response.MixDesignCode);
        Assert.Equal(5500, response.RequiredPsi);
        Assert.Equal("Beams", response.PieceType);
        Assert.Equal(42, response.TestSetId);
        Assert.Equal(today, response.DateDue);
    }
}
