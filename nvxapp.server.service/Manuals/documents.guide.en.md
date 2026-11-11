## Getting started
This chapter shows where to begin depending on your role. The document management items are in the **Documents** section of the menu; the configuration is in the **Settings** section, visible only to the CompanyPowerAdmin.

### If you are an employee {User}
1. Open **My documents** from the menu.
2. Look for the documents with a dot next to the title: those are the ones you have not opened yet.
3. Press **Download** to save the file or **Open** to see its details.
4. If the **Acknowledgement** column shows "To acknowledge", read the document and press **Acknowledge**.
5. If the **Signature** column shows "To sign", first press **Download** and read the document, then press **Sign** and confirm.

> You only see the documents published in your name: draft documents and those of other employees are never shown to you.

### If you are an administrator {CompanyAdmin, CompanyPowerAdmin}
1. Open **Archive** and try the filters: folder, type, year and **Search in text**.
2. Use **Upload document** to file your first company document (for example a DURC or a registry extract).
3. Check in **Storage** how much space the company uses and the maximum size of a single file.
4. If you also handle payslips and personnel documents, check in **Employees** that you are enabled: without the permission the page tells you so.

### Initial setup {CompanyPowerAdmin}
On first use the document management prepares the default types and folders by itself. Before you start uploading it is worth:
1. Checking in **Types** the retention years, the acknowledgement and the multiple send of the personnel types.
2. Reviewing in **Folders** the proposed structure (Personnel, Administration, Customers, Notices) and adding the folders you need.
3. Deciding in **Permissions** which CompanyAdmins see and upload personnel documents.
4. Setting in **Settings** the days in trash, the versions to keep and the email notifications.

## Key concepts

### Categories
The category of the type decides who sees the document and which data is needed.
| Category | Used for | Who sees it | Required data |
|---|---|---|---|
| Personnel | Payslips, tax certificates, employment contracts, notices to a single employee | The employee (only when Published) and the enabled administrators | Employee |
| Company | DURC, F24, registry extracts, internal documents | CompanyAdmin and CompanyPowerAdmin | Nothing beyond the title |
| Customer | Contracts and documents of a customer in the shared registry | CompanyAdmin and CompanyPowerAdmin | Customer |

### Document status
| Status | Meaning |
|---|---|
| Draft | The document is filed but the employee cannot see it; in the archive the title shows "(Draft)". |
| Published | The document is visible to the employee concerned. It is the status proposed for a new document. |
| Archived | The document can still be consulted by administrators but is no longer visible to the employee. |

### Versions
Each document has a current version and may have previous versions. The employee sees and downloads only the current version; previous versions are visible only to those who can edit the document. With the **Versions to keep** setting the oldest versions are deleted, except in documents that require acknowledgement.

### Acknowledgement
If the type **Requires acknowledgement**, the employee must confirm they have read the document. The date and time of the confirmation are recorded and the administrator sees them among the document **Recipients**, together with the date of the first opening and of the last download. When a **New version** is uploaded, the acknowledgement and the first opening are reset: the employee will have to confirm again.

### Signature
A type can suggest **Requires signature** (personnel only). It is only a suggested value: when you upload or edit a document you can remove or add the signature request for that document, even after sending it. The employee signs with **Sign** after downloading the document: it is a simple electronic signature that records date and time, version, SHA-256 fingerprint of the file, IP address and browser. Signing also counts as acknowledgement. The administrator sees the signature among the **Recipients** and, in the **Archive**, the **Signature** column shows the status of each document: "Signed on …", "Signed x of y", "To be signed" or "Signature request not active". A **New version** resets the signature (it must be signed again), but previous signatures stay in the **Access log** and the versions of signed documents are never deleted.

> A simple electronic signature is freely assessed by a court: it fits confirmations and acceptances (rules, privacy notices, deliveries). Contracts and deeds that require written form need an advanced or qualified signature.

### Permissions: sees and uploads
Personnel documents are confidential. The CompanyPowerAdmin always sees and uploads them; a CompanyAdmin only if enabled on the **Permissions** page.
- **Sees**: views personnel documents read-only (without recipients and without previous versions).
- **Uploads**: uploads, edits, moves to the trash and deletes personnel documents and manages the employee registry. Uploading includes seeing.

### Retention
Each type can have **Retention years**. A document is past retention when those years have passed since the document date (or since the upload, if the date is missing): in the archive the title shows the ⏳ symbol and the document page shows a warning. The document is never deleted automatically.

### Storage
The company has a storage plan. Each upload checks the maximum size of a single file and the remaining space; every version takes up space. Space is freed only by permanent deletion (from the trash or by the automatic version cleanup), not by moving a document to the trash.

@@ /doclist
### Fields
Fields of the **Document** page (new or existing document).
| Field | Description |
|---|---|
| Choose the file | Only for a new document, required. The title is proposed from the file name. Allowed formats: PDF, P7M, images (PNG, JPG, GIF, WEBP, TIF, BMP), Word, Excel, PowerPoint, OpenDocument, RTF, TXT, CSV, XML, ZIP, EML, MSG. |
| Type | Required. Only active types are listed; personnel types only for those who can upload them. It determines the following fields. |
| Employee / Employees | Only for the Personnel category, required. Opens the employee selection dialog. |
| Customer | Only for the Customer category, required: a customer from the shared registry. |
| Folder | Optional; the list shows the full path of nested folders. |
| Title | Required, up to 200 characters. |
| Description | Optional, up to 1000 characters. |
| Date | Document date; used for sorting, the year filter and the retention calculation. |
| Month and Year | Only if the type has a period: month from 1 to 12, year between 1900 and 2200. |
| Expiry | Only if the type has an expiry date. |
| Status | Draft, Published (proposed) or Archived. |
| Version notes | Only for a new document, optional (up to 500 characters): they stay on the first version. |

In the archive list the **Reference** column shows the employee, the customer or the folder of the document; **Period** shows month/year or the document date.

### How to
Upload the monthly payslips, one per employee:
1. Press **Upload document** and choose the payslip file.
2. Choose the **Payslip** type and press the **Employee** row.
3. Search the employee by last name, first name, tax code or employee number and tap it: the dialog closes by itself.
4. Enter the **Month** and **Year** of the period and leave the status **Published**.
5. Press **Confirm**: the employee finds the payslip in **My documents**.

Deliver a notice to several employees:
1. Press **Upload document**, choose the file and a personnel type with **Staff multiple send** turned on.
2. Press the **Employees** row: in the dialog tick the employees or use **Select all**, then **Confirm**.
3. The row shows the employee name or "N employees selected".
4. Fill in title and status and press **Confirm**: one document is created for each employee with the same file.
5. If some documents were not created, you stay on the page and a message tells you how many succeeded and why the others failed.

Upload a new version:
1. Open the document from the archive with **Open**.
2. Press **New version** and choose the updated file.
3. The new version becomes the current one; the previous ones stay in the **Versions** list and can be downloaded by tapping them.

Recover a document moved to the trash:
1. From the page menu switch to **Trash**.
2. Find the document and press **Restore**.
3. With **Back to the archive** you return to the normal view.

### Example
The HR office must deliver the new company regulations to 25 employees and get a read confirmation. It uses the "Notice to the employee" type, which requires acknowledgement, after turning on **Staff multiple send** in **Types**. It uploads the PDF once, picks the 25 employees with **Select all** and confirms. In the following days it opens the single documents and checks in **Recipients** who has already acknowledged.

### Watch out
- Personnel documents can be uploaded and edited only with the **Uploads** permission; otherwise the message "Uploading personnel documents requires the specific permission." appears.
- An inactive type cannot be used for new documents ("The type is no longer active").
- The email notification to the employee is sent only when a document is uploaded in **Published** status. If you upload it as **Draft** and publish it later, no email is sent.
- Moving a document to the **Trash** does not free space. **Delete** in the Trash view and **Empty the trash** permanently delete documents and files, with no way to recover them.
- The trash is visible only to those who can edit documents; **Empty the trash** deletes only the documents you can edit.
- The list shows at most 500 documents: if the warning appears, refine the filters.
- Text search finds words contained in PDF, Word (docx), Excel (xlsx), OpenDocument and text files. Images, scans without text and the old .doc and .xls formats can be found only by title.
- Those who can only see personnel documents open them read-only: they do not see recipients or previous versions.

=> /docemployeelist /doctypelist /docstorage /doclog

@@ /docemployeelist
### Fields
Fields of the **Employee** page (New employee or Edit).
| Field | Description |
|---|---|
| Last name and First name | Up to 100 characters each; at least one of the two is needed. |
| Tax code | Optional; 16 characters (or 11 digits). It is saved in upper case and must be unique within the company. |
| Employee number | Optional, up to 20 characters, unique within the company. |
| Personal email | Optional, must be a valid address. Used for notifications when the employee has no account. |
| End date | End date of the employment. |
| Active | If you untick it the employee is disabled and shown only with **Show disabled**. |

In the list the **Account** column shows the user name of those who use the app, or "No access (registry only)".

### How to
Add an employee without an account:
1. Press **New employee**.
2. Enter last name, first name and, if you know them, tax code and employee number: they help find the employee in the selection dialog.
3. Enter the **Personal email** if you want them to receive new document notifications.
4. Press **Confirm**.

Record that an employee has left:
1. Press **Edit** on the employee row.
2. Enter the **End date** and, if they should no longer appear, untick **Active**.
3. Press **Confirm**: their documents stay in the archive.

### Example
A company without the attendance module enters here the ten workers who do not use the app, with tax code and personal email. When it uploads the payslips it picks them in the dialog by employee number; each worker receives the notification email, and the documents will already be in the app on the day they get an account.

### Watch out
- Adding and editing employees requires the **Uploads** permission; those with **Sees** only can view the list but not change it.
- A tax code or employee number already used by another employee is rejected with a message showing the duplicate value.
- Employees with an account are the company users with the User role: they appear automatically and must not be entered by hand.

=> /doclist /docpermissions

@@ /doctypelist
### Fields
Fields of the **Document type** page.
| Field | Description |
|---|---|
| Code | Required, up to 20 characters; saved in upper case and must be unique. |
| Description | Required, up to 100 characters: the name shown in the lists. |
| Category | Personnel, Company (proposed) or Customer. |
| Staff multiple send | Only for the Personnel category. If on, several employees can be chosen together when uploading. |
| Retention years | Optional, at least 1. Empty means no limit. |
| Has a period (month/year) | The document shows Month and Year (for example payslips, tax certificates, F24). |
| Has an expiry date | The document shows the Expiry date (for example DURC, certificates). |
| Requires acknowledgement | The employee must confirm reading the documents of this type. |
| Requires signature | Personnel category only. Suggests the signature request when a document is uploaded; whoever sends it can remove or add it. |
| Order | Position of the type in the lists. |
| Active | An inactive type is not offered for new documents. |

### How to
Create a type for internal circulars with read confirmation:
1. Press **New type**.
2. Enter a code (for example CIRCULAR) and a description.
3. Choose the **Personnel** category and turn on **Staff multiple send**.
4. Turn on **Requires acknowledgement** and enter the retention years.
5. Press **Confirm**.

### Example
The default personnel types are Payslip (5 years, period, acknowledgement), Tax certificate (CU) (10 years), Employment contract (10 years), Medical certificate (5 years, expiry), Training certificate (10 years, expiry) and Notice to the employee (5 years, acknowledgement). A company that often sends collective notices turns on **Staff multiple send** only on "Notice to the employee", keeping payslips on single choice to avoid mistakes.

### Watch out
- The category cannot be changed if the type is already used by documents, because it decides who can see them.
- **Delete** removes the type only if no document uses it; otherwise it deactivates it. Inactive types are shown again with **Also show inactive**.
- Changing the acknowledgement of a type applies to the documents uploaded from then on.
- Changing **Requires signature** on a type does not affect documents already uploaded: the request is changed on the single document.

=> /doclist /docfolderlist /docsettings

@@ /docfolderlist
### Fields
Fields of the **Folder** page.
| Field | Description |
|---|---|
| Description | Required, up to 100 characters. |
| Parent folder | Optional: the folder it sits in. Empty = top-level folder. |
| Order | Position of the folder in the lists. |
| Visible to employees | Published company documents in the folder can also be viewed, read-only, by employees. |

### How to
Create a subfolder for a year:
1. Press **New folder**.
2. Enter the description, for example "2026".
3. Choose the "Administration" folder as **Parent folder**.
4. Press **Confirm**: in the filters and documents it will appear with its full path.

### Example
To keep the accounting documents in order the administrator creates the "F24" and "DURC" folders under "Administration". In the archive they filter by folder and immediately find all the F24 of the year.

### Watch out
- The default folders (Personnel, Administration, Customers, Notices) cannot be deleted.
- A folder can be deleted only if it is empty, that is without documents and without subfolders: with documents **Delete** is not shown, with subfolders the deletion is rejected.
- A folder cannot be placed inside itself or inside one of its subfolders.
- **Visible to employees** concerns company documents: personnel documents always stay confidential, whatever folder they are in.

=> /doclist /doctypelist

@@ /docpermissions
### Fields
| Field | Description |
|---|---|
| User | User name and roles of the administrator. |
| Sees | Views personnel documents read-only and sees the employee list. |
| Uploads | Uploads, edits and deletes personnel documents and manages employees. Turning it on also turns on Sees. |

### How to
Enable a payroll colleague:
1. Open **Permissions**.
2. Find the colleague in the list.
3. Turn on **Uploads**: the change is saved immediately, no confirmation needed.

### Example
Two CompanyAdmins work in administration: one prepares the payslips, the other handles only accounting. The first gets **Uploads**, the second no permission: they keep managing DURC, F24 and customer contracts without seeing any personnel document.

### Watch out
- Only company administrators can be enabled; the CompanyPowerAdmin is always enabled and their row cannot be changed.
- As long as **Uploads** is on, **Sees** stays on even if you try to turn it off: to remove all access turn off Uploads first.
- Permissions apply to all personnel documents, payslips included: there are no permissions per type or per folder.

=> /docemployeelist /doclog

@@ /docsettings
### Fields
| Field | Description |
|---|---|
| Days in trash | Required, 0 or more; default 30. 0 = the trash is never emptied automatically. |
| Versions to keep | Required, 0 or more; default 0 (all). |
| Email notifications to employees | On by default. Works only if email sending is configured. |

### How to
1. Open **Settings**.
2. Change the values.
3. Press **Confirm**.

### Example
With **Days in trash** at 60 and **Versions to keep** at 3, a document moved to the trash by mistake can be recovered for two months, and a price list updated every month keeps only its last three versions, freeing space.

### Watch out
- The trash is cleaned up at night (after 3 a.m.): the deletion is permanent.
- The version reduction is applied when a new version of the document is uploaded, not right after changing the setting.
- The email goes to the employee's **Personal email** or, if missing, to the email of their account; with neither, no notification is sent.

=> /doctypelist /docstorage

@@ /doclog
### Fields
| Field | Description |
|---|---|
| Date | Date and time of the operation. |
| User | User name of whoever performed it. |
| Action | Upload, New version, Edit, View, Download, Acknowledgement, Trash, Restore, Deletion. |
| Detail | Version number and file name; for a deletion, the document title. |

### How to
Check who downloaded a payslip:
1. Open **Access log**.
2. Type the file name or the user name in the page filter.
3. Read the rows with the **Download** and **View** actions.

### Example
An employee claims they never received their contract. The log shows their user name with a **Download** and an **Acknowledgement** of that file, with date and time.

### Watch out
- The log is reserved for the CompanyPowerAdmin and shows the last 2000 operations of the company, most recent first.
- The log shows the operations on documents that still exist, including those in the trash: after permanent deletion the rows of that document no longer appear. Before deleting an important document, check its history here.

=> /doclist /docpermissions

@@ /docstorage
### Fields
| Field | Description |
|---|---|
| Plan | Name of the company's storage plan. |
| Used | MB used, MB of the plan and percentage. |
| Files | Number of files, maximum size of a single file and warning threshold. |
| Almost full / Out of space | Badge shown above the warning threshold or above 100%. |

### Example
The company has a 1 GB plan with a warning at 80%. Above 80% the page shows **Almost full** and the warning email arrives; the administrator empties the trash and reduces the versions to keep before asking for a larger plan.

### Watch out
- A file larger than the maximum size is rejected with a message showing the file MB and the allowed maximum.
- When space runs out the message shows how many MB are left: delete documents from the trash or ask for a larger plan.
- With a multiple send the space is counted for every copy: 20 employees for a 2 MB file take 40 MB.
- The warning email goes to the company's CompanyPowerAdmin and to the centre that follows it.

=> /docsettings /doclist

@@ /docmine
### Fields
| Field | Description |
|---|---|
| Title | Name of the document; the dot means you have not opened it yet. |
| Type | Kind of document, for example Payslip or Employment contract. |
| Period | Reference month/year, or the document date. |
| Acknowledgement | "To acknowledge" if you still have to confirm, otherwise a tick with the confirmation date. |

### Example
At the beginning of the month Mark receives the email "New document: Payslip". He opens the app, goes to **My documents**, downloads the October payslip and presses **Acknowledge**: the column shows the tick with today's date.

### Watch out
- If the company uploads a new version of a document, the dot reappears and the acknowledgement goes back to "To acknowledge": it must be confirmed again.
- The acknowledgement can be confirmed only once per version and only for the documents you are a recipient of.
- You see and download only the current version of the document.

=> /doclist

## Glossary
| Term | Meaning |
|---|---|
| Type | Template that sets category, retention, expiry, period and acknowledgement of a document. |
| Category | Personnel, Company or Customer: decides who sees the document. |
| Period | Month and year a document refers to, as for payslips. |
| Acknowledgement | The employee's read confirmation, recorded with date and time. |
| Recipient | Employee the document is delivered to; opening, download and acknowledgement are recorded for them. |
| Version | One of the files uploaded over time for the same document. |
| Multiple send | Upload of the same file to several employees, with one document each. |
| Retention | Number of years after which the document is flagged as past retention. |
| Trash | Area of deleted documents that can still be recovered. |
| Permission | A CompanyAdmin's right to see or upload personnel documents. |
| Access log | List of all operations on documents. |

## Troubleshooting

### I cannot find the type I need when uploading {CompanyAdmin, CompanyPowerAdmin}
The type may be inactive, or it is a personnel type and you do not have the **Uploads** permission. The CompanyPowerAdmin can reactivate it in **Types** or enable you in **Permissions**.

### I cannot choose several employees {CompanyAdmin, CompanyPowerAdmin}
Multiple choice is available only for a new document with a personnel type that has **Staff multiple send** turned on. For a document already saved only one employee can be set.

### The file is rejected {CompanyAdmin, CompanyPowerAdmin}
Read the message: file type not allowed (for example an executable), empty file, file larger than the maximum size, or out of space. The **Storage** page shows the maximum size and the remaining space.

### Text search does not find a document {CompanyAdmin, CompanyPowerAdmin}
Text is read only from PDFs containing text, Word (docx), Excel (xlsx), OpenDocument and text files. For scans and images search by title or use the type, folder and year filters.

### The employee did not receive the email {CompanyAdmin, CompanyPowerAdmin}
The email is sent only if **Email notifications to employees** is on, the document was uploaded already **Published** and the employee has a personal email or an account with an email. A document published later does not trigger the notification.

### I do not see the Acknowledge button {User}
The button appears only for documents whose type requires acknowledgement and that you have not confirmed yet. If the column shows a tick, the confirmation is already recorded.
