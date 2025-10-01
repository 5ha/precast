using Microsoft.Extensions.Logging;

namespace PrecastTracker.Services;

public class AgeCalculatorService : BaseService<AgeCalculatorService>, IAgeCalculatorService
{
    public AgeCalculatorService(ILogger<AgeCalculatorService> logger) : base(logger)
    {
    }

    public string CalculateAgeOfTest(DateTime castingDate, TimeSpan? batchingStartTime, DateTime? testingDate)
    {
        if (!testingDate.HasValue)
            return string.Empty;

        // First check if test is >= 2 days old using date-only comparison
        var daysDifference = (testingDate.Value.Date - castingDate.Date).Days;

        // If test is >= 2 days old, return just the day number (ignore time components)
        if (daysDifference >= 2)
            return daysDifference.ToString();

        // For tests < 2 days old, use precise time calculation
        var startDateTime = batchingStartTime.HasValue
            ? castingDate.Add(batchingStartTime.Value)
            : castingDate;

        var timeSpan = testingDate.Value - startDateTime;
        var days = (int)timeSpan.TotalDays;
        var hours = timeSpan.Hours;
        var minutes = timeSpan.Minutes;

        return $"{days}d {hours}:{minutes:00}";
    }
}
