# PrecastTracker Domain Glossary

## Industry Context

Precast concrete manufacturing involves creating concrete elements (walls, slabs, beams, tees, etc.) in a controlled factory environment before transporting them to construction sites. Understanding the production workflow is essential to understanding our data model.

## Production Workflow

1. **Job Order**: A construction project orders specific precast elements
2. **Production Scheduling**: Elements are scheduled for production on specific beds (casting surfaces)
3. **Concrete Delivery**: Ready-mix concrete trucks deliver different mixes throughout the day
4. **Casting**: Concrete is poured into forms on beds to create elements
5. **Test Cylinders**: Small test cylinders are filled from each delivery for quality testing
6. **Curing**: Elements and test cylinders are placed in ovens to accelerate curing
7. **Testing**: Cylinders are crushed at specified ages (1 day, 7 days, 28 days) to verify strength

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
- **Code**: Bed identifier (e.g., "16", "2", "13")

**Real-world context:** A precast plant typically has multiple beds (10-50+). Each bed can produce different elements on different days.

**Relationships:**
- A Bed can be used for multiple Pours

---

### Mix Design
A concrete recipe specifying the proportions of cement, aggregates, water, and admixtures.

**Properties:**
- **Code**: Mix identifier (e.g., "824.1", "2515.11")

**Real-world context:** Different structural requirements need different concrete strengths. A 6000 PSI mix for beams is different from a 4500 PSI mix for wall panels.

**Relationships:**
- A Mix Design is used in multiple Elements

---

### Pour
**Definition:** A production run for a specific Job on a specific Bed on a specific Date.

**Properties:**
- **Code**: Pour identifier (e.g., "6497", "6539")
- **JobId**: Which project this production run is for
- **BedId**: Which casting surface is being used
- **Casting Date**: When the concrete was poured

**Real-world context:**
On September 10, 2025, Bed 16 might be used to produce elements for Job "25-009 Dickinson HS". This production run is assigned Pour ID "6539". Throughout the day, multiple concrete trucks arrive with different mixes, each creating different elements on that same bed.

**Important:** A single Pour can have multiple Placements because:
- Different structural elements need different concrete mixes
- Concrete is delivered in batches throughout the day
- Multiple truck loads arrive at different times

**Relationships:**
- A Pour belongs to one Job
- A Pour uses one Bed
- A Pour contains multiple Placements (different concrete deliveries/mixes)

**Example:**
```
Pour 6539 (Bed 16, Job 25-009 Dickinson HS, Date 09/10/2025):
├─ Placement 1: Mix 622.1 delivered at 17:00 by trucks 6,7,8 (31.78 yards)
└─ Placement 2: Mix 2509.1 delivered at 13:15 by trucks 3,4,5 (17.3 yards)
```

---

### Placement
**Definition:** A specific concrete placement within a Pour - represents the act of placing fresh concrete into forms. Each placement is one concrete delivery/batch used to cast specific pieces.

**Industry terminology note:** In the precast concrete industry, "placement" is the standard term for the act of placing fresh concrete into forms. This is distinct from the finished product (the element/piece), which is what gets shipped to the job site.

**Properties:**
- **PourId**: Which production run this belongs to
- **MixDesignId**: Which concrete recipe was used
- **YardsPerBed**: Volume of concrete delivered (cubic yards)
- **Batching Start Time**: When this concrete batch was prepared
- **Truck Numbers**: Which ready-mix trucks delivered this concrete (e.g., "1, 2, 3, 4")
- **Piece Type**: What's being cast (Walls, Slabs, Tees, etc.)
- **OvenId**: Which curing oven will be used

**Real-world context:**
During Pour 6539, at 13:15, trucks 3, 4, and 5 arrive with 17.3 cubic yards of Mix 2509.1. This concrete is used to cast wall panels. Test cylinders from this delivery are placed in Oven 2. This entire concrete placement is one Placement.

Later that same day at 17:00, trucks 6, 7, and 8 arrive with 31.78 cubic yards of Mix 622.1 for different wall panels. This is a separate Placement in the same Pour.

**Why Placement matters:**
Each concrete delivery must be tested independently for quality control. If one batch fails testing, only the finished pieces (elements) from that specific placement are affected.

**Relationships:**
- A Placement belongs to one Pour
- A Placement uses one Mix Design
- A Placement has multiple Concrete Tests (different test ages and cylinder types)

---

### Concrete Test
**Definition:** A strength test performed on a concrete test cylinder at a specific age.

**Properties:**
- **PlacementId**: Which concrete placement this test is from
- **Test Code**: Test identifier (e.g., "9005", "9005.1", "9005.2")
- **Cylinder ID**: Type/age indicator:
  - **1C**: 1-day accelerated cure test
  - **7C**: 7-day standard cure test
  - **28C**: 28-day standard cure test
- **Age of Test**: Actual age when tested (e.g., "7", "28", "0d 12:53" for hours)
- **Testing Date**: When the cylinder was crushed
- **Required PSI**: Target strength the concrete must achieve
- **Break #1, #2, #3**: Individual test results (PSI) from crushing cylinders
- **Average PSI**: Mean of the break results
- **Comments**: Any notes about the test
- **Oven ID**: Which oven the test cylinder was cured in

**Real-world context:**
From Placement 1 (Mix 2509.1, 17.3 yards, Time 13:15), three test cylinders are filled:
- One cylinder (1C) goes in an accelerated-heat oven, tested in ~12-24 hours
- One cylinder (7C) cures normally, tested after 7 days
- One cylinder (28C) cures normally, tested after 28 days

Each cylinder is crushed in a compression machine, producing 2-3 test results that are averaged.

**Test Code numbering:**
- `9011`: Primary test series
- `9011.1`: Secondary test from same placement (additional verification)
- `9011.2`: Third test from same placement

**Why multiple tests matter:**
- 1-day tests allow quick release of elements from beds (faster production)
- 7-day tests verify early strength development
- 28-day tests verify final design strength
- Multiple cylinders ensure test reliability

**Relationships:**
- A Test belongs to one Element
- Tests with the same base code (e.g., 9011, 9011.1) are from the same Element

---

## Example: Complete Data Flow

**Scenario:** Dickinson High School needs precast wall panels.

1. **Job Created:** "25-009 Dickinson HS"

2. **Production Scheduled:** September 10, 2025, Bed 16

3. **Pour Begins:** Pour ID "6539" starts on Bed 16 for Job 25-009

4. **Morning Delivery (Element 1):**
   - Time: 13:15
   - Trucks 3, 4, 5 deliver 17.3 yards of Mix 2509.1
   - Used to cast wall panels
   - 3 test cylinders filled → Tests 9011 (7C), 9011 (28C), 9011.1 (1C)
   - Cylinders placed in Oven 2

5. **Afternoon Delivery (Element 2):**
   - Time: 17:00
   - Trucks 6, 7, 8 deliver 31.78 yards of Mix 622.1
   - Used to cast different wall panels
   - 1 test cylinder filled → Test 9010.2 (1C)
   - Cylinder placed in Oven 16

6. **Testing:**
   - Test 9011.1 (1C): Crushed after 12 hours → 3580, 3503 PSI (avg 3542)
   - Test 9011 (7C): Crushed after 7 days → 6463, 6427 PSI (avg 6445)
   - Test 9011 (28C): Scheduled for 28 days
   - Test 9010.2 (1C): Crushed after ~9 hours → 4419, 4305 PSI (avg 4368)

**Result:** Pour 6539 contains 2 Elements with 4 total Tests, ensuring quality control for all concrete delivered that day.

---

## Data Relationships Summary

```
Job (1) ──< (M) Pour
Bed (1) ──< (M) Pour
MixDesign (1) ──< (M) Element
Pour (1) ──< (M) Element
Element (1) ──< (M) ConcreteTest
```

**Key Insight:** The Pour → Element → Test hierarchy ensures traceability from the final strength test result all the way back to which truck delivered which concrete at what time for which job.
