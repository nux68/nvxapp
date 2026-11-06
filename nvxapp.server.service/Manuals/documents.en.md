# Document management manual
Archive of company documents and personnel documents

## Introduction
The document management gathers in a single archive the company documents (compliance certificates, registry extracts, tax payment forms, customer contracts...) and the personnel documents (payslips, tax statements, employment contracts, notices). Each document has a **type**, can be in a **folder** and can have several **versions**.

Functions depend on the role:
- **Employee** (User role): sees their own documents and confirms they have read them;
- **CompanyAdmin**: manages the company archive; sees and uploads personnel documents only if authorised;
- **CompanyPowerAdmin**: all of the above, always authorised for personnel documents, plus the configuration (types, folders, permissions, settings, access log).

The pages are in the **Documents** section of the menu, subsection **Documents**.

> Personnel documents are confidential: only the employee concerned and the authorised administrators can see them. Every opening, download and change is recorded in the access log.

## For the employee: my documents {User}
### Viewing documents
1. Open **Documents > Documents > My documents**.
2. The list shows the documents concerning you, most recent first; a dot next to the title marks the ones you have not opened yet.
3. With **Download** you save the file; with **Open** you see the document details and its versions.

The list also shows the documents of the folders the company makes visible to all employees (for example notices).

### Read confirmation
Some types (for example payslips, contracts, notices) require you to confirm you have read them. The Read confirmation column shows **To confirm**.
1. Read the document (download or open it).
2. Press **Read confirmation** and confirm.
3. The date and time of the confirmation are recorded and the company sees them among the document recipients.

> If the document changes (new version), the confirmation stays linked to what you read: ask the office if in doubt.

## Document archive {CompanyPowerAdmin, CompanyAdmin}
### Searching documents
Open **Documents > Documents > Archive**. Filters let you search by folder, type, category, employee, customer, year and **text**: the search looks at title, description and the text contained in the files (PDF, text documents...).

Next to the documents you see useful warnings: documents **expiring** or expired (for types with an expiry) and documents **beyond the retention period** of their type.

> Documents beyond retention are only flagged: they are never deleted automatically. You decide whether to delete them.

### Uploading a document
1. In the Archive press **Upload document**.
2. Choose the **type**: depending on the category the right fields appear (the employee for personnel documents, the customer for customer documents).
3. Enter title, optional description, date, period (month and year) if the type has one, expiry if it has one, folder.
4. Choose the **status**: as **Draft** the document is not yet visible to the employee; **Published** makes it visible; **Archived** keeps it without highlighting it.
5. Choose the file and confirm.

If the same file is already in another document you get a warning (the upload still happens). If the company enabled it, the employee receives an email when a document is published for them.

**Roles:** company and customer documents are uploaded by CompanyAdmin and CompanyPowerAdmin; personnel documents only by those authorised to upload them.

### Document detail
From the Archive open a document to:
- change its data (type, title, folder, period, expiry, status);
- **Download** the latest version or a previous one;
- upload a **New version** (with a note): previous versions remain available, unless limited in the settings;
- see the **Recipients**: when the employee opened and downloaded the document and when they confirmed reading it.

### Trash
With **Trash** the document leaves the archive but can still be recovered.
1. In the Archive switch to the **Trash** view.
2. Use **Restore** to bring a document back to the archive, or **Delete** to remove it permanently with its files.
3. **Empty the trash** permanently deletes all the documents in the trash.

Documents in the trash for longer than the days set in the settings are deleted automatically at night.

## Employees {CompanyPowerAdmin, CompanyAdmin}
The **Documents > Documents > Employees** page lists the employees to whom personnel documents are delivered. It is visible only to those authorised for personnel documents.
- Employees with the **key icon** (highlighted) have an account and use the app: they are the company users with the User role and appear here automatically.
- The others are **registry only**: their documents are archived, but they cannot view them in the app until they get access.

With **New employee** you add an employee without an account (surname, first name, tax code, employee number, email for notices); with **Edit** you update the data or set the end date. The list options also show deactivated employees.

### Send to several employees
To deliver the same document to several employees (for example a notice or a regulation):
1. On the Employees page press **Send to several employees**.
2. Choose the employees: you can search them by name, tax code, employee number or user, use **Select all** (it applies to those shown) and limit the list to those **with an account**.
3. Choose a **personnel** type, enter title, description, date, period or expiry if required, status.
4. Choose the file and press **Send**.

For each chosen employee a personnel document is created with their own copy of the file, their read confirmation (if the type requires it) and their entry in the log. At the end you see how many documents were created; if it was not possible for some employees, the list explains why.

> Storage is checked before sending for all the copies: if it is not enough, nothing is uploaded.

## Configuration (CompanyPowerAdmin) {CompanyPowerAdmin}
### Types
**Documents > Documents > Types**. Each type has a code, a description and a **category**:
- **Personnel**: documents of an employee, confidential;
- **Company**: company archive, for administrators;
- **Customer**: file of a customer of the shared registry.

For each type choose the **retention years** (empty = no limit), whether it has an **expiry**, whether it requires a **read confirmation** and whether it has a **period** (month/year, for example payslips). At the first activation the most common types are ready (payslip, tax statement, contract, medical certificate, training, notice, tax payment form, compliance certificate, registry extract, customer contract, generic). A type used by documents cannot be deleted: it is deactivated.

### Folders
**Documents > Documents > Folders**. Folders organise the archive and can be nested. A folder **visible to employees** makes the company documents it contains readable, read-only, by all employees (useful for notices and regulations).

### Permissions
**Documents > Documents > Permissions** decides which CompanyAdmins **see** and which **upload** personnel documents (uploading includes seeing). The CompanyPowerAdmin is always authorised and appears as a fixed row.

### Settings
**Documents > Documents > Settings**:
- **Days in the trash**: after how many days documents in the trash are deleted automatically (0 = never);
- **Versions to keep**: how many versions to keep for each document (0 = all); documents requiring a read confirmation always keep them;
- **Email notices to employees**: email to the employee when they receive a new document.

### Access log
**Documents > Documents > Access log** shows who uploaded, opened, downloaded, changed, trashed, restored or deleted each document, with date and time.

## Storage {CompanyPowerAdmin, CompanyAdmin}
**Documents > Documents > Storage** shows the space used and the company package, the number of files, the maximum size of a file and the warning threshold. When usage passes the threshold the administrators receive a warning; above 100% no new documents can be uploaded. For a larger package contact the dealer.
