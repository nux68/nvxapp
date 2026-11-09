# CRM manual
Customers, leads, deals, activities and offers

## Introduction
The CRM helps you follow the relationship with the company's customers and prospects: registries and contact persons, deals (opportunities), sales activities (calls, emails, meetings), offers, notes and attachments, with reports on the results.

This manual is built on **your menu**: you find a chapter for every item of the **CRM** section you can see, describing what you can do on that page. The CRM is available when it is active for your company: it is activated by the dealer.

### Who does what
- **Sales rep (User role)**: works on the customers they can see (see "Visibility"), logs activities, follows leads and opportunities, prepares and sends offers.
- **CompanyAdmin**: sees all customers and all deals, assigns owners, reads the reports, reassigns a sales rep's data.
- **CompanyPowerAdmin**: everything the CompanyAdmin does, plus the CRM configuration (tables and settings).

> A user can have several roles: for example a CompanyAdmin who is also a sales rep sees both the administration and the sales rep items.

### Visibility
The company chooses what sales reps see (from **CRM settings**):
- **Only their own customers** (default): the sales rep sees the customers they own, those without an owner ("shared") and those they are involved in, i.e. with an opportunity they own or an activity assigned to them. They see their own leads and those without an owner.
- **All customers**: every sales rep sees everything.

Administrators always see everything. The rules apply everywhere: lists, cards, pipeline, agenda, offers and reports.

### Notes and attachments
Many cards (customers, opportunities, leads, offers) have **Notes** and **Attachments** sections.
- **Notes**: write the text and press **Add**; a note can be **pinned** at the top. It can be edited or deleted by the author or an administrator.
- **Attachments**: press **Upload a file** (maximum 20 MB); from each row you download or delete the file. It can be deleted by whoever uploaded it or by an administrator.

@@ /crmcustomerlist
The CRM customers are the company's shared customer registry, the same used by attendance and document management: the CRM adds its sales data.

In the list you see the visible customers with status, owner, city, open opportunities and their value, next activity. At the top you can filter by **owner**, **status** and **tag**; the search field searches the text. In **Options** you find **Show disabled**. From the **+** button you find **New customer** and **Export to Excel**.

### Creating a customer
1. Press **+** and choose **New customer**.
2. Fill in the company name and the other data: registry type, sales status, owner, source, tags, tax data and contacts.
3. Confirm: the customer stays open and the Contacts, Opportunities, Activities, Offers, Notes and Attachments sections appear.

> The sales rep who creates a customer becomes its owner. A sales rep can take a customer without an owner or leave one of their own; assigning a customer to another user is up to the administrators.

### Sales status
- **Prospect**: potential customer (default).
- **Customer**: acquired customer.
- **Former customer**.
- **Not interested**.

The **Consent to marketing communications** box records the customer's consent with the date it was given; clearing it deletes the date.

### The customer card
- **Details**: registry and sales data (saved with **Confirm**).
- **Contacts**: the contact persons; a new contact is added with **+** › **Contact**: the window opens with surname, first name, role, phone, mobile, email, notes and the "Main contact" box (only one per customer).
- **Opportunities**, **Activities**, **Offers**: the customer's items.

To add something to a saved customer press **+** and choose from the menu that opens from the bottom: **Contact**, **Opportunity**, **Activity** or **Offer**; the item is created already linked to the customer.
- **Notes** and **Attachments**: as described in the introduction.

> Customers are not deleted from the CRM: they are deactivated (**Active** field). Permanent deletion is done from Administration and is blocked, with a message, if the customer is used by opportunities, offers, activities or attachments.

### Exporting to Excel
From the **+** button choose **Export to Excel**: you download the list with the chosen filters, including status, owner, city, email, phone, tags, open opportunities and value, last and next activity.

@@ /crmleadlist
A lead is a contact not yet qualified, for example coming from the website or a trade fair. The list shows the open leads; in **Options** you find **Also show converted and discarded**.

### Managing a lead
1. Press **+** and choose **New lead**.
2. Enter surname, first name, role, company, email, phone, source and status (New, Contacted, Qualified, Discarded).
3. Set owner, marketing consent and notes, then save.
4. After saving, add **activities** and **notes** to the lead.

### Converting a lead into a customer
1. Open the lead and press **Convert**.
2. Choose whether to create a **new customer** with the lead's data or link it to an **existing customer**.
3. If you want, leave **Also create an opportunity** selected, with title and value.
4. Confirm: the customer card opens.

> With the conversion the lead's person becomes a contact person of the customer, the lead's activities and notes move to the customer and the lead becomes **Converted**.

@@ /crmpipeline
An opportunity is a deal with a customer: title, value, probability, phase and expected close date. The pipeline shows the opportunities in columns, one per phase: first the open phases, then the won and lost ones (of the last 30 days). Each column shows the count and the total value. You can filter by **owner** and switch to the **List** view.

### Moving an opportunity
1. Drag a card to another column to change its phase.
2. On a smartphone use the arrows button on the card.
3. Moving it to a **lost** phase prompts you for the loss reason.
4. Click a card to open the opportunity.

### The opportunity card
- **Phase**: the probability is proposed from the phase (you can change it).
- **Expected close** and, for won or lost phases, the **close date** (today is proposed).
- **Loss reason**: in lost phases.
- **Owner**, **source**, the customer's **contact person** and **notes**.

The **Activities**, **Offers**, **Notes**, **Attachments** and **History** sections complete the card. The **history** records every change of phase and value, with who made it and when: it is the basis of the reports.

> Deleting an opportunity also deletes its history, activities and notes; offers remain unlinked. If it has attachments, delete them first.

@@ /crmactivitylist
The agenda collects the activities to do and the completed ones. It is the first item of the sales reps' menu and is divided into sections:
- **Today**: today's activities and the to-do ones already started.
- **Overdue**: to-do activities whose start has passed; the red number shows how many.
- **Upcoming**: to-do activities of the following days.
- **Done / cancelled**: the history.

Administrators have the **Only mine** switch to also see the activities of others.

### Creating an activity
1. Press **+** and choose **New activity**.
2. Choose the **type** (call, email, meeting…): the end is proposed with the type's duration.
3. Enter **subject**, **start**, **end** and who it is **assigned to** (normally you).
4. Link the activity to a **customer** (and, if you want, to a contact person and an open opportunity) or to a **lead**.
5. Confirm.

> An activity must be linked to a customer or a lead. You can also add it from the card of a customer, an opportunity or a lead. A sales rep can assign activities to themselves or hand over their own activities; other assignee changes are made by administrators.

### Marking an activity as done
1. In the activity row press **Done**.
2. Write the outcome (for example "call back at the end of the month") and confirm.

### Reminders
Before a to-do activity starts, the assignee receives a pop-up notice with the **Open** button. The notice time (default 30 minutes) is set in CRM settings. The notice only arrives if you are connected to the application; the activity stays in the agenda anyway.

@@ /crmofferlist
The list shows the visible offers with number, customer, subject, date, status and total; from each row you download the **PDF**.

### Preparing an offer
1. Press **+** and choose **New offer**.
2. Choose the **customer**, the **contact person** and, if any, the **opportunity**.
3. Check **date** and **valid until** and write the **subject**.
4. Add the **lines**: choosing a **product** fills in description, price, unit of measure and VAT; you can change quantity, price, discount % and VAT %.
5. Press **Confirm**: the offer gets its number (per year, with the prefix if set) and stays open.

The totals update as you type. You can also add the offer from the customer or opportunity card.

### PDF and sending
- **PDF** downloads the offer with the company and customer data, the lines, the totals, the notes and the configured terms.
- **Send** opens the sending window: the email is proposed from the contact person or the customer; the message is optional (empty = default text). The PDF is attached and the offer becomes **Sent**, with the sending date.

### Offer statuses
Draft, Sent, Accepted, Rejected: the status is changed in the offer header. Only **draft** offers without attachments can be deleted.

> If email sending is not configured a message appears: in that case download the PDF and send it with your own mail.

@@ /crmreport
The reports show, for the chosen period (by default from the start of the year), the sales trend:
- **won** opportunities (with their value) and **lost** ones in the period, with the win rate;
- **leads** created and converted, with the conversion percentage;
- **open pipeline by phase** and **by owner**, with value and weighted value (value × probability);
- **won by owner** and **lost by reason**;
- **activities done per user**.

> Reports only count the data the user can see: a sales rep sees their own numbers, administrators see everything.

@@ /crmparlist
Here you collect the CRM tables, created at activation with default values and reused across all the cards:
- **Phases**: description, type (open, won, lost), proposed probability and order in the pipeline;
- **Activity types**: icon, colour and proposed duration;
- **Loss reasons** and **Sources**;
- **Products**: code, description, price, unit of measure and VAT, proposed in the offer lines;
- **Tags**: to group customers, with a colour.

> A value already in use cannot be deleted: deactivate it, it stays in the history but is no longer offered. At least one active phase of each type and at least one activity type must remain.

@@ /crmsettings
Here you set the CRM's general behaviour for the company:
- **sales rep visibility** (only their own customers / all);
- **currency** (3-letter code, for example EUR);
- **reminder notice** in minutes (0 = no reminder);
- for offers: **number prefix**, **default validity** in days and **terms** printed at the bottom of the PDF.

@@ /crmreassign
This function moves a sales rep's data to another user in one go, for example when they leave the company.
1. Choose the **From** user.
2. Choose the **To** user (or "Nobody": the data stays without an owner).
3. Choose what to reassign: customers, opportunities, leads, activities to do.
4. Press **Reassign**: the number of reassigned items is shown.

> Activities to do are reassigned only when a recipient is chosen. When a user is removed from the company their customers, opportunities and leads stay without an owner and their to-do activities are cancelled.

## Frequently asked questions
### Why can't I see a customer or an opportunity?
It depends on the **visibility** set by the company. With "Only their own customers" you see those you own, those without an owner and those you are involved in. Administrators always see everything.

### How do I change a customer's owner?
A sales rep can take a customer without an owner or leave one of their own. Assigning a customer to another user, or moving a sales rep's data in bulk, is up to the administrators (Reassignment).

### I can't delete a customer: why?
Customers are not deleted from the CRM, they are deactivated. Permanent deletion is blocked if the customer is used by opportunities, offers, activities or attachments.

### The offer won't send by email.
If email sending is not configured a message appears: download the offer PDF and send it with your own mail.
