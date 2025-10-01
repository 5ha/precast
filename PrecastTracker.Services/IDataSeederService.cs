namespace PrecastTracker.Services;

public interface IDataSeederService : IService
{
    Task SeedFromCsvAsync(string csvFilePath);
}
