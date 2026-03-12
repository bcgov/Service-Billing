# Service Billing Application - Features & Fixes Documentation
**Period:** February 23, 2025 - March 2025  
**Target Framework:** .NET 8  
**Project Type:** Razor Pages

---

## Table of Contents
1. [Quantity Calculation Enhancements](#1-quantity-calculation-enhancements)
2. [Date Validation & Data Integrity](#2-date-validation--data-integrity)
3. [Charge Creation & Fiscal History](#3-charge-creation--fiscal-history)
4. [Charge Editing & Auto-Promotion](#4-charge-editing--auto-promotion)
5. [Quarter Views & Filtering](#5-quarter-views--filtering)
6. [Reports & Excel Export](#6-reports--excel-export)
7. [Bug Fixes](#7-bug-fixes)
8. [User Experience Improvements](#8-user-experience-improvements)

---

## 1. Quantity Calculation Enhancements

### 1.1 Quarter-Aware Quantity Calculation
**Issue:** Charges created with dates in previous quarters had incorrect quantities calculated.

**Solution:**
- Modified `CreateBill` method to calculate quantity based on the **actual quarter** the charge belongs to, not just current quarter
- Quantity respects quarter start/end boundaries
- Example: Charge StartDate=Nov 1, EndDate=Dec 30 → Quantity=2 (Nov, Dec) in Q3

**Files Modified:**
- `Models/Repositories/BillRepository.cs` (lines 475-488)

**Code:**
```csharp
// Only recalculate for month-based services
if (fiscalPeriod != null && bill.ServiceCategory != null && 
    !string.IsNullOrEmpty(bill.ServiceCategory.UOM) && 
    bill.ServiceCategory.UOM.Equals("month", StringComparison.OrdinalIgnoreCase))
{
    DateTime periodStart = DetermineStartOfQuarterForPeriod(fiscalPeriod.Period);
    DateTime periodEnd = DetermineEndOfQuarter(periodStart);
    newBill.Quantity = CalculateQuantityForQuarter(bill, periodStart, periodEnd);
}
```

### 1.2 Non-Month UOM Support
**Issue:** Hour-based and other non-month services had their quantities overwritten during save.

**Solution:**
- Added UOM check before recalculating quantities
- Only month-based services (UOM="month") get auto-calculated
- Hour, Instance, License, etc. preserve user-entered quantities

**Example:**
- 8 hours @ $100/hour → Quantity stays 8, not changed to 3 months

**Files Modified:**
- `Models/Repositories/BillRepository.cs` (CreateBill, Update methods)

### 1.3 Quantity Explanation UI
**Feature:** Real-time quantity explanation when creating/editing charges.

**Implementation:**
- Added `<div id="quantityExplanation">` to Create and Edit views
- JavaScript updates explanation when dates change
- Shows helpful messages like: *"Billing for 2 month(s) in Fiscal 25/26 Quarter 3 (from Nov 1 to Dec 30, 2025)"*

**Files Modified:**
- `Views/Bills/Create.cshtml` (line 64)
- `Views/Bills/Edit.cshtml` (line 66)
- JavaScript in both views

---

## 2. Date Validation & Data Integrity

### 2.1 StartDate Range Validation
**Feature:** Prevent charges from being created/edited with invalid start dates.

**Rules:**
- **Minimum:** Previous quarter start
- **Maximum:** Next quarter end
- **Reasoning:** Maintains data integrity, prevents backdating abuse

**Error Messages:**
- Too early: *"Start date cannot be earlier than the previous fiscal period (Fiscal XX/YY Quarter Z)"*
- Too late: *"Start date cannot be beyond the next fiscal period (Fiscal XX/YY Quarter Z)"*

**Files Modified:**
- `Controllers/BillsController.cs` (Create POST, Edit POST)

### 2.2 EndDate Validation
**Feature:** Ensure EndDate is logically valid.

**Rule:** EndDate cannot be before StartDate

**Error Message:** *"End date cannot be before the start date."*

**Files Modified:**
- `Controllers/BillsController.cs` (lines 458-475 Create, lines 319-323 Edit)

### 2.3 Historical Data Protection
**Feature:** Protect charges with historical fiscal records from date changes.

**Logic:**
- If charge StartDate is before previous quarter → **Cannot be changed**
- If charge StartDate is within editable range → **Can be changed within limits**
- User can revert to original StartDate → **Validation passes**

**Error Message:** *"This charge's start date has been recognized in historical fiscal periods and cannot be changed. Changing this date would corrupt historical billing records."*

**Files Modified:**
- `Controllers/BillsController.cs` (Edit POST, lines 275-317)

### 2.4 Client-Side Validation Clearing
**Feature:** Validation errors clear immediately when user corrects the input.

**Implementation:**
- Date field changes trigger validation error removal
- Provides immediate feedback to users
- Reduces confusion about whether error is still valid

**Files Modified:**
- `Views/Bills/Create.cshtml` (JavaScript)
- `Views/Bills/Edit.cshtml` (JavaScript)

---

## 3. Charge Creation & Fiscal History

### 3.1 Fiscal Period Assignment
**Feature:** Charges are automatically assigned to the correct fiscal period based on StartDate.

**Logic:**
```
If StartDate in Q3 (Oct-Dec) → Assign to Q3
If StartDate in Q4 (Jan-Mar) → Assign to Q4
If StartDate in Q1 (Apr-Jun) → Assign to Q1
If StartDate in Q2 (Jul-Sep) → Assign to Q2
```

**Files Modified:**
- `Controllers/BillsController.cs` (Create POST, lines 483-503)

### 3.2 FiscalHistory Creation for Previous Quarters
**Issue:** Charges created in previous quarters that stayed there had no FiscalHistory record.

**Solution:**
- When charge starts in previous quarter AND ends before current quarter
- Create FiscalHistory record immediately
- Store quantity, unit price, and notes for that quarter

**Example:**
- Create charge: StartDate=Nov 1, EndDate=Dec 30 (Q3)
- Current quarter: Q4
- Result: Charge stays in Q3, FiscalHistory created for Q3

**Files Modified:**
- `Controllers/BillsController.cs` (Create POST, lines 517-538)

### 3.3 Automatic Promotion Logic
**Feature:** Charges that span quarters are automatically promoted.

**Scenarios:**

| StartDate | EndDate | Current Quarter | Result |
|-----------|---------|----------------|--------|
| Nov 1 | Dec 30 | Q4 | Stay in Q3 (not promoted) |
| Nov 1 | Feb 15 | Q4 | Promote to Q4, history in Q3 |
| Nov 1 | (none) | Q4 | Promote to Q4, history in Q3 |
| Jan 1 | (none) | Q4 | Stay in Q4 (no promotion) |

**Files Modified:**
- `Controllers/BillsController.cs` (Create POST, lines 509-561)

### 3.4 Duplicate FiscalHistory Prevention
**Issue:** FiscalHistory was being created twice for charges in previous quarters.

**Solution:**
- Only create FiscalHistory manually for charges that **stay** in previous quarter
- Let PromoteCharge create FiscalHistory for charges that get **promoted**
- Added duplicate check in `AddBillFiscalHistoryToContext`

**Files Modified:**
- `Controllers/BillsController.cs` (Create POST logic)
- `Models/Repositories/BillRepository.cs` (AddBillFiscalHistoryToContext, lines 270-282)

---

## 4. Charge Editing & Auto-Promotion

### 4.1 Date Change Detection
**Feature:** System detects when start or end dates are modified.

**Implementation:**
- Load original bill from database
- Compare submitted dates with original
- Only recalculate/validate if dates actually changed

**Files Modified:**
- `Models/Repositories/BillRepository.cs` (Update method, lines 510-545)

### 4.2 Quantity Recalculation on Edit
**Feature:** When editing dates for month-based charges, quantity auto-updates.

**Scenarios:**
- **Current quarter charge:** Change EndDate → Recalculate quantity for current quarter
- **Non-month service:** Date changes don't affect quantity
- **Historical charge:** Protected from date changes (see 2.3)

**Files Modified:**
- `Models/Repositories/BillRepository.cs` (Update method, lines 523-545)

### 4.3 Auto-Promotion on Date Edit
**Feature:** Editing a charge's dates can trigger promotion to current quarter.

**Example:**
- Charge in Q3: StartDate=Nov 1, EndDate=Dec 30
- Edit EndDate to Feb 15
- Result: Charge promoted to Q4, FiscalHistory created for Q3

**Logic:**
```csharp
bool shouldPromote = (editedBill.EndDate == null || editedBill.EndDate.Value >= currentQuarterStart);
```

**Files Modified:**
- `Models/Repositories/BillRepository.cs` (Update method, lines 542-567)

---

## 5. Quarter Views & Filtering

### 5.1 Fixed "Current Quarter" Filter Bug
**Issue:** Selecting "current" filter showed wrong quarter label.

**Root Cause:** JavaScript was setting "Current Quarter" as the text, but backend expected "current" (lowercase).

**Solution:** Changed condition from string comparison to checking `searchParams.QuarterFilter == "current"`

**Files Modified:**
- `Controllers/BillsController.cs` (QueryForCharges method, lines 885-896)

### 5.2 Previous Quarter Query Enhancement
**Issue:** Charges that stayed in previous quarter (no FiscalHistory) weren't showing.

**Solution:** Include both:
1. Charges with FiscalHistory for that quarter (promoted FROM)
2. Charges with CurrentFiscalPeriodId for that quarter (stayed IN)

**Query Logic:**
```csharp
query = query.Where(b => billIdsWithHistory.Contains(b.Id) || 
                         b.CurrentFiscalPeriodId == previousFiscalPeriod.Id);
```

**Files Modified:**
- `Controllers/BillsController.cs` (QueryForCharges, lines 907-922, lines 938-955)

### 5.3 Next Quarter Visibility Fix
**Issue:** Charges ending exactly on quarter start date (e.g., April 1) didn't show in Next Quarter view.

**Root Cause:** Condition used `>` instead of `>=`

**Solution:** Changed to `>=` for both view query and promotion logic

**Example:** Charge with EndDate=April 1 now appears in Q1 (Apr-Jun) view

**Files Modified:**
- `Models/Repositories/BillRepository.cs` (line 203)
- `Controllers/BillsController.cs` (line 928)

### 5.4 Historical Quarter Dropdown Cleanup
**Feature:** Next quarter no longer appears in historical quarters dropdown.

**Reasoning:** Next quarter should use "Next" filter, not be in historical list.

**Implementation:**
```csharp
string nextQuarterString = _billRepository.DetermineCurrentQuarter(_billRepository.DetermineStartOfNextQuarter());
fiscalPeriodsStrings.Remove(nextQuarterString);
```

**Files Modified:**
- `Controllers/BillsController.cs` (Index method, lines 72-81)

---

## 6. Reports & Excel Export

### 6.1 Historical Quarter Report Fix
**Issue:** Reports for historical quarters (e.g., "Fiscal 24/25 Quarter 3") showed wrong amounts.

**Root Cause:** `searchParams.QuarterString` was never set in ShowReport and WriteToExcel methods.

**Solution:** Added same initialization logic as GetBillsTable:
```csharp
if (searchParams != null && !string.IsNullOrEmpty(searchParams.QuarterFilter) &&
    searchParams.QuarterFilter.StartsWith("Fiscal"))
    searchParams.QuarterString = searchParams.QuarterFilter;
```

**Files Modified:**
- `Controllers/BillsController.cs` (ShowReport, WriteToExcel methods)

### 6.2 On-the-Fly Amount Calculation
**Feature:** Charges without FiscalHistory get amounts calculated dynamically for historical quarters.

**Scenario:**
- 267 charges in Q3, but only 265 have FiscalHistory
- Those 2 charges: stayed in Q3, never promoted, no FiscalHistory

**Solution:**
- For charges with FiscalHistory → Use stored amounts ✅
- For charges without FiscalHistory but CurrentFiscalPeriodId = viewing quarter:
  - Recalculate quantity for that quarter
  - Use current unit price
  - Calculate Amount = unitPrice × quantity

**Files Modified:**
- `Controllers/BillsController.cs` (QueryForCharges, lines 1019-1057)
- `Models/Repositories/BillRepository.cs` (DetermineStartOfQuarterForPeriod made public)
- `Models/Repositories/IBillRepository.cs` (added method to interface)

---

## 7. Bug Fixes

### 7.1 Timezone Issue with Quarter Boundaries
**Issue:** Selecting January 1 (or any quarter start date) treated it as previous quarter.

**Root Cause:** `DateTimeOffset` comparison with timezone conversion
- Jan 1 at 00:00 PST = Dec 31 at 08:00 UTC
- Comparison: Dec 31 < Jan 1 → Treated as Q3 instead of Q4

**Solution:** Compare `.Date` property only, ignoring time and timezone:
```csharp
bill.StartDate.Value.Date < currentQuarterStart.Date
```

**Affected Comparisons:**
- Create validation (3 places)
- Edit validation (3 places)
- Promotion logic (2 places)

**Files Modified:**
- `Controllers/BillsController.cs` (8 comparisons fixed)

### 7.2 Duplicate Check Bug in PromoteCharge
**Issue:** Duplicate check was checking wrong fiscal period ID.

**Bug:**
```csharp
// Was checking NEW period, but creating history for OLD period
if (_fiscalHistoryRepository.GetFiscalHistoryByIdAndChargeId(newFiscalId, chargeId) == null)
```

**Fix:**
```csharp
// Now checks OLD period (the one we're creating history for)
if (_fiscalHistoryRepository.GetFiscalHistoryByIdAndChargeId(currentFiscalId, chargeId) == null)
```

**Files Modified:**
- `Models/Repositories/BillRepository.cs` (AddBillFiscalHistoryToContext, line 274)

---

## 8. User Experience Improvements

### 8.1 Real-Time Quantity Updates
**Feature:** Quantity field updates automatically when dates or service category change.

**Implementation:**
- AJAX call to `GetBillAmount` endpoint
- Updates Quantity, Amount, and UOM fields
- Shows quantity explanation text

**Files Modified:**
- `Views/Bills/Create.cshtml` (JavaScript)
- `Views/Bills/Edit.cshtml` (JavaScript)

### 8.2 Improved Error Messages
**Before:** Generic validation errors  
**After:** Specific, actionable messages with quarter information

**Examples:**
- ✅ *"Start date cannot be earlier than the previous fiscal period (Fiscal 24/25 Quarter 3). If you need to create a charge with an earlier start date, please contact a Service Billing administrator."*
- ✅ *"This charge's start date has been recognized in historical fiscal periods and cannot be changed."*
- ✅ *"End date cannot be before the start date."*

### 8.3 Quantity Explanation Text
**Feature:** Helpful context about quantity calculations.

**Examples:**
- *"Billing for 2 month(s) in Fiscal 25/26 Quarter 3 (from Nov 1 to Dec 30, 2025)"*
- *"Billing for 3 month(s) in Fiscal 25/26 Quarter 4"*
- *"Unit of measure is Hour, not month-based"*

---

## Technical Implementation Details

### Database Schema Changes
**None** - All changes work with existing schema.

### New Methods Added

**BillRepository.cs:**
- `DetermineStartOfQuarterForPeriod(string)` - Made public for on-the-fly calculations

**BillsController.cs:**
- `GetPreviousQuarterStart()` - Helper for validation
- `GetQuarterAfterNext(DateTimeOffset)` - Helper for validation

### Modified Method Signatures
**None** - All changes maintain backward compatibility.

### Performance Considerations
- Historical quarter views: Additional query to get FiscalPeriod
- On-the-fly calculation: Runs for charges without FiscalHistory (typically 0-2 per quarter)
- Impact: Minimal (microseconds per charge)

---

## Testing Recommendations

### Critical Test Scenarios

1. **Create charge in previous quarter (ended)**
   - StartDate=Nov 1, EndDate=Dec 30
   - Verify: Stays in Q3, FiscalHistory created, appears in Q3 view

2. **Create charge in previous quarter (ongoing)**
   - StartDate=Nov 1, no EndDate
   - Verify: Promoted to Q4, FiscalHistory for Q3, appears in both views

3. **Edit historical charge dates**
   - Old charge from Q2 2024
   - Try changing StartDate
   - Verify: Error message about historical data

4. **Edit recent charge to extend**
   - Q3 charge ending Dec 30
   - Change EndDate to Feb 15
   - Verify: Promoted to Q4, quantities correct in both quarters

5. **Non-month services**
   - Create 8 hours, StartDate=Jan 1, EndDate=Mar 31
   - Verify: Quantity stays 8, not changed to 3

6. **Quarter boundary dates**
   - Create charge with StartDate=Jan 1 (Q4 start)
   - Verify: Assigned to Q4, not treated as Q3

7. **Historical quarter reports**
   - Select "Fiscal 24/25 Quarter 3"
   - Click "Show Report"
   - Verify: Amounts match Q3, not current quarter

8. **Next quarter view**
   - Create charge: StartDate=Jan 1, EndDate=April 1
   - Verify: Appears in Next Quarter (Q1) with quantity=1

---

## Known Limitations

1. **Historical Unit Prices:** On-the-fly calculations use current unit prices, not historical prices from the quarter being viewed. This affects charges without FiscalHistory records.

2. **Manual Quantity Override:** If a user manually changes quantity in Edit view, and the service is month-based, changing dates will recalculate quantity (losing the manual override).

3. **Date Range Validation:** Only validates quarter ranges, not specific date validity (e.g., Feb 30 would pass validation but fail at DB level).

---

## Migration Notes

### Deploying These Changes

1. **No database migrations required** ✅
2. **Application restart required** (hot reload won't work due to interface changes)
3. **Backward compatible** with existing data
4. **Existing FiscalHistory records** work as-is
5. **No breaking changes** to API or external integrations

### Post-Deployment Verification

```sql
-- Verify FiscalHistory counts by quarter
SELECT 
    fp.Period,
    COUNT(fh.Id) AS FiscalHistoryCount,
    COUNT(DISTINCT fh.BillId) AS UniqueBillCount
FROM FiscalHistory fh
JOIN FiscalPeriod fp ON fh.PeriodId = fp.Id
GROUP BY fp.Period
ORDER BY fp.Period DESC;

-- Verify charges without FiscalHistory in historical quarters
SELECT 
    fp.Period,
    COUNT(b.Id) AS BillsWithoutHistory
FROM Bills b
JOIN FiscalPeriod fp ON b.CurrentFiscalPeriodId = fp.Id
LEFT JOIN FiscalHistory fh ON fh.BillId = b.Id AND fh.PeriodId = fp.Id
WHERE fh.Id IS NULL
  AND fp.Period != 'Fiscal 25/26 Quarter 4' -- Exclude current quarter
GROUP BY fp.Period;
```

---

## Future Enhancements (Out of Scope)

1. **Historical Unit Price Storage:** Store unit prices in FiscalHistory for all charges, not just promoted ones
2. **Bulk Date Edit:** Allow administrators to edit dates for multiple charges at once
3. **Audit Trail:** Track all quantity recalculations with before/after values
4. **Preview Mode:** Show impact of date changes before saving
5. **Automatic FiscalHistory Creation:** Create FiscalHistory for ALL charges when quarter ends, not just during promotion

---

## Support Documentation

### SQL Queries for Analysis

**Sum charge amounts by fiscal period:**
```sql
SELECT 
    fp.Period,
    COUNT(fh.Id) AS ChargeCount,
    SUM(fh.UnitPriceAtFiscal * fh.QuantityAtFiscal) AS TotalAmount
FROM FiscalHistory fh
JOIN FiscalPeriod fp ON fh.PeriodId = fp.Id
GROUP BY fp.Period
ORDER BY fp.Period DESC;
```

**Complete quarter view (FiscalHistory + CurrentBills):**
```sql
SELECT 
    fp.Period,
    'FiscalHistory' AS Source,
    COUNT(fh.Id) AS ChargeCount,
    SUM(fh.UnitPriceAtFiscal * fh.QuantityAtFiscal) AS TotalAmount
FROM FiscalHistory fh
JOIN FiscalPeriod fp ON fh.PeriodId = fp.Id
GROUP BY fp.Period

UNION ALL

SELECT 
    fp.Period,
    'CurrentBills' AS Source,
    COUNT(b.Id) AS ChargeCount,
    SUM(b.Amount) AS TotalAmount
FROM Bills b
JOIN FiscalPeriod fp ON b.CurrentFiscalPeriodId = fp.Id
GROUP BY fp.Period
ORDER BY Period DESC, Source;
```

---

**Document Version:** 1.0  
**Last Updated:** March 2026  
**Authors:** Development Team  
**Review Status:** Ready for QA
