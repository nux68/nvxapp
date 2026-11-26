# ANALISI_INFRASTRUCTURE_NOME_APP

Cambio del nome dell'applicazione da **SiSignore** a **HR++**.

## 1. Come è gestito oggi il nome

Il nome è già centralizzato: nel codice non va mai scritto a mano.

- **Client**: `SITE_NAME` in `nvxapp.client/src/nvx/Utility/infrastructure/site-name.ts`. Vale il primo valore non nullo tra:
  `SITE_NAME_ALL_FUNCTION` → `SITE_NAME_ATTENDANCE_TRACKING` → `SITE_NAME_CRM` → `SITE_NAME_DOCUMENT_MANAGEMENT` → `SITE_NAME_INFRASTRUCTURE`.
  Ogni valore sta in un file `site-name-<ramo>.ts` che il proprio ramo riempie (in Infrastructure sono `null`).
- **Server**: `SiteName.Value` in `nvxapp.server.service/ClientServer-Service/infrastructure/Site/SiteName.cs` = `SiteNameAttendanceTracking.Value ?? SiteNameInfrastructure.Value`.

Dove si vede `SITE_NAME` (client):

| Dove | File |
|---|---|
| Titolo della scheda del browser | `app/app.component.ts` (`setTitle`) |
| Intestazione del menu laterale e pagina di accesso | `login-page.component.ts`, menu |
| Vetrina (home e pagine prezzi di ogni applicativo) | `showcase-*.service.ts`, `showcase-home-page` |
| Titolo del manuale PDF | `user-manual-page.component.ts` → `UserManualService` (server) |
| Email di contatto della vetrina | `showcase-infrastructure.service.ts`: `info@${SITE_NAME.toLowerCase()}.it` |

Dove si vede `SiteName.Value` (server): email di recupero password, di registrazione e firma delle notifiche (`PasswordRecoveryService`, `RegistrationService`, `NotificationSender`).

### Valori attuali per ramo

| Ramo | Client (`SITE_NAME`) | Server (`SiteName.Value`) |
|---|---|---|
| Infrastructure, HumanResources, Compliance, Assets | MyTemplate | MyTemplate |
| AttendanceTracking | SiSignore | SiSignore |
| CRM | SiSignore | **MyTemplate** (manca il valore lato server) |
| DocumentManagement | SiSignore | **MyTemplate** (manca il valore lato server) |
| AllFunction | SiSignore | SiSignore (preso da AttendanceTracking: non esiste un valore AllFunction lato server) |

## 2. Cosa si deve cambiare

### 2.1 Nome del sito (pagine, menu, accesso, vetrina, email)

| File | Ramo dove si modifica | Da → a |
|---|---|---|
| `site-name-attendance-tracking.ts` | AttendanceTracking_Dev | SiSignore → HR++ |
| `SiteNameAttendanceTracking.cs` | AttendanceTracking_Dev | SiSignore → HR++ |
| `site-name-crm.ts` | CRM_Dev | SiSignore → HR++ |
| `site-name-document-management.ts` | DocumentManagement_Dev | SiSignore → HR++ |
| `site-name-all-function.ts` | AllFunction_Dev | SiSignore → HR++ |

Da decidere: se HR++ vale per tutti i rami che oggi si chiamano SiSignore (proposta) o solo per l'applicativo completo (AllFunction). Nel secondo caso si cambia solo `site-name-all-function.ts` e serve il valore lato server del punto 2.2.

### 2.2 Nome lato server dell'applicativo completo (lacuna)

Il server non ha un valore per AllFunction, CRM e DocumentManagement: oggi le email di AllFunction dicono SiSignore solo perché lo prende da AttendanceTracking, e quelle di CRM e Documentale dicono MyTemplate. Proposta: aggiungere `SiteNameAllFunction.cs` (in Infrastructure con `null`, riempito su AllFunction_Dev) con la stessa precedenza del client, ed eventualmente `SiteNameCrm.cs` e `SiteNameDocumentManagement.cs`.

### 2.3 Nome dell'app installata e del collegamento sulla schermata Home

Oggi l'app installata e il collegamento creati dal telefono si chiamano **"NVX App"** / **"NVX"**: vengono dal manifest, non da `SITE_NAME`.

| File | Ramo | Da → a |
|---|---|---|
| `nvxapp.client/src/manifest.webmanifest`: `name` | Infrastructure_Dev | NVX App → HR++ |
| `nvxapp.client/src/manifest.webmanifest`: `short_name` (nome sotto l'icona) | Infrastructure_Dev | NVX → HR++ |
| `nvxapp.client/src/index.html`: `<title>` (titolo prima che parta l'app) | Infrastructure_Dev | NVX App → HR++ |

Il manifest è un file fisso, uguale per tutti i rami: diventa HR++ anche per Infrastructure (oggi MyTemplate). In alternativa si genera nella build di ogni ramo, ma è più lavoro senza un vantaggio reale.

Il telefono legge il manifest solo all'installazione: per vedere il nuovo nome l'app va disinstallata e reinstallata.

### 2.4 Mittente delle email

`nvxapp.server/appsettings.json` → `Email:FromName` = "SiSignore" (uguale su tutti i rami): → HR++.
Da controllare a mano (file con segreti, non letti): un eventuale `NVXAPP_Email__FromName` in `DockerLocale.env`, `DockerVps.env` o nelle variabili di Windows, che avrebbe la precedenza.

### 2.5 Email di contatto della vetrina

`showcase-infrastructure.service.ts` costruisce l'indirizzo dal nome: con HR++ diventerebbe `info@hr++.it`, che non è un indirizzo valido. Va separato dal nome: una costante a parte (es. `SITE_EMAIL` accanto a `SITE_NAME`) con l'indirizzo vero.

## 3. Cosa non cambia

- Nomi tecnici interni: cartelle e progetti `nvxapp`, namespace, database, container Docker, rami, `PRODID` del calendario (`-//nvxapp//Calendario//IT`). Non si vedono nell'app.
- Nome del server MCP (`McpEndpoint.cs`: `Name = "nvxapp"`): compare nei client AI collegati. Si può rinominare a parte; il collegamento esistente continua a funzionare.
- Traduzioni, manuali e documenti: non contengono "SiSignore" (usano il nome dal codice).

## 4. Rischi del nome "HR++"

- Il carattere `+` non crea problemi dove il nome compare oggi: testo delle pagine, titolo, email (HTML già codificato), titolo del PDF. Non compare in URL o nomi di file.
- Unico punto rotto: l'email della vetrina (2.5).
- Nella vetrina e nei testi il nome è inserito dentro le frasi tradotte: con un nome breve come HR++ conviene rileggere la vetrina dopo il cambio.

## 5. Ordine dei lavori e merge

1. Infrastructure_Dev: manifest, `index.html`, `appsettings.json`, email della vetrina, `SiteNameAllFunction.cs` (null) → catena Infrastructure → tutti i moduli.
2. AttendanceTracking_Dev, CRM_Dev, DocumentManagement_Dev: i rispettivi `site-name-*` → stabili.
3. AllFunction_Dev: `site-name-all-function.ts` e `SiteNameAllFunction.cs` = HR++ → AllFunction → pubblicazione.

## 6. Checklist

- [x] Deciso: HR++ ovunque possibile, su tutti i rami oggi SiSignore; per indirizzi email e domini "hrplusplus"
- [x] `manifest.webmanifest`: `name` e `short_name` = HR++ (Infrastructure_Dev)
- [x] `index.html`: `<title>` = HR++ (Infrastructure_Dev)
- [x] `appsettings.json`: `Email:FromName` = HR++ (Infrastructure_Dev)
- [ ] Controllo a mano di `NVXAPP_Email__FromName` nei file di ambiente (utente)
- [x] Email della vetrina separata dal nome: `SITE_SLUG` in site-name.ts ("HR++" -> "hrplusplus", quindi info@hrplusplus.it) (Infrastructure_Dev)
- [x] `SiteNameAllFunction.cs`, `SiteNameCrm.cs`, `SiteNameDocumentManagement.cs` lato server con la stessa precedenza del client (Infrastructure_Dev null, HR++ nei rami)
- [x] `site-name-attendance-tracking.ts` e `SiteNameAttendanceTracking.cs` = HR++ (AttendanceTracking_Dev)
- [x] `site-name-crm.ts` = HR++ (CRM_Dev)
- [x] `site-name-document-management.ts` = HR++ (DocumentManagement_Dev)
- [x] `site-name-all-function.ts` = HR++ (AllFunction_Dev)
- [x] Catena dei merge, build e pubblicazione
- [ ] Controllo: titolo, menu, accesso, vetrina, email, manuale PDF, nome dell'app installata (dopo reinstallazione)
