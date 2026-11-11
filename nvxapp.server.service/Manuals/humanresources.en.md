# Human Resources manual
The employee file

## Introduction
The Human Resources module covers the employee file beyond attendance: payslips and tax certificates (CU), training and skills, onboarding and offboarding. It is available when it is active for your company: the centre (or the firm, with a licence) activates it. It also works without Attendance and Document Management.

This manual follows **your menu**: there is a chapter for each item of the **Human Resources** section you can see.

### Who does what
- **CompanyPowerAdmin**: configures the module and the checklist templates; everything else.
- **CompanyAdmin** (HR office): employees, payslips and CU, courses, training plans and checklists of all employees.
- **Employee** (User role): sees their own payslips and CU and acknowledges them.
- **Accounting firm**: uploads and checks the payslips of the firm's companies with Human Resources active (it does not see the rest of the file).

### Deadlines, notifications and acknowledgements
The module uses the shared services: course and checklist item deadlines go to the **deadline schedule** (with the notice set in Settings), new documents arrive with a **notification** and reading is confirmed through **acknowledgements** (with Document Management active it becomes a signature of the document).

@@ /hremployeelist
The **Employees** list shows the employee data shared by all applications: tax code, employee number, hire and end dates and account. With the **+** button you create a new employee; **Edit** opens the record.

- **Tax code** and **employee number** are used to split the single payslip PDF: they are unique among the company's employees.
- **Hire and end dates** generate the checklists from the default templates.
- **Account**: from the record you can **create the account** of the employee (User role, counts towards the licence user limit) or **disable it** when they leave. With an account the employee sees their payslips.

@@ /hrpayslips
In the **Payslips and CU** page the HR office uploads the **single PDF** produced by the payroll software.

1. Press **+** and choose **Upload the single PDF**: set the type (payslip or CU), the year and, for payslips, the month (13th month included), then choose the file and press **Upload and split**.
2. The **split by employee** continues in the background (progress is in the jobs bar): each page goes to the employee whose **tax code** appears in the text; otherwise the **employee number** is searched (written as "Matricola 123").
3. Unrecognised pages (no code, several employees on the same page, scans without text) stay **to check**: in the detail you **assign** them to the right employee (the one whose surname and name appear on the page is suggested) or **ignore** them.
4. With **Publish** the documents appear in the employees' area: employees with an account receive a notification and an acknowledgement request.

The detail also shows the **acknowledgement report**: confirmed, waiting and employees without an account. Each employee's PDF is downloaded with **PDF**. An upload can be deleted until it is published; pages assigned after publication are published immediately.

@@ /hrmypayslips
In **My payslips** you find the payslips and CU published for you. **PDF** downloads the document; **I confirm I have read it** confirms the acknowledgement (with Document Management active the confirmation is the signature of the document, from the **Acknowledgements** page).

@@ /hrcourselist
The company's **Courses** catalogue: title, provider, duration, certificate **validity** in months (empty = no expiry), acquired **skill** (the company's shared skills) and mandatory course. With **+** you create a course; in a saved course you find its **editions** and add one with **+**.

In the edition you add the **participants** with **+** and set their outcome: with **Passed** the employee acquires the course skill and the **expiry** (obtained date plus validity) enters the deadline schedule; when the course is renewed, the previous deadline is closed. After saving you upload each participant's **certificate**.

A course with editions cannot be deleted: disable it instead.

@@ /hrplanlist
**Training plans** state, for a year, which courses each employee must attend and by when. Progress counts the rows whose course the employee **passed** in the plan's year. With **+** you create a plan; in the plan you add employees with **+** and choose the course of each row.

@@ /hrchecklistlist
**Onboarding and offboarding checklists** are generated automatically from the **default template** when an employee has a hire (or end) date from 30 days ago onwards; if the date changes and the checklist has not started yet, it moves to the new date. With **+** you create one manually choosing the employee and the template.

In the checklist each item has a due date (with a deadline in the deadline schedule, notifying the owner or the administrators):
- tick the item when done;
- for items with a **document**: upload the file and press **Request acknowledgement**; when the employee confirms, the item is completed automatically;
- for items with an **account action**: **Create account** or **Disable account** perform the action and complete the item.

When all items are completed the checklist is completed; **Cancelled** closes it with no deadlines or pending acknowledgements.

@@ /hrchecklisttemplatelist
**Checklist templates** (CompanyPowerAdmin only) define the items of onboarding and offboarding checklists: description, days from the date (negative = before), owner, document to hand over or sign, account action. The **default** template of each type generates the checklists from the employees' dates. Deleting a template does not affect checklists already generated.

@@ /hrstudiopayslips
The **accounting firm** sees in **Company payslips** the firm's companies with Human Resources active: after choosing the company it uploads the single PDF, checks the pages and publishes like the HR office. The firm does not see the other data of the file.

@@ /hrsettings
In the **Settings** page the CompanyPowerAdmin sets the general parameters of the module:
- **Deadline notice days**: how many days in advance the module reports the deadlines it records in the shared deadline schedule (courses and checklist items, default 30).
- **Notifications on**: clear it and the module sends no notifications (e.g. "new payslip"). Each user still chooses how to receive them (in the app or by email) from the notification preferences.
- **Notes**: internal notes about the configuration.

Press **Confirm** to save.
