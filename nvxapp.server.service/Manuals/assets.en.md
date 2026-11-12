# Company assets manual
Items, fleet and bookings

## Introduction
The Company assets module covers what the company owns and who it is assigned to: fleet, assigned items, bookings of desks, rooms and pool cars. It is available when it is active for your company: the centre (or the firm, with a licence) activates it.

This manual follows **your menu**: there is a chapter for each item of the **Company assets** section you can see. The items you see depend on your role.

### Who does what
- **CompanyPowerAdmin**: configures the module and the bookable resources and does everything the CompanyAdmin does.
- **CompanyAdmin** (purchasing, logistics, IT): manages vehicles, assignments, deadlines, costs and fines, items with deliveries and returns; sees and cancels all bookings.
- **Employee**: books desks, rooms and pool cars; in the **My resources** page sees the items they hold (with the reports to confirm), the assigned vehicle and the fines concerning them.

@@ /astsettings
In the **Settings** page (*Configuration* menu) the CompanyPowerAdmin sets the general parameters of the module:
- **Deadline notice days**: how many days in advance the module reports the deadlines it records in the shared deadline schedule (default 30).
- **Notifications on**: clear it and the module sends no notifications or reminders. Each user still chooses how to receive them (in the app or by email) from the notification preferences.
- **Bookings**: maximum length of a booking (hours) and maximum advance (days); 0 = no limit. Bookings need no approval.
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
The **My resources** page shows the items you hold (and the ones returned in the last year) with the status of the reports: the ones to confirm open with **Open** in the acknowledgements page. Below you find the vehicle assigned to you, with the fringe benefit for the year for information, and the fines notified to you.

@@ /astgoods
The **Items** page has three sections. Use the **+** at the bottom to add an item or a category.

### Items
The list of items (PCs, phones, badges, equipment...) with their status and the employee holding them. An item has a unique inventory code, the category, description, make and model, serial number, purchase date, value, site and warranty expiry (which goes into the **deadline schedule**). The *Delivered* status is handled by delivery and return; with the item in stock you can set it *Available*, *Under repair* or *Disposed*.

### Delivery and return
In the card of an available item the **+** records the **delivery** to an employee, with date and condition. The employee receives the **delivery report** to confirm in the acknowledgements page; with Document management active the report becomes a PDF to sign. From the row of the open delivery you record the **return** (date, condition and item status on return): the employee receives the return report. The history shows the status of the reports for each delivery. An employee without a user receives no reports: only the delivery is recorded.

### To return
Items and vehicles still held by employees with a **termination date** (recorded by Attendance or Human resources). Every night the administrators receive a notification with the new terminations within the notice days and each return goes into the **deadline schedule** on the termination date; the deadline closes when the item is returned or the vehicle assignment ends. The **Check now** button runs the check immediately.

### Categories
The item categories: on activation you find the default ones. A category used by items cannot be deleted: deactivate it.

@@ /astbookings
The **Bookings** page shows one week at a time (the arrows change week) the bookings of desks, rooms and pool cars. You can filter by resource or see only yours. Of other people's bookings you only see that the resource is *Busy*; administrators also see who booked.

### Booking
With the **+** at the bottom choose the resource, start and end (and optionally a title and notes). If the resource is already booked at that time the booking is rejected, even when two people book at the same moment. The limits of the settings apply (maximum length and maximum advance).

From the row you edit or cancel your future bookings; administrators can cancel anyone's (the person receives a notification).

### Personal calendar
Your bookings appear in your personal calendar (Google, Outlook, Apple): the **Calendar** button opens the page with the address to add.

@@ /astresources
The **Bookable resources** page (*Configuration* menu, CompanyPowerAdmin) lists desks, rooms and pool cars. With the **+** you add a resource: type, description, site, the seats for rooms and, for cars, the fleet vehicle with *Pool* use. A resource with future bookings cannot be deleted: deactivate it.
