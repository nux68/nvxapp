# Basic functions manual
Access, menu, users, companies, registries and support

## Introduction
This manual describes the functions shared by the whole application. It is built on **your menu**: one chapter for each section you see and one paragraph for each item, with a screenshot of the page. The applications active for your company (attendance, CRM, document management) each have their own manual, downloadable from the **Support** section.

The application is organised on several levels:
- **Dealer**: manages the financial advisors and activates the applications;
- **Financial advisor**: follows the companies;
- **Company**: its administrators and employees use the active applications.

### The menu
The side menu is divided into expandable sections: **Access**, **Administration**, one section for each active application and **Support**. After logging in, the first available application opens. Sections and subsections open and close one at a time; next to the sections you see the counters of things to do (for example the tickets assigned to you).

### Background jobs
Long operations (exports, reports, manuals) run in the background: progress appears in the **jobs bar** at the top of the page. When a file is ready press its button to download it; you can close the notification when you no longer need it.

### Impersonating a user {SuperUser, PowerAdmin, Admin, DealerPowerAdmin, DealerAdmin, FinancialAdvisorPowerAdmin, FinancialAdvisorAdmin, CompanyPowerAdmin, CompanyAdmin}
From the lists of users, dealers, financial advisors and companies the **Impersonate** button lets you enter the application "as" another user, to see what they see or to help them. The path of the impersonated users appears at the top: clicking a name takes you back to that user.
> Operations performed while impersonating are recorded with the user who logged in.

### Company roles {CompanyPowerAdmin, CompanyAdmin}
- **CompanyPowerAdmin**: main administrator; configures the applications and the users' menu.
- **CompanyAdmin**: company administrator.
- **User**: employee or collaborator.

A user can have several roles in the same company (for example CompanyAdmin and User).

@@ /dashboard
The home page after logging in. In the **Ask the assistant** box you can write (or dictate) what you want to do: the assistant opens the right page or performs the operation. Otherwise choose an item from the menu.

@@ /user
Your data: user name, email and language. Here you also change your **password**.
1. Open the item and check your data.
2. To change the password enter the current one and the new one, then confirm.
> If you forgot your password use **Forgot password** on the login page: you will receive an email with a link to set a new one.

@@ /companypoweradmin
The page reserved to the company's CompanyPowerAdmin, with information about the company and the active applications.

@@ /companyadmin
The CompanyAdmin page, with information about the company you administer.

@@ /usercompanylist
The company users, with their roles.
1. With **+** you add a user: email, user name and roles.
2. With **Edit** you change roles and data; with **Impersonate** you enter the application as that user.
3. In the list **Options** enable **Show disabled** to also see deactivated users and, if allowed, **Enable deletions**.
> Deletion first shows a summary of what will be removed and asks you to type the user name to confirm.

@@ /customerlist
The company's customers: a single registry used by all applications (attendance jobs, CRM opportunities, customer file in the document management).
1. With **+** you create a customer: company name, type (customer, supplier or both), tax data, address and contacts.
2. After the first save add the **contact persons**; one can be the main contact.
3. A **deactivated** customer stays in the history but is no longer offered in selections: you see it with **Show disabled**.
> A customer used by an application cannot be deleted: the message tells you where it is used. In that case deactivate it.

@@ /companymenurestriction
Here the CompanyPowerAdmin hides from CompanyAdmins and Users the menu items they must not use. System items (login, dashboard, user data) cannot be hidden. Hidden items also disappear from the manuals of those who do not see them.

@@ /superuser
The SuperUser page, with general information about the installation.

@@ /dealerlist
The dealers. With **+** you create a new one with its first administrator; with **Edit** you change its data; with **Impersonate** you enter the dealer as its administrator.

@@ /userlist
All the users of the application, with their links to dealers, financial advisors and companies. From here the SuperUser can delete a user with all their links.

@@ /companymove
Moves a company from one financial advisor to another, with its users and data.

@@ /storageplanlist
The storage packages of the document management (for example 1, 5 and 20 GB), with the default package assigned to new companies.

@@ /companystoragelist
The document management space used by each company: package, used space and warnings. From here you assign a different package to a company.

@@ /poweradmin
The page of the installation PowerAdmin.

@@ /admin
The page of the installation administrators.

@@ /dealerpoweradmin
The DealerPowerAdmin page, with information about the dealer.

@@ /dealeradmin
The DealerAdmin page, with information about the dealer.

@@ /userdealerlist
The administrators of the dealer: add, edit, deactivate or impersonate.

@@ /financialadvisorlist
The financial advisors followed by the dealer.
1. With **+** you create an advisor with its first administrator.
2. In the advisor's card you enable the **applications** it can use.
3. With **Impersonate** you enter the advisor to work on its companies.
> The applications of a company (attendance, CRM, document management) can only be activated by the SuperUser and the dealer's DealerPowerAdmin, from the company card.

@@ /financialadvisorpoweradmin
The FinancialAdvisorPowerAdmin page, with information about the advisor.

@@ /financialadvisoradmin
The FinancialAdvisorAdmin page, with information about the advisor.

@@ /userfinancialadvisorlist
The administrators of the financial advisor: add, edit, deactivate or impersonate.

@@ /companylist
The companies followed by the advisor.
1. With **+** you create a company with its first administrator: a guided procedure collects the basic data.
2. With **Edit** you open the company card (data and active applications).
3. With **Impersonate** you enter the company to work on its behalf.

@@ /ticketlist
Support requests (tickets). With **+** you open a new one choosing the request type, describe the problem and attach files if needed. In the detail you follow the answers, reply or close the ticket. The counter next to the item shows the tickets assigned to you.

@@ /tickettypelist
The ticket request types, with the support levels that handle them.

@@ /licensemanagement
The licences of your company (or firm) for Attendance, CRM and Documents: for each function the level (Free, Standard or Pro), whether it is yours or the firm's, the expiry date and the maximum number of users. The PowerAdmin subscribes, changes plan, renews and turns on automatic renewal; the payment history is here too. A firm sees the companies covered by its licences.

@@ /licensecataloglist
The licence price lists: for each subject type (company or firm) and function, the three plans Free, Standard and Pro with monthly and yearly prices and maximum users. The SuperUser manages the global price lists and the settings; an enabled centre can define its own price lists.

@@ /aidocumentlist
The documentation the AI assistant uses to answer tickets: uploaded documents linked to the request types.

@@ /tickettypeaisetting
For each request type choose whether and how the AI assistant proposes an answer to tickets.
