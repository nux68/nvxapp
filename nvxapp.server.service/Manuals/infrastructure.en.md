# Basic functions manual
Access, menu, support, users, companies and shared registries

## Introduction
This manual describes the functions shared by the whole application, available whatever applications are activated for the company (attendance, CRM, document management). Each application has its own manual, downloadable from the **Support** section of the menu.

The application is organised on several levels:
- **Dealer**: manages the financial advisors and activates the applications;
- **Financial advisor**: manages the companies it follows;
- **Company**: its administrators and employees use the active applications.

> The menu items you see depend on your roles and on the applications active for your company: if a function described here does not appear, it is not meant for your role.

## Accessing the application
### First access and login
1. Open the application address: before logging in you see the public pages of the site.
2. Choose **Login** and enter your user name (or email) and password.
3. After logging in the **Dashboard** opens, with the assistant and the side menu.

If you forgot your password, use **Forgot password** on the login page: you will receive an email with a link to set a new one.

### User data and password change
From the **Access** section of the menu open **User data** to check your details and change your password.

### Logout
Use **Logout** in the **Access** section. If you are impersonating another user, instead of Logout you see the path of the impersonated users (see below).

## The menu
The side menu is divided into expandable sections:
- **Access**: dashboard, user data, logout;
- **Administration**: shared functions (users, companies, registries, configuration);
- one section for each **active application** (Attendance, CRM, Documents), with its subsections;
- **Support**: tickets, documentation and manuals.

After logging in, the first available application opens with its first subsection. Sections and subsections open and close one at a time. Next to the sections you see the counters of the things to do (for example the tickets assigned to you).

## Users and roles
### Roles
- **CompanyPowerAdmin**: main administrator of the company; configures the applications and the users' menu.
- **CompanyAdmin**: company administrator.
- **User**: employee or collaborator using the applications.

A user can have several roles in the same company (for example CompanyAdmin and User).

### Managing the company users
1. Open **Administration > General > Users**.
2. Use the **+** button to add a user and enter email, user name and roles.
3. With **Edit** you change roles and data; with **Impersonate** you enter the application as that user (see below).

In the list options you find **Show disabled** to also see deactivated users.

### Impersonating a user
Administrators can enter the application "as" another user to check what they see or to help them. The path of the impersonated users appears at the top: clicking a name takes you back to that user.

> Operations performed while impersonating a user are recorded with the user who logged in.

## Users' menu
The CompanyPowerAdmin can hide from CompanyAdmins and Users the menu items they must not use: **Administration > Parameters > Users' menu**. System items (login, dashboard, user data) cannot be hidden.

## Shared registries
### Customers
The company's customers are a single registry used by all applications (attendance jobs, CRM opportunities, customer file in the document management). They are managed in **Administration > Registry > Customers**.

1. With **+** you create a customer: company name, type (customer, supplier or both), tax data, address and contacts.
2. After the first save you can add the **contact persons** (one can be the main contact).
3. A **deactivated** customer stays in the history but is no longer offered in selections; it is shown with **Show disabled**.

> A customer used by an application (jobs, opportunities, offers, documents) cannot be deleted: the message tells you where it is used. In that case deactivate it.

## Support
### Tickets
From **Support > Tickets** you open a support request, follow the answers and attach files. The counter next to the item shows the tickets assigned to you.

### Manuals
From **Support** you download the PDF manuals: this one and those of the active applications. The PDF is prepared in a few seconds and appears in the jobs bar at the top of the page, with the button to download it.

## Background jobs
Long operations (exports, reports, manuals) run in the background: their progress appears in the jobs bar at the top of the page. When a file is ready, click the download button; you can close the notification when you no longer need it.

## Applications and storage
The applications of a company are activated by the dealer (or the SuperUser). The document management uses disk space in packages: usage and package are shown on the **Storage** page of the document management; when the space is about to run out the administrators receive a warning.
