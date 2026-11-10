# Widget della dashboard — Presenze

Dettaglio del ramo **AttendanceTracking**. Il funzionamento generale (catalogo per ramo, configurazione per
utente, tabella `UserDashboard`, stili comuni) è in `ANALISI_WIDGET.md` (Infrastructure).

## 1. Widget

| Chiave | Titolo | Dimensione | Ruoli | Predefinito | Dati |
|---|---|---|---|---|---|
| `attendance.presentToday` | Presenti oggi | small (1/3) | CompanyPowerAdmin, CompanyAdmin | sì | `Dip_Widget/Widget_PresenzeOggiGet` |
| `attendance.requests` | Richieste | small (1/3) | CompanyPowerAdmin, CompanyAdmin, User | sì | `Dip_Widget/Widget_RichiesteGet` |

- Compaiono solo se le **presenze sono attive** per l'azienda (applicativo attivo e licenza: ANALISI_LICENZE.md §15.6).
- Dichiarazione: `Utility/infrastructure/widget-attendance-tracking.service.ts` (in Infrastructure è vuoto).
- Servizio server comune: `Dip_WidgetService` (`ClientServer-Service/GestionePresenze/Dip_WidgetService/`),
  controller `Dip_WidgetController`.

## 2. Presenti oggi (`attendance.presentToday`)

Indicatori **Presenti** (verde) e **Assenti** (rosso), barra dei presenti, "su N dipendenti in servizio" e, se ci
sono, gli **assenti giustificati**; collegamento **Dettagli** alla pagina "Personale presente" (`/presentstaff`).

### 2.1 Endpoint `Dip_Widget/Widget_PresenzeOggiGet`

Riservato a CompanyPowerAdmin e CompanyAdmin (altrimenti `WidgetAdminsOnly`), come la voce di menu
"Personale presente".

| Passo | Regola |
|---|---|
| Dipendenti | utenti dell'azienda con ruolo **User** (`UserCompanyList` con `FilteredRoles = User`) |
| CompanyAdmin | solo i dipendenti dei reparti che **amministra** (`Az_SediRepartoUser.EnabledToAdmin`), assegnati al reparto oggi (`UserInDepartment`, `DataDal`/`DataAl`): stessa regola di report ed export |
| In servizio | anagrafica attiva e non cessata; rapporto di lavoro con assunzione ≤ oggi e licenziamento assente o ≥ oggi. Un dipendente conta una volta anche con più rapporti |
| Presente | l'**ultima timbratura valida di oggi** (`GiornoCompetenza` = oggi, stato `Diretta` o `Approvata`) **non è un'uscita**: stessa regola della pagina "Personale presente" (`PresentStaffService`) |
| Assente | in servizio e non presente |
| Assente giustificato | assente con un giustificativo **di giornata** (`InputType = AllDay`) di oggi, valido (`Diretta` o `Approvata`) |

Uscita: `Data`, `Totale`, `Presenti`, `Assenti`, `AssentiGiustificati` (compresi negli assenti).

Le timbrature si leggono con **una sola query** per tutti i rapporti (non una per dipendente come la pagina),
così il widget resta veloce anche con molti dipendenti.

## 3. Richieste (`attendance.requests`)

| Utente | Indicatori | Collegamento |
|---|---|---|
| CompanyPowerAdmin, CompanyAdmin | **Da approvare** (arancione se > 0), **Approvate nel mese**; sotto, se ci sono, le proprie richieste in attesa | `/requestlistadmin` |
| User | **In attesa** (arancione se > 0), **Approvate nel mese** | `/requestlistuser` |

### 3.1 Endpoint `Dip_Widget/Widget_RichiesteGet`

| Valore | Regola |
|---|---|
| `Admin` | l'utente è CompanyPowerAdmin o CompanyAdmin |
| `DaApprovare` | da `Dip_GG_RichiestaService.RichiestaCount` con `Admin = true`: le richieste che l'utente può approvare **adesso** (approvatore unico, approvazione multilivello, gerarchia), lo stesso numero del badge del menu "Richieste" |
| `InAttesa` | da `RichiestaCount`: proprie richieste con la fase corrente (revoca o richiesta) ancora da decidere |
| `ApprovateMese` | richieste con giorno (`Data`) nel mese corrente, approvate (`RichiestaStato = Approvata`, senza revoca): dei dipendenti dell'azienda per gli amministratori, proprie per l'utente |

## 4. File

| Livello | File |
|---|---|
| Servizio | `nvxapp.server.service/ClientServer-Service/GestionePresenze/Dip_WidgetService/Dip_WidgetService.cs` (+ `Models/`) |
| Controller | `nvxapp.server/Controllers/GestionePresenze/Dip_WidgetController.cs` |
| Messaggi | `AttendanceTrackingMessages` (`WidgetAdminsOnly`) |
| Client, API | `ClientServer-Service/GestionePresenze/Dip_Widget/dip-widget.service.ts`, `Models/dip-widget-model.ts` |
| Client, widget | `shared/components/GestionePresenze/widgets/` (`present-today-widget`, `requests-widget`) |
| Catalogo | `Utility/infrastructure/widget-attendance-tracking.service.ts` |
| Traduzioni | `assets/i18n/attendance-tracking/{it,en}.json`, sezione `attendance.widgets` |

## 5. Sviluppi possibili

- Aggiornamento in tempo reale con le notifiche SignalR già usate dai badge (`RichiestaNotifier`).
- Per l'utente: "La mia giornata" (ore lavorate oggi, ultima timbratura) e saldo ferie/permessi.
- Elenco nominativo degli assenti al passaggio del mouse sull'indicatore.
