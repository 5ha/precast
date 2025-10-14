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
        var mixDesignRequirementCache = new Dictionary<string, MixDesignRequirement>(); // Key: "MixDesignCode_TestType"
        var jobCache = new Dictionary<string, Job>();
        var bedCache = new Dictionary<string, Bed>();
        var productionDayCache = new Dictionary<DateTime, ProductionDay>();
        var pourCache = new Dictionary<string, Pour>(); // Key: "PourId"
        var mixBatchCache = new Dictionary<int, MixBatch>();
        var placementCache = new Dictionary<string, Placement>(); // Key: complex combination
        var testSetCache = new Dictionary<string, TestSet>(); // Key: "PlacementId_TestType"

        foreach (var line in dataLines)
        {
            var fields = ParseCsvLine(line);

            if (fields.Length < 21)
                continue;

            // Parse fields
            var testIdStr = fields[0].Trim();
            var cylinderId = fields[1].Trim();
            var castingDate = ParseDate(fields[2]);
            var mixDesignCode = fields[3].Trim();
            var volume = ParseDecimal(fields[4]);
            var bedCode = fields[5].Trim();
            var startTimeStr = fields[6].Trim();
            var jobCode = fields[7].Trim();
            var jobName = fields[8].Trim();
            var truckNumbers = fields[9].Trim();
            var pourIdStr = fields[10].Trim();
            var pieceType = fields[11].Trim();
            var ovenId = fields[12].Trim();
            var testingDate = ParseTestingDate(fields[14]);
            var requiredPsi = ParseInt(fields[15]) ?? 0;
            var break1 = ParseInt(fields[16]);
            var break2 = ParseInt(fields[17]);
            var break3 = ParseInt(fields[18]);
            var comments = fields[20].Trim();

            // Parse Test ID to get MixBatchId (strip .1, .2, etc.)
            var mixBatchId = ParseMixBatchId(testIdStr);

            // Parse Cylinder ID to get TestType
            var testType = ParseTestType(cylinderId);

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

            // Get or create MixDesignRequirement
            var reqKey = $"{mixDesignCode}_{testType}";
            if (!mixDesignRequirementCache.TryGetValue(reqKey, out var mixDesignRequirement))
            {
                mixDesignRequirement = context.MixDesignRequirements.FirstOrDefault(r =>
                    r.MixDesignId == mixDesign.MixDesignId && r.TestType == testType);

                if (mixDesignRequirement == null && requiredPsi > 0)
                {
                    mixDesignRequirement = new MixDesignRequirement
                    {
                        MixDesignId = mixDesign.MixDesignId,
                        TestType = testType,
                        RequiredPsi = requiredPsi
                    };
                    context.MixDesignRequirements.Add(mixDesignRequirement);
                    await context.SaveChangesAsync();
                }

                if (mixDesignRequirement != null)
                    mixDesignRequirementCache[reqKey] = mixDesignRequirement;
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

            // Get or create Bed (bedCode is the BedId as string, parse it)
            if (!bedCache.TryGetValue(bedCode, out var bed))
            {
                if (!int.TryParse(bedCode, out var bedId))
                    throw new InvalidOperationException($"Invalid Bed ID: {bedCode}");

                bed = context.Beds.FirstOrDefault(b => b.BedId == bedId);
                if (bed == null)
                {
                    bed = new Bed { BedId = bedId };
                    context.Beds.Add(bed);
                    await context.SaveChangesAsync();
                }
                bedCache[bedCode] = bed;
            }

            // Get or create ProductionDay
            if (!productionDayCache.TryGetValue(castingDate, out var productionDay))
            {
                productionDay = context.ProductionDays.FirstOrDefault(pd => pd.Date == castingDate);
                if (productionDay == null)
                {
                    productionDay = new ProductionDay { Date = castingDate };
                    context.ProductionDays.Add(productionDay);
                    await context.SaveChangesAsync();
                }
                productionDayCache[castingDate] = productionDay;
            }

            // Parse Pour ID as integer
            if (!int.TryParse(pourIdStr, out var pourId))
                throw new InvalidOperationException($"Invalid Pour ID: {pourIdStr}");

            // Get or create Pour (with explicit ID from CSV)
            var pourKey = $"{pourId}";
            if (!pourCache.TryGetValue(pourKey, out var pour))
            {
                pour = context.Pours.FirstOrDefault(p => p.PourId == pourId);

                if (pour == null)
                {
                    pour = new Pour
                    {
                        PourId = pourId,
                        JobId = job.JobId,
                        BedId = bed.BedId
                    };
                    context.Pours.Add(pour);
                    await context.SaveChangesAsync();
                }
                pourCache[pourKey] = pour;
            }

            // Get or create MixBatch (with explicit ID)
            if (!mixBatchCache.TryGetValue(mixBatchId, out var mixBatch))
            {
                mixBatch = context.MixBatches.FirstOrDefault(mb => mb.MixBatchId == mixBatchId);
                if (mixBatch == null)
                {
                    mixBatch = new MixBatch
                    {
                        MixBatchId = mixBatchId,
                        ProductionDayId = productionDay.ProductionDayId,
                        MixDesignId = mixDesign.MixDesignId
                    };
                    context.MixBatches.Add(mixBatch);
                    await context.SaveChangesAsync();
                }
                mixBatchCache[mixBatchId] = mixBatch;
            }

            // Parse start time (time only)
            var startTime = ParseStartTimeSpan(startTimeStr);

            // Get or create Placement
            var placementKey = $"{pour.PourId}_{mixBatch.MixBatchId}_{volume}_{startTime}_{pieceType}_{ovenId}";
            if (!placementCache.TryGetValue(placementKey, out var placement))
            {
                placement = context.Placements.FirstOrDefault(p =>
                    p.PourId == pour.PourId &&
                    p.MixBatchId == mixBatch.MixBatchId &&
                    p.Volume == volume &&
                    p.StartTime == startTime &&
                    p.PieceType == pieceType &&
                    p.OvenId == (string.IsNullOrWhiteSpace(ovenId) ? null : ovenId));

                if (placement == null)
                {
                    placement = new Placement
                    {
                        PourId = pour.PourId,
                        MixBatchId = mixBatch.MixBatchId,
                        PieceType = pieceType,
                        StartTime = startTime,
                        Volume = volume,
                        OvenId = string.IsNullOrWhiteSpace(ovenId) ? null : ovenId
                    };
                    context.Placements.Add(placement);

                    try
                    {
                        await context.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException(
                            $"Failed to save Placement. " +
                            $"PourId={pour.PourId}, MixBatchId={mixBatch.MixBatchId}, " +
                            $"TestIdStr={testIdStr}, CastingDate={castingDate:yyyy-MM-dd}", ex);
                    }
                }
                placementCache[placementKey] = placement;
            }

            // Create Deliveries for this Placement (parse truck numbers)
            if (!string.IsNullOrWhiteSpace(truckNumbers))
            {
                var trucks = truckNumbers.Split(',').Select(t => t.Trim()).Where(t => !string.IsNullOrEmpty(t));
                foreach (var truck in trucks)
                {
                    // Check if delivery already exists for this placement and truck
                    var existingDelivery = context.Deliveries.FirstOrDefault(d =>
                        d.PlacementId == placement.PlacementId && d.TruckId == truck);

                    if (existingDelivery == null)
                    {
                        var delivery = new Delivery
                        {
                            PlacementId = placement.PlacementId,
                            TruckId = truck
                        };
                        context.Deliveries.Add(delivery);
                    }
                }
            }

            // Get or create TestSet
            var testSetKey = $"{placement.PlacementId}_{testType}";
            if (!testSetCache.TryGetValue(testSetKey, out var testSet))
            {
                testSet = context.TestSets.FirstOrDefault(ts =>
                    ts.PlacementId == placement.PlacementId &&
                    ts.TestType == testType);

                if (testSet == null)
                {
                    testSet = new TestSet
                    {
                        PlacementId = placement.PlacementId,
                        TestType = testType,
                        TestingDate = testingDate,
                        Comments = string.IsNullOrWhiteSpace(comments) ? null : comments
                    };
                    context.TestSets.Add(testSet);
                    await context.SaveChangesAsync();
                }
                testSetCache[testSetKey] = testSet;
            }

            // Create ConcreteTest records for each break value
            var breaks = new[] { break1, break2, break3 }.Where(b => b.HasValue).ToList();
            foreach (var breakPsi in breaks)
            {
                var test = new ConcreteTest
                {
                    TestSetId = testSet.TestSetId,
                    BreakPsi = breakPsi!.Value
                };
                context.ConcreteTests.Add(test);
            }
        }

        await context.SaveChangesAsync();
    }

    private static int ParseMixBatchId(string testIdStr)
    {
        // Strip .1, .2, etc. to get the MixBatchId
        var parts = testIdStr.Split('.');
        if (int.TryParse(parts[0], out var id))
            return id;
        return 0;
    }

    private static int ParseTestType(string cylinderId)
    {
        // "7C" = 7, "28C" = 28, "1C" = 1
        if (cylinderId.EndsWith("C"))
        {
            var numStr = cylinderId.Substring(0, cylinderId.Length - 1);
            if (int.TryParse(numStr, out var testType))
                return testType;
        }
        return 0;
    }

    private static TimeSpan ParseStartTimeSpan(string timeStr)
    {
        if (string.IsNullOrWhiteSpace(timeStr))
            return TimeSpan.Zero;

        if (TimeSpan.TryParseExact(timeStr, @"h\:mm", CultureInfo.InvariantCulture, out var time))
            return time;

        return TimeSpan.Zero;
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
