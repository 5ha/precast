# Tester Dashboard and Workflow

## Tester's Role in Precast Concrete Production

In the precast concrete industry, the **tester** (also called lab technician or quality control technician) is responsible for:
- Tracking test cylinders through curing
- Performing compression tests at specified ages
- Recording test results
- Identifying failed tests that require attention
- Managing testing schedules

## Tester's Dashboard - What They See on Login

Based on the data model, here's what a tester needs to see when they log in:

### 1. **Today's Testing Queue** (Priority #1)
```
Tests Due Today
┌─────────────┬──────────┬─────────┬──────────┬────────┬──────────┐
│ Test Code   │ Cylinder │ Age Due │ Job      │ Mix    │ Req. PSI │
├─────────────┼──────────┼─────────┼──────────┼────────┼──────────┤
│ 9011        │ 7C       │ 7 days  │ 25-009   │ 2509.1 │ 6000     │
│ 9012.1      │ 1C       │ 1 day   │ 25-020   │ 824.1  │ 3500     │
│ 9015        │ 28C      │ 28 days │ 25-015   │ 622.1  │ 5000     │
└─────────────┴──────────┴─────────┴──────────┴────────┴──────────┘
```

**Why this matters:** Testers work from a daily schedule. Missing a test means the data is invalid.

### 2. **Overdue Tests** (Alert Section)
Tests that should have been performed but weren't:
- Visual alerts (red badges)
- Sorted by how many days overdue
- Quick-action buttons to mark as "tested" or "cylinder lost"

### 3. **Untested Placements** (Gap Detection)
Recent concrete placements that have NO test cylinders recorded yet:
```
Missing Test Cylinders
┌──────────┬─────────────┬─────────────┬──────────┐
│ Pour     │ Placement   │ Cast Date   │ Mix      │
├──────────┼─────────────┼─────────────┼──────────┤
│ 6539     │ 17:00       │ 2 days ago  │ 622.1    │
└──────────┴─────────────┴─────────────┴──────────┘
```

**Industry insight:** Sometimes in the chaos of production, test cylinders get made but not logged. This view catches those gaps.

### 4. **Recently Completed Tests**
Last 10-20 tests with quick visual indicators:
- ✅ Passed (avg PSI ≥ required PSI)
- ⚠️ Failed (avg PSI < required PSI)
- 📝 Pending review (results entered, need QC approval)

### 5. **Quick Entry Form** (Always Visible)
Testers need to enter results FAST:
- Test code lookup (autocomplete)
- 2-3 break result fields
- Auto-calculate average
- Pass/fail indicator
- Comments field for anomalies

## Optimized Tester Workflow

### Morning Routine:
1. **Login** → See dashboard with today's testing queue
2. **Check oven schedules** → Know which cylinders to pull from which oven
3. **Pull cylinders** → Match physical cylinders to test codes
4. **Perform tests** → Use compression machine
5. **Enter results** → Quick entry form on dashboard
6. **Review alerts** → Check for any overdue or failed tests

### Key Workflow Moments:

**Scenario A: Routine 7-day test**
```
1. Tester sees "Test 9011 (7C) - Due Today"
2. Clicks test → Opens quick entry modal
3. Enters: Break1: 6463, Break2: 6427
4. System auto-calculates avg: 6445 PSI
5. System shows: ✅ PASS (required: 6000 PSI)
6. Clicks "Save" → Test marked complete
```

**Scenario B: Failed test requiring attention**
```
1. Tester enters results: Break1: 3200, Break2: 3150
2. System calculates avg: 3175 PSI
3. System shows: ⚠️ FAIL (required: 5000 PSI)
4. System prompts: "Add comment? Notify supervisor?"
5. Tester adds comment: "Low strength, possible cold weather cure"
6. System triggers alert to QC manager
```

## Dashboard Layout

```
╔═══════════════════════════════════════════════════════════╗
║  PrecastTracker - Tester Dashboard         [Profile] [🔔3]║
╠═══════════════════════════════════════════════════════════╣
║                                                             ║
║  ⚠️ ALERTS                                                  ║
║  • 2 tests overdue    • 1 failed test needs review         ║
║                                                             ║
║  📋 TESTS DUE TODAY (5)                    [+ Quick Entry] ║
║  ┌─────────────────────────────────────────────────────┐  ║
║  │ 9011    7C    7d    25-009 Dickinson    2509.1  6000│  ║
║  │ 9012.1  1C    1d    25-020 Woodbury     824.1   3500│  ║
║  │ ...                                                  │  ║
║  └─────────────────────────────────────────────────────┘  ║
║                                                             ║
║  📅 UPCOMING TESTS (Next 7 Days)           [View Calendar] ║
║  • Wed 10/15: 3 tests (2x 7C, 1x 1C)                       ║
║  • Thu 10/16: 7 tests (5x 7C, 2x 28C)                      ║
║                                                             ║
║  ✅ RECENT COMPLETIONS                                      ║
║  ┌─────────────────────────────────────────────────────┐  ║
║  │ ✅ 9010  7C  6445 PSI  PASS  (req: 6000)  2 hrs ago │  ║
║  │ ⚠️ 9008  1C  3175 PSI  FAIL  (req: 5000)  3 hrs ago │  ║
║  └─────────────────────────────────────────────────────┘  ║
║                                                             ║
║  🔍 SEARCH TESTS  |  📊 REPORTS  |  🏭 OVEN STATUS         ║
╚═══════════════════════════════════════════════════════════╝
```

## Critical Data Model Additions Needed

Looking at the current `ConcreteTest` entity, I notice some missing fields for the tester workflow:

**Missing Fields:**
1. **OvenId** - Which oven was used (helps testers locate cylinders)
2. **AgeOfTest** - Actual age when tested (for 1C tests, might be "0d 12:53")
3. **TestStatus** - Enum: Pending, InProgress, Completed, Failed, Cancelled
4. **TestedBy** - User who performed the test (accountability)
5. **SupervisorReviewed** - Boolean for QC approval workflow
6. **AveragePsi** - Calculated field for quick sorting/filtering

## Dashboard Views Breakdown

### Primary View: Tests Due Today
- **Sort order:** By test age (1C tests first, then 7C, then 28C)
- **Display fields:** Test Code, Cylinder ID, Job Code, Mix Design, Required PSI
- **Actions:** Quick entry button for each test
- **Visual indicators:** Color coding for test type (1C = yellow, 7C = blue, 28C = green)

### Secondary View: Overdue Tests
- **Sort order:** Most overdue first
- **Display fields:** Same as primary + "Days Overdue" column
- **Actions:** Mark as tested, Mark as lost/damaged
- **Visual indicators:** Red background, escalating alert icons

### Tertiary View: Upcoming Schedule
- **Date range:** Next 7 days
- **Display:** Grouped by date, count of tests by cylinder type
- **Purpose:** Planning workload and resource allocation

### Support View: Recent Completions
- **Time range:** Last 24 hours
- **Display fields:** Test Code, Cylinder ID, Average PSI, Pass/Fail, Time tested
- **Purpose:** Quick verification that data entry was correct
- **Actions:** Edit results if mistake caught early

### Utility View: Search/Filter
- **Filter options:**
  - By job code
  - By mix design
  - By date range
  - By pass/fail status
  - By test status (pending/completed)
- **Purpose:** Finding historical test data for engineering reviews

## Batch Operations

### Batch Test Entry
When multiple cylinders from the same placement are tested together:
```
Batch Entry: Tests from Placement #456 (Mix 2509.1)
┌───────────┬────────┬────────┬────────┬─────────┬────────┐
│ Test Code │ Break1 │ Break2 │ Break3 │ Average │ Status │
├───────────┼────────┼────────┼────────┼─────────┼────────┤
│ 9011      │ [____] │ [____] │ [____] │ [calc]  │ [____] │
│ 9011.1    │ [____] │ [____] │ [____] │ [calc]  │ [____] │
│ 9011.2    │ [____] │ [____] │ [____] │ [calc]  │ [____] │
└───────────┴────────┴────────┴────────┴─────────┴────────┘
                                           [Save All Tests]
```

## Notification Rules

### When to Notify Tester:
- Tests due tomorrow (end of shift reminder)
- Tests due today (morning notification)
- Tests now overdue (immediate)

### When to Notify QC Manager:
- Test failed (average PSI < required PSI)
- Test significantly exceeded requirements (potential data entry error)
- Multiple tests from same placement failed
- Test overdue by 3+ days

### When to Notify Project Manager:
- Critical path test failed (1C tests that gate production)
- Multiple sequential failures on same mix design
