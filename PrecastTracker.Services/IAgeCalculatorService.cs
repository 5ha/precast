namespace PrecastTracker.Services;

public interface IAgeCalculatorService : IService
{
    /// <summary>
    /// Calculates the age of a concrete test from casting date to testing date.
    /// For tests >= 2 days old: returns just the day number (e.g., "7", "28") using date-only calculation.
    /// For tests < 2 days old: returns detailed format (e.g., "0d 12:53", "1d 3:45") using precise time calculation.
    /// </summary>
    /// <param name="castingDate">The date the concrete was cast</param>
    /// <param name="batchingStartTime">Optional batching start time (used for tests < 2 days)</param>
    /// <param name="testingDate">The date/time the test was performed</param>
    /// <returns>Age string in format "7" or "0d 12:53", or empty string if testingDate is null</returns>
    string CalculateAgeOfTest(DateTime castingDate, TimeSpan? batchingStartTime, DateTime? testingDate);
}
