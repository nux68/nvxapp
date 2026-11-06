# Attendance management manual
Clockings, requests, calendar, expense reports and month closing

## Introduction
Attendance management brings together clockings, holidays and leave, overtime, expense reports and monthly time sheets in a single application. Employees clock in and make requests from the browser of their smartphone or computer, managers approve, the administration closes the month and prepares the export for payroll.

The functions are in the **Attendance** section of the menu. The items depend on your role:
- **Employee (User)**: clocking, requests, calendar, expense reports;
- **Manager / CompanyAdmin**: approvals, employees' calendar, staff present, holiday plan, expense review;
- **CompanyPowerAdmin**: all of the above, plus month closing, exports and configuration.

> You can also use the Dashboard assistant: write or say "clock in", "request holidays from the 10th to the 14th" or "open the calendar" and the assistant does it or opens the right page.

Shared functions (access, users, customers, support) are described in the **Basic functions manual**.

## Employee {User}
### Clocking
1. Open **Attendance > Clocking**.
2. If the company requires it, choose the **activity** you are working on.
3. Press the entry or exit button: the clocking is recorded with the current date and time.

At the top you see the last recorded clocking. If you forgot a clocking or recorded a wrong one you cannot change it directly: use **Clocking request**.

### Clocking request
Used to ask for a forgotten or wrong clocking to be added or corrected.
1. Open **Attendance > Clocking request**.
2. Enter day, time, type (entry or exit) and the activity if needed.
3. Confirm: the request goes to the manager of your department.

### Justification request
Holidays, leave, sickness, overtime and the other reasons provided by the company are requested with a justification.
1. Open **Attendance > Justification request**.
2. Choose the type of justification and the period (whole days or hours).
3. For an overtime authorisation enter the extra hours you ask to be recognised on each day: hours worked beyond the authorised ones are not recognised.
4. Add a note if needed and confirm.

### My requests
In **Attendance > Requests** you find all your requests with their status (pending, approved, rejected). A pending request can be deleted. The counter next to the menu item shows the pending requests.

### Calendar
**Attendance > Calendar** shows your month: clockings, justifications, expense reports and the hours calculated for each day according to your working-hours profile. Click a day to see the details.

### Expense reports
1. Open **Expense reports > Refund request**.
2. Write the description of the report (e.g. "Customer trip to Milan").
3. Add one or more expenses: date, expense type, amount and description. For mileage refunds enter the km and the car.
4. Attach receipts and invoices: PDF and images are accepted, up to 5 MB each and 15 MB in total.
5. Confirm: the report goes for approval.

In **Expense reports > My expense reports** you follow their status. Until a report is approved you can change it (it goes back to pending approval) or cancel it; if it is already approved you can ask for it to be revoked. Approving the report approves all its expenses.

## Manager and CompanyAdmin {CompanyPowerAdmin, CompanyAdmin}
### Approving requests
1. Open **Attendance > General > Requests**: you find the requests of the employees of the departments you approve.
2. Open a request to see its details.
3. Use **Approve** or **Reject**; you are asked to confirm.

Requests reach the approvers of the employee's department; the administrator can approve for everybody. The counter next to the item shows the requests to examine.

### Employees' calendar
In **Attendance > General > Calendar** choose an employee and see their month with clockings, justifications and expense reports; from here you can also approve or reject requests. On days outside the employment relationship a request can only be rejected.

### Staff present
**Staff present** shows in real time who is at work, who is absent and why.

### Holiday plan
**Holiday plan** shows the planned absences of all employees, useful to organise shifts and closing periods.

### Expense review
1. Open **Expense reports > Expense review**.
2. Choose the period and, if needed, the employees.
3. Open a report to see expenses and attachments, then approve or reject it.

### Department users (CompanyAdmin)
**Users** lists the employees of the departments you manage, and lets you open their card.

## CompanyPowerAdmin: managing the month {CompanyPowerAdmin}
### HR calendar
**HR calendar** is the tool to correct the data of any employee:
1. choose the employee and the month;
2. click a day to add a clocking, a justification or a reason code, or to change or delete them;
3. use **Calculate** to recalculate the time sheet of the period after the corrections.

> On days outside the employee's employment relationship no entries are allowed: existing ones can only be deleted.

### Monthly reconciliation
1. Open **Monthly reconciliation** and choose the month.
2. Check the time sheets: anomalies are highlighted.
3. If corrections are needed, make them from the HR calendar and recalculate the month.
4. Select the time sheets and approve them in bulk.

> Only closed time sheets can be approved: a month in progress stays provisional. After approval the days of the month can no longer be changed. Extra hours of approved time sheets are credited to the hour bank.

### Reason code export
**Reason code export** produces the file for the payroll program from the month's time sheets, according to the chosen **export model**. The file is prepared in the background and appears in the jobs bar at the top, with the button to download it.

### Activity statistics and reports
**Activity statistics** shows the hours worked per customer, job and activity in the period. The PDF reports (activities, holiday plan, reconciliation, expense reports) are also generated in the background and downloaded from the jobs bar.

## CompanyPowerAdmin: configuration {CompanyPowerAdmin}
### First setup
On first access the **wizard** prepares the company with ready-made reason codes, justifications, working hours and parameters. After the wizard, sites, departments and employees are completed.

### Configuration
In **Parameters > Configuration** you set the general rules of the company: request approval mode, default customer and job and the other attendance parameters.

### Sites and departments
- **Sites**: the company's sites, with their holidays.
- **Departments**: for each department you set who manages it and who approves its employees' requests.

### Employees
Employees are the company users with the User role. Their card holds the employment relationship (start and end dates, working-hours profile), the department and the skills. An **employee import** from file is also available, with AI-assisted column mapping.

### Jobs, activities and skills
- **Jobs**: linked to a customer (from the shared registry **Administration > Registry > Customers**) and divided into sub-jobs.
- **Activities**: the work items employees clock on.
- **Skills**: the abilities required by the activities. A sub-job can only be linked to departments where at least one of its activities is carried out and to users with at least one of the required skills.

### Working hours, profiles and holidays
- **Working hours**: the daily time slots.
- **Working-hours profiles**: an employee's cycle of working hours (e.g. 5-day week) and the rules for extra hours: additional work or overtime, daily allowance, hours recognised only if authorised.
- **Holidays**: public holidays, common or per site.

### Justifications and reason codes
- **Reason codes**: the codes that end up in the time sheet and in the export (hours or amounts). The type of a reason code already in use cannot be changed.
- **Justifications**: what the employee requests (holidays, leave, overtime authorisation...), linked to the reason code and to any counters. An amount justification records only an amount in euro and does not affect hours, counters or the holiday plan.

### Export models
In **Export models** you define the file format for the payroll program. You can attach the record layout, the manual or a sample file of the payroll program and add instructions (e.g. "semicolon separator"): the AI prepares the format. With the test on one employee's data you check the result before using it.

### Expense types
**Expense types** lists the items of the expense reports (meals, accommodation, train, mileage refund...) with their rules.

## Frequently asked questions
- **Do employees need to install anything?** No: they use the browser of their smartphone or computer.
- **Who approves requests?** The approvers of the employee's department; the administrator can approve for everybody.
- **Can I use it with my payroll program?** Yes: the reason code export is configured with models suited to your payroll program.
- **A clocking is wrong, how do I correct it?** The employee makes a clocking request; the administrator can correct it directly from the HR calendar.
