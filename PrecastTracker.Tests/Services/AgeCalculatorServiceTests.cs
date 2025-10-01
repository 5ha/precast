using Microsoft.Extensions.Logging;
using PrecastTracker.Services;

namespace PrecastTracker.Tests.Services;

public class AgeCalculatorServiceTests
{
    private readonly IAgeCalculatorService _service;

    public AgeCalculatorServiceTests()
    {
        var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<AgeCalculatorService>();
        _service = new AgeCalculatorService(logger);
    }

    #region Null/Empty Testing Date Tests

    [Fact]
    public void CalculateAgeOfTest_NullTestingDate_ReturnsEmptyString()
    {
        // Arrange
        var castingDate = new DateTime(2025, 9, 9);
        TimeSpan? batchingStartTime = new TimeSpan(9, 24, 0);

        // Act
        var result = _service.CalculateAgeOfTest(castingDate, batchingStartTime, null);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    #endregion

    #region Tests >= 2 Days Old (Date-Only Calculation)

    [Fact]
    public void CalculateAgeOfTest_ExactlyTwoDays_ReturnsJustDayNumber()
    {
        // Arrange
        var castingDate = new DateTime(2025, 9, 9);
        var testingDate = new DateTime(2025, 9, 11); // Exactly 2 days later
        TimeSpan? batchingStartTime = new TimeSpan(9, 24, 0);

        // Act
        var result = _service.CalculateAgeOfTest(castingDate, batchingStartTime, testingDate);

        // Assert
        Assert.Equal("2", result);
    }

    [Fact]
    public void CalculateAgeOfTest_SevenDays_ReturnsJustDayNumber()
    {
        // Arrange
        var castingDate = new DateTime(2025, 9, 9);
        var testingDate = new DateTime(2025, 9, 16); // 7 days later
        TimeSpan? batchingStartTime = new TimeSpan(9, 24, 0);

        // Act
        var result = _service.CalculateAgeOfTest(castingDate, batchingStartTime, testingDate);

        // Assert
        Assert.Equal("7", result);
    }

    [Fact]
    public void CalculateAgeOfTest_TwentyEightDays_ReturnsJustDayNumber()
    {
        // Arrange
        var castingDate = new DateTime(2025, 9, 9);
        var testingDate = new DateTime(2025, 10, 7); // 28 days later
        TimeSpan? batchingStartTime = new TimeSpan(9, 24, 0);

        // Act
        var result = _service.CalculateAgeOfTest(castingDate, batchingStartTime, testingDate);

        // Assert
        Assert.Equal("28", result);
    }

    [Fact]
    public void CalculateAgeOfTest_SevenDaysWithTime_IgnoresTimeComponent()
    {
        // Arrange
        var castingDate = new DateTime(2025, 9, 9, 9, 24, 0);
        var testingDate = new DateTime(2025, 9, 16, 15, 30, 0); // 7 days later with different time
        TimeSpan? batchingStartTime = new TimeSpan(9, 24, 0);

        // Act
        var result = _service.CalculateAgeOfTest(castingDate, batchingStartTime, testingDate);

        // Assert
        Assert.Equal("7", result);
    }

    [Fact]
    public void CalculateAgeOfTest_SevenDaysWithNoBatchingTime_StillReturnsSevenDays()
    {
        // Arrange
        var castingDate = new DateTime(2025, 9, 9);
        var testingDate = new DateTime(2025, 9, 16);
        TimeSpan? batchingStartTime = null;

        // Act
        var result = _service.CalculateAgeOfTest(castingDate, batchingStartTime, testingDate);

        // Assert
        Assert.Equal("7", result);
    }

    [Fact]
    public void CalculateAgeOfTest_EightDaysButActually47Hours_ReturnsEightDays()
    {
        // Arrange - This tests the boundary: 8 date-days apart but only 47 hours actual time
        var castingDate = new DateTime(2025, 9, 9, 23, 0, 0);
        var testingDate = new DateTime(2025, 9, 17, 22, 0, 0); // 8 date-days but only ~191 hours
        TimeSpan? batchingStartTime = new TimeSpan(23, 0, 0);

        // Act
        var result = _service.CalculateAgeOfTest(castingDate, batchingStartTime, testingDate);

        // Assert
        Assert.Equal("8", result); // Returns 8 because date difference is 8
    }

    #endregion

    #region Tests < 2 Days Old (Precise Time Calculation)

    [Fact]
    public void CalculateAgeOfTest_ZeroDays12Hours53Minutes_ReturnsDetailedFormat()
    {
        // Arrange
        var castingDate = new DateTime(2025, 9, 9);
        var batchingStartTime = new TimeSpan(9, 24, 0);
        var testingDate = new DateTime(2025, 9, 9, 22, 17, 0);

        // Act
        var result = _service.CalculateAgeOfTest(castingDate, batchingStartTime, testingDate);

        // Assert
        Assert.Equal("0d 12:53", result);
    }

    [Fact]
    public void CalculateAgeOfTest_OneDayAnd3Hours45Minutes_ReturnsDetailedFormat()
    {
        // Arrange
        var castingDate = new DateTime(2025, 9, 9);
        var batchingStartTime = new TimeSpan(9, 24, 0);
        var testingDate = new DateTime(2025, 9, 10, 13, 9, 0);

        // Act
        var result = _service.CalculateAgeOfTest(castingDate, batchingStartTime, testingDate);

        // Assert
        Assert.Equal("1d 3:45", result);
    }

    [Fact]
    public void CalculateAgeOfTest_OneDayExactly24Hours_ReturnsOneDayZeroHours()
    {
        // Arrange
        var castingDate = new DateTime(2025, 9, 9);
        var batchingStartTime = new TimeSpan(9, 24, 0);
        var testingDate = new DateTime(2025, 9, 10, 9, 24, 0);

        // Act
        var result = _service.CalculateAgeOfTest(castingDate, batchingStartTime, testingDate);

        // Assert
        Assert.Equal("1d 0:00", result);
    }

    [Fact]
    public void CalculateAgeOfTest_LessThanOneDay_ReturnsZeroDaysWithHours()
    {
        // Arrange
        var castingDate = new DateTime(2025, 9, 9);
        var batchingStartTime = new TimeSpan(9, 24, 0);
        var testingDate = new DateTime(2025, 9, 9, 18, 45, 0);

        // Act
        var result = _service.CalculateAgeOfTest(castingDate, batchingStartTime, testingDate);

        // Assert
        Assert.Equal("0d 9:21", result);
    }

    [Fact]
    public void CalculateAgeOfTest_LessThan2DaysWithNoBatchingTime_UsesDateOnly()
    {
        // Arrange
        var castingDate = new DateTime(2025, 9, 9);
        TimeSpan? batchingStartTime = null;
        var testingDate = new DateTime(2025, 9, 9, 12, 30, 0);

        // Act
        var result = _service.CalculateAgeOfTest(castingDate, batchingStartTime, testingDate);

        // Assert
        Assert.Equal("0d 12:30", result);
    }

    [Fact]
    public void CalculateAgeOfTest_OneDayButDateDifferenceIsTwo_UsesDateDifference()
    {
        // Arrange - 2 date-days apart but less than 24 hours actual time
        var castingDate = new DateTime(2025, 9, 9, 23, 0, 0);
        var batchingStartTime = new TimeSpan(23, 0, 0);
        var testingDate = new DateTime(2025, 9, 11, 1, 0, 0); // 2 days later (date-wise) but only 26 hours

        // Act
        var result = _service.CalculateAgeOfTest(castingDate, batchingStartTime, testingDate);

        // Assert
        Assert.Equal("2", result); // Should use date-only calculation since date difference is 2
    }

    [Fact]
    public void CalculateAgeOfTest_BoundaryCase_OneDayAnd23Hours59Minutes()
    {
        // Arrange - Just under 2 days
        var castingDate = new DateTime(2025, 9, 9);
        var batchingStartTime = new TimeSpan(9, 0, 0);
        var testingDate = new DateTime(2025, 9, 11, 8, 59, 0); // 1d 23:59

        // Act
        var result = _service.CalculateAgeOfTest(castingDate, batchingStartTime, testingDate);

        // Assert
        Assert.Equal("2", result); // Date difference is 2, so uses date-only calculation
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void CalculateAgeOfTest_SameDay_ReturnsZeroDaysWithTime()
    {
        // Arrange
        var castingDate = new DateTime(2025, 9, 9);
        var batchingStartTime = new TimeSpan(9, 0, 0);
        var testingDate = new DateTime(2025, 9, 9, 15, 30, 0);

        // Act
        var result = _service.CalculateAgeOfTest(castingDate, batchingStartTime, testingDate);

        // Assert
        Assert.Equal("0d 6:30", result);
    }

    [Fact]
    public void CalculateAgeOfTest_SameDayAndTime_ReturnsZero()
    {
        // Arrange
        var castingDate = new DateTime(2025, 9, 9);
        var batchingStartTime = new TimeSpan(9, 24, 0);
        var testingDate = new DateTime(2025, 9, 9, 9, 24, 0);

        // Act
        var result = _service.CalculateAgeOfTest(castingDate, batchingStartTime, testingDate);

        // Assert
        Assert.Equal("0d 0:00", result);
    }

    [Fact]
    public void CalculateAgeOfTest_LeapYearBoundary_CalculatesCorrectly()
    {
        // Arrange
        var castingDate = new DateTime(2024, 2, 28);
        var testingDate = new DateTime(2024, 3, 6); // 7 days later (across leap day)
        TimeSpan? batchingStartTime = new TimeSpan(10, 0, 0);

        // Act
        var result = _service.CalculateAgeOfTest(castingDate, batchingStartTime, testingDate);

        // Assert
        Assert.Equal("7", result);
    }

    [Fact]
    public void CalculateAgeOfTest_MonthBoundary_CalculatesCorrectly()
    {
        // Arrange
        var castingDate = new DateTime(2025, 9, 28);
        var testingDate = new DateTime(2025, 10, 5); // 7 days later, crosses month boundary
        TimeSpan? batchingStartTime = new TimeSpan(9, 0, 0);

        // Act
        var result = _service.CalculateAgeOfTest(castingDate, batchingStartTime, testingDate);

        // Assert
        Assert.Equal("7", result);
    }

    [Fact]
    public void CalculateAgeOfTest_YearBoundary_CalculatesCorrectly()
    {
        // Arrange
        var castingDate = new DateTime(2024, 12, 28);
        var testingDate = new DateTime(2025, 1, 4); // 7 days later, crosses year boundary
        TimeSpan? batchingStartTime = new TimeSpan(9, 0, 0);

        // Act
        var result = _service.CalculateAgeOfTest(castingDate, batchingStartTime, testingDate);

        // Assert
        Assert.Equal("7", result);
    }

    [Fact]
    public void CalculateAgeOfTest_VeryLongDuration_CalculatesCorrectly()
    {
        // Arrange
        var castingDate = new DateTime(2025, 1, 1);
        var testingDate = new DateTime(2025, 12, 31); // 364 days later
        TimeSpan? batchingStartTime = new TimeSpan(9, 0, 0);

        // Act
        var result = _service.CalculateAgeOfTest(castingDate, batchingStartTime, testingDate);

        // Assert
        Assert.Equal("364", result);
    }

    #endregion

    #region Minutes Formatting

    [Fact]
    public void CalculateAgeOfTest_SingleDigitMinutes_FormatsWithLeadingZero()
    {
        // Arrange
        var castingDate = new DateTime(2025, 9, 9);
        var batchingStartTime = new TimeSpan(9, 24, 0);
        var testingDate = new DateTime(2025, 9, 9, 10, 29, 0); // 1 hour 5 minutes

        // Act
        var result = _service.CalculateAgeOfTest(castingDate, batchingStartTime, testingDate);

        // Assert
        Assert.Equal("0d 1:05", result);
    }

    [Fact]
    public void CalculateAgeOfTest_ZeroMinutes_FormatsWithDoubleZero()
    {
        // Arrange
        var castingDate = new DateTime(2025, 9, 9);
        var batchingStartTime = new TimeSpan(9, 0, 0);
        var testingDate = new DateTime(2025, 9, 9, 12, 0, 0);

        // Act
        var result = _service.CalculateAgeOfTest(castingDate, batchingStartTime, testingDate);

        // Assert
        Assert.Equal("0d 3:00", result);
    }

    #endregion
}
