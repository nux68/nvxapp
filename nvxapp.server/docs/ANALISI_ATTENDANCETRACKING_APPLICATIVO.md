# Analisi – Applicativo Gestione Presenze (AttendanceTracking)

Branch: **AttendanceTracking_Dev** (riceve i merge da Infrastructure, si mergia in AttendanceTracking). Documento scritto nel ramo di lavoro `AttendanceTracking_Dev_analisi_applicativo`.
Documento generale: `ANALISI_INFRASTRUCTURE_NUOVI_MODULI.md` (§3.1, aree `AT-*` in §10.2). Dettagli delle singole aree: i documenti `ANALISI_ATTENDANCETRACKING_*` (§1.3).

Stato (25/11/2026): **modulo completo e in uso**. Il documento descrive il modulo **com'è oggi**, letto nel codice; non è un progetto da realizzare. Il lavoro emerso dall'analisi è nella checklist (§13).

---

## 1. Scopo del documento e come usarlo

### 1.1 A cosa serve

Dare in un posto solo il quadro del modulo Presenze: dove stanno le cose, quali regole valgono e perché, cosa dipende da cosa, dove sono le trappole. Lo usano soprattutto un agente "esperto presenze" (risposte, valutazione degli impatti, controllo delle modifiche) e gli sviluppatori. I documenti di dettaglio restano la fonte per le singole aree (formule, pause, turni...): qui si sintetizza e si rimanda.

**Il codice fa fede.** Ogni regola è stata letta nel codice e porta il riferimento `file:metodo` (niente numeri di riga, invecchiano). Dove un documento, il manuale o un commento dicono altro, vale il codice e la differenza è in §11. Convenzioni di lettura:

- **verificato** = letto nel codice (con riferimento); **sospetto** = letto nel codice ma l'effetto va provato; **da verificare** = non verificato (non sono stati avviati server, database o prove).
- Percorsi abbreviati: `SRV/` = `nvxapp.server.service/ClientServer-Service/GestionePresenze/`; `DAT/` = `nvxapp.server.data/`; `CTL/` = `nvxapp.server/Controllers/GestionePresenze/`; `CLI/` = `nvxapp.client/src/nvx/`; `CHAT/` = `nvxapp.server.service/ClientServer-Service/ChatAiHandler/GestionePresenze/`; `DOCS/` = `nvxapp.server/docs/`.
- Ruoli = codici di Infrastructure: `User`, `CompanyAdmin` (responsabile di reparto), `CompanyPowerAdmin` (ufficio del personale / HR). Un utente può avere più ruoli nella stessa azienda (mai dedurre il ruolo dal primo).

### 1.2 Dove cercare la risposta

| Domanda | Sezione |
|---|---|
| Come si calcola una giornata o una voce del cartellino | §5 (passi) e §6 (regole) |
| Chi può vedere o fare una cosa, e dove lo si controlla | §3 |
| Dove sta il codice di una funzione (pagina, servizio, entità, documento) | §7 |
| Cosa succede se cambio X | §10 |
| Cosa è noto che non torna (documenti, codice morto, rischi) | §11 |
| Cosa è da decidere o da fare | §12 e §13 |

### 1.3 Documenti di dettaglio

| Documento | Contenuto | Stato |
|---|---|---|
| `ANALISI_ATTENDANCETRACKING_PAUSE.md` | Pause pranzo (timbrata/automatica) e brevi; interpretazione della sequenza con gli intervalli di riconoscimento (ancore); formula unica di detrazione; modello `Par_OrarioPausa` | Implementata (ramo `_pause`). Aperti: fase 2 (buono pasto, soglie a scaglioni, voce "pausa detratta" nel PDF, stato "in pausa"), validazione intervalli nel client, prova su un mese reale |
| `ANALISI_ATTENDANCETRACKING_FORMULE.md` | Formule personalizzate JavaScript (Jint) nel calcolo giornata, profili formule per periodo, legacy RilevaFull, API della facciata `x`, AI formula | Implementata l'11/11. Aperti: lavori 46 (Ricalcola periodo) e 63 (prova su giorni bloccati), domande 4-5 (profilo predefinito, fase Ante) |
| `ANALISI_ATTENDANCETRACKING_EXPORT_AI.md` | Export verso le paghe: modello `Par_ExportCau`, documentazione, "ricetta" scritta dall'AI e file generato dal server, metadati JSON, matricola e codice fiscale, tipo Orario/Importo delle causali, `Verso` | Completata il 29/10 (43/43). Il §1 descrive lo stato precedente |
| `ANALISI_ATTENDANCETRACKING_TIMBRATURA.md` | Timbratura evoluta (AT-TIMB): QR rotante, geofencing, kiosk con PIN/badge, offline, terminali, reparto-attività | Mergiata. Aperto `AT-TIMB.6a` (prova su tablet reale) |
| `ANALISI_ATTENDANCETRACKING_TURNI.md` | Turni e pianificazione (AT-TURNI): modelli, coperture, scambio, impatto sul calcolo, turni multipli, report | Mergiata. Aperto `AT-TURNI.12` (catena dei merge) |
| `ANALISI_ATTENDANCETRACKING_SMARTWORKING.md` | Lavoro agile (AT-SW): accordi, calendario, giustificativo dedicato | Mergiata, nessun aperto |
| `ANALISI_ATTENDANCETRACKING_ANOMALIE_AI.md` | Anomalie delle timbrature, giornate da sistemare, previsione assenze (AT-AI) | Mergiata. Aperto AN1 (soglie configurabili, richiede migration) |
| `ANALISI_ATTENDANCETRACKING_CONTROLLO_GESTIONE.md` | Ore e costi per commessa, marginalità (AT-CDG) | Mergiata con scelte provvisorie: costo orario e ricavi non persistenti; nota spese non collegata alla commessa |
| `ANALISI_ATTENDANCETRACKING_WIDGET.md` | Widget della dashboard delle Presenze | **Datato** (§11, V1): descrive il widget "Richieste" che non è più nelle Presenze |
| `ANALISI_INFRASTRUCTURE_NUOVI_MODULI.md` | Mappa dei gruppi/applicativi, dati in Common (`AT-COM`), aree `AT-*` | Riferimento per le decisioni D1-D9 |
| `ANALISI_INFRASTRUCTURE_RICHIESTE.md` | Hub comune delle richieste (`Dip_GG_Richiesta` in Common, `RICH-AT.*`) | Implementata; l'elenco dei "problemi del codice attuale" (§2) è in parte superato (§11, V19) |
| `ANALISI_INFRASTRUCTURE_LICENZE.md` | Piani Free/Standard/Pro, limite utenti, `ILicenseGuard` | Fase 4 (inibizione delle funzioni) non fatta: nelle Presenze solo le formule richiedono Pro |
| `ANALISI_INFRASTRUCTURE_CHAT_AI.md`, `ANALISI_INFRASTRUCTURE_MCP.md` | Comandi chat/voce delle presenze e strumenti MCP | Implementati; MCP-44 (esclusi `CalcolaGiorno`, `DeleteClocking`) da decidere |
| `ANALISI_INFRASTRUCTURE_CANCELLAZIONE.md` | Cancellazione di azienda e utente | Per le Presenze manca il deleter (`ATT-INT.1`) |
| `ANALISI_INFRASTRUCTURE_WIDGET.md`, `ANALISI_INFRASTRUCTURE_SHOWCASE.md` | Dashboard e vetrina | Le Presenze usano i punti di estensione (§9) |
| `ANALISI_INFRASTRUCTURE_PERSONALIZZAZIONI.md` | Funzioni su misura per azienda | Solo analisi; nessun legame con le Presenze oggi |
| `piano-security.md` (radice del repo) | Autorizzazione solo lato client, piano di correzione | Analisi fatta, implementazione in pausa (§3.3) |
| `nvxapp.server.service/Manuals/attendance.<lingua>.md` e `attendance.guide.<lingua>.md` | Manuale breve (204 righe) e guida (880 righe), in PDF dalla sezione Assistenza | Non coprono lavoro agile, timbratura in sede/kiosk, formule, anomalie (§11, V15) |

---

## 2. Applicativo e schema

| Voce | Valore |
|---|---|
| `ApplicationType` | `AttendanceTracking = 2` (`DAT/Infrastructure/Tenancy/ApplicationType.cs`) |
| Schema | `tenant_<IdAzienda>_2` in multi-tenant, `public` in modalità singola |
| Contesto | `AttendanceTrackingDbContext` (55 `DbSet`), registrato in `Installers4AttendanceTracking.cs:InstallServices`; migration in `DAT/Migrations/AttendanceTracking`, storico `__EFMigrationsHistory_AttendanceTracking` |
| Migration attuali | `AttendanceTracking_Init` (12/11, rigenerata: il DB si ricrea), `AttendanceTracking_TurniMultipli`, `AttendanceTracking_TimbraturaReparto` |
| Prefissi tabelle | `Az_` (azienda, sedi, commesse), `Dip_` (dipendente e movimenti), `Par_` (parametri). Nessun prefisso proprio del modulo |
| Licenza | `AmbitoLicenza.AttendanceTracking`; listini "Piano base presenze" (studi) e "... aziende" con Free/Standard/Pro (`GestionePresenzeLicenseCatalogSeed.cs`); limite utenti predefinito 10/50/illimitati |
| Attivazione | `GestionePresenzeCompanyInitializer.cs` (priorità 20, `ActivateOnNewCompany = true`): ogni nuova azienda ha le Presenze attive, con sede, reparto, commessa e parametri di partenza |
| Codice del manuale | `attendance` |
| i18n | server `DAT/Localization/AttendanceTrackingMessages.resx` e `DAT/Localization/AttendanceTrackingMessages.en.resx`; client `nvxapp.client/src/assets/i18n/attendance-tracking/it.json` e `en.json`, chiavi `attendance.*` |
| Cartelle | server `Controllers/GestionePresenze` (58 controller), `ClientServer-Service/GestionePresenze`, `ClientServer-Service/ChatAiHandler/GestionePresenze`, `Entities/Tenant/GestionePresenze`, `Repositories/Tenant/GestionePresenze`; client `pages/GestionePresenze`, `ClientServer-Service/GestionePresenze`, `Utility/GestionePresenze`, `shared/components/GestionePresenze` |

### 2.1 Come i dati si dividono tra schemi

Il contesto delle Presenze lavora con `search_path = "tenant_<id>_2", "tenant_<id>_0", public` (`TenantSearchPathInterceptor.cs:SearchPath`): le tabelle comuni si usano **senza qualificatore** e sono mappate con `ExcludeFromMigrations` (`AttendanceTrackingDbContext.cs:Gen_PublicReferences_GestionePresenze`). Le crea Infrastructure (contesto Common).

| Dove | Cosa |
|---|---|
| `public` (di piattaforma) | `AspNetUsers`, `Company` (solo lettura, `ExcludeFromMigrations`) |
| Common `tenant_<id>_0` (sempre presente) | `Az_Anagrafica`, `Az_Cliente`, `Az_ClienteContatto`, `Dip_Anagrafica` (con `DataAssunzione`/`DataCessazione`), `Az_Sedi`, `Az_SediReparto`, `Par_Competenza`, `Dip_Competenza`, `Dip_RapportoAutovetture`, **`Dip_GG_Richiesta`** (usata anche dalle prenotazioni di Risorse aziendali). Scritture solo con i repository comuni (`DAT/Repositories/Tenant/Common/*CommonRepository.cs`) |
| `tenant_<id>_2` (Presenze) | tutto il resto: configurazione, commesse, rapporto di lavoro, parametri di calcolo, movimenti, risultati, export, turni, timbratura evoluta, lavoro agile, formule |

Le classi `*.Presenze.cs` (`Dip_Anagrafica.Presenze.cs`, `Az_Sedi.Presenze.cs`, `Az_SediReparto.Presenze.cs`, `Dip_GG_Richiesta.Presenze.cs`, `Dip_RapportoAutovetture.Presenze.cs`) sono `partial` e aggiungono le navigazioni verso le tabelle delle Presenze; le entità Common non conoscono le Presenze. Le date di assunzione/cessazione del dipendente comune sono tenute allineate ai rapporti di lavoro da `GestionePresenzeUserUtility.cs:AllineaDateDipendente` (assunzione = prima `DataAss`; cessazione = `DataLic` dell'ultimo rapporto, nulla se uno è aperto).

### 2.2 Modulo e licenze

- Il server nega l'accesso al contesto se l'applicativo non è attivo per l'azienda (`ApplicationDbContextBase.cs`); i comandi chat/MCP dell'applicativo compaiono solo con le Presenze attive (`IntentDefinition.Application`).
- **Livelli**: l'unica funzione che richiede il piano Pro è quella delle formule (`FormuleAccesso.cs:VerificaAsync`, `FormuleRunner.cs:PreparaRapportoAsync`; lato client `funzionalita-gestione-presenze.service.ts`). Senza Pro le formule non girano e i loro log restano come sono. Il limite utenti dell'azienda è quello della licenza Presenze (controllato in Infrastructure, `LicenseGuard.cs:CanAddUserAsync`). Con il sistema licenze spento non c'è nessun limite.

---

## 3. Ruoli e visibilità

### 3.1 Ruoli e funzioni (dal menu: `main-menu-attendance-tracking.service.ts`)

| Ruolo | Cosa fa |
|---|---|
| **User** (dipendente) | Timbra (`/timeclockuser`, in sede `/timbraturaqr`), chiede giustificativi e timbrature mancanti, note spese; vede il proprio calendario (`/usertimesheet`), il lavoro agile, i propri turni e scambi, le proprie richieste |
| **CompanyAdmin** (responsabile di reparto) | Richieste da approvare, calendario dei dipendenti dei reparti che amministra (`/admintimesheet`), personale presente, piano ferie, anomalie, accordi di lavoro agile, pianificazione turni, esame note spese, postazione QR e kiosk, schede dei dipendenti (`/userdepartmentlist`) |
| **CompanyPowerAdmin** (HR) | Tutto quanto sopra più: Calendario HR (`/poweradmintimesheet`), quadratura mensile, export causali, statistiche attività, **parametri** (giustificativi, causali, configurazione, orari, profili orari, festività, sedi/timbratura, turni, modelli export, tipi spesa, formule, profili formule), commesse, reparti, sedi, attività, competenze |
| Studio, centro, amministratori di piattaforma | Nessuna funzione propria: entrano nell'azienda "lavorando come" un suo utente; per loro c'è solo il wizard `/companyattendancewizard` (`RoleGuard4CompanyList`) |

Il `CompanyAdmin` riceve le richieste **solo** se nel reparto è indicato come amministratore con "Approva richieste" attivo (`Az_SediRepartoUser`).

### 3.2 Visibilità per reparto

`Az_SediRepartoUser` lega un utente a un reparto con quattro attributi: `UserInDepartment` (è un dipendente del reparto; `DataDal`/`DataAl` valgono solo per le attività consentite), `EnabledToAdmin` (amministra il reparto e ne vede i dipendenti), `EnabledToApproval` e `ApprovalZOrder` (approva le richieste e in che ordine).

- Assegnazione di partenza: `GestionePresenzeUserUtility.cs:AssegnaRepartoDefault` mette l'utente nel reparto predefinito della sede predefinita (admin: `EnabledToAdmin` + `EnabledToApproval`, in coda agli approvatori; user: `UserInDepartment`). Si cambia dalla pagina del reparto (`Az_SediRepartoService.cs:Az_SediRepartoPut`).
- Il `CompanyPowerAdmin` vede tutti i dipendenti dell'azienda, il `CompanyAdmin` solo quelli dei reparti con `EnabledToAdmin`. Il filtro è applicato in: `TimeSheet_ExportService.cs:DipendentiAmministrati` (export e PDF), `Dip_GG_NotaSpesaService.cs:DipendentiAmministrati`, `Dip_AnomalieService.cs`, `Dip_WidgetService.cs`, `ReportService.cs`, `AttendanceTrackingDigestProvider.cs`, e nell'elenco reparti dell'admin (`Az_SediRepartoService.cs:Get4Admin`).
- Gli approvatori di una richiesta sono i `EnabledToAdmin && EnabledToApproval` dei reparti del richiedente, ordinati per `ApprovalZOrder` (`Dip_GG_RichiestaService.cs:Send`). La regola di approvazione è `Az_Cfg.ApprovazioneTipo` (§8.1).

### 3.3 Dove si applicano i permessi (importante)

| Livello | Come | Copertura |
|---|---|---|
| Client | `roles` delle voci di menu e `RoleGuard4*` nelle rotte (`route-attendance-tracking.service.ts`) | Tutte le pagine |
| Server, controller | solo `[Authorize]` (209 azioni, nessun ruolo né policy): `CTL/*Controller.cs` | Nessun controllo di ruolo |
| Server, servizi | controlli puntuali con `HaRuolo`, `IsInRoleAsync`, `DipendentiAmministrati` | **Solo** in: `Az_SediRepartoService`, `Dip_AnagraficaService`, `Dip_AnomalieService`, `Dip_GG_NotaSpesaService`, `Dip_GG_RichiestaService` (`VerificaPermesso`, solo per le azioni del client), `Dip_WidgetService`, `DipendentiImportService`, `ReportService`, `TimbraturaEvolutaService` (`VerificaAdmin`), `TimeSheet_ExportService`, `Par_TurnoService`/`Dip_TurnoService` (`TurniUtility.IsAdminAsync`), `Dip_AccordoSWService`, `Par_FormuleService`/`Par_ProfiloFormuleService`/`FormuleAccesso` (`CompanyPowerAdmin` + Pro), `Az_CommessaCdgService` |
| Chat e MCP | `IntentDefinition.Usable4Role` e `ChatPresenzeHelper.PuoAccedere`; i servizi non ricontrollano | Comandi della chat e dell'MCP |

**Tutto il resto non controlla il ruolo sul server**: parametri (`Par_*`), `TimeSheet_EngineService.cs:Calculate`, `Dip_PeriodoService.cs` (approvazione e riapertura del mese), `Dip_GG_Timbratura/Giustificativi/Causali` (inserimento per qualunque rapporto; **la cancellazione dal client invece è controllata** dal 25/11/2026, `ATT-SEC.2`: solo `CompanyPowerAdmin`, dipendente dell'azienda, mese non approvato, `GestionePresenzeUserUtility.cs:VerificaCancellazione`), contatori, anagrafica del rapporto di lavoro, commesse. Le richieste si possono fare "per un altro utente" (`IdAspNetUsers` nel modello) senza che il servizio verifichi che chi chiama lo amministri. È il quadro già descritto in `piano-security.md` (`[Authorize(Roles)]` + filtro per azienda); per le Presenze la matrice proposta coincide con quella del menu. Finché quel piano non è attuato, ogni nuovo endpoint va deciso caso per caso (§10).

---

## 4. Modello dati

Perno di tutto è il **rapporto di lavoro** (`Dip_RapportoLavoro`): un dipendente comune (`Dip_Anagrafica`, Common) ha 1..N rapporti (assunzioni successive); movimenti, risultati, profili e richieste puntano al rapporto con `IdDip_RapportoLavoro`. Le entità stanno in `DAT/Entities/Tenant/GestionePresenze` (e `Common`); il contesto che le configura è `AttendanceTrackingDbContext.cs` (non ci sono indici oltre a quelli indicati qui).

### 4.1 Organizzazione e configurazione

| Entità | Scopo | Legami |
|---|---|---|
| `Az_Cfg` (Presenze) | Una riga per azienda: `ApprovazioneTipo` (`SigleAdmin`, `AllAdmin`, `AllAdminHierarchy`), cliente predefinito | `Az_Anagrafica` (Common) |
| `Az_Sedi`, `Az_SediReparto` (Common, `*.Presenze.cs`) | Sedi e reparti (reparto figlio di reparto con `IdAz_SediReparto`) | Usati anche da Compliance, HR, Assets |
| `Az_SediRepartoUser` | Utente del reparto / amministratore / approvatore con ordine (§3.2) | `IdAspNetUsers` (utente di piattaforma) |
| `Az_Commessa` → `Az_SubCommessa` → `Az_SubCommessaAttivita` | Commessa (cliente comune `Az_Cliente`), sottocommessa con periodo, attività della sottocommessa. Ogni timbratura è su un'attività | `Par_Attivita` (colori), `Az_SubCommessaUser` (utente), `Az_SubCommessaSediReparto` (reparto) |
| `Par_Attivita`, `Par_AttivitaCompetenza`, `Az_SediAttivita`, `Az_SediRepartoAttivita` | Attività e loro abilitazione per competenza, sede, reparto | `Par_Competenza`/`Dip_Competenza` (Common) |
| `Az_SediTimbratura` | Per sede: coordinate e raggio, regola geofence, QR (segreto, durata, obbligatorio), kiosk (token), `DifferitaMaxOre` | Unica per sede |
| `Par_Festivita` | Calendario festività dell'azienda (`DataRicorrente`, `DataSingola`, `Pasquetta`) | Letto da motore e anomalie |

### 4.2 Dipendente e rapporto di lavoro

| Entità | Scopo | Legami |
|---|---|---|
| `Dip_Anagrafica` (Common + `Dip_Anagrafica.Presenze.cs`) | Dati anagrafici (cognome, nome, matricola, codice fiscale, email, date di assunzione/cessazione, attivo) | `IdAspNetUsers`; 1..N `Dip_RapportoLavoro` |
| `Dip_RapportoLavoro` | `DataAss`, `DataLic`, `TipoContratto` (`FullTime`, `PartTime`), `OreSettimanaliContratto`, `Ccnl`, `DestinazioneEccedenze` (`Pagamento`, `BancaOre`), ultima attività usata | Pivot dei movimenti |
| `Dip_ProfiloOrario` | Periodo `Dal`-`Al` con il profilo orario applicato e il giorno di partenza del ciclo | `Par_ProfiloOrario`. Nessun controllo di sovrapposizioni o buchi (§11) |
| `Dip_ProfiloFormule` | Periodo con il profilo formule (solo Pro) | `Par_ProfiloFormule` |
| `Dip_Rapporto_Giustificativi_Maturazione` | Ore maturate al mese per giustificativo (contatori) | `Par_Giustificativi` |
| `Dip_Contatori_Riporto` | Riporto di apertura anno per giustificativo, sempre manuale | `Par_Giustificativi` |
| `Dip_TimbraturaPin` | PIN (hash HMAC, 4-8 cifre, unico per azienda) e badge per il kiosk | `Dip_Anagrafica` |
| `Dip_RapportoAutovetture` (Common) | Autovettura del dipendente con rimborso al km; la gestisce anche Risorse aziendali | Note spese |
| `Dip_AccordoSW` | Accordo di lavoro agile: periodo, giorni massimi a settimana, giorni consentiti (maschera `1111100`), richiesta e data di firma | Rapporto |

### 4.3 Parametri di calcolo

| Entità | Scopo |
|---|---|
| `Par_Orario` + `Par_OrarioIntervalloHH` + `Par_OrarioPausa` | Orario: tipo `IntervalloOrario` (coppie Dalle/Alle con limiti sinistro/destro, arrotondamento e verso, flag `*_Use_4_Match`) oppure `MonteOre` (`Hh_Teo_MonteOre`); pause pranzo/brevi (modalità `Rilevata`/`Automatica`, durata, soglia, finestra, quota retribuita, causale) |
| `Par_ProfiloOrario` + `Par_ProfiloOrarioGG` | Profilo `Settimanale` o `Ciclico` (`NumGiorniCiclo`); per giorno uno o più orari ordinati per `ZOrder`; orario festivo (`idPar_Orario_Festivo`); regole delle ore in più (§6) e causali del profilo |
| `Par_Causali` | Codici che finiscono nel cartellino e nell'export; `Tipo` `Orario` o `Importo` (non modificabile se usata, `GestionePresenzeTipoValore.cs`) |
| `Par_Giustificativi` | Ciò che si chiede/inserisce: tipo di input (`InteraGiornate`, `Intervallo`), `Tipo` (ore o importo), causale, `Segno` (`Up`, `Down`, `Neutral`), contatore (`NoContatore`, `Contatore`, `ContatoreConAvviso`, `ContatoreConBlocco`), flag `BancaOre`, `AutorizzaStraordinario`, `AnnullaPausa`, `LavoroAgile`, `VisualizzaInPianoFerie` |
| `Par_Formule`, `Par_ProfiloFormule`, `Par_ProfiloFormuleDett` | Formule JavaScript e profili (ordine, periodo, fase `Giorno` o `Finale`, sospesa); log in `Dip_GG_FormuleLog` |
| `Par_Turno`, `Az_SediRepartoTurno` | Modello di turno (orario o riposo, colore) e turni abilitati per reparto con copertura minima e giorni |
| `Par_TipoSpesa` | Tipi di spesa delle note spese (generica, chilometrica) |
| `Par_Arrotondamenti` | **Non usata dal calcolo** (§11, V6) |

### 4.4 Movimenti, risultati, export

| Entità | Scopo | Note |
|---|---|---|
| `Dip_GG_Timbratura` | `Timbratura` (valore corrente), `TimbraturaOriginale` (mai toccata), `TimbraturaArrotondata` (calcolata), `GiornoCompetenza`, `TimbraturaTipo` (`Entrata`, `Uscita`, `SenzaVerso`), `RichiestaStato`, attività | Alla timbratura il verso è `SenzaVerso`: lo assegna il motore. `Dip_GG_TimbraturaInfo` ne conserva l'origine (app, QR, kiosk), posizione, esito geofence, QR slot |
| `Dip_GG_Giustificativi` | Un record per **giorno**: giustificativo, ore o importo, stato della richiesta, richiesta collegata | Creati da `Send` (una riga per giorno lavorativo) o inseriti direttamente. `IdJustificationType` non è usato (§11) |
| `Dip_GG_Causali` | Risultato per giorno e causale: `Valore` (ore, `TimeOnly`) o `Importo` | Riscritte dal motore a ogni ricalcolo del giorno (§5.3, §11 S1) |
| `Dip_GG_Richiesta` (Common) | Richiesta da approvare: tipo, periodo, `Dati` JSON per tipo, stato, cronologia approvatori (JSON), revoca con propria cronologia | §8.1 |
| `Dip_GG_NotaSpesa` (+ `_Allegato`, byte nel DB) | Spesa con importo, km, autovettura, stato | Legata alla richiesta di tipo `NotaSpesa` |
| `Dip_Turno`, `Dip_TurnoScambio` | Turno pianificato (unico per rapporto-giorno-turno; più turni al giorno ammessi) e proposta di scambio con stato | Il turno sostituisce l'orario del profilo (§5.2) |
| `Dip_GG_Result` | Una riga per rapporto e giorno: `HH_Teo`, `HH_Lav`, minuti (lavorabili, assenze, ordinarie, eccedenza, notturna, supplementare, straordinari, non riconosciute, non autorizzate, pausa), `Festivo`, `BancaOre`, `Stato` a bit (§5.6) | Nessun indice unico (rapporto, giorno): §11 V17 |
| `Dip_Periodo` | Cartellino mensile per rapporto (unico per `Anno`, `Mese`): totali in minuti, `Stato` (`Provvisorio`, `DaApprovare`, `Approvato`), `Min_Riconosciute` deciso dall'HR, approvazione | Fissa il blocco del mese (§5.7) |
| `Par_ExportCau`, `Par_ExportCau_Causali`, `Par_ExportCau_Docs` | Modello di export verso le paghe: codice di output per causale con tipo elaborazione (giornaliera/mensile), unità (ore/giorni/importo) e `Verso`; documentazione (byte nel DB, testo estratto) | Letti da `TimeSheet_ExportService.cs:Export` |
| `My_template1` | Tabella modello di sviluppo, senza uso funzionale | §11 V7 |

---

## 5. Flusso e motore

### 5.1 Dalla timbratura all'export paghe

```
timbratura (app / QR / kiosk / richiesta / HR / chat / MCP)
   -> Dip_GG_Timbratura  (verso SenzaVerso, GiornoCompetenza = data civile)
   -> ricalcolo del giorno: TimeSheet_EngineService.Calculate (in background)
         -> Dip_GG_Result (giorno) + Dip_GG_Causali + Dip_Periodo (mese, Provvisorio)
   -> HR: Calendario HR, correzioni, "Giornate da sistemare" (anomalie)
   -> Quadratura mensile: DaApprovare -> Approvato  (giorni del mese bloccati, Locked)
   -> Export causali: TimeSheet_ExportService.Export legge Dip_GG_Causali -> file per le paghe
```

I **valori che escono** (cartellino, export, report, chat, widget) sono sempre quelli **salvati** da un calcolo: nessuno ricalcola al volo. Se il calcolo non è stato rifatto dopo una modifica, il cartellino mostra il vecchio valore.

### 5.2 Quando si ricalcola (verificato)

Ogni scrittura di un movimento chiama `Calculate` per il solo giorno toccato (a meno di `ExcludeRicalc`, che il motore usa nei salvataggi interni per non andare in ciclo):

| Evento | Chi chiama |
|---|---|
| Timbratura semplice, evoluta, QR, kiosk, modifica, cancellazione | `Dip_GG_TimbraturaService.cs` (`Stamp`, `Dip_GG_TimbraturaPut`, `Dip_GG_TimbraturaDelete`), `TimbraturaEvolutaService.cs:Registra` |
| Giustificativo, causale | `Dip_GG_GiustificativiService.cs`, `Dip_GG_CausaliService.cs` (`Put`, `Delete`) |
| Richiesta inviata, approvata, rifiutata, revocata, cancellata | `Dip_GG_RichiestaService.cs` (`Send`, `SetState`, `Dip_GG_RichiestaPut/Delete`) |
| Riga del risultato | `Dip_GG_ResultService.cs` (`Dip_GG_ResultPut`, `Dip_GG_Result_Init`) |
| Tetto ore riconosciute del mese | `Dip_PeriodoService.cs:Dip_Periodo_SetRiconosciute` (tutto il mese) |
| Turni assegnati o scambiati | `Dip_TurnoService.cs:RicalcolaAsync`, `Dip_TurnoScambioService.cs` |
| "Calcola" dell'HR (periodo, utenti, con le quattro opzioni) e comando chat `CalcolaGiorno` | `TimeSheet_EngineController` → `TimeSheet_EngineService.cs:Calculate` |

**Non scatta nessun ricalcolo** quando cambiano orari, profili orari, festività, giustificativi/causali (parametri), maturazioni, profili formule o l'assegnazione di un profilo (nessun servizio `Par_*` chiama il motore). I giorni restano come sono finché non si ricalcola; per i mesi approvati il blocco comunque impedisce il ricalcolo (§5.7).

Le opzioni del ricalcolo (`TimeSheet_CalculateModel`: `Approva_Richieste_Timbrature`, `Approva_Richieste_Giustificativo`, `Genera_Timbrature_Mancanti`, `Genera_Giustificativo_Assenza`) sono **tutte false** nei ricalcoli automatici: solo il "Calcola" dell'HR e la chat le attivano, e valgono solo nel periodo richiesto.

`Calculate` risponde subito e lavora in un thread in background (`ServiceBase.cs:RunInBackground`, `Task.Run`: non è Hangfire, non sopravvive a un riavvio, nessun tentativo ripetuto); l'avanzamento arriva via SignalR (`LongJobProgress`, tipo `TimeSheet_Engine.Calculate`) e a fine lavoro `NotificaCartellini` avvisa i dipendenti ricalcolati e gli amministratori (chi ha avviato il ricalcolo è escluso, tranne se arriva da MCP). **Non c'è serializzazione**: due ricalcoli sullo stesso rapporto possono sovrapporsi (§11 V17).

### 5.3 Passi del calcolo

`Calculate` carica i dati con `Get_Timesheet_AllData` (`Get_OrariSchema_4User` + `Dip_GG_AllData_AllData`) per i **mesi interi** coinvolti, estesi alle settimane a cavallo (`RangeCaricamento`), e per ogni rapporto del dipendente esegue `CalcolaRapporto`.

| # | Passo | Dove | Cosa |
|---|---|---|---|
| 0 | Orario del giorno | `Get_OrariSchema_4User` | Rapporti sovrapposti al periodo; `Dip_ProfiloOrario` che contiene il giorno (il primo); `NumGiorno` = giorno della settimana 1..7 (settimanale) oppure `((giorno − Dal).Days + NumGiornoPartenzaCiclo) % NumGiorniCiclo` (ciclico); orari del giorno per `ZOrder`; se il giorno è in `Par_Festivita` e il profilo ha l'orario festivo, vale quello; i turni pianificati sostituiscono tutto (più turni: orario composto in memoria con id negativo, `ComponiOrario`) |
| 1 | Periodi di conteggio | `CalcolaRapporto` | Periodi (giorno, settimana lun-dom, mese del profilo) toccati dal ricalcolo, e di conseguenza tutti i giorni da ricalcolare |
| 2 | Formule: preparazione | `FormuleRunner.cs:PreparaRapportoAsync` | Solo piano Pro: profili formule e formule dei giorni |
| 3a | Opzioni | `CalcolaGiorno` | (se richieste) approva richieste di giustificativo e di timbratura del giorno, **escluse le autorizzazioni alle ore in più** (`IsAutorizzazioneStraordinario`); genera timbrature mancanti (`GeneraTimbratureMancanti`, solo giorni prima di oggi) |
| 3b | Verso e arrotondamento | `AssegnaVersoTimbrature` | Azzera il verso, interpreta le timbrature valide con le ancore dell'orario (`GestionePresenzeSequenza.cs:Interpreta`), assegna `Entrata`/`Uscita` e arrotonda quelle abbinate. Esiti: `Err_TimbratureMancanti`, `Err_5`, `Warning_5` |
| 3c | Ore teoriche e lavorate | `CalcolaOreTeoriche`, `CalcolaOreLavorate` → `HH_Teo`, `HH_Lav` | Teorico = somma delle coppie ± effetto delle pause; lavorato = segmenti entrata/uscita arrotondati al netto della pausa (`SegmentiLavorati`, `GestionePresenzePause.cs:Applica`) |
| 3d | Assenza generata | `GeneraGiustificativoAssenza` | (se richiesta, giorni prima di oggi) crea o aggiorna il giustificativo "assenza ingiustificata" del profilo per la differenza teorico − lavorato |
| 3e | Causali del giorno | `GeneraCausali` | (A) giustificativi validi → causali e lavorabile; (B) ore per coppia, pausa, ore notturne fuori coppia; (C) ordinarie = lavorato entro il lavorabile; (D) eccedenza del giorno con arrotondamento e franchigia. Mette le causali in `_causaliGiorno`; stato `Err_2` (lavorato < lavorabile) o `Warning_1` (ore in più) |
| 3f | Formule fase `Giorno` | `FormuleRunner.cs:Esegui` | Dopo le causali del giorno, prima della qualifica di periodo |
| 4 | Qualifica del periodo | `GestionePresenzeEccedenze.cs:QualificaPeriodo` | Ore in più → straordinario, supplementare, eccedenti non straordinarie, per periodo (§6.6) |
| 4b | Supplementare oltre limite | `SeparaSupplementare` | Solo part-time, per settimana lun-dom: entro `SupplLimitePerc` delle ore contrattuali è "standard", oltre è "eccedente" |
| 4c | Tetto dell'HR | `ApplicaTetto` | Se `Dip_Periodo.Min_Riconosciute` è impostato (mese non approvato), toglie le ore qualificate dagli ultimi giorni (diventano "non riconosciute") |
| 4d | Causali delle ore in più | `AggiungiCausaliEccedenze` | Causali del profilo (straordinario, notturno, festivo, supplementare, eccedenti); con `DestinazioneEccedenze = BancaOre` tutto va su `IdCausale_BancaOre` |
| 4e | Formule fase `Finale` | `FormuleRunner.cs:Esegui` | Lavorano sulle causali già qualificate |
| 4f | Scrittura causali | `ScriviCausali` | Marca da cancellare **tutte** le causali del giorno, riusa quelle con lo stesso codice e aggiunge le nuove (anche a importo) |
| 5 | Cartellino mensile | `AggiornaCartellinoMese` | Somma i giorni del mese in `Dip_Periodo`; stato `Provvisorio` o `DaApprovare` (§5.7) |
| 6 | Salvataggio | `SaveData` | Solo gli elementi con hash cambiato, nell'ordine: `Dip_GG_Result`, timbrature, giustificativi, richieste, causali, log formule. I salvataggi passano dai servizi con `ExcludeRicalc` e come sottoprocesso (quindi **senza** il controllo di rapporto/mese approvato) |

Il giorno di un mese approvato (`GG_ResultStato.Locked`) viene saltato da `CalcolaGiorno` e mantiene la propria qualifica.

### 5.4 Interpretazione della sequenza (ancore)

`GestionePresenzeSequenza.cs:Ancore` trasforma le coppie dell'orario in **ancore** (ora attesa di entrata/uscita) con intervallo di riconoscimento `[ora − Limite_SX, ora + Limite_DX]`; si creano solo per i capi con `*_Use_4_Match`. `Interpreta` procede così: (1) scarta le timbrature doppie (a meno di `MinutiDoppia = 2` dalla precedente) con avviso; (2) abbina in ordine le timbrature alle ancore dentro l'intervallo (massimo numero di ancore, a parità minima distanza); (3) le ancore vuote prendono una timbratura fuori intervallo della zona adiacente, con avviso; le coppie facoltative (pausa pranzo automatica) possono restare vuote; (4) le timbrature rimaste sono uscite intermedie (U poi E) dentro una coppia, lavoro fuori orario (E poi U) altrove; un numero dispari nello stesso tratto rende il giorno non interpretabile. Un'ancora vuota non ammessa lascia il giorno **incompleto**: nessun verso, nessuna ora conteggiata, `Err_TimbratureMancanti`.

### 5.5 Pause

`GestionePresenzePause.cs`: **pranzo** (una per orario): rilevata = stacchi tra segmenti lavorati che toccano la finestra; dovuta = `DurataMin` se il lavorato lordo supera `SogliaMin`; con modalità `Automatica` (e senza giustificativo "pausa non fruita" nel giorno) detrae `max(0, dovuta − rilevata − RetribuitaMin)` a partire dall'inizio della finestra; con `Rilevata` solo segnala. **Breve**: stacchi dentro le coppie fuori dalla finestra del pranzo. In entrambe i minuti timbrati fino a `RetribuitaMin` tornano lavorati. Avvisi: `Warning_3` (pausa insufficiente o non fruita), `Warning_4` (oltre la durata massima). Il teorico del giorno cambia con `VariazioneTeorico` (pranzo dentro una coppia toglie la parte non retribuita; nello stacco aggiunge quella retribuita). Dettaglio in `ANALISI_ATTENDANCETRACKING_PAUSE.md`.

### 5.6 Stato del giorno (`GG_ResultStato`, maschera a bit)

Principali: `Init` (non calcolato), `OK`, `Locked` (mese approvato), `Warning`, `Err`. Dettagli usati dal motore: `Err_TimbratureMancanti` (giorno incompleto), `Err_2` (lavorato minore del lavorabile), `Err_4` (anomalia segnalata da una formula), `Err_5` (sequenza non interpretabile), `Warning_1` (ore in più), `Warning_2` (formula), `Warning_3` e `Warning_4` (pause), `Warning_5` (timbratura doppia ignorata o fuori intervallo). Gli helper sono in `Dip_GG_Result_Helper.cs` (`AddDetail`, `Combine`). Un giorno ancora `Init` più vecchio di ieri è mostrato come `Warning` da `Dip_GG_ResultService.cs:Dip_GG_Result_Get_4Calculation`; le righe `Dip_GG_Result` mancanti si creano a richiesta (`Dip_GG_Result_Init`).

### 5.7 Periodo, chiusura e blocchi

- Stato del mese (`Dip_Periodo.Stato`): **`Provvisorio`** se l'ultimo giorno del mese (con il conteggio settimanale: la domenica della sua ultima settimana) non è ancora passato **o** ci sono ore in più provvisorie o autorizzazioni da approvare; **`DaApprovare`** altrimenti (`AggiornaCartellinoMese`); **`Approvato`** solo per azione dell'HR (`Dip_PeriodoService.cs:Dip_Periodo_SetStato`, accettata solo da `DaApprovare`: "Quadratura mensile").
- **Approvazione** = `BloccaGiorni`: i `Dip_GG_Result` del mese esistenti prendono il bit `Locked`. **Riapertura** → `DaApprovare` e giorni sbloccati. Un mese approvato non viene più aggiornato da `AggiornaCartellinoMese`.
- **Movimenti fuori rapporto o in mese approvato**: `GestionePresenzeUserUtility.cs:VerificaGiorniNelRapporto` rifiuta i giorni prima di `DataAss`, dopo `DataLic` e nei mesi `Approvato`. È chiamata da inserimenti e modifiche di timbrature, giustificativi, causali, richieste (invio e approvazione), kiosk/QR, turni, chat `RegisterClocking`. Le cancellazioni dal client (`Dip_GG_TimbraturaDelete`, `Dip_GG_GiustificativiDelete`, `Dip_GG_CausaliDelete`, `Dip_GG_RichiestaDelete`) usano `VerificaCancellazione` (ruolo, azienda e mese approvato; vedi V10), che **non** controlla il rapporto di lavoro: il client permette di togliere i movimenti residui fuori rapporto. Né l'una né l'altra sono chiamate dai salvataggi e dalle cancellazioni in sottoprocesso del motore. `SetState` (rifiuto, annullamento, revoca approvata) cancella i dettagli con `Dip_GG_Richiesta_Canc_Dettaglio` senza controllare il mese approvato (§11 V10).
- Il **blocco vale per i giorni già presenti in `Dip_GG_Result` al momento dell'approvazione**; un giorno senza riga non risulta bloccato (da verificare se può capitare: il cartellino normalmente li crea tutti).
- Il **tetto delle ore** (`Min_Riconosciute`) si cambia solo a mese non approvato e provoca il ricalcolo del mese.

### 5.8 Export verso le paghe

`TimeSheet_ExportService.cs:Export` (job in background, tipo `TimeSheet_Engine.Export`): i dipendenti esportabili sono quelli che l'utente può vedere (`DipendentiEsportabili`); `PreparaMetadati` legge `Dip_GG_Causali` del periodo con `Get_Timesheet_AllData` e `ExportMetadatiBuilder.cs:Costruisci` raggruppa per codice di output del modello: somma i minuti con il segno del `Verso` (`Sommatoria`/`Sottrazione`), per giorno o per mese (`TipoElaborazione`), come ore a 2 decimali, giorni (un giorno vale ±1 per il segno della somma) o importo; valori uguali a zero scartati, negativi segnalati nell avviso. Se il modello ha documentazione, `Par_ExportCau_RicettaService` ottiene la "ricetta" dall'AI (salvata o rigenerata) e `ExportRicettaRenderer.cs:Scrivi` produce TXT, CSV, tracciato fisso, XML, JSON o XLSX; se la ricetta non si può ottenere il file è il **JSON dei dati** con un avviso (i dati personali non vanno all'AI, solo documentazione ed esempio sintetico). **L'export non controlla lo stato del mese**: esporta anche mesi provvisori, non calcolati o con giorni in errore (§11 V21). Prima dell'export, "Giornate da sistemare" (§8.6) è lo strumento di controllo.

---

## 6. Regole e casi limite

Solo regole lette nel codice. `R*` sono etichette per rimandare dal resto del documento.

### 6.1 Timbrature e giorno di competenza

| # | Regola | Dove |
|---|---|---|
| R1 | Il **giorno di competenza è la data civile** della timbratura: una timbratura dopo la mezzanotte cade nel giorno dopo. Vale per timbratura semplice, evoluta/QR/kiosk e richiesta di timbratura. Solo l'HR può cambiarlo a mano (campo "giorno di competenza" nel dialog della timbratura) | `Dip_GG_TimbraturaService.cs:Stamp`, `TimbraturaEvolutaService.cs:Registra`, `Dip_GG_RichiestaService.cs:Add_Dip_GG_Timbratura`; client `edit-dip-gg-timbratura-dialog` |
| R2 | La timbratura nasce **senza verso** (`SenzaVerso`) e con `TimbraturaArrotondata` uguale all'ora: entrata/uscita e arrotondamento li assegna il motore a ogni ricalcolo del giorno | `AssegnaVersoTimbrature` |
| R3 | Contano solo le timbrature `Diretta` o `Approvata`; quelle di una richiesta ancora in attesa non entrano nel calcolo | `AssegnaVersoTimbrature`, `Get_Timbrature_Giorno` |
| R4 | **Timbrature dispari o mancanti**: se un'ancora non facoltativa resta vuota il giorno è incompleto (nessun verso, nessuna ora, `Err_TimbratureMancanti`); sequenza non interpretabile → `Err_5`. Con l'opzione "genera timbrature mancanti" il calcolo crea timbrature `Diretta` sull'ora attesa, solo nei giorni passati e solo per le ancore non ammesse vuote | `GestionePresenzeSequenza.cs:Interpreta`, `TimeSheet_EngineService.cs:GeneraTimbratureMancanti` |
| R5 | **Doppie**: una timbratura a meno di 2 minuti dalla precedente valida è ignorata, con `Warning_5` | `GestionePresenzeSequenza.cs` (`MinutiDoppia`) |
| R6 | Una timbratura **semplice** può avere i secondi come inviati dal client; quella **evolutiva** (QR, kiosk, "in sede") è troncata al minuto | `Dip_GG_TimbraturaService.cs:Stamp`, `TimbraturaEvolutaService.cs:Stamp` |
| R7 | Ogni timbratura è su un'**attività**: se non indicata si propone quella dell'ultima timbratura, poi quella del rapporto, poi la prima consentita (o quella predefinita dell'azienda). Dal client sono ammesse solo le attività consentite: quelle delle sottocommesse dove l'utente è collegato direttamente o tramite un suo reparto | `GestionePresenzeUserUtility.cs:GetIdAz_SubCommessaAttivita_Proposta`, `:GetIdAz_SubCommessaAttivita_Consentite`, `:VerificaAttivitaConsentita` |
| R8 | Il rapporto di lavoro "corrente" delle operazioni del dipendente è **il primo trovato** in `Dip_RapportoLavoro` per quell'anagrafica, senza ordinamento né data (§11 V11) | `GestionePresenzeUserUtility.cs:Get_DipAna_DipRapp` |
| R9 | Al **primo accesso** `Get_DipAna_DipRapp(…, true)` può creare dati: anagrafica comune, reparto e competenza di default, rapporto con assunzione al primo del mese di creazione dell'azienda e profilo orario predefinito `DAY7H8` fino al 31/12/2099 | `GestionePresenzeUserUtility.cs:Get_DipAna_DipRapp` |

### 6.2 Orari, profili, festività, turni

| # | Regola | Dove |
|---|---|---|
| R10 | Il profilo del giorno è il primo `Dip_ProfiloOrario` con `Dal <= giorno <= Al`. **Senza profilo** il giorno non ha orario: teorico 0, le ore timbrate non sono contate e non c'è nessun errore | `Get_OrariSchema_4User`, `GeneraCausali` |
| R11 | Con più orari nello stesso giorno (`ZOrder`) conta **solo il primo** in teorico, verso, lavorato e causali; fanno eccezione i turni (orario composto) | `CalcolaOreTeoriche`, `CalcolaOreLavorate`, `AssegnaVersoTimbrature`, `GeneraCausali` |
| R12 | **Festività**: solo il calendario `Par_Festivita` (data ricorrente, data singola, Pasquetta) sostituisce l'orario con quello festivo del profilo. La domenica con `DomenicaFestiva` rende festivo il **risultato** (straordinario festivo) ma **non** cambia l'orario | `GestionePresenzeFestivita.cs:IsFestivita`, `GeneraCausali` |
| R13 | Un **turno pianificato** sostituisce l'orario del profilo, anche nei festivi; il riposo vale l'orario festivo del profilo (0 ore). Più turni compatibili nello stesso giorno → orario composto (fasce in ordine di inizio, pause di tutti) | `Get_OrariSchema_4User`, `ComponiOrario`, `TurniUtility.cs:VerificaGiorno` |
| R14 | Turni nello stesso giorno compatibili solo se: non ripetuti, nessuno è riposo o a monte ore, fasce non sovrapposte (contigue sì) | `TurniUtility.cs:VerificaGiorno` |
| R15 | **Orario a monte ore** (`MonteOre`): teorico = `Hh_Teo_MonteOre`; il lavorato è la somma delle timbrature del giorno lette come durata (ore:minuti), senza ancore né pause | `CalcolaOreTeoriche`, `CalcolaOreLavorate` |
| R16 | **Coppie a cavallo della mezzanotte**: ancore, fasce di causale e finestra della pausa le gestiscono (un'ora che precede la precedente è del giorno dopo), ma il **teorico le conta 0** perché scarta le durate negative; la pianificazione turni invece aggiunge 24 ore. Un turno 22:00-06:00 ha quindi teorico 0 nel motore e 480 minuti nella pagina dei turni (§11 V9) | `GestionePresenzeSequenza.cs:Ancore`, `GestionePresenzePause.cs:Finestra`, `CalcolaOreTeoriche`, `TurniUtility.cs:OrarioTesto` |
| R17 | La risoluzione dell'orario del giorno esiste **in tre copie**: il motore, `GestionePresenzeOreTeoriche` (usata da richieste, lavoro agile) e `GestionePresenzeAnomalie`. Vanno tenute allineate a mano | `Get_OrariSchema_4User`, `GestionePresenzeOreTeoriche.cs:GetOreTeoriche`, `GestionePresenzeAnomalie.cs` |

### 6.3 Arrotondamenti e flessibilità

| # | Regola | Dove |
|---|---|---|
| R18 | Si arrotondano **solo le timbrature abbinate a un'ancora**, con intervallo e verso (`Up`/`Down`) dell'ancora (`Dalle_/Alle_Arrotondamento`, `_Verso`). Le altre restano com'erano. `TimbraturaOriginale` non cambia mai | `GestionePresenzeSequenza.cs:Arrotonda`, `Roundings.cs` |
| R19 | **Flessibilità**: una timbratura è "dentro" se cade in `[ora − Limite_SX, ora + Limite_DX]` (orari assoluti, circolari su 24 ore). Fuori intervallo ma nella zona adiacente viene accettata con avviso (ritardo, anticipo). Senza `*_Use_4_Match` non c'è ancora: se nessun capo è ancora, tutto è lavoro fuori orario a coppie E/U e niente viene arrotondato | `GestionePresenzeSequenza.cs:Ancore`, `:Interpreta` |
| R20 | `Par_Arrotondamenti` **non è usata**: gli arrotondamenti vengono dalle coppie dell'orario | §11 V6 |

### 6.4 Pause

| # | Regola | Dove |
|---|---|---|
| R21 | Una pausa pranzo **automatica** rende facoltative le timbrature dello stacco tra le due coppie e detrae i minuti mancanti dal lavorato (§5.5); un giustificativo con `AnnullaPausa` nel giorno disattiva detrazione e avviso | `TimeSheet_EngineService.cs:AncoreFacoltative`, `:SegmentiLavorati`, `GestionePresenzePause.cs:Applica` |
| R22 | Il salvataggio di un orario controlla gli intervalli di riconoscimento delle coppie e le regole delle pause (errore dal server, non dal client) | `GestionePresenzeOrarioValidazione.cs:ControllaIntervalli`, `:ControllaPause` |

### 6.5 Giustificativi, causali, contatori

| # | Regola | Dove |
|---|---|---|
| R23 | Contano solo i giustificativi `Diretta`/`Approvata`. `Segno` `Up` aumenta il lavorabile, `Down` lo riduce e conta tra le assenze, `Neutral` non lo cambia. Con causale, il valore va sulla causale. Il lavorabile non scende sotto zero | `GeneraCausali` |
| R24 | Giustificativo **a importo**: l'importo va sulla causale a importo, nessun effetto sulle ore. Una causale a importo non si registra dalla chat | `GeneraCausali`, `RegisterCausaleHandler.cs` |
| R25 | `AutorizzaStraordinario` non ha causale: fissa il massimo di ore in più riconosciute nel giorno se il profilo ha `StraoSoloAutorizzato`; le richieste di autorizzazione non si approvano mai in automatico dal calcolo | `GeneraCausali`, `IsAutorizzazioneStraordinario` |
| R26 | **Giornata intera**: ore = teorico del giorno (`GestionePresenzeOreTeoriche`), saltando i giorni a teorico 0; senza profilo valgono le ore inviate. L'autorizzazione alle ore in più vale per tutti i giorni (anche riposo e festivi) e sempre per le ore indicate. I record per giorno sono creati **all'invio** e non seguono modifiche successive di profilo (da verificare) | `Dip_GG_RichiestaService.cs:OreGiustificativoPerGiorno`, `:Add_Dip_GG_Giustificativi` |
| R27 | **Contatori** (ferie, ROL...): maturato = riporto (solo manuale) + `OreMaturazione` × mesi del rapporto; per i giustificativi `BancaOre` il maturato è la somma di `Min_BancaOre` dei **cartellini approvati**; goduto = **tutti** i `Dip_GG_Giustificativi` di quel giustificativo, anche in attesa. Periodi: precedente, corrente, successivo, fine mese | `ContatoriService.cs:CalcolaMesi` |
| R28 | Alla richiesta: `Contatore` solo informativo; `ContatoreConAvviso` avvisa se il saldo a oggi è minore delle ore; `ContatoreConBlocco` rifiuta se il saldo futuro è minore. Il saldo è calcolato sul **mese della data iniziale** della richiesta | `Dip_GG_RichiestaService.cs:ValidaContatoriAsync` |
| R29 | Il riporto d'anno **non** si calcola da solo: lo inserisce l'HR (`IsManuale = true`) | `ContatoriService.cs:Riporto_Upsert` |
| R30 | Una causale già usata non cambia `Tipo` (ore/importo); causali e giustificativi orari non accettano causali a importo | `GestionePresenzeTipoValore.cs` |
| R31 | Le **causali** del giorno sono **riscritte** a ogni ricalcolo: quelle esistenti con lo stesso codice prendono il valore calcolato, le altre vengono cancellate. Una causale inserita a mano (`Dip_GG_CausaliPut`, chat `RegisterCausale`) subisce lo stesso trattamento (§11 S1) | `TimeSheet_EngineService.cs:ScriviCausali` |

### 6.6 Ore in più (straordinario, supplementare, banca ore)

| # | Regola | Dove |
|---|---|---|
| R32 | **Eccedenza del giorno** = lavorato − lavorabile. Se positiva: arrotondata **per difetto** a blocchi di `EccArrotondamento` (predefinito 15 minuti) e azzerata sotto `EccFranchigiaMin`. Se negativa (ore mancanti) resta com'è. Con `StraoSoloAutorizzato` si riconoscono solo le ore autorizzate; il resto è "non autorizzato" (con la quota "da autorizzare" se c'è una richiesta in attesa) | `GeneraCausali` |
| R33 | `EccedenzeAttive = false` sul profilo: nessuna qualifica, le ore in più restano come eccedenza del giorno | `CalcolaRapporto` |
| R34 | **Qualifica per periodo** (giorno, settimana lun-dom o mese, `TipoConteggio` del profilo): somma delle eccedenze (le ore mancanti compensano quelle in più); `X` = lavorato del periodo (+ assenze se `AssenzeContanoSoglia`); soglia = teorico del periodo (full-time, limitato da `StraoSogliaHHFullTime`/`SogliaSettimanaleFT` se impostati) o, part-time, soglia full-time del periodo (mensile: teorico riproporzionato); straordinario = `X − soglia` tra 0 e l'eccedenza; il resto è supplementare (part-time) o ore eccedenti non straordinarie (full-time). Le ore qualificate si attribuiscono partendo dall'**ultimo** giorno con ore in più | `GestionePresenzeEccedenze.cs:QualificaPeriodo`, `:SogliaPeriodo` |
| R35 | Un periodo **non ancora concluso** (ultimo giorno ≥ oggi) non qualifica niente: le ore in più restano `EccProvvisoria` e le causali di straordinario compaiono solo a periodo chiuso (con conteggio mensile, dopo la fine del mese) | `QualificaPeriodo` |
| R36 | Straordinario **notturno** = parte dell'eccedenza svolta fuori dalle coppie nella fascia `NotturnoDalle`-`NotturnoAlle` (predefinita 22:00-06:00); **festivo** se il giorno è festivo (§R12) | `GeneraCausali` (`OreNotturne`), `GestionePresenzeEccedenze.cs:AssegnaStraordinario` |
| R37 | Supplementare part-time oltre `SupplLimitePerc` delle ore settimanali contrattuali (predefinito 25%): "eccedente", in ordine cronologico nella settimana | `GestionePresenzeEccedenze.cs:SeparaSupplementare` |
| R38 | **Tetto dell'HR**: `Min_Riconosciute` riduce le ore qualificate del mese togliendole dagli ultimi giorni (da `StraoFestNott` a `EccOrd`); le ore tolte sono "non riconosciute" | `GestionePresenzeEccedenze.cs:ApplicaTetto` |
| R39 | **Banca ore**: con `DestinazioneEccedenze = BancaOre` le ore qualificate vanno sulla causale del profilo `IdCausale_BancaOre` e il giorno è marcato `BancaOre`; il recupero è un giustificativo con flag `BancaOre` | `AggiungiCausaliEccedenze`, `ContatoriService.cs:CalcolaMesi` |

### 6.7 Richieste e approvazioni

| # | Regola | Dove |
|---|---|---|
| R40 | `Send`: servono approvatori configurati nel reparto del richiedente; senza, errore `NoApproverEnabled` **anche per l'inserimento diretto dell'HR** (`FromHR`). L'HR (`FromHR`) crea la richiesta già `Approvata` | `Dip_GG_RichiestaService.cs:Send` |
| R41 | La richiesta crea subito i dettagli (timbratura, giustificativi per giorno, nota spese) con lo stato della richiesta. **Rifiuto, cancellazione o revoca approvata cancellano fisicamente** timbratura e giustificativi; le spese restano con lo stato della nota | `Dip_GG_Richiesta_Canc_Dettaglio` |
| R42 | Strategie (`Az_Cfg.ApprovazioneTipo`): `SigleAdmin` (basta uno), `AllAdmin` (tutti), `AllAdminHierarchy` (in ordine di `ApprovalZOrder`, ognuno solo dopo il precedente). Con `AllAdmin*` un solo rifiuto rifiuta la richiesta; l'HR (`FromHR`) può forzare | `SetState`, `VerificaPermesso` |
| R43 | Permessi di `SetState` (solo azioni del client, non il sottoprocesso del motore): revoca e cancellazione = richiedente o admin; approvare/rifiutare = approvatore della fase corrente, con `SigleAdmin` anche un admin dell'azienda; `FromHR` solo admin | `Dip_GG_RichiestaService.cs:VerificaPermesso` |
| R44 | La **revoca** (annullamento di una richiesta approvata) ha una cronologia propria; si chiede una volta sola; ritirabile finché in attesa; revoca approvata → dettagli cancellati e, per gli scambi di turno, turni ripristinati | `SetState`, `RichiestaRegole.PuoChiedereAnnullamento` |
| R45 | Il ricalcolo "approva richieste" non tocca le autorizzazioni alle ore in più; approva giorno per giorno solo richieste di un giorno solo (`Data == DataA`) | `TimeSheet_EngineService.cs:ApprovaRichiesta` |
| R46 | `ApprovazioneInCorso` è lo stato intermedio; `ParzialmenteApprovata` esiste ma **non viene mai assegnato** (§11 V5) | `SetState` |

### 6.8 Timbratura evoluta

| # | Regola | Dove |
|---|---|---|
| R47 | **QR**: codice `sede.slot.firma` con firma HMAC del segreto della sede, slot di `QrDurataSecondi` (30); vale lo slot corrente o il precedente; **una sola volta per dipendente** (indice unico sede+slot+rapporto, con compensazione se due timbrature partono insieme) | `TimbraturaEvolutaService.cs:VerificaCodiceQr`, `:Registra`; `AttendanceTrackingDbContext.cs:Gen_TimbraturaEvoluta` |
| R48 | **Geofence**: distanza dal punto della sede ≤ raggio + precisione dichiarata dal dispositivo (contata al massimo per 100 m); regola `Segnala` (avviso) o `Blocca`; nei giorni di **lavoro agile approvato** il controllo è escluso | `TimbraturaEvolutaService.cs:Stamp`, `LavoroAgileUtility.cs:IsGiornoLavoroAgile` |
| R49 | **Offline**: vale l'ora del dispositivo se non oltre `DifferitaMaxOre` della sede (24 senza configurazione) né nel futuro oltre 5 minuti; la timbratura è marcata `Differita` | `TimbraturaEvolutaService.cs:Stamp` |
| R50 | **Kiosk**: dispositivo registrato per sede con token e un amministratore collegato; il dipendente si identifica con PIN (4-8 cifre, unico per azienda, assegnato solo dall'HR) o badge; 5 errori in un minuto bloccano per un minuto (conteggio **in memoria**, non condiviso tra istanze né dopo un riavvio); l'uscita dal terminale richiede la password di **un qualunque** `CompanyPowerAdmin` dell'azienda | `TimbraturaEvolutaService.cs:KioskDipendenteAsync`, `:PinPut`, `:TerminaleSblocca` |
| R51 | Il badge è confrontato e salvato **in chiaro**; il PIN è un hash HMAC con chiave `Timbratura:PinKey` (altrimenti la chiave JWT) | `TimbraturaEvolutaService.cs:PinHash`, `:KioskDipendenteAsync` |

### 6.9 Lavoro agile e turni

| # | Regola | Dove |
|---|---|---|
| R52 | Richiesta di un giorno di lavoro agile (giustificativo con flag `LavoroAgile`): serve un accordo valido per quel giorno, **firmato** (presa visione confermata), giorno consentito dalla maschera, giorno non già richiesto, limite settimanale `MaxGiorniSettimana`. Il controllo è anche sulle richieste dell'HR | `LavoroAgileUtility.cs:ValidaRichiestaAsync`, `Dip_GG_RichiestaService.cs:Send` |
| R53 | Un turno cambiato o tolto **annulla** le proposte di scambio ancora in attesa del collega; l'assegnazione dei turni rifiuta giorni fuori rapporto o in mese approvato (con avviso, senza interrompere le altre righe) | `Dip_TurnoService.cs:Put`, `:ChiudiScambiAperti` |
| R54 | Approvato uno scambio, i turni si scambiano (`IDip_TurnoScambioHandler.ApplicaAsync`); rifiutato/annullato lo stato dello scambio si aggiorna; revoca approvata → turni come prima | `Dip_GG_RichiestaService.cs:SetState`, `Dip_TurnoScambioService.cs` |

### 6.10 Formule

| # | Regola | Dove |
|---|---|---|
| R55 | Le formule sono JavaScript eseguito con Jint sul server: timeout 500 ms, 50.000 istruzioni, 16 MB. Un errore **non ferma il calcolo**: va nel log (`Dip_GG_FormuleLog`) e alza `Warning_2`; un'anomalia dichiarata dalla formula alza `Err_4` | `FormuleScriptHost.cs`, `FormuleRunner.cs:Esegui` |
| R56 | Fasi: `Giorno` (dopo le causali del giorno) e `Finale` (dopo la qualifica, prima della scrittura). Salvare una formula con errori di sintassi è **rifiutato**. Solo `CompanyPowerAdmin` con piano Pro | `Par_FormuleService.cs:Par_FormulePut`, `FormuleAccesso.cs` |

---

## 7. Mappa funzionale

Pagine in `CLI/pages/GestionePresenze/<pagina>`; servizi client in `CLI/ClientServer-Service/GestionePresenze/<Area>`; server `SRV/<Servizio>/` e `CTL/<Servizio>Controller.cs`; le entità sono quelle del §4.

| Area | Pagine client | Servizio server | Entità principali | Documento |
|---|---|---|---|---|
| Timbratura semplice | `time-clock-user-page` (`/timeclockuser`) | `Dip_GG_TimbraturaService` (`Stamp`, `PrepareStamp`) | `Dip_GG_Timbratura` | TIMBRATURA |
| Timbratura evoluta | `timbratura-qr-page`, `-postazione-page`, `-kiosk-page`, `-pin-page`, `-sede-page`; `terminale.service.ts`, `timbratura-offline.service.ts` | `TimbraturaEvolutaService` | `Az_SediTimbratura`, `Dip_GG_TimbraturaInfo`, `Dip_TimbraturaPin` | TIMBRATURA |
| Cartellino | `time-sheet-user-page`, `time-sheet-admin-page`, `time-sheet-power-admin-page` (Calendario HR); `time-sheet.service.ts` | `TimeSheet_EngineService`, `Dip_GG_ResultService`, `Dip_GG_GiustificativiService`, `Dip_GG_CausaliService` | `Dip_GG_Result`, `Dip_GG_Causali`, `Dip_GG_Giustificativi` | §5; PAUSE, FORMULE |
| Quadratura e chiusura mese | `quadratura-mensile-page` | `Dip_PeriodoService` | `Dip_Periodo` | §5.7 |
| Richieste | `request-justification-user-page`, `request-clocking-user-page`, `nota-spesa-request-user-page`; pagine comuni `/attendancerequests`, `/attendancerequestsadmin` (Infrastructure) | `Dip_GG_RichiestaService`, `AtRichiestaTipoHandler` | `Dip_GG_Richiesta` (Common) | RICHIESTE |
| Giustificativi e causali (parametri) | `justification-*`, `causali-*` | `Par_GiustificativiService`, `Par_CausaliService` | `Par_Giustificativi`, `Par_Causali` | §6.5 |
| Orari, profili, festività | `orari-*`, `profilo-orario-*`, `festivita-*` | `Par_OrarioService`, `Par_OrarioIntervalloHHService`, `Par_ProfiloOrarioService`, `Par_ProfiloOrarioGGService`, `Par_FestivitaService`, `Dip_ProfiloOrarioService` | `Par_Orario*`, `Par_ProfiloOrario*`, `Dip_ProfiloOrario`, `Par_Festivita` | PAUSE |
| Contatori e piano ferie | `vacation-plan-page`; scheda contatori in `user-department-edit-page` | `ContatoriService`, `VacationPlanService`, `Dip_Rapporto_Giustificativi_MaturazioneService` | `Dip_Rapporto_Giustificativi_Maturazione`, `Dip_Contatori_Riporto` | §6.5 |
| Personale presente, statistiche | `present-staff-page`, `activity-statistics-page` | `PresentStaffService`, `ActivityStatisticsService` | `Dip_GG_Timbratura` | – |
| Export paghe | `export-causali-page`, `export-cau-list-page`, `export-cau-edit-page` | `TimeSheet_ExportService`, `Par_ExportCauService`, `Par_ExportCau_CausaliService`, `Par_ExportCau_DocsService`, `Par_ExportCau_RicettaService`, `ExportMetadatiBuilder`, `ExportRicettaRenderer` | `Par_ExportCau*` | EXPORT_AI |
| Nota spese | `nota-spese-user-page`, `nota-spese-admin-page`, `tipo-spesa-*` | `Dip_GG_NotaSpesaService`, `Par_TipoSpesaService`, `Dip_RapportoAutovettureService` | `Dip_GG_NotaSpesa`, `Par_TipoSpesa`, `Dip_RapportoAutovetture` | – |
| Turni | `turno-list-page`, `turno-edit-page`, `turno-coperture-page`, `pianificazione-turni-page`, `miei-turni-page` | `Par_TurnoService`, `Dip_TurnoService`, `Dip_TurnoScambioService`, `TurniUtility`, `TurniReportPdf` | `Par_Turno`, `Az_SediRepartoTurno`, `Dip_Turno`, `Dip_TurnoScambio` | TURNI |
| Lavoro agile | `accordi-sw-list-page`, `accordo-sw-edit-page`, `lavoro-agile-page` | `Dip_AccordoSWService`, `LavoroAgileUtility` | `Dip_AccordoSW` | SMARTWORKING |
| Anomalie e previsione | `anomalie-page` | `Dip_AnomalieService`, `GestionePresenzeAnomalie`, `GestionePresenzeAnomalieWriter` | (nessuna scrittura) | ANOMALIE_AI |
| Commesse e controllo di gestione | `commessa-*`, `cdg-commesse-page`, `activity-*` | `Az_Commessa*Service`, `Az_SubCommessa*Service`, `Az_CommessaCdgService`, `Par_AttivitaService` | `Az_Commessa`, `Az_SubCommessa*`, `Par_Attivita` | CONTROLLO_GESTIONE |
| Organizzazione e dipendenti | `department-*`, `az-sedi-*`, `user-department-*` (lista, scheda, wizard), `competence-*`, `company-cfg-page`, `company-attendance-wizard-page`, importazione da file | `Az_SediRepartoService`, `Az_SediService`, `Dip_AnagraficaService`, `Dip_RapportoLavoroService`, `Dip_CompetenzaService`, `Az_CfgService`, `DipendentiImportService` | `Az_Sedi*`, `Az_SediRepartoUser`, `Dip_Anagrafica`, `Dip_RapportoLavoro`, `Az_Cfg` | §3 |
| Formule | `formule-*`, `profilo-formule-*`, scheda "Profilo formule" del dipendente | `Par_FormuleService`, `Par_ProfiloFormuleService`, `Dip_ProfiloFormuleService`, `FormuleEngine/*` | `Par_Formule`, `Par_ProfiloFormule*`, `Dip_ProfiloFormule`, `Dip_GG_FormuleLog` | FORMULE |
| Report PDF | `report-fab.service.ts` | `ReportService` (`ReportPdf`, `ReportPdfWriter`) | – | §8.4 |
| Dashboard | widget in `shared/components/GestionePresenze/widgets` | `Dip_WidgetService`, `Dip_AnomalieService`, `Az_CommessaCdgService` | – | WIDGET |

Menu, rotte, widget, vetrina e interfaccia delle richieste sono dichiarati in cinque file del client `CLI/Utility/infrastructure/*-attendance-tracking.service.ts` (§9).

---

## 8. Richieste, notifiche, job, report, assistente

### 8.1 Richieste e approvazioni

Tipi (`TipoRichiesta`): `Timbratura`, `Giustificativo`, `NotaSpesa`, `ScambioTurno` (Presenze) e `PrenotazioneRisorsa` (Risorse aziendali); `ApprovazioneStraordinario` esiste ma non si crea mai (l'autorizzazione è un giustificativo con `AutorizzaStraordinario`, §11 V5). Stati (`StatoRichiesta`): `Diretta` (inserimento senza approvazione), `Immessa`, `ApprovazioneInCorso`, `Approvata`, `Rifiutata`, `Cancellata` (più `ParzialmenteApprovata`, mai assegnato). La **cronologia** degli approvatori sta nel JSON `RichiestaApprovazioneData` (e `RevocaApprovazioneData` per l'annullamento); `RevocaStato` guida la seconda fase.

Il motore dei tipi delle Presenze è `Dip_GG_RichiestaService` (`Send`, `SetState`, `NotaSpesaUpdate`, `GetAll4User`, `GetAll4Admin`, `RichiestaCount`). L'elenco e i pulsanti nelle pagine comuni li costruisce Infrastructure (`RequestService`) interrogando `AtRichiestaTipoHandler` (strategia dell'azienda, testi delle righe, azioni che chiamano `SetState`). Approvare dal calendario o dall'elenco comune passa dallo stesso `SetState`.

### 8.2 Notifiche

- Aggiornamento di pagine aperte via SignalR: `RichiestaNotifier`, `GiustificativiNotifier` (e `RequestNotifier` di Infrastructure per l'hub); fine ricalcolo `NotificaCartellini`.
- Il solo uso di `INotificationSender` è nello scambio turni (`Dip_TurnoScambioService.cs:Notifica`). Le Presenze **non dichiarano categorie di notifica proprie**, non registrano scadenze (`IDeadlineRegistry`) e non pubblicano eventi nel calendario (`ICalendarEventProvider`): da decidere se serve (§12).

### 8.3 Job e lavori in background

Tutti con `RunInBackground` (thread in processo, avanzamento via SignalR, file in `NVXSystem.SalvaFileJob`, pulizia `JobFileCleanupService`): ricalcolo (`TimeSheet_Engine.Calculate`), export paghe (`...Export`), PDF cartellino utente/admin (`...ExportPdf_4User/4Admin`), report PDF (`Report.Pdf`), importazione dipendenti (`DipendentiImport.Esegui`), report dei turni (`Turni.ReportPdf`) — elencati in `GestionePresenze_JobType.cs`. **Hangfire non è usato** dalle Presenze (la configurazione `Hangfire:UseHangfire` esiste ma non c'è nessun job registrato). Servizi periodici delle Presenze: nessuno; il riepilogo AI usa `AttendanceTrackingDigestProvider` dentro `DigestMaintenanceService` (Infrastructure).

### 8.4 Report ed export

`ReportService.cs:ReportExportPdf` (`ReportTipo`: attività totali e per dipendente, piano ferie, quadratura mensile per reparto, note spese per tipo e per dipendente, giornate da sistemare, marginalità commesse), cartellino PDF (`TimeSheetPdfWriter`), report dei turni; solo i dipendenti visibili all'utente (§3.2). Export paghe: §5.8.

### 8.5 Chat AI, comandi vocali, MCP

- Comandi in `CHAT/*Handler.cs` (24): lettura (`WhoIsPresent`, `ClockingsOfDay`, `ClockingAnomalies`, `DaysToFix`, `HoursWorked`, `LeaveBalance`, `PendingRequests`, `VacationPlanQuery`, `EmployeeList`, `EmployeeTimeSheet`, `PrintReport`), self-service (`ClockNow`, `RequestLeave`, `RequestClocking`, `ClockingsOfDay`, `HoursWorked`, `LeaveBalance`), HR (`RegisterClocking`, `UpdateClocking`, `DeleteClocking`, `RegisterJustification`, `RegisterJustificationAmount`, `RegisterJustificationPeriod`, `RegisterCausale`, `CalcolaGiorno`, `ApproveRequests`), navigazione (`OpenTimeSheetPowerAdmin`, rotte in `AttendanceTrackingRouteProvider`) e i comandi "apri entità" (`OpenableEntities.cs`). Ruolo dichiarato in `Usable4Role`; i servizi non ricontrollano (§3.3).
- **MCP**: nome dello strumento = `attendance_` + nome del comando in snake case (`McpToolNaming.ToolName`): è un contratto con i client AI. Pubblicati 21: `who_is_present`, `clockings_of_day`, `clocking_anomalies`, `days_to_fix`, `hours_worked`, `leave_balance`, `pending_requests`, `vacation_plan_query`, `employee_list`, `employee_time_sheet`, `print_report` (link temporaneo al PDF), `clock_now`, `request_leave`, `request_clocking` (modifiche personali), `register_clocking`, `update_clocking`, `register_justification`, `register_justification_amount`, `register_justification_period`, `register_causale`, `approve_requests` (amministrative). **Esclusi**: `CalcolaGiorno`, `DeleteClocking` (`McpAccess = Excluded`) e la navigazione. Limiti solo via MCP: 31 giorni per periodo e 20 richieste per approvazione (`ChatPresenzeHelper.cs`); un dipendente per chiamata; le richieste create via MCP portano il marcatore di provenienza nel corpo; un ricalcolo avviato da MCP avvisa anche chi lo ha lanciato.
- Frasi ed esempi in `Examples`; prove con `tools/ChatAiPhraseTest` (doc CHAT_AI).

### 8.6 Anomalie e giornate da sistemare

`GestionePresenzeAnomalie.cs:Analizza` (solo regole sui dati salvati, esclusi oggi, il futuro e i giorni `Locked`): timbrature dispari, giornata vuota, errore di calcolo, non calcolata, richieste in attesa, fuori orario (tolleranza 30 minuti), giorno non lavorativo, pausa mancante o eccessiva (lavoro continuo oltre 360 minuti, pausa minima 10, massima 120), ritardi ricorrenti (3 nel periodo), con spiegazione in linguaggio naturale dall'AI **solo dai fatti** (`GestionePresenzeAnomalieWriter`, `AI:AnomalieLLM`); previsione delle assenze (storico 365 giorni, minimo 20 giornate). Pagina `/attendanceanomalies`, widget `attendance.daysToFix` e `attendance.absenceForecast`, comando `DaysToFix`.

### 8.7 Widget

Dichiarati in `widget-attendance-tracking.service.ts`: `attendance.presentToday` (admin, predefinito), `attendance.daysToFix`, `attendance.absenceForecast` (admin), `attendance.jobHours` (HR). Il widget delle richieste è oggi di Infrastructure (`infrastructure.requests`).

### 8.8 Smart working

Accordo (`Dip_AccordoSW`) con firma tramite `IAcknowledgementService` (fonte `at.accordo-sw`; con il Documentale diventa il verbale di accordo), calendario del lavoro agile (`Dip_AccordoSWService.cs:Calendario`), richiesta del giorno come giustificativo `LavoroAgile` con i controlli di R52; giustificativo e causale `SW` (neutro) creati nei parametri di default e al primo uso (`LavoroAgileUtility.cs:GetOrCreateGiustificativoAsync`).

### 8.9 Controllo di gestione

`Az_CommessaCdgService.cs:CdgGet` (calcolo puro in `CdgCalcolo.cs:Calcola`) prende le ore per sottocommessa da `ActivityStatisticsService` (segmenti tra timbrature con attività, non dai risultati del motore; giornate in errore o non calcolate escluse e segnalate), le moltiplica per il costo orario e aggiunge le note spese ripartite in proporzione alle ore del mese; ricavi e costo orario sono impostati nella pagina `/jobmargins`, **non salvati** (ricordati dal browser): scelta provvisoria in attesa di campi persistenti (CONTROLLO_GESTIONE).

### 8.10 Importazioni

`DipendentiImportService.cs`: elenco di persone estratto con l'AI da un file (testo, documento, foglio, immagine; massimo 5 MB e 500 dipendenti), correzione nel client, inserimento in job come il wizard "Nuovo dipendente" (utente, anagrafica, rapporto, profilo orario, competenze). **Il contenuto del file (nomi, email, codice fiscale) va al provider AI configurato.** Solo `CompanyAdmin` e `CompanyPowerAdmin`. Il wizard è `nuovo-dipendente.service.ts` / `user-department-wizard-page`.

---

## 9. Dipendenze e integrazioni

### 9.1 Infrastructure e Presenze

Infrastructure **non ha riferimenti diretti alla logica delle Presenze**. Riferimenti verificati (nessuno altera il comportamento):

- `Account/Models/UserCompanyModel.cs` ha due `using` verso `GestionePresenze.*.Models` non utilizzati; altri riferimenti sono commenti (`AccountService.cs`, `ICompanyInitializer.cs`).
- Licenze: `LicenseGuard.cs`, `LicenseService.cs`, `LicenseDefaults.cs` nominano `AmbitoLicenza.AttendanceTracking` perché il **limite utenti** è definito solo per le Presenze (policy, `CompanyUserLimitAsync`).
- `McpToolService.cs:McpToolNaming` associa il prefisso `attendance` all'applicativo.
- Il progetto host registra il contesto e `ITimeSheet_EngineService_OnlyCalculate` in `nvxapp.server/Utility/Installers4AttendanceTracking.cs`; i servizi delle Presenze si registrano da soli per scansione dell'assembly. `OnlyCalculate` esiste per **spezzare il ciclo di dipendenze** tra i servizi che ricalcolano e il motore (risolve `ITimeSheet_EngineService` al momento dell'uso).

### 9.2 Punti di estensione usati dalle Presenze

| Punto di estensione (Infrastructure) | Implementazione delle Presenze | Note |
|---|---|---|
| `ICompanyInitializer` | `GestionePresenzeCompanyInitializer` (priorità 20, attiva sulle nuove aziende) | Crea struttura aziendale e, **solo se non esistono causali**, i parametri di partenza (`GestionePresenzeParametriDefault.cs:InitParametri`: causali, giustificativi, orari `DIURNO8H`/`CONT8H`/`RIPOSO`, profilo `DAY7H8`, modello export, tipi spesa, festività, turni) |
| `IUserInitializer` | `GestionePresenzeUserInitializer` (priorità 20) | Anagrafica, rapporto e reparto del nuovo utente |
| `ILicenseCatalogSeed` | `GestionePresenzeLicenseCatalogSeed` | Listini iniziali |
| `IRichiestaTipoHandler` | `AtRichiestaTipoHandler` | Tipi delle Presenze nelle pagine comuni; l'unico altro estensore è Risorse aziendali |
| (cablato) `IDip_TurnoScambioHandler` | `Dip_TurnoScambioHandler` | `SetState` lo chiama per `ScambioTurno`; non c'è un punto di estensione generico per i tipi |
| `IDigestProvider` | `AttendanceTrackingDigestProvider` | Riepilogo AI (anomalie, richieste) |
| `UserManualMarkdownProvider` | `AttendanceTrackingUserManualProvider` | Manuale `attendance` |
| `IRouteProvider`, `ICommandHandler` (via `BaseCommandHandler`), `IOpenableEntity` | `AttendanceTrackingRouteProvider`, `CHAT/*Handler`, `OpenableEntities.cs` | Chat, voce, MCP |
| `ICommonEntityUsage` | `AttendanceTrackingCommonEntityUsage` | Un cliente con commesse non si cancella |
| `IAcknowledgementService` (uso) | accordo di lavoro agile, fonte `at.accordo-sw` | Con il Documentale diventa firma del verbale |
| `ILongJobNotifier`, `IJobNotifier`, `INotificationSender` (uso) | job, avvisi, scambio turni | §8.2-8.3 |
| Client: `iMainMenuService`, provider di rotte, `iWidgetProvider`, vetrina, `iRequestUiProvider` | `main-menu-`, `route-`, `widget-`, `showcase-`, `request-ui-attendance-tracking.service.ts` | In Infrastructure sono stub vuoti; i file delle Presenze sono dello stesso nome nel ramo |

**Non implementati** (verificato): `IApplicationDataDeleter` (cancellazione dati: §13 `ATT-INT.1`), `IStorageUsageProvider` (gli allegati di note spese e documentazione export stanno nel DB e non entrano in nessuna quota), `IDeadlineRegistry`/scadenzario, `ICalendarEventProvider`.

### 9.3 Cosa le Presenze usano di Infrastructure

`IAccountService` (ruoli e utenti: reparti, import, richieste), `ILlmClient` (import dipendenti, ricetta dell'export, formule AI, spiegazione anomalie), `ILicenseGuard` (Pro per le formule), `ICommonCompanyUtility` e `ICommonDipendenteUtility` (azienda e anagrafica comuni), `IUserCompanyRepository`, i repository comuni di Common, `RichiestaRegole`/`RequestService`, `LlmClient`, `NVXSystem.SalvaFileJob`. Chiavi di configurazione: `AI:UseLLM`, `AI:AnomalieLLM`, `AI:ExportLLM`, `AI:FormuleLLM`, `AI:ExportMaxChars`, `AI:ExportMaxTokens`, `AI:AnomalieMaxTokens`, `AI:ImportMaxChars`, `Timbratura:PinKey`.

### 9.4 Altri moduli

| Modulo | Legame |
|---|---|
| Risorse aziendali (Assets) | Gestisce `Dip_RapportoAutovetture` (Common); le Presenze lo leggono per i rimborsi chilometrici delle note spese. Usa `Dip_GG_Richiesta` (Common) per le prenotazioni: le Presenze filtrano per `IdDip_RapportoLavoro != null` e tipi propri |
| Risorse Umane | Legge date di assunzione e cessazione dal dipendente comune (le tiene allineate `AllineaDateDipendente`); dati del rapporto esposti dallo strumento `attendance_employee_list` |
| CRM | Il cliente (`Az_Cliente`) è comune: le commesse delle Presenze lo usano; `AttendanceTrackingCommonEntityUsage` ne blocca la cancellazione |
| Documentale | Firma dell'accordo di lavoro agile (§8.8) |
| Compliance | Nessun legame diretto (sedi e reparti comuni) |
| AllFunction | Nella catena dei merge le Presenze precedono CRM, Documentale, HR, Compliance, Assets |

---

## 10. Guida agli impatti

| Se tocchi | Controlla | Trappole note |
|---|---|---|
| **Il motore** (`TimeSheet_EngineService.cs`) | `GestionePresenzeOreTeoriche.cs` e `GestionePresenzeAnomalie.cs` (copie della risoluzione orario, R17); `GestionePresenzeEccedenze.cs`, `GestionePresenzePause.cs`, `GestionePresenzeSequenza.cs`; `FormulaContext.cs` (la facciata `x` legge `FormulaDatiGiorno`); chi legge `Dip_GG_Result` (`ReportService`, `TimeSheet_ExportService` PDF, `PresentStaffService`, `ActivityStatisticsService`, `Dip_WidgetService`, chat `HoursWorked`/`EmployeeTimeSheet`/`ClockingsOfDay`, client `time-sheet.service.ts`) | Un campo nuovo di `Dip_GG_Result` o `Dip_Periodo` richiede migration (e hash in `SaveData`). I ricalcoli sono in background senza lock. I giorni `Locked` non si ricalcolano. Provare su un mese reale con orari con e senza pausa, part-time, turni, mese approvato |
| **Causali, giustificativi, profilo orario** (parametri) | `Par_ExportCau_Causali` (le righe del modello puntano alle causali), `Par_ProfiloOrario.IdCausale_*`, `Par_Orario*.IdCausale_*`, `Par_OrarioPausa.IdCausale_Pausa`, codici nelle formule (`FormulaCodici`), `GestionePresenzeParametriDefault.cs` (codici fissi: `ORD1`, `STRAO`, `SW`...), `LavoroAgileUtility.cs` (crea `SW` al primo uso) | **Nessun ricalcolo** automatico dopo la modifica (§5.2): i giorni passati restano com'erano. `Tipo` di una causale usata non cambia |
| **Richieste e approvazioni** | `Dip_GG_RichiestaService.cs` (`Send`, `SetState`, `VerificaPermesso`, `Dip_GG_Richiesta_Canc_Dettaglio`), `AtRichiestaTipoHandler.cs`, `RequestService`/`RichiestaRegole` (Infrastructure), `request-ui-attendance-tracking.service.ts`, chat `ApproveRequestsHandler`/`PendingRequestsHandler`, widget `infrastructure.requests`, Risorse aziendali (stessa tabella) | `SetState` fa tre cose insieme: stato, dettagli (cancellazione fisica), ricalcolo. Il sottoprocesso del motore **salta** `VerificaPermesso`. Il tipo `ScambioTurno` passa da un handler cablato. Nuovo tipo di richiesta = enum in Common (Infrastructure_Dev) + handler |
| **Migration o entità** | Entità di Common (`Dip_Anagrafica`, `Az_Sedi`, `Dip_GG_Richiesta`...) si modificano **solo su Infrastructure_Dev** e poi si portano; le Presenze hanno il proprio contesto e le proprie migration; `ExcludeFromMigrations` per le tabelle comuni; classi `*.Presenze.cs` per le navigazioni; `has-pending-model-changes`; una migration `Init` rigenerata ricrea il DB | Le classi con lo stesso nome in Common e in Presenze non devono coesistere (storia di `AT-COM`). Unico contesto per schema: più rami che aggiungono migration insieme confliggono |
| **Timbratura** (semplice, evoluta, richiesta, chat, MCP) | `Dip_GG_TimbraturaService.cs`, `TimbraturaEvolutaService.cs:Registra`, `Add_Dip_GG_Timbratura`, `RegisterClockingHandler.cs`, `UpdateClockingHandler.cs`, client `timbratura-offline.service.ts` | Tutti i percorsi devono dare `GiornoCompetenza`, attività, verifica rapporto/mese e ricalcolo allo stesso modo. Il chat `RegisterClocking` una volta saltava i controlli (corretto, MCP) |
| **Orari e profili** | `GestionePresenzeOrarioValidazione.cs`, turni (`Par_Turno.IdPar_Orario`), anomalie, `GetOreTeoriche`, pagine `orari-*`/`profilo-orario-*`, formule che leggono le fasce | Più `ZOrder` nel giorno: solo il primo; coppie a cavallo di mezzanotte: teorico 0 (R16); nessun controllo di sovrapposizioni dei periodi del profilo |
| **Ruoli e visibilità** | Menu e `RoleGuard4*` (client), servizi con `HaRuolo`/`DipendentiAmministrati` (§3.3), `Usable4Role`, `piano-security.md` | Un endpoint nuovo senza controllo nel servizio è raggiungibile da qualunque utente autenticato. Il `CompanyAdmin` vede solo i reparti con `EnabledToAdmin`: ripetere il filtro in ogni nuovo elenco |
| **Export** | `ExportMetadatiBuilder.cs` (contratto con le ricette AI salvate), `ExportRicettaRenderer.cs`, `Par_ExportCau*`, pagina `export-cau-edit-page`, documento EXPORT_AI | Cambiare i nomi dei campi del JSON dei metadati rompe le ricette già generate. L'export non guarda lo stato del mese (V21) |
| **Formule** | `FormuleCatalogo.cs`, `FormulaContext.cs` (attributo `FormulaApi`: il catalogo dei metodi arriva al client da `Par_FormuleService.cs:Metodi`), `FormuleScriptHost.cs`, licenza Pro | La facciata `x` è un contratto con le formule dei clienti |
| **Turni** | `TurniUtility.VerificaGiorno`, `Get_OrariSchema_4User` (DaySlot), `GestionePresenzeOreTeoriche`, `Dip_TurnoScambioHandler`, report turni | Scambio e revoca toccano i turni passando da `SetState` |
| **Menu, rotte, widget, vetrina, i18n** | Cinque file `*-attendance-tracking.service.ts`; `assets/i18n/attendance-tracking/*.json` e `AttendanceTrackingMessages.resx` e `AttendanceTrackingMessages.en.resx`; `attendance.<lingua>.md` e la guida (le rotte `@@ /...` devono esistere) | Il manuale non copre molte funzioni (V15). Le voci "Richieste" aprono pagine di Infrastructure |
| **Chat / MCP** | `CHAT/*Handler.cs`, `McpAccess`, `Usable4Role`, `ChatPresenzeHelper.cs` | Il nome del comando è il nome dello strumento MCP: cambiarlo rompe i client. Un comando nuovo senza `McpAccess` esplicito risulta `Excluded` (navigazione), `AdminWrite` (se chiede conferma) o `Read` (`IntentDefinition.EffectiveMcpAccess`) |
| **Cancellazione di azienda/utente** | `IApplicationDataDeleter` (manca per le Presenze), relazioni `Restrict` interne, FK verso `AspNetUsers` (`Az_SediRepartoUser`, `Az_SubCommessaUser`, `Dip_Anagrafica`) | Senza deleter togliere un utente da un'azienda lascia i dati delle presenze |

---

## 11. Incongruenze, codice non usato, aree senza documentazione

Il codice fa fede. **V** = verificato nel codice; **S** = sospetto, da provare.

### 11.1 Documenti, manuale e commenti

| # | Cosa | Riferimento |
|---|---|---|
| V1 | `ANALISI_ATTENDANCETRACKING_WIDGET.md` descrive il widget `attendance.requests` e `Dip_Widget/Widget_RichiesteGet`: **non esistono più** nelle Presenze (il widget è `infrastructure.requests`); la tabella ne elenca 5, il client ne dichiara 4 | `widget-attendance-tracking.service.ts`, `Dip_WidgetController.cs` |
| V2 | Non si usa **Hangfire** per i lavori delle Presenze (`RunInBackground`); è configurabile (`Hangfire:UseHangfire`) ma senza job registrati | `ServiceBase.cs:RunInBackground`, `Installers.cs` |
| V3 | I documenti SMARTWORKING, TIMBRATURA, TURNI citano migration `AttendanceTracking_SmartWorking/_Timbratura/_Turni`: oggi le migration sono tre (`Init` rigenerata il 12/11, `TurniMultipli`, `TimbraturaReparto`) | `DAT/Migrations/AttendanceTracking` |
| V4 | Il commento di `Dip_Contatori_Riporto` dice "calcolato automaticamente a fine anno"; il codice non lo calcola mai: il riporto è sempre manuale (`IsManuale = true`) | `Dip_Contatori_Riporto.cs`, `ContatoriService.cs:Riporto_Upsert` |
| V5 | `StatoRichiesta.ParzialmenteApprovata` e `TipoRichiesta.ApprovazioneStraordinario` non vengono mai assegnati/creati (l'autorizzazione alle ore in più è un giustificativo). La chat `ApproveRequests` mappa "straordinario/overtime" proprio su `ApprovazioneStraordinario`: filtro che non trova mai niente (**sospetto** sull'effetto) | `ApproveRequestsHandler.cs`, `Dip_GG_RichiestaService.cs:SetState` |
| V8 | `Par_Turno.cs` descrive `Dip_Turno` "uno per giorno": con `TurniMultipli` l'indice unico è (rapporto, data, turno). `Dip_GG_Giustificativi.IdJustificationType` non è mai letto né scritto | `Par_Turno.cs`, `AttendanceTrackingDbContext.cs`, `Dip_GG_Giustificativi.cs` |
| V15 | Il manuale e la guida non trattano lavoro agile, timbratura in sede/QR/kiosk/geofence, formule, anomalie (una sola citazione); turni e pause solo marginalmente | `Manuals/attendance*.md` |
| V19 | `ANALISI_INFRASTRUCTURE_RICHIESTE.md` §2 elenca come problemi aperti `SetState` senza controllo e `lista[idxReq - 1]` fuori indice: risolti (`VerificaPermesso`, controllo `idxReq == 0`). Resta il filtro anno/mese commentato: `Get_Dip_GG_Richiesta` carica tutte le richieste dei rapporti | `Dip_GG_RichiestaService.cs:Get_Dip_GG_Richiesta` |

### 11.2 Codice non usato o da ripulire

| # | Cosa | Riferimento |
|---|---|---|
| V6 | `Par_Arrotondamenti`: entità, repository, servizio, controller e servizio client senza nessun uso nel calcolo e nessuna pagina | `Par_Arrotondamenti.cs`, `Par_ArrotondamentiService.cs` |
| V7 | `My_template1` (entità, servizio, controller, pagine con rotte ancora registrate, voce di menu commentata) e `SRV/TextFile1.txt` sono scheletri di sviluppo | `route-attendance-tracking.service.ts`, `My_Template1Service.cs` |
| V12 | `GeneraGiustificativoAssenza`: un blocco di circa cento righe commentato (versione con richiesta) e il giustificativo esistente è cercato per `IdPar_Giustificativi` e `Data == day` **senza filtrare per rapporto né per `ToBeDeleted`** (**sospetto**: con più rapporti nello stesso giorno o giustificativi già cancellati) | `TimeSheet_EngineService.cs:GeneraGiustificativoAssenza` |
| V13 | In `SaveData` le timbrature `ToBeDeleted` non vengono cancellate (ramo "to do"); nessun punto del motore le marca | `TimeSheet_EngineService.cs:SaveData` |

### 11.3 Rischi di comportamento

| # | Cosa | Riferimento |
|---|---|---|
| V9 | **Turni a cavallo della mezzanotte**: teorico 0 (`CalcolaOreTeoriche`, `GetOreTeoriche`), quindi tutto il lavorato risulta eccedenza; la competenza è la data civile (R1): l'uscita dopo la mezzanotte è del giorno dopo, e il giorno di inizio risulta incompleto (deduzione dal codice, da provare), salvo correzione manuale del giorno di competenza. La pianificazione turni mostra invece la durata corretta. Limite già dichiarato in PAUSE ("fuori perimetro") | R1, R16 |
| V10 | **Risolta in parte (25/11/2026, `ATT-SEC.2`, da provare)**: le **cancellazioni dal client** di timbratura, giustificativo, causale e richiesta ora usano `VerificaCancellazione`: solo `CompanyPowerAdmin` (anche se ha più ruoli; chi "lavora come" un altro è valutato come quell'utente), rapporto di un dipendente dell'azienda corrente, mese non approvato (tutti i giorni per la richiesta); fuori rapporto si può ancora cancellare perché il client lo permette. **Restano scoperti**: il sottoprocesso (`isSubProcess=true`: il motore cancella giustificativi, richieste e causali senza controlli); `SetState` (rifiuto, annullamento, revoca approvata) che cancella timbrature e giustificativi della richiesta con `Dip_GG_Richiesta_Canc_Dettaglio` anche in un mese approvato; `CompanyAdmin` e `User` non possono più chiamare le Delete (il client non gliele offriva). `DeleteClocking` resta escluso da MCP (MCP-44) | `Dip_GG_*Service.cs` (`...Delete`), `GestionePresenzeUserUtility.cs:VerificaCancellazione`, `ANALISI_INFRASTRUCTURE_MCP.md` (MCP-44) |
| V11 | **Rapporti multipli** (riassunzione): `Get_DipAna_DipRapp` prende il primo rapporto senza ordine e **tutti i caricamenti per il calcolo** (`Dip_GG_*_Get_4Calculation`) usano solo quello, mentre `Get_OrariSchema_4User` carica tutti i rapporti del periodo: gli altri rapporti verrebbero calcolati senza timbrature, giustificativi e richieste (deduzione dal codice, da provare) | `GestionePresenzeUserUtility.cs:Get_DipAna_DipRapp`, `Dip_GG_TimbraturaService.cs:Dip_GG_Timbratura_Get_4Calculation` e analoghi |
| V14 | **Tre definizioni di "presente"**: pagina Personale presente (ultima timbratura di quel giorno ≠ `Uscita`), widget (ultima per `TimbraturaOriginale` ≠ `Uscita`), chat `WhoIsPresent` (numero dispari di timbrature valide). Differiscono nelle giornate con timbrature non interpretabili | `PresentStaffService.cs`, `Dip_WidgetService.cs`, `WhoIsPresentHandler.cs` |
| V16 | In `Send` se non si risolvono rapporto o reparti del richiedente, o `Az_SediReparto.Get4User` fallisce, il metodo ritorna senza richiesta e **senza messaggio** (**sospetto**) | `Dip_GG_RichiestaService.cs:Send` |
| V17 | **Nessun indice unico** su `Dip_GG_Result` (rapporto, giorno) né su `Dip_GG_Causali` (rapporto, giorno, causale), e nessuna serializzazione dei ricalcoli: due ricalcoli sovrapposti possono duplicare righe (il motore usa `FirstOrDefault`) | `AttendanceTrackingDbContext.cs`, `Dip_GG_ResultService.cs:Dip_GG_Result_Init` |
| V18 | Il calcolo di un solo giorno carica sempre i mesi interi e, con le eccedenze attive, ricalcola tutti i giorni del periodo di conteggio (settimana o mese): è il costo di ogni timbratura | `TimeSheet_EngineService.cs:RangeCaricamento`, `CalcolaRapporto` |
| V20 | Nessuna validazione di sovrapposizioni o buchi tra i periodi di `Dip_ProfiloOrario`, né tra i rapporti di lavoro di uno stesso dipendente; il motore prende il primo che combacia | `Dip_ProfiloOrarioService.cs`, `Dip_RapportoLavoroService.cs` |
| V21 | L'export paghe legge `Dip_GG_Causali` senza guardare stato del mese, giorni `Init`, errori o provvisori | `TimeSheet_ExportService.cs:PreparaMetadati` |
| V22 | Gli allegati delle note spese e la documentazione dei modelli export sono `byte[]` nel DB, fuori da ogni quota di spazio; il badge del kiosk è in chiaro; il PIN usa come chiave quella di `Timbratura:PinKey` o, in mancanza, la chiave JWT | `Dip_GG_NotaSpesa_Allegato.cs`, `TimbraturaEvolutaService.cs:PinHash` |
| S1 | Le **causali inserite a mano** (pagina del giorno, chat `RegisterCausale`) sono salvate e subito il giorno viene ricalcolato: `ScriviCausali` cancella le causali del giorno che il calcolo non riproduce (e sovrascrive quelle con lo stesso codice). Da provare se la causale manuale sopravvive | `Dip_GG_CausaliService.cs:Dip_GG_CausaliPut`, `TimeSheet_EngineService.cs:ScriviCausali` |
| S2 | Un giorno senza riga in `Dip_GG_Result` quando il mese viene approvato non risulta `Locked` (`BloccaGiorni` lavora sulle righe esistenti) | `Dip_PeriodoService.cs:BloccaGiorni` |
| S3 | Statistiche attività e controllo di gestione leggono le **timbrature** (segmenti tra attività) e non i risultati del motore: le ore per commessa possono non coincidere con il cartellino (pause, arrotondamenti, richieste in attesa: da verificare) | `ActivityStatisticsService.cs`, `CdgCalcolo.cs` |
| S4 | Contatori: per una richiesta che copre due mesi il saldo è calcolato sul mese della data iniziale; il "goduto" conta anche i giustificativi in attesa | `Dip_GG_RichiestaService.cs:ValidaContatoriAsync`, `ContatoriService.cs:CalcolaMesi` |
| S5 | I giustificativi per giorno di una giornata intera si fissano all'invio con il teorico di allora: una modifica successiva del profilo non li aggiorna | `Dip_GG_RichiestaService.cs:OreGiustificativoPerGiorno` |

### 11.4 Aree senza documentazione di dettaglio

Contatori e piano ferie, nota spese e rimborsi chilometrici, organizzazione (sedi, reparti, approvatori, wizard dipendente), importazione dipendenti con l'AI, report PDF, personale presente, anagrafica del rapporto di lavoro e profili orari. Sono descritti solo in questo documento (§6, §7, §8).

---

## 12. Decisioni aperte

| # | Decisione | Note |
|---|---|---|
| ATT-D1 | Ruoli e visibilità **sul server** per le Presenze (§3.3) | Oggi solo client. Va coordinata con `piano-security.md`; in particolare Calculate, approvazione del mese, `Par_*` (le Delete dal client sono già controllate, `ATT-SEC.2`) |
| ATT-D2 | Turni notturni: competenza e teorico | Proposta da valutare: competenza = giorno di inizio del turno per le coppie a cavallo; teorico con +24 ore. Tocca R1, R16, anomalie, turni |
| ATT-D3 | Ricalcolo automatico (o avviso "da ricalcolare") dopo modifica dei parametri | Oggi nessuno (§5.2) |
| ATT-D4 | Export: bloccare, avvisare o lasciare libero con mesi non approvati o giorni in errore | Oggi libero (V21) |
| ATT-D5 | Causali manuali: devono sopravvivere al ricalcolo? | Dipende dalla prova di S1 |
| ATT-D6 | Riporto contatori a fine anno: automatico o solo manuale | Oggi manuale (V4) |
| ATT-D7 | Più orari nello stesso giorno (`ZOrder`): comporli come i turni o vietarli | Oggi solo il primo (R11) |
| ATT-D8 | Rapporti multipli: quale è "corrente" alla data e come si caricano nel calcolo | V11 |
| ATT-D9 | Una sola risoluzione dell'orario del giorno per motore, richieste e anomalie | R17 |
| ATT-D10 | Notifiche, scadenzario e calendario per le Presenze (scadenza accordi lavoro agile, giorni da sistemare, richieste ferme) | Oggi non usati (§8.2) |
| ATT-D11 | Costo orario e ricavi del controllo di gestione persistenti | CONTROLLO_GESTIONE |
| ATT-D12 | Cosa distingue Standard e Pro oltre alle formule e al limite utenti | LICENZE §5: da definire |
| ATT-D13 | Profilo formule predefinito per i nuovi assunti, fase `Ante` | FORMULE §11, domande 4-5 |

---

## 13. Checklist

Legenda: ✅ svolto · ⏳ da fare · 🔄 in corso · 🔍 da decidere

I codici `ATT-*` sono nuovi (i codici `AT-*` dei documenti di dettaglio restano quelli: nessuna collisione). Le voci `ATT-OPEN.*` rimandano ai punti ancora aperti nei documenti di dettaglio senza copiarli.

#### `ATT-DOC` – Documentazione
- ✅ `ATT-DOC.1` Documento d'insieme del modulo (questo documento), 25/11/2026
- ⏳ `ATT-DOC.2` Aggiornare `ANALISI_ATTENDANCETRACKING_WIDGET.md`: il widget delle richieste è `infrastructure.requests`, quattro widget nelle Presenze (V1)
- ⏳ `ATT-DOC.3` Aggiornare nei documenti SMARTWORKING, TIMBRATURA, TURNI i nomi delle migration (oggi `AttendanceTracking_Init`, `_TurniMultipli`, `_TimbraturaReparto`, V3)
- ⏳ `ATT-DOC.4` Manuale `attendance.<lingua>.md` e guida: sezioni su lavoro agile, timbratura in sede/QR/kiosk/geofence, turni e scambi, formule, anomalie e pause (V15)
- ⏳ `ATT-DOC.5` Correggere i commenti non veri nel codice: `Dip_Contatori_Riporto.cs` (riporto automatico), `Par_Turno.cs` (un turno per giorno), commento di testata del motore su `GiornoCompetenza` (V4, V8)
- ⏳ `ATT-DOC.6` Documenti di dettaglio per le aree senza analisi: contatori e piano ferie, nota spese, organizzazione e approvatori, importazione dipendenti (§11.4)

#### `ATT-SEC` – Permessi e sicurezza
- 🔍 `ATT-SEC.1` Controllo di ruolo e di reparto nei servizi delle Presenze (§3.3): da coordinare con `piano-security.md` (fasi 3 e 5) invece di farlo a parte; priorità: `TimeSheet_EngineService.Calculate`, `Dip_PeriodoService` (approva/riapre/tetto), `Dip_GG_Timbratura/Giustificativi/Causali`, `Par_*`, contatori, anagrafica e rapporto di lavoro, `IdAspNetUsers` delle richieste per altri
- 🔄 `ATT-SEC.2` Cancellazioni di timbratura, giustificativo, causale, richiesta: fatto il 25/11/2026 (nel ramo `AttendanceTracking_Dev_cancellazioni`, build ok, **da provare a mano**): `VerificaCancellazione` = solo `CompanyPowerAdmin`, dipendente dell'azienda, mese non approvato (fuori rapporto si può cancellare, come permette il client); sottoprocessi esenti. Restano: `Canc_Dettaglio` di `SetState` senza controllo del mese approvato (V10) e la decisione se riammettere `DeleteClocking` in MCP (MCP-44)
- 🔍 `ATT-SEC.3` Kiosk: conteggio dei tentativi in memoria (non condiviso tra istanze), badge in chiaro, sblocco con la password di un qualunque `CompanyPowerAdmin` (R50, R51, V22)
- ⏳ `ATT-SEC.4` Sapere dove finiscono i dati personali dell'importazione dipendenti (file inviato al provider AI, §8.10): informativa o opzione per disattivarla

#### `ATT-MOT` – Motore di calcolo
- 🔍 `ATT-MOT.1` Turni a cavallo della mezzanotte: competenza della timbratura e ore teoriche (ATT-D2, V9). Prima una prova su un turno 22:00-06:00 reale
- ⏳ `ATT-MOT.2` Rapporti multipli: ordine e scelta del rapporto in corso in `Get_DipAna_DipRapp` e caricamento di tutti i rapporti nei `Dip_GG_*_Get_4Calculation` (V11, ATT-D8)
- 🔍 `ATT-MOT.3` Ricalcolo automatico, o segnalazione "da ricalcolare", dopo modifica di orari, profili, festività, giustificativi, causali, maturazioni, profili formule (ATT-D3)
- ⏳ `ATT-MOT.4` Provare S1 (causale inserita a mano e subito ricalcolata) e decidere: protezione delle causali manuali o rimozione della funzione (ATT-D5)
- ⏳ `ATT-MOT.5` Indici unici su `Dip_GG_Result` (rapporto, giorno) e `Dip_GG_Causali` (rapporto, giorno, causale) e serializzazione dei ricalcoli dello stesso rapporto (V17): richiede migration, controllo dei duplicati esistenti prima
- 🔍 `ATT-MOT.6` Una sola risoluzione dell'orario del giorno (motore, `GestionePresenzeOreTeoriche`, `GestionePresenzeAnomalie`) oppure un test che le confronti (R17, ATT-D9)
- 🔍 `ATT-MOT.7` Più `ZOrder` nello stesso giorno: comporre o vietare con validazione al salvataggio del profilo (R11, ATT-D7)
- ⏳ `ATT-MOT.8` Validare sovrapposizioni e buchi tra i `Dip_ProfiloOrario` e tra i rapporti di lavoro di uno stesso dipendente (V20)
- ⏳ `ATT-MOT.9` Pulizia: blocchi commentati e filtro per rapporto/`ToBeDeleted` in `GeneraGiustificativoAssenza`; ramo "to do" di `SaveData`; `Par_Arrotondamenti`; `IdJustificationType`; `My_template1` e rotte; `TextFile1.txt` (V6, V7, V8, V12, V13)
- 🔍 `ATT-MOT.10` Una sola definizione di "presente" per pagina, widget e chat (V14)
- ⏳ `ATT-MOT.11` Verificare S2 (giorno senza riga `Dip_GG_Result` non bloccato dall'approvazione del mese) e S4/S5 (saldo sul mese iniziale, giustificativi per giorno fissati all'invio)
- ⏳ `ATT-MOT.12` Verificare S3: confronto tra ore per commessa (statistiche e controllo di gestione) e cartellino su un mese con pause e arrotondamenti

#### `ATT-RIC` – Richieste
- ⏳ `ATT-RIC.1` Chat `ApproveRequests`: "straordinari" è mappato su `ApprovazioneStraordinario`, tipo mai creato; allineare all'autorizzazione come giustificativo (V5)
- ⏳ `ATT-RIC.2` `Get_Dip_GG_Richiesta` carica tutte le richieste dei rapporti: filtro per periodo o paginazione (V19)
- ⏳ `ATT-RIC.3` `Send`: messaggio quando mancano rapporto o reparti del richiedente (V16); valutare se l'inserimento diretto dell'HR deve richiedere comunque un approvatore configurato (R40)
- ⏳ `ATT-RIC.4` Togliere gli stati e i tipi mai assegnati (`ParzialmenteApprovata`, `ApprovazioneStraordinario`) o dare loro un uso (V5)

#### `ATT-EXP` – Export e chiusura
- 🔍 `ATT-EXP.1` Export con mesi non approvati, giorni in errore o provvisori: avviso o blocco (ATT-D4, V21). Si appoggia alle regole di `GestionePresenzeAnomalie` (giornate da sistemare)

#### `ATT-CNT` – Contatori
- 🔍 `ATT-CNT.1` Riporto di fine anno automatico o solo manuale (ATT-D6, V4)

#### `ATT-INT` – Integrazione con Infrastructure
- ⏳ `ATT-INT.1` `IApplicationDataDeleter` per le Presenze (conteggi, dati dell'azienda in modalità singola, dati di un utente tolto da un'azienda) — voce di `ANALISI_INFRASTRUCTURE_CANCELLAZIONE.md`, da fare qui su AttendanceTracking_Dev
- 🔍 `ATT-INT.2` Categorie di notifica, scadenzario (accordi di lavoro agile in scadenza, giornate da sistemare) ed eventi nel calendario per le Presenze (ATT-D10)
- 🔍 `ATT-INT.3` Quota di spazio per gli allegati delle note spese e la documentazione dei modelli export (`IStorageUsageProvider`, V22)
- 🔍 `ATT-INT.4` Differenza dei piani Standard e Pro oltre alle formule (ATT-D12): applicare `ILicenseGuard` alle funzioni scelte

#### `ATT-OPEN` – Punti ancora aperti nei documenti di dettaglio (rimandi)
- ⏳ `ATT-OPEN.1` Pause: fase 2 (buono pasto, soglie a scaglioni, voce "pausa detratta" nel PDF e nell'export, stato "in pausa"), validazione degli intervalli nel client, prova su un mese reale → `ANALISI_ATTENDANCETRACKING_PAUSE.md` §13
- ⏳ `ATT-OPEN.2` Formule: lavoro 46 (Ricalcola periodo), 63 (prova su giorni bloccati e cambio di profilo), domande 4-5 → `ANALISI_ATTENDANCETRACKING_FORMULE.md` §11-§12
- ⏳ `ATT-OPEN.3` `AT-TIMB.6a` Prova su dispositivo reale (tablet) → `ANALISI_ATTENDANCETRACKING_TIMBRATURA.md` §7
- ⏳ `ATT-OPEN.4` `AT-TURNI.12` Catena dei merge di dialog e report → `ANALISI_ATTENDANCETRACKING_TURNI.md` §9
- 🔍 `ATT-OPEN.5` AN1 Soglie configurabili delle anomalie (migration) → `ANALISI_ATTENDANCETRACKING_ANOMALIE_AI.md` §10
- 🔍 `ATT-OPEN.6` Costo orario e ricavo persistenti; collegamento diretto della nota spese alla commessa (ATT-D11) → `ANALISI_ATTENDANCETRACKING_CONTROLLO_GESTIONE.md` §8
- 🔍 `ATT-OPEN.7` MCP-44: `CalcolaGiorno` e `DeleteClocking` esclusi per prudenza (collegato a `ATT-SEC.2` e a un ricalcolo "puro" da MCP) → `ANALISI_INFRASTRUCTURE_MCP.md`
