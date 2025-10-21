using Microsoft.EntityFrameworkCore;
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
    public async Task GetTestQueueAsync_CallsRepositoryWithCorrectEndDate()
    {
        // Verify that GetTestQueueAsync passes the endDate parameter to the repository

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

        var endDate = DateTime.Today.AddDays(7);

        mockRepository
            .Setup(r => r.GetTestQueueAsync(endDate))
            .ReturnsAsync(new List<TestCylinderQueueProjection>());

        // Act
        await business.GetTestQueueAsync(endDate);

        // Assert
        mockRepository.Verify(
            r => r.GetTestQueueAsync(endDate),
            Times.Once);
    }

    [Fact]
    public async Task GetTestQueueAsync_MapsProjectionToDtoCorrectly()
    {
        // Verify that all properties including DateTested are correctly mapped from
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
        var testedDate = today.AddDays(-1);
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
                TestSetDayId = 100,
                DateDue = today,
                DateTested = testedDate
            }
        };

        mockRepository
            .Setup(r => r.GetTestQueueAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(projections);

        // Act
        var result = await business.GetTestQueueAsync(today.AddDays(7));

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
        Assert.Equal(100, response.TestSetDayId);
        Assert.Equal(today, response.DateDue);
        Assert.Equal(testedDate, response.DateTested);
    }

    [Fact]
    public async Task SaveTestSetDayDataAsync_ReturnsUpdatedTestCylinderQueueResponse()
    {
        // Verify that SaveTestSetDayDataAsync returns the updated TestCylinderQueueResponse
        // from the repository after saving

        // Arrange
        var mockRepository = new Mock<ITesterReportRepository>();
        var mockTestSetDayRepository = new Mock<ITestSetDayRepository>();
        var mockTestResultService = new Mock<ITestResultService>();
        var mockUnitOfWork = new Mock<IUnitOfWork>();
        var mockLogger = new Mock<ILogger<TesterReportBusiness>>();
        var business = new TesterReportBusiness(
            mockRepository.Object,
            mockTestSetDayRepository.Object,
            mockTestResultService.Object,
            mockUnitOfWork.Object,
            mockLogger.Object);

        var today = DateTime.Today;
        var testSetDayId = 100;
        var castDate = today.AddDays(-7);
        var testedDate = today;

        // Setup repository mocks
        mockTestSetDayRepository
            .Setup(r => r.GetCastDateAsync(testSetDayId))
            .ReturnsAsync(castDate);

        var updatedProjection = new TestCylinderQueueProjection
        {
            TestCylinderCode = "TEST-123",
            OvenId = "Oven1",
            DayNum = 7,
            CastDate = castDate,
            CastTime = new TimeSpan(14, 30, 45),
            JobCode = "25-100",
            JobName = "Test Job Name",
            MixDesignCode = "MIX-500",
            RequiredPsi = 5500,
            PieceType = "Beams",
            TestSetId = 42,
            TestSetDayId = testSetDayId,
            DateDue = today,
            DateTested = testedDate
        };

        mockRepository
            .Setup(r => r.GetTestQueueItemAsync(testSetDayId))
            .ReturnsAsync(updatedProjection);

        var request = new Contracts.DTOs.RequestResponse.SaveTestSetDayDataRequest
        {
            TestSetDayId = testSetDayId,
            DateTested = testedDate,
            Comments = "Test completed",
            CylinderBreaks = new List<Contracts.DTOs.RequestResponse.TestCylinderBreakInput>()
        };

        // Act
        var result = await business.SaveTestSetDayDataAsync(request);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal("TEST-123", result.Data.TestCylinderCode);
        Assert.Equal(testSetDayId, result.Data.TestSetDayId);
        Assert.Equal(testedDate, result.Data.DateTested);

        // Verify that GetTestQueueItemAsync was called after save
        mockRepository.Verify(
            r => r.GetTestQueueItemAsync(testSetDayId),
            Times.Once);
    }

    [Fact]
    public async Task SaveTestSetDayDataAsync_ReturnsErrorWhenUpdatedProjectionNotFound()
    {
        // Verify that SaveTestSetDayDataAsync returns error if the updated projection
        // cannot be retrieved after save

        // Arrange
        var mockRepository = new Mock<ITesterReportRepository>();
        var mockTestSetDayRepository = new Mock<ITestSetDayRepository>();
        var mockTestResultService = new Mock<ITestResultService>();
        var mockUnitOfWork = new Mock<IUnitOfWork>();
        var mockLogger = new Mock<ILogger<TesterReportBusiness>>();
        var business = new TesterReportBusiness(
            mockRepository.Object,
            mockTestSetDayRepository.Object,
            mockTestResultService.Object,
            mockUnitOfWork.Object,
            mockLogger.Object);

        var today = DateTime.Today;
        var testSetDayId = 100;
        var castDate = today.AddDays(-7);
        var testedDate = today;

        // Setup repository mocks
        mockTestSetDayRepository
            .Setup(r => r.GetCastDateAsync(testSetDayId))
            .ReturnsAsync(castDate);

        // Return null from GetTestQueueItemAsync to simulate failure
        mockRepository
            .Setup(r => r.GetTestQueueItemAsync(testSetDayId))
            .ReturnsAsync((TestCylinderQueueProjection?)null);

        var request = new Contracts.DTOs.RequestResponse.SaveTestSetDayDataRequest
        {
            TestSetDayId = testSetDayId,
            DateTested = testedDate,
            Comments = "Test completed",
            CylinderBreaks = new List<Contracts.DTOs.RequestResponse.TestCylinderBreakInput>()
        };

        // Act
        var result = await business.SaveTestSetDayDataAsync(request);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Null(result.Data);
        Assert.NotEmpty(result.Errors);
    }
}
