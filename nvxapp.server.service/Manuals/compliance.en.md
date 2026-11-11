# Compliance manual
Regulatory compliance

## Introduction
The Compliance module covers regulatory compliance: workplace safety (Italian Legislative Decree 81/08), whistleblowing (Legislative Decree 24/2023), GDPR records. It is available when it is active for your company: the centre (or the firm, with a licence) activates it.

This manual follows **your menu**: there is a chapter for each item of the **Compliance** section you can see. Health and safety and the GDPR register are available; whistleblowing will follow the legal review.

### Who does what
- **CompanyPowerAdmin**: configures the module, manages health and safety and the GDPR register.
- **CompanyAdmin** (safety manager, HR office, internal DPO): health and safety and GDPR register.
- **Users enabled for health data** (occupational physician or designated safety manager, chosen in the settings): the medical examinations with health data.
- **Employee**: sees their own courses, examinations (outcome and expiry only), PPE received and appointments, and confirms deliveries and appointments in the acknowledgements.
- **Firm**: manages the GDPR register and views the module deadlines of the companies it follows.

@@ /cmpsettings
In the **Settings** page the CompanyPowerAdmin sets the general parameters of the module:
- **Deadline notice days**: how many days in advance the module reports the deadlines it records in the shared deadline schedule (default 30).
- **Notifications on**: clear it and the module sends no notifications or reminders. Each user still chooses how to receive them (in the app or by email) from the notification preferences.
- **Users enabled for health data**: the occupational physician or the designated safety manager; only they see and write the health data of the examinations.
- **Notes**: internal notes about the configuration.

Press **Confirm** to save.

@@ /cmp81
The **Health and safety** page collects the obligations of Italian Legislative Decree 81/08, one section at a time (at the top):
- **Courses attended**: each employee's courses with date, hours, provider and expiry. If you leave the expiry empty it is calculated from the validity required by the job roles. The most recent course of a type renews the previous ones.
- **Training status**: for each employee the courses required by their job roles and the last one attended; missing and expired ones are highlighted and **Add** records the course.
- **Medical examinations**: type, fitness outcome, summary of prescriptions and date of the next examination. Health data can be seen and written only by the users enabled in the settings; they are stored encrypted.
- **PPE deliveries**: at each delivery the employee (if they have a user) receives an acknowledgement request, which is the delivery report.
- **Appointments**: safety manager, officer, workers' representative, supervisors, emergency and first aid officers, occupational physician; internal (with acknowledgement by the appointee) or external.
- **Documents (risk assessment)**: risk assessment, emergency plans and procedures with version, review and expiry.
- **Job roles**, **Courses per job role**, **Employees' job roles** and **PPE catalogue**: the tables used by the sections above.

Use **+** to add an item to the open section; edit or delete it from its row. The deadlines (courses, examinations, appointments, documents) go to the **Deadlines** page and are updated at every save.

@@ /cmpgdpr
The **GDPR register** page contains:
- **Processing activities**: the record of processing activities (art. 30): purposes, legal basis, data subjects, data, recipients, transfers, erasure time limits, security measures.
- **External processors**: processors and appointments (art. 28), with the agreement expiry in the deadline list.
- **Data breaches**: the breach register (art. 33). If the breach entails a risk, it must be notified to the supervisory authority within 72 hours of discovery: the deadline stays open until you record the notification date. When a breach is recorded the administrators receive a notification.

**Excel** prepares the complete register: the file can be downloaded from the jobs bar when it is ready.

@@ /cmpmine
In **My compliance** you find your courses with their expiry, your medical examinations (type, date, outcome and expiry), the PPE you received and your appointments. PPE deliveries and appointments are confirmed in the **Acknowledgements** page (button at the bottom).

@@ /cmpstudio
In **Companies compliance** the firm chooses one of its companies with Compliance active (next to the name, the open and expired deadlines) and manages its GDPR register (processing activities, processors, breaches, Excel export); the **Deadlines** section shows the module deadlines of the company.
