# Scheduler Dashboard and Workflow

## Scheduler's Role in Precast Concrete Production

In the precast concrete industry, the **scheduler** (also called production planner or production coordinator) is the "air traffic controller" of the precast plant. They solve a complex puzzle: matching customer orders to available production capacity while considering delivery deadlines, bed availability, and crew capabilities.

### Core Responsibilities:
- Manage job backlog and delivery commitments
- Assign jobs to beds (create Pours)
- Balance workload across production lines
- Handle schedule conflicts and urgent orders
- Track bed availability and maintenance windows
- Ensure production capacity meets demand

---

## Scheduler's Dashboard - What They See on Login

Based on the data model, here's what a scheduler needs to see when they log in:

### 1. **Weekly Production Calendar** (Priority #1)

```
Production Schedule - Week of Oct 22-28, 2025
┌──────────┬─────────────────┬─────────────────┬─────────────────┐
│ Bed      │ Mon 10/22       │ Tue 10/23       │ Wed 10/24       │
├──────────┼─────────────────┼─────────────────┼─────────────────┤
│ Bed 2    │ 25-009 Dickinson│ 25-009 Dickinson│ [Available]     │
│          │ ⏱️ Day 2 of 3    │ ⏱️ Day 3 of 3    │                 │
├──────────┼─────────────────┼─────────────────┼─────────────────┤
│ Bed 13   │ 25-020 Woodbury │ [Available]     │ [Available]     │
│          │ ⏱️ Day 1 of 1    │                 │                 │
├──────────┼─────────────────┼─────────────────┼─────────────────┤
│ Bed 16   │ 🔧 Maintenance   │ 🔧 Maintenance   │ [Available]     │
├──────────┼─────────────────┼─────────────────┼─────────────────┤
│ Bed 18   │ [Available]     │ [Available]     │ [Available]     │
└──────────┴─────────────────┴─────────────────┴─────────────────┘

Legend: [Available] | Scheduled | 🔧 Maintenance | 🚫 Decommissioned
```

**Why this matters:** Visual calendar shows capacity at a glance. Schedulers need to see conflicts immediately.

---

### 2. **Unscheduled Jobs Queue** (Alert Section)

Jobs awaiting assignment, sorted by urgency:

```
Jobs Awaiting Schedule
┌──────────┬────────────────────┬───────────┬──────────────┬──────────┐
│ Job Code │ Name               │ Due Date  │ Est. Days    │ Priority │
├──────────┼────────────────────┼───────────┼──────────────┼──────────┤
│ 25-045   │ Lincoln Mall       │ 10/25 🔴  │ 2 days       │ URGENT   │
│ 25-032   │ Roosevelt Arena    │ 10/30     │ 3 days       │ HIGH     │
│ 25-018   │ Jefferson Tower    │ 11/05     │ 5 days       │ NORMAL   │
└──────────┴────────────────────┴───────────┴──────────────┴──────────┘
```

**Visual indicators:**
- 🔴 Red badge: Due within 3 days
- 🟠 Orange badge: Due within 1 week
- 🟢 Green: Due in 1+ weeks

**Industry insight:** Missing delivery deadlines damages customer relationships. This queue keeps urgent jobs visible.

---

### 3. **Bed Status Overview**

Quick status of all production beds:

```
Bed Availability Summary
┌─────────────┬──────────┬───────────────────────────────────────┐
│ Status      │ Count    │ Beds                                  │
├─────────────┼──────────┼───────────────────────────────────────┤
│ Available   │ 12       │ 1, 4, 5, 7, 8, 9, 10, 11, 14, 17...   │
│ Scheduled   │ 6        │ 2, 3, 6, 13, 15, 20                   │
│ Maintenance │ 2        │ 16, 19                                │
│ Decommissioned │ 1     │ 22                                    │
└─────────────┴──────────┴───────────────────────────────────────┘
```

---

### 4. **Recent Schedule Changes**

Audit trail for accountability:

```
Recent Activity
┌──────────────┬───────────┬────────────────────────────────────┐
│ Time         │ User      │ Action                             │
├──────────────┼───────────┼────────────────────────────────────┤
│ 2 hrs ago    │ Maria S.  │ Scheduled Pour 6540 (25-045)       │
│ 4 hrs ago    │ John T.   │ Moved Pour 6538 from Bed 2 → Bed 7 │
│ Yesterday    │ Maria S.  │ Bed 16 set to Maintenance          │
└──────────────┴───────────┴────────────────────────────────────┘
```

---

## Optimized Scheduler Workflow

### Morning Routine:
1. **Login** → See weekly calendar with all bed assignments
2. **Review alerts** → Check unscheduled jobs and approaching deadlines
3. **Check bed status** → Note maintenance schedules and availability
4. **Plan assignments** → Assign jobs to available beds
5. **Coordinate** → Communicate schedule to plant manager and crews
6. **Monitor** → Track progress throughout the day

---

### Key Workflow Moments:

#### **Scenario A: Scheduling a New Job**

```
1. Scheduler sees "25-045 Lincoln Mall" in Unscheduled Jobs (DUE: 10/25 🔴)
2. Clicks job → Opens job details modal:
   - Job Code: 25-045
   - Name: Lincoln Mall Expansion
   - Piece Type: Walls
   - Estimated Production Days: 2
   - Delivery Deadline: 10/25
   - Special Requirements: (none)

3. Clicks "Schedule Job" button
4. System shows calendar view with available beds highlighted
5. Scheduler selects Bed 18 and start date 10/23
6. System prompts:
   "Schedule Pour for Job 25-045 on Bed 18?"
   Start Date: 10/23
   Estimated End: 10/24 (2 days)
   [Confirm] [Cancel]

7. Clicks Confirm → Pour 6540 created
8. Calendar updates → Bed 18 now shows "25-045 Lincoln Mall" on 10/23-10/24
9. Job moves from Unscheduled to Scheduled
10. Casting crew sees new Pour in their upcoming schedule
```

---

#### **Scenario B: Handling a Schedule Conflict (Maintenance)**

```
1. Plant Manager submits maintenance request: "Bed 16 needs hydraulic repair"
2. Scheduler sees notification: "Maintenance requested for Bed 16"
3. Clicks Bed 16 on calendar → Opens bed detail view:
   - Current Status: Scheduled (Pour 6538 - 25-020 Woodbury)
   - Days Remaining: 1 day
   - Can complete before maintenance

4. Scheduler decides to wait until Pour 6538 completes
5. Schedules maintenance starting 10/23
6. Clicks Bed 16 → "Set Bed Status" → Maintenance
7. Modal prompts:
   "Set Bed 16 to Maintenance?"
   Start Date: 10/23
   Estimated Duration: 2 days
   Reason: Hydraulic repair
   [Confirm] [Cancel]

8. Clicks Confirm → Bed status updated to Maintenance
9. Calendar updates → Bed 16 shows 🔧 for 10/23-10/24
10. System prevents scheduling new pours on Bed 16 during maintenance
11. Any jobs planned for Bed 16 during this period trigger conflict alert
```

---

#### **Scenario C: Urgent Order Requires Quick Scheduling**

```
1. Sales calls: "Rush order! 25-055 must start today!"
2. Scheduler creates new Job: 25-055 "Emergency Medical Center"
3. Opens calendar view, filters for available beds today
4. Sees Bed 4 currently available
5. Clicks Bed 4 cell for today → "Schedule Pour"
6. Selects Job 25-055, confirms start date = today
7. System creates Pour 6541 with ProductionDay = today
8. Scheduler notifies:
   - Batch plant operator (prepare concrete)
   - Casting crew (prioritize setup for Bed 4)
   - Plant manager (expedited schedule)
9. Production begins same day
```

---

#### **Scenario D: Viewing Pour Progress**

```
1. Scheduler clicks on scheduled Pour on calendar
2. System shows Pour Details modal:
   - Pour ID: 6539
   - Job: 25-009 Dickinson HS
   - Bed: 16
   - Status: In Progress
   - Started: 10/21
   - Estimated Completion: 10/23
   - Placements Created: 2
     • 10/21 - Walls, 17.3 cu yd, Mix 2509.1
     • 10/21 - Walls, 31.78 cu yd, Mix 622.1

3. Scheduler sees production has started
4. Confirms schedule is on track
5. Can adjust downstream scheduling based on actual progress
```

---

## Dashboard Layout

```
╔═══════════════════════════════════════════════════════════════╗
║  PrecastTracker - Scheduler Dashboard      [Profile] [🔔 2]   ║
╠═══════════════════════════════════════════════════════════════╣
║                                                                 ║
║  ⚠️ ALERTS                                                      ║
║  • 1 job due in 3 days (unscheduled)  • Bed 16 maintenance    ║
║                                                                 ║
║  📅 PRODUCTION CALENDAR                    [+ Schedule Job]    ║
║  Week View: Oct 22-28, 2025        [◀ Prev Week] [Next Week ▶]║
║  ┌──────────────────────────────────────────────────────────┐ ║
║  │ Bed 2  │ 25-009 │ 25-009 │ [Avail] │ [Avail] │ [Avail] │ ║
║  │        │ Day 2/3│ Day 3/3│         │         │         │ ║
║  ├────────┼────────┼────────┼─────────┼─────────┼─────────┤ ║
║  │ Bed 13 │ 25-020 │ [Avail]│ [Avail] │ [Avail] │ [Avail] │ ║
║  │        │ Day 1/1│        │         │         │         │ ║
║  ├────────┼────────┼────────┼─────────┼─────────┼─────────┤ ║
║  │ Bed 16 │🔧 Maint│🔧 Maint│ [Avail] │ [Avail] │ [Avail] │ ║
║  ├────────┼────────┼────────┼─────────┼─────────┼─────────┤ ║
║  │ Bed 18 │ [Avail]│ [Avail]│ [Avail] │ [Avail] │ [Avail] │ ║
║  └────────┴────────┴────────┴─────────┴─────────┴─────────┘ ║
║                                                                 ║
║  📋 UNSCHEDULED JOBS (3)                   [View All Jobs]     ║
║  ┌──────────────────────────────────────────────────────────┐ ║
║  │🔴 25-045  Lincoln Mall      Due: 10/25  (2d)   [Schedule]│ ║
║  │🟠 25-032  Roosevelt Arena   Due: 10/30  (3d)   [Schedule]│ ║
║  │🟢 25-018  Jefferson Tower   Due: 11/05  (5d)   [Schedule]│ ║
║  └──────────────────────────────────────────────────────────┘ ║
║                                                                 ║
║  🏭 BED STATUS SUMMARY                                          ║
║  • Available: 12 beds   • Scheduled: 6 beds                    ║
║  • Maintenance: 2 beds  • Decommissioned: 1 bed                ║
║                                                [Manage Beds]    ║
║                                                                 ║
║  📋 RECENT ACTIVITY                                             ║
║  • 2h ago - Maria S. scheduled Pour 6540 (25-045)              ║
║  • 4h ago - John T. moved Pour 6538 from Bed 2 → Bed 7         ║
║                                          [View Schedule History]║
║                                                                 ║
║  🔍 SEARCH JOBS  |  📊 CAPACITY REPORT  |  📋 ALL JOBS         ║
╚═══════════════════════════════════════════════════════════════╝
```

---

## Critical Data Model Additions Needed

Looking at the current data model for scheduler workflow:

### Entities Scheduler Creates:
- **Job** - Customer orders (Code, Name, DeliveryDeadline)
- **Pour** - Job-to-Bed assignment (JobId, BedId)
- **ProductionDay** - Production date records

### Missing Fields:

**Job Entity:**
1. **DeliveryDeadline** - When customer needs the pieces (nullable Date)
2. **EstimatedProductionDays** - How many days to complete (nullable int)
3. **PieceType** - What will be produced (Walls, Slabs, Tees, etc.) - for bed filtering
4. **CustomerName** - Who ordered this job
5. **Priority** - Urgency indicator (Normal, High, Urgent)

**Pour Entity:**
1. **EstimatedStartDate** - Planned start (nullable Date)
2. **EstimatedEndDate** - Planned completion (nullable Date)
3. **Status** - Enum: Scheduled, InProgress, Completed, Cancelled

**Bed Entity:**
1. **Status** - Enum: Active, Maintenance, Decommissioned (already implemented)
2. **Capabilities** - Which piece types can this bed handle (future enhancement)

---

## Dashboard Views Breakdown

### Primary View: Production Calendar
- **Time range:** Current week (7 days) or Month view
- **Display fields:** Bed rows, Date columns, Pour assignments
- **Visual indicators:**
  - Color coding by job
  - Icons for bed status (🔧 Maintenance, 🚫 Decommissioned)
  - Progress indicators (Day X of Y)
- **Actions:** Click cell to schedule, click pour to view details
- **Sort order:** Beds sorted by BedId numerically

### Secondary View: Unscheduled Jobs Queue
- **Sort order:** By delivery deadline (urgent first)
- **Display fields:** Job Code, Name, Due Date, Estimated Days, Priority
- **Actions:** Schedule button for each job
- **Visual indicators:** Color badges for urgency (red/orange/green)
- **Filter options:** By priority, by piece type, by delivery date range

### Tertiary View: Bed Status Summary
- **Display:** Grouped counts by status
- **Purpose:** Quick capacity overview
- **Actions:** Click to filter calendar by status

### Support View: Recent Activity
- **Time range:** Last 24-48 hours
- **Display fields:** Timestamp, User, Action description
- **Purpose:** Audit trail and coordination awareness

### Utility View: Bed Management
- **List view:** All beds with current status
- **Display fields:** BedId, Status, Current Pour, Next Available Date
- **Actions:** Set status, view history, decommission bed
- **Filter options:** By status, by availability date range

---

## Key Screens & Components

### Screen 1: Production Calendar (Main View)
- **Layout:** Week view with beds as rows, days as columns
- **Interaction:** Click cells to schedule, click pours to view details
- **Visual cues:**
  - Color coding by job
  - Icons for bed status
  - Duration indicators (Day 1 of 3, Day 2 of 3)
  - Available cells show [Available]
- **Actions:** Click cell to schedule, click pour to edit

### Screen 2: Schedule Pour Modal
- **Triggered by:** Clicking "Schedule Job" or [Available] calendar cell
- **Fields:**
  - Job dropdown (autocomplete with code + name) - filtered to unscheduled only
  - Bed selection (shows selected if triggered from calendar cell)
  - Start date picker
  - Estimated duration (auto-calculated from job if available)
- **Validation:**
  - Bed must be available on selected dates
  - Warn if delivery deadline cannot be met
  - Prevent double-booking
- **Actions:** [Confirm Schedule] [Cancel]

### Screen 3: Pour Details Modal
- **Triggered by:** Clicking a scheduled pour on calendar
- **Display:**
  - Pour ID
  - Job Code, Name, Customer
  - Bed assignment
  - Scheduled Start/End dates
  - Status (Scheduled, InProgress, Completed)
  - Placements created (if casting has begun)
    • Date, Piece Type, Volume, Mix Design
  - Test status for each placement
- **Actions:** [Edit Schedule] [Cancel Pour] [View Placements] [Close]

### Screen 4: Create/Edit Job Modal
- **Triggered by:** Clicking "+ Schedule Job" or editing existing job
- **Fields:**
  - Job Code (required, unique)
  - Job Name (required)
  - Customer Name
  - Piece Type (dropdown: Walls, Slabs, Tees, Beams, etc.)
  - Estimated Production Days
  - Delivery Deadline (date picker)
  - Priority (dropdown: Normal, High, Urgent)
  - Special Requirements (text area)
- **Actions:** [Save Job] [Cancel]

### Screen 5: Bed Management View
- **List of all beds:**
  - BedId
  - Current Status (Available, Scheduled, Maintenance, Decommissioned)
  - Current Pour (if scheduled) - shows Job Code
  - Next Available Date (calculated)
  - Actions: [Set Status] [View History]
- **Actions:** [Add New Bed] [Export Bed Report]

### Screen 6: Set Bed Status Modal
- **Triggered by:** Clicking "Set Status" on a bed
- **Fields:**
  - Status dropdown (Available, Maintenance, Decommissioned)
  - Start date (defaults to today)
  - End date (nullable - for maintenance duration)
  - Reason/Notes (text area)
- **Validation:**
  - Warn if bed has scheduled pours during maintenance period
  - Require reason for decommissioning
- **Actions:** [Confirm] [Cancel]

---

## Critical Features for Scheduler

### 1. **Visual Calendar Interface**
- Clear, intuitive week/month view
- Click-to-schedule interaction
- Real-time conflict detection
- Color coding for easy recognition

### 2. **Capacity Planning**
- See bed utilization at a glance
- Identify bottlenecks (all beds scheduled)
- Balance workload across beds
- Track maintenance windows

### 3. **Conflict Detection & Validation**
- Alert if scheduling during maintenance
- Warn if delivery deadline cannot be met
- Prevent double-booking same bed
- Highlight beds unavailable for selected dates

### 4. **Timeline Visualization**
- Multi-day pours span across calendar
- Clear start/end dates
- Progress indicators for active pours (Day X of Y)
- Distinguish between scheduled and in-progress

### 5. **Quick Filters & Search**
- Show only available beds
- Filter by piece type compatibility
- View specific date ranges
- Search jobs by code or name

### 6. **Urgency Management**
- Visual priority indicators (red/orange/green)
- Auto-sort by delivery deadline
- Alert for jobs due soon without schedule
- Track time until deadline

### 7. **Audit Trail**
- Track who scheduled what and when
- View schedule change history
- Accountability for coordination
- Communication trail

---

## Critical Queries for Scheduler Dashboard

### Query 1: Get Production Calendar Data
```
Get all Pours for date range with Job and Bed details
- Include: Pour, Job (Code, Name), Bed, ProductionDays
- Where: ProductionDay.Date BETWEEN startDate AND endDate
- Order by: Bed.BedId, ProductionDay.Date
- Purpose: Populate calendar grid
```

### Query 2: Get Unscheduled Jobs
```
Get all Jobs without Pours
- Where: Job has no related Pour records
- Order by: DeliveryDeadline ASC (most urgent first)
- Include: Job Code, Name, DeliveryDeadline, EstimatedDays, Priority
- Purpose: Show jobs awaiting schedule
```

### Query 3: Get Bed Availability
```
Get Beds with current status
- Include: Bed
- Where: Bed.Status filter as needed
- Group by: Status
- Purpose: Show bed availability summary
```

### Query 4: Get Pour Progress
```
Get Pour with Placement count
- Include: Pour, Job, Bed, COUNT(Placements)
- Where: PourId = selected
- Purpose: Show if casting has begun and progress
```

### Query 5: Get Bed Schedule
```
Get all Pours for specific Bed
- Include: Pour, Job, ProductionDays
- Where: BedId = selected
- Order by: ProductionDay.Date DESC
- Purpose: View bed history and upcoming schedule
```

---

## Notification Rules

### When to Notify Scheduler:
- Job approaching delivery deadline without schedule (3 days warning)
- Bed maintenance request submitted
- Pour cancelled or delayed by production
- New urgent job added to backlog

### When to Notify Plant Manager:
- Scheduler created/modified production schedule
- Bed status changed to maintenance or decommissioned
- Urgent job scheduled for immediate production
- Schedule conflict detected requiring resolution

### When to Notify Casting Crew:
- New Pour scheduled for their bed
- Pour start date approaching (1 day warning)
- Pour schedule changed (different bed or date)
- Bed status changed to maintenance (affects their work)

---

## Industry Best Practices

### Scheduling Principles:
1. **Load Balancing** - Distribute work evenly across beds to maximize throughput
2. **Buffer Time** - Build in cushion for unexpected delays or quality issues
3. **Maintenance Windows** - Schedule bed maintenance during low-demand periods
4. **Rush Order Protocol** - Reserve capacity for urgent customer needs
5. **Communication** - Proactively notify all affected parties of schedule changes

### Common Challenges:
- **Overbooking** - Promising delivery dates without available capacity
- **Maintenance Conflicts** - Beds needing repair during scheduled production
- **Weather Delays** - Outdoor curing affected by temperature/precipitation
- **Material Shortages** - Concrete supply issues delaying scheduled pours
- **Equipment Failures** - Unplanned downtime affecting schedule

### Success Metrics:
- **On-Time Delivery Rate** - % of jobs meeting delivery deadline
- **Bed Utilization** - % of time beds are actively producing
- **Schedule Stability** - How often schedule changes after initial plan
- **Lead Time Accuracy** - Estimated vs actual production time
