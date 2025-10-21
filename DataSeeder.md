# PrecastTracker Data Seeder

The PrecastTracker Data Seeder is a console application designed to populate your development database with various test scenarios. This tool is useful for manual testing, frontend development, and exploring different data states.

## Purpose

While developing, you often need realistic data in your database to test features. Instead of manually creating data through the UI or writing SQL scripts, this seeder provides predefined scenarios that you can run with a single command.

## Running the Seeder

### From the Solution Directory

```bash
# Run a specific scenario
dotnet run --project PrecastTracker.DataSeeder <scenario-name>

# Show help and available scenarios
dotnet run --project PrecastTracker.DataSeeder
dotnet run --project PrecastTracker.DataSeeder -- --help
dotnet run --project PrecastTracker.DataSeeder -- -h
```

### From the DataSeeder Project Directory

```bash
# Run a specific scenario
dotnet run <scenario-name>

# Show help and available scenarios
dotnet run
dotnet run -- --help
dotnet run -- -h
```

## Available Scenarios

### TypicalProductionDay
```bash
dotnet run --project PrecastTracker.DataSeeder TypicalProductionDay
```
Three placements with tests due today (1-day, 7-day, and 28-day tests). This scenario creates:
- Placement from 7 days ago with a 7-day test due today
- Placement from 1 day ago with a 1-day test due today
- Placement from 28 days ago with a 28-day test due today
- Previous tests already completed with results
- All tests due today are untested (ready for the tester to complete)

### OverdueTests
```bash
dotnet run --project PrecastTracker.DataSeeder OverdueTests
```
Placements with overdue test cylinders to verify queue and notification features. This scenario creates:
- A placement from one week ago
- Test cylinders that are 6 days overdue
- Test cylinders due today
- Future test cylinders

### PlacementsWithMissingTests
```bash
dotnet run --project PrecastTracker.DataSeeder PlacementsWithMissingTests
```
Multiple placements without any test sets to verify gap detection functionality. This scenario creates:
- 3 placements across 2 jobs
- None of the placements have associated test sets
- Useful for testing gap detection and alerts

### FullProductionWeek
```bash
dotnet run --project PrecastTracker.DataSeeder FullProductionWeek
```
A complete week of production across multiple jobs and beds without test data. This scenario creates:
- 8 placements spread across Monday through Friday
- 4 different jobs
- 4 different beds
- No test sets created (all placements have gaps)

## Exit Codes

The application returns different exit codes to indicate success or failure:

- **0** - Success (scenario seeded successfully, or help displayed)
- **1** - Invalid scenario name
- **2** - Execution failure (database error, migration failure, etc.)

### Checking Exit Codes

```bash
# Run and check exit code (Linux/Mac/WSL)
dotnet run --project PrecastTracker.DataSeeder TypicalProductionDay; echo "Exit code: $?"

# Run and check exit code (Windows PowerShell)
dotnet run --project PrecastTracker.DataSeeder TypicalProductionDay; echo "Exit code: $LASTEXITCODE"
```

## How It Works

1. **Database Reset**: Each scenario automatically drops and recreates the database from migrations, ensuring a clean slate.
2. **Entity Creation**: Uses helper methods to create entities in the correct order, respecting foreign key constraints.
3. **Idempotent Helpers**: Base entities (Jobs, Beds, etc.) use "Ensure" methods that only create if the entity doesn't already exist, allowing realistic data reuse (e.g., multiple placements for the same job).

## AI Integration

This tool is designed to be AI-friendly:
- Running without arguments lists all available scenarios
- Clear, parseable output
- Distinct exit codes for programmatic usage
- Scenarios are self-documenting with one-sentence descriptions

An AI can:
1. Call the seeder without arguments to discover available scenarios
2. Choose the appropriate scenario based on the descriptions
3. Run the selected scenario
4. Check the exit code to verify success

## Technical Details

- **Database**: SQLite at `../App_Data/PrecastTracker.db`
- **Migrations**: Uses `EnsureDeleted()` + `Migrate()` to reset the database
- **Helper Classes**: Organized into Base, Mid-Level, and High-Level entity helpers in `Helpers/EntityHelpers.cs`
- **Project Type**: .NET 9.0 Console Application

## Development

To add a new scenario:

1. Add the scenario name to `IsValidScenario()` method
2. Add a case to `ExecuteScenario()` switch statement
3. Implement the scenario method using the helper classes
4. Update the `ShowAvailableScenarios()` method with the new scenario description
5. Update this documentation

## Examples

```bash
# Typical workflow: seed data, then start the API
dotnet run --project PrecastTracker.DataSeeder TypicalProductionDay
dotnet run --project PrecastTracker.WebApi

# Test overdue test functionality
dotnet run --project PrecastTracker.DataSeeder OverdueTests
# Then navigate to the Tester Dashboard to see overdue tests

# Test gap detection
dotnet run --project PrecastTracker.DataSeeder PlacementsWithMissingTests
# Then check reports for placements missing test data
```
