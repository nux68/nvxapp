# Analisi – Presenze: controllo di gestione sulle commesse (AT-CDG)

Data: 11/11/2026. Ramo: **AttendanceTracking_Dev** (lavoro su `AttendanceTracking_Dev_an`, worktree `wt_at-an`).
Area `AT-CDG` di `ANALISI_INFRASTRUCTURE_NUOVI_MODULI.md` (§3.1, decisione D2: il controllo di gestione resta nelle Presenze).

**Vincolo di questo lavoro**: nessuna migration e nessuna modifica a entità o tabelle del contesto AttendanceTracking
(un'altra area aggiunge migration allo stesso contesto in parallelo).

---

## 1. Obiettivo

Per commessa e sottocommessa, in un periodo:
- **ore lavorate** (timesheet: timbrature con l'attività della sottocommessa);
- **costo del personale** = ore × costo orario del dipendente (AT-CDG.1, AT-CDG.2);
- **note spese** attribuite alla commessa (AT-CDG.2);
- **ricavo** (facoltativo) e **marginalità** = ricavo − costi, in valore e percentuale (AT-CDG.3);
- report PDF solo tramite job, widget della dashboard (AT-CDG.3).

## 2. Situazione

| Dato | Dove | Nota |
|---|---|---|
| Ore per attività/sottocommessa/commessa | `ActivityStatisticsService.ActivityStatisticsGet` (mese, dipendenti) | Segmenti tra timbrature con `IdAz_SubCommessaAttivita`; giornate con errori o non calcolate escluse (elencate a parte) |
| Commesse | `Az_Commessa` (cliente) → `Az_SubCommessa` → `Az_SubCommessaAttivita` | nessun campo di ricavo o budget |
| Rapporto di lavoro | `Dip_RapportoLavoro` | **nessun campo di costo** (né retribuzione, né livello) |
| Note spese | `Dip_GG_NotaSpesa` (rapporto, data, importo, stato) | **non collegate a una commessa** |
| Parametri senza schema | `AppSetting` (public, chiave/valore, 500 caratteri) | troppo piccolo per i costi per dipendente; nessuna tabella chiave/valore negli schemi delle aziende |

## 3. Decisioni

| # | Decisione | Scelta provvisoria |
|---|---|---|
| CDG1 | Da dove prendere il costo orario | Non esiste un campo e non si possono aggiungere colonne (vincolo). **Costo orario impostabile nella pagina del report, non persistente sul server**: un costo predefinito per tutti più un costo per dipendente facoltativo. Il browser lo ricorda (localStorage, solo comodità dell'utente) e lo invia a ogni richiesta e al job del PDF. In futuro: campo `CostoOrario` (con data di validità) su `Dip_RapportoLavoro` con una migration |
| CDG2 | Ricavo della commessa | Come CDG1: **ricavo per sottocommessa impostabile nella pagina**, non persistente. Senza ricavo si mostrano solo i costi (margine non calcolato) |
| CDG3 | Attribuzione delle note spese | La nota spese non ha la commessa: si **ripartisce per mese e dipendente in proporzione alle ore** lavorate dal dipendente su ciascuna sottocommessa in quel mese. Se il dipendente non ha ore nel mese va sulla **sottocommessa predefinita del rapporto** (`Dip_RapportoLavoro.IdAz_SubCommessaAttivita`). Le ripartizioni si arrotondano al centesimo e l'ultima quota prende il resto (il totale torna sempre) |
| CDG4 | Note spese considerate | Approvate o inserite direttamente (`Approvata`, `Diretta`), con data nel periodo |
| CDG5 | Ore considerate | Quelle di `ActivityStatisticsService` (stesse regole della pagina Statistiche attività): le giornate escluse (errori, non calcolate) si contano e si segnalano, come nella pagina |
| CDG6 | Periodo | Da un mese a un altro (al massimo 12 mesi), le ore si leggono mese per mese |
| CDG7 | Chi vede il report | CompanyPowerAdmin (come Statistiche attività e Commesse). Il CompanyAdmin no: vedrebbe costi di dipendenti di altri reparti |
| CDG8 | Widget | `attendance.jobHours`: ore del mese corrente per commessa (prime 5) e, se il browser conosce il costo predefinito impostato nella pagina, costo stimato. Non predefinito |

## 4. Calcolo (verificabile a mano)

Per ogni sottocommessa `s` e periodo:
- `Ore(s)` = Σ minuti dei dipendenti su `s` / 60
- `CostoPersonale(s)` = Σ per dipendente `minuti(dip, s) / 60 × CostoOrario(dip)`, con `CostoOrario(dip)` = costo del
  dipendente se impostato, altrimenti il predefinito (0 se nessuno dei due)
- `NoteSpese(s)` = Σ per mese e dipendente `TotaleNote(dip, mese) × minuti(dip, s, mese) / minuti(dip, mese)` (CDG3)
- `Costo(s)` = `CostoPersonale(s)` + `NoteSpese(s)`
- `Margine(s)` = `Ricavo(s)` − `Costo(s)`; `Margine%(s)` = `Margine(s) / Ricavo(s) × 100` (solo con ricavo > 0)
- La commessa somma le sue sottocommesse (il ricavo della commessa è la somma dei ricavi impostati)
- Importi arrotondati al centesimo (`MidpointRounding.AwayFromZero`)

## 5. Funzioni

| Elemento | Dettaglio |
|---|---|
| API | `Az_CommessaCdg/CdgGet` (periodo, costi, ricavi, filtro commesse) → righe per commessa e sottocommessa con il dettaglio per dipendente, totali, giornate escluse, note spese non attribuite; `Az_CommessaCdg/Widget_OreCommesseGet` |
| Pagina | **"Marginalità commesse"** (`/jobmargins`, CompanyPowerAdmin, menu Gestione commesse): periodo dal mese al mese, costo orario predefinito, costi per dipendente, ricavo per sottocommessa, tabella commesse/sottocommesse espandibile con il dettaglio per dipendente |
| Report | PDF "Marginalità commesse" tramite job (`ReportService`, `ReportTipo.MarginalitaCommesse`), con gli stessi costi e ricavi inviati dalla pagina |
| Widget | `attendance.jobHours` (CDG8) |

## 6. File

| Livello | File |
|---|---|
| Calcolo | `nvxapp.server.service/ClientServer-Service/GestionePresenze/Az_CommessaCdgService/` (servizio, modelli, `CdgCalcolo` puro) |
| API | `nvxapp.server/Controllers/GestionePresenze/Az_CommessaCdgController.cs` |
| Report | `ReportService` (`ReportTipo.MarginalitaCommesse`) |
| Client | `ClientServer-Service/GestionePresenze/Az_CommessaCdg/`, pagina `pages/GestionePresenze/cdg-commesse-page/`, widget `shared/components/GestionePresenze/widgets/job-hours-widget`, catalogo, menu, route, i18n |

---

## 7. Checklist

Legenda: ✅ svolto · ⏳ da fare · 🔍 da decidere

- ⏳ `AT-CDG.1` Costo orario del dipendente: impostabile nella pagina (predefinito + per dipendente), non persistente (CDG1)
- ⏳ `AT-CDG.2` Costi della commessa: ore del timesheet × costo orario + note spese ripartite (CDG3)
- ⏳ `AT-CDG.3` Report di marginalità per commessa/sottocommessa e periodo (pagina + PDF tramite job) e widget
- ⏳ Testi it/en
- ⏳ Verifica: build server e client, nessuna migration, prova su DB usa e getta con valori calcolati a mano
- 🔍 Costo orario e ricavo persistenti (campo su `Dip_RapportoLavoro` e su `Az_SubCommessa`, con migration) quando il contesto sarà libero
- 🔍 Collegamento diretto della nota spese alla commessa (colonna `IdAz_SubCommessa` facoltativa su `Dip_GG_NotaSpesa`)
