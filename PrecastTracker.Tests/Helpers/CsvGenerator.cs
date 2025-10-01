using System.Text;
using PrecastTracker.Contracts.DTOs.RequestResponse;

namespace PrecastTracker.Tests.Helpers;

public static class CsvGenerator
{
    public static string GenerateCsv(IEnumerable<ConcreteReportResponse> data)
    {
        var csv = new StringBuilder();

        // Header
        csv.AppendLine("Test ID,Cylinder ID,Casting Date,Mix Design,Yards/Bed,Bed ID,Batching Start Time,Job ID,Job Name,Truck No.,Pour ID,Piece Type,Oven ID,Age of Test,Testing Date,Required (PSI),Break #1,Break #2,Break #3,Average PSI,Comments");

        // Data rows
        foreach (var row in data)
        {
            // Only quote TruckNo if it contains a comma
            var truckNo = row.TruckNo.Contains(',') ? $"\"{row.TruckNo}\"" : row.TruckNo;
            csv.AppendLine($"{row.TestId},{row.CylinderId},{row.CastingDate},{row.MixDesign},{row.YardsPerBed},{row.BedId},{row.BatchingStartTime},{row.JobId},{row.JobName},{truckNo},{row.PourId},{row.PieceType},{row.OvenId},{row.AgeOfTest},{row.TestingDate},{row.Required},{row.Break1},{row.Break2},{row.Break3},{row.AveragePsi},{row.Comments}");
        }

        return csv.ToString();
    }
}
