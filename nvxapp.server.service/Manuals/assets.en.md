# Company assets manual
Items, fleet and bookings

## Introduction
The Company assets module covers what the company owns and who it is assigned to: fleet, assigned items, bookings of desks, rooms and pool cars. It is available when it is active for your company: the centre (or the firm, with a licence) activates it.

This manual follows **your menu**: there is a chapter for each item of the **Company assets** section you can see. The items you see depend on your role.

### Who does what
- **CompanyPowerAdmin**: configures the module and does everything the CompanyAdmin does.
- **CompanyAdmin** (purchasing, logistics, IT): manages vehicles, assignments, deadlines, costs and fines.
- **Employee**: in the **My resources** page sees the assigned vehicle and the fines concerning them.

@@ /astsettings
In the **Settings** page the CompanyPowerAdmin sets the general parameters of the module:
- **Deadline notice days**: how many days in advance the module reports the deadlines it records in the shared deadline schedule (default 30).
- **Notifications on**: clear it and the module sends no notifications or reminders. Each user still chooses how to receive them (in the app or by email) from the notification preferences.
- **Notes**: internal notes about the configuration.

Press **Confirm** to save.

@@ /astvehicles
The **Vehicles** page lists the company's vehicles with the current assignee and the next deadline. Use the **+** at the bottom to add a vehicle; from the row you edit or delete it (with assignments, deadlines, costs and fines). The **Show disposed vehicles** box in the options also shows the ones no longer in use.

### The vehicle
Licence plate (unique in the company), make, model, fuel, registration date, site and kilometres. **Use**: *Assigned* (to an employee) or *Pool* (booked from the bookings). **Contract**: owned, rental or leasing, with supplier, number, expiry, included km and fee; the contract expiry goes into the **deadline schedule**. **Fringe benefit**: ACI cost per km and CO2 emissions.

Once the vehicle is saved, the **Assignments**, **Deadlines**, **Costs** and **Fines** sections appear below; the **+** opens the menu to choose what to add.

### Assignments and fringe benefit
The assignment sets the employee and the period (a vehicle is assigned to one person at a time). With **mixed use** the annual fringe benefit is 15,000 km times the ACI cost times the percentage (leaving 0 uses the emissions one: up to 60 g/km 25%, up to 160 30%, up to 190 50%, above 60%); the share for the current year is shown too. The value is for information: the payroll export stays in Attendance.

Each assignment creates or updates the **employee's car** (with the reimbursement per km) that Attendance expense reports use for mileage reimbursements.

### Deadlines
Road tax, insurance, roadworthiness test, service (also by kilometres) or other: each deadline goes into the shared **deadline schedule** with the notice of the settings and reminders to the administrators. Entering the date it was done closes the deadline.

### Costs and fines
**Costs** (fuel, maintenance, tolls...) show the total for the year; the kilometres entered update the vehicle's. In a **fine**, if you do not set the driver, it is whoever had the vehicle on the offence date; the **Send** button notifies the fine to the driver (who needs a user account) and sets it to *Notified*.

@@ /astmine
The **My resources** page shows the vehicle assigned to you, with the fringe benefit for the year for information, and the fines notified to you.
