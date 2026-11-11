## Getting started
This part tells you where to start according to your role. The menu items mentioned are those of the **Attendance** section and of the related sections.

### If you are an employee {User}
1. At the start and end of work open **Clock in/out** and press **Confirm**.
2. For holidays, leave or extra hours to be authorised use **Absence justification request**.
3. If you forgot a clocking use **Clocking request**.
4. Follow the status of your requests in **Requests** and check the month in **Calendar**.
5. For reimbursements use **Reimbursement request** and follow your reports in **My expense reports**.

> Until a request is approved the calendar shows it as pending and the hours of that day may look incomplete: this is normal.

### If you are a department manager {CompanyAdmin}
- In **Requests** you find the employees' requests to review; the number next to the item shows how many are waiting for your decision.
- In **Calendar** you see the month of an employee of the departments you manage and approve requests directly from the day.
- **Staff present** and **Holiday plan** give you the overview of presences and absences.
- In **Expense review** you approve or reject expense reports.
- In **Users** you open the employees' records.

You receive requests only if you are set as an administrator of the department with **Approves requests** ticked: this is done in **Departments**, **Administrators** tab.

### If you manage HR {CompanyPowerAdmin}
Recommended order for going live:
1. Check the pay codes and absence justifications proposed by the wizard (**Pay codes**, **Absence justifications**).
2. Define **Schedules**, **Work schedule profiles** and the **Public holidays** calendar.
3. Prepare **Skills**, **Activities**, **Sites**, **Departments** (with administrators and approvers) and **Job orders**.
4. Enter the employees with employment, work schedule profile and skills.
5. Choose the request approval mode in **Configuration**.
6. Every month: corrections in the **HR calendar**, approval in **Monthly reconciliation**, payroll file with **Pay code export**.

The **New employee** wizard guides the entry in five steps: User, Employment relationship, Work schedule profile, Skills and Counter accrual. With **Import employees from file** you can upload a spreadsheet, a document, a text or an image: the assistant reads surname, first name, email, username and hire date and lets you check the data before they are inserted (up to 500 employees per import, files up to 5 MB).

## Key concepts
This chapter explains how the program builds the time sheet. Knowing these mechanisms helps you understand why a day is in error or why extra hours are not yet classified.

### Scheduled, worked and workable hours
- **Scheduled hours**: the hours planned by the schedule of the day, i.e. the sum of the FROM-TO pairs of the schedule that the employee's work schedule profile assigns to that day. A schedule without pairs is a rest day (0 hours). On public holidays the **Holiday schedule** of the profile applies.
- **Worked hours**: the sum of the entry-exit pairs, computed on the rounded clockings.
- **Workable hours**: the scheduled hours adjusted by the absence justifications of the day. A justification with sign "-" (holidays, leave) removes hours from what is due and counts as absence; with sign "+" it adds them; with "No effect" it does not change what is due.
- **Extra hours** or **missing hours**: the difference between worked and workable hours.

### Clockings, direction and rounding
A clocking is recorded without a direction. The calculation sorts the clockings of the day and matches them with the pairs of the schedule: the first is an entry, the second an exit and so on. Every start and end time of a pair has a tolerance window (**Lower limit** and **Upper limit**): a clocking inside the window is matched to that time and rounded according to the **Rounding type** (1, 5, 15, 30 or 60 minutes) and the **Rounding direction** ("+" forward, "-" backward).

Every clocking keeps the originally recorded time (**Original clocking**), the time possibly corrected by HR (**Clocking**) and the time used in the calculation (**Rounded clocking**). The **Reference day** is the day the clocking belongs to: it is normally its date, but for a shift ending after midnight HR can assign the exit to the previous day.

Clockings that fall **inside** a pair, between its entry and exit, are an exit and a return (for example a coffee break): the time between them is not counted as worked. Clockings before or after the schedule are work outside the schedule. A clocking repeated within 2 minutes is ignored; an early exit or a late return beyond the tolerance is still matched to the schedule, with a warning.

### Breaks
The lunch break is set in the schedule, **Breaks** tab:
- **Clocked**: the employee clocks out and back in; the calculation flags a break shorter than the minimum or longer than the maximum, but does not correct the hours.
- **Automatic**: clocking the break is optional. If it is missing, or the break is shorter than the minimum, the calculation deducts the missing minutes from the hours worked. If the employee worked through the break, HR enters the **Break not taken** justification and nothing is deducted that day.

The break is due only beyond the threshold of hours worked (usually 6 hours). The **window** says when the break falls; if left empty, the gap between the pairs applies, from the exit's left limit to the return's right limit. The **paid minutes** stay paid even though the employee is on a break.

**Short breaks** (coffee, errands) must always be clocked and are deducted from the hours worked, except for the paid minutes per day. The day detail in the timesheet shows the break clocked, deducted and paid; the monthly reconciliation shows the total break deducted.

### Day status
| Status | Meaning |
|---|---|
| Not yet processed | The day has not been calculated yet. |
| Day correct | Clockings and hours match the schedule. |
| Warnings present | The day is calculated but something should be checked, for example more hours worked than expected. |
| Errors present | Clockings are missing compared to the expected schedule or fewer hours were worked than expected. |
| Day locked | The day belongs to an approved time sheet and is not recalculated. |

### When the calculation runs
A day is recalculated automatically when a clocking is recorded, when a request is sent, approved or deleted and when HR changes an entry. The **HR calendar** also allows manual recalculation of one day, part of the month or the whole month; after changing schedules or work schedule profiles the months concerned must be recalculated manually.

### Absence justifications and pay codes
- An **absence justification** is what the employee requests or HR enters: holidays, leave, sickness, authorisation of extra hours. It changes the hours due and can draw on a counter.
- A **pay code** is the code that the calculation writes on the day and that ends up in the payroll export: worked hours (through the pay code of the time interval), justification hours (through the linked pay code), overtime, additional hours, hour bank.
- Both can be **hour-based** or **amount-based**. Amount-based ones record euro only and do not affect hours.

### Extra hours: additional hours, overtime, hour bank
If **Calculate overtime and additional hours** is active in the work schedule profile, extra hours are classified over the profile's **Counting period** (daily, weekly Monday-Sunday or monthly):
- missing hours of one day offset extra hours of another day in the same period (**offset**);
- for a full-time employee, hours beyond the scheduled hours of the period become **overtime** if they exceed the threshold, otherwise **excess non-overtime hours**;
- for a part-time employee, extra hours are **additional hours** up to the full-time threshold and overtime beyond it; additional hours exceeding the percentage of the contractual weekly hours are **additional hours beyond the limit**;
- overtime is **night overtime** for the part worked in the profile's night band and **holiday overtime** on public holidays;
- the **daily allowance** excludes the first extra minutes and hours are recognised in blocks (for example of 15 minutes);
- with **Extra hours only if authorised**, at most the hours of an approved authorisation justification for that day are recognised; the rest is shown as "unauthorised".

Until the counting period is over, extra hours remain **provisional**. If the employee's employment has **Credited to the hours bank**, the classified hours of approved time sheets go to the hour bank instead of being paid.

### The monthly time sheet
| Status | Meaning |
|---|---|
| Not calculated | No calculation exists yet for the month. |
| Provisional | The period is in progress, some extra hours are still provisional or authorisations are pending. |
| To be approved | The month is over and calculated: closed, awaiting approval. |
| Approved | The days are locked: no new clockings, requests or corrections until HR reopens it. |

### Requests and approvals
Every request (absence justification, clocking, expense report) goes through the same statuses: **Submitted** right after sending, **Approval in progress** or **Partially approved** when more approvers are needed, then **Approved**, **Rejected** or **Cancelled**. Entries recorded directly, such as a clocking from the terminal, are **Direct**. An approved request cannot be deleted: you ask for its **revocation**, which also needs approval.

### Counters
Absence justifications with a counter (holidays, leave) have a monthly **accrued** amount, a **used** amount and a **balance**. The balance includes the carry-over from the previous year and is shown for the previous month, the current month and the following months. The counter type decides what happens to a request exceeding the balance (see **Absence justifications**).

@@ /timeclockuser
### Fields
| Field | Description |
|---|---|
| Time | Shows the current date and time, updated every minute. |
| Last clocking | Time and date of the last clocking recorded from this page. |
| Job order activity | Required. Lists customer, job order, sub job order and activity you can clock on. The activity of your last clocking is proposed. |

### How to
1. Check the activity proposed in **Job order activity** and change it if today you work on another one.
2. Press **Confirm**.
3. Check the time in **Last clocking**.

You do not have to say whether it is an entry or an exit: the calculation assigns the direction from the order of the day's clockings. That is why you must clock every pair planned by your schedule: one missing clocking leaves the day in error.

### Example
Your schedule is 08:30-12:30 and 13:30-17:30. You clock at 08:27, 12:31, 13:29 and 17:34: the calculation recognises entry, exit, entry, exit, applies the schedule's rounding and the day is correct. If you forget the 17:34 clocking, the day has three clockings and stays in error until you send a **Clocking request**.

### Watch out
- You cannot clock on a day outside your employment (before hiring or after termination).
- You cannot clock in a month whose time sheet is already approved: HR must reopen it first.
- Only activities of the sub job orders you are linked to can be used; if you are not linked to any, the activity proposed by the company applies.

=> /requestclockinguser /usertimesheet

@@ /requestlistuser
### Request statuses
| Status | Meaning |
|---|---|
| Submitted | Sent, no approver has decided yet. |
| Approval in progress | Some approvers have approved, others still have to. |
| Partially approved | Approval started but not completed. |
| Approved | The request is valid and enters the time sheet. |
| Rejected | At least one approver rejected it: it has no effect. |
| Cancelled | You withdrew it before approval. |
| Direct | Entry recorded without approval, for example from the terminal or by HR. |

The columns are **From**, **To**, **Type** (clocking, absence justification, expense report), **Details** and **Status**.

### How to
1. Find the request in the list.
2. Use the delete command of the row.
3. Confirm the question "Do you confirm deleting the request?".

If the request is still pending it is cancelled at once. If it is already approved a **revocation request** starts: the approvers review it and, if they approve it, the entry disappears from the time sheet. While the revocation is pending you can withdraw it from the **Calendar** with **Delete revocation**.

=> /usertimesheet /requestjustificationuser /requestclockinguser

@@ /usertimesheet
### What you see
| Item | Meaning |
|---|---|
| Clockings | Each clocking with its direction (E entry, U exit) and time; if linked to a request, its status. |
| Absence justifications | Holidays, leave and other justifications of the day, with hours or amount. |
| Scheduled and worked hours | The hours planned by the schedule and those resulting from the clockings. |
| Extra | Hours beyond what is due (or missing), with their classification: additional hours, overtime, hour bank. |
| Expense report | The day's expense reports with the total; the details show expenses and attachments. |
| Day status | The icon of the day: correct, warnings, errors, locked or not processed. |
| Monthly summary | The summary of the time sheet: scheduled hours, actual hours worked, leave and absences, extra hours and where they go, period balance. |

### How to
1. Click an item of the day (clocking, absence justification or expense report).
2. Choose **Details** to see its data, or **Delete request** if it is one of your requests.
3. To withdraw a revocation that is still pending choose **Delete revocation**.

The **+** button at the bottom quickly opens the absence justification or clocking request and, with **PDF**, prepares the time sheet of the month shown: the file is downloaded from the jobs bar at the top when ready.

### Example
In the middle of the month the summary shows "awaiting reconciliation" next to the extra hours: your profile counts overtime weekly or monthly and hours are classified only when the period is over. It is not an error: at the end of the period they become additional hours, overtime or offset hours.

### Watch out
- Extra hours stay provisional until the counting period of your profile is over.
- Extra hours beyond an approved authorisation are shown as "unauthorised" and are not recognised, if your profile requires authorisation.
- The days of an approved month are locked: ask HR for corrections.

=> /requestjustificationuser /requestclockinguser /requestlistuser

@@ /requestjustificationuser
### Fields
| Field | Description |
|---|---|
| Absence justification | Required. The justifications available in the company (holidays, leave, authorisation of extra hours...). |
| Start date | Required. First day of the period. |
| End date | Required. Last day of the period (same as the start date for a single day). |
| Full day | If on, each day requests the scheduled hours of your schedule. Not available for authorisation of extra hours. |
| Hours and Min | The hours requested for each day when you do not request the full day: at most 8 hours, 16 for an authorisation of extra hours. |
| Amount per day (€) | Only for amount-based justifications, instead of hours: greater than zero. |
| Notes | Optional, visible to the approvers. |

### How to
1. Choose **Leave** (or the justification used in your company) in **Absence justification**.
2. Set the same day in **Start date** and **End date**.
3. Leave **Full day** off and enter 2 in **Hours** and 0 in **Min**.
4. Press **Send**.

### Example
You request holidays from Monday 4 to Sunday 10 with **Full day**. The program counts only the days with hours planned by your schedule: Saturday and Sunday off are skipped and the request is worth 5 days of holidays for the scheduled hours of each day. If your current holiday balance is not enough but the balance including the following months is, you get a warning and the request is sent anyway.

### Watch out
- A period with no working days is refused.
- If the justification has a **Counter with block** and the balance, even counting future accrual, does not cover the requested hours, the request is refused showing the available balance.
- If your department has no enabled approver the request cannot be sent: tell the administration.
- Days outside your employment and days of an already approved month are not allowed.
- An authorisation of extra hours applies to every day of the period, including Saturdays, Sundays and holidays, and is always decided by an approver: the calculation never approves it automatically.

=> /requestlistuser /usertimesheet

@@ /requestclockinguser
### Fields
| Field | Description |
|---|---|
| Date and time | Required. The exact day and time of the forgotten or wrong clocking. |
| Job order activity | Required. The activity you worked on, chosen among those allowed. |

### How to
1. Set the date and time of the missing clocking.
2. Choose the activity in **Job order activity**.
3. Press **Confirm**.

Each request adds a single clocking: if you forgot two, send two requests. As with the terminal, no direction is given: the calculation assigns it from the order of the day's clockings.

### Example
On Tuesday you left at 17:30 without clocking. The day is in error because of missing clockings. You send a request for Tuesday at 17:30: while pending, the calendar shows it as a request; after approval the clocking enters the time sheet and the day is recalculated.

### Watch out
- The activity must be among those of the sub job orders you are linked to.
- Clockings cannot be requested on days outside your employment or in an already approved month.

=> /timeclockuser /requestlistuser /usertimesheet

@@ /notaspesarequestuser
### Fields
| Field | Description |
|---|---|
| Expense report description | Optional, up to 200 characters. E.g. "Customer trip to Milan". |
| Date | Required, for each expense. |
| Expense type | Required, for each expense. The types are defined by the administration. |
| Amount (€) | Required, greater than zero. Calculated for the mileage allowance. |
| Description | Optional, up to 200 characters. E.g. "lunch with the customer". |
| Attach receipt / invoice | PDF or images (JPEG, PNG, WEBP, HEIC): up to 5 MB each, at most 10 per expense and 15 MB for the whole report. |
| Car and Km driven | Only for the mileage allowance: the car of your employment and the km (greater than zero). |

### How to
1. Press **Add expense** and choose a mileage allowance expense type.
2. Press **Enter km and car**.
3. Choose the **Car** and enter the **Km driven**: the amount is km times the car's mileage rate.
4. Confirm the window and complete the other expenses.
5. Press **Send**.

### Example
Trip to Milan: train 45.00 euro with the PDF ticket, lunch 18.50 euro with a photo of the receipt, 120 km with the company car at 0.30 euro per km (36.00 euro calculated). The report has three expenses for 99.50 euro and goes to approval as a single request.

### Watch out
- At least one expense is needed.
- If there are no cars in your employment you cannot enter mileage allowances: ask your administrator to add them.
- The expense report does not change the attendance time sheet.

=> /notaspeseuser

@@ /notaspeseuser
### Statuses and actions
| Situation | What you can do |
|---|---|
| Pending approval | Edit it (saving puts it back to pending) or **Cancel report**. |
| Approved | **Request revocation**: the revocation goes to the approvers. |
| Revocation pending | Wait for the decision on the revocation. |
| Revoked | Nothing: the report is no longer valid. |
| Rejected or cancelled | Nothing: if needed, create a new report. |

The **New expense report** button opens a new report directly.

### How to
1. Open the pending report.
2. Correct expenses or attachments.
3. Press **Save**: the report goes back to pending approval.

### Watch out
- A report already approved, rejected or cancelled can no longer be edited.
- You can only edit your own expense reports.

=> /notaspesarequestuser

@@ /requestlistadmin
### Filters
| Filter | What it shows |
|---|---|
| To approve | Requests waiting for your decision. |
| All | All requests of the employees you manage. |
| Approved | Only the requests already approved. |

The columns are **Surname**, **Name**, **From**, **To**, **Type**, **Details** and **Status**.

### How approval works
The approvers of a request are the administrators of the employee's department with **Approves requests** ticked, in the sequence given in **Order**. What happens depends on the mode chosen in **Configuration**:
| Mode | Effect |
|---|---|
| Single administrator | One approver's decision is enough. |
| All administrators | Everybody must approve; meanwhile the request is "Approval in progress". One rejection rejects it. |
| All administrators, hierarchically | As above, but in order: you see the request to approve only after the previous approver has approved it. |

### Example
With hierarchical mode the Production department has the department head (order 1) and the director (order 2) as approvers. The director sees a worker's holidays among the requests to approve only after the department head has approved them; if the department head rejects them, the request is rejected and never reaches the director.

### Watch out
- A request with days outside the employee's employment can only be rejected.
- The list also contains **revocations** of requests already approved: approving the revocation removes the entry from the time sheet.
- Requests for authorisation of extra hours must always be decided here or from the calendar: the calculation never approves them automatically.

=> /admintimesheet /vacationplan /companycfgedit /departmentlist

@@ /poweradmintimesheet
### Day commands
Click the day number for the calculation and insert commands, click an entry to edit or delete it.
| Command | Effect |
|---|---|
| Calculate (day) | Recalculates that day only. |
| Calculate from 01 to (day) | Recalculates from the start of the month up to the day. |
| Calculate from (day) to (end of month) | Recalculates from the day to the end of the month. |
| Calculate the whole month | Recalculates the whole month. |
| Add clocking to day | Inserts a clocking. |
| Add absence justification to day | Inserts an absence justification, hour-based or amount-based. |
| Add pay code to day | Inserts a pay code directly, with hours or amount. |
| Approve request / Reject request | Decides a pending request of the employee. |

### Calculate window
| Field | Description |
|---|---|
| From, To | The period to recalculate, proposed by the chosen command. |
| Selection | The employees to recalculate (the one shown is proposed). |
| Approve clocking requests | Approves the pending clocking requests of the period. On by default. |
| Approve absence justification requests | Approves the pending absence justification requests, except authorisations of extra hours. On by default. |
| Generate missing clockings | Completes the missing pairs according to the schedule. On by default. |
| Generate absence justifications | Covers missing hours with the profile's unjustified absence justification. On by default. |

The last two options act only on past days, never on today or the future.

### Clocking fields
| Field | Description |
|---|---|
| Reference day | Required. The day the clocking belongs to (e.g. an exit after midnight assigned to the day before). |
| Clocking | Required. The valid time. |
| Original clocking | Required. The originally recorded time, kept for reference. |
| Rounded clocking | Required. The time used in the calculation; the calculation rewrites it applying the rounding. |
| Job order activity | Required. HR can use any activity. |

### Monthly summary
Below the calendar the **Monthly summary** shows the time sheet and the HR actions:
- **Recognised extra hours (h:mm, empty = all)** with **Apply and recalculate**: caps the classified extra hours of the month; the hours removed are shown as "not recognised" (they are removed starting from the last days).
- **Recognise all** and **Reject all**: recognise all extra hours or none.
- **Approve**: approves the time sheet when it is closed, awaiting approval.
- **Reopen**: brings an approved time sheet back to "to be approved" and makes the days editable again.

The **+** button opens the absence justification or clocking request for the employee shown, entered by HR, and **PDF** prepares the monthly time sheets of the employees you choose.

### How to
1. Click the wrong clocking and choose **Edit clocking**.
2. Correct **Clocking** (the original is kept) and, for a night shift, the **Reference day**.
3. Confirm and choose **Calculate** on the day.
4. Check that the day status is correct.

### Example
An employee forgot yesterday's exit and made no request. From the day choose **Calculate** leaving **Generate missing clockings** on: the program completes the pair according to the schedule. If you prefer a precise time, use **Add clocking to day** instead.

### Watch out
- With the default options the calculation approves the pending requests of the period: if you want to leave them to the managers, switch the approval options off before confirming.
- On days outside the employment no entries can be added, only deleted.
- A day of an approved month is not edited or recalculated: use **Reopen** in the summary.
- Recognised hours cannot be changed on an approved time sheet: reopen it first.

=> /quadraturamensile /exportcausali /requestlistadmin

@@ /quadraturamensile
### Columns
| Column | Meaning |
|---|---|
| Employee | Surname and first name, with contract (full-time or part-time) and hour bank, if any. |
| Status | Not calculated, Provisional, To be approved or Approved. |
| Worked | Hours worked compared to scheduled hours. |
| Extra | Hours beyond what is due; "prov." marks the part that is still provisional. |
| Add. | Additional hours (part-time). |
| Overtime | Overtime. |
| Excess | Non-overtime extra hours of full-time employees. |
| Not recog. | Hours not recognised by HR or "not auth."; "to authorise" marks authorisations still pending. |

The button next to each employee opens their month in the **HR calendar**.

### How to
1. Choose year and month (and the department, if needed).
2. Press **Select to approve**: the closed time sheets are selected.
3. Press **Approve (n)** and confirm.

### Example
At the beginning of November you approve October. One employee is still **Provisional**: they have an overtime authorisation request not yet decided. After the request is approved and the month recalculated, the time sheet becomes **To be approved** and you can approve it.

### Watch out
- A time sheet stays provisional until the month is over; with weekly counting the Sunday of the month's last week must also be over.
- It also stays provisional with extra hours not yet classified or pending authorisations.
- If "Recalculate the month from the HR Calendar." appears the data must be updated: recalculate the month before approving.
- From the **+** button choose **Report** to print the monthly reconciliation as a PDF.

=> /poweradmintimesheet /exportcausali

@@ /exportcausali
### Fields
| Field | Description |
|---|---|
| Year and month | The month to export: automatically sets From and To to the first and last day. |
| Department and Employees | Optional, to limit the export to some employees. |
| From, To | Required. You can narrow the period within the month. |
| Export type | Required. The export template to apply. |

### How to
1. Choose year and month.
2. If needed, limit departments and employees.
3. Choose the template in **Export type**.
4. Press **Run** and confirm.
5. Download the file from the jobs bar when the completion message appears.

### Example
The payroll consultant wants the October file by the 5th. You approve the time sheets in **Monthly reconciliation**, then export October with the template of their payroll program. The final message reports two employees without an employee number: you complete the number in their records and repeat the export.

### Watch out
- The export reads the calculated pay codes: if you corrected something, recalculate the month before exporting.
- A template without linked pay codes produces nothing: add them to the template.
- If no employee has pay codes of the template in the period, no file is generated.
- If the payroll program format is not available, the JSON data file is produced.
- At the end warnings may appear: employees without an employee number, or negative values due to rows with "Subtract".

=> /exportcaulist /quadraturamensile /causalilist

@@ /activitystatistics
### Views
| View | What it shows |
|---|---|
| Activity totals | Hours per activity, with job order, sub job order and customer, and the overall total. |
| By employee | The same hours split by employee. |
| Excluded days | The days not counted, with employee, date and status (not processed, error, warning). |

### How to
1. Choose the period, departments and employees.
2. Check the **Excluded days** view and, if there are days listed, correct and recalculate them from the **HR calendar**.
3. From the **+** button choose **Report** and then **Activity totals** or **By employee** for the PDF.

### Watch out
- Hours per activity come from the clockings: days not yet processed or with errors are not counted.

=> /commessalist /activitylist /poweradmintimesheet

@@ /presentstaff
### How to read the page
Filter by **Department** and **Employees**. For each employee you see a status icon, surname, first name and the last clocking of the day with direction (E or U) and time; a dash means there are no clockings.

### Watch out
- The page is based on recorded clockings: someone who forgot to clock in shows no clockings even if they are at work.

=> /vacationplan /admintimesheet

@@ /vacationplan
### How to
1. Choose year and month, departments and employees.
2. Click a pending absence to approve or reject it with **Approve request** or **Reject request**.
3. To print use the **+** button, **Report** and then **Holiday plan**.

### Watch out
- Only absence justifications with **Show in holiday plan** on appear; amount-based ones never appear.

=> /justificationlist /requestlistadmin

@@ /admintimesheet
### What you can do
- Click a pending request and choose **Approve request** or **Reject request**.
- Open the **Expense report details (approve / reject)** of a report of the day.
- With the **+** button, enter an absence justification or clocking request for the employee and, with **PDF**, prepare the monthly time sheets of the employees you choose.

### Watch out
- You only see the employees of the departments you manage.
- Clockings and absence justifications cannot be edited directly from here: direct corrections are made in the **HR calendar**.

=> /requestlistadmin /notaspeseadmin /vacationplan

@@ /userdepartmentlist
### Employee record
| Section | Fields |
|---|---|
| Personal data | Surname and First name (required, up to 50 characters), Employee number (up to 20, unique in the company), Tax code (checked and unique), Disabled. |
| Skills | The employee's skills: they decide which activities the employee can perform. If none is set, the default skill is assigned. |
| Employment | Hire date, Termination date, Contract type (Full-time or Part-time), Contractual weekly hours (0-168), CCNL (up to 50), Overtime and additional hours (Paid in the payslip or Credited to the hours bank). |
| Work schedule profiles | The work schedule profiles of the employment, each with From, To and Cycle start day no. |
| Counters | Per year: Previous year carry-over (also with Read from), Monthly accrual per justification (format HHH:MM) and monthly detail of accrued, used and balance. |
| Expenses | Cars for mileage allowances: Description (required, up to 50), Licence plate (up to 15), Mileage rate (€ per km). |

The **Contractual weekly hours** are the basis for the additional-hours limit; if left empty the scheduled hours of the week are used. **Show disabled** also lists disabled employees.

### How to
1. Open the employee record and the **Work schedule profiles** section.
2. Close the current profile by setting **To** to the day before the change.
3. Add the new profile with **From** on the date of the change.
4. For a shift profile, set in **Cycle start day no.** the cycle day to start from.
5. Recalculate the months concerned from the **HR calendar**.

### Example
From 1 March an employee goes part-time at 20 hours. In the employment you set **Part-time** and 20 in **Contractual weekly hours**, close the full-time profile on 28 February and add the part-time profile from 1 March: from March extra hours become additional hours up to the full-time threshold.

### Watch out
- The termination date cannot be earlier than the hire date.
- Employee number and tax code cannot already be used by another employee.
- A car used in expense reports cannot be deleted.

=> /departmentlist /profiliorarilist /competencelist

@@ /notaspeseadmin
### Filters and views
| Item | Description |
|---|---|
| Period, Department, Employees | Select the reports to review. |
| Expense type | All or a single type. |
| Status | Pending and approved, Pending approval, Approved, Rejected or cancelled, All. |
| By employee / By expense type | Group the expenses. |
| Data / Chart | Detailed list or chart of the totals. |
| Expand all / Collapse all | Open or close all groups. |

### How to
1. Filter on the **Pending approval** status.
2. Open the details of the expense report: you see expenses, attachments and total.
3. Press **Approve** or **Reject** and confirm.
4. For a report with **Revocation requested**, decide the revocation the same way.

From the **+** button choose **Report** and then **By expense type** or **By employee** for the PDF.

### Watch out
- Expense report review is reserved to the company administrators.
- The decision applies to the whole report: a single expense cannot be approved on its own.

=> /tipospesalist /admintimesheet

@@ /justificationlist
### Fields
| Field | Description |
|---|---|
| Code | Required, up to 5 characters (e.g. FER, ROL, MAL). |
| Description | Required, up to 50 characters. |
| Background colour, Text colour | The colours of the justification in the calendar and holiday plan. |
| Type | Hours or Amount. It cannot be changed after the first use. |
| Sign | "-" removes hours from what is due (holidays, leave), "+" adds them, "No effect" does not change what is due. Required. |
| Reason code | The pay code written in the time sheet and used by the export. It must be of the same type as the justification. |
| Show in holiday plan | Shows the justification in the holiday plan. |
| Counter type | No counter, Counter, Counter with warning, Counter with block. Required. |
| Hour bank recovery | The counter accrues the extra hours credited to the hour bank in approved time sheets. |
| Authorise extra hours | Makes the justification an overtime authorisation. |

### Counter types
| Type | Behaviour on requests |
|---|---|
| No counter | No check. |
| Counter | Keeps accrued, used and balance, without checks. |
| Counter with warning | Warns if the current balance does not cover the request but sends it. |
| Counter with block | Refuses the request if not even the balance including future accrual covers it. |

### Example
For an overtime authorisation you create the AUTS justification with **Authorise extra hours** on, sign "No effect", no pay code and no counter. In work schedule profiles with **Extra hours only if authorised**, an approved AUTS of 2 hours allows at most 2 extra hours to be recognised on that day.

### Watch out
- A justification already in use cannot change type (hours or amount).
- An amount-based justification needs an amount-based pay code and an hour-based one an hour-based pay code.
- Amount-based justifications do not affect hours, counters, hour bank or holiday plan.

=> /causalilist /profiliorarilist /vacationplan

@@ /causalilist
### Fields
| Field | Description |
|---|---|
| Code | Required, up to 5 characters. |
| Description | Required, up to 50 characters. |
| Type | Hours or Amount. |

### Where they are used
- In the time intervals of schedules (**Worked hours pay code**) and in the hours bank.
- In absence justifications, to turn the absence into a payroll code.
- In the **Overtime** tab of work schedule profiles (overtime, additional hours, hour bank).
- In export templates, where each pay code gets the code of the payroll program.

### Watch out
- The type of a pay code already in use cannot be changed.
- Amount-based pay codes cannot be used in profiles, schedules and time slots: hour-based ones are needed there.

=> /justificationlist /exportcaulist /orarilist

@@ /companycfgedit
### Fields
| Field | Description |
|---|---|
| Approval Type | Required. Single administrator, All administrators or All administrators, hierarchically. |

### Example
A small company chooses **Single administrator**: the owner's or manager's yes is enough. A company with several levels chooses hierarchical mode and sets the **Order** of the approvers in each department.

=> /departmentlist /requestlistadmin

@@ /orarilist
### Fields
| Field | Description |
|---|---|
| Code | Required, up to 10 characters. |
| Description | Required, up to 50 characters. |
| Clocking type | Time range (pairs of times) or Hours bank / Hours bank value (only a total of scheduled hours). |
| Number of pairs | The planned entry-exit pairs; 0 = rest (0 scheduled hours). |
| Hours bank pay code, Theoretical hours | Hours bank only: the pay code and the hours planned for the day. |

For each pair, in the **Time ranges** tab:
| Field | Description |
|---|---|
| From, To | The planned start and end times. |
| Lower limit, Upper limit | The window within which a clocking is matched to that time. |
| Rounding type | 1, 5, 15, 30 or 60 minutes. |
| Rounding direction | "+" rounds forward, "-" backward. |
| Use for matching | Uses that time to recognise clockings. |
| Worked hours pay code | Required. The hour-based pay code that receives the worked hours of the pair. |

### Example
Schedule 08:30-12:30 and 13:30-17:30. For the 08:30 entry you set limits 08:00 and 09:00, 15-minute rounding with sign "+": whoever clocks at 08:34 is counted from 08:45. For the 17:30 exit you set 15-minute rounding with sign "-": an exit at 17:41 counts as 17:30.

### Watch out
- Changes to a schedule apply to later calculations: recalculate the months concerned.
- Amount-based pay codes cannot be used in schedules.

=> /profiliorarilist /causalilist

@@ /profiliorarilist
### Fields
| Field | Description |
|---|---|
| Description, Code | Required, up to 50 and 10 characters. |
| Profile Type | Weekly (fixed 7-day cycle) or Shift (cycle of 1 to 15 days). |
| No. of Cycle Days | Shifts only: the length of the cycle. |
| Holiday schedule | Required. The schedule applied on public holidays. |
| Unjustified absence justification | Required. The one the calculation uses to cover missing hours when absence justifications are generated. |
| Days | For each day of the cycle the schedule to apply; without a schedule the day is a rest day. |

In the **Overtime** tab:
| Field | Description |
|---|---|
| Calculate overtime and additional hours | If off, extra hours are only shown in the time sheet. |
| Counting period | Daily, Weekly (Mon-Sun) or Monthly. |
| Full-time daily threshold | Used for part-time employees with daily counting. |
| Full-time weekly threshold (hours) | 0-168, default 40. |
| Additional hours limit (% of contractual weekly hours) | 0-100, default 25. |
| Extra hours recognised in blocks of | 1, 5, 15, 30 or 60 minutes, default 15. |
| Daily allowance (unrecognised minutes) | Below this threshold the day's extra hours are not recognised. |
| Holidays and leave hours count towards the threshold | Absences are added to worked hours to decide overtime. |
| Sunday is a holiday | Sunday is treated as a public holiday. |
| Extra hours only if authorised | At most the hours authorised for the day are recognised. |
| Night shift from, to | The night band, default 22:00-06:00. |
| Pay codes | Base overtime and additional hours pay codes (required) and optional pay codes for night, holiday, beyond the limit, excess and hour bank. |

### How to
1. From the **+** button choose **Duplicate a profile**.
2. Choose the starting profile in **Profile to duplicate**.
3. Change description, code and the days that differ.
4. Save and assign it to the employees from their record.

### Example
Horizontal part-time of 20 hours: **Weekly** profile with schedule 08:00-12:00 Monday to Friday and rest on Saturday and Sunday; weekly counting, additional hours limit 25%. With 20 contractual hours, additional hours within the limit go up to 5 hours a week; hours beyond become additional hours beyond the limit, up to the full-time threshold.

### Watch out
- Profile changes do not recalculate past months by themselves: recalculate from the **HR calendar**.
- The pay codes of profiles must be hour-based.

=> /orarilist /userdepartmentlist /quadraturamensile

@@ /festivitalist
### Fields
| Field | Description |
|---|---|
| Description | Required, up to 50 characters. |
| Type | Every year, same day; Only on the given date; Easter Monday (calculated). |
| Day / Date | Required for the first two types; for recurring holidays the year is ignored. |

### Example
For the patron saint's day on 7 December enter "Saint Ambrose" with type **Every year, same day**. For a one-off company bridge day use **Only on the given date**.

### Watch out
- On public holidays the holiday schedule of the work schedule profile applies, and hours worked beyond the scheduled hours count as holiday overtime.
- Holidays added after the calculation enter the time sheet only after the month is recalculated.

=> /profiliorarilist /poweradmintimesheet

@@ /exportcaulist
### Fields
| Field | Description |
|---|---|
| Code | Required, up to 10 characters. |
| Description | Required, up to 50 characters. |
| Instructions for the AI | Optional, up to 1000 characters. |
| Documentation | Record layout, manual or sample file of the payroll program: PDF, DOCX, ODT, XLSX, TXT, CSV, XML, JSON or MD, up to 5 MB and at most 10 documents. |

In the **Pay codes** tab, for each row:
| Field | Description |
|---|---|
| Code | Required. The item code in the payroll program. |
| Pay code | Required. The attendance pay code to export. |
| Processing type | Daily or Monthly. |
| Unit | Hours, Days or Amount. |
| Sign | Add or Subtract: rows with the same code are added together, with Subtract the value is deducted. |

### How to
1. Save the template first, then press **Upload documents**.
2. In **Format test** choose **Month** and **Employee**.
3. Press **Test the format** and check the first lines of the file.
4. If the result is not right, adjust the instructions and press **Regenerate with AI**.

With **Duplicate an export** you copy an existing template with its documents and format.

### Watch out
- Rows with the same code must have the same processing type and the same unit.
- The Amount unit is only for amount-based pay codes, and an amount-based pay code requires the Amount unit.
- A scanned PDF contains no text and cannot be used by the AI.
- Very long documentation is only partly read.
- Without documentation the export produces the JSON data file.

=> /exportcausali /causalilist

@@ /tipospesalist
### Fields
| Field | Description |
|---|---|
| Code | Required, up to 10 characters. |
| Description | Required, up to 50 characters. |
| Type | Generic or Mileage allowance: with Mileage allowance the report asks for km and car and the amount is calculated. |

### Watch out
- An expense type used in any expense cannot be deleted.
- The mileage allowance works only if the employee record has cars with a mileage rate.

=> /notaspesarequestuser /notaspeseadmin /userdepartmentlist

@@ /commessalist
### Fields
| Field | Description |
|---|---|
| Description | Required, up to 50 characters. |
| Customer | Required, from the shared customer register. |
| Sub job order description | The description of the sub job order. |
| Start date, End date | The period of the sub job order. |
| Activities, Departments, Users | The activities of the sub job order and the linked departments and users. |

### How to
1. Create the job order with **New job order** and choose the customer.
2. Add a sub job order and link the activities.
3. Link the departments where those activities are carried out.
4. Link the users who have the required skills.
5. From now on the linked employees see the activities in **Clock in/out**.

### Watch out
- When an activity is unlinked, the departments and users no longer compatible are unlinked automatically.
- A customer used in a job order cannot be deleted from the customer register.

=> /activitylist /departmentlist /activitystatistics

@@ /departmentlist
### Fields
| Field | Description |
|---|---|
| Site | Chosen in the list: the department belongs to a site. |
| Department description | Required, up to 20 characters. |
| Admin | The user manages the department and sees its employees. |
| Approves requests | The administrator receives the requests of the department's employees. |
| Order | The approver's position in the chain, used by hierarchical approval. |
| Department activities | The activities carried out in the department (Enabled). |
| Users working in the department | The department's employees, chosen among those with the required skills. |

### How to
1. Open the department and the **Administrators** tab.
2. Tick **Admin** and **Approves requests** for the manager.
3. Set the **Order** if you use hierarchical approval.
4. Save.

### Watch out
- If no administrator of the department has **Approves requests**, its employees' requests cannot be sent.
- When an activity is removed, users with no compatible activities left are unlinked from the department.

=> /azsedilist /companycfgedit /requestlistadmin

@@ /azsedilist
### Fields
| Field | Description |
|---|---|
| Description | Required, up to 20 characters. |
| Activities performed at the site | The activities enabled at the site (Enab.). |

=> /departmentlist /activitylist

@@ /activitylist
### Fields
| Field | Description |
|---|---|
| Description | Required, up to 50 characters. |
| Background colour, Text colour | How the activity appears in clockings and statistics. |
| Skills required | The skills needed to perform the activity. |

### Watch out
- The default activity must have at least one skill.
- The activity's skills decide which users can be linked to the departments and sub job orders that use it.

=> /competencelist /commessalist /azsedilist

@@ /competencelist
### Fields
| Field | Description |
|---|---|
| Description | Required, up to 50 characters. |

### Example
You create the skills "Welding" and "Testing". The activity "Frame assembly" requires "Welding": only employees with that skill can be linked to the departments and sub job orders with that activity and clock on it.

=> /activitylist /userdepartmentlist

## Glossary
- **Approver**: department administrator with "Approves requests" ticked, who decides the requests of the department's employees.
- **Hour bank**: where classified extra hours go when the employment provides for crediting them to the hour bank instead of paying them.
- **Time sheet**: the monthly attendance summary of an employee; it can be provisional, to be approved or approved.
- **Pay code**: code written on the day by the calculation (worked hours, absences, overtime) and passed to the payroll program.
- **Cycle**: the sequence of days of a work schedule profile; 7 for weekly profiles, from 1 to 15 for shifts.
- **Skill**: an ability of an employee, required by activities.
- **Counter**: balance of an absence justification (accrued, used, balance), for example holidays or leave.
- **Pair**: one entry and one exit planned by the schedule.
- **Daily allowance**: extra minutes per day that are not recognised.
- **Reference day**: the day a clocking is assigned to.
- **Absence justification**: absence or authorisation requested by the employee or entered by HR, hour-based or amount-based.
- **Excess hours**: extra hours of a full-time employee that do not exceed the overtime threshold.
- **Workable hours**: scheduled hours plus or minus the justifications of the day.
- **Scheduled hours**: hours planned by the day's schedule.
- **Counting period**: day, week or month over which extra hours are classified.
- **Work schedule profile**: assigns a schedule to each day of the cycle and contains the extra-hours rules.
- **Revocation**: request to cancel a request already approved, subject to approval.
- **Sub job order**: part of a job order with linked activities, departments and users.
- **Additional hours**: extra hours of a part-time employee up to the full-time threshold.

## Troubleshooting

### The request is not sent: no administrator enabled for approval
The employee's department has no administrator with **Approves requests** ticked. Whoever manages HR must enable at least one in **Departments**, **Administrators** tab.

### Insufficient balance
The absence justification has a counter with block and the balance, including future accrual, does not cover the requested hours. Reduce the period or the hours, or ask the administration to check accrual and carry-over in the employee record.

### The requested period contains no working days
Your schedule plans no hours in the chosen period (rest days or holidays). Check the dates: absences are requested on working days.

### Entry refused: day before hiring or after termination
Clockings and requests are allowed only within the employment period. If the employment dates are wrong they must be corrected in the employee record.

### The month's time sheet is approved
No entries can be added or changed in an approved month. Whoever manages HR can use **Reopen** in the monthly summary of the **HR calendar**, correct, recalculate and approve again.

### The selected activity is not among those allowed
You can only clock on the activities of the sub job orders you are linked to. Ask the administration to link you to the right sub job order.

### The day is in error
Usually a clocking is missing (odd number) or fewer hours were worked than planned. The employee sends a **Clocking request** or an absence justification; HR can correct from the **HR calendar** and recalculate.

### The time sheet is still provisional {CompanyPowerAdmin}
The month is not over, some extra hours are still to be classified or authorisations are pending. Wait for the end of the period, have the requests decided, recalculate the month and try again.

### The export produces no file {CompanyPowerAdmin}
The template has no linked pay codes, or no employee has pay codes of the template in the period. Check the template in **Export templates** and that the month has been calculated.

### The export produces a JSON file {CompanyPowerAdmin}
The payroll program format was not available (documentation missing or the AI did not produce a valid format). Upload the documentation to the template and use **Test the format** before repeating the export.

### The expense report can no longer be edited
It is already approved, rejected or cancelled. If approved you can only request its revocation; otherwise create a new report.

### I cannot enter a mileage allowance
There are no cars in your employment. The administrator must add them in the **Expenses** section of your record, with the mileage rate.

### I cannot delete an expense type or a car {CompanyPowerAdmin, CompanyAdmin}
They are already used in some expenses and must be kept for the history of the expense reports.

### I cannot change the type of a pay code or an absence justification {CompanyPowerAdmin}
After the first use the type (hours or amount) is fixed. Create a new pay code or absence justification with the desired type.
