# Widget della dashboard — Risorse Umane

Dettaglio del ramo **HumanResources**. Il funzionamento generale (catalogo per ramo, configurazione per
utente, tabella `UserDashboard`, stili comuni) è in `ANALISI_INFRASTRUCTURE_WIDGET.md` (Infrastructure).
Le regole sui ruoli del modulo sono in `ANALISI_HUMANRESOURCES_APPLICATIVO.md` §3.

## 1. Widget

| Chiave | Titolo | Dimensione | Ruoli | Predefinito | Dati |
|---|---|---|---|---|---|
| `hr.myPayslips` | I miei cedolini | small (1/3) | CompanyPowerAdmin, CompanyAdmin, User | sì | `Hr_Cedolino/Hr_MieiDocumenti` (esistente) |
| `hr.onboarding` | Assunzioni e cessazioni | medium (1/2) | CompanyPowerAdmin, CompanyAdmin | no | `Hr_Widget/Hr_WidgetOnboardingGet` (nuovo) |
| `hr.trainingExpiring` | Attestati in scadenza | medium (1/2) | CompanyPowerAdmin, CompanyAdmin | no | `Hr_Widget/Hr_WidgetFormazioneGet` (nuovo) |

- Compaiono solo se **Risorse Umane è attivo** per l'azienda (applicativo attivo e licenza), gruppo "Risorse Umane"
  nella pagina di configurazione della dashboard.
- **Ruoli**: "I miei cedolini" come la voce di menu omonima (ogni ruolo dell'azienda, solo i propri documenti); gli altri
  due come le pagine Checklist e Corsi, che il server riserva all'ufficio del personale (`HrUtility.AmministratoreAsync`).
  Lo studio paghe (FinancialAdvisor*) non ha una dashboard configurabile: nessun widget lato studio.
- Dichiarazione: `Utility/infrastructure/widget-human-resources.service.ts` (in Infrastructure è vuoto), zorder 500100-500300.

## 2. I miei cedolini (`hr.myPayslips`)

Indicatori **Ultimo cedolino** (mese e anno dell'ultimo cedolino pubblicato) e **Da confermare** (documenti con la presa
visione chiesta e non ancora confermata), numero dei documenti disponibili e collegamento **Apri i miei cedolini**
(`/hrmypayslips`).

- Dati: `Hr_MieiDocumenti` (area del dipendente), già ordinati dal più recente; nessuna modifica al server.
- Utente non collegato a un dipendente (`dipendente = false`, es. un amministratore esterno): messaggio dedicato.
- Predefinito: nella dashboard mai configurata compare a tutti i ruoli dell'azienda (chi non è dipendente può toglierlo).

## 3. Assunzioni e cessazioni (`hr.onboarding`)

Indicatori **Checklist aperte**, **Voci scadute** (rosso) e **Prossimi 7 giorni** (arancione), ripartizione assunzioni /
cessazioni e le prime 5 voci da fare (data prevista, voce, dipendente, tipo); clic su una voce → `/hrchecklistedit`
della sua checklist, collegamento **Apri le checklist** (`/hrchecklistlist`).

### 3.1 Endpoint `Hr_Widget/Hr_WidgetOnboardingGet`

| | |
|---|---|
| Servizio | `Hr_WidgetService` (`ClientServer-Service/HumanResources/Hr_WidgetService.cs`) |
| Controller | `Hr_WidgetController` (in `Controllers/HumanResources/HrControllers.cs`) |
| Permessi | `HrUtility.AmministratoreAsync` (CompanyPowerAdmin o CompanyAdmin), altrimenti `NotAllowed` |
| Uscita | `ChecklistAperte`, `Assunzioni`, `Cessazioni`, `VociScadute`, `VociProssime`, `GiorniProssime` (7), `Voci[]` (max 5) |

- Prima del calcolo si sincronizzano le checklist generate dal modello predefinito (`HrOnboardingCore.SincronizzaAsync`),
  come fa l'elenco delle checklist: le assunzioni/cessazioni recenti compaiono subito anche nel widget.
- Voci considerate: non completate, di checklist **aperte**, con data prevista entro oggi + 7 giorni.

## 4. Attestati in scadenza (`hr.trainingExpiring`)

Indicatori **Scaduti** (rosso), **Entro 30 giorni** (arancione), **Entro 60 giorni**, e i primi 5 attestati per data di
scadenza (data, corso, dipendente, "obbligatorio"); clic → scheda del corso (`/hrcourseedit`), collegamento
**Apri i corsi** (`/hrcourselist`).

### 4.1 Endpoint `Hr_Widget/Hr_WidgetFormazioneGet`

| | |
|---|---|
| Servizio / Controller | come §3.1 |
| Permessi | `HrUtility.AmministratoreAsync` |
| Uscita | `Scaduti`, `Entro30`, `Entro60` (31-60 giorni), `GiorniFinestra` (60), `Attestati[]` (max 5) |

Calcolo:

- partecipanti con esito **Superato** e `DataScadenza` valorizzata, di **corsi attivi** dell'azienda;
- solo **dipendenti in forza** (`Attivo` e senza cessazione passata);
- per dipendente e corso conta solo **l'ultimo attestato** (scadenza più lontana): un rinnovo supera i precedenti,
  come nello scadenzario comune (`IDeadlineRegistry`, fonte `hr.corso`).

## 5. File

| Livello | File |
|---|---|
| Servizio | `nvxapp.server.service/ClientServer-Service/HumanResources/Hr_WidgetService.cs`, `Models/HrWidgetModels.cs` |
| Controller | `nvxapp.server/Controllers/HumanResources/HrControllers.cs` (`Hr_WidgetController`) |
| Client, API | `ClientServer-Service/HumanResources/hr.service.ts` (`WidgetOnboardingGet`, `WidgetFormazioneGet`), `Models/hr-model.ts` |
| Client, widget | `shared/components/HumanResources/widgets/` (`hr-my-payslips-widget`, `hr-onboarding-widget`, `hr-training-expiring-widget`, `_hr-widget.scss`) |
| Catalogo | `Utility/infrastructure/widget-human-resources.service.ts` |
| Traduzioni | `assets/i18n/humanresources/{it,en}.json`, sezione `hr.widgets` |

## 6. Sviluppi possibili

- Piani di formazione: corsi del piano non ancora seguiti oltre la data "entro il".
- Lato studio paghe: caricamenti da verificare/pubblicare (richiede una dashboard per i ruoli dello studio).
- Per l'utente semplice: le voci delle checklist di cui è responsabile.

## 7. Checklist

| # | Punto | Stato |
|---|---|---|
| HR-W.1 | Endpoint `Hr_Widget/Hr_WidgetOnboardingGet` e `Hr_WidgetFormazioneGet` (servizio, modelli, controller) | ✅ fatta, mergiata |
| HR-W.2 | Client: modelli e metodi in `hr.service.ts` | ✅ fatta, mergiata |
| HR-W.3 | Widget `hr.myPayslips` (riusa `Hr_MieiDocumenti`) | ✅ fatta, mergiata |
| HR-W.4 | Widget `hr.onboarding` | ✅ fatta, mergiata |
| HR-W.5 | Widget `hr.trainingExpiring` | ✅ fatta, mergiata |
| HR-W.6 | Registrazione nel catalogo `widget-human-resources.service.ts` | ✅ fatta, mergiata |
| HR-W.7 | Traduzioni `hr.widgets` (it, en) | ✅ fatta, mergiata |
| HR-W.8 | Build server (`dotnet build`) e client (`ng build`) | ✅ fatta, mergiata |
| HR-W.9 | Prova end-to-end su un'azienda con Risorse Umane attivo (dati di checklist e attestati) | ⏳ da fare |
| HR-W.10 | Merge HumanResources_Dev → HumanResources → AllFunction_Dev → AllFunction | ✅ fatta (12/11/2026) |
