# Attendance management manual
Clockings, requests, calendar, expense reports and month closing

## Introduction
Attendance management brings together clockings, holidays and leave, overtime, expense reports and monthly time sheets in a single application. Employees clock in and make requests from the browser of their smartphone or computer, managers approve, the administration closes the month and prepares the export for payroll.

This manual is built on **your menu**: you find a chapter for each item you see in the **Attendance** section, with the screenshot of the page. The items available depend on your role.

Shared functions (access, users, customers, support) are described in the **Basic functions manual**.

### Roles {CompanyPowerAdmin, CompanyAdmin}
- **Employee (User)**: clocks in, sends requests, checks their own calendar and manages expense reports.
- **Manager / CompanyAdmin**: approves requests, checks employees' calendars, sees staff present and the holiday plan, reviews expense reports.
- **CompanyPowerAdmin**: does all of the above, plus month closing, exports, reports and the configuration of the application.

A user can have more than one role in the same company (for example CompanyAdmin and User): in that case they see both the employee items and the administration items.

### The assistant
From the **Ask the assistant** box on the Dashboard you can write or say what you want to do: "clock in", "request holidays from the 10th to the 14th" or "open the calendar". The assistant does it or opens the right page.

### Extra hours and the hour bank
Hours worked beyond the scheduled time become additional work or overtime according to the employee's working-hours profile and the authorisations. Extra hours of approved time sheets are credited to the **hour bank**. The calculation rules (daily allowance, thresholds, hours recognised only if authorised) are set in the working-hours profiles.

@@ /timeclockuser
Here you record entry and exit. At the top you always see the last recorded clocking.
1. Choose the activity, if the company requires it.
2. Press **Entry** or **Exit**.
3. Check the recorded time.

The clocking is recorded with the current date and time. If you forgot a clocking or recorded a wrong one you cannot change it directly: use the **Clocking request**.

@@ /requestlistuser
The list of all your requests (justifications, clockings, corrections) with their status: **pending**, **approved** or **rejected**. Open a request to see its details.

A pending request can be deleted. The counter next to the menu item shows how many requests are pending.

@@ /usertimesheet
Your monthly time sheet. It shows, day by day, clockings, justifications, expense reports and the hours calculated according to your working-hours profile.

Move through the months with the arrows and click a day to open its details. It is the tool with which you check that your attendance and absences are correct before the month is closed.

@@ /requestjustificationuser
Here you request holidays, leave, sickness, overtime and the other reasons provided by the company.
1. Choose the type of justification.
2. Enter the period, in whole days or in hours.
3. Add a note, if needed.
4. Confirm to send.

For an overtime authorisation enter the extra hours you ask to be recognised on each day: hours worked beyond the authorised ones are not recognised. The request goes to the approvers of your department.

@@ /requestclockinguser
Used to ask for a forgotten or wrong clocking to be added or corrected.
1. Enter the day.
2. Enter the time and the type, entry or exit.
3. Choose the activity, if needed.
4. Confirm to send.

The request goes to the manager of your department, who approves or rejects it. Once approved, the clocking enters the time sheet.

@@ /notaspesarequestuser
Here you prepare and send a new expense report for a refund.
1. Write the description of the report, for example "Customer trip to Milan".
2. Add the expenses: date, type, amount and description.
3. Attach receipts and invoices.
4. Confirm to send the report for approval.

For mileage refunds enter the km and the car. The attachments allowed are PDF and images, up to 5 MB each and 15 MB in total.

@@ /notaspeseuser
The list of your expense reports with the status of each one. Open a report to see its expenses and attachments.

Until a report is approved you can change it (it goes back to pending approval) or cancel it; if it is already approved you can ask for it to be revoked. Approving the report approves all its expenses.

@@ /requestlistadmin
The requests of the employees of the departments you approve, waiting for your review.
1. Open a request to see its details.
2. Press **Approve** or **Reject**.
3. Confirm your choice.

Requests reach the approvers of the employee's department; the administrator can approve for everybody. The counter next to the item shows the requests to examine.

@@ /poweradmintimesheet
The HR calendar is the tool to correct the data of any employee.
1. Choose the employee and the month.
2. Click a day to add or change clockings, justifications and reason codes.
3. Press **Calculate** to recalculate the time sheet of the period.

On days outside the employee's employment relationship no new entries are allowed: existing ones can only be deleted.

@@ /quadraturamensile
The month closing: here you check the time sheets and approve them in bulk.
1. Choose the month.
2. Check the time sheets: anomalies are highlighted.
3. Correct from the HR calendar and recalculate, if needed.
4. Select the time sheets and approve them.

Only closed time sheets can be approved: a month in progress stays provisional. After approval the days of the month can no longer be changed and the extra hours are credited to the hour bank.

@@ /exportcausali
Produces the file for the payroll program from the month's time sheets, according to the chosen **export model**.

Choose the month and the model, then start the export. The file is prepared in the background and appears in the jobs bar at the top, with the button to download it.

@@ /activitystatistics
Shows the hours worked per customer, job and activity in the chosen period. It is useful to analyse the effort on jobs and for reporting.

The data can be filtered by period and by employee; the PDF reports are generated in the background and downloaded from the jobs bar.

@@ /presentstaff
Shows in real time who is at work, who is absent and why. It is the immediate picture of the staff situation at the moment you open the page.

@@ /vacationplan
Shows the planned absences of all employees on a single calendar. It is useful to organise shifts, coverage and closing periods while avoiding overlaps.

@@ /admintimesheet
The calendar of the employees of the departments you manage. Choose an employee and see their month with clockings, justifications and expense reports.

From here you can also approve or reject their requests. On days outside the employment relationship a request can only be rejected.

@@ /userdepartmentlist
The list of the employees of the departments you manage. From here you open an employee's card to consult their data.

@@ /notaspeseadmin
The review of employees' expense reports.
1. Choose the period and, if needed, the employees.
2. Open a report to see its expenses and attachments.
3. Approve or reject the report.

Approving a report approves all the expenses it contains.

@@ /justificationlist
Justifications are what the employee requests: holidays, leave, overtime authorisation and the like. Each justification is linked to a reason code and to any counters.

An amount justification records only an amount in euro and does not affect hours, counters or the holiday plan. From here you create, change and disable the justifications available in the company.

@@ /causalilist
Reason codes are the codes that end up in the time sheet and in the export, by hours or by amount. They are the base on which the justifications and the payroll export rely.

The type of a reason code already in use cannot be changed. From here you manage the company's list of reason codes.

@@ /companycfgedit
Here you set the general rules of attendance management: request approval mode, default customer and job and the other parameters of the application.

These are the settings that apply to the whole company and that determine the behaviour of clockings, requests and time sheets.

@@ /orarilist
Working hours are the daily time slots: they define the hours scheduled in a working day. They are the building blocks of the working-hours profiles.

From here you create and change the working hours used in the company.

@@ /profiliorarilist
Working-hours profiles describe an employee's cycle of working hours, for example the 5-day week, and the rules for extra hours: additional work or overtime, daily allowance, hours recognised only if authorised.

The working-hours profile is assigned to the employee in their card and drives the time sheet calculation.

@@ /festivitalist
Holidays are the public holidays, common to the whole company or specific to a site. They enter the time sheet calculation.

From here you manage the calendar of holidays and link it, where needed, to the individual sites.

@@ /exportcaulist
Export models define the file format for the payroll program.

You can attach the record layout, the manual or a sample file of the payroll program and add instructions (for example "semicolon separator"): the AI prepares the format. With the test on one employee's data you check the result before using it in the monthly export.

@@ /tipospesalist
Expense types are the items of the expense reports: meals, accommodation, train, mileage refund and the like, each with its own rules.

From here you define which expense types employees can use when they fill in a report.

@@ /commessalist
Jobs represent the company's work and are linked to a customer from the shared registry (**Administration > Registry > Customers**). They are divided into sub-jobs.

A sub-job can only be linked to departments where at least one of its activities is carried out and to users with at least one of the required skills. From here you manage jobs and sub-jobs that employees clock on.

@@ /departmentlist
Departments organise the company's employees. For each department you set who manages it and who approves its employees' requests.

Departments determine who receives the requests and who can see and correct the employees' time sheets.

@@ /azsedilist
Sites are the company's workplaces, each with its own holidays. From here you create and change the sites and link the holiday calendar to them.

@@ /activitylist
Activities are the work items employees clock on. They are linked to sub-jobs and require certain skills.

From here you manage the list of activities available for clocking.

@@ /competencelist
Skills are the abilities required by the activities. They serve to link the right people to the right work: a sub-job can only be linked to users with at least one of the required skills.

From here you manage the company's list of skills.

## First setup {CompanyPowerAdmin}
On first access the **wizard** prepares the company with ready-made reason codes, justifications, working hours and parameters. After the wizard, sites, departments and employees are completed.

Employees are the company users with the User role. Their card holds the employment relationship (start and end dates, working-hours profile), the department and the skills. An **employee import** from file is also available, with AI-assisted column mapping.

## Frequently asked questions
- **Do employees need to install anything?** No: they use the browser of their smartphone or computer.
- **Who approves requests?** The approvers of the employee's department; the administrator can approve for everybody.
- **Can I use it with my payroll program?** Yes: the reason code export is configured with models suited to your payroll program.
- **A clocking is wrong, how do I correct it?** The employee makes a clocking request; the administrator can correct it directly from the HR calendar.
- **Why can't I approve a month?** Only closed time sheets can be approved: a month still in progress stays provisional.
