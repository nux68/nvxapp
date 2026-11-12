# Analisi – Richieste da approvare comuni a tutti gli applicativi (Infrastructure)

Analisi del 12/11/2026, branch **Infrastructure_Dev** (lavoro su `Infrastructure_Dev_richieste`, worktree `wt_infra_richieste`).
Documenti collegati: `ANALISI_ASSETS_APPLICATIVO.md` (decisione `AST-D4`, prenotazioni senza approvazione),
`ANALISI_INFRASTRUCTURE_NUOVI_MODULI.md` (servizi comuni `INF-SVC`), `ANALISI_INFRASTRUCTURE_WIDGET.md` (stub per ramo).

Stato: **variante del 12/11/2026 (§3bis): `Dip_GG_Richiesta` in Common; lavori in corso**.

---

## 1. Richiesta

> Esiste già un'approvazione delle richieste: aggiungere le prenotazioni di Risorse aziendali al ciclo standard
> (timbrature / giustificativi / note spese), quindi sia per admin che per user si dovrà accedere alla pagina delle
> richieste **anche se Presenze non è attivo**.

## 2. Situazione di partenza

Oggi il ciclo delle richieste è **tutto dentro Gestione Presenze**:

| Parte | Dove | Vincolo |
|---|---|---|
| Dati | `Dip_GG_Richiesta` (testata, `Dati` JSON per tipo, cronologia approvazioni JSON, revoca) nello schema di Presenze `tenant_<id>_2` | Richiedente = `IdDip_RapportoLavoro` (rapporto di lavoro di Presenze) |
| Approvatori | `Az_SediRepartoUser` (`EnabledToApproval`, `ApprovalZOrder`) e `Az_Cfg.ApprovazioneTipo` (`SigleAdmin` / `AllAdmin` / `AllAdminHierarchy`) | Schema di Presenze |
| Servizio | `Dip_GG_RichiestaService` (`GetAll4User`, `GetAll4Admin`, `RichiestaCount`, `Send`, `SetState`) | Effetti dell'esito cablati per tipo (timbrature, giustificativi, note spese, scambio turni, ricalcolo del cartellino) |
| Pagine | `requestlistuser`, `requestlistadmin` (+ pagine di invio) in `pages/GestionePresenze` | Esistono **solo** nel ramo AttendanceTracking; rotte con `applicationGuard(AttendanceTracking)` |
| Menu e badge | `main-menu-attendance-tracking.service.ts` con `RichiestaNotifierService.daApprovare` / `inAttesa` | Il notifier restituisce 0 se Presenze non è attivo |
| Aggiornamento | evento SignalR `RichiestaNotifier` | Solo refresh di liste e contatori, nessuna notifica (`INotificationSender`) |
| Widget, chat AI | `attendance.requests`, `ApproveRequestsHandler`, `PendingRequestsHandler` | Legati a Presenze |

Non esiste un punto di estensione generico (l'unico è `IDip_TurnoScambioHandler`, cablato su un valore dell'enum).
Le prenotazioni di Risorse aziendali (`Ast_Prenotazione`) hanno solo gli stati Confermata/Annullata.

Problemi trovati nel codice attuale di Presenze (da correggere, vedi `RICH-AT.4`):
- `SetState` non controlla che l'utente sia un approvatore o un amministratore: con `SigleAdmin` qualunque utente autenticato può approvare chiamando l'API.
- `Get_Dip_GG_Richiesta`: `lista[idxReq - 1]` senza controllo `idxReq > 0` (in `AllAdminHierarchy` il primo approvatore va fuori indice); il filtro per anno/mese è commentato (carica tutte le richieste).

## 3. Soluzione proposta: "hub" delle richieste in Infrastructure

Stesso schema già usato per calendario (`ICalendarEventProvider`), riepilogo AI (`IDigestProvider`), notifiche
(`INotificationCategoryProvider`): **Infrastructure possiede le pagine e il contratto, ogni applicativo porta le sue richieste**.
I dati **restano nei moduli** (nessuna migrazione di `Dip_GG_Richiesta`): l'hub li aggrega.

```
 Pagine "Richieste" (Infrastructure)          RequestHubService (Infrastructure)
  /requests        (le mie richieste)  ──►   per ogni IRequestProvider con applicativo attivo:
  /requestsadmin   (da approvare)              ├─ AttendanceTracking: Dip_GG_Richiesta (esistente)
  badge menu, widget                           └─ Assets: Ast_Prenotazione "in attesa"
```

### 3.1 Server (Infrastructure)

Cartella `ClientServer-Service/infrastructure/Request/`:

- `IRequestProvider` (scoperto dallo scanner dei servizi, come gli altri provider):
  - `ApplicationType Application`
  - `Task<RequestCounts> CountAsync()` → `DaApprovare`, `InAttesa` (per l'utente corrente)
  - `Task<List<RequestItem>> ListUserAsync(RequestFilter)` / `ListAdminAsync(RequestFilter)`
  - `Task ApproveAsync(string key)`, `RejectAsync(string key, string? motivo)`, `CancelAsync(string key)` (annullamento o revoca da parte del richiedente)
  - ogni provider controlla **lui** i permessi (chi può approvare) e applica gli effetti dell'esito
- `RequestItem`: `Key` stabile e unica tra i moduli (es. `at.richiesta-12`, `ast.prenotazione-5`), `Application`, `Tipo` (codice) e `TipoTesto` (già tradotto), `Richiedente`, `Dal`, `Al`, `Dettaglio` (testo), `Stato` generico (`InAttesa`, `InApprovazione`, `Approvata`, `Rifiutata`, `Annullata`) e `StatoTesto` (testo del modulo, es. "approvata, revoca in attesa"), `CanApprove`, `CanReject`, `CanCancel`, `Url` facoltativo (pagina di dettaglio), `Extra` (chiave per azioni client del modulo, es. dettaglio nota spese).
- `RequestHubService` + `RequestController` (`api/Request/RequestCount`, `RequestListUser`, `RequestListAdmin`, `RequestApprove`, `RequestReject`, `RequestCancel`): chiama solo i provider il cui applicativo è attivo per l'azienda (gli altri non vengono toccati: niente `ApplicationNotActiveException`); smista approva/rifiuta/annulla al provider in base al prefisso della `Key`.
- Evento SignalR generico `RequestNotifier` (`IRequestNotifier.NotifyAsync(utenti)`): i provider lo chiamano quando una richiesta cambia; il client ricarica contatori e liste.
- Categoria di notifica `richieste` (`INotificationCategoryProvider` di Infrastructure): "nuova richiesta da approvare" agli approvatori, "richiesta approvata/rifiutata" al richiedente. I moduli la usano tramite `INotificationSender`.

### 3.2 Client (Infrastructure)

- Pagine `pages/infrastructure/request-list-user-page` (`/requests`) e `request-list-admin-page` (`/requestsadmin`):
  colonne Applicativo/Tipo, Richiedente (solo admin), Dal, Al, Dettaglio, Stato; filtro Da approvare / Tutte / Approvate (admin),
  ricerca testo, filtro per applicativo quando ce n'è più di uno; azioni Approva, Rifiuta (con motivo), Annulla/Revoca, Dettaglio.
- `RequestNotifierService` (Infrastructure): signal `daApprovare` / `inAttesa` dal server (somma di tutti i provider), ascolta `RequestNotifier`.
- Menu (`main-menu-infrastructure.service.ts`): "Richieste da approvare" (CompanyPowerAdmin, CompanyAdmin) e "Le mie richieste" (User, e gli admin che hanno richieste proprie) con `anyApplication: [AttendanceTracking, Assets]` e badge dal nuovo notifier.
  Le voci delle **pagine di invio** restano nei rami (richiesta giustificativo/timbratura/rimborso in Presenze, nuova prenotazione in Assets).
- Estensione per ramo (stub vuoti in Infrastructure, come widget/menu/rotte): `request-ui-attendance-tracking.service.ts`, `request-ui-assets.service.ts` con `iRequestUiProvider` → azioni aggiuntive per `Extra` (es. dialog di dettaglio della nota spese) e pulsanti "Nuova richiesta" della pagina utente.
- Widget `infrastructure.requests` ("Richieste": da approvare / in attesa) al posto di `attendance.requests`.

### 3.3 Presenze (AttendanceTracking)

- `AtRequestProvider : IRequestProvider` che **riusa** `Dip_GG_RichiestaService` (`GetAll4User`, `GetAll4Admin`, `RichiestaCount`, `SetState`): nessun cambiamento del modello dati né della logica di approvazione (strategie, gerarchia, revoca, effetti sul cartellino).
- Testi di tipo/stato/dettaglio calcolati lato server (oggi li fa il client con `getDatitext` / `get_StatoRichiesta_Approval_text`).
- `requestlistuser` / `requestlistadmin`: le rotte puntano alle nuove pagine (redirect), le voci di menu di Presenze vengono tolte (le porta Infrastructure); il selettore dei reparti della pagina admin diventa un filtro "richiedente/reparto" testuale (la visibilità per reparto la applica già il server).
- `RichiestaNotifier` esistente: continua per le pagine di Presenze che lo usano (cartellino, piano ferie) e in più chiama il notifier generico.
- Widget `attendance.requests`, chat AI (`ApproveRequestsHandler`, `PendingRequestsHandler`): passano dall'hub, così includono anche le prenotazioni. La rotta vocale "richieste" punta a `/requests` / `/requestsadmin`.

### 3.4 Risorse aziendali (Assets) – prenotazioni con approvazione (chiude `AST-D4`)

- `Ast_Risorsa.RichiedeApprovazione` (bool, predefinito no; es. sì per le auto in pool).
- `AstStatoPrenotazione`: + `InAttesa = 2`, `Rifiutata = 3`; `Ast_Prenotazione` + `IdAspNetUsersDecisione`, `DataDecisione`, `MotivoRifiuto`. Migration `AstPrenotazioniApprovazione`.
- Prenotazione su una risorsa con approvazione → nasce **InAttesa** e **occupa già la fascia** (il controllo dei conflitti considera Confermata + InAttesa), così due persone non chiedono la stessa auto alla stessa ora.
- Approvatori: **gli amministratori dell'azienda** (CompanyPowerAdmin, CompanyAdmin) — approvazione singola: basta uno (vedi §7, D1).
- `AstRequestProvider : IRequestProvider`: elenco/conteggio delle prenotazioni InAttesa (admin) e delle proprie (user); approva → Confermata, rifiuta → Rifiutata con motivo (libera la fascia), annulla da parte dell'utente.
- Notifiche (categoria `richieste`, rispettando `Ast_Cfg.NotificheAttive`): nuova richiesta agli amministratori, esito al richiedente.
- Feed .ics e widget "Le mie prenotazioni": solo le confermate (le InAttesa con indicazione "in attesa" nella pagina Prenotazioni).
- Scadenza: una richiesta ancora InAttesa all'ora di inizio viene annullata automaticamente (dal servizio giornaliero esistente o al primo accesso), con notifica.
- Pagina Prenotazioni e "Le mie prenotazioni": stato InAttesa/Rifiutata visibile; pagina risorsa: spunta "Richiede approvazione".
- Comandi vocali: `AstBookResource` risponde "richiesta inviata, in attesa di approvazione" quando la risorsa la richiede.

## 3bis. Variante del 12/11/2026 (sostituisce la parte "dati" del §3)

Richiesta dell'utente: **`Dip_GG_Richiesta` esce dallo schema delle Presenze e va in Common**; le approvazioni delle
Presenze restano come sono (responsabili di reparto, strategie, gerarchia, revoca); le prenotazioni di Risorse
aziendali si appoggiano a `Dip_GG_Richiesta` per il funzionamento di base; la pagina delle approvazioni resta quella
comune di Infrastructure (§3.2). Decisioni: le prenotazioni le approvano gli **amministratori dell'azienda** (D1 invariata);
il DB si ricrea: le modifiche entrano nelle migration `Init` unificate (`InitCommon`, `AttendanceTracking_Init`, `InitAssets`), nessuna copia dei dati.

**Dati (Common, come `Dip_RapportoAutovetture` in INF-COM.3)**
- `Dip_GG_Richiesta` (partial) in `Entities/Tenant/Common`, stessi campi di oggi piu':
  `IdAz_Anagrafica` (azienda, obbligatorio), `IdAspNetUsers` (utente richiedente), `IdDip_Anagrafica` (facoltativo: chi prenota
  una risorsa puo' non essere un dipendente censito), `IdDip_RapportoLavoro` diventa **facoltativo e senza FK** in Common
  (lo valorizzano le Presenze). Enum `TipoRichiesta` + `PrenotazioneRisorsa = 5`; `StatoRichiesta` e la cronologia invariati.
- Le navigazioni verso le Presenze (rapporto di lavoro, giustificativi, timbrature, note spese) stanno in
  `Dip_GG_Richiesta.Presenze.cs` del ramo AttendanceTracking; il contesto delle Presenze mappa la tabella con
  `ExcludeFromMigrations()`. Scritture dal repository Common (`Dip_GG_RichiestaCommonRepository`).

**Motore**
- Presenze: `Dip_GG_RichiestaService` resta il motore dei suoi tipi (Send, SetState con permessi, effetti sul cartellino), sulla tabella comune.
- Infrastructure: `RichiestaCommonService`, motore semplice per i tipi approvati dagli amministratori dell'azienda
  (oggi: prenotazioni): crea la richiesta con l'elenco degli amministratori nella cronologia, approva/rifiuta/annulla,
  notifica (categoria `richieste`) e chiama il gestore del tipo per gli effetti.
- Pagine: `RequestService` legge **direttamente** la tabella comune (mie = `IdAspNetUsers`; da approvare = richieste
  dell'azienda che l'amministratore puo' decidere adesso, calcolate dalla cronologia e dalla strategia del tipo);
  i testi e le azioni li da' il gestore del tipo `IRichiestaTipoHandler` (sostituisce `IRequestProvider`):
  Presenze → `SetState` esistente; Risorse aziendali → `RichiestaCommonService` + aggiornamento di `Ast_Prenotazione`.

**Risorse aziendali**: `Ast_Prenotazione.IdDip_GG_Richiesta` (richiesta collegata, se la risorsa richiede approvazione) e
lo stato della prenotazione allineato all'esito (serve al controllo dei conflitti); tolte le colonne di decisione
aggiunte prima (`IdAspNetUsersDecisione`, `DataDecisione`, `MotivoRifiuto`, `RichiestaApprovazione`): stanno nella richiesta.

**Cosa resta del lavoro precedente**: pagine, menu, badge, widget, notifier SignalR, categoria di notifica, testi,
comandi vocali, client di Presenze e Risorse aziendali, correzioni di `SetState`. Si rifanno: il contratto server
dell'hub, i provider dei due moduli, la parte dati di Risorse aziendali.

## 4. Visibilità e permessi

| Ruolo | Le mie richieste | Richieste da approvare |
|---|---|---|
| User | sì (richieste di Presenze e prenotazioni in attesa/rifiutate) | no |
| CompanyAdmin / CompanyPowerAdmin | sì, se ha richieste proprie | sì: Presenze secondo gli approvatori dei reparti (invariato), Assets tutte le prenotazioni in attesa |
| Studio | no | no |

Le pagine sono di Infrastructure: visibili se l'azienda ha **almeno uno** tra Presenze e Risorse aziendali attivo; ogni provider espone solo le richieste del proprio applicativo attivo.

## 5. Ordine dei lavori e merge

1. `Infrastructure_Dev_richieste` (hub, pagine, notifier, menu, stub) → `Infrastructure_Dev` → `Infrastructure`.
2. `AttendanceTracking_Dev` (riceve Infrastructure): `AtRequestProvider`, redirect e pulizia del menu, correzioni `RICH-AT.4`.
3. `Assets_Dev` (riceve Infrastructure): approvazione delle prenotazioni e `AstRequestProvider`.
4. Catena "Applicativo completo" verso `AllFunction_Dev` / `AllFunction` (conflitti attesi: stub `request-ui-*.service.ts`, voci di menu, file i18n e `*Messages.resx`).

## 6. Rischi

- Cambia la pagina più usata di Presenze: va provata con tutte e tre le strategie di approvazione e con la revoca.
- Le funzioni Approva/Rifiuta usate dentro cartellino e piano ferie (`time-sheet-admin-page`, `vacation-plan-page`) restano su `Dip_GG_RichiestaService`: devono continuare a funzionare invariate.
- Prestazioni: la pagina admin chiama tutti i provider; i provider devono filtrare lato server (Presenze oggi carica tutte le richieste, `RICH-AT.4`).

## 7. Decisioni (confermate il 12/11/2026)

| # | Decisione | Scelta |
|---|---|---|
| D1 | Chi approva le prenotazioni | ✅ Gli amministratori dell'azienda (CompanyAdmin/CompanyPowerAdmin), basta uno |
| D2 | Pagine di Presenze | ✅ Sostituite dalle pagine dell'hub (redirect delle vecchie rotte) |
| D3 | Correzioni di sicurezza/robustezza di `SetState` e `Get_Dip_GG_Richiesta` | ✅ Farle subito in `RICH-AT.4` |
| D4 | Richiesta non decisa all'ora di inizio | ✅ Annullata automaticamente con notifica |
| D5 | Dove stanno le richieste | ✅ (variante 12/11) `Dip_GG_Richiesta` in Common per tutti i moduli (§3bis) |
| D6 | Dati esistenti | ✅ Il DB si ricrea: modifiche nelle migration `Init`, nessuna copia |

## 8. Checklist

Legenda: ✅ svolto · ⏳ da fare · 🔄 in corso · 🔍 da decidere

#### Analisi
- ✅ `RICH-0` Studio del ciclo attuale di Presenze e dei provider di Infrastructure; analisi scritta
- ✅ `RICH-D` Decisioni D1–D4 (§7) confermate il 12/11/2026

#### `RICH-INF` – Infrastructure
- ✅ `RICH-INF.1` (prima versione, a provider) `IRequestProvider`, `RequestHubService`, `RequestController`, notifier SignalR generico
- ✅ `RICH-INF.2` Categoria di notifica `richieste` e testi (it/en)
- ✅ `RICH-INF.3` Pagine `/requests` e `/requestsadmin`, `RequestNotifierService`, menu con badge, stub `request-ui-*`, comandi vocali
- ✅ `RICH-INF.4` Widget `infrastructure.requests`
- ✅ `RICH-INF.8` Badge sull'intestazione dei gruppi del menu (es. "Servizi"): somma dei contatori delle voci visibili, come per le sezioni degli applicativi; senza, a gruppo chiuso l'arrivo di una richiesta non si vedeva (prima le voci delle richieste stavano nella sezione Presenze, che il totale lo mostrava)
- ✅ `RICH-INF.9` Ciclo completo delle richieste approvate dagli amministratori (`RichiestaCommonService`), come le presenze: richiesta → approvata / rifiutata (fine); approvata → il richiedente puo' chiedere l'annullamento (`RevocaStato`), che va approvato o rifiutato; l'annullamento in attesa si puo' ritirare. Provato sulle prenotazioni (19 controlli)
- ✅ `RICH-INF.6` (variante, commit 4f232d79) `Dip_GG_Richiesta` in Common, `CommonDbContext`, repository, `InitCommon` rigenerata
- ✅ `RICH-INF.7` (variante) `IRichiestaTipoHandler`, `RequestService` sulla tabella comune, `RichiestaCommonService`
- 🔄 `RICH-INF.5` Build server e client ✅; merge in `Infrastructure` con la catena finale

#### `RICH-AT` – Presenze
- ✅ `RICH-AT.1` (prima versione) `AtRequestProvider`, testi lato server — da trasformare in gestore del tipo (`RICH-AT.6`)
- ✅ `RICH-AT.2` Redirect di `requestlistuser`/`requestlistadmin`, voci di menu tolte, dettaglio nota spese come azione `request-ui-attendance-tracking`
- ✅ `RICH-AT.3` Widget `attendance.requests` tolto (sostituito da `infrastructure.requests`), chat AI e rotte vocali
- ✅ `RICH-AT.4` Correzioni: permessi in `SetState`, indice della gerarchia, AsNoTracking, `vistaApprovatore`; filtro per periodo lasciato disattivato (motivato)
- ✅ `RICH-AT.5` Prova API su DB usa e getta (SigleAdmin, revoca, gerarchia)
- ✅ `RICH-AT.6` (variante, branch `AttendanceTracking_Dev_richieste`, commit 78aea959) Tabella in Common: partial con le navigazioni, mapping `ExcludeFromMigrations`, scritture dal repository Common (`SalvaRichiestaAsync` valorizza azienda/dipendente/utente), `AtRichiestaTipoHandler`, `AttendanceTracking_Init` rigenerata; prova API ok (SigleAdmin, revoca, gerarchia, FK tra schemi)
- 🔍 `RICH-AT.7` Deleter dei dati delle Presenze (`IApplicationDataDeleter`) assente: eliminando lo schema delle presenze le richieste in Common restano finché non si cancella il dipendente o l'azienda
- ✅ `RICH-AT.8` Voci "Richieste" di nuovo anche nel menu Presenze (amministratori: da approvare; dipendenti: le mie), con i badge, verso le pagine comuni

#### `RICH-AST` – Risorse aziendali
- ✅ `RICH-AST.1`–`RICH-AST.5` (prima versione, ciclo proprio in `Ast_Prenotazione`): spunta "Richiede approvazione", conflitti con le in attesa, scadenza pigra, notifiche, client, comandi vocali, analisi del modulo; prova API 25/25
- ✅ `RICH-AST.6` (variante, branch `Assets_Dev_richieste`, commit 46147cd0) Prenotazione appoggiata a `Dip_GG_Richiesta` (`IdDip_GG_Richiesta`), colonne di decisione tolte, `AstRichiestaTipoHandler` su `RichiestaCommonService`, `InitAssets` rigenerata; prova API 33/33

#### Merge
- ⏳ `RICH-M` DB ricreato e catena: Infrastructure → AttendanceTracking, Assets → Applicativo completo (AllFunction_Dev, AllFunction)
