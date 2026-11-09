## Getting started
This guide adds to the page descriptions the fields, step-by-step procedures, examples and the messages you may come across. The parts below are shown according to your roles.

### If you are a sales rep {User}
Your starting page is the **Agenda**: it lists today's activities, the overdue ones and the upcoming ones.
1. Open **Agenda** and check the **Overdue** activities (the red number).
2. After each call or meeting press **Done** and write the outcome.
3. Schedule the next activity right away with **+** and **New activity**.
4. When a new contact comes in, create it in **Leads**; when they are interested, press **Convert**.
5. Follow your deals in the **Pipeline** and prepare offers from **Offers** or from the customer card.

> Whatever you create without choosing an owner becomes yours: new customers, leads and opportunities have you as owner, new activities are assigned to you.

### If you are an administrator {CompanyAdmin, CompanyPowerAdmin}
You always see all customers, deals and activities, whatever visibility is chosen for the sales reps.
- In the **Agenda** switch off **Only mine** to see the other users' activities too.
- In the **Pipeline** filter by **owner** to check each person's work.
- In **Reports** compare won, lost and pipeline by owner.
- With **Reassignment** you move all of a sales rep's data at once.

> A customer, lead or opportunity created by an administrator without an owner stays **without an owner** (shared): assign it if needed.

### Initial setup {CompanyPowerAdmin}
The CRM starts with ready-made phases, activity types, loss reasons and sources. Before the sales reps start working:
1. Open **Settings** and choose the sales rep visibility, the currency and the reminder notice.
2. On the same page set the prefix, default validity and terms of the offers.
3. Open **Tables** and adapt the pipeline **Phases** with their probability.
4. Enter the **Products** with price, unit of measure and VAT, if you quote from a price list.
5. Create the **Tags** to group customers and check **Sources** and **Loss reasons**.

## Key concepts
### The sales path
A new contact comes in as a **lead**; once qualified it is **converted** into a **customer**. With the customer you open an **opportunity** (the deal), prepare an **offer** and, in the end, the opportunity is closed as **won** or **lost**. **Activities** (calls, emails, meetings) go along the whole path.

| Item | What it is | Example |
|---|---|---|
| Lead | Contact not yet qualified | Mario Rossi, met at a trade fair |
| Customer | Company of the shared registry, with the CRM sales data | Rossi Srl |
| Opportunity | Deal with value, probability and phase | Software supply, 15,000 EUR, 50% |
| Activity | Something to do or already done | Call Rossi back on Thursday at 10 |
| Offer | Quote with lines, totals, PDF and sending | Offer 14/2026 |

### Phases and probability
Each phase has a **type**: **Open**, **Won** or **Lost**. The default phases are:

| Phase | Type | Suggested probability |
|---|---|---|
| New | Open | 10% |
| Qualified | Open | 25% |
| Proposal | Open | 50% |
| Negotiation | Open | 75% |
| Won | Won | 100% |
| Lost | Lost | 0% |

When you choose a phase the probability is suggested from the phase; you can change it. Moving the opportunity to a won phase sets the probability to 100%, to a lost phase sets it to 0%. Won and lost phases close the opportunity and set its close date.

### Weighted value
The **weighted value** is the opportunity value multiplied by its probability: 10,000 EUR at 25% weighs 2,500 EUR. Added up over all open opportunities it gives a realistic forecast of what will come in; you find it in the open pipeline reports.

### Owner and visibility
Customers, leads and opportunities have an **owner**; activities have an **assignee**. With the **Only their own customers** visibility a sales rep sees the customers they own, those without an owner and those they are involved in; they see the opportunities, activities and offers of those customers. An item you cannot see gives the message "not found or not visible".
- A sales rep can take a customer, lead or opportunity **without an owner** or release one of their own.
- A sales rep can assign an activity to themselves or hand their own activities to others.
- Any other change of owner is up to the administrators: otherwise the message "Only company administrators can assign to others." appears.

### Reminders
Before an activity **To do** starts, the assignee gets a pop-up notice, with the notice set in Settings (default 30 minutes). If you change the start or the assignee of the activity the reminder is prepared again.

### Deactivate instead of deleting
Customers and table values already in use are not deleted: they are deactivated. A deactivated item stays in the history and in the documents that use it, but is no longer offered in selections.

@@ /crmcustomerlist
### Fields
**Registry and sales data** section:

| Field | Description |
|---|---|
| Company name | Required, maximum 100 characters. |
| Registry type | Customer, Supplier, Customer and supplier. Suppliers are not offered in opportunities and offers. |
| Status | Sales status: Prospect (default), Customer, Former customer, Not interested. |
| Active | When switched off the customer is no longer offered in selections but stays in the history. |
| Owner | The sales rep following the customer, or None (shared customer). For a sales rep their own name is suggested. |
| Source | Where the customer comes from (Sources table). |
| Tags | One or more tags to group customers; also used as a filter in the list. |
| Industry | Free text, maximum 100 characters. |
| Website | Maximum 200 characters. |
| Estimated revenue | Number, not negative. |
| Employees | Number, not negative. |
| Consent to marketing communications | Records the consent with its date (Consent given on). |

**Tax data and contacts** section:

| Field | Description |
|---|---|
| VAT number | 11 digits. |
| Tax code | 16 alphanumeric characters or 11 digits. |
| Recipient code (SDI) | 6 or 7 alphanumeric characters. |
| Address, Postcode, City | Maximum 100, 10 and 50 characters. |
| Province, Country | 2-letter code (for example MI, IT). |
| Phone | Maximum 30 characters. |
| Email, PEC | Valid email addresses, maximum 100 characters. The email is suggested as offer recipient when the contact person has none. |
| Contact person | Free text, maximum 100 characters. |
| Notes | Maximum 500 characters. |

**Contact person** dialog (customer contacts):

| Field | Description |
|---|---|
| Surname, First name | At least one of them is required; maximum 100 characters each. |
| Role | For example Purchasing office; maximum 100 characters. |
| Phone, Mobile | Maximum 30 characters; the list shows the mobile number, if any. |
| Email | Valid email, maximum 100 characters; suggested as offer recipient. |
| Main contact | Only one per customer: it is highlighted in the contact list. |
| Notes | Maximum 500 characters. |

### How to
Add contacts, opportunities, activities and offers from the customer card:
1. Open the customer from the list (the customer must already be saved).
2. Press the **+** button at the bottom and choose **Add to customer**.
3. From the menu that opens from the bottom choose **Contact**, **Opportunity**, **Activity** or **Offer**.
4. Fill in the dialog or page that opens: the customer is already set.
5. Confirm: you return to the card, with the new item in the matching section.

Edit a contact:
1. Open the **Contacts** section of the card.
2. On the contact row press **Edit** (or **Delete** to remove it).
3. Correct the data and confirm.

### Example
The front office receives a request from Bianchi Arredi Srl. Laura, a sales rep, creates the customer with status Prospect, source Website and tag Furniture; after confirming, the card stays open. With **+** she adds the contact Paolo Bianchi (Purchasing office, main contact), then a "First call" activity for tomorrow at 10 and an opportunity "Head office furniture" worth 8,000 EUR.

### Watch out
- Contacts, opportunities, activities, offers, notes and attachments can be added only after the customer has been saved the first time.
- On an existing customer **Confirm** saves and returns to the list.
- "The company name is required.": fill in the company name.
- "Only company administrators can assign to others.": a sales rep can only take a customer without an owner or release one of their own.
- "Customer not found or not visible.": the customer is outside your visibility (for example it was assigned to another sales rep).
- The Excel export contains only the customers you can see, with the active filters.

=> /crmleadlist /crmpipeline /crmactivitylist /crmofferlist

@@ /crmleadlist
### Fields
| Field | Description |
|---|---|
| Surname, First name, Role | Maximum 100 characters each. |
| Company | Maximum 100 characters; on conversion it becomes the company name of the new customer. |
| Email | Valid email, maximum 100 characters. |
| Phone | Maximum 30 characters. |
| Status | New (default), Contacted, Qualified, Discarded; Converted is set by the conversion. |
| Source | Where the lead comes from (Sources table). |
| Owner | For a sales rep their own name is suggested; None = shared lead. |
| Consent to marketing communications | Moves to the customer on conversion. |
| Notes | Maximum 2000 characters. |

**Convert the lead into a customer** dialog:

| Field | Description |
|---|---|
| Customer | New customer from the lead data (default) or an existing active customer. |
| Also create an opportunity | Selected by default; the opportunity starts in the first open phase. |
| Title | Suggested from the lead's company or name; maximum 200 characters. |
| Value | Value of the new opportunity, not negative. |

### How to
Log a call to a lead:
1. Open the lead and go to the **Activities** section.
2. Press **Add**: the activity is already linked to the lead.
3. Choose the Call type, write the subject and confirm.

### Example
At a trade fair Marco collects the business card of Giulia Verdi, purchasing manager at Verdi Impianti. He creates the lead with source Trade fair / event and status New, then a call for Monday. After the call he marks the activity as done and moves the lead to Qualified; when Giulia asks for a quote he presses **Convert**, chooses **New customer from the lead data** and creates the opportunity "Warehouse system" worth 12,000 EUR: the card of the new customer Verdi Impianti opens, with Giulia as main contact.

### Watch out
- "Enter the lead's name or company.": at least surname, first name or company is needed.
- The **Convert** button appears only on a saved lead not yet converted; "The lead has already been converted into a customer." if it already was.
- When converting into a new customer: company name = the lead's company (or name), status Prospect, same owner, source and consent as the lead.
- The lead's person becomes the main contact only if the customer had no contacts yet.
- A converted lead can still be viewed with **Also show converted and discarded** and shows the customer it was converted into.

=> /crmcustomerlist /crmpipeline /crmactivitylist

@@ /crmpipeline
### Fields
| Field | Description |
|---|---|
| Title | Required, maximum 200 characters. |
| Customer | Required; active customers that are not suppliers only are offered. |
| Contact | Contact person of the chosen customer (optional). |
| Phase | Required; a new opportunity starts from the first open phase. |
| Value | Amount in the CRM currency, not negative. |
| Probability % | From 0 to 100; suggested by the phase. |
| Expected close | Date on which you expect to close the deal. |
| Close date | Only in won or lost phases; if empty, today's date is set. |
| Loss reason | Only in lost phases (Loss reasons table). |
| Owner | For a sales rep their own name is suggested. |
| Source | Where the deal comes from. |
| Notes | Maximum 2000 characters. |

### How to
Create an opportunity:
1. Press **+** and choose **New opportunity** (or use the **+** of the customer card).
2. Write the title, choose the customer and, if you like, the contact.
3. Check phase, value and probability and enter the expected close.
4. Confirm: the card appears in the phase column.

Close a won deal:
1. Drag the card to the won phase column (on a smartphone use the arrows button and choose **Change phase**).
2. Open the opportunity and check the **Close date**.
3. Set the status of the linked offer to Accepted.

### Example
The opportunity "Head office furniture" (8,000 EUR) is in Proposal at 50%: it weighs 4,000 EUR. The customer asks for a discount and Laura moves it to Negotiation: the suggested probability becomes 75% and the history records the change. Two weeks later the customer picks a competitor: Laura drags the card to Lost and chooses the reason "Chose a competitor"; the loss shows up in the Lost by reason report.

### Watch out
- "The opportunity title is required.": fill in the title.
- "Invalid phase.": the chosen phase no longer exists; reload the page and choose another one.
- "The opportunity has attachments: delete them first.": to delete the opportunity, remove its attachments first.
- In the board view won and lost opportunities are only those of the last 30 days; the others are found from the customer card.
- The history records a change only when the phase or the value changes.

=> /crmcustomerlist /crmofferlist /crmreport /crmactivitylist

@@ /crmactivitylist
### Fields
| Field | Description |
|---|---|
| Type | Required (Activity types table); the first active type is suggested. |
| Subject | Required, maximum 200 characters. |
| Start | Required, date and time; the next full hour is suggested. |
| End | Optional; suggested from the type duration; it cannot be before the start. |
| Assigned to | Who has to carry it out; the logged-in user is suggested. |
| Customer | The activity's customer (active customers). |
| Contact | Contact person of the customer. |
| Opportunity | One of the customer's open opportunities. |
| Description | Maximum 4000 characters. |
| Status | To do (default), Done, Cancelled. |
| Outcome | How it went; maximum 2000 characters. |

Default activity types:

| Type | Suggested duration |
|---|---|
| Call | 15 minutes |
| Email | 10 minutes |
| Meeting | 60 minutes |
| Task | 30 minutes |

### How to
Cancel or move an activity:
1. Open the activity from its row with **Edit**.
2. To move it change **Start** and **End**; to cancel it set the **Status** to Cancelled.
3. Confirm: a cancelled activity moves to **Done / cancelled** and generates no reminder.

Hand an activity to a colleague:
1. Open the activity and change **Assigned to**.
2. Confirm: the activity appears in the colleague's agenda, and they will get the reminder.

### Example
On Monday at 9 Marco opens the agenda: under **Overdue** there is "Call Verdi Impianti back" from Friday. He calls, presses **Done** and writes the outcome "Send offer by Wednesday". Right after, he creates a Task activity "Prepare Verdi offer" for Tuesday at 15:00, linked to the customer and to the opportunity "Warehouse system"; on Tuesday at 14:30 he gets the reminder.

### Watch out
- "Link the activity to a customer or a lead.": choose the customer (a lead activity is created from the lead card).
- "The end is before the start.": correct the dates.
- "The activity subject is required." and "Invalid activity type." are solved by filling in subject and type.
- "Only company administrators can assign to others.": a sales rep cannot reassign another user's activity.
- The agenda shows activities from the last 60 days to the next 180: older or later ones are found from the customer, lead or opportunity card.
- A lead activity moves to the customer when the lead is converted.

=> /crmcustomerlist /crmleadlist /crmsettings

@@ /crmofferlist
### Fields
Header:

| Field | Description |
|---|---|
| Customer | Required; active customers that are not suppliers only. |
| Contact | Contact person: their email is suggested for sending. |
| Opportunity | Optional; it must belong to the chosen customer. |
| Date | Required; today is suggested. Its year determines the numbering. |
| Valid until | If empty it is calculated with the default validity from Settings. |
| Status | Draft, Sent, Accepted, Rejected. |
| Subject | Maximum 200 characters. |
| Notes | Maximum 4000 characters; printed in the PDF. |

Lines:

| Field | Description |
|---|---|
| Product | Optional; fills description, price, UoM and VAT from the price list. |
| Description | Required for each line; maximum 500 characters. |
| Qty | 1 is suggested. |
| Price | Unit price. |
| Discount % | From 0 to 100. |
| VAT % | 22 is suggested. |

Line amount = quantity × price × (1 - discount/100), rounded to the cent. The taxable amount is the sum of the line amounts, VAT is calculated line by line, the total is taxable plus VAT.

**Send the offer** dialog:

| Field | Description |
|---|---|
| Recipient email | Required and valid; suggested from the contact or the customer. |
| Message | Optional, maximum 4000 characters; empty = default text. |

### How to
Add or remove lines:
1. Press **Add line** below the lines.
2. Choose a product or type the description directly.
3. To remove a line press the bin at the end of the line.
4. Press **Confirm** to save: the totals are recalculated.

Record the outcome of an offer:
1. Open the sent offer.
2. Set the **Status** to Accepted or Rejected and press **Confirm**.
3. Also update the phase of the linked opportunity in the pipeline.

### Example
Laura prepares the offer for Bianchi Arredi: she chooses the customer, the contact Paolo Bianchi and the opportunity "Head office furniture". She adds 10 desks at 450 EUR with a 10% discount (4,050 EUR) and assembly at 300 EUR: taxable 4,350 EUR, VAT 22% 957 EUR, total 5,307 EUR. With **Confirm** the offer gets its number (with prefix OF it becomes for example OF14/2026); with **Send** it goes to Paolo's address and the status becomes Sent.

### Watch out
- "The offer must have at least one line." and "Each line must have a description.": complete the lines before confirming.
- **PDF** and **Send** appear only after the first save.
- "Sending email is not configured.": download the PDF and send it with your own mail. "Invalid email address.": correct the recipient.
- "Only draft offers can be deleted." and "The offer has attachments: delete them first.": a sent offer is not deleted, it is set to Rejected.
- The numbering restarts from 1 every new year.

=> /crmcustomerlist /crmpipeline /crmsettings /crmparlist

@@ /crmreport
### How to
Read the results of a period:
1. Set **From** and **To**.
2. Press **Refresh**.
3. At the top read won, lost, win rate and lead conversion; below you find the tables by phase, owner, reason and user.

### Example
At the end of the quarter the administrator sets 1 July to 30 September: 12 won and 8 lost give a 60% win rate. In **Lost by reason** "Price" prevails: they decide to review the price lists. In **Open pipeline by owner** they see that Marco has 40,000 EUR open but only 12,000 EUR weighted: his deals are still in the early phases.

### Watch out
- "The period is not valid.": the To date cannot be before the From date.
- Won and lost are the opportunities whose close date falls in the period; converted leads are those created in the period and already converted.
- The win rate is won divided by (won + lost) in the period.
- The open pipeline does not depend on the period: it shows the opportunities open right now.
- Opportunities without an owner appear as "(no owner)", losses without a reason as "(no reason given)".

=> /crmpipeline /crmactivitylist /crmleadlist

@@ /crmparlist
### Fields
| Field | Description |
|---|---|
| Description | Required, maximum 100 characters (all tables). |
| Phase type | Phases only: Open, Won, Lost. |
| Probability % | Phases only: from 0 to 100, suggested in opportunities. |
| Icon (Ionicons name) | Activity types only: icon name, for example call, mail, people. |
| Duration (minutes) | Activity types only: used to suggest the end (default 30). |
| Colour | Activity types and Tags: primary, secondary, tertiary, success, warning, danger, medium. |
| Code | Products only: maximum 30 characters. |
| Price, UoM, VAT % | Products only: suggested in offer lines; UoM maximum 10 characters, VAT 22 suggested. |
| Order | Position in selections and, for phases, in the pipeline. |
| Active | A deactivated value is no longer offered. |

### How to
Add a value:
1. Choose the table (Phases, Activity types, Loss reasons, Sources, Products, Tags).
2. Press **+** and **New value**.
3. Fill in the fields and confirm.

### Example
A company wants a "Demo done" phase between Qualified and Proposal: it creates the phase with type Open, probability 40% and order 25 (the default phases have order 10, 20, 30...). From then on the pipeline shows the new column.

### Watch out
- "The value is in use: deactivate it instead of deleting it.": deactivate it with **Active**.
- "At least one active phase of each type (open, won, lost) must remain.": this also applies when changing the type of a phase.
- "At least one active activity type must remain.".
- "The description is required.": fill in the description.

=> /crmpipeline /crmofferlist /crmsettings

@@ /crmsettings
### Fields
| Field | Description |
|---|---|
| Sales reps (User role) see | Only their own customers (default) or All customers. |
| Currency (3-letter code) | Required; default EUR. Shown next to opportunity and offer values. |
| Reminder notice (minutes) | Default 30; 0 = no reminder; maximum one week. |
| Number prefix | Maximum 10 characters, placed before the offer number (for example OF14/2026). |
| Default validity (days) | Default 30; used when Valid until is empty; 0 = no suggested expiry. |
| Terms at the bottom of the PDF | Maximum 2000 characters: payment, delivery and warranty terms. |

### Example
A small company with three sales reps who cover for each other chooses **All customers**, a 15-minute notice, prefix OF, 60 days validity and as terms "Payment 30 days from invoice. Delivery 4 weeks from order.".

### Watch out
- A change of visibility applies at once to all sales reps, in lists, pipeline, agenda and reports.
- The prefix also applies to the number shown for existing offers.

=> /crmparlist /crmreassign

@@ /crmreassign
### How to
Reassign only part of the data, for example the leads:
1. Choose the **From** and **To** users.
2. Clear **Customers**, **Opportunities** and **Activities to do**, leaving only **Leads**.
3. Press **Reassign**.

> For the chosen type all the user's data is moved, open and closed; for a single customer or opportunity change the owner directly in the card.

### Example
Marco leaves the company. The administrator chooses From: Marco, To: Laura, leaves customers, opportunities, leads and activities to do selected and presses **Reassign**: the message shows for example "Reassigned: customers 34, opportunities 12, leads 9, activities 21.".

### Watch out
- "Choose the user to reassign from and a different recipient.": From is required and To must be different.
- With To = Nobody, customers, opportunities and leads are left without an owner, while the activities to do stay with whoever had them.
- Only activities **To do** are moved; done or cancelled ones stay with the original user.
- Reassignment applies to all of the user's data, including inactive items.

=> /crmcustomerlist /crmsettings

## Glossary
- **Agenda**: list of activities, split into Today, Overdue, Upcoming, Done / cancelled.
- **Assignee**: the user who has to carry out an activity (Assigned to).
- **Shared customer**: customer without an owner, visible to all sales reps.
- **Main contact**: the main contact person of a customer; there is only one.
- **Phase**: step of the pipeline; it can be open, won or lost.
- **Lead**: contact not yet qualified, to be converted into a customer.
- **Opportunity**: deal with a customer, with value, probability and phase.
- **Pipeline**: all the opportunities arranged by phase.
- **Prospect**: potential customer, the default sales status.
- **Owner**: the sales rep following a customer, lead or opportunity.
- **History**: log of the phase and value changes of an opportunity.
- **Win rate**: won divided by (won + lost) in the period.
- **Weighted value**: value × probability of an opportunity.

## Troubleshooting
### "not found or not visible" appears
The item has been deleted or is no longer within your visibility, for example because the customer was assigned to another sales rep. Ask an administrator to check it.

### I cannot change the owner
A sales rep can only take an item without an owner or release one of their own. To assign it to a colleague an administrator is needed, or the Reassignment function.

### I cannot find a customer when choosing it for an opportunity or an offer
Only active customers that are not suppliers only are offered. Check the **Active** and **Registry type** fields in the customer card.

### I do not see the PDF and Send buttons
They appear after the offer is saved for the first time: press **Confirm**.

### I do not get reminders
The reminder arrives only if you are logged in to the application, the activity is To do and assigned to you, and the notice in Settings is not 0.

### "The CRM is being set up for the company: try again in a moment." appears
On first access the CRM creates the default tables: wait a few seconds and repeat the operation.

### "Operation not allowed in the CRM." appears
The function requires a role you do not have, for example the configuration (CompanyPowerAdmin only) or the reassignment (administrators only).
