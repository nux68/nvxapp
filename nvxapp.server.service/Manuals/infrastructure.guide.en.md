## Getting started
This chapter walks you through the first operations, according to your role. You only see the parts that concern the roles you have.

### The first login
1. Open the login page, enter **User Name** and **Password** (at least 3 characters), then press **Login**.
2. If you received a temporary password, use the **Change password** link on the login page to choose your own (at least 8 characters).
3. After logging in the **Dashboard** opens: tell the assistant what you want to do or choose an item from the menu.
4. With the round button at the bottom you can switch between the light and dark theme and between Italian and English.

> Type the user name exactly as it was given to you; several users can share the same email, which is why login and password recovery always ask for the user name.

### If you manage a company {CompanyPowerAdmin}
1. Open **Users** and create the users of your staff, choosing the User or CompanyAdmin role for each one.
2. If you use attendance, CRM or document management, open **Customers** and enter your customers: the registry is shared by all applications.
3. With **User menus** you hide from CompanyAdmins and Users the items they must not use.
4. For any problem open a ticket from **Tickets**: it reaches your financial advisor.

### If you are a company administrator {CompanyAdmin}
You work on the applications active for your company, with the items the CompanyPowerAdmin left in your menu. For support requests use **Tickets**: you can open a request as company administrator, which reaches the financial advisor.

### If you are an employee {User}
In the menu you find the functions of the applications you use every day. For a question or a problem open a ticket from **Tickets** choosing "Employee": the request reaches your company manager.

### If you work at a financial advisor {FinancialAdvisorPowerAdmin, FinancialAdvisorAdmin}
1. Open **Companies** and create the companies you follow: each one gets two administrators (CompanyPowerAdmin and CompanyAdmin).
2. Ask your dealer to activate the applications each company needs: the financial advisor cannot activate them.
3. With **Impersonate** you enter the company as its CompanyPowerAdmin to configure it or help it.
4. In **Financial Advisor users** you manage the colleagues who work with you.

### If you work at a dealer {DealerPowerAdmin, DealerAdmin}
1. Open **Financial Advisors** and create the dealer's financial advisors, each with its administrators.
2. The DealerPowerAdmin enables the financial advisors' applications and, impersonating a financial advisor, activates the companies' applications from their record.
3. In **Document storage** you check the document management space used by the dealer's companies.
4. In **Dealer users** you manage the dealer's colleagues.

### If you are the SuperUser {SuperUser}
1. Create the dealers from **Dealers**; from there, impersonating, you move down to financial advisors and companies.
2. Prepare the ticket **Request types** and, if you use automatic answers, the **AI documentation** and the values of **Configure AI answers**.
3. If document management is active, define the **Storage plans** and the default values.
4. With **Move company** you move a company under another financial advisor.

## Key concepts

### The hierarchy
The application is a tree of nodes: each node has its own users and only sees what is below it.
| Level | Who it is | Roles |
|---|---|---|
| SuperUser | Who manages the whole installation | SuperUser |
| DEV | The technical team that receives tickets from the SuperUser | PowerAdmin, Admin |
| Dealer | The reseller: creates financial advisors and activates applications | DealerPowerAdmin, DealerAdmin |
| Financial Advisor | The professional firm that follows the companies | FinancialAdvisorPowerAdmin, FinancialAdvisorAdmin |
| Company | The customer company | CompanyPowerAdmin, CompanyAdmin |
| User | The company's employee or collaborator | User |

### Power and Admin
Each node has two kinds of administrator. The **Power** one (DealerPowerAdmin, FinancialAdvisorPowerAdmin, CompanyPowerAdmin) is the main administrator: configures, decides and receives the tickets not yet assigned. The **Admin** works alongside, but sees some settings read only. When a dealer, financial advisor or company is created, two main users are always created, one of each kind, with names taken from the node name (for example Rossi_PowerAdmin and Rossi_Admin).
> Main Power users cannot be deleted one by one: they only go away when their node is deleted.

### Impersonating
By impersonating you enter the application as another user and see exactly their menu.
- From **Dealers**, **Financial Advisors** and **Companies** you enter as the main (Power) administrator of that node.
- From the company **Users** you can impersonate only users with the User role, not administrators.
- You can go down several levels in a row (dealer, then financial advisor, then company): the path at the top shows the chain and one click takes you back to a previous level.
- Operations reserved to the SuperUser stay allowed while impersonating, because the user who logged in is checked.

### Applications
Attendance, CRM and document management are activated company by company from the company record. Only the SuperUser and the DealerPowerAdmin of the company's dealer can activate or deactivate them. Deactivating an application blocks access but keeps the data: reactivating it brings everything back. The **Firm applications** are something else: they enable the firm-side functions and do not activate anything on the companies.

### The round button
At the bottom of the page there is a round button that opens in two directions.
- Upwards: the assistant chat, the microphone for voice commands, the light/dark theme switch and the language switch (the button shows the language you switch to; the page reloads).
- To the left: the actions of the current page, for example the **+** to add an item.

### Lists
Lists have the **Search** box at the top and, where useful, the **Options**: **Show disabled** also shows disabled rows, **Enable deletion** shows the **Delete** button on each row (only for those allowed to delete). A **Disabled** row is not deleted: it leaves the lists, and a disabled user no longer receives tickets or warnings.

### Notifications
- Next to the **Tickets** item the counter shows the tickets assigned to you; at each step of a ticket that concerns you a popup appears with the button to open it.
- Long jobs (exports, reports, manuals) report in the jobs bar when the file is ready.
- When a company's document storage goes over the warning threshold and when it reaches 100%, the company's CompanyPowerAdmin and the dealer's DealerPowerAdmin receive a warning.

@@ /dashboard
### How to
1. Press **Ask the assistant…**: the chat opens.
2. Type your request in your own words, for example "open customers" or "switch to dark theme".
3. If the operation changes data, the assistant shows a summary and waits for your confirmation; opening a page needs no confirmation.

### Things you can ask
- **help** or **what can you do**: the list of commands available for your roles, starting with those of the current page.
- **open** followed by the name of a page, or "open the record" of an item giving its name.
- **dark theme** or **light theme**, **language English** or **language Italian**.
- **cancel** or **reset** to drop the request in progress.

### Voice commands
With the microphone of the round button the assistant listens continuously; alternatively hold down the **Hold to talk** button. After a period of inactivity the microphone turns off by itself: press **Turn back on** to restart it. Voice replies are turned on and off from the chat header.
> Operations that change data are never confirmed automatically, not even by voice.

=> /ticketlist /user

@@ /user
### Changing the password
The **Change password** page opens from the link with the same name on the login page and does not require you to be logged in.
| Field | Description |
|---|---|
| Username | Your user name, required. |
| Current password | The password you use today, required. |
| New password | Required, at least 8 characters, different from the current one. |
| Confirm new password | Must be the same as the new password. |
Press **Save password** and then sign in with the new password.

### Recovering a forgotten password
1. On the login page press **Forgot your password?**.
2. Enter your **Username** and press **Send link**.
3. Open the email you received (check the spam folder too) and press **Reset the password**.
4. Choose the new password (at least 8 characters), confirm it and press **Save password**.

### Watch out
- The recovery link is valid for 24 hours and can be used only once; if it expired, request a new one with **New link**.
- To avoid revealing which user names exist, the confirmation is always the same: if the email does not arrive, check that you typed the user name correctly.
- When changing the password, a wrong user name and a wrong current password give the same message.

=> /dashboard

@@ /companypoweradmin
### What you can do
As CompanyPowerAdmin you are the company's main administrator:
- you create users and decide their roles;
- you maintain the customer registry shared by the applications;
- you choose which menu items CompanyAdmins and Users see;
- you receive the tickets opened by employees and those not yet assigned, and open tickets to the financial advisor;
- you manage the AI documentation and the automatic support answers for your company.
> You do not activate the company's applications: the SuperUser or the dealer's DealerPowerAdmin does.

=> /usercompanylist /customerlist /companymenurestriction /ticketlist

@@ /companyadmin
### What you can do
As CompanyAdmin you work on the active applications with the items the CompanyPowerAdmin left you. You handle the tickets assigned to you, open requests to the financial advisor and read (without changing) the company's AI documentation and automatic answer settings.

=> /ticketlist /aidocumentlist

@@ /usercompanylist
### Fields
In the **New user** window and in the user record:
| Field | Description |
|---|---|
| UserName | Name used to log in, required, up to 50 characters. It must be unique in the whole application and cannot be changed after creation. |
| Email | Required for a new user, must be a valid address. Several users can share the same email. |
| Password | Only for a new user, required. |
| Confirm Password | Must match the password. |
| Role | One or more roles among User and CompanyAdmin; User is proposed. The main CompanyPowerAdmin keeps their role. |
| Disabled | Removes the user from the lists: they no longer receive tickets or warnings. You find them again with **Show disabled**. |

### How to
1. Press **+**; if an application allows importing, choose **New user** or **Import from file**.
2. Fill in the fields and press **Confirm**: the record of the new user opens.
3. Give the user their user name and temporary password, and ask them to change it with **Change password** from the login page.

### Example
Laura Bianchi joins the company and will also handle her colleagues' requests. You create the user l.bianchi with her email, a temporary password and the roles User and CompanyAdmin: as User she uses the applications, as CompanyAdmin she can receive the tickets you assign to her.

### Watch out
- The company's main administrators cannot be deleted: they have no **Delete** button.
- Deleting a user removes them from the company and deletes their data in the company's applications; the user is deleted completely only if not linked to other nodes, otherwise they stay linked elsewhere.
- Tickets opened by the deleted user are deleted; those assigned to them go back to their level without an assignee.
- You cannot delete your own user, nor the company's last active CompanyPowerAdmin.
- The password chosen here for a new user may be short; to change it later at least 8 characters are needed.

=> /companymenurestriction /companylist /ticketlist

@@ /customerlist
### Fields
| Field | Description |
|---|---|
| Company name | Required, up to 100 characters. |
| Contact person | Name of the contact person, up to 100 characters. |
| Type | Customer, Supplier or Customer and supplier; Customer is proposed. |
| Active | On for new customers; off, the customer stays in the history but is not offered in selections. |
| VAT number | Optional, exactly 11 digits. |
| Tax code | Optional, 16 characters (individual) or 11 digits (company). |
| Address, Postcode, City | Up to 100, 10 and 50 characters. |
| Province | 2-letter code, for example MI. |
| Country | 2-letter code, for example IT. |
| Phone | Up to 30 characters. |
| Email and PEC | Valid addresses, up to 100 characters. |
| Recipient code (SDI) | For electronic invoicing: 7 characters, 6 for the Public Administration. |
| Notes | Free text, up to 500 characters. |

### Contact persons
In the **Contact person** window you enter Surname, First name, Role, Phone, Mobile, Email, Notes and the **Main contact** switch. At least the surname or the first name is needed. A customer has only one main contact: marking one, the others stop being main.

### Example
You enter the customer Ferramenta Verdi Srl, type Customer, with VAT number, address in Bergamo (province BG, country IT) and SDI recipient code. After the first save you add Mario Verdi as main contact and the purchasing office as a second contact person.

### Watch out
- Only the CompanyPowerAdmin and the CompanyAdmin can change the customer registry; applications (for example the CRM) may have their own pages with their own rules.
- A customer used by an application cannot be deleted: the message tells you where and how many times it is used (for example jobs). Deactivate it by turning off **Active**.
- The item appears only if the company has at least one active application that uses customers.

=> /usercompanylist /companymenurestriction

@@ /companymenurestriction
### How to
1. Open **User menus**: for each item there is a switch in the **Admin** column and one in the **User** column.
2. Turn off the switch of the items to hide from that role; where the item is not available for the role there is no switch.
3. Press **Confirm** to save.

### Example
An application offers employees an item your company does not use: in the **User** column turn that item off and press **Confirm**. Employees no longer see it, CompanyAdmins still do.

### Watch out
- Items can only be removed: a role will never see an item the program does not provide for it.
- Removed pages cannot be opened even by typing their address in the browser.
- Items added in the future by the applications are visible until you remove them.
- Only the CompanyPowerAdmin can change this configuration.

=> /usercompanylist /companypoweradmin

@@ /superuser
### What you can do
The SuperUser manages the installation:
- creates and manages dealers and, impersonating them, financial advisors and companies;
- manages the PowerAdmin and Admin users of the DEV level;
- moves a company under another financial advisor;
- defines the document management storage plans and the default values;
- prepares the ticket request types, the AI documentation and the default values of the automatic answers;
- opens tickets to the DEV level, assigning them directly to a PowerAdmin or Admin.

=> /dealerlist /userlist /companymove /tickettypelist

@@ /dealerlist
### Fields
| Field | Description |
|---|---|
| Description | Name of the dealer, required, up to 20 characters. Special characters are removed on creation. |
| Email | Only on creation, required: email of the two main administrators. |
| Password | Only on creation, required: initial password of the two main administrators. |
| Confirm Password | Must match the password. |
| Disabled | Removes the dealer from the lists (applies to its main administrators). |
While you type the name, a warning shows the user names that will be created, for example Nord_Admin and Nord_PowerAdmin.

### How to
1. Press **+**, fill in the fields and press **Confirm**.
2. Give the dealer the two user names and the initial password.
3. With **Impersonate** you enter the dealer as DealerPowerAdmin to create its financial advisors.

### Watch out
- User names are unique in the whole application: choose a dealer name that does not generate names already in use.
- Deleting a dealer deletes its whole branch: financial advisors, companies with their application data, documents, tickets born in the branch and the users not linked elsewhere. Before confirming, the window shows the summary and asks you to type the dealer name.
- Only the SuperUser deletes a dealer.

=> /financialadvisorlist /companymove /userlist

@@ /userlist
### Fields
| Field | Description |
|---|---|
| UserName | User name, required, up to 50 characters, unique in the whole application. |
| Email | Required for a new user, valid address. |
| Password and Confirm Password | Only for a new user, required and equal. |
| Role | Required: PowerAdmin or Admin. |

### How to
The list shows the users with the PowerAdmin or Admin role, that is the DEV level that receives tickets from the SuperUser. Press **+** to create one; users of dealers, financial advisors and companies are created from their own lists.

### Watch out
- Only the SuperUser deletes, after turning on **Enable deletion** in the **Options**.
- A PowerAdmin user is never deleted; the SuperUser and your own user cannot be deleted.
- Deletion is permanent: tickets opened by the user are deleted, those assigned to them go back to the level without an assignee.

=> /dealerlist /superuser

@@ /companymove
### Fields
| Field | Description |
|---|---|
| Company to move | Required. The **Dealer** and **Financial Advisor** filters help you find it; below, the current position appears. |
| Move under | The target **Dealer** and **Financial Advisor**; the financial advisor is required and must differ from the current one. |

### How to
1. Choose the company: the page shows the **Current position**.
2. Choose the target dealer and financial advisor: the page shows the **New position**.
3. Press **Confirm** and answer the confirmation question.

### What happens
- Users, active applications, data and hidden menu items stay with the company.
- Users of the old financial advisor no longer see the company; those of the new one find it in **Companies**.
- The company's tickets that are not closed move to the new financial advisor (and to the new dealer if it changes); those that were at the financial advisor or dealer go back to the level without an assignee.

### Watch out
The operation is reserved to the SuperUser, even while impersonating another user.

=> /companylist /financialadvisorlist /ticketlist

@@ /storageplanlist
### Fields
**Storage plan** record:
| Field | Description |
|---|---|
| Description | Name of the plan, required, up to 50 characters, different from the others. |
| Space (MB) | Required, greater than zero; 1024 is proposed. |
| Maximum file size (MB) | Optional; empty means the installation default. |
| Order | Position in the list. |
| Active | An inactive plan stays with the companies that have it but can no longer be assigned. |
| Default | Assigned to companies when they activate document management; there is always exactly one. |

**Default values** window (from the page's round button):
| Field | Description |
|---|---|
| Maximum file size (MB) | Required, greater than zero; 50 is proposed. |
| Warning threshold (%) | Required, from 1 to 99; 80 is proposed. |

### Watch out
- The default plan always stays active and cannot be deleted: first make another plan the default.
- A plan used by some company is not deleted: it is only deactivated.
- If there is no active plan, companies cannot upload documents.

=> /companystoragelist

@@ /companystoragelist
### Fields
**Company document storage** record, with the used space and number of files at the top:
| Field | Description |
|---|---|
| Plan | Required, among the active plans. |
| Maximum file size (MB) | Optional, greater than zero; empty means the plan or default value. |
| Warning threshold (%) | Optional, from 1 to 99; empty means the default value. |

### Who can do what
| Role | What they see | What they change |
|---|---|---|
| SuperUser | All companies | Plan and limits |
| DealerPowerAdmin | The dealer's companies | Plan and limits |
| DealerAdmin | The dealer's companies | Nothing |
| Financial Advisor | The financial advisor's companies | Nothing |

### Example
Rossi Spa is at 85% of its 1 GB plan and received the warning. The DealerPowerAdmin opens its row, chooses the 5 GB plan and confirms: the company can keep uploading documents.

### Watch out
- Beyond 100% the company can no longer upload documents until it frees space or gets a larger plan.
- If you assign a plan smaller than the space already used a warning appears: the operation succeeds, but uploads stay blocked until usage drops below the new limit.
- An inactive plan can no longer be assigned.

=> /storageplanlist /companylist

@@ /poweradmin
### What you can do
The PowerAdmin belongs to the DEV level: receives the tickets the SuperUser assigns or that move up to DEV, assigns them to Admin colleagues, asks for information and resolves them.

=> /ticketlist /userlist

@@ /admin
### What you can do
The Admin belongs to the DEV level: works on the tickets assigned to them, adds notes, asks for information and resolves them.

=> /ticketlist

@@ /dealerpoweradmin
### What you can do
As DealerPowerAdmin you are the dealer's main administrator:
- you create financial advisors and enable their applications;
- you activate and deactivate the companies' applications from their record (entering the financial advisor with **Impersonate**);
- you assign document storage plans to the dealer's companies;
- you manage the dealer's colleagues, AI documentation and automatic answers;
- you receive the tickets that move up from the financial advisors.

=> /financialadvisorlist /userdealerlist /companystoragelist /ticketlist

@@ /dealeradmin
### What you can do
As DealerAdmin you work with the DealerPowerAdmin: you see the financial advisors and the document storage of the dealer's companies, handle the tickets assigned to you and read (without changing) the dealer's AI documentation. Applications and storage plans are changed by the DealerPowerAdmin.

=> /financialadvisorlist /ticketlist

@@ /userdealerlist
### Fields
| Field | Description |
|---|---|
| UserName | Required, up to 50 characters, unique in the whole application; cannot be changed after creation. |
| Email | Required for a new user, valid address. |
| Password and Confirm Password | Only for a new user, required and equal. |
| Role | Required: DealerPowerAdmin or DealerAdmin. |
| Disabled | Removes the user from the dealer's lists: they no longer receive tickets or warnings. |

### Watch out
- The SuperUser and the DealerPowerAdmin delete dealer users; the dealer's main administrators cannot be deleted.
- The dealer must always have at least one active DealerPowerAdmin.

=> /dealerpoweradmin /financialadvisorlist

@@ /financialadvisorlist
### Fields
| Field | Description |
|---|---|
| Description | Name of the financial advisor, required, up to 20 characters; special characters are removed on creation. |
| Email, Password, Confirm Password | Only on creation: email and initial password of the two main administrators. |
| Disabled | Removes the financial advisor from the lists (applies to its main administrators). |
| Firm applications | Enable the firm-side functions, for example the document management firm area. |

### Watch out
- Firm applications can be enabled only by the SuperUser and the dealer's DealerPowerAdmin; the DealerAdmin sees them read only.
- Enabling an application for the financial advisor does not activate it on its companies.
- Deleting a financial advisor deletes its companies with their data, the documents, the tickets born there and the users not linked elsewhere. The SuperUser and the dealer's DealerPowerAdmin can do it.

### Example
The dealer takes on the Neri firm: the DealerPowerAdmin creates the financial advisor "StudioNeri" with the owner's email, enables document management among the firm applications and gives out the user names StudioNeri_PowerAdmin and StudioNeri_Admin.

=> /companylist /userfinancialadvisorlist /dealerpoweradmin

@@ /financialadvisorpoweradmin
### What you can do
As FinancialAdvisorPowerAdmin you are the financial advisor's main administrator:
- you create the companies you follow and, if needed, delete them;
- you enter the companies with **Impersonate** to configure or assist them;
- you manage the financial advisor's colleagues;
- you receive the tickets that move up from the companies and assign them;
- you maintain the financial advisor's AI documentation and automatic answers.

=> /companylist /userfinancialadvisorlist /ticketlist /aidocumentlist

@@ /financialadvisoradmin
### What you can do
As FinancialAdvisorAdmin you work on the financial advisor's companies, handle the tickets assigned to you and read (without changing) the financial advisor's AI documentation and automatic answers.

=> /companylist /ticketlist

@@ /userfinancialadvisorlist
### Fields
| Field | Description |
|---|---|
| UserName | Required, up to 50 characters, unique in the whole application; cannot be changed after creation. |
| Email | Required for a new user, valid address. |
| Password and Confirm Password | Only for a new user, required and equal. |
| Role | Required: FinancialAdvisorPowerAdmin or FinancialAdvisorAdmin. |
| Disabled | Removes the user from the financial advisor's lists: they no longer receive tickets or warnings. |

### Watch out
- The SuperUser, the FinancialAdvisorPowerAdmin and the dealer's DealerPowerAdmin delete financial advisor users; the main administrators cannot be deleted.
- The financial advisor must always have at least one active FinancialAdvisorPowerAdmin.

=> /financialadvisorpoweradmin /companylist

@@ /companylist
### Fields
| Field | Description |
|---|---|
| Description | Name of the company, required, up to 20 characters; special characters are removed on creation. |
| Email | Only on creation, required: email of the CompanyPowerAdmin and the CompanyAdmin. |
| Password and Confirm Password | Only on creation: initial password of the two administrators. |
| Disabled | Removes the company from the lists (applies to its main administrators). |
| Applications | Attendance, CRM, Document management: ticked and saved, they are activated; unticking blocks access but keeps the data. |

### Example
The financial advisor takes on Rossi Spa. The FinancialAdvisorPowerAdmin creates the company "RossiSpa" with the administrator's email: the users RossiSpa_PowerAdmin and RossiSpa_Admin are created. Then they ask the dealer to activate attendance; the DealerPowerAdmin opens the company record, ticks Attendance and confirms.

### Watch out
- Applications are activated only by the SuperUser and the dealer's DealerPowerAdmin: for the others the section is read only, and a company created by the financial advisor starts without applications.
- The generated user names must be unique in the whole application: avoid company names already in use.
- Deleting a company is permanent: the data of all its applications, documents, tickets and users not linked elsewhere are deleted. Export first the data you must keep (attendance and payroll have retention obligations). The SuperUser and the financial advisor's FinancialAdvisorPowerAdmin can do it.

=> /usercompanylist /companymove /companystoragelist

@@ /ticketlist
### The views
| View | What it contains |
|---|---|
| Assigned to me | Tickets assigned to you, or that reached your level when you are the node's Power. |
| Opened by me | The requests you opened. |
| Others | Tickets of the levels below you and those you handled in the past. |
With **State** you filter Open, In progress, Waiting for information, Resolved or Closed. Open and Resolved rows are highlighted.

### New ticket fields
| Field | Description |
|---|---|
| Open as | The role you open with, if you have more than one: it decides who receives the request. |
| Request type | Required; only the types available for the chosen role and the active applications are offered. |
| Assign to | Only for the SuperUser: the PowerAdmin or Admin who receives the ticket. |
| Subject | Required, up to 200 characters. |
| Description | Required, up to 4000 characters. |
| Attachments | Up to 5 files at a time, each up to 10 MB and up to 20 MB in total. |
Allowed files are PDF, images, text, CSV, JSON, XML, Office and OpenDocument documents, ZIP and 7Z archives, EML and MSG emails.

### Ticket actions
| Button | What it is for |
|---|---|
| Assign | The level's Power entrusts the ticket to a colleague. |
| Forward | Moves the ticket to the upper level provided by the request type. |
| Ask for information | Asks the opener, or an intermediate level the ticket went through, for details. |
| Reply | Answers a request for information. |
| Note | Adds a text or an attachment without changing the state. |
| Change type | Changes the request type. |
| Resolve | Sends the solution to the opener. |
| Close ticket | The opener closes a resolved ticket for good. |
| Reopen | The opener reopens a resolved ticket that does not satisfy them. |

### How to
1. Open the ticket from the list: at the top you see state, level and the user handling it.
2. In the **History** you read every step, with texts and attachments to download.
3. Press the action button, write the text and confirm.

### Example
An employee opens a ticket "I can't see my August holidays" as Employee: it reaches the CompanyPowerAdmin, who assigns it to the CompanyAdmin. The CompanyAdmin asks the employee for information, gets the reply and resolves. The employee reads the solution and closes the ticket.

### Watch out
- A closed ticket can no longer be reopened: to go back to the list without closing it use **Back to list**.
- If another user has just worked on the same ticket, your action is rejected: reload the page and try again.
- If the request type provides no upper level, the ticket cannot be forwarded.
- The company's only CompanyPowerAdmin cannot open tickets as employee, because nobody could handle them.
- When the ticket shows **Automatic answer in progress**, the AI assistant is preparing an answer; if an operator acts first, the automatic answer stops.

=> /tickettypelist /aidocumentlist /tickettypeaisetting

@@ /tickettypelist
### Fields
| Field | Description |
|---|---|
| Code | Required, up to 20 characters, different from the other types. |
| Description | Required, up to 100 characters: the text offered to whoever opens the ticket. |
| Application | General or an application: types of an application are offered to companies and employees only if the application is active. |
| Enabled levels | At least two, among SuperUser, DEV, Dealer, Financial Advisor, Company and User. |
| Order | Position in the list of types. |
| Active | An inactive type can no longer be selected, but stays on existing tickets. |

### How levels work
The ticket is opened from an enabled level and always moves to the first enabled level above the current one; levels not enabled are skipped. A type is offered to whoever opens from a level only if at least one other enabled level is above it; tickets opened by the SuperUser need the SuperUser and DEV levels.

### Example
The type PRES01 "Clocking problems", application Attendance, with the levels User, Company and Financial Advisor: the employee opens to the company, the company can forward to the financial advisor, and the ticket goes no higher.

### Watch out
Request types are managed only by the SuperUser; they are not deleted, they are deactivated.

=> /ticketlist /aidocumentlist /tickettypeaisetting

@@ /aidocumentlist
### Fields
**AI document** record:
| Field | Description |
|---|---|
| Title | Required, up to 200 characters. |
| Description | Up to 1000 characters: what the document is for; it helps the assistant decide when to use it. |
| Request types | The document is used only for tickets of these types; no type means all types. |
| Application | General or the application the document refers to. |
| Also for lower levels | The document is also used for tickets of the levels below. |
| Original file | PDF, Word, OpenDocument, Excel or text, up to 10 MB; its text replaces the content. |
| Content | Required, up to 100,000 characters: the text the assistant reads. |
| Order | If there are too many documents, the assistant reads those with the lowest order first. |
| Active | An inactive document is not used. |

### How to
1. If you manage several levels, choose the **Level** at the top.
2. Press **+** (**New document**) and fill in title and description.
3. Press **Upload a file** to extract the text from a file, or write the content; correct the text if needed.
4. Choose the request types and press **Confirm**.

### Example
The financial advisor uploads its payroll handbook once, with **Also for lower levels** on and the type "Payroll questions": the assistant uses it to answer tickets of that type reaching the financial advisor and its companies, where the use of upper-level documentation is on.

### Watch out
- The SuperUser and the Power users of dealer, financial advisor and company change the documents; Admins see them read only.
- The content of the documents is sent to the AI service: do not upload data that must not leave the company.
- Old-format .doc and .xls files cannot be read: save them as .docx or .xlsx. A scanned PDF contains no text: write or paste the text in the content. A password-protected file cannot be read.
- Beyond 100,000 characters split the text into several documents.
- When a document is deleted, answers already given still mention it by title.

=> /tickettypeaisetting /tickettypelist /ticketlist

@@ /tickettypeaisetting
### Fields
| Field | Description |
|---|---|
| Level | If you manage several levels, the one to configure. |
| Customize | Only for a node: on, you use your own values; off, the SuperUser's default values apply. |
| AI answer | Whether the assistant answers tickets of that type when they reach the level. |
| Attempts | From 1 to 10: how many automatic answers it tries before leaving the ticket to an operator. |
| Use the documentation of the upper levels | Whether the assistant also uses the upper levels' documents published for the lower levels; off, it uses only the node's own. |

### How it works
When a ticket reaches the level, the assistant answers if the AI answer is on for that type and there are usable documents. If it finds the answer, the ticket goes back to the opener as resolved; if the answer is not satisfactory and the ticket is reopened, the assistant tries again while attempts remain, then the ticket goes to an operator. Operators are notified only when the assistant has finished.

### Example
The SuperUser sets as default, for the Company level, the AI answer on with 2 attempts. A company that prefers always to answer in person opens the page, turns on **Customize** for the type and turns off **AI answer**.

### Watch out
- The default values of each level are decided by the SuperUser; for the DEV level only those apply.
- In the node only the Power changes them; Admins see the page read only.
- The assistant never answers tickets opened by the SuperUser or tickets that reach the SuperUser.

=> /aidocumentlist /tickettypelist /ticketlist

@@ /paymentreport
### What you can do
Companies and firms pay their licences to the platform; every month the platform passes each centre its share, that is the amount collected from the centre's customers minus the platform fee. On this page the SuperUser sees, by year or by month, each centre's collected amount, the fee, the centre's share, how much has already been paid and how much is confirmed but still to be transferred.

### How to
1. Choose year and month (or **Whole year**).
2. Once the month has ended, from the round button choose **Generate the month's statements**: a draft is created for each centre with payments to settle.
3. With **Detail** open the centre, check the draft and confirm it: the centre receives the email with the statement PDF.
4. After the transfer, choose **Pay** on the statement and enter date and reference: the centre receives the confirmation.

### Watch out
- Only successful payments count; test (debug) payments are shown but are not included in statements.
- Shares and fees in the list are estimates with the current percentage; the final amounts are those of the statements.
- Payments of customers without a centre stay entirely with the platform.

=> /paymentdealer /paymentsettings

@@ /paymentdealer
### What you can do
The licence payments of the centre's customers and the monthly statements with which the platform pays the centre its share. The SuperUser chooses the centre; the centre's users always see their own and only confirmed statements.

### Statement states
| State | Description |
|---|---|
| Draft | Preview the SuperUser can regenerate or delete; not visible to the centre. |
| Confirmed | The payments are locked on the statement and the centre has received the email; the share is to be transferred. |
| Paid | The share has been transferred: date and reference of the transfer are on the statement. |
| Cancelled | The statement is no longer valid and its payments go back to be settled. |

### How to
- **Send a summary**: choose the month or **Whole year** and, from the round button, **Send the summary by email**; it goes to the centre's administration email with the PDF.
- **Record the centre's invoice** (DealerPowerAdmin or SuperUser): on a confirmed or paid statement choose **Invoice** and enter number and date.
- **Download the data**: from the round button **PDF** (summary of the period) or **Excel** (summary and payment details).

### Example
A company of the centre pays 100 euros on 10 September, the fee is 20%. On 5 October the SuperUser generates and confirms the September statement: centre share 80 euros. The centre receives the email; when the transfer is made the statement shows **Paid**.

### Watch out
- A payment confirmed after its month's statement was closed goes into the next statement.
- If an already paid payment is refunded, the next statement deducts it as an adjustment.

=> /paymentdealerconfig /paymentreport

@@ /paymentdealerconfig
### Fields
| Field | Description |
|---|---|
| Fee in % | From 0 to 100; empty uses the general fee. Only the SuperUser changes it, and it applies to statements generated from then on. |
| IBAN | Account receiving the transfer, letters and digits only (spaces are removed). |
| Account holder | Holder of the account. |
| Administration email | Receives statements and summaries; separate multiple addresses with ;. Empty: the centre's DealerPowerAdmins. |

=> /paymentdealer

@@ /paymentsettings
### Fields
| Field | Description |
|---|---|
| General fee in % | From 0 to 100, for centres without their own fee. |
| Automatic statements and summaries | On: every month, from the given day, the draft statements of the previous month are generated; in the given month each centre receives the summary of the previous year. |
| Statement generation day | From 1 to 28. |
| Annual summary month | Month in which the previous year's summary is sent. |

### Watch out
- Automatic drafts must always be confirmed by the SuperUser: confirming sends the email to the centre.
- Without email sending configured, summaries are not sent.

=> /paymentreport

## Glossary
- **Dealer**: the reseller that creates financial advisors and activates the companies' applications (in Italian Centro).
- **Financial advisor**: the professional firm that follows a group of companies (in Italian Studio).
- **Company**: the customer company, with its administrators and employees.
- **Node**: a single dealer, financial advisor or company in the hierarchy.
- **DEV**: the technical level of the PowerAdmin and Admin users, which receives tickets from the SuperUser.
- **Power**: the main administrator of a node (DealerPowerAdmin, FinancialAdvisorPowerAdmin, CompanyPowerAdmin).
- **Admin**: the administrator working alongside the Power, with some settings read only.
- **Main users**: the two administrators created together with the node; they cannot be deleted one by one.
- **Impersonate**: enter the application as another user, seeing their menu.
- **Application**: a module activated per company: attendance, CRM, document management.
- **Disabled**: state of a row removed from the lists without deleting it.
- **Ticket**: a support request that moves from one level to another.
- **Request type**: the category of a ticket, with the levels that can open and receive it.
- **Enabled level**: a level involved by a request type.
- **Assignee**: the user handling a ticket.
- **AI documentation**: the texts the assistant uses to answer tickets by itself.
- **Storage plan**: the maximum document management space assigned to a company.
- **Warning threshold**: the percentage of used space beyond which a warning is sent.
- **Jobs bar**: the bar at the top with the progress of long operations and the files to download.
- **Contact person**: a contact of the customer; there is only one main contact.

## Troubleshooting

### At login "Wrong password" or "User name not found" appears
Check that you type the user name exactly as it was given to you (not the email). If you do not remember the password use **Forgot your password?**; if the problem persists ask your company administrator.

### The link to reset the password does not work
The link is valid for 24 hours and only once. If you already used it or it expired, go back to **Forgot your password?** and request a new one.

### I cannot find an item in the menu
Items depend on your roles, on the applications active for your company and on the items the CompanyPowerAdmin hid in **User menus**. A hidden page cannot be opened even by typing its address. Ask your CompanyPowerAdmin.

### "Application is not active for company" appears
The application was deactivated or never activated. Only the SuperUser or the dealer's DealerPowerAdmin can activate it; the data of a deactivated application is kept and becomes visible again on reactivation.

### There are no request types in the new ticket
With the chosen role there are no active request types that move up to a higher level, or the types concern inactive applications. Try another role in **Open as** or ask the SuperUser to review the types.

### "The ticket was changed by another user"
Someone worked on the ticket while you had it open. Reload the page, read the last step in the history and repeat the action if it is still needed.

### An attachment is rejected
Each step accepts up to 5 files, each up to 10 MB and 20 MB in total; empty files and types not allowed (for example programs) are rejected. Compress the files into a ZIP or spread them over several steps.

### I cannot delete a customer
The customer is used by an application: the message tells you where. Turn off **Active** in its record: it stays in the history but is no longer offered.

### The deletion is rejected
- The name you type must match exactly the one shown in the window.
- Main (Power) administrators cannot be deleted by themselves: they go away only when their node is deleted.
- Each node must keep at least one active Power: first give the role to another user.
- You cannot delete your own user or the SuperUser.

### Documents cannot be uploaded for lack of space
The company's plan space is used up or the file exceeds the maximum size. Delete documents you no longer need or ask the dealer for a larger plan; if the message says there is no active plan, the SuperUser must create one.

### "An unexpected error occurred" appears with a code
Write down the code given in the message and send it to support by opening a ticket: it helps find the error.

### The manual does not download
The PDF is prepared in the background and appears in the jobs bar at the top with the download button. The manual of an application not active for your company is not available.
