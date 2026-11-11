# Analisi – Formule personalizzate nel calcolo giornata

Data: 11/11/2026 — ramo di analisi: `AttendanceTracking_Dev`
Stato: **solo analisi, nessuna modifica al codice**.

Origine: replica di `wfFormule.aspx`, `wfProfiliFormule.aspx` e del collegamento in `wfDipendentiModifica.aspx` (RilevaFull, `C:\REPO\Repo_Presenze`).
Modulo: Presenze. Visibilità: power admin. Voci nel menu **Parametri**.

---

## 0. Sintesi e decisioni

L'utente scrive piccoli script ("formule") che personalizzano il conteggio della singola giornata, per esempio:
- "se ORDI > 4h sposta l'eccedenza su STR1";
- "se manca il terminale metti un'anomalia".

Le formule si raggruppano in **profili**: righe ordinate, ognuna con un periodo di validità. Un profilo si collega al dipendente. Durante il calcolo della giornata il motore esegue, in ordine, le formule del profilo attivo.

Nel legacy la formula è **codice C# compilato a runtime** (CodeDom), che gira in-process con piena fiducia e scrive direttamente sul DB. nvxapp invece è **multi-tenant**, quindi va cambiato approccio: un amministratore di un'azienda non deve poter eseguire codice arbitrario sul server condiviso.

### Decisioni proposte (da confermare)

| # | Decisione | Proposta |
|---|-----------|----------|
| D1 | Linguaggio delle formule | **JavaScript in sandbox con Jint** (§4.1). C# con Roslyn si scarta perché in multi-tenant è un rischio di sicurezza inaccettabile |
| D2 | Cosa può toccare una formula | **Solo i dati in memoria del calcolo** (causali, importi, warning e log del giorno), tramite una facciata `x`. Niente accesso diretto al DB: salva il motore con `ScriviCausali`/`SaveData` |
| D3 | Collegamento al dipendente | **`Dip_ProfiloFormule` storicizzato Dal/Al per rapporto di lavoro**, come `Dip_ProfiloOrario`. Il legacy aveva un campo singolo senza storico: ricalcolare il passato usava il profilo attuale |
| D4 | Visibilità | Ruolo **`CompanyPowerAdmin`**, come tutta la sezione Parametri: menu, route guard e controllo `HaRuolo` nei servizi |
| D5 | Fasi di esecuzione | v1: **Giorno** (dopo `GeneraCausali`) e **Finale** (prima di `ScriviCausali`). La fase **Ante** del legacy, che lavora sulle timbrature grezze, passa alla fase 2 |
| D6 | Errori di runtime | Non bloccano il calcolo. Si scrive un log per giorno e si alza un **warning sul giorno**, visibile nel cartellino: nel legacy restavano solo in `LogInfo` |
| D7 | Password di sblocco | **Eliminata**: nel legacy era scritta in chiaro nel JS. La protezione è il ruolo, più il flag `DiSistema` per le formule di base (fase 2) |
| D8 | AI formula | Si riusa `ILlmClient.CompleteJsonAsync` (Infrastructure). Il catalogo dei metodi generato dalla facciata entra nel prompt; il server valida il codice prodotto. **All'AI non vanno dati dei dipendenti** |
| D9 | Editor | CodeMirror 6 caricato in lazy nella pagina di modifica, con autocomplete su `x.` dal catalogo metodi. Alternativa senza dipendenze: textarea con modale "metodi" come nel legacy |
| D10 | Ricalcolo dopo una modifica | Nessun ricalcolo automatico. Nel profilo c'è il pulsante "Ricalcola periodo", che usa l'endpoint `TimeSheet_Engine/Calculate` già esistente |

### Decisioni ancora aperte

1. D1 (Jint): conferma, oppure si vuole compatibilità sintattica col C# legacy per portare le formule esistenti.
2. D4: per "poweradmin" si intende `CompanyPowerAdmin` (HR o amministratore dell'azienda) o `PowerAdmin` di piattaforma? L'analisi assume **CompanyPowerAdmin**, perché `PowerAdmin` non accede ai dati aziendali del modulo.
3. D9: va bene aggiungere CodeMirror come dipendenza del client?

---

## 1. Situazione attuale – legacy (RilevaFull)

### 1.1 Pagine

| Pagina | Controller JS | Funzione |
|---|---|---|
| `wfFormule.aspx` | `Scripts/angular_Site/Controllers/Controller_Formule.js` | CRUD delle formule, Duplica, Ctrl Sintax, Log formula / Log tutte, Lista metodi (Excel), AI formula, autocomplete su `x.`, "PW sblocco" |
| `wfProfiliFormule.aspx` | `Controller_ProfiliFormule.js` | CRUD dei profili: testata più righe (formula, Priorità, Dal, Al, Sospesa, ElaboraAnte), Duplica |
| `wfFormuleWiz.aspx` | `Controller_FormuleWiz.js` | Assegnazione massiva di un profilo ai dipendenti. **Bug**: legge una `Formule` con l'ID del profilo (`Function_FormuleWiz.cs:340`) |
| `wfDipendentiModifica.aspx` | `DipendentiModificaCtrl.js` | Select `IDProfiloFormule` (riga 1595), salvato da `SaveDipendenteController.cs:748` |

**API.** Il server è `RilevaFull_WEB.API`, con un controller WebApi per azione:
- formule: `Formule_Index`, `_Dettaglio_Get`, `_Update`, `_Delete`, `_Copy`, `_CtrlSintax`, `_ReadLog`, `_MetodiExp`, `_Health`, `_Ask`;
- profili: `ProfiliFormule_*`;
- wizard: `FormuleWiz_*`.

### 1.2 Tabelle (SQL Server)

```sql
Formule            (ID, IDBaseCP int NULL, DescrBreve varchar(20), Titolo varchar(60), Descrizione varchar(max), Formula varchar(max))
ProfiloFormule     (ID, DescrBreve varchar(20), Descrizione varchar(200))
ProfiloFormuleDett (ID, IDProfiloFormule FK, IDFormule FK, Priorita int, Dal datetime, Al datetime, Sospesa bit, ElaboraAnte bit)
LogInfo            (ID, Tipo 0 info/1 warn/2 err/3 debug, Data, Azione='formula', Evento=IDFormula, Info, InfoAgg, Utente)
Dipendenti.IDProfiloFormule int NULL   -- nessuno storico per data
```

### 1.3 Motore

**File:** `RilevaFull_WEB.FormuleEngine/Formule.cs`, 13.5k righe.

**Compilazione**
- Il testo della formula viene inserito nel metodo `EvalFormula` della classe `FormuleEngine.Script` (template `FormulaTemplate.txt`).
- Poi viene compilato con `CSharpCodeProvider` in una DLL su disco `[ID]Db.dll`, con cache di 36 ore, ed eseguito via reflection.
- L'unica protezione è una blacklist testuale (`System.IO`, `System.Data`, cicli `for`/`while`/`foreach`/`do`).

**L'oggetto `x`** è un `Formule_DEF`. Contiene il contesto (`CurrIDDipendente`, `Curr_Day`, `Action`) e circa 15 sotto-oggetti, che leggono e scrivono **direttamente sul DB**:

| Sotto-oggetto | Uso |
|---|---|
| `x.Attiva` | Filtro: giorni della settimana, mesi, festivo, `ApplicaLaFormula()` |
| `x.Causale` | `GetOre`, `PutOre`, `AddOre`, `CopiaOreDaA`, `SpostaOreDaA`, … |
| `x.Giustificativo` | |
| `x.Timbrate` | Coppie, conteggi, ore lavorate, terminali |
| `x.Orario` | Orario teorico |
| `x.Contatori` | |
| `x.Anomalie` | |
| `x.Stato` | |
| `x.Time` | Helper su TimeSpan |
| `x.GeneralF` | Date |
| `x.Log` | |
| `x.Pianificazione` | |
| `x.Cantiere` | |
| `x.GruppoCausali` | |
| `x.Dipendenti` | |

**Lista metodi.** La genera la reflection su `Formule_DEF`, con le descrizioni prese dai `<summary>` XML e l'attributo `[inevidenza]`.

**Esecuzione** (`Function_Cartellino.Async2.cs:6010`), per ogni dipendente e giorno:
1. salvataggio del dato utente;
2. formule **Ante** (`ElaboraAnte=true`);
3. cancellazione delle causali, riconoscimento orario, accoppiamento timbrature;
4. `AssegnaOreCausali`;
5. formule **Post**;
6. consolida.

Si eseguono le righe del profilo non sospese con `Dal <= giorno <= Al`, `ORDER BY Priorita`. Un errore viene scritto in `LogInfo` e non blocca il calcolo.

**AI formula.** Un proxy HTTP verso un servizio esterno (FastAPI, `FormulaCreatorServiceUrl`): `/health` e `/ask {message}` → `{code, sources}`.

### 1.4 Problemi del legacy da non replicare

1. Codice C# arbitrario in-process, senza sandbox: inaccettabile in multi-tenant.
2. Password di sblocco solo client-side (`"cpvi-psl"` / `"isit2018"` nel JS).
3. Profilo sul dipendente senza storico.
4. Gli errori finiscono in una tabella log che nessuno guarda.
5. Il Ctrl Sintax salva la formula prima di controllarla.
6. La cache DLL di 36 ore può far girare una versione vecchia della formula.
7. Bug nel Wizard (§1.1).
8. Scrittura diretta su DB da dentro la formula: nessuna atomicità con il calcolo.

---

## 2. Situazione attuale – nvxapp (`AttendanceTracking_Dev`)

### 2.1 Motore di calcolo

**File:** `nvxapp.server.service/ClientServer-Service/GestionePresenze/TimeSheet_EngineService/TimeSheet_EngineService.cs`

| Metodo | Riga | Cosa fa |
|---|---|---|
| `Calculate` | 240 | Job in background con progressi SignalR. Carica `Get_Timesheet_AllData` e chiama `CalcolaRapporto` per ogni rapporto |
| `Get_OrariSchema_4User` | 441 | Carica rapporti, `Dip_ProfiloOrario` (502), DaySlots (543-620), orari, giustificativi. **Qui si caricano i profili formule** |
| `CalcolaRapporto` | 867 | (1) periodi, (2) `CalcolaGiorno` per ogni giorno, (3) qualifica periodo e tetto, **(4) result e causali (958-983)**, (5) cartellino mese. I giorni `Locked` vengono saltati |
| `CalcolaGiorno` | 1066 | Verso timbrature, `HH_Teo`, `HH_Lav`, giustificativo assenza, **`GeneraCausali` (1109)**, poi `Stato = Combine(...)` (1111) |
| `GeneraCausali` | 1969 | Riempie `_causaliGiorno[(rapporto, giorno)]` (`Dictionary<int, TimeSpan>`) e `_importiGiorno` (righe 2201-2202) |
| `ScriviCausali` | 2225 | Differenza con le `Dip_GG_Causali` esistenti |
| `SaveData` | 1118 | Salva i record con hash cambiato (`ExcludeRicalc=true`) |

**Punti di aggancio** (verificati):
- **Fase Giorno**: in `CalcolaGiorno`, dopo `GeneraCausali` (riga 1109) e prima di `Combine`. Sono disponibili le causali del giorno, i result giornalieri, le timbrature e l'orario. Le eccedenze di periodo non sono ancora qualificate.
- **Fase Finale**: in `CalcolaRapporto`, passo 4, dopo `AggiungiCausaliEccedenze` (riga 979) e prima di `ScriviCausali` (riga 982). Il result è completo, compresi straordinario e supplementare qualificati. Il risultato viene salvato dai meccanismi esistenti.
  - Nota: alla riga 975 c'è `if (!_causaliGiorno.TryGetValue(...)) continue;`. Se il giorno ha formule e non ha causali, va creato un dizionario vuoto.

### 2.2 Anagrafica dipendente

**Front:** `nvxapp.client/src/nvx/pages/GestionePresenze/user-department-edit-page/`.
- Per ogni rapporto ci sono i sotto-segmenti GENERALE, PROF_HH, CONTATORI, SPESA, VARIE.
- Il profilo orario è una lista per rapporto, gestita con il dialog `edit-dip-profilo-orario-dialog`.
- Gli id nuovi sono negativi. Il salvataggio invia tutto `Dip_Anagrafica4EditModel` a `Dip_AnagraficaPut`.

**Back:**
- `Dip_AnagraficaService` carica i profili in Get (righe 224-233) e li salva in Put (righe 375-394).
- `Dip_ProfiloOrarioService.Put` cancella i record assenti e fa upsert degli altri.
- Il motore sceglie il profilo con `Dal <= giorno && Al >= giorno` (`TimeSheet_EngineService.cs:561`).

**`Dip_ProfiloFormule` è un clone di questo schema.**

### 2.3 Template CRUD e testata+righe

- CRUD semplice: **`Par_TipoSpesa`**, con entità, repository, service, mapper, controller, client service, pagine list/edit, route, menu, i18n e default. Vedi `docs/GENERAZIONE_TABELLE_GUIDA_COMPLETA.md`.
- Testata più righe: **`Par_ProfiloOrario` + `Par_ProfiloOrarioGG`**, con le pagine `profilo-orario-list/edit-page` e il dialog di dettaglio.

### 2.4 Menu, ruoli e LLM

**Menu**: `nvxapp.client/src/nvx/Utility/infrastructure/main-menu-attendance-tracking.service.ts`.
- L'intestazione Parametri è alla riga 36 (`group: 1`, `roles: ['CompanyPowerAdmin']`).
- Le voci stanno alle righe 37-44, con zorder da 202000 a 207500.
- **Lo zorder 208000 è già occupato** da `menu.menuRestriction` di Infrastructure.

**Route**: `route-attendance-tracking.service.ts`, con `RoleGuard4CompanyPowerAdmin`.

**Ruoli**:
- I controller hanno solo `[Authorize]`.
- Il controllo di ruolo sta nel servizio, con `HaRuolo("CompanyPowerAdmin")` (es. `ReportService.cs:87/566`).

**LLM**: `infrastructure/ChatAI/LlmClient.cs`, che espone `ILlmClient.CompleteJsonAsync(systemPrompt, userMessage, schema, …)`.
- Configurazione `AI` in `appsettings.json`.
- Precedente con lo stesso schema "l'AI genera, il server valida": `Par_ExportCau_RicettaService` + `ExportRicettaRenderer`.

**Scripting**: nessuna libreria presente (né Jint, né NCalc, né Roslyn).

---

## 3. Modello dati (tenant, PostgreSQL, EF Core)

Tutte le entità derivano da `BaseEntity` (Id, CreationDate, ModifiedDate, ChangeUser). La migrazione EF va in `Migrations/AttendanceTracking`.

```text
Par_Formule
  IdAz_Anagrafica   int  FK cascade
  Codice            varchar(20)  NOT NULL   -- unico per azienda
  Titolo            varchar(60)  NOT NULL
  Descrizione       text         NULL       -- "siate esaustivi"
  Testo             text         NOT NULL   -- codice JS
  Hash              varchar(64)  NULL       -- SHA256 di Testo, chiave della cache dello script compilato
  DiSistema         bool         default false   -- (fase 2) formula di base, sola lettura per CompanyPowerAdmin

Par_ProfiloFormule
  IdAz_Anagrafica   int FK cascade
  Codice            varchar(20)  NOT NULL   -- unico per azienda
  Descrizione       varchar(200) NOT NULL

Par_ProfiloFormuleDett
  IdPar_ProfiloFormule int FK cascade
  IdPar_Formule        int FK restrict
  Ordine               int  NOT NULL (>0)   -- legacy: Priorita
  Dal, Al              date NOT NULL        -- default 01/01/2000 – 31/12/2100
  Sospesa              bool NOT NULL default false
  Fase                 enum FormulaFase { Giorno=1, Finale=2, Ante=0 (fase 2) }   -- legacy: ElaboraAnte

Dip_ProfiloFormule
  IdDip_RapportoLavoro int FK cascade
  IdPar_ProfiloFormule int FK restrict
  Dal, Al              date NOT NULL

Dip_GG_FormuleLog
  IdDip_RapportoLavoro int FK cascade
  Giorno               date
  IdPar_Formule        int  NULL FK set null
  Tipo                 enum { Info, Warning, Errore, Debug }
  Messaggio            varchar(500)
  Data                 timestamp
  -- a ogni ricalcolo di (rapporto, giorno) si cancellano i log precedenti: crescita contenuta
```

**Regole di cancellazione:**
- `Par_Formule` non si cancella se è usata in `Par_ProfiloFormuleDett`. `Par_ProfiloFormule` non si cancella se è usata in `Dip_ProfiloFormule`. Si usa `UserMessageException`, come in `Par_TipoSpesaService.Delete`.
- Le righe del dettaglio si cancellano in cascata con la testata. Nel legacy la cancellazione falliva per la FK NO ACTION.

**Stato del giorno:** si dedica uno slot libero di `GG_ResultStato` (es. `Warning_n` = "Errore formula"), con la relativa label nel cartellino.

---

## 4. Motore formule

### 4.1 Perché Jint

| | Roslyn C# (come il legacy) | **Jint (JS)** | NCalc / espressioni |
|---|---|---|---|
| Compatibilità con le formule legacy | Alta | Media: la sintassi `x.Causale.GetOre("ORDI")`, `if`, `var` resta uguale; cambiano TimeSpan e i tipi | Bassa |
| Sicurezza in multi-tenant | **No**: codice nativo in-process, senza sandbox in .NET 10 | **Sì**: interprete puro, CLR non esposto (`AllowClr` off), limiti `TimeoutInterval`, `MaxStatements`, `LimitMemory`, `LimitRecursion` | Sì |
| Potenza (if, variabili, funzioni) | Piena | Piena; i cicli sono ammessi ma limitati dai contatori | Solo espressioni |
| Diagnostica sintassi | Compilatore | Parser Esprima con riga e colonna | Sì |
| Prestazioni | Ottime dopo la compilazione | Buone: `Engine.PrepareScript` con cache per Hash, un engine riusato per job | Ottime |

**Raccomandazione:** il pacchetto NuGet `Jint` (MIT) in `nvxapp.server.service.csproj`, solo sul ramo AttendanceTracking.

### 4.2 Componenti server (nuovi)

Cartella `ClientServer-Service/GestionePresenze/FormuleEngine/`:

**`FormuleScriptHost`**
- Crea l'`Engine` Jint con i limiti:
  - timeout 200 ms per esecuzione;
  - 50.000 statement;
  - 4 MB di memoria;
  - ricorsione massima 20.
- Espone solo `x`.
- Cache statica `ConcurrentDictionary<(IdFormula, Hash), Prepared<Script>>`.
- `CtrlSintassi(testo)` → elenco errori con riga e colonna. Analizza anche l'AST: segnala come errori i membri `x.*` che non esistono nel catalogo.

**`FormulaContext` (la facciata `x`)**
- È costruita per ogni rapporto e giorno sopra gli oggetti in memoria del motore: `AllData`, `orariSchema`, `causali`, `importi`, `dip_GG_Result`.
- Mappa i **codici** causale e giustificativo sugli Id (`Par_Causali.Codice`).
- Lavora in **minuti interi** (gli `int` dei `Min_*`); la conversione da e verso TimeSpan è interna.

**`FormuleCatalogo`**
- Reflection sui metodi della facciata marcati `[FormulaApi(Categoria, Descrizione, InEvidenza)]`.
- Produce la "Lista metodi" (endpoint, autocomplete, prompt AI). Sostituisce gli XML doc e `[inevidenza]` del legacy.

**`FormuleRunner`**
- `Esegui(fase, rapporto, giorno, ...)`:
  1. risolve il `Dip_ProfiloFormule` attivo nel giorno;
  2. prende le righe non sospese con `Dal <= giorno <= Al` e con la fase richiesta, ordinate per `Ordine`;
  3. esegue le formule una dopo l'altra, catturando gli errori;
  4. raccoglie i log in memoria;
  5. in caso di errore alza il warning sul result.

### 4.3 API della facciata `x` – v1

Le funzioni marcate ★ sono quelle "in evidenza".

| Area | Membri |
|---|---|
| Contesto | `x.Giorno` (Date) ★, `x.GiornoSettimana` (1=lun…7=dom), `x.Festivo` ★, `x.Fase`, `x.Rapporto.TipoContratto / OreSettimanali / Ccnl`, `x.Dipendente.Matricola` |
| `x.Attiva` (compatibile col legacy) | `Sempre(b)`, `Lun..Dom(b)`, `Gennaio..Dicembre(b)`, `Festivo(b)`, `NonFestivo(b)`, `ApplicaLaFormula()` ★ |
| `x.Causale` | `GetMin(cod)` ★, `SetMin(cod, min)` ★, `AddMin(cod, min)`, `Sposta(da, a, min?)` ★, `Copia(da, a, min?)`, `TogliEccedenza(cod, maxMin, codDest)` ★, `Esiste(cod)`, `Elenco()`, `GetImporto(cod)`, `SetImporto(cod, val)` (per le causali di tipo Importo) |
| `x.Giustificativo` | `GetMin(cod)`, `Esiste(cod)`, `Elenco()`: sola lettura in v1 |
| `x.Timbrate` | `Count`, `Get(i)` → `{ Ora (min da mezzanotte), Verso, Tipo }`, `Prima()`, `Ultima()`, `MinutiLavorati()`, `MinutiLavoratiTra(dalle, alle)` |
| `x.Orario` | `Codice`, `MinTeorici` ★, `Fasce` → `[{Dalle, Alle}]`, `ProfiloCodice` |
| `x.Result` | Sola lettura: `HH_Teo`, `HH_Lav`, `Min_Lavorabili`, `Min_Assenze`, `Min_Ord`, `Min_Ecc`, `Min_EccNott`, `Min_Strao*`, `Min_Suppl*`, `BancaOre`. Nella fase Giorno i campi di periodo valgono 0 |
| `x.Stato` | `Warning(msg)` ★ (warning sul giorno più log), `Anomalia(msg)` (stato di errore sul giorno più log) |
| Log | `x.Log(msg)` ★, `x.Debug(msg)` (scritto solo quando si esegue "Prova formula") |
| `x.Time` | `HHMM(min)` → "hh:mm", `Min(h, m)`, `Arrotonda(min, passo, modo)` con modo eccesso, difetto o vicino |

Non sono previsti in v1, e si valutano in seguito: contatori, cantieri, pianificazione, scrittura di timbrature o giustificativi (cioè la fase Ante), sedi e reparti.

**Template di una nuova formula** (come il legacy):

```js
// Controllo applicabilità formula
// se Attiva.Sempre(true) le altre proprietà vengono ignorate
x.Attiva.Sempre(true);
// x.Attiva.Festivo(true); x.Attiva.NonFestivo(true);
// x.Attiva.Lun(true); ... x.Attiva.Dom(true);
if (!x.Attiva.ApplicaLaFormula()) return;
// inizio formula
```

**Esempio:** "se ORDI supera 4 ore, l'eccedenza va su STR1" (nel legacy `x.TogliEccedenza("ORDI",4,0,"STR1")`):

```js
x.Attiva.Sempre(true);
if (!x.Attiva.ApplicaLaFormula()) return;
x.Causale.TogliEccedenza("ORDI", x.Time.Min(4, 0), "STR1");
```

Il testo della formula viene avvolto in una funzione (`(function(){ ... })()`), così il `return` di primo livello è valido. Le righe riportate negli errori vengono riallineate.

### 4.4 Integrazione nel calcolo (`TimeSheet_EngineService`)

1. **`Get_OrariSchema_4User`**: carica `Dip_ProfiloFormule` sovrapposti al periodo, più `Par_ProfiloFormule`, il dettaglio e le `Par_Formule` coinvolte. Le liste vanno aggiunte a `OrariSchema_4User_OutModel`. Se l'azienda non ha profili formule il costo è zero.
2. **`CalcolaGiorno`**: dopo `GeneraCausali`, `_formuleRunner.Esegui(FormulaFase.Giorno, …)`. Se una formula alza un warning, il risultato entra in `Combine`.
3. **`CalcolaRapporto`**, passo 4: dopo `AggiungiCausaliEccedenze`, `Esegui(FormulaFase.Finale, …)`. Va creato il dizionario delle causali se manca.
4. **`SaveData`**: per ogni (rapporto, giorno) calcolato cancella e reinserisce `Dip_GG_FormuleLog`.
5. **Simulazione** (per "Prova formula"): nuovo flag `SoloSimulazione` nel modello di calcolo. Esegue il calcolo per un rapporto e un giorno **senza `SaveData`** e restituisce le causali prima e dopo le formule, i result e i log, compresi quelli di debug.
6. I giorni `Locked` (mese approvato) restano esclusi: è il comportamento già esistente.

---

## 5. API (controller `[Authorize]` + `HaRuolo("CompanyPowerAdmin")` nel servizio)

| Controller / azione | Note |
|---|---|
| `Par_Formule/GetAll`, `Get`, `Put`, `Delete` | `Put` aggiorna l'Hash e controlla l'unicità del Codice. `Delete` è bloccata se la formula è usata |
| `Par_Formule/Duplica` | `{Id, Codice, Titolo}`; il titolo di default è "copia di …" |
| `Par_Formule/CtrlSintassi` | `{Testo}` → `[{Riga, Colonna, Messaggio}]`. **Non salva**, diversamente dal legacy |
| `Par_Formule/Metodi` | Catalogo `{Categoria, Firma, Testo, Descrizione, InEvidenza}`; c'è anche l'export in Excel con ClosedXML, come il legacy |
| `Par_Formule/Prova` | `{Testo, IdDip_RapportoLavoro, Giorno}` → simulazione (§4.4.5) |
| `Par_Formule/Log` | `{IdPar_Formule?, Dal, Al}` → righe di `Dip_GG_FormuleLog` ("Log formula" e "Log tutte") |
| `Par_Formule/GeneraAI` | §7 |
| `Par_ProfiloFormule/GetAll`, `Get` (testata più righe), `Put` (upsert delle righe con id negativo e cancellazione di quelle mancanti), `Delete`, `Duplica` | Clone di `Par_ProfiloOrario` |
| `Dip_ProfiloFormule` | Nessun controller: passa da `Dip_AnagraficaGet/Put`, come `Dip_ProfiloOrario` |

---

## 6. Front-end (Angular + Ionic, NgModule, pattern esistenti)

### 6.1 Formule – `formule-list-page` / `formule-edit-page`

**Lista** (`nvx-list`)
- Colonne: Codice, Titolo, Descrizione (troncata).
- Ricerca e azioni Modifica / Duplica / Elimina.
- Il pulsante "nuovo" sta nel fab.

**Modifica** (`BasePageConfirmCancelComponent`), divisa in segmenti:
- **Dati**: Codice (max 20), Titolo (60), Descrizione.
- **Formula**:
  - editor con autocomplete su `x.` dal catalogo `Metodi` (D9);
  - indicatore "Ln, Col";
  - il template viene precompilato sulle formule nuove.
- **Log**: log della formula filtrati per periodo, con il pallino colorato per tipo.

**Azioni nel fab o nella toolbar:**
- Controlla sintassi: mostra gli errori ed evidenzia la riga.
- Metodi: pannello o modale con ricerca, filtro per categoria e "solo in evidenza"; un clic inserisce il metodo.
- Prova: dialog con dipendente, rapporto e giorno → causali prima e dopo, warning e log.
- AI formula: §7.
- Duplica.

### 6.2 Profili formule – `profilo-formule-list-page` / `profilo-formule-edit-page`

- **Testata**: Codice, Descrizione.
- **Righe**: `nvx-list` ordinata per Ordine, con le colonne Ordine, Formula (Codice – Titolo), Fase, Dal, Al, Sospesa.
- Il dialog `edit-par-profilo-formule-dett-dialog` serve ad aggiungere o modificare una riga: select della formula, Ordine > 0, Fase, Dal/Al con default 2000–2100, Sospesa.
- Azioni: Duplica profilo, **Ricalcola periodo** (D10).
- Fase 2: azione "Assegna a dipendenti" (sostituisce `wfFormuleWiz`, §9).

### 6.3 Dipendente – `user-department-edit-page`

- Nuovo sotto-segmento del rapporto, **PROF_FORM** ("Profilo formule"), accanto a PROF_HH.
- Lista con colonne Profilo, Dal, Al e dialog `edit-dip-profilo-formule-dialog`, clonato da `edit-dip-profilo-orario-dialog`.
- Voce nel fab per il ramo del segmento. Id negativi per i record nuovi.
- Il profilo non è obbligatorio: senza profilo non si eseguono formule.
- Controllo di **sovrapposizione delle date** nel dialog e nel servizio (oggi manca anche per il profilo orario).
- Il sotto-segmento si vede solo a `CompanyPowerAdmin`.

### 6.4 Servizi, cache, menu, route e i18n

**Servizi**: `src/nvx/ClientServer-Service/GestionePresenze/Par_Formule/`, `Par_ProfiloFormule/` + `Models/`.

**Cache**: `shared-parameter-gestione-presenze.service.ts`, con una nuova voce `ParTable.Par_ProfiloFormule` nei `loaders`, per la select del dipendente.

**Menu Parametri** (`main-menu-attendance-tracking.service.ts`, `group: 1`):
  ```ts
  { menuType: MenuType.MenuItem, zorder: 207600, group: 1, title: 'attendance.menu.formule',        roles: ['CompanyPowerAdmin'], component: 'FormuleListPageComponent',        url: '/formulelist',        icon: 'code-slash-outline' },
  { menuType: MenuType.MenuItem, zorder: 207700, group: 1, title: 'attendance.menu.profiliFormule', roles: ['CompanyPowerAdmin'], component: 'ProfiloFormuleListPageComponent', url: '/profiloformulelist', icon: 'layers-outline' },
  ```

**Route**: `formulelist`, `formuleedit`, `profiloformulelist`, `profiloformuleedit`, con `RoleGuard4CompanyPowerAdmin`.

**i18n**: in `assets/i18n/attendance-tracking/it.json` ed `en.json`:
- `attendance.menu.formule` e `attendance.menu.profiliFormule`;
- `attendance.pages.formuleList`, `formuleEdit`, `profiloFormuleList`, `profiloFormuleEdit`;
- le label dei dialog e del segmento del dipendente;
- i messaggi server in `AttendanceTrackingMessages(.en).resx`.

---

## 7. AI formula

**Endpoint**: `Par_Formule/GeneraAI {Richiesta, TestoAttuale?}`.

**Prompt e chiamata**
- System prompt con regole del linguaggio, template, **catalogo metodi** generato da `FormuleCatalogo`, e da 5 a 10 esempi.
- Il system prompt è versionato, come la ricetta export.
- `ILlmClient.CompleteJsonAsync` con lo schema `{ codice: string, spiegazione: string }`.
- Provider: sezione `AI:FormuleLLM` se c'è, altrimenti `UseLLM`.

**Validazione**: il server passa il risultato a `CtrlSintassi`. Se ci sono errori fa **un solo** nuovo tentativo, mandando gli errori all'AI, e poi restituisce codice, spiegazione e avvisi.

**Privacy**: si inviano solo la richiesta, il testo della formula e la documentazione, **mai dati dei dipendenti** (vedi `ANALISI_EXPORT_AI.md` §7).

**Front**: dialog con prompt e risultato, e i pulsanti "Sostituisci", "Accoda" e "Copia" (come il legacy).

**Fase 3 (facoltativa)**: "Converti formula RilevaFull". Si incolla il C# legacy e l'AI lo traduce sulla facciata `x` v1, segnalando i metodi non supportati.

---

## 8. Sicurezza

- Sandbox Jint: CLR off, nessun `require`/`import`, nessun accesso a file o rete, limiti di tempo, statement e memoria.
- Se un limite viene superato, la formula va in errore, l'errore finisce nel log e il calcolo continua.
- La facciata espone solo i dati del rapporto e del giorno in calcolo: niente query, niente altri dipendenti, niente altre aziende.
- Ruolo verificato **nel servizio** (`HaRuolo("CompanyPowerAdmin")`) su tutte le azioni di formule e profili. La guard client da sola non basta.
- Testo limitato a 20.000 caratteri.
- Audit: `ChangeUser` e `ModifiedDate` di `BaseEntity`. Fase 2: storico delle versioni della formula.

---

## 9. Fasi successive (fuori v1)

| Tema | Contenuto |
|---|---|
| Fase Ante | Formule che modificano timbrature o giustificativi in memoria prima di `GeneraCausali` (equivale a `ElaboraAnte`) |
| Assegnazione massiva | Dalla pagina profilo: scelta dei dipendenti e della data Dal, crea `Dip_ProfiloFormule` e chiude il precedente (sostituisce `wfFormuleWiz`, senza il suo bug) |
| Formule di sistema | Flag `DiSistema` (legacy `IDBaseCP`): sola lettura per l'azienda, duplicabili; seed in `GestionePresenzeParametriDefault` |
| API estese | Contatori, cantieri e commesse, pianificazione, sedi |
| Conversione legacy | §7, fase 3 |
| Storico versioni | Tabella `Par_FormuleVersione` |

---

## 10. Rami e merge

- Tutto il lavoro si fa su **`AttendanceTracking_Dev`**, in un worktree dedicato. La catena è AttendanceTracking_Dev → AttendanceTracking → AllFunction_Dev → AllFunction, previa conferma.
- Nessuna modifica è necessaria su Infrastructure: `LlmClient`, i ruoli e il menu esistono già.
- **Attenzione al merge in AllFunction**: il `PackageReference` Jint in `nvxapp.server.service.csproj`, la nuova migrazione EF di AttendanceTracking e le chiavi `.resx` possono andare in conflitto con gli altri rami. Prima della catena serve una prova di merge in un worktree temporaneo.

---

## 11. Domande aperte

1. Conferma di D1 (JavaScript con Jint) invece del C# legacy.
2. "Poweradmin" vuol dire `CompanyPowerAdmin`? (D4)
3. Va bene CodeMirror come dipendenza? (D9)
4. Il profilo formule è facoltativo per tutti i dipendenti, o serve un **profilo predefinito** da assegnare ai nuovi assunti (nel legacy, regola Unicomm: profilo obbligatorio)?
5. La fase Ante serve già in v1?

---

## 12. Lista lavori (checklist)

Numerazione unica per tutto il documento. `[ ]` da fare, `[x]` fatto (con nota).

### Stato per sezione

| Sezione | Contenuto | Lavori | Fatti | Stato |
|---|---|---|---|---|
| 12.0 | Decisioni | 1–5 | 0 | da fare |
| 12.1 | Modello dati | 6–13 | 0 | da fare |
| 12.2 | Motore formule (Jint) | 14–21 | 0 | da fare |
| 12.3 | Integrazione nel calcolo | 22–27 | 0 | da fare |
| 12.4 | API | 28–33 | 0 | da fare |
| 12.5 | Front formule | 34–41 | 0 | da fare |
| 12.6 | Front profili | 42–46 | 0 | da fare |
| 12.7 | Dipendente | 47–51 | 0 | da fare |
| 12.8 | Menu, route, i18n, permessi | 52–55 | 0 | da fare |
| 12.9 | AI formula | 56–59 | 0 | da fare |
| 12.10 | Verifiche e merge | 60–66 | 0 | da fare |
| **Totale** | | **66** | **0** | |

### 12.0 Decisioni (lavori 1–5) (§0, §11)
- [ ] 1. Confermare D1: linguaggio JavaScript con Jint
- [ ] 2. Confermare D4: visibilità CompanyPowerAdmin
- [ ] 3. Confermare D9: editor CodeMirror 6 oppure textarea con modale
- [ ] 4. Decidere sul profilo predefinito o obbligatorio per i nuovi assunti
- [ ] 5. Decidere se la fase Ante entra in v1

### 12.1 Modello dati (lavori 6–13) — ramo `AttendanceTracking_Dev` (§3)
- [ ] 6. Entità `Par_Formule` più DbSet e FK in `AttendanceTrackingDbContext`, indice unico (IdAz, Codice)
- [ ] 7. Entità `Par_ProfiloFormule` + `Par_ProfiloFormuleDett`, enum `FormulaFase`
- [ ] 8. Entità `Dip_ProfiloFormule` (figlia di `Dip_RapportoLavoro`)
- [ ] 9. Entità `Dip_GG_FormuleLog`
- [ ] 10. Slot `GG_ResultStato` "Errore formula", con la label nel cartellino
- [ ] 11. Repository (×5)
- [ ] 12. DTO e mapper AutoMapper
- [ ] 13. Migrazione EF `AttendanceTracking_Formule`

### 12.2 Motore formule (lavori 14–21) (§4.1–4.3)
- [ ] 14. Pacchetto NuGet Jint in `nvxapp.server.service.csproj`
- [ ] 15. `FormuleScriptHost`: engine con limiti, wrapper della funzione, cache per (Id, Hash)
- [ ] 16. `CtrlSintassi`: parser con riga e colonna, più controllo AST dei membri `x.*`
- [ ] 17. `FormulaContext`: contesto, `Attiva`, `Causale`, `Giustificativo`, `Timbrate`, `Orario`, `Result`, `Stato`, `Log`, `Time`
- [ ] 18. Attributo `[FormulaApi]` e `FormuleCatalogo` (reflection)
- [ ] 19. `FormuleRunner`: risoluzione del profilo nel giorno, filtro delle righe, ordinamento, cattura errori, log in memoria
- [ ] 20. Template della nuova formula e testi di help
- [ ] 21. Insieme di formule di esempio per i test (TogliEccedenza, Sposta, Warning, errore di runtime, ciclo infinito che deve andare in timeout)

### 12.3 Integrazione nel calcolo (lavori 22–27) (§4.4)
- [ ] 22. `Get_OrariSchema_4User`: caricamento di profili, righe e formule in `OrariSchema_4User_OutModel`
- [ ] 23. Aggancio della fase Giorno in `CalcolaGiorno`, dopo `GeneraCausali`
- [ ] 24. Aggancio della fase Finale in `CalcolaRapporto`, passo 4, prima di `ScriviCausali` (dizionario creato se manca)
- [ ] 25. `SaveData`: sostituzione di `Dip_GG_FormuleLog` per giorno
- [ ] 26. Modalità `SoloSimulazione` (senza salvataggio) con risultato prima e dopo
- [ ] 27. Verifica che un'azienda senza profili non abbia impatto sulle prestazioni

### 12.4 API (lavori 28–33) (§5)
- [ ] 28. `Par_FormuleService` + controller: GetAll, Get, Put, Delete, Duplica
- [ ] 29. CtrlSintassi, Metodi (con export Excel), Log
- [ ] 30. Prova (simulazione su rapporto e giorno)
- [ ] 31. `Par_ProfiloFormuleService` + controller: GetAll, Get, Put (righe), Delete, Duplica
- [ ] 32. `Dip_ProfiloFormuleService` e integrazione in `Dip_AnagraficaGet/Put`, con controllo di sovrapposizione Dal/Al
- [ ] 33. Controllo `HaRuolo("CompanyPowerAdmin")` su tutte le azioni, messaggi `.resx` it/en

### 12.5 Front formule (lavori 34–41) (§6.1)
- [ ] 34. Client service e modelli `Par_Formule`
- [ ] 35. `formule-list-page` (`nvx-list`, fab nuovo, duplica, elimina)
- [ ] 36. `formule-edit-page`: segmento Dati
- [ ] 37. Editor della formula con autocomplete `x.` e indicatore Ln/Col
- [ ] 38. Controlla sintassi, con evidenza degli errori
- [ ] 39. Pannello Metodi (ricerca, categorie, in evidenza, inserimento)
- [ ] 40. Dialog Prova (dipendente, giorno → risultato e log)
- [ ] 41. Segmento Log

### 12.6 Front profili (lavori 42–46) (§6.2)
- [ ] 42. Client service e modelli `Par_ProfiloFormule`
- [ ] 43. `profilo-formule-list-page`
- [ ] 44. `profilo-formule-edit-page` (testata e righe ordinate)
- [ ] 45. Dialog della riga `edit-par-profilo-formule-dett-dialog`
- [ ] 46. Azioni Duplica e Ricalcola periodo

### 12.7 Dipendente (lavori 47–51) (§6.3)
- [ ] 47. `Dip_ProfiloFormule` in `Dip_Anagrafica4EditModel` (client e server)
- [ ] 48. Sotto-segmento PROF_FORM nel rapporto (visibile solo a CompanyPowerAdmin)
- [ ] 49. Dialog `edit-dip-profilo-formule-dialog` con controllo di sovrapposizione
- [ ] 50. Voce nel fab e gestione degli id negativi per i rapporti nuovi
- [ ] 51. `ParTable.Par_ProfiloFormule` nella cache dei parametri

### 12.8 Menu, route, i18n, permessi (lavori 52–55) (§6.4)
- [ ] 52. Voci "Formule" e "Profili formule" nel menu Parametri (zorder 207600 e 207700)
- [ ] 53. Route con `RoleGuard4CompanyPowerAdmin` e moduli lazy
- [ ] 54. Chiavi i18n it/en (menu, pagine, dialog, segmento)
- [ ] 55. Verifica della visibilità: CompanyAdmin e User non vedono voci, route e segmento

### 12.9 AI formula (lavori 56–59) (§7)
- [ ] 56. System prompt versionato con il catalogo metodi e gli esempi
- [ ] 57. `GeneraAI` con `CompleteJsonAsync`, validazione e un nuovo tentativo
- [ ] 58. Configurazione del provider `AI:FormuleLLM` (facoltativa)
- [ ] 59. Dialog AI nel front (Sostituisci, Accoda, Copia)

### 12.10 Verifiche e merge (lavori 60–66) (§10)
- [ ] 60. Build del server e `ng build` del client
- [ ] 61. Prova end-to-end: creare una formula, metterla in un profilo, assegnarla a un dipendente, calcolare, controllare le causali
- [ ] 62. Prova con errore di runtime e con timeout: warning sul giorno e log presenti, calcolo completato
- [ ] 63. Prova sui giorni bloccati (mese approvato) e sul cambio di profilo a metà mese
- [ ] 64. Commit locale sul ramo di lavoro
- [ ] 65. Prova di merge in un worktree temporaneo su AllFunction_Dev (csproj, migrazioni, resx)
- [ ] 66. Catena di merge AttendanceTracking_Dev → AttendanceTracking → AllFunction_Dev → AllFunction, **previa conferma**

---

## 13. Lavori fatti (registro)

| Data | Sezione | Lavoro | Ramo | Commit | Note |
|---|---|---|---|---|---|
| 11/11/2026 | — | Analisi | `AttendanceTracking_Dev_formule` | — | Documento creato |
