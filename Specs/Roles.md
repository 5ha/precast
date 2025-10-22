# PrecastTracker User Roles

This document defines the roles that directly interact with the PrecastTracker application for data capture and review.

## Application Roles

### 1. **SCHEDULER**
**Application Activities:**
- Create/view **Jobs**
- Create **Pours** (assign Job to Bed)
- View **Bed** availability
- Create **ProductionDay** records
- View/update **Bed** status

---

### 2. **BATCHER**
**Application Activities:**
- View **MixDesign** recipes
- Create **MixBatch** records (mix + date)
- View **ProductionDay** schedule

---

### 3. **PLACER**
**Application Activities:**
- View **Pour** assignments
- Create **Placement** records (PieceType, StartTime, Volume, OvenId, MixBatchId)
- Create **TestSet** for each Placement
- Create **TestSetDay** records (1, 7, 28 days)
- Create **TestCylinder** records with codes

---

### 4. **TESTER**
**Application Activities:**
- View test queue (**TestSetDay** where DateDue is near/overdue)
- Update **TestCylinder.BreakPsi** values
- Update **TestSetDay.DateTested**
- Add **TestSetDay.Comments**
- View untested placements report

**Status:** ✅ Already implemented

---

### 5. **SUPERVISOR**
**Application Activities:**
- View all **TestSetDay** results
- Analyze **ConcreteReport**
- Review pass/fail status
- View test trends by **MixDesign**
- View failed tests requiring action

---

### 6. **ENGINEER**
**Application Activities:**
- Create/update **MixDesign** recipes
- Create/update **MixDesignRequirement** (strength targets)
- Analyze **TestSetDay** patterns
- View **ConcreteReport** trends

---

### 7. **SHIPPER**
**Application Activities:**
- View **Pour** completion status
- View **TestSetDay** completion (DateTested not null)
- View **Placement** details for shipping docs
- Generate quality certificates from test data

---

## Role Summary

**7 Application Roles** that directly capture or review data in the system:

1. **SCHEDULER** - Creates Jobs, Pours, ProductionDays, manages Bed schedules
2. **BATCHER** - Views MixDesigns, creates MixBatch records
3. **PLACER** - Creates Placements and all test structures (TestSet, TestSetDay, TestCylinder)
4. **TESTER** - Updates test results and completion status
5. **SUPERVISOR** - Reviews all quality data and reports
6. **ENGINEER** - Manages MixDesigns and strength requirements
7. **SHIPPER** - Views completion status for release decisions

---

## Notes

- Plant Manager and Project Manager roles perform oversight but do not directly interact with the application for data entry
- Truck Dispatcher role may be handled through external dispatch systems
- These roles represent distinct application functions - actual staff may perform multiple roles depending on plant size
