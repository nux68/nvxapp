# Document management manual
Archive of company documents and personnel documents

## Introduction
The document management gathers in a single archive the company documents (compliance certificates, registry extracts, tax payment forms, customer contracts...) and the personnel documents (payslips, tax statements, employment contracts, notices). This manual is built on **your menu**: you find a chapter for every item you see in the **Documents** section, with the screenshot of the page.

Each document has a **type**, can be in a **folder** and can have several **versions**. The type decides how the document behaves: how many years it must be kept, whether it has an expiry, whether it requires the employee's read confirmation and whether it has a period (month and year, like payslips).

### Document management roles {CompanyPowerAdmin, CompanyAdmin}
Functions depend on the role:
- **Employee** (User role): sees their own documents and confirms they have read them.
- **CompanyAdmin**: manages the company archive; sees and uploads personnel documents only if authorised.
- **CompanyPowerAdmin**: all of the above, always authorised for personnel documents, plus the configuration (types, folders, permissions, settings, access log).

### Confidentiality and tracking {CompanyPowerAdmin, CompanyAdmin}
Personnel documents are confidential: only the employee concerned and the authorised administrators can see them. Every opening, download and change is recorded in the **access log**, with date and time.

> Customer documents use the shared registry (customers and suppliers): a "customer" type document is linked to the customer record and enters their file.

@@ /doclist
The archive is where you search, upload and manage the company documents and, if you are authorised, the personnel and customer documents.

Filters let you search by folder, type, category, employee, customer and year, or by **text**: the search looks at the title, the description and the text contained in the files (PDF, text documents...). Next to the documents you see useful warnings: documents **expiring** or expired (for types with an expiry) and documents **beyond the retention period** of their type.

> Documents beyond retention are only flagged: they are never deleted automatically. You decide whether to delete them.

To upload a new document:
1. Press **Upload document**.
2. Choose the type: the right fields appear for the category (the employee for personnel, the customer for customer documents). For personnel press the **Employee** row: the list of employees opens, with search by surname, first name, tax code or employee number. If the type has **Staff multiple send** you can choose more than one (with **Select all**): one document is created for each with the same file, and at the end you see how many were created and why the others failed.
3. Fill in the data: title, description, date, period and expiry if required, folder.
4. Choose the status: **Draft** is not yet visible to the employee, **Published** makes it visible, **Archived** keeps it without highlighting it.
5. Upload the file and confirm.

If the same file is already in another document you get a warning, but the upload still happens. If the company enabled email notices, the employee receives a message when a document is published for them. Company and customer documents are uploaded by CompanyAdmin and CompanyPowerAdmin; personnel documents only by those authorised.

Opening a document from the archive you can change its data, download the latest version or a previous one, upload a **New version** with a note (previous versions remain available, unless limited in the settings) and see the **Recipients**: when the employee opened and downloaded the document and when they confirmed reading it.

With **Trash** a document leaves the archive but can still be recovered:
1. Switch to the **Trash** view.
2. Use **Restore** to bring it back to the archive.
3. Use **Delete** to remove it permanently with its files.

With **Empty the trash** you permanently delete all the documents in the trash. Documents left in the trash beyond the days set in the settings are deleted automatically at night.

@@ /docemployeelist
Here you find the list of the employees to whom personnel documents are delivered and their file. The page is visible only to those authorised for personnel documents.

- Employees with the **key icon** (highlighted) have an account and use the app: they are the company users with the User role and appear here automatically.
- The others are **registry only**: their documents are archived, but they cannot view them in the app until they get access.

With **New employee** you add an employee without an account (surname, first name, tax code, employee number, email for notices); with **Edit** you update the data or set the end date. The list options also show deactivated employees.

To deliver the same document to several employees (for example a notice or a regulation) use a type with **Staff multiple send** and upload the document from the archive: when choosing the employee you can select more than one.

> Storage is checked before sending for all the copies: if it is not enough, nothing is uploaded.

@@ /doctypelist
Types define how the documents in the archive behave. Each type has a code, a description and a **category**:
- **Personnel**: documents of an employee, confidential;
- **Company**: company archive, for administrators;
- **Customer**: file of a customer of the shared registry.

For each type choose the **retention years** (empty = no limit), whether it has an **expiry**, whether it requires a **read confirmation**, whether it suggests a **signature** (changeable at each send) and whether it has a **period** (month/year, for example payslips). For personnel types, **Staff multiple send** lets you choose several employees at once when uploading a document (one document each, same file).

At the first activation the most common types are ready (payslip, tax statement, contract, medical certificate, training, notice, tax payment form, compliance certificate, registry extract, customer contract, generic). A type used by documents cannot be deleted: it is deactivated.

@@ /docfolderlist
Folders organise the archive and can be nested, to give a clear structure to the documents (for example by year, by area or by subject).

A folder **visible to employees** makes the company documents it contains readable, read-only, by all employees: it is useful for notices, regulations and forms that must reach everyone.

@@ /docpermissions
Here you decide which CompanyAdmins can work on the personnel documents, which are confidential.

For each administrator choose who **sees** and who **uploads** the personnel documents; those authorised to upload also see them. The CompanyPowerAdmin is always authorised and appears as a fixed row that cannot be changed.

@@ /docsettings
The settings control the general behaviour of the archive:
- **Days in the trash**: after how many days documents in the trash are deleted automatically (0 = never);
- **Versions to keep**: how many versions to keep for each document (0 = all); documents requiring a read confirmation always keep them;
- **Email notices to employees**: send an email to the employee when they receive a new document.

@@ /doclog
The access log keeps track of everything that happens to the documents: who uploaded, opened, downloaded, changed, trashed, restored or deleted each document, with date and time.

It is the tool to verify the confidentiality of personnel documents and to reconstruct the history of a document when in doubt.

@@ /docstorage
This page shows the space used and the company package, the number of files, the maximum size allowed for a single file and the warning threshold.

When usage passes the threshold the administrators receive a warning; above 100% no new documents can be uploaded. For a larger storage package contact the dealer that follows your company.

@@ /docmine
Here you find the documents concerning you as an employee: payslips, contracts, notices and the other documents the company archives in your name.

The list shows the documents most recent first; a dot next to the title marks the ones you have not opened yet. With **Download** you save the file, with **Open** you see the document details and its versions. The list also shows the documents of the folders the company makes visible to all employees (for example notices).

Some types (for example payslips, contracts, notices) require you to confirm you have read them: the **Read confirmation** column shows "To confirm".
1. Read the document, downloading or opening it.
2. Press **Read confirmation**.
3. Confirm: the date and time are recorded and the company sees them among the recipients.

> If the document changes (new version), the confirmation stays linked to what you read: ask the office if in doubt.

## Frequently asked questions
### I deleted a document by mistake, can I recover it?
If it is still in the trash, yes: open the archive, switch to the **Trash** view and use **Restore**. Once the trash is emptied, or the set days have passed, the deletion is permanent.

### Why can't I upload a new document? {CompanyPowerAdmin, CompanyAdmin}
Most likely the company storage has passed 100%: check the storage page. Above the limit uploads are blocked until you free space or move to a larger package.

### An employee cannot see their documents, why? {CompanyPowerAdmin, CompanyAdmin}
Check that the employee has an account (key icon) and that the documents are in **Published** status: in Draft status they are not visible. Registry-only employees cannot view documents in the app until they get access.
