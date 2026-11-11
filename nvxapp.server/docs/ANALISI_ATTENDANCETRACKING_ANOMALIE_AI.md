# Analisi – Presenze: timbrature anomale, giornate da sistemare e previsione delle assenze (AT-AI)

Data: 11/11/2026. Ramo: **AttendanceTracking_Dev** (lavoro su `AttendanceTracking_Dev_an`, worktree `wt_at-an`).
Area `AT-AI` di `ANALISI_INFRASTRUCTURE_NUOVI_MODULI.md` (§3.1 "Analisi AI", §10.2).

**Vincolo di questo lavoro**: nessuna migration e nessuna modifica a entità o tabelle del contesto AttendanceTracking
(un'altra area aggiunge migration allo stesso contesto in parallelo). Tutto si calcola dai dati che esistono già.

---

## 1. Obiettivo

1. **Timbrature anomale** (AT-AI.1): trovare con **regole deterministiche** le giornate con problemi (timbrature dispari,
   orari fuori profilo, pause mancanti o eccessive, giornate vuote con presenza attesa, ritardi ricorrenti, errori del
   calcolo). L'AI serve **solo** a riassumere e spiegare i fatti già calcolati; senza AI il testo si genera dai fatti.
2. **Giornate da sistemare prima dell'export paghe** (AT-AI.2): per un periodo, l'elenco delle giornate che bloccano
   un export corretto (errori), separate da quelle solo da verificare (avvisi).
3. **Previsione delle assenze** (AT-AI.3): statistica semplice e spiegabile sullo storico e sul piano ferie; nessun
   modello opaco.
4. **Widget, chat e riepilogo** (AT-AI.4): widget nel catalogo delle Presenze, comando chat/vocale "giornate da
   sistemare", sezione delle Presenze nel riepilogo AI settimanale (`IDigestProvider`, INF-AI).

## 2. Dati usati (già esistenti)

| Dato | Tabella | Uso |
|---|---|---|
| Timbrature | `Dip_GG_Timbratura` (`GiornoCompetenza`, `Timbratura`, `RichiestaStato`) | valide = `Diretta` o `Approvata`; in attesa = `Immessa`, `ApprovazioneInCorso`, `ParzialmenteApprovata` |
| Giustificativi | `Dip_GG_Giustificativi` (`InputType`, `Hours`, `RichiestaStato`) + `Par_Giustificativi.VisualizzaInPianoFerie` | copertura della giornata; piano ferie |
| Risultato del calcolo | `Dip_GG_Result` (`Stato` a bit, `HH_Teo`, `HH_Lav`, `Min_Assenze`) | errori/avvisi del motore, giornate non calcolate, storico delle assenze |
| Orario atteso | `Dip_ProfiloOrario` → `Par_ProfiloOrario(GG)` → `Par_Orario` → `Par_OrarioIntervalloHH`, `Par_OrarioPausa`, `Par_Festivita` | ore teoriche, finestra della giornata, primo ingresso atteso, pause configurate |
| Rapporti | `Dip_RapportoLavoro` (`DataAss`, `DataLic`), `Dip_Anagrafica` | dipendenti in servizio nel giorno |

Il profilo del giorno si risolve con la **stessa regola** del motore e di `GestionePresenzeOreTeoriche` (orario con
`ZOrder` più basso del giorno, giorno del ciclo per i profili ciclici, orario festivo nelle festività del calendario).

## 3. Regole delle anomalie (AT-AI.1)

Si valutano i giorni **fino a ieri** (oggi è in corso). Ogni anomalia ha una **gravità**:
**Errore** = la giornata va sistemata prima dell'export paghe; **Avviso** = da verificare.

| Codice | Regola | Gravità | Fonte |
|---|---|---|---|
| `TimbratureDispari` | numero dispari di timbrature valide in una giornata con orario a coppie (manca un'entrata o un'uscita) | Errore | timbrature |
| `GiornataVuota` | ore teoriche > 0, nessuna timbratura valida, nessun giustificativo valido nel giorno | Errore | orario + timbrature + giustificativi |
| `ErroreCalcolo` | il calcolo del giorno ha un errore (`Err`: timbrature mancanti, ore mancanti, sequenza non interpretabile, formule) | Errore | `Dip_GG_Result.Stato` |
| `NonCalcolata` | giornata con ore teoriche o timbrature ma senza risultato del calcolo (o ancora `Init`) | Errore | `Dip_GG_Result` |
| `RichiestaInAttesa` | timbrature o giustificativi del giorno ancora da approvare | Errore | stato delle richieste |
| `FuoriOrario` | timbratura prima dell'inizio o dopo la fine della finestra dell'orario (limiti di riconoscimento della prima e dell'ultima coppia, altrimenti `Dalle`/`Alle` ± 30') | Avviso | orario + timbrature |
| `GiornoNonLavorativo` | timbrature in un giorno con ore teoriche = 0 (riposo, festivo) | Avviso | orario + timbrature |
| `PausaMancante` | orari **senza** pause configurate: tratto continuo di lavoro oltre 6 ore senza uno stacco di almeno 10' (D.Lgs 66/2003 art. 8); con pause configurate vale l'avviso del motore (`Warning_3`) | Avviso | timbrature / `Warning_3` |
| `PausaEccessiva` | orari **senza** pause configurate: stacco tra un'uscita e il rientro oltre 2 ore; con pause configurate vale `Warning_4` | Avviso | timbrature / `Warning_4` |
| `Ritardo` | prima entrata oltre l'inizio della prima coppia (limite destro di riconoscimento, altrimenti `Dalle` + 5') | Avviso (solo conteggio) | orario + timbrature |
| `RitardiRicorrenti` | **3 o più** ritardi dello stesso dipendente nel periodo | Avviso (per dipendente) | aggregato dei ritardi |

- Le soglie (30', 6 ore, 10', 2 ore, 5', 3 ritardi) sono costanti del servizio (`GestionePresenzeAnomalie`), documentate qui.
- Gli orari a **monte ore** non hanno coppie: si applicano solo `GiornataVuota`, `NonCalcolata`, `ErroreCalcolo`, `RichiestaInAttesa`.
- Un giorno può avere più anomalie; la giornata è "da sistemare" se ne ha almeno una di gravità Errore.

### 3.1 Ruolo dell'AI

- Il servizio calcola i **fatti** (conteggi per tipo, giornate e dipendenti coinvolti) e li passa all'AI con il provider
  già configurato (`ILlmClient`), con un prompt che vieta di aggiungere numeri, nomi, date o cause che non sono nei fatti
  (stesso schema di `DigestWriter`, INF-AI.2).
- Configurazione: `AI:AnomalieLLM` (vuoto = provider di `AI:UseLLM`; `none` = sempre senza AI).
- **Ripiego senza AI**: se l'AI non è configurata, non risponde o risponde vuoto, il testo è l'elenco dei fatti.
- L'AI non decide nulla: non cambia gravità, non aggiunge anomalie, non corregge timbrature.

## 4. Giornate da sistemare prima dell'export paghe (AT-AI.2)

- Pagina **"Anomalie presenze"** (`/attendanceanomalies`, CompanyPowerAdmin e CompanyAdmin): periodo mensile, reparti e
  dipendenti con il selettore comune (`app-sedi-reparto-user-navigation`), filtro "solo da sistemare", elenco per giorno
  e dipendente con le anomalie, apertura del Calendario HR sul dipendente, ritardi ricorrenti, "Spiega" (testo AI).
- Il CompanyAdmin vede solo i dipendenti dei reparti che amministra (stessa regola di report, widget ed export).
- Report PDF "Giornate da sistemare" tramite il **meccanismo dei job** (`ReportService`, nuovo `ReportTipo`).

## 5. Previsione delle assenze (AT-AI.3)

Metodo (spiegato anche nella pagina):

1. **Storico**: ultimi 365 giorni fino a ieri, giornate calcolate (`Dip_GG_Result`) con ore teoriche > 0 dei dipendenti
   selezionati. Per ogni giornata la quota di assenza è `min(1, Min_Assenze / minuti teorici)` (assenza giustificata
   che riduce il lavorabile; mezza giornata di permesso = 0,5).
2. **Tasso per giorno della settimana** `T(g)` = somma delle quote di assenza / numero di giornate lavorative di quel
   giorno della settimana. Con meno di 20 giornate per quel giorno si usa il tasso medio generale.
3. **Fattore del mese** `F(m)` = tasso del mese di calendario / tasso medio generale (1 se il mese ha meno di 20
   giornate nello storico o il tasso medio è 0). Coglie la stagionalità (es. agosto).
4. **Assenze già pianificate** `P(d)`: dipendenti con un giustificativo valido o in attesa nel giorno `d` di un tipo
   visibile nel piano ferie (`VisualizzaInPianoFerie`).
5. **Previsione** per i prossimi 30 giorni, solo per i giorni lavorativi di ciascun dipendente (ore teoriche > 0 dal
   profilo orario): attese statistiche `S(d) = Σ dipendenti in servizio T(g) × F(m)`; **previste** `A(d) = max(P(d), S(d))`
   (le assenze pianificate sono certe, la statistica copre quelle non pianificate).
6. **Picco**: giorno con `A(d)` ≥ 20% dei dipendenti attesi al lavoro, oppure con `P(d)` ≥ 2 e almeno il doppio della media
   giornaliera delle assenze pianificate nel periodo.

Il risultato riporta anche i tassi usati (per giorno della settimana e per mese) e il numero di giornate dello storico,
così il calcolo si può verificare a mano.

## 6. Widget, chat, riepilogo (AT-AI.4)

| Elemento | Dettaglio |
|---|---|
| Widget `attendance.daysToFix` | "Giornate da sistemare" fino a ieri, dal primo del mese corrente (nei primi 10 giorni del mese dal primo del mese precedente, tempo di export paghe): da sistemare (rosso), da verificare (arancione), dipendenti coinvolti; link alla pagina. CompanyPowerAdmin, CompanyAdmin. Non predefinito |
| Widget `attendance.absenceForecast` | Assenze previste nei prossimi 7 giorni (media, giorno di picco, pianificate). CompanyPowerAdmin, CompanyAdmin. Non predefinito |
| Chat/voce | intento `DaysToFix` ("giornate da sistemare", "cosa devo sistemare prima delle paghe di ottobre"), slot mese facoltativo (default mese corrente), ruolo HR |
| Riepilogo AI | `AttendanceTrackingDigestProvider` (`Application = AttendanceTracking`): anomalie del periodo del riepilogo (giornate da sistemare = critica, avvisi, ritardi ricorrenti) e assenze pianificate nella settimana successiva. Legge con `IApplicationDbContextFactory` e i repository costruiti sul contesto dello schema dell'azienda (fuori dalla richiesta) |

## 7. Decisioni

| # | Decisione | Scelta provvisoria |
|---|---|---|
| AN1 | Dove salvare soglie e parametri delle regole | **Costanti nel codice** (§3): nessuna colonna nuova (vincolo). Se serviranno per azienda: `Az_Cfg` o tabella parametri con una migration futura |
| AN2 | Salvare le anomalie trovate | **No**: si calcolano al volo dai dati (sono pochi giorni per volta e cambiano a ogni correzione) |
| AN3 | Anomalie del giorno corrente | **Escluse** (giornata in corso); la chat "anomalie di oggi" esistente resta com'è |
| AN4 | Provider AI per la spiegazione | `AI:AnomalieLLM`, vuoto = `AI:UseLLM`, `none` = senza AI; ripiego sul testo dai fatti |
| AN5 | Metodo della previsione | Tassi per giorno della settimana × fattore del mese, massimo con le assenze pianificate (§5). Niente modelli opachi |
| AN6 | Riepilogo per CompanyAdmin | Solo i reparti che amministra; studio e CompanyPowerAdmin vedono tutta l'azienda |
| AN7 | Export PDF delle giornate da sistemare | Report dei job esistente (`ReportService`), nuovo tipo `GiornateDaSistemare` |

## 8. File

| Livello | File |
|---|---|
| Regole | `nvxapp.server.service/ClientServer-Service/GestionePresenze/_utility/GestionePresenzeAnomalie.cs` (anomalie e previsione, solo repository) |
| Testo AI | `.../_utility/GestionePresenzeAnomalieWriter.cs` |
| Servizio/API | `.../GestionePresenze/Dip_AnomalieService/` + `nvxapp.server/Controllers/GestionePresenze/Dip_AnomalieController.cs` |
| Riepilogo | `.../GestionePresenze/Dip_AnomalieService/AttendanceTrackingDigestProvider.cs` |
| Chat | `.../ChatAiHandler/GestionePresenze/DaysToFixHandler.cs` |
| Report | `ReportService` (`ReportTipo.GiornateDaSistemare`) |
| Client | servizio `ClientServer-Service/GestionePresenze/Dip_Anomalie/`, pagina `pages/GestionePresenze/anomalie-page/`, widget `shared/components/GestionePresenze/widgets/days-to-fix-widget`, `absence-forecast-widget`, catalogo `widget-attendance-tracking.service.ts`, menu, route, `assets/i18n/attendance-tracking/{it,en}.json` |

## 9. Prova (11/11/2026)

DB usa e getta `nvx_test_atan`, server su porta 5333; azienda con Presenze, tre dipendenti (ruolo User) con profilo
DIURNO8H (09-13, 14-18, limiti ±15', pausa pranzo rilevata), timbrature di ottobre 2026 inserite apposta, calcolo con
`TimeSheet_Engine/Calculate`. **77 controlli ok**, tra cui:
- dispari (06/10: 3 timbrature), fuori orario (07/10: 07:00 prima delle 08:45), giornata vuota (08/10), tre ritardi di
  30' (ritardo ricorrente, media 30') con ore mancanti del motore, richiesta in attesa (15/10), sabato con timbrature
  (giorno non lavorativo); ferie del 16/10 non segnalate come giornata vuota; totali 7 da sistemare, 2 da verificare;
  filtro "solo da sistemare", filtro dipendente, periodo troppo lungo, utente semplice escluso, CompanyAdmin (vede i
  dipendenti del reparto predefinito che amministra, nessuno senza reparti amministrati), testi in inglese;
- giornate di novembre senza ricalcolo (`Init`): giornata vuota e non calcolata (widget 21 = 7 feriali x 3);
- spiegazione con l'AI (provider configurato) e ripiego senza AI (`AI:AnomalieLLM=none`) con i fatti;
- previsione su storico finto (Verdi assente tutti i lunedi' da novembre 2025): giornate dello storico, tasso medio,
  tasso del lunedi', fattore di novembre e previsione del 16/11 (3 attesi, 2 pianificate, statistica 2,4 = 3 x T(lun) x F(nov))
  confrontati con lo stesso calcolo fatto in Python sui dati del DB; picco; sabato senza attesi;
- riepilogo AI: sezione `presenze.anomalie` critica (12 giornate = 4 feriali x 3 dipendenti, solo ruolo User),
  assenze pianificate della settimana dopo (4); nessuna sezione per il CompanyAdmin senza reparti; riepilogo dello studio;
- report PDF "Giornate da sistemare" tramite job; comando chat "giornate da sistemare di ottobre 2026".

Nota: con giustificativi di giornata inseriti a mano (SQL) il motore segna comunque `Err_TimbratureMancanti` sulla giornata
di ferie senza timbrature: la regola lo riporta come "Errore del calcolo: timbrature mancanti" (non come giornata vuota).
Da verificare con i giustificativi inseriti dalle pagine.

---

## 10. Checklist

Legenda: ✅ svolto · ⏳ da fare · 🔍 da decidere

- ✅ `AT-AI.1` Regole deterministiche delle anomalie (§3), servizio e API, spiegazione con AI e ripiego senza AI — `GestionePresenzeAnomalie`, `GestionePresenzeAnomalieWriter`, `Dip_AnomalieService`/`Dip_AnomalieController` (`AnomalieGet`, `AnomalieSpiega`)
- ✅ `AT-AI.2` Giornate da sistemare: pagina `/attendanceanomalies` (menu "Anomalie presenze", CompanyPowerAdmin e CompanyAdmin), filtri, Calendario HR, report PDF tramite job (`ReportTipo.GiornateDaSistemare`)
- ✅ `AT-AI.3` Previsione delle assenze (§5): `PrevisioneAssenzeGet`, sezione "Previsione assenze" della pagina con il metodo e i tassi
- ✅ `AT-AI.4` Widget `attendance.daysToFix` e `attendance.absenceForecast`, comando chat/voce `DaysToFix` (+ route "anomalie presenze"), `AttendanceTrackingDigestProvider`
- ✅ Testi it/en (server resx e client)
- ✅ Verifica: `dotnet build` e `ng build --configuration development` ok, `has-pending-model-changes` = "No changes", prova su DB usa e getta (§9, 77 controlli insieme ad AT-CDG)
- 🔍 Soglie configurabili per azienda (AN1): richiede una migration, da fare quando il contesto sarà libero
