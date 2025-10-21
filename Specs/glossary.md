# PrecastTracker Domain Glossary

## Industry Context

Precast concrete manufacturing involves creating concrete elements (walls, slabs, beams, tees, etc.) in a controlled factory environment before transporting them to construction sites. Understanding the production workflow is essential to understanding our data model.

## Production Workflow

1. **Job Order**: A construction project orders specific precast elements
2. **Production Scheduling**: Jobs are assigned to specific beds (casting surfaces) via Pours
3. **Mix Batching**: Concrete mix designs are batched on specific dates creating MixBatches
4. **Concrete Delivery**: Ready-mix trucks deliver concrete from MixBatches throughout the day
5. **Casting**: Concrete is placed into forms on beds to create specific piece types (Placements)
6. **Test Cylinders**: Small test cylinders are filled from each MixBatch for quality testing (TestSets)
7. **Curing**: Elements and test cylinders are placed in ovens to accelerate curing
8. **Testing**: Cylinders are crushed at specified ages (1 day, 7 days, 28 days) to verify strength

## Domain Entities

### Job
A construction project that requires precast concrete elements.

**Properties:**
- **Code**: Business identifier (e.g., "25-020")
- **Name**: Project name (e.g., "Woodbury HS")

**Example:** Job "25-020 Woodbury HS" might order 50 wall panels and 20 tee beams.

**Relationships:**
- A Job can have multiple Pours (production runs on different days/beds)

---

### Bed
A casting surface where precast elements are formed. Think of it as a production station.

**Properties:**
- **BedId**: Bed identifier (e.g., 16, 2, 13)

**Real-world context:** A precast plant typically has multiple beds (10-50+). Each bed can produce different elements on different days.

**Relationships:**
- A Bed can be used for multiple Pours

---

### Production Day
Represents a specific calendar date on which production occurs. This entity ensures that all activities (Pours, MixBatches) happening on the same day reference a single date source.

**Properties:**
- **ProductionDayId**: Unique identifier
- **Date**: The calendar date (e.g., 09/10/2025)

**Real-world context:**
September 10, 2025 is a production day. All pours scheduled for that day and all mix batches prepared that day reference this single ProductionDay record.

**Why this matters:**
Concrete has a limited shelf life and must be used within hours of batching. A ProductionDay provides date normalization and ensures that MixBatches are tied to specific dates. Since Placements reference MixBatches, this inherently enforces that placements use concrete from the correct day.

**Relationships:**
- A ProductionDay can have multiple MixBatches
- A ProductionDay can have multiple Placements (through MixBatches)

---

### Mix Design
A concrete recipe specifying the proportions of cement, aggregates, water, and admixtures.

**Properties:**
- **MixDesignId**: Unique identifier
- **Code**: Mix identifier (e.g., "824.1", "2515.11")

**Real-world context:** Different structural requirements need different concrete strengths. A 6000 PSI mix for beams is different from a 4500 PSI mix for wall panels.

**Relationships:**
- A Mix Design is used in multiple MixBatches
- A Mix Design has multiple MixDesignRequirements (strength requirements at different test ages)

---

### Mix Design Requirement
The required strength specifications for a mix design at different test ages.

**Properties:**
- **MixDesignRequirementId**: Unique identifier
- **MixDesignId**: Which mix design this requirement belongs to
- **TestType**: Age of test (1, 7, or 28 days)
- **RequiredPsi**: Target strength the concrete must achieve at this age

**Real-world context:**
Mix Design 2509.1 has the following requirements:
- 1-day test: 3500 PSI (early release criteria)
- 7-day test: 6000 PSI (early strength verification)
- 28-day test: 6000 PSI (design strength)

Mix Design 622.1 might have different requirements:
- 1-day test: 3500 PSI
- 7-day test: 5000 PSI
- 28-day test: 5000 PSI

**Why this matters:**
Mix designs are engineered recipes with specific strength targets. These requirements are part of the mix design specification and should be defined once, not repeated for every test.

**Relationships:**
- A MixDesignRequirement belongs to one Mix Design
- TestSets validate against the appropriate MixDesignRequirement based on TestType

---

### Pour
**Definition:** The assignment of a Job to a Bed. A Pour represents the production scheduling decision - which job will be worked on and on which bed. A Pour typically lasts one day but can span multiple days, which can be determined by examining the Placements associated with that Pour.

**Properties:**
- **PourId**: Surrogate primary key
- **JobId**: Which project this production run is for
- **BedId**: Which casting surface is being used

**Real-world context:**
Bed 16 is assigned to produce elements for Job "25-009 Dickinson HS". This assignment is Pour ID "6539". Over one or more days, different piece types will be cast using various concrete batches. To determine which days Pour 6539 spanned, query the distinct ProductionDays from its Placements.

**Relationships:**
- A Pour belongs to one Job
- A Pour uses one Bed
- A Pour contains multiple Placements (different piece types being cast on potentially different days)

**Example:**
```
Pour 6539 (Bed 16, Job 25-009 Dickinson HS):
├─ Placement 1: ProductionDay 09/10/2025, Walls, Mix 622.1, started at 17:00, Oven 16
├─ Placement 2: ProductionDay 09/10/2025, Walls, Mix 2509.1, started at 13:15, Oven 2
└─ Placement 3: ProductionDay 09/11/2025, Walls, Mix 2515.3, started at 08:30, Oven 2
```

---

### Placement
**Definition:** A specific piece type being cast during a Pour. A Placement represents the physical act of placing concrete to create a specific type of element.

**Industry terminology note:** In the precast concrete industry, "placement" is the standard term for the act of placing fresh concrete into forms. This is distinct from the finished product (the element/piece), which is what gets shipped to the job site.

**Properties:**
- **PlacementId**: Surrogate primary key
- **PourId**: Which production run this belongs to
- **MixBatchId**: Which concrete batch is being used
- **PieceType**: What's being cast (Walls, Slabs, Tees, etc.)
- **StartTime**: Time of day when concrete placement began (time only - the date comes from MixBatch.ProductionDay)
- **Volume**: Volume of concrete used (cubic yards)
- **OvenId**: Which curing oven will be used

**Date Derivation:**
The date when a Placement occurred is determined by following the relationship: Placement → MixBatch → ProductionDay. Since concrete must be used the same day it is batched, the Placement inherently occurs on the same day as its MixBatch.

**Real-world context:**
During Pour 6539, at 13:15, wall panels are cast using MixBatch #12345 (Mix 2509.1, ProductionDay 09/10/2025). The placement uses 17.3 cubic yards and the finished pieces will cure in Oven 2. This is one Placement.

Later that same day at 17:00, additional wall panels are cast using a different MixBatch #12346 (Mix 622.1, ProductionDay 09/10/2025), using 31.78 cubic yards, curing in Oven 16. This is a separate Placement in the same Pour.

If Pour 6539 continues the next day, a third Placement might occur at 08:30 using MixBatch #12389 (ProductionDay 09/11/2025).

**Why Placement matters:**
Placement-specific 1-day tests allow quick quality checks for specific pieces. If a placement-specific test fails, only those pieces are affected.

**Relationships:**
- A Placement belongs to one Pour
- A Placement uses one MixBatch (which determines the ProductionDay)
- A Placement has multiple TestSets (1-day, 7-day, and 28-day tests)

---

### MixBatch
**Definition:** A concrete mix design that has been batched (prepared) on a specific ProductionDay. Represents the actual production of concrete from a mix design recipe.

**Properties:**
- **MixBatchId**: Surrogate primary key
- **ProductionDayId**: When the batch was prepared
- **MixDesignId**: Which mix design recipe was used

**Real-world context:**
Mix Design 2509.1 is batched on September 10, 2025 (ProductionDay #100). This creates MixBatch #12345. Throughout the day, multiple trucks deliver concrete from this batch to various placements. The batch is tested at 7 and 28 days to verify it meets the mix design specifications.

**Why MixBatch matters:**
Quality control testing (7-day and 28-day tests) is typically performed at the batch level, not per placement. This ensures the concrete recipe was properly executed. 1-day tests may be placement-specific for early strength verification.

**Relationships:**
- A MixBatch is based on one Mix Design
- A MixBatch belongs to one ProductionDay
- A MixBatch is delivered by multiple Deliveries (trucks)
- A MixBatch is used in one or more Placements (on the same ProductionDay)
- A MixBatch's test results can be found via Placements → TestSets

---

### Delivery
**Definition:** A ready-mix truck delivering concrete from a MixBatch.

**Properties:**
- **DeliveryId**: Unique identifier
- **TruckId**: Truck number (e.g., "3", "6", "7")
- **MixBatchId**: Which batch this delivery is from

**Real-world context:**
MixBatch #12345 (Mix 2509.1, ProductionDay 09/10/2025) is delivered by trucks 3, 4, and 5 throughout the day. Each truck is one Delivery.

**Why Delivery matters:**
Tracking which trucks delivered which batches provides traceability for quality issues and logistics management.

**Relationships:**
- A Delivery belongs to one MixBatch
- Multiple Deliveries can deliver from the same MixBatch

---

### TestSet
**Definition:** A container for all test cylinders related to a specific Placement. Each Placement has one TestSet which contains TestSetDays for different test ages (1, 7, and 28 days).

**Properties:**
- **TestSetId**: Unique identifier
- **PlacementId**: Which placement this test set is for (required)

**Calculated Properties:**
- **MixBatchId**: Retrieved via Placement.MixBatchId (not stored directly)

**Real-world context:**
Each Placement will have a single TestSet that organizes all the testing requirements for that placement. The TestSet contains multiple TestSetDays, one for each required test age (1, 7, and 28 days).

**Relationships:**
- A TestSet belongs to one Placement (required)
- A TestSet contains multiple TestSetDays (one for each test age)
- A TestSet's MixBatch can be found via Placement.MixBatchId

---

### TestSetDay
**Definition:** Represents testing scheduled for a specific age (1, 7, or 28 days) within a TestSet. Contains the test cylinders that will be crushed at that age.

**Properties:**
- **TestSetDayId**: Unique identifier
- **TestSetId**: Which TestSet this belongs to
- **DayNum**: Age of test (1, 7, or 28 days)
- **DateDue**: Scheduled testing date set by the application when the TestSetDay is created (`ProductionDay.Date + DayNo`). The code must always populate this during creation so the due date persists even before any cylinders are tested.
- **DateTested**: The date we actually ran the test (nullable - null means not tested yet)
- **Comments**: Any notes about this test day

**Calculated Properties:**
- **AgeOfTest**: Calculated as the actual test execution date (or DateDue when future) minus Placement.StartTime
- **AveragePsi**: Calculated from the average of all TestCylinder.BreakPsi values in this TestSetDay
- **RequiredPsi**: Retrieved via Placement → MixBatch → MixDesign → MixDesignRequirement (where TestType matches DayNum)

**Real-world context:**
For Placement #456 (MixBatch #12345, Mix 2509.1, StartTime 13:15 on ProductionDay 09/10/2025):
- TestSet #1 contains:
  - TestSetDay #1: DayNum 1, DateDue 09/11/2025 (ProductionDay.Date + 1)
  - TestSetDay #2: DayNum 7, DateDue 09/17/2025 (ProductionDay.Date + 7)
  - TestSetDay #3: DayNum 28, DateDue 10/08/2025 (ProductionDay.Date + 28)
- RequiredPsi values come from MixDesignRequirements for Mix 2509.1

For Placement #457 (MixBatch #12346, Mix 622.1, StartTime 17:00 on ProductionDay 09/10/2025):
- TestSet #2 contains:
  - TestSetDay #4: DayNum 1, DateDue 09/11/2025 (ProductionDay.Date + 1)
  - TestSetDay #5: DayNum 7, DateDue 09/17/2025 (ProductionDay.Date + 7)
  - TestSetDay #6: DayNum 28, DateDue 10/08/2025 (ProductionDay.Date + 28)
- RequiredPsi values come from MixDesignRequirements for Mix 622.1

**Why TestSetDays matter:**
- 7-day and 28-day tests verify the MixBatch meets design specifications (can query by MixBatchId via Placement)
- 1-day tests allow quick release of specific pieces from that Placement
- Multiple test cylinders per TestSetDay ensure reliability
- DateTested tracks completion status (null = not tested, non-null = tested on that date)

**Relationships:**
- A TestSetDay belongs to one TestSet (required)
- A TestSetDay contains multiple TestCylinders (individual cylinder breaks)

---

### TestCylinder
**Definition:** An individual compression test result from crushing one test cylinder.

**Properties:**
- **TestCylinderId**: Unique identifier
- **Code**: What was written on the physical cylinder for identification (e.g., "12345-7-25-020"). Default format (implemented in UI): `[MixBatchId]-[DayNo]-[JobCode]`
- **TestSetDayId**: Which test set day this belongs to
- **BreakPsi**: The compression strength result (PSI) - nullable until tested

**Real-world context:**
TestSetDay #2 (7-day test, DayNum 7, for Placement 456 / MixBatch #12345, DateTested 09/17/2025) has three test cylinders crushed:
- TestCylinder #1: BreakPsi 6463
- TestCylinder #2: BreakPsi 6427
- TestCylinder #3: BreakPsi 6445

Average PSI for the TestSetDay = 6445 PSI

**Note:** All cylinders in a TestSetDay are tested together on the same date. The testing date is stored at the TestSetDay level, not on individual cylinders.

**Why multiple breaks matter:**
Multiple test cylinders from the same batch ensure test reliability. If one cylinder was damaged or improperly cured, the other results provide verification.

**Relationships:**
- A TestCylinder belongs to one TestSetDay
- Multiple TestCylinders combine to determine if a TestSetDay passes or fails

---

## Example: Complete Data Flow

**Scenario:** Dickinson High School needs precast wall panels.

1. **Job Created:** "25-009 Dickinson HS"

2. **Production Day Created:** ProductionDay #100, Date 09/10/2025

3. **Production Scheduled:** Bed 16 assigned to Job 25-009
   - Pour #6539 created: JobId "25-009", BedId "16"

4. **Mix Batching:**
   - MixBatch #12345: MixDesignId 2509.1, ProductionDayId 100
   - MixBatch #12346: MixDesignId 622.1, ProductionDayId 100

5. **Deliveries:**
   - Delivery #1: Truck 3, MixBatch #12345
   - Delivery #2: Truck 4, MixBatch #12345
   - Delivery #3: Truck 5, MixBatch #12345
   - Delivery #4: Truck 6, MixBatch #12346
   - Delivery #5: Truck 7, MixBatch #12346
   - Delivery #6: Truck 8, MixBatch #12346

6. **Placements:**
   - Placement #456: PourId 6539, MixBatchId 12345, PieceType "Walls", StartTime 13:15, Volume 17.3 yards, OvenId 2
   - Placement #457: PourId 6539, MixBatchId 12346, PieceType "Walls", StartTime 17:00, Volume 31.78 yards, OvenId 16

7. **MixDesignRequirements Referenced:**
   - Mix 2509.1 Requirements: 1-day: 3500 PSI, 7-day: 6000 PSI, 28-day: 6000 PSI
   - Mix 622.1 Requirements: 1-day: 3500 PSI, 7-day: 5000 PSI, 28-day: 5000 PSI

8. **TestSets and TestSetDays Created:**
   - TestSet #1: PlacementId 456
     - TestSetDay #101: DayNum 1, DateDue 09/11/2025 (ProductionDay.Date + 1), DateTested null
     - TestSetDay #102: DayNum 7, DateDue 09/17/2025 (ProductionDay.Date + 7), DateTested null
     - TestSetDay #103: DayNum 28, DateDue 10/08/2025 (ProductionDay.Date + 28), DateTested null
   - TestSet #2: PlacementId 457
     - TestSetDay #104: DayNum 1, DateDue 09/11/2025 (ProductionDay.Date + 1), DateTested null
     - TestSetDay #105: DayNum 7, DateDue 09/17/2025 (ProductionDay.Date + 7), DateTested null
     - TestSetDay #106: DayNum 28, DateDue 10/08/2025 (ProductionDay.Date + 28), DateTested null

9. **Testing Results:**
   - TestSetDay #102 (7-day, Placement 456, Mix 2509.1, DateTested 09/17/2025):
     - TestCylinder #1: BreakPsi 6463
     - TestCylinder #2: BreakPsi 6427
     - Average: 6445 PSI → PASS (required 6000 per MixDesignRequirement)

   - TestSetDay #101 (1-day, Placement 456, Mix 2509.1, DateTested 09/11/2025):
     - TestCylinder #3: BreakPsi 3580
     - TestCylinder #4: BreakPsi 3503
     - Average: 3542 PSI → PASS (required 3500 per MixDesignRequirement)

   - TestSetDay #104 (1-day, Placement 457, Mix 622.1, DateTested 09/11/2025):
     - TestCylinder #5: BreakPsi 4419
     - TestCylinder #6: BreakPsi 4305
     - Average: 4362 PSI → PASS (required 3500 per MixDesignRequirement)

**Result:** Pour 6539 has 2 Placements using 2 MixBatches, delivered by 6 trucks, with 2 TestSets (6 TestSetDays) ensuring quality control for all concrete used that day.

---

## Data Relationships Summary

```
ProductionDay (1) ──< (M) MixBatch

Job (1) ──< (M) Pour
Bed (1) ──< (M) Pour
Pour (1) ──< (M) Placement

MixDesign (1) ──< (M) MixDesignRequirement
MixDesign (1) ──< (M) MixBatch
MixBatch (1) ──< (M) Delivery
MixBatch (1) ──< (M) Placement
Placement (1) ──< (M) TestSet

TestSet (1) ──< (M) TestSetDay
TestSetDay (1) ──< (M) TestCylinder
```

**Key Insight:** The hierarchy ensures complete traceability and data integrity:
- **Pour**: Represents a Job-Bed assignment that can span multiple days
- **ProductionDay + MixBatch**: Ensures concrete batches are tied to specific dates
- **Natural Date Enforcement**: Since Placement references MixBatch, and MixBatch references ProductionDay, the date of a Placement is inherently enforced to be the same as the date the concrete was batched
- **Simplified Model**: No composite foreign keys needed - all relationships use simple foreign keys

**Traceability:**
- From a **TestCylinder** result → **TestSetDay** → **TestSet** → **Placement** → **MixBatch** → **MixDesign** (what recipe was used)
- From a **TestSetDay** → **TestSet** → **Placement** → **MixBatch** → **ProductionDay** (when it happened)
- From a **TestSetDay** → **TestSet** → **Placement** → **Pour** → **Job** (which project)
- From a **MixBatch** → **Delivery** records (which trucks delivered)
- From a **TestSetDay** → **TestSet** → **Placement** → **MixBatch** → **MixDesign** → **MixDesignRequirement** (required strength criteria)

This allows answering critical questions like:
- "Which job used the concrete that failed testing?" - Follow TestSetDay → TestSet → Placement → Pour → Job
- "Which trucks delivered the batch that passed with 6445 PSI?" - Follow TestSetDay → TestSet → Placement → MixBatch → Delivery
- "Can we release the pieces from Placement #456 based on the 1-day test results?" - Query TestSetDay where TestSet.PlacementId = 456 and DayNum = 1, compare AveragePsi to MixDesignRequirement.RequiredPsi
- "What were all the test results for MixBatch #12345?" - Query TestSetDay where TestSet.Placement.MixBatchId = 12345
- "What strength is required for this test?" - Follow TestSetDay → TestSet → Placement → MixBatch → MixDesign → MixDesignRequirement (where TestType matches DayNum)
- "Which test days are scheduled for today?" - Query TestSetDay where Placement.ProductionDay.Date + DayNum = Today
- "Which test days are complete?" - Query TestSetDay where DateTested IS NOT NULL
- "Which days did Pour 6539 span?" - Query SELECT DISTINCT ProductionDayId FROM Placement JOIN MixBatch WHERE PourId = 6539
