# Widget della dashboard — Compliance

Dettaglio del ramo **Compliance**. Il funzionamento generale (catalogo per ramo, configurazione per utente, tabella
`UserDashboard`, stili comuni) è in `ANALISI_INFRASTRUCTURE_WIDGET.md` (Infrastructure); i permessi del modulo in
`ANALISI_COMPLIANCE_APPLICATIVO.md` §3.

## 1. Widget

| Chiave | Titolo | Dimensione | Ruoli | Predefinito | Dati |
|---|---|---|---|---|---|
| `compliance.deadlines` | Scadenze Compliance | medium (1/2) | CompanyPowerAdmin, CompanyAdmin | sì | `Cmp_Widget/Cmp_WidgetScadenzeGet` (nuovo) |
| `compliance.trainingStatus` | Formazione obbligatoria | small (1/3) | CompanyPowerAdmin, CompanyAdmin | no | `Cmp_Sicurezza/Cmp_FormazioneStato` (esistente) |
| `compliance.gdpr` | Registro GDPR | small (1/3) | CompanyPowerAdmin, CompanyAdmin | no | `Cmp_Widget/Cmp_WidgetGdprGet` (nuovo) |
| `compliance.mine` | I miei adempimenti | small (1/3) | CompanyPowerAdmin, CompanyAdmin, User | sì | `Cmp_Sicurezza/Cmp_Mine` (esistente) |

- Compaiono solo se **Compliance è attivo** per l'azienda (applicativo attivo e licenza), come gli altri applicativi.
- **Ruoli**: i tre widget di gestione sono degli amministratori dell'azienda, come le pagine "Sicurezza sul lavoro"
  e "Registro GDPR" (il server ricontrolla `CmpContesto.Amministratore` e risponde `NotAllowed` agli altri).
  "I miei adempimenti" è di chiunque lavori nell'azienda, come la voce di menu omonima.
- Lo studio (FinancialAdvisor*) non ha una dashboard configurabile: continua a usare la pagina "Compliance aziende".
- Gli abilitati ai dati sanitari che non sono amministratori non hanno widget dedicati: le visite le aprono da `/cmp81`.
- `zorder` nella fascia 600000 (come il menu di Compliance); dichiarazione in
  `Utility/infrastructure/widget-compliance.service.ts` (in Infrastructure è vuoto).

## 2. Scadenze Compliance (`compliance.deadlines`)

Indicatori **Scadute** e **Entro N giorni** (N = giorni di preavviso della configurazione di Compliance) e l'elenco dei
soli tipi con qualcosa da guardare, con i conteggi in evidenza (rosso scadute, arancione in scadenza). Ogni riga apre la
sezione della pagina giusta passando lo stato `{ sezione }` (come le scadenze e le notifiche):

| Tipo (fonte) | Pagina | Sezione |
|---|---|---|
| `cmp.formazione` (corsi) | `/cmp81` | `formazione` |
| `cmp.visita` (visite mediche) | `/cmp81` | `visite` |
| `cmp.nomina` (nomine) | `/cmp81` | `nomine` |
| `cmp.documento` (DVR e documenti) | `/cmp81` | `documenti` |
| `cmp.responsabile` (contratti dei responsabili esterni) | `/cmpgdpr` | `responsabili` |
| `cmp.violazione` (termine di notifica) | `/cmpgdpr` | `violazioni` |

Collegamento **Apri Sicurezza sul lavoro**. Etichette dei tipi: `cmp.enums.fonte.*` (già esistenti).

### 2.1 Endpoint `Cmp_Widget/Cmp_WidgetScadenzeGet`

| | |
|---|---|
| Servizio | `Cmp_WidgetService` (`ClientServer-Service/Compliance/Cmp_WidgetService.cs`, modelli in `Models/CmpWidgetModels.cs`) |
| Controller | `Cmp_WidgetController` (`Controllers/Compliance/CmpWidgetController.cs`) |
| Permessi | `CmpContesto.Amministratore` (CompanyPowerAdmin o CompanyAdmin), altrimenti `NotAllowed` |
| Ingresso | `Cmp_EmptyInModel` |
| Uscita | `Aree[]` (`Fonte` senza prefisso `cmp.`, `Scadute`, `InScadenza`), `Scadute`, `InScadenza`, `GiorniPreavviso` |

Legge lo **scadenzario comune** (tabella `Scadenza`, `Application = Compliance`, `Stato = Aperta`), che i servizi di
Compliance tengono aggiornato a ogni salvataggio (`ICmpScadenze`): i corsi e le visite rinnovati hanno già la scadenza
chiusa, quindi si conta solo l'ultimo di ogni tipo. Scaduta = data < oggi; in scadenza = da oggi a oggi + preavviso
**della scadenza** (`GiorniPreavviso`), lo stesso criterio di `Cmp_StudioScadenze` e del promemoria notturno.
Le prese visione dei DPI non sono scadenze e non si contano.

**Privacy**: delle visite si contano solo le scadenze (nessun esito, limitazione o dato sanitario), che gli amministratori
vedono già nella pagina.

## 3. Formazione obbligatoria (`compliance.trainingStatus`)

Indicatori **Mancanti**, **Scaduti**, **In scadenza** e la riga "N dipendenti non in regola su M"; collegamento alla
sezione **Situazione formazione** (`/cmp81`, sezione `stato`).

Usa `Cmp_FormazioneStato` così com'è (corsi richiesti dalle mansioni attive dei dipendenti attivi, con l'ultimo corso
svolto): `Stato` 0 mancante, 2 in scadenza, 3 scaduto; non in regola = dipendenti con almeno un corso mancante o scaduto.
Senza mansioni o corsi richiesti il widget spiega cosa configurare.

## 4. Registro GDPR (`compliance.gdpr`)

Indicatori **Violazioni da notificare** (rosso se almeno una è oltre le 72 ore, arancione altrimenti, verde se nessuna) e
**Responsabili esterni da rinnovare** (contratti scaduti + in scadenza), cliccabili sulle rispettive sezioni di
`/cmpgdpr`; righe con le violazioni oltre il termine, i trattamenti attivi (con dati particolari) e le violazioni degli
ultimi 12 mesi; collegamento **Apri il registro GDPR**.

### 4.1 Endpoint `Cmp_Widget/Cmp_WidgetGdprGet`

| | |
|---|---|
| Permessi | `CmpContesto.Amministratore`, altrimenti `NotAllowed` |
| Uscita | `TrattamentiAttivi`, `TrattamentiDatiParticolari`, `ResponsabiliScaduti`, `ResponsabiliInScadenza`, `ViolazioniDaNotificare`, `ViolazioniOltreTermine`, `Violazioni12Mesi` |

- Violazione da notificare: `Rischio > 0` e `DataNotificaGarante` vuota (come `NotificaDovuta` di `Cmp_GdprService`);
  oltre il termine se sono passate più di 72 ore da `DataScoperta`.
- Responsabili: `DataScadenza` del contratto; in scadenza entro i giorni di preavviso della configurazione.
- Solo conteggi: nessun testo delle violazioni o dei trattamenti esce dal servizio.

## 5. I miei adempimenti (`compliance.mine`)

Per l'utente collegato a un dipendente dell'azienda (`Cmp_Mine`): indicatori **Corsi da rinnovare** (scaduti + in
scadenza, solo l'ultimo corso di ogni tipo) e **DPI da confermare** (consegne con presa visione richiesta e non data);
riga con la **data della prossima visita medica** (scadenza dell'ultima visita, evidenziata se scaduta o vicina) e, se ci
sono, le nomine in corso; collegamento **Apri i miei adempimenti** (`/cmpmine`).

- In dashboard compare **solo la data** della visita: l'esito resta nella pagina personale (dashboard visibile a chi passa
  davanti allo schermo).
- Utente non collegato a un dipendente (es. amministratore esterno): messaggio dedicato, nessun errore.

## 6. File

| Livello | File |
|---|---|
| Servizio | `nvxapp.server.service/ClientServer-Service/Compliance/Cmp_WidgetService.cs`, `Models/CmpWidgetModels.cs` |
| Controller | `nvxapp.server/Controllers/Compliance/CmpWidgetController.cs` |
| Client, API | `ClientServer-Service/Compliance/cmp-widget.service.ts`, `Models/cmp-widget-model.ts` (+ `CmpService.FormazioneStato` e `Mine` esistenti) |
| Client, widget | `shared/components/Compliance/widgets/` (`cmp-deadlines-widget`, `cmp-training-widget`, `cmp-gdpr-widget`, `cmp-mine-widget`, `cmp-widget-nav.ts`) |
| Catalogo | `Utility/infrastructure/widget-compliance.service.ts` |
| Traduzioni | `assets/i18n/compliance/{it,en}.json`, sezione `cmp.widgets` |

Registrazione dei servizi: automatica (scansione dell'assembly dei servizi in `Installers.cs`), nessuna modifica.
Nessuna migrazione: si leggono solo tabelle esistenti.

## 7. Sviluppi possibili

- DPI consegnati in attesa di presa visione, lato amministratore (oggi solo nel widget personale).
- Widget per gli abilitati ai dati sanitari non amministratori: visite in scadenza (servirebbe un ruolo/permesso nel catalogo
  dei widget, oggi basato solo sui ruoli).
- Andamento mensile delle scadenze chiuse in ritardo.

## 8. Checklist

| # | Punto | Stato |
|---|---|---|
| CMP-W.1 | Endpoint `Cmp_WidgetScadenzeGet` (scadenzario comune, solo amministratori) | ✅ fatta, da mergiare |
| CMP-W.2 | Endpoint `Cmp_WidgetGdprGet` (conteggi registro GDPR, solo amministratori) | ✅ fatta, da mergiare |
| CMP-W.3 | Client API `cmp-widget.service.ts` e modelli | ✅ fatta, da mergiare |
| CMP-W.4 | Widget `compliance.deadlines` | ✅ fatta, da mergiare |
| CMP-W.5 | Widget `compliance.trainingStatus` (riusa `Cmp_FormazioneStato`) | ✅ fatta, da mergiare |
| CMP-W.6 | Widget `compliance.gdpr` | ✅ fatta, da mergiare |
| CMP-W.7 | Widget `compliance.mine` (riusa `Cmp_Mine`, solo data della visita) | ✅ fatta, da mergiare |
| CMP-W.8 | Registrazione in `widget-compliance.service.ts` | ✅ fatta, da mergiare |
| CMP-W.9 | Traduzioni it/en `cmp.widgets` | ✅ fatta, da mergiare |
| CMP-W.10 | Build server e client | ✅ fatta |
| CMP-W.11 | Prova manuale in AllFunction con Compliance attivo (dopo la catena dei merge) | ⏳ da fare |
