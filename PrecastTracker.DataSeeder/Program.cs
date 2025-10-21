using PrecastTracker.Data;
using PrecastTracker.DataSeeder.Helpers;
using Microsoft.EntityFrameworkCore;

namespace PrecastTracker.DataSeeder;

class Program
{
    private static ApplicationDbContext _context = null!;

    static int Main(string[] args)
    {
        // Check for help flag
        if (args.Length > 0 && (args[0] == "--help" || args[0] == "-h"))
        {
            ShowAvailableScenarios();
            return 0;
        }

        // Initialize database context
        // Use absolute path relative to current working directory to support running from any location
        var dbPath = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "PrecastTracker.db");
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite($"Data Source={dbPath}")
            .Options;

        _context = new ApplicationDbContext(options);

        // If no arguments or invalid arguments, show available scenarios
        if (args.Length == 0 || !IsValidScenario(args[0]))
        {
            ShowAvailableScenarios();
            return args.Length == 0 ? 0 : 1; // Exit code 1 for invalid scenario
        }

        // Execute the requested scenario
        try
        {
            ExecuteScenario(args[0]);
            Console.WriteLine($"Successfully seeded scenario: {args[0]}");
            return 0; // Success
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error seeding scenario '{args[0]}': {ex.Message}");
            return 2; // Exit code 2 for execution failure
        }
        finally
        {
            _context.Dispose();
        }
    }

    private static void ShowAvailableScenarios()
    {
        Console.WriteLine("PrecastTracker Data Seeder");
        Console.WriteLine("=========================");
        Console.WriteLine();
        Console.WriteLine("Usage: PrecastTracker.DataSeeder <scenario-name>");
        Console.WriteLine();
        Console.WriteLine("Example:");
        Console.WriteLine("  PrecastTracker.DataSeeder TypicalProductionDay");
        Console.WriteLine();
        Console.WriteLine("Available Scenarios:");
        Console.WriteLine();
        Console.WriteLine("  TypicalProductionDay");
        Console.WriteLine("    Eight placements spanning different timelines:");
        Console.WriteLine("      - 3 placements with tests overdue by 1 day (1-day, 7-day, and 28-day tests)");
        Console.WriteLine("      - 3 placements with tests due today (1-day, 7-day, and 28-day tests)");
        Console.WriteLine("      - 2 placements with tests due tomorrow (1-day and 7-day tests)");
        Console.WriteLine();
        Console.WriteLine("  OverdueTests");
        Console.WriteLine("    Placements with overdue test cylinders to verify queue and notification features.");
        Console.WriteLine();
        Console.WriteLine("  PlacementsWithMissingTests");
        Console.WriteLine("    Multiple placements without any test sets to verify gap detection functionality.");
        Console.WriteLine();
        Console.WriteLine("  FullProductionWeek");
        Console.WriteLine("    A complete week of production across multiple jobs and beds without test data.");
        Console.WriteLine();
        Console.WriteLine("Exit Codes:");
        Console.WriteLine("  0 - Success");
        Console.WriteLine("  1 - Invalid scenario name");
        Console.WriteLine("  2 - Execution failure");
    }

    private static bool IsValidScenario(string scenarioName)
    {
        return scenarioName switch
        {
            "TypicalProductionDay" => true,
            "OverdueTests" => true,
            "PlacementsWithMissingTests" => true,
            "FullProductionWeek" => true,
            _ => false
        };
    }

    private static void ExecuteScenario(string scenarioName)
    {
        // Always reset database before seeding
        ResetDatabase();

        switch (scenarioName)
        {
            case "TypicalProductionDay":
                SeedTypicalProductionDay();
                break;
            case "OverdueTests":
                SeedOverdueTests();
                break;
            case "PlacementsWithMissingTests":
                SeedPlacementsWithMissingTests();
                break;
            case "FullProductionWeek":
                SeedFullProductionWeek();
                break;
        }
    }

    #region Database Management

    private static void ResetDatabase()
    {
        _context.Database.EnsureDeleted();
        _context.Database.Migrate();
    }

    #endregion

    #region Scenario Implementations

    private static void SeedTypicalProductionDay()
    {
        var today = DateTime.Today;

        // Placement from 8 days ago - will have a 7-day test due yesterday (OVERDUE by 1 day)
        var placement1 = HighLevelEntityHelpers.CreatePlacement(
            context: _context,
            jobCode: "25-020",
            jobName: "Woodbury High School",
            bedId: 1,
            mixDesignCode: "824.1",
            productionDate: today.AddDays(-8),
            pieceType: "Walls",
            startTime: new TimeSpan(8, 30, 0),
            volume: 12.5m,
            ovenId: "A1"
        );

        // Placement from 2 days ago - will have a 1-day test due yesterday (OVERDUE by 1 day)
        var placement2 = HighLevelEntityHelpers.CreatePlacement(
            context: _context,
            jobCode: "25-020",
            jobName: "Woodbury High School",
            bedId: 1,
            mixDesignCode: "824.1",
            productionDate: today.AddDays(-2),
            pieceType: "Tees",
            startTime: new TimeSpan(10, 15, 0),
            volume: 8.3m,
            ovenId: "B2"
        );

        // Placement from 29 days ago - will have a 28-day test due yesterday (OVERDUE by 1 day)
        var placement3 = HighLevelEntityHelpers.CreatePlacement(
            context: _context,
            jobCode: "25-015",
            jobName: "Downtown Parking Ramp",
            bedId: 2,
            mixDesignCode: "2515.11",
            productionDate: today.AddDays(-29),
            pieceType: "Slabs",
            startTime: new TimeSpan(9, 0, 0),
            volume: 15.0m,
            ovenId: null
        );

        // Placement from 6 days ago - will have a 7-day test due tomorrow (NOT overdue)
        var placement4 = HighLevelEntityHelpers.CreatePlacement(
            context: _context,
            jobCode: "25-018",
            jobName: "Library Expansion",
            bedId: 3,
            mixDesignCode: "824.1",
            productionDate: today.AddDays(-6),
            pieceType: "Columns",
            startTime: new TimeSpan(7, 15, 0),
            volume: 9.8m,
            ovenId: "C1"
        );

        // Placement from today - will have a 1-day test due tomorrow (NOT overdue)
        var placement5 = HighLevelEntityHelpers.CreatePlacement(
            context: _context,
            jobCode: "25-018",
            jobName: "Library Expansion",
            bedId: 3,
            mixDesignCode: "824.1",
            productionDate: today,
            pieceType: "Beams",
            startTime: new TimeSpan(13, 45, 0),
            volume: 11.2m,
            ovenId: "C2"
        );

        // Placement from 7 days ago - will have a 7-day test due today
        var placement6 = HighLevelEntityHelpers.CreatePlacement(
            context: _context,
            jobCode: "25-022",
            jobName: "Community Center",
            bedId: 4,
            mixDesignCode: "1200.3",
            productionDate: today.AddDays(-7),
            pieceType: "Walls",
            startTime: new TimeSpan(8, 0, 0),
            volume: 14.5m,
            ovenId: "D1"
        );

        // Placement from 1 day ago - will have a 1-day test due today
        var placement7 = HighLevelEntityHelpers.CreatePlacement(
            context: _context,
            jobCode: "25-022",
            jobName: "Community Center",
            bedId: 4,
            mixDesignCode: "1200.3",
            productionDate: today.AddDays(-1),
            pieceType: "Tees",
            startTime: new TimeSpan(10, 30, 0),
            volume: 7.8m,
            ovenId: "D2"
        );

        // Placement from 28 days ago - will have a 28-day test due today
        var placement8 = HighLevelEntityHelpers.CreatePlacement(
            context: _context,
            jobCode: "25-019",
            jobName: "Retail Complex",
            bedId: 5,
            mixDesignCode: "3500.5",
            productionDate: today.AddDays(-28),
            pieceType: "Slabs",
            startTime: new TimeSpan(9, 15, 0),
            volume: 16.0m,
            ovenId: null
        );

        // Create test sets for placement 1 (poured 8 days ago - 7-day test OVERDUE by 1 day)
        var testSet1 = HighLevelEntityHelpers.CreateTestSet(_context, placement1.PlacementId);

        // 1-day test was completed 7 days ago
        var testSetDay1_Day1 = HighLevelEntityHelpers.CreateTestSetDay(
            context: _context,
            testSetId: testSet1.TestSetId,
            dayNum: 1,
            dateDue: today.AddDays(-7),
            dateTested: today.AddDays(-7),
            comments: "Normal strength"
        );
        HighLevelEntityHelpers.CreateTestCylinder(_context, testSetDay1_Day1.TestSetDayId, "12345-1-25-020", 3250);
        HighLevelEntityHelpers.CreateTestCylinder(_context, testSetDay1_Day1.TestSetDayId, "12345-2-25-020", 3180);

        // 7-day test was due yesterday (OVERDUE by 1 day, not yet tested)
        var testSetDay1_Day7 = HighLevelEntityHelpers.CreateTestSetDay(
            context: _context,
            testSetId: testSet1.TestSetId,
            dayNum: 7,
            dateDue: today.AddDays(-1),
            dateTested: null,
            comments: null
        );
        HighLevelEntityHelpers.CreateTestCylinder(_context, testSetDay1_Day7.TestSetDayId, "12345-7-25-020", null);
        HighLevelEntityHelpers.CreateTestCylinder(_context, testSetDay1_Day7.TestSetDayId, "12345-8-25-020", null);

        // 28-day test is due in 20 days
        var testSetDay1_Day28 = HighLevelEntityHelpers.CreateTestSetDay(
            context: _context,
            testSetId: testSet1.TestSetId,
            dayNum: 28,
            dateDue: today.AddDays(20),
            dateTested: null,
            comments: null
        );
        HighLevelEntityHelpers.CreateTestCylinder(_context, testSetDay1_Day28.TestSetDayId, "12345-28-25-020", null);
        HighLevelEntityHelpers.CreateTestCylinder(_context, testSetDay1_Day28.TestSetDayId, "12345-29-25-020", null);

        // Create test sets for placement 2 (poured 2 days ago - 1-day test OVERDUE by 1 day)
        var testSet2 = HighLevelEntityHelpers.CreateTestSet(_context, placement2.PlacementId);

        // 1-day test was due yesterday (OVERDUE by 1 day, not yet tested)
        var testSetDay2_Day1 = HighLevelEntityHelpers.CreateTestSetDay(
            context: _context,
            testSetId: testSet2.TestSetId,
            dayNum: 1,
            dateDue: today.AddDays(-1),
            dateTested: null
        );
        HighLevelEntityHelpers.CreateTestCylinder(_context, testSetDay2_Day1.TestSetDayId, "12346-1-25-020", null);
        HighLevelEntityHelpers.CreateTestCylinder(_context, testSetDay2_Day1.TestSetDayId, "12346-2-25-020", null);

        // 7-day test is due in 5 days
        var testSetDay2_Day7 = HighLevelEntityHelpers.CreateTestSetDay(
            context: _context,
            testSetId: testSet2.TestSetId,
            dayNum: 7,
            dateDue: today.AddDays(5)
        );
        HighLevelEntityHelpers.CreateTestCylinder(_context, testSetDay2_Day7.TestSetDayId, "12346-7-25-020", null);
        HighLevelEntityHelpers.CreateTestCylinder(_context, testSetDay2_Day7.TestSetDayId, "12346-8-25-020", null);

        // 28-day test is due in 26 days
        var testSetDay2_Day28 = HighLevelEntityHelpers.CreateTestSetDay(
            context: _context,
            testSetId: testSet2.TestSetId,
            dayNum: 28,
            dateDue: today.AddDays(26)
        );
        HighLevelEntityHelpers.CreateTestCylinder(_context, testSetDay2_Day28.TestSetDayId, "12346-28-25-020", null);
        HighLevelEntityHelpers.CreateTestCylinder(_context, testSetDay2_Day28.TestSetDayId, "12346-29-25-020", null);

        // Create test sets for placement 3 (poured 29 days ago - 28-day test OVERDUE by 1 day)
        var testSet3 = HighLevelEntityHelpers.CreateTestSet(_context, placement3.PlacementId);

        // 1-day test was completed 28 days ago
        var testSetDay3_Day1 = HighLevelEntityHelpers.CreateTestSetDay(
            context: _context,
            testSetId: testSet3.TestSetId,
            dayNum: 1,
            dateDue: today.AddDays(-28),
            dateTested: today.AddDays(-28),
            comments: "Good strength"
        );
        HighLevelEntityHelpers.CreateTestCylinder(_context, testSetDay3_Day1.TestSetDayId, "12347-1-25-015", 4200);
        HighLevelEntityHelpers.CreateTestCylinder(_context, testSetDay3_Day1.TestSetDayId, "12347-2-25-015", 4150);

        // 7-day test was completed 22 days ago
        var testSetDay3_Day7 = HighLevelEntityHelpers.CreateTestSetDay(
            context: _context,
            testSetId: testSet3.TestSetId,
            dayNum: 7,
            dateDue: today.AddDays(-22),
            dateTested: today.AddDays(-22),
            comments: "Excellent results"
        );
        HighLevelEntityHelpers.CreateTestCylinder(_context, testSetDay3_Day7.TestSetDayId, "12347-7-25-015", 6100);
        HighLevelEntityHelpers.CreateTestCylinder(_context, testSetDay3_Day7.TestSetDayId, "12347-8-25-015", 6050);

        // 28-day test was due yesterday (OVERDUE by 1 day, not yet tested)
        var testSetDay3_Day28 = HighLevelEntityHelpers.CreateTestSetDay(
            context: _context,
            testSetId: testSet3.TestSetId,
            dayNum: 28,
            dateDue: today.AddDays(-1),
            dateTested: null
        );
        HighLevelEntityHelpers.CreateTestCylinder(_context, testSetDay3_Day28.TestSetDayId, "12347-28-25-015", null);
        HighLevelEntityHelpers.CreateTestCylinder(_context, testSetDay3_Day28.TestSetDayId, "12347-29-25-015", null);

        // Create test sets for placement 4 (poured 6 days ago - 7-day test due tomorrow)
        var testSet4 = HighLevelEntityHelpers.CreateTestSet(_context, placement4.PlacementId);

        // 1-day test was completed 5 days ago
        var testSetDay4_Day1 = HighLevelEntityHelpers.CreateTestSetDay(
            context: _context,
            testSetId: testSet4.TestSetId,
            dayNum: 1,
            dateDue: today.AddDays(-5),
            dateTested: today.AddDays(-5),
            comments: "Good results"
        );
        HighLevelEntityHelpers.CreateTestCylinder(_context, testSetDay4_Day1.TestSetDayId, "12348-1-25-018", 3350);
        HighLevelEntityHelpers.CreateTestCylinder(_context, testSetDay4_Day1.TestSetDayId, "12348-2-25-018", 3280);

        // 7-day test is due tomorrow (not yet tested)
        var testSetDay4_Day7 = HighLevelEntityHelpers.CreateTestSetDay(
            context: _context,
            testSetId: testSet4.TestSetId,
            dayNum: 7,
            dateDue: today.AddDays(1),
            dateTested: null,
            comments: null
        );
        HighLevelEntityHelpers.CreateTestCylinder(_context, testSetDay4_Day7.TestSetDayId, "12348-7-25-018", null);
        HighLevelEntityHelpers.CreateTestCylinder(_context, testSetDay4_Day7.TestSetDayId, "12348-8-25-018", null);

        // 28-day test is due in 22 days
        var testSetDay4_Day28 = HighLevelEntityHelpers.CreateTestSetDay(
            context: _context,
            testSetId: testSet4.TestSetId,
            dayNum: 28,
            dateDue: today.AddDays(22),
            dateTested: null,
            comments: null
        );
        HighLevelEntityHelpers.CreateTestCylinder(_context, testSetDay4_Day28.TestSetDayId, "12348-28-25-018", null);
        HighLevelEntityHelpers.CreateTestCylinder(_context, testSetDay4_Day28.TestSetDayId, "12348-29-25-018", null);

        // Create test sets for placement 5 (poured today - 1-day test due tomorrow)
        var testSet5 = HighLevelEntityHelpers.CreateTestSet(_context, placement5.PlacementId);

        // 1-day test is due tomorrow (not yet tested)
        var testSetDay5_Day1 = HighLevelEntityHelpers.CreateTestSetDay(
            context: _context,
            testSetId: testSet5.TestSetId,
            dayNum: 1,
            dateDue: today.AddDays(1),
            dateTested: null,
            comments: null
        );
        HighLevelEntityHelpers.CreateTestCylinder(_context, testSetDay5_Day1.TestSetDayId, "12349-1-25-018", null);
        HighLevelEntityHelpers.CreateTestCylinder(_context, testSetDay5_Day1.TestSetDayId, "12349-2-25-018", null);

        // 7-day test is due in 7 days
        var testSetDay5_Day7 = HighLevelEntityHelpers.CreateTestSetDay(
            context: _context,
            testSetId: testSet5.TestSetId,
            dayNum: 7,
            dateDue: today.AddDays(7),
            dateTested: null,
            comments: null
        );
        HighLevelEntityHelpers.CreateTestCylinder(_context, testSetDay5_Day7.TestSetDayId, "12349-7-25-018", null);
        HighLevelEntityHelpers.CreateTestCylinder(_context, testSetDay5_Day7.TestSetDayId, "12349-8-25-018", null);

        // 28-day test is due in 28 days
        var testSetDay5_Day28 = HighLevelEntityHelpers.CreateTestSetDay(
            context: _context,
            testSetId: testSet5.TestSetId,
            dayNum: 28,
            dateDue: today.AddDays(28),
            dateTested: null,
            comments: null
        );
        HighLevelEntityHelpers.CreateTestCylinder(_context, testSetDay5_Day28.TestSetDayId, "12349-28-25-018", null);
        HighLevelEntityHelpers.CreateTestCylinder(_context, testSetDay5_Day28.TestSetDayId, "12349-29-25-018", null);

        // Create test sets for placement 6 (poured 7 days ago - 7-day test due today)
        var testSet6 = HighLevelEntityHelpers.CreateTestSet(_context, placement6.PlacementId);

        // 1-day test was completed 6 days ago
        var testSetDay6_Day1 = HighLevelEntityHelpers.CreateTestSetDay(
            context: _context,
            testSetId: testSet6.TestSetId,
            dayNum: 1,
            dateDue: today.AddDays(-6),
            dateTested: today.AddDays(-6),
            comments: "Normal strength"
        );
        HighLevelEntityHelpers.CreateTestCylinder(_context, testSetDay6_Day1.TestSetDayId, "12350-1-25-022", 3200);
        HighLevelEntityHelpers.CreateTestCylinder(_context, testSetDay6_Day1.TestSetDayId, "12350-2-25-022", 3150);

        // 7-day test is due today (not yet tested)
        var testSetDay6_Day7 = HighLevelEntityHelpers.CreateTestSetDay(
            context: _context,
            testSetId: testSet6.TestSetId,
            dayNum: 7,
            dateDue: today,
            dateTested: null,
            comments: null
        );
        HighLevelEntityHelpers.CreateTestCylinder(_context, testSetDay6_Day7.TestSetDayId, "12350-7-25-022", null);
        HighLevelEntityHelpers.CreateTestCylinder(_context, testSetDay6_Day7.TestSetDayId, "12350-8-25-022", null);

        // 28-day test is due in 21 days
        var testSetDay6_Day28 = HighLevelEntityHelpers.CreateTestSetDay(
            context: _context,
            testSetId: testSet6.TestSetId,
            dayNum: 28,
            dateDue: today.AddDays(21),
            dateTested: null,
            comments: null
        );
        HighLevelEntityHelpers.CreateTestCylinder(_context, testSetDay6_Day28.TestSetDayId, "12350-28-25-022", null);
        HighLevelEntityHelpers.CreateTestCylinder(_context, testSetDay6_Day28.TestSetDayId, "12350-29-25-022", null);

        // Create test sets for placement 7 (poured 1 day ago - 1-day test due today)
        var testSet7 = HighLevelEntityHelpers.CreateTestSet(_context, placement7.PlacementId);

        // 1-day test is due today (not yet tested)
        var testSetDay7_Day1 = HighLevelEntityHelpers.CreateTestSetDay(
            context: _context,
            testSetId: testSet7.TestSetId,
            dayNum: 1,
            dateDue: today,
            dateTested: null,
            comments: null
        );
        HighLevelEntityHelpers.CreateTestCylinder(_context, testSetDay7_Day1.TestSetDayId, "12351-1-25-022", null);
        HighLevelEntityHelpers.CreateTestCylinder(_context, testSetDay7_Day1.TestSetDayId, "12351-2-25-022", null);

        // 7-day test is due in 6 days
        var testSetDay7_Day7 = HighLevelEntityHelpers.CreateTestSetDay(
            context: _context,
            testSetId: testSet7.TestSetId,
            dayNum: 7,
            dateDue: today.AddDays(6),
            dateTested: null,
            comments: null
        );
        HighLevelEntityHelpers.CreateTestCylinder(_context, testSetDay7_Day7.TestSetDayId, "12351-7-25-022", null);
        HighLevelEntityHelpers.CreateTestCylinder(_context, testSetDay7_Day7.TestSetDayId, "12351-8-25-022", null);

        // 28-day test is due in 27 days
        var testSetDay7_Day28 = HighLevelEntityHelpers.CreateTestSetDay(
            context: _context,
            testSetId: testSet7.TestSetId,
            dayNum: 28,
            dateDue: today.AddDays(27),
            dateTested: null,
            comments: null
        );
        HighLevelEntityHelpers.CreateTestCylinder(_context, testSetDay7_Day28.TestSetDayId, "12351-28-25-022", null);
        HighLevelEntityHelpers.CreateTestCylinder(_context, testSetDay7_Day28.TestSetDayId, "12351-29-25-022", null);

        // Create test sets for placement 8 (poured 28 days ago - 28-day test due today)
        var testSet8 = HighLevelEntityHelpers.CreateTestSet(_context, placement8.PlacementId);

        // 1-day test was completed 27 days ago
        var testSetDay8_Day1 = HighLevelEntityHelpers.CreateTestSetDay(
            context: _context,
            testSetId: testSet8.TestSetId,
            dayNum: 1,
            dateDue: today.AddDays(-27),
            dateTested: today.AddDays(-27),
            comments: "Good strength"
        );
        HighLevelEntityHelpers.CreateTestCylinder(_context, testSetDay8_Day1.TestSetDayId, "12352-1-25-019", 4100);
        HighLevelEntityHelpers.CreateTestCylinder(_context, testSetDay8_Day1.TestSetDayId, "12352-2-25-019", 4050);

        // 7-day test was completed 21 days ago
        var testSetDay8_Day7 = HighLevelEntityHelpers.CreateTestSetDay(
            context: _context,
            testSetId: testSet8.TestSetId,
            dayNum: 7,
            dateDue: today.AddDays(-21),
            dateTested: today.AddDays(-21),
            comments: "Strong results"
        );
        HighLevelEntityHelpers.CreateTestCylinder(_context, testSetDay8_Day7.TestSetDayId, "12352-7-25-019", 5900);
        HighLevelEntityHelpers.CreateTestCylinder(_context, testSetDay8_Day7.TestSetDayId, "12352-8-25-019", 5850);

        // 28-day test is due today (not yet tested)
        var testSetDay8_Day28 = HighLevelEntityHelpers.CreateTestSetDay(
            context: _context,
            testSetId: testSet8.TestSetId,
            dayNum: 28,
            dateDue: today,
            dateTested: null,
            comments: null
        );
        HighLevelEntityHelpers.CreateTestCylinder(_context, testSetDay8_Day28.TestSetDayId, "12352-28-25-019", null);
        HighLevelEntityHelpers.CreateTestCylinder(_context, testSetDay8_Day28.TestSetDayId, "12352-29-25-019", null);
    }

    private static void SeedOverdueTests()
    {
        var today = DateTime.Today;
        var lastWeek = today.AddDays(-7);

        var placement = HighLevelEntityHelpers.CreatePlacement(
            context: _context,
            jobCode: "25-018",
            jobName: "Bridge Deck Project",
            bedId: 1,
            mixDesignCode: "1200.5",
            productionDate: lastWeek,
            pieceType: "Slabs",
            startTime: new TimeSpan(9, 0, 0),
            volume: 15.0m,
            ovenId: "C1"
        );

        var testSet = HighLevelEntityHelpers.CreateTestSet(_context, placement.PlacementId);

        var testSetDay1 = HighLevelEntityHelpers.CreateTestSetDay(
            context: _context,
            testSetId: testSet.TestSetId,
            dayNum: 1,
            dateDue: lastWeek.AddDays(1),
            dateTested: null
        );
        HighLevelEntityHelpers.CreateTestCylinder(_context, testSetDay1.TestSetDayId, "OD-1-25-018", null);
        HighLevelEntityHelpers.CreateTestCylinder(_context, testSetDay1.TestSetDayId, "OD-2-25-018", null);

        var testSetDay7 = HighLevelEntityHelpers.CreateTestSetDay(
            context: _context,
            testSetId: testSet.TestSetId,
            dayNum: 7,
            dateDue: today,
            dateTested: null
        );
        HighLevelEntityHelpers.CreateTestCylinder(_context, testSetDay7.TestSetDayId, "OD-7-25-018", null);
        HighLevelEntityHelpers.CreateTestCylinder(_context, testSetDay7.TestSetDayId, "OD-8-25-018", null);

        var testSetDay28 = HighLevelEntityHelpers.CreateTestSetDay(
            context: _context,
            testSetId: testSet.TestSetId,
            dayNum: 28,
            dateDue: lastWeek.AddDays(28),
            dateTested: null
        );
        HighLevelEntityHelpers.CreateTestCylinder(_context, testSetDay28.TestSetDayId, "OD-28-25-018", null);
        HighLevelEntityHelpers.CreateTestCylinder(_context, testSetDay28.TestSetDayId, "OD-29-25-018", null);
    }

    private static void SeedPlacementsWithMissingTests()
    {
        var today = DateTime.Today;

        HighLevelEntityHelpers.CreatePlacement(
            context: _context,
            jobCode: "25-022",
            jobName: "Stadium Renovation",
            bedId: 3,
            mixDesignCode: "3000.2",
            productionDate: today,
            pieceType: "Columns",
            startTime: new TimeSpan(14, 30, 0),
            volume: 6.5m,
            ovenId: null
        );

        HighLevelEntityHelpers.CreatePlacement(
            context: _context,
            jobCode: "25-022",
            jobName: "Stadium Renovation",
            bedId: 3,
            mixDesignCode: "3000.2",
            productionDate: today,
            pieceType: "Beams",
            startTime: new TimeSpan(15, 45, 0),
            volume: 4.2m,
            ovenId: "D3"
        );

        HighLevelEntityHelpers.CreatePlacement(
            context: _context,
            jobCode: "25-025",
            jobName: "Office Complex",
            bedId: 4,
            mixDesignCode: "2000.1",
            productionDate: today,
            pieceType: "Walls",
            startTime: new TimeSpan(7, 0, 0),
            volume: 18.0m,
            ovenId: "A2"
        );
    }

    private static void SeedFullProductionWeek()
    {
        var today = DateTime.Today;
        var monday = today.AddDays(-(int)today.DayOfWeek + 1);

        HighLevelEntityHelpers.CreatePlacement(_context, "25-030", "Metro Station", 1, "5000.3", monday, "Walls", new TimeSpan(7, 0, 0), 20.0m, "A1");
        HighLevelEntityHelpers.CreatePlacement(_context, "25-030", "Metro Station", 1, "5000.3", monday, "Slabs", new TimeSpan(9, 30, 0), 15.5m, "A2");

        var tuesday = monday.AddDays(1);
        HighLevelEntityHelpers.CreatePlacement(_context, "25-031", "Hospital Wing", 2, "4500.2", tuesday, "Columns", new TimeSpan(8, 0, 0), 12.0m, "B1");
        HighLevelEntityHelpers.CreatePlacement(_context, "25-032", "Shopping Mall", 3, "3500.1", tuesday, "Tees", new TimeSpan(10, 0, 0), 18.5m, "C1");

        var wednesday = monday.AddDays(2);
        HighLevelEntityHelpers.CreatePlacement(_context, "25-030", "Metro Station", 1, "5000.3", wednesday, "Beams", new TimeSpan(7, 30, 0), 22.0m, "A1");

        var thursday = monday.AddDays(3);
        HighLevelEntityHelpers.CreatePlacement(_context, "25-033", "School Addition", 4, "4000.5", thursday, "Walls", new TimeSpan(8, 15, 0), 16.0m, "D1");
        HighLevelEntityHelpers.CreatePlacement(_context, "25-033", "School Addition", 4, "4000.5", thursday, "Slabs", new TimeSpan(11, 0, 0), 14.0m, "D2");

        var friday = monday.AddDays(4);
        HighLevelEntityHelpers.CreatePlacement(_context, "25-031", "Hospital Wing", 2, "4500.2", friday, "Walls", new TimeSpan(7, 0, 0), 19.0m, "B2");
    }

    #endregion
}
