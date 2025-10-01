using System.Globalization;
using PrecastTracker.Data;
using PrecastTracker.Data.Entities;

namespace PrecastTracker.Tests.Helpers;

public static class TestDataSeeder
{
    public static async Task SeedFromCsvAsync(ApplicationDbContext context, string csvFilePath)
    {
        var lines = await File.ReadAllLinesAsync(csvFilePath);

        // Skip header and BOM
        var dataLines = lines.Skip(1).Where(l => !string.IsNullOrWhiteSpace(l)).ToList();

        var mixDesignCache = new Dictionary<string, MixDesign>();
        var jobCache = new Dictionary<string, Job>();
        var bedCache = new Dictionary<string, Bed>();
        var pourCache = new Dictionary<string, Pour>();
        var placementCache = new Dictionary<string, Placement>();

        foreach (var line in dataLines)
        {
            var fields = ParseCsvLine(line);

            if (fields.Length < 21)
                continue;

            // Parse fields
            var testCode = fields[0].Trim();
            var cylinderId = fields[1].Trim();
            var castingDate = ParseDate(fields[2]);
            var mixDesignCode = fields[3].Trim();
            var yardsPerBed = ParseDecimal(fields[4]);
            var bedCode = fields[5].Trim();
            var batchingStartTime = ParseTime(fields[6]);
            var jobCode = fields[7].Trim();
            var jobName = fields[8].Trim();
            var truckNumbers = fields[9].Trim();
            var pourCode = fields[10].Trim();
            var pieceType = fields[11].Trim();
            var ovenId = fields[12].Trim();
            var testingDate = ParseTestingDate(fields[14]);
            var requiredPsi = ParseInt(fields[15]) ?? 0;
            var break1 = ParseInt(fields[16]);
            var break2 = ParseInt(fields[17]);
            var break3 = ParseInt(fields[18]);
            var comments = fields[20].Trim();

            // Get or create MixDesign
            if (!mixDesignCache.TryGetValue(mixDesignCode, out var mixDesign))
            {
                mixDesign = context.MixDesigns.FirstOrDefault(m => m.Code == mixDesignCode);
                if (mixDesign == null)
                {
                    mixDesign = new MixDesign { Code = mixDesignCode };
                    context.MixDesigns.Add(mixDesign);
                    await context.SaveChangesAsync();
                }
                mixDesignCache[mixDesignCode] = mixDesign;
            }

            // Get or create Job
            if (!jobCache.TryGetValue(jobCode, out var job))
            {
                job = context.Jobs.FirstOrDefault(j => j.Code == jobCode);
                if (job == null)
                {
                    job = new Job { Code = jobCode, Name = jobName };
                    context.Jobs.Add(job);
                    await context.SaveChangesAsync();
                }
                jobCache[jobCode] = job;
            }

            // Get or create Bed
            if (!bedCache.TryGetValue(bedCode, out var bed))
            {
                bed = context.Beds.FirstOrDefault(b => b.Code == bedCode);
                if (bed == null)
                {
                    bed = new Bed { Code = bedCode };
                    context.Beds.Add(bed);
                    await context.SaveChangesAsync();
                }
                bedCache[bedCode] = bed;
            }

            // Get or create Pour
            var pourKey = $"{pourCode}_{jobCode}_{bedCode}_{castingDate:yyyyMMdd}";
            if (!pourCache.TryGetValue(pourKey, out var pour))
            {
                pour = context.Pours.FirstOrDefault(p =>
                    p.Code == pourCode &&
                    p.JobId == job.JobId &&
                    p.BedId == bed.BedId &&
                    p.CastingDate == castingDate);

                if (pour == null)
                {
                    pour = new Pour
                    {
                        Code = pourCode,
                        CastingDate = castingDate,
                        JobId = job.JobId,
                        BedId = bed.BedId
                    };
                    context.Pours.Add(pour);
                    await context.SaveChangesAsync();
                }
                pourCache[pourKey] = pour;
            }

            // Get or create Placement
            var placementKey = $"{pourKey}_{mixDesignCode}_{yardsPerBed}_{batchingStartTime?.ToString() ?? ""}_{truckNumbers}_{pieceType}_{ovenId}";
            if (!placementCache.TryGetValue(placementKey, out var placement))
            {
                placement = context.Placements.FirstOrDefault(p =>
                    p.PourId == pour.PourId &&
                    p.MixDesignId == mixDesign.MixDesignId &&
                    p.YardsPerBed == yardsPerBed &&
                    p.BatchingStartTime == batchingStartTime &&
                    p.TruckNumbers == truckNumbers &&
                    p.PieceType == pieceType &&
                    p.OvenId == (string.IsNullOrWhiteSpace(ovenId) ? null : ovenId));

                if (placement == null)
                {
                    placement = new Placement
                    {
                        PourId = pour.PourId,
                        MixDesignId = mixDesign.MixDesignId,
                        YardsPerBed = yardsPerBed,
                        BatchingStartTime = batchingStartTime,
                        TruckNumbers = string.IsNullOrWhiteSpace(truckNumbers) ? null : truckNumbers,
                        PieceType = string.IsNullOrWhiteSpace(pieceType) ? null : pieceType,
                        OvenId = string.IsNullOrWhiteSpace(ovenId) ? null : ovenId
                    };
                    context.Placements.Add(placement);
                    await context.SaveChangesAsync();
                }
                placementCache[placementKey] = placement;
            }

            // Create ConcreteTest
            var test = new ConcreteTest
            {
                TestCode = testCode,
                CylinderId = cylinderId,
                PlacementId = placement.PlacementId,
                TestingDate = testingDate,
                RequiredPsi = requiredPsi,
                Break1 = break1,
                Break2 = break2,
                Break3 = break3,
                Comments = string.IsNullOrWhiteSpace(comments) ? null : comments
            };

            context.ConcreteTests.Add(test);
        }

        await context.SaveChangesAsync();
    }

    private static string[] ParseCsvLine(string line)
    {
        var fields = new List<string>();
        var currentField = new System.Text.StringBuilder();
        var inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            var c = line[i];

            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    currentField.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                fields.Add(currentField.ToString());
                currentField.Clear();
            }
            else
            {
                currentField.Append(c);
            }
        }

        fields.Add(currentField.ToString());
        return fields.ToArray();
    }

    private static DateTime ParseDate(string dateStr)
    {
        if (DateTime.TryParseExact(dateStr, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            return date;

        return DateTime.MinValue;
    }

    private static DateTime? ParseTestingDate(string dateStr)
    {
        if (string.IsNullOrWhiteSpace(dateStr) || dateStr.Contains("#"))
            return null;

        if (dateStr.Contains("/"))
        {
            // Try full date with time
            if (DateTime.TryParse(dateStr, out var fullDate))
                return fullDate;

            // Try with current year assumption for MM/dd format
            var parts = dateStr.Split(' ');
            var datePart = parts[0];

            if (DateTime.TryParseExact(datePart, "M/d", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                return new DateTime(2025, date.Month, date.Day);
            }
        }

        return null;
    }

    private static TimeSpan? ParseTime(string timeStr)
    {
        if (string.IsNullOrWhiteSpace(timeStr))
            return null;

        if (TimeSpan.TryParseExact(timeStr, @"h\:mm", CultureInfo.InvariantCulture, out var time))
            return time;

        return null;
    }

    private static decimal ParseDecimal(string value)
    {
        if (decimal.TryParse(value, out var result))
            return result;
        return 0;
    }

    private static int? ParseInt(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        if (int.TryParse(value, out var result))
            return result;

        return null;
    }
}
