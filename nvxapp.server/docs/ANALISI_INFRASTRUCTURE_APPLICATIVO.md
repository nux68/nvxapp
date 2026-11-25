# Analisi – Applicativo "Funzioni base" (Infrastructure)

Branch: **Infrastructure_Dev** (si mergia in Infrastructure; da lì i merge vanno verso i moduli e poi verso AllFunction, mai al contrario).
Documento d'insieme del ramo comune a tutti i moduli. Stato: **analisi del codice com'è oggi**, scritta il 25/11/2026 leggendo `Infrastructure_Dev` (worktree `wt_infra_analisi`); i punti di estensione sono stati controllati in sola lettura su `AllFunction` per vedere chi li usa. Nessuna modifica al codice.
Gemelli per ramo: `ANALISI_CRM_APPLICATIVO.md`, `ANALISI_ASSETS_APPLICATIVO.md`, `ANALISI_COMPLIANCE_APPLICATIVO.md`, `ANALISI_HUMANRESOURCES_APPLICATIVO.md`, `ANALISI_DOCUMENTMANAGEMENT_APPLICATIVO.md`, `ANALISI_ATTENDANCETRACKING_APPLICATIVO.md` (stanno nei rispettivi rami).

Legenda dei marcatori usati nel testo: **[V]** verificato nel codice (file e classe/metodo citati) · **[S]** sospetto, letto ma non provato · **[?]** da verificare (non verificabile in sola lettura: database, esecuzione, rami non letti). Non sono stati avviati server, build, test né database.

---

## 1. Scopo del documento e come usarlo

**A chi serve.** Principalmente a un agente "esperto base" (e agli sviluppatori) che deve rispondere a domande sulle funzioni comuni, valutare l'impatto di una modifica su tutti i moduli, controllare una modifica prima del merge.

**Come si legge.**
- Per sapere *dove sta una cosa*: §2 (progetti), §6 (mappa per area: pagine → servizio → entità → documento).
- Per sapere *come un modulo si aggancia alle funzioni base*: **§7** (punti di estensione), la parte più utile.
- Per sapere *cosa si rompe se tocco X*: §11.
- Per sapere *cosa non fidarsi dei documenti*: §12 (il codice fa fede).
- Per i lavori ancora da fare: §14 (checklist, codici `FB-…`).

**Convenzioni.** I percorsi sono relativi alla radice della solution. Dei riferimenti si cita file e classe/metodo, mai la riga. "Common" è lo schema dei dati comuni dell'azienda (`ApplicationType.Common = 0`). Nei moduli "AllFunction" è il ramo con tutti gli applicativi.

### 1.1 Documenti di dettaglio (tutti nel ramo Infrastructure)

| Documento | Contenuto | Stato (letto il 25/11/2026) |
|---|---|---|
| `nvxapp.server/docs/ANALISI_INFRASTRUCTURE_NUOVI_MODULI.md` | Raggruppamento delle funzioni nei moduli, dati spostati in Common, servizi comuni (`INF-SVC`, `INF-CAL`, `INF-AI`, `INF-COM`, `INF-APP`), procedura per un nuovo applicativo (§7), elenco delle aree di lavoro | Quasi tutte le aree ✅; restano `DM-PEC`, `DM-CONS`, `CMP-WB` (bloccata), `INF-CAL.4`, `ADM`, `ANA.5/6/10`. Il §11 "Da fare" è obsoleto [V] |
| `…/ANALISI_INFRASTRUCTURE_LICENZE.md` | Listini, piani Free/Standard/Pro, abbonamenti, `ILicenseGuard`, gateway di pagamento, disdetta | Fasi 0-2 fatte, PayPal sandbox (§7.4) fatto; intestazione non aggiornata; 3 punti aperti |
| `…/ANALISI_INFRASTRUCTURE_PAGAMENTI_CENTRI.md` | Ripartizione degli incassi ai centri (`Dealer`), rendiconti, riepiloghi | Implementata, 3 punti aperti |
| `…/ANALISI_INFRASTRUCTURE_RICHIESTE.md` | Richieste da approvare comuni (`Dip_GG_Richiesta` in Common, `IRichiestaTipoHandler`) | Implementata; 4 punti ancora ⏳/🔄 (catena dei merge: probabilmente superati) |
| `…/ANALISI_INFRASTRUCTURE_CANCELLAZIONE.md` | Cancellazione fisica di centri, studi, aziende, utenti | Implementata; 3 punti aperti (deleter presenze, token di utente eliminato, cancellazione in background) |
| `…/ANALISI_INFRASTRUCTURE_CHAT_AI.md` | Chat AI e comandi vocali (motore, slot, handler) | 40/41 punti; voce da provare nel browser |
| `…/ANALISI_INFRASTRUCTURE_INVIO_VOCALE_MANUALE.md` | Invio manuale dei comandi dettati | Implementata; test manuale aperto |
| `…/ANALISI_INFRASTRUCTURE_MCP.md` | Server MCP per AI esterne, token personali, livelli, impersonificazione | Fasi 1-4 e 6 fatte, fase 5 (OAuth/claude.ai) in sospeso; 8 punti aperti |
| `…/ANALISI_INFRASTRUCTURE_WIDGET.md` | Catalogo widget della dashboard (stub per ramo) | Implementata; documento generale |
| `…/ANALISI_INFRASTRUCTURE_SHOWCASE.md` | Home, Funzionalità, Prezzi composte per ramo | Implementata (24/11), 1 verifica visiva aperta |
| `…/ANALISI_INFRASTRUCTURE_DOCKER.md` | Distribuzione Docker/Caddy, segreti in variabili d'ambiente | 11/14 risolti, 3 non necessari con un'istanza; manca la preparazione del VPS (stato 27/10) |
| `…/ANALISI_INFRASTRUCTURE_PERSONALIZZAZIONI.md` | Richieste di funzioni su misura a pagamento | **Solo analisi** (22/11): 44 punti ⏳/🔍, nessun codice [V: nessuna entità `Pers_*`] |
| `…/ANALISI_INFRASTRUCTURE_CONFRONTO_BIZNEO.md` | Confronto funzionale con Bizneo HR | Prima stesura (20/11), 30 voci da verificare |
| `nvxapp.server.service/ANALISI_INFRASTRUCTURE_TICKET.md` | Ticket di assistenza a livelli | Implementato, ma l'intestazione dice "nessun codice scritto" [V, §12] |
| `piano-security.md` (radice) | Piano di messa in sicurezza di autenticazione/autorizzazione | Analisi in pausa; **in parte superato dal codice** (§4.5) |
| `nvxapp.server/docs/CONTRIBUTING*.md`, `PATTERN_AND_CONVENTIONS.md`, `QUICK_REFERENCE.md`, `GENERAZIONE_TABELLE_GUIDA_COMPLETA.md`, `TROUBLESHOOTING.md` | Linee guida di sviluppo | Utili per i pattern, **datati** nei dettagli (§12) |
| `nvxapp.server.service/Manuals/infrastructure*.md` | Manuale utente delle funzioni base (testo + guida) | Introduzione ferma a "presenze, CRM, documentale" [V] |
| `nvxapp.server/Note/*.txt` (`Migrazioni.txt` ecc.) | Note operative (migration, variabili d'ambiente, git) | `Migrazioni.txt` utile; versioni citate datate |

---

## 2. Architettura

### 2.1 Progetti e strati

| Progetto | Ruolo | Dove guardare |
|---|---|---|
| `nvxapp.server` | Web API (.NET 11, `net11.0`): controller, registrazioni DI, servizi in background, endpoint MCP, file statici del client | `Program.cs`, `Utility/Installers*.cs`, `Utility/McpEndpoint.cs`, `Utility/*MaintenanceService.cs`, `Controllers/Infrastructure/*` |
| `nvxapp.server.service` | Logica di business: `ServiceBase`, servizi, modelli `GenericRequest/GenericResult`, mapper, hub SignalR, RabbitMQ, manuali (risorse `.md`) | `Base/ServiceBase.cs`, `ClientServer-Service/infrastructure/*`, `ClientServer-Service/ChatAiHandler/infrastructure/*`, `Hub/SignalRHub.cs`, `Manuals/` |
| `nvxapp.server.data` | Entità, `DbContext`, repository, tenancy, messaggi (`resx`), migration | `Infrastructure/*DbContext*.cs`, `Infrastructure/Tenancy/*`, `Entities/Public`, `Entities/Tenant/Common`, `Repositories/*`, `Migrations/*`, `Localization/*` |
| `nvxapp.client` | Angular 22 + Ionic 9, solo codice in `src/nvx` (non in `src/app/nvx`) | `src/nvx/pages/infrastructure`, `src/nvx/Utility/infrastructure`, `src/nvx/shared`, `src/nvx/ClientServer-Service/Infrastructure` |
| `tools/ChatAiPhraseTest`, `nvxapp.server/Tools/ManualScreenshots` | Prove delle frasi della chat; script che rigenera le schermate dei manuali | rispettivi `README.md` |
| `Dockerfile`, `docker-compose*.yml`, `docker/Caddyfile*` | Immagine unica client+server, PostgreSQL, RabbitMQ, reverse proxy | `ANALISI_INFRASTRUCTURE_DOCKER.md` |

Dipendenze: `nvxapp.server` → `nvxapp.server.service` → `nvxapp.server.data`. Non esiste un progetto di test del server [V: `nvxapp.sln`]; il client ha file `*.spec.ts`.

Flusso di una chiamata [V]: client `HttpClient.post(apiUri + 'Controller/Metodo', GenericRequest<T>)` → controller (`NvxControllerBase`, route `api/[controller]`, `[Authorize]` per azione) → servizio (`ServiceBase.ExecuteAction`) → repository (`Repository<TDbContext,T>`) → `DbContext`. Il servizio risponde sempre con `GenericResult<T>` (`success`, `data`, `messages`, `token`); l'interceptor del client (`http/http-interceptor.ts`) aggiunge `Authorization` e `Accept-Language`, sostituisce il token con quello della risposta e apre il dialog comune degli errori se `success = false`.

Registrazione automatica [V: `Installers.InstallServices`, `InstallRepositories`, `InstallMappers`]:
- tutte le classi pubbliche non generiche di `nvxapp.server.service` sono registrate **Scoped** con le interfacce che implementano (`NetCore.AutoRegisterDi`); sono escluse le `IRabbitMqListenerService`, le `IRouteProvider` (Singleton, registrate a parte) e i `record`;
- i repository: tutte le classi di `nvxapp.server.data.Repositories*`, Scoped;
- AutoMapper: `AddMaps` su tutti gli assembly già caricati all'avvio;
- i servizi **in background** (`IHostedService`) vanno nel progetto `nvxapp.server` (cartella `Utility`) e registrati a mano in `Installers.InstallServices`: nello scanner del service diventerebbero Scoped (lo dice il commento di `JobFileCleanupService`).

### 2.2 Rami e flusso dei merge

| # | Ramo di sviluppo | Si mergia in | Riceve i merge da |
|---|---|---|---|
| 1 | `Infrastructure_Dev` | `Infrastructure` | – |
| 2-7 | `AttendanceTracking_Dev`, `CRM_Dev`, `DocumentManagement_Dev`, `HumanResources_Dev`, `Compliance_Dev`, `Assets_Dev` | il ramo senza `_Dev` | `Infrastructure` |
| 8 | `AllFunction_Dev` | `AllFunction` | `Infrastructure` per primo, poi i sei moduli |

Regole (dal `CLAUDE.md`): i merge vanno **sempre da Infrastructure verso i moduli**, mai al contrario; modifiche su worktree dedicato; niente commit verso `origin`; la catena si esegue solo su conferma. "Amministrazione" (`ApplicationType 8`) è riservata e non esiste. Conseguenza pratica: **un file di Infrastructure si modifica solo su `Infrastructure_Dev`**, anche quando serve a un modulo; il modulo lo riceve col merge.

### 2.3 Cosa può e cosa non può stare in Infrastructure

**Può** (e deve): identità, gerarchia centro/studio/azienda, ruoli, applicativi attivi, licenze, multi-azienda e provisioning, dati comuni (Common) e relativi servizi, servizi trasversali (notifiche, scadenzario, calendario, presa visione, richieste, riepilogo, ticket, storage, export/job, chat AI, MCP, widget, manuali), le **interfacce** dei punti di estensione con la loro implementazione "base", gli **stub vuoti per ramo** (client: `route-<modulo>.service.ts`, `main-menu-<modulo>.service.ts`, `widget-…`, `showcase-…`, `i18n-…`, `site-name-…`; server: `Installers4<Modulo>.cs`), il valore dell'enum `ApplicationType` di ogni modulo.

**Non può**: tabelle o logica di un solo modulo; riferimenti (using, navigazioni EF, chiamate) a classi di un modulo; campi di un solo modulo nelle tabelle Common (regola: un dato di un solo modulo va in una tabella di estensione 1-1 del modulo, `Az_Cliente.cs`). Il contesto Common **ignora** per costruzione le navigazioni verso le tabelle degli applicativi (`CommonDbContext.IgnoreApplicationNavigations`), così le entità comuni possono essere `partial` e ogni ramo vi aggiunge le sue navigazioni.

Dove Infrastructure oggi conosce i moduli comunque: §12.4.

---

## 3. Multi-azienda e dati

### 3.1 Applicativi, contesti e schemi [V: `Tenancy/ApplicationType.cs`, `DataLayerInstaller`]

| Valore | `ApplicationType` | Contesto | Schema multi-azienda | Attivazione |
|---|---|---|---|---|
| – | (tabelle comuni) | `PublicDbContext` | `public` (sempre) | – |
| 0 | `Common` | `CommonDbContext` | `tenant_<IdAzienda>_0` | **sempre**, mai in `CompanyApplication` |
| 1 | `Moke` | `MokeDbContext` | `tenant_<IdAzienda>_1` | demo (`MyTable`) |
| 2-7 | `AttendanceTracking`, `Crm`, `DocumentManagement`, `HumanResources`, `Compliance`, `Assets` | un contesto per ramo (`AddApplicationDbContext<T>` in `Installers4<Modulo>`) | `tenant_<IdAzienda>_<n>` | `CompanyApplication` (+ licenza) |
| 8 | `Administration` | riservato, non esiste | – | – |

Il valore numerico finisce nel nome dello schema: **non si cambia mai**. L'enum esiste anche in TypeScript (`user-load-model.ts`) e va tenuto uguale.

### 3.2 Modalità singola e multi-azienda
- `DbParameter:MultiTenant` si decide **al primo avvio** su database vuoto: viene salvato in `AppSetting` (chiave `TenancyMode`) e agli avvii successivi un valore diverso **ferma l'avvio** (`DatabaseInitializer.ResolveTenancyModeAsync`). Il default di `appsettings.json` è `true`.
- Multi-azienda: uno schema per azienda e applicativo. Modalità singola: tutto in `public`; le aziende si distinguono con `Az_Anagrafica.IdCompany` e per questo le query dei dati comuni **devono** filtrare per azienda (in multi-azienda il filtro è ridondante ma non va tolto, il codice deve funzionare in entrambe).

### 3.3 Regole dei contesti [V: `ApplicationDbContextBase`, `DatabaseInitializer.CheckTableNames`, `TenantSearchPathInterceptor`]
- Il modello **non contiene il nome dello schema**: lo decide il `search_path` impostato all'apertura di ogni connessione. Per un applicativo: `"tenant_<id>_<app>", "tenant_<id>_0", public`; per Common: `"tenant_<id>_0", public`; modalità singola: `public`.
- Un nome di tabella appartiene a **un solo contesto** (in singola stanno tutte in `public`): all'avvio un duplicato blocca l'applicazione.
- Un contesto può leggere `public` (mappata con schema `public` esplicito) e Common (mappata **senza** schema), sempre con `ExcludeFromMigrations()` e in **sola lettura**; mai tabelle di un altro applicativo; `public` non referenzia mai tabelle di un applicativo. Le FK reali tra schemi dello stesso database sono ammesse.
- Storico migration separato per contesto: `__EFMigrationsHistory_<Applicativo>` (public: `__EFMigrationsHistory`). Le migration non contengono lo schema e valgono per entrambe le modalità; una migration riguarda un solo contesto (`--context`). Cartelle: `Migrations/Public`, `Common`, `Moke` (e una per modulo nei rami).
- Letture di Common dentro le query di un modulo con repository `<Prefisso>_<Entita>ReadRepository`; scritture solo con `Repositories/Tenant/Common` (`CONTRIBUTING_nvxapp_data.md`, "REGOLA BASE").

### 3.4 Risoluzione dell'azienda e dello schema [V: `TenantSchemaAccessor`, `TenantScope`]
- `ITenantSchemaAccessor.GetSchema(app)`: azienda = `TenantScope.CurrentCompany` (job, inizializzatori) oppure claim `company` del token; senza azienda → `ApplicationNotActiveException`; se l'applicativo è attivabile (tutti tranne Common) e non è attivo in `CompanyApplication` → `ApplicationNotActiveException`. L'elenco degli attivi è letto una volta per richiesta.
- `TenantScope.Use(idCompany)` (AsyncLocal) serve fuori da una richiesta: `RunInBackground` lo imposta dall'azienda del token; `CompanyInitializerRegistry` lo imposta sull'azienda che sta inizializzando.
- Fuori richiesta si usa `IApplicationDbContextFactory.Create(app, schema)` (schema esplicito, nessun controllo di attivazione) e si costruisce `new XRepository(db, serviceProvider, httpContextAccessor)`; è il pattern di deleter, provider e `LicenseGuard.ActiveClientsAsync`.
- Il contesto risolve lo schema alla prima apertura e non lo cambia più; non si riusa un contesto tra aziende.

### 3.5 Migration, provisioning, inizializzatori [V: `DatabaseInitializer.InitializeAsync`, `TenantProvisioningService`]
Ordine all'avvio: controllo nomi tabella → migrate `public` → risoluzione modalità → `MigrateAllAsync` (singola: tutti i contesti in ordine di valore, Common per primo; multi-azienda: **Common di ogni azienda**, poi ogni coppia azienda/applicativo **attivo** con contesto registrato) → `LicenseCatalogSeedRunner` (listini iniziali `ILicenseCatalogSeed`).
- In multi-azienda un errore su uno schema è **registrato e saltato** (le altre aziende partono); in singola l'errore ferma l'avvio. Conseguenza: un'azienda può restare con lo schema non migrato mentre l'app è "su" [V, `MigrateAllAsync`].
- `EnsureApplicationAsync(idCompany, app)` crea/migra lo schema (prima Common) all'attivazione e quando nasce un'azienda; `DropApplicationSchemaAsync` (disdetta licenza, **mai Common**) e `DropCompanySchemasAsync` (cancellazione azienda) usano `DROP SCHEMA … CASCADE` sulla connessione di `PublicDbContext`, quindi dentro la sua transazione.
- Dati iniziali: `Migrations/Public/PopolateDB.cs` (ruoli, utenti iniziali, ramo DEMO) e `PopolateTicketType.cs` dentro la migration `InitPublic`; azienda nuova → `ICompanyInitializer` (§7.1) con `CommonCompanyInitializer` (priorità 5, crea schema Common e `Az_Anagrafica`) e `InfrastructureCompanyInitializer` (10, vuoto); utente nuovo → `IUserInitializer` con `InfrastructureUserInitializer` (10, crea `Dip_Anagrafica`).
- Gli inizializzatori hanno una **cache statica in memoria** per azienda e applicativo (`CompanyInitializerRegistry._initialized`, `CommonCompanyUtility._ensured`): ripristinare un database senza riavviare l'app può lasciare la cache "vera" e lo schema vuoto [S]. `CompanyInitializerRegistry.Forget` la azzera alla disdetta.
- **Moke** (valore 1) è un applicativo di esempio ancora registrato e attivabile (§12.3).

### 3.6 Cambio azienda e applicativo
- Il "cambio azienda" non esiste come azione: l'azienda è nel token. Si cambia con `Account/UserLoad` (impersonificazione/discesa nella gerarchia), che rilascia un nuovo token con `company`, `financialadvisor`, `dealer` dell'utente richiesto e ricarica `ActiveApplications`, `FinancialAdvisorApplications`, `MenuRestrictions`, `License` (`UserLoadModel`).
- Attivazione/disattivazione applicativo: `CompanyApplicationService` (disattivare blocca l'accesso, schema e dati restano). Chi può: `ApplicationActivationAuthorizer` (SuperUser; DealerPowerAdmin del centro; con licenze accese anche FinancialAdvisorPowerAdmin dello studio, e solo se l'azienda è coperta).
- Lato client: `AuthService.hasApplication/hasFinancialAdvisorApplication`, `applicationGuard(app)` aggiunto da `RouteService.forApplication` a ogni rotta del modulo, voci di menu filtrate da `MainMenuService.forApplication`. **Sono aiuti all'interfaccia**: il blocco vero è `TenantSchemaAccessor` sul server.
- Spostamento di un'azienda sotto un altro studio: `CompanyMoveService` (solo SuperUser, l'azienda segue il solo `Company.IdFinancialAdvisor`; ticket aperti riallineati).

---

## 4. Autenticazione, ruoli, permessi, "lavora come", sicurezza

### 4.1 Autenticazione e token [V]
- `Account/Login` (anonimo per assenza di `[Authorize]`): `AccountService.Login` verifica la password con `SignInManager.CheckPasswordSignInAsync(…, lockoutOnFailure: false)` ed emette un JWT (`UtilToken.GenerateJwtToken`) con i claim `sub`, `useridfirstconnection`, `tenant`, `dealer`, `financialadvisor`, `company`. **Nessun claim di ruolo**; `tenant` non è valorizzato da login e `UserLoad` (resta vuoto: residuo, lo usano solo i token MCP).
- `ServiceBase.OkResponse` rigenera il token **a ogni risposta** (tranne i sotto-processi `isSubProcess`); il client lo sostituisce (`NvxHttpInterceptor`). Il token sta solo in memoria nel client (`AuthService`): un F5 richiede un nuovo login; un 401 fa il logout.
- `UtilToken` usa `AddSeconds(ExpireMinutes)`: con `ExpireMinutes = 1200` il token vale 20 minuti, non 20 ore; il refresh a ogni risposta lo maschera [V].
- Validazione JWT in `Installers.InstallAuthentication` (issuer, audience, firma, scadenza, `ClockSkew = 0`); per SignalR il token arriva da `?access_token=` solo per il percorso `/chatHub`.
- Segreti: `appsettings.json` non contiene valori sensibili (stringa di connessione, chiave JWT, password, chiavi AI/PayPal sono vuote); si impostano con variabili `NVXAPP_*` e `Installers.CheckRequiredSecrets` ferma l'avvio se mancano la connection string o una chiave JWT di almeno 32 caratteri.
- Password: `AddIdentityCore` con lunghezza minima **3** e nessun altro requisito; alla creazione di utenti/centri/studi/aziende senza password indicata si usa il valore predefinito scritto nel codice (sette `…Put` di `AccountService`). Non c'è blocco dopo tentativi falliti.
- Utenti iniziali: creati dalla migration `InitPublic` (`PopolateDB`) con password note nel codice: da cambiare in ogni installazione reale [V, valori non riportati].

### 4.2 Ruoli [V: `ApplicationRole.cs`, `TicketRules`]

`RoleCode` (enum numerico, valori **non** da rinumerare): `User 0` · `CompanyAdmin 10` · `CompanyPowerAdmin 11` · `FinancialAdvisorAdmin 100` · `FinancialAdvisorPowerAdmin 101` · `DealerAdmin 1000` · `DealerPowerAdmin 1001` · `Admin 10000` · `PowerAdmin 10001` · `SuperUser 100000`.
- Gerarchia dei dati: `Dealer` (centro) → `FinancialAdvisor` (studio, `IdDealer`) → `Company` (azienda, `IdFinancialAdvisor`) → `UserCompany`; i collegamenti utente-nodo sono `UserDealer`, `UserFinancialAdvisor`, `UserCompany` (con `Disabled`). Ogni nodo ha un PowerAdmin; il PowerAdmin globale non si elimina mai.
- Non è un'ereditarietà: un SuperUser non usa le pagine azienda, **scende** con `UserLoad`.
- **Ruoli multipli**: a livello azienda un utente può essere `User` + `CompanyAdmin`/`CompanyPowerAdmin`. Il client usa sempre la lista (`AuthService.hasRole`, `RoleNames`, `IsInGroup…`). Sul server `PuoLavorareCome` usa il ruolo **più alto** (`LivelloRuolo`). Attenzione: `AccountService.UserLoad` e `IdentitaDi` ricavano centro/studio/azienda con uno `switch` su `roles[0]`; oggi i ruoli azienda finiscono nello stesso ramo e non fa danno, ma **mai** dedurre da `roles[0]` ruolo o permessi (vedi `TicketService`, che legge tutti i ruoli).
- Ruoli e voci di menu: `CompanyMenuRestriction` (CompanyPowerAdmin nasconde voci per ruolo) è applicata **solo dal client** (`menuRestrictionGuard`, filtro del menu): le API non la controllano [V].

### 4.3 Permessi: dove sono davvero [V]
- **Controller**: ogni azione ha `[Authorize]` (o `[AllowAnonymous]`), nessuna policy né `Roles=`; non c'è fallback policy. Aperte per scelta: i tre endpoint di recupero password, i tre di registrazione, `LicensePublicPriceList`, il feed `.ics` (token nell'indirizzo), `Account/DownloadLink` (codice firmato). Aperte **per assenza** di attributo: `Account/Login` (voluto), `MyMokeLongJob/StartJob` e `ExportJob`, `WeatherForecast/Get` (demo, §12.5).
- **Servizi**: i controlli di ruolo sono scritti nei servizi che li richiedono: `ApplicationActivationAuthorizer`, `DeletionService`, `CompanyMoveService`, `TicketService`/`TicketRules`, `LicenseService`, `PaymentReportService`, `StorageService`, `McpToolService` (livelli). Il ruolo e il nodo sono quelli del **token** (utente corrente), mai quelli indicati dal client; per "chi ha fatto il login" si usa `UserIdFirstConnection`.
- **Client**: venti e più `RoleGuard4…` in `pages/RoleGuard/infrastructure` + `authenticatedGuard`, `applicationGuard`, `financialAdvisorApplicationGuard`, `licenseLevelGuard`, `menuRestrictionGuard`, `showcaseGuard`. Sono UX, non sicurezza.
- **Isolamento tra aziende**: in multi-azienda lo schema deriva dal claim `company` (firmato), quindi i dati degli applicativi di un'azienda non sono raggiungibili da un'altra; le tabelle di `public` (ticket, utenti, listini…) si proteggono con i controlli dei servizi. In modalità singola il filtro per `Az_Anagrafica` è l'unica barriera.

### 4.4 "Lavora come" (impersonificazione) [V]
- Regola server (`AccountService.PuoLavorareCome`, usata da `UserLoad` e dagli strumenti MCP): ammesso l'utente del login stesso; altrimenti un utente con ruolo **più basso** (pari no) e nel **ramo** di chi ha fatto il login (centro → studi → aziende → utenti; collegamenti `Disabled` non contano; Admin/PowerAdmin/SuperUser: chiunque sotto di loro). Si valuta l'utente del **login** (`UserIdFirstConnection`), non l'impersonato: valgono le catene e il "torna indietro". Il rifiuto è registrato (Warning) e risponde con messaggio generico.
- Client: `UserNavigationService` tiene la catena nel browser; `RoleGuard4UserImpersonate`; `breadcrumbs-impersonate`.
- Tracciamento: job, export e SignalR vanno al gruppo `UserGroup-<UserIdFirstConnection>` (`SignalRHub.UserGroup`), quindi all'utente del login anche mentre impersona; il download di un file di job è consentito solo a lui (`Account/Download`).
- MCP: `nvx_who_can_i_work_as`, `nvx_work_as`, `nvx_stop_working_as`; l'identità scelta resta sul token personale e scade da sola (`Mcp:ImpersonationHours`).
- Punto aperto: `Account/UserRoles` accetta un `IdAspNetUsers` arbitrario senza `PuoLavorareCome` [V, §12.5].

### 4.5 Stato rispetto a `piano-security.md`

Il piano è stato scritto prima di alcune modifiche (vedi anche §12.2). Confronto con il codice:

| Fase del piano | Stato nel codice | Nota |
|---|---|---|
| 0 Segreti fuori dal repository | **Fatta** [V] | `NVXAPP_*`, `CheckRequiredSecrets`; rotazione della chiave JWT compromessa: [?] |
| 1 Ruoli nel token (+ bug `AddSeconds`) | **Non fatta** [V] | nessun claim di ruolo; bug presente |
| 2 Blindare `UserLoad` | **Fatta con regola diversa** [V] | `PuoLavorareCome` ("solo chi sta sotto", `MCP-A7b`); `UserRoles` non coperto |
| 3 `[Authorize]` con ruoli/policy + fallback | **Non fatta** [V] | i controlli sono nei servizi (§4.3) |
| 4 Irrobustimento (password, lockout, scadenza) | **Non fatta** [V] | minimo 3 caratteri, nessun lockout, password predefinita |
| 5 Chiusura IDOR (`IdAz_Anagrafica` + filtro globale) | **Non fatta** [V] | in multi-azienda mitigata dagli schemi |
| 6 Test xUnit sulle regole | **Non fatta** [V] | nessun progetto di test server |

Il piano dà il perimetro "senza rifare il multi-tenant, `SharedSchema` statico, `MultiTenant: false`": oggi il multi-azienda a schemi è realizzato e `MultiTenant` vale `true` nei file di configurazione.

### 4.6 Altre osservazioni di sicurezza [V salvo dove indicato]
- `SignalRHub`: la mappa `/chathub` non richiede autenticazione (`[Authorize]` commentato): una connessione anonima entra nel gruppo `UserGroup-`. `SendMessage` valida il token a mano ed è codice di prova (legge `MyTable` di Moke).
- `AccountService.UserLoad` manda a **tutti** i client connessi `ReceiveMessage` con "<username> è entrato" (`Clients.All`): diffonde nomi utente.
- `Account/Download`: i file senza `.meta` (non registrati con `NVXSystem.SalvaFileJob/RegistraFileJob`) li scarica qualunque utente autenticato. `ExportFolder` sta nella cartella temporanea del sistema.
- Token MCP: salvato come hash (`McpTokenValue.Hash`), schema di autenticazione separato (`McpToken`), non vale per le API e viceversa; conferma in due passi con codice firmato (`McpConfirmation`) per le modifiche amministrative; limiti per token. Provato con Claude Desktop (doc MCP).
- Il token di un utente eliminato resta valido fino alla scadenza (doc CANCELLAZIONE, punto aperto).
- Chiavi DataProtection su database (`public."DataProtectionKeys"`), `ApplicationName` fisso.

---

## 5. Modello dati comune

### 5.1 Schema `public` (`PublicDbContext`, `PublicDbContext_Infrastructure.cs`)

| Argomento | Entità (cartella `Entities/Public`) | Scopo e legami |
|---|---|---|
| Identità | `ApplicationUser`, `ApplicationRole` (Identity), `DataProtectionKey` | utenti, ruoli (`Code` = `RoleCode`) |
| Gerarchia | `Dealer`, `FinancialAdvisor`, `Company`, `UserDealer`, `UserFinancialAdvisor`, `UserCompany` | centro → studio → azienda → utenti; `Disabled` sui collegamenti |
| Applicativi | `CompanyApplication`, `FinancialAdvisorApplication`, `CompanyMenuRestriction` | applicativi attivi per azienda/abilitati per studio; voci di menu nascoste per ruolo |
| Impostazioni | `AppSetting` | chiave/valore (`TenancyMode`, parametri licenze, pagamenti, soglie, ultime esecuzioni dei job) |
| Registrazione | `CompanyRegistrationRequest` | richiesta self-service in due tempi (azienda o studio) |
| Licenze | `Lic_Listino`, `Lic_Piano`, `Lic_Abbonamento`, `Lic_Pagamento` (+ `LicenseEnums`) | listini per ambito (`AmbitoLicenza`: valori = `ApplicationType`, `Cumulativo = 100`), piani Free/Standard/Pro, abbonamenti, incassi (restano dopo la cancellazione del soggetto) |
| Pagamenti ai centri | `Pag_Rendiconto`, `Pag_DealerConfig`, `Pag_InvioEmail` | rendiconti e riepiloghi per centro |
| Servizi comuni | `Scadenza`, `Notifica`, `NotificaPreferenza`, `PresaVisione`, `CalendarioFeed`, `Riepilogo` (`ServiziComuni.cs`, `Riepilogo.cs`) | scadenzario (chiave applicativo + fonte + riferimento), notifiche e preferenze, prese visione/firme, feed `.ics` per utente, riepilogo AI settimanale |
| Ticket | `TicketType`, `TicketTypeLevel`, `Ticket`, `TicketCronology`, `TicketAttach`, `TicketTypeAiSetting`, `TicketCronologyDocument` | assistenza a livelli SuperUser/Dev/Dealer/Studio/Azienda/Utente |
| Documentazione AI | `Document`, `DocumentTicketType` | documenti dei livelli letti dall'AI per rispondere ai ticket (non è il documentale) |
| Spazio file | `CompanyStorage`, `CompanyStorageHistory` | quota, occupato, prenotato, numero di file per azienda |
| MCP | `McpAccessToken`, `McpCompanySetting`, `McpCallLog` | token personali (hash), impostazione per azienda, registro chiamate |
| Cancellazione | `DeletionLog` | traccia delle eliminazioni fisiche |
| Dashboard | `UserDashboard` | widget scelti dall'utente |

Gli enum si salvano come interi e **non si rinumerano** (commenti di `ServiziComuni.cs`, `Riepilogo.cs`, `LicenseEnums.cs`). Tutte le entità derivano da `BaseEntity` (`Id`, `CreationDate`, `ModifiedDate`, `ChangeUser`; `IDataChangeStatEntity`), valorizzati da `Repository.CreateAsync/UpdateAsync`.

### 5.2 Schema Common (`CommonDbContext`, `Entities/Tenant/Common`)

| Entità | Scopo | Note |
|---|---|---|
| `Az_Anagrafica` | ancora dell'azienda nei dati tenant (1-1 con `Company`) | creata una volta da `ICommonCompanyUtility`, mai dai moduli; in singola distingue le aziende |
| `Az_Cliente`, `Az_ClienteContatto` | clienti/fornitori (`Tipo`, `Attivo`) e persone di riferimento | gestiti da Infrastructure (pagine `customer-*`); limite clienti della licenza CRM |
| `Az_Sedi`, `Az_SediReparto` | sedi e reparti ad albero | gestione nelle pagine delle Presenze; qui sola lettura (`CommonOrganizzazioneService`) |
| `Par_Competenza`, `Dip_Competenza` | competenze e loro assegnazione | HR, Compliance, Presenze |
| `Dip_Anagrafica` | dipendente (utente facoltativo, `DataAssunzione`/`DataCessazione`, `Attivo`) | creata da `InfrastructureUserInitializer`; resta alla cessazione/eliminazione utente |
| `Dip_RapportoAutovetture` | autovettura del dipendente | legata a `IdDip_Anagrafica` (non al rapporto di lavoro) |
| `Dip_GG_Richiesta` | richiesta da approvare di qualsiasi modulo (`Dati` JSON, cronologie approvatori) | scritture solo da `Dip_GG_RichiestaCommonRepository`; `IdDip_RapportoLavoro` è un numero senza FK |

Le entità Common sono `partial`: i rami aggiungono navigazioni (es. `Dip_Anagrafica.Presenze.cs`) che `CommonDbContext` ignora. Per ogni entità Common c'è un repository in `Repositories/Tenant/Common` (nome `<Entita>CommonRepository` quando esiste lo stesso nome nelle presenze).

---

## 6. Mappa funzionale

Pagine = cartelle in `nvxapp.client/src/nvx/pages/infrastructure` (rotte in `Utility/infrastructure/route-infrastructure.service.ts`); servizio client in `ClientServer-Service/Infrastructure/<Area>`; servizio server in `nvxapp.server.service/ClientServer-Service/infrastructure/<Area>`; controller in `nvxapp.server/Controllers/Infrastructure`.

| Area | Pagine client (rotta) | Servizio server (controller) | Entità | Documento |
|---|---|---|---|---|
| Accesso e account | `login-page`, `logout-page`, `forgot-password-page`, `reset-password-page`, `change-password-page`, `confirm-email-page`, `user-page` | `AccountService` (`Account`), `PasswordRecoveryService` | `ApplicationUser`, `ApplicationRole` | manuale `infrastructure.guide` |
| Registrazione self-service | `register-page`, showcase | `RegistrationService` (`Registration`): conferma via email 24 h, crea azienda/studio nel ramo `DEMO` | `CompanyRegistrationRequest` | – |
| Gerarchia: centri, studi, aziende, utenti | `dealer-*`, `financial-advisor-*`, `company-*` (+ `company-wizard`), `user-*`, `usercompany-*` (+ wizard), `user-dealer-*`, `user-financial-advisor-*`, pagine di ruolo `super-user`, `power-admin`, `admin`, `dealer-*-admin`, `financial-advisor-*-admin`, `company-*-admin` | `AccountService` (`Dealer/FinancialAdvisor/Company/UserCompany/User/UserDealer/UserFinancialAdvisor` × `List/Get/Put`), `ICompanyInitializerRegistry`, `IUserInitializerRegistry` | `Dealer`, `FinancialAdvisor`, `Company`, `User*` | `ANALISI_INFRASTRUCTURE_CANCELLAZIONE.md` (§1) |
| Lavora come | `user-impersonate-page`, `breadcrumbs-impersonate` | `AccountService.UserLoad`, `PuoLavorareCome` | – | §4.4, `…_MCP.md` §7.1 |
| Applicativi attivi | sezione applicativi di `company-edit-page` / `financial-advisor-edit-page` | `CompanyApplicationService`, `FinancialAdvisorApplicationService`, `ApplicationActivationAuthorizer` | `CompanyApplication`, `FinancialAdvisorApplication` | `…_LICENZE.md` §15.6, `…_NUOVI_MODULI.md` §7 |
| Menu utenti (voci nascoste per ruolo) | `company-menu-restriction-page` | `CompanyMenuRestrictionService` | `CompanyMenuRestriction` | solo client (§4.2) |
| Licenze | `license-settings-page`, `license-catalog-list/edit-page`, `license-management-page`, `license-subscribe-page` | `LicenseService`, `LicenseGuard`, `LicenseSubscriptionManager`, `LicenseCatalogResolver`, gateway `PayPalPaymentGateway`/debug, `LicenseMaintenanceService` (`License`) | `Lic_*` | `…_LICENZE.md` |
| Pagamenti ai centri | `payment-report-page`, `payment-dealer-page`, `payment-dealer-config-page`, `payment-settings-page` | `PaymentReportService`, `PaymentSettlementManager`, `PaymentReportPdf`, `PaymentMaintenanceService` (`PaymentReport`) | `Pag_*` | `…_PAGAMENTI_CENTRI.md` |
| Spazio file e archiviazione | `storage-settings-page`, `company-storage-list/edit-page` | `StorageService`, `StorageQuotaService`, `IFileStorage` (`FileSystemFileStorage`), `StorageMaintenanceService` (`Storage`) | `CompanyStorage(+History)` | `ANALISI_DOCUMENTMANAGEMENT_APPLICATIVO.md` (ramo DM) |
| Clienti e contatti | `customer-list-page`, `customer-edit-page` | `Az_ClienteService`, `Az_ClienteContattoService` (`Az_Cliente`, `Az_ClienteContatto`) | `Az_Cliente`, `Az_ClienteContatto` | `ANALISI_CRM_APPLICATIVO.md` §2 (ramo CRM) |
| Sedi, reparti, competenze (lettura) | nessuna: la gestione è nelle Presenze | `CommonOrganizzazioneService` (`CommonOrganizzazione`) | `Az_Sedi`, `Az_SediReparto`, `Par_Competenza`, `Dip_Competenza` | `…_NUOVI_MODULI.md` §6 (`INF-COM`) |
| Anagrafica dipendenti, autovetture | nessuna in Infrastructure | `ICommonDipendenteUtility` (`GetOrCreateDip_AnagraficaAsync`, `AgganciaDip_AnagraficaAsync`…), `InfrastructureUserInitializer`, `CommonUserCompanyListExtension` | `Dip_Anagrafica`, `Dip_RapportoAutovetture` | idem |
| Richieste da approvare | `request-list-page` (`/requests`, `/requestsadmin`) | `RequestService`, `RichiestaCommonService`, `RichiestaRegole`, `IRequestNotifier` (`Request`) | `Dip_GG_Richiesta` | `…_RICHIESTE.md` |
| Notifiche | `notification-list-page`, `notification-preference-page`, contatori nel menu | `NotificationSender`, `NotificationService`, `UserNotifier` (`Notification`) | `Notifica`, `NotificaPreferenza` | `…_NUOVI_MODULI.md` (`INF-SVC.4`) |
| Scadenzario | `deadline-list-page`, `deadline-edit-page`, widget | `DeadlineRegistry`, `DeadlineReminder`, `DeadlineService`, `DeadlineMaintenanceService` (`Deadline`) | `Scadenza` | `INF-SVC.1-2,5` |
| Calendario (feed .ics) | `calendar-feed-page` | `CalendarFeedService`, `IcsWriter`, `ICalendarEventProvider` (`CalendarFeed`) | `CalendarioFeed` | `INF-CAL` |
| Presa visione / firma | `acknowledgement-list-page` | `AcknowledgementService`, `AcknowledgementUserService`, `AcknowledgementNotifier` (`Acknowledgement`) | `PresaVisione` | `INF-SVC.3`, `ANALISI_DOCUMENTMANAGEMENT_FIRMA_DIGITALE.md` |
| Riepilogo AI settimanale | `digest-list-page`, `digest-detail-page` | `DigestGenerator`, `DigestWriter`, `DigestService`, `DigestMaintenanceService` (`Digest`) | `Riepilogo` | `INF-AI` |
| Ticket di assistenza | `ticket-list/new/detail-page`, `ticket-type-list/edit-page`, `ticket-type-ai-setting-page`, `ticket-action-dialog`, `ticket-attach` | `TicketService`, `TicketRules`, `TicketTypeService`, `TicketTypeAiSettingService`, `TicketAiResolverService`, `TicketNotifier` (`Ticket*`) | `Ticket*` | `nvxapp.server.service/ANALISI_INFRASTRUCTURE_TICKET.md` |
| Documenti AI dei livelli | `ai-document-list/edit-page` | `DocumentService` (`Document`) | `Document`, `DocumentTicketType` | `ANALISI_DOCUMENTMANAGEMENT_DOCUMENTAZIONE_AI.md` (ramo DM) |
| Chat AI e comandi vocali | `fab-menu` (chat), `speech.service`, `speech-output.service` | `ChatAIService`, `LlmClient`, `CommandRegistry`, `CommandSlotProcessor`, `SlotNormalizers`, `InMemoryChatSessionStore`, handler in `ChatAiHandler/infrastructure` (`ChatAI`) | – | `…_CHAT_AI.md`, `…_INVIO_VOCALE_MANUALE.md` |
| Accesso di AI esterne (MCP) | `mcp-token-list/edit-page`, `mcp-call-log-page`, `mcp-company-setting-page` (rotte `aiaccess*`) | `McpEndpoint` (server), `McpToolService`, `McpAccessService`, `McpTokenAuthenticator` (`McpAccess`) | `Mcp*` | `…_MCP.md` |
| Widget e dashboard | `dashboard-page`, `widget-config`, `widgets/*` | `UserDashboardService` (`UserDashboard`) | `UserDashboard` | `…_WIDGET.md` |
| Cancellazione fisica | `deletion-dialog` | `DeletionService`, `CommonDataDeleter`, `IApplicationDataDeleter` (`Deletion`) | `DeletionLog` | `…_CANCELLAZIONE.md` |
| Spostamento azienda | `company-move-page` | `CompanyMoveService` (`CompanyMove`) | – | – |
| Manuali e vetrina | `user-manual-page`, `showcase-*-page`, `showcase-footer` | `UserManualService`, `UserManualPdf`, `IUserManualProvider`, `VoiceCommandsUserManualProvider` (`UserManual`) | – | `…_SHOWCASE.md`, `Tools/ManualScreenshots/README.md` |
| Export, job e file | `background-working`, `LongJobNotifierService`, `JobNotifierService` | `ILongJobNotifier`, `IJobNotifier`, `ServiceBase.RunInBackground`, `NVXSystem` (`Helpers/Sistema.cs`), `JobFileCleanupService`, `Account/Download` | file in cartella temporanea + `.meta` | §8.1 |
| Email | – | `EmailService` (MailKit, SMTP) | – | `…_DOCKER.md` §8 |
| Parametri e ruoli | `parameter-loader` | `ParameterService` (`Parameter`) | – | – |
| Personalizzazioni | **nessuna** (solo analisi) | – | – | `…_PERSONALIZZAZIONI.md` |
| Demo (Moke) | – | `MyTableService`, `MyMokeLongJobService`, `WeatherForecastService` | `MyTable` | §12.5 |

Processi in background del progetto `nvxapp.server/Utility` (tutti `BackgroundService`, registrati in `Installers.InstallServices`) [V]: `JobFileCleanupService` (ogni ora, `Export:RetentionHours`), `StorageMaintenanceService` (riallineamento notturno spazio, usa `IStorageUsageProvider`), `LicenseMaintenanceService` (rinnovi, scadenze, avvisi), `PaymentMaintenanceService` (bozze mensili, riepiloghi), `DeadlineMaintenanceService` (promemoria), `DigestMaintenanceService` (riepilogo settimanale, `Digest:Weekly`/`Hour`). Più il listener demo RabbitMQ (§9.2).

---

## 7. Punti di estensione per i moduli

Principio [V]: Infrastructure **non conosce i moduli**. Un modulo si aggancia (a) implementando un'interfaccia in una classe pubblica del proprio ramo (lo scanner `Installers.InstallServices` la registra, nessuna riga da aggiungere), oppure (b) valorizzando uno **stub** già presente in Infrastructure (client e `Installers4<Modulo>`), oppure (c) chiamando un servizio comune. Dove un'estensione richiede invece di toccare un file di Infrastructure è segnalato come **trappola**.

### 7.1 Server: interfacce da implementare

Chi implementa = ramo `AllFunction`, letto in sola lettura (AT presenze, CRM, DM documentale, HR, CMP compliance, AST risorse aziendali).

| Punto | Contratto (file) | Registrazione e uso | Chi lo implementa |
|---|---|---|---|
| `ApplicationType` + `AddApplicationDbContext<T>(app)` | `Tenancy/ApplicationType.cs`, `DataLayerInstaller` | `Installers4<Modulo>.InstallServices` (chiamato da `Program.cs`); il contesto deriva da `ApplicationDbContextBase` e ha design-time factory | AT, CRM, DM, HR, CMP, AST (CRM aggiunge anche un `HostedService` per i promemoria) |
| `ICompanyInitializer` | `Initializer/CompanyInit/ICompanyInitializer.cs`: `Name`, `Priority`, `Application`, `InitializeAsync(Company)` | `CompanyInitializerRegistry`: esegue i comuni (`Application == null`) e quelli degli applicativi **attivi**, in ordine di `Priority`; errore = log e si prosegue; cache in memoria; all'attivazione `InitializeApplicationAsync`. Priorità note: Common 5, Infrastructure 10, AT 20, DM 30, CRM 40, HR 50, CMP 60, AST 70 | AT, CRM, DM, HR, CMP, AST |
| `IUserInitializer` | `Initializer/User/IUserInitializer.cs` (come sopra, con `UserCompany`) | alla creazione/aggancio di un utente a un'azienda | solo AT (oltre a Infrastructure) |
| `IApplicationDataDeleter` | `Deletion/IApplicationDataDeleter.cs`: `Application`, `CompanyCountsAsync`, `DeleteCompanyDataAsync` (solo modalità singola), `DeleteUserDataAsync` | `DeletionService`: anteprima e cancellazione; gira **prima** della transazione, ogni metodo deve essere ripetibile; Common per ultimo (`DeleterAzienda`) | CRM, DM, HR, CMP, AST. **Manca AT** [V, punto aperto del doc CANCELLAZIONE] |
| `ICommonEntityUsage` | `Common/ICommonEntityUsage.cs`: `Az_ClienteUsageAsync` | `Az_ClienteService` blocca l'eliminazione di un cliente usato con un messaggio chiaro; chiamato solo se l'azienda ha usato l'applicativo | AT, CRM, DM |
| `IUserCompanyListExtension` | `Account/Extension/IUserCompanyListExtension.cs` | completa `AccountService.UserCompanyList` (applicativo attivo, Common sempre) | nessun modulo (solo `CommonUserCompanyListExtension`) |
| `IRichiestaTipoHandler` | `Request/IRichiestaTipoHandler.cs`: `Application`, `Tipi`, `StrategiaAsync`, `CompletaAsync`, `ApproveAsync`, `RejectAsync`, `CancelAsync` | `RequestService` unisce i tipi dei gestori degli applicativi attivi; dopo ogni cambio il modulo avvisa con `IRequestNotifier`; permessi ricontrollati nelle azioni | AT (`AtRichiestaTipoHandler`), AST (`AstRichiestaTipoHandler`) |
| `ICalendarEventProvider` | `Calendar/ICalendarEventProvider.cs`: `EventsAsync(idCompany, idAspNetUsers, dal, al)` | `CalendarFeedService`; chiamato da richiesta **anonima** (token): niente `CurrentUserId`, schemi via `IApplicationDbContextFactory`; `Uid` stabile con prefisso del modulo | Infrastructure (scadenze), CRM, AST |
| `IDigestProvider` | `Digest/IDigestProvider.cs`: `Application`, `SectionAsync(idCompany, idAspNetUsers?, dal, al)` | `DigestGenerator`: solo **fatti**, il testo lo scrive l'AI (o i fatti così come sono); chiamato in background senza utente | Infrastructure (scadenze, prese visione, notifiche), AT. **Mancano CRM, DM, HR, CMP, AST** |
| `INotificationCategoryProvider` | `Notification/NotificationSender.cs`: `Categories` (`NotificationCategory`: `Code` con prefisso del modulo, `Application`, `DefaultApp/Email`, `TitoloText`) | `NotificationSender.SendAsync` usa le preferenze dell'utente; pagina preferenze mostra le categorie degli applicativi attivi | HR, CMP, AST (Infrastructure ha le sue); AT/CRM/DM no |
| `IAcknowledgementChannel` | `Acknowledgement/AcknowledgementService.cs`: `Code`, `Priority`, `UrlPerUtente`, `AvailableAsync`, `StartAsync` | `AcknowledgementService` sceglie il canale disponibile con priorità più alta; base = conferma nella pagina (priorità 0) | DM (firma, priorità 10) |
| `IStorageUsageProvider` | `Storage/IStorageUsageProvider.cs`: `UsageAsync` | `StorageMaintenanceService` riallinea lo spazio notturno | DM |
| `ILicenseCatalogSeed` | `License/Seed/ILicenseCatalogSeed.cs`: `Name`, `Listini()` (+ helper `LicenseCatalogSeedData`) | `LicenseCatalogSeeder` all'avvio, una sola volta (listini globali) | Infrastructure (Cumulativo), AT, CRM, DM, HR, CMP, AST |
| `ICommandHandler` / `BaseCommandHandler` | `ChatAI/ICommandHandler.cs`: `IntentDefinition` (nome, slot, `Application`, `Usable4Role`, `RequiresConfirmation`, `McpAccess`, `Category`, `Pages`, `Examples`…) + `ExecuteAsync(slots)` | `CommandRegistry`/`IntentCatalog` si costruiscono dalla DI; **lo stesso handler** diventa comando della chat/voce **e** strumento MCP (`McpToolService`), filtrato per ruolo e applicativo attivo | tutti i moduli |
| `IRouteProvider` | `ChatAI/IRouteProvider.cs`: `Routes` (nome → rotta), `RoleRoutes`, `PromptPages` | scoperto dallo scanner e registrato **Singleton**; alimenta il comando "vai a…" | tutti |
| `IOpenableEntity` | `ChatAI/IOpenableEntity.cs`: `Key`, `Words`, `Roles`, `EditRoute`, `Application`, `LoadAsync` | usato da `OpenEntityHandler` ("apri la causale…"); una classe per anagrafica, niente nuovo comando | AT, CRM, HR, AST |
| `IUserManualProvider` / `UserManualMarkdownProvider` | `UserManual/UserManual.cs`: `Code`, `Application`; testi in `nvxapp.server.service/Manuals/<codice>.<lingua>.md` (+ `.guide.`) | `UserManualService` compone il manuale dal **menu reale dell'utente**; job in background; sintassi `@@ /rotta` | tutti |
| `IValidator<T>` | `nvxapp.server.data/Interfaces/IValidateEntity.cs` | `Repository.UpsertAsync` lo cerca nella DI e, se c'è, blocca il salvataggio con `UserMessageException` | [?] chi lo usa nei moduli |
| `IRabbitMqListenerService` | `RabbitMQ/Listener` (classe astratta `RabbitMqListenerService`) | ogni implementazione diventa `HostedService`; oggi esiste solo il demo | nessuno (solo `RabbitMqListenerService_Demo`) |
| `IHostedService` di modulo | – | **non** nel service: va in `nvxapp.server/Utility` e registrato in `Installers4<Modulo>` | CRM (`CrmReminderService`) |
| Profili AutoMapper, repository, controller | convenzioni di cartella | automatici (scanner, `AddMaps`, `MapControllers`) | tutti |

**Servizi comuni da chiamare** (non da implementare): `IDeadlineRegistry` (`UpsertAsync/CloseAsync/RemoveAsync/RemoveAllAsync`: chiave azienda + applicativo + fonte con prefisso del modulo + riferimento; cambiare la data riapre la scadenza e riazzera i promemoria; usato da AST, CMP, CRM, HR), `INotificationSender`, `IAcknowledgementService`, `IFileStorage` + `IStorageQuotaService` (prenota → salva → conferma; solo DM), `ILicenseGuard`, `IRichiestaCommonService`, `ICommonCompanyUtility`, `ICommonDipendenteUtility`, `ICommonOrganizzazioneService`, `IDataChangeNotifier`, `ILongJobNotifier`, `IEmailService`, `ILlmClient`.

**Ordine d'uso dei dati comuni da un modulo** [V, `CONTRIBUTING_nvxapp_data.md`]: scritture solo con i repository di `Repositories/Tenant/Common`; letture dentro le query del modulo con `<Prefisso>_<Entita>ReadRepository` sul contesto del modulo; collegamenti a righe di un altro modulo solo come ID senza FK.

### 7.2 Client: stub per ramo e registri

Tutti gli stub sono `providedIn: 'root'` in `nvxapp.client/src/nvx/Utility/infrastructure` e vuoti in Infrastructure; il ramo del modulo li riempie.

| Punto | File | Cosa fa il modulo | Composizione (file di Infrastructure) |
|---|---|---|---|
| Rotte | `route-<modulo>.service.ts` (`getRoutes()`, `getStudioRoutes()`) | elenca le sue rotte con le sue guardie di ruolo | `RouteService`: aggiunge `applicationGuard(app)` (e `financialAdvisorApplicationGuard` per lo studio) e `menuRestrictionGuard` a tutte |
| Menu | `main-menu-<modulo>.service.ts` (`iMainMenuService`: `Pages4SuperUser/Admin/DealerAdmin/FinancialAdvisorAdmin/CompanyAdmin/User`, `Pages4Support`, `GetWorkingMode`) | voci per ruolo | `MainMenuService`: voci mostrate solo se l'applicativo è attivo (o abilitato per lo studio); le voci di Infrastructure con `anyApplication` compaiono se c'è almeno un applicativo; `VociNavigabili` va alla chat |
| Widget | `widget-<modulo>.service.ts` (`iWidgetProvider.Widgets`, `WidgetDefinition` con chiave `<app>.<nome>`) | catalogo e componenti standalone | `WidgetService`: filtra per applicativo attivo e ruoli; le chiavi salvate ma non più note restano salvate |
| Vetrina | `showcase-<modulo>.service.ts` | sostituisce voci, rotte e sezioni dei contenuti | `ShowcaseService`: ordine di precedenza AT, CRM, DM, HR, CMP, AST, AllFunction (l'ultimo vince); non dipende dagli applicativi attivi |
| Traduzioni | `i18n-<modulo>.ts` (`I18N_FOLDER_*`) → `src/assets/i18n/<cartella>/<lingua>.json` | chiavi con prefisso proprio | `i18n-folders.ts` unisce nell'ordine dell'elenco |
| Nome del sito | `site-name-<modulo>.ts` | – | `site-name.ts`: AllFunction → AT → CRM → DM → Infrastructure (HR/CMP/AST non hanno nome proprio) |
| Richieste | `request-ui-<modulo>.service.ts` (`iRequestUiProvider`: `Actions`, `NewRequests`) | azioni sulle righe e voci "Nuova richiesta" | `RequestUiService` e `APPLICATIVI_RICHIESTE` **elencano a mano** AT e AST (trappola §7.3) |
| Dialog | `DIALOG_REDEFINITION` (`dialog-redefinition.ts`) | sostituisce un dialog di Infrastructure | registrazione in `app.module`; vale solo se tutti gli applicativi indicati sono attivi; priorità 10 (singolo) / 100 (combinazioni) |
| Parametri iniziali | `shared/shared-parameter-gestione-presenze.service.ts` (stub) | carica i parametri del modulo dopo il login | `parameter-loader` (Infrastructure) lo chiama se l'applicativo AT è attivo |
| Pipe e componenti del modulo | `shared/shared-component-gestione-presenze-module.module.ts`, `shared/components/<Modulo>` | – | – |
| Aggiornamento dati da comando esterno | `DataChangedNotifierService.of(app)` | la pagina si ricarica quando un comando (MCP) cambia i suoi dati | evento SignalR `DataChangedNotifier` |

Eventi SignalR [V]: `LongJobProgress`, `JobNotifierAsync`, `NotificationNew`, `UserNotifier`, `RequestNotifier`, `AcknowledgementNotifier`, `TicketNotifier`, `DataChangedNotifier`, `ReceiveMessage` (demo/broadcast).

### 7.3 Aggiungere un applicativo: cosa serve oltre agli stub

Procedura base in `ANALISI_INFRASTRUCTURE_NUOVI_MODULI.md` §7. Controllando il codice, i punti di Infrastructure da toccare **a mano** per un modulo nuovo (non coperti da interfaccia o stub) sono [V]:
- valore in `ApplicationType` (server) **e** in `user-load-model.ts` (client); `AmbitoLicenza` e liste di ambiti in `LicenseGuard`, `LicenseService`, `LicenseDefaults`; listino in un `ILicenseCatalogSeed`;
- costruttori di `MainMenuService`, `RouteService`, `WidgetService`, `ShowcaseService`, `i18n-folders.ts` (un nuovo import e un nuovo servizio per ramo);
- `McpToolNaming.Prefix` (prefisso degli strumenti MCP; il nome fa parte del contratto con i client AI) e `McpToolNaming.Sigla` (sigle `Crm`, `Doc`, `Hr`, `Cmp`, `Ast` già all'inizio dei nomi dei comandi del modulo; per AT non c'è sigla);
- `user-manual-page.component.ts` (`APPLICATIVO`: codice manuale → applicativo), `main-menu-infrastructure.service.ts` (`APPLICATIVI_MODULI`), `request-ui.service.ts` e `APPLICATIVI_RICHIESTE` se porta richieste;
- `CommonDbContext.IgnoreApplicationNavigations` non va toccato: ignora da solo le entità dei nuovi moduli (esclude tutte le entità del progetto dati che non sono comuni);
- `CLAUDE.md` (elenco rami e catena di AllFunction).
Questi file sono di Infrastructure: si modificano su `Infrastructure_Dev` e arrivano con la catena (rischio di conflitto di merge banale su `i18n-folders.ts`, `main-menu.service.ts`, `route.service.ts`).

---

## 8. Standard di sviluppo

### 8.1 Server
- **Accesso ai dati solo con i repository** (un repository per entità, interfaccia nello stesso file, cartelle `Repositories/Public`, `Tenant/Common`, `Tenant/<Applicativo>`); i servizi iniettano solo interfacce di repository, mai un `DbContext`. Operazioni particolari (SQL atomico, UPDATE condizionali, transazioni) sono metodi del repository (es. `ICompanyStorageRepository.ReserveAsync`, `IAppSettingRepository.GetValueAsync`); modifiche massive con `ExecuteUpdateAsync/ExecuteDeleteAsync` su `FindAll`. Fuori richiesta: `IApplicationDbContextFactory` + `new XRepository(db, serviceProvider, httpContextAccessor)`. Sottoquery in variabili locali. Regola completa: `CONTRIBUTING_nvxapp_data.md` "REGOLA BASE". Oggi i soli punti che creano un contesto dalla factory sono `LicenseGuard.ActiveClientsAsync` e `CommonDataDeleter` [V].
- **Servizio**: deriva da `ServiceBase`; ogni metodo ha firma `public virtual async Task<GenericResult<XOutModel>> Metodo(GenericRequest<XInModel> model, bool isSubProcess)`, corpo dentro `ExecuteAction(model, async () => {…}, isSubProcess)`, interfaccia nello stesso file (`: IServiceBase`), modelli nella sottocartella `Models` con nomi `…InModel/…OutModel` (mai `Dto`). Errori previsti: `throw new UserMessageException(InfrastructureMessages.Get("Chiave", args))`: arrivano al client com'è, già nella lingua; ogni altro errore diventa "errore imprevisto" con un codice di 8 caratteri e il dettaglio solo nel log. Un servizio che ne chiama un altro passa `isSubProcess = true` (nessun token, eccezione rilanciata).
- **Mapping**: solo profili AutoMapper (`Mappers/Public`, `Mappers/Tenant/…`), mai a mano nei servizi.
- **Controller**: `[ApiController] [Route("api/[controller]")]`, derivano da `NvxControllerBase`, azioni `[Authorize] [HttpPost] [Route("Nome")]` che restituiscono direttamente il `GenericResult` (anche le letture sono POST).
- **Export e file generati solo con i job** (`CLAUDE.md` del client): il servizio avvia `RunInBackground` (nuovo scope DI, token e `TenantScope` propagati: **mai** `Task.Run`), notifica l'avanzamento con `ILongJobNotifier.LongJobProgressAsync` (`LongJobCategory.FileGeneration`, `JobId`, `DownloadUrl`), salva il file con `NVXSystem.SalvaFileJob(jobId, owner, nomeVisibile, estensione, byte[])` (estensioni ammesse: csv, txt, tsv, json, xml, xlsx, pdf, dat, prn, zip; nome su disco `{jobId}.{ext}`; `.meta` con il proprietario). Esempio completo: `UserManualService.UserManualGenerate`. Il client mostra il lavoro nella barra (`background-working`) e scarica con `Account/Download/<file>`; i file si cancellano dopo `Export:RetentionHours` (`JobFileCleanupService`).
- **Lingua**: ogni testo per l'utente da `InfrastructureMessages`/`ChatAIMessages` (`resx` it/en in `nvxapp.server.data/Localization`; i moduli hanno le proprie); la lingua viene da `Accept-Language` (`UseRequestLocalization`), la cultura di date e numeri da `RunTime:Culture`.
- **Entità**: derivano da `BaseEntity`; nome della tabella al singolare; `DbSet` con `public virtual`; enum salvati come interi senza rinumerare; le entità comuni nuove si aggiungono a `CommonDbContext` e al repository `…CommonRepository`.
- **Migration**: una per contesto, mai a mano, mai con lo schema dentro; due aree che migrano lo stesso contesto non si mergiano insieme senza controllare lo snapshot (`NUOVI_MODULI` §10.0 punto 4).

### 8.2 Client
- Codice solo in `nvxapp.client/src/nvx` (non `src/app/nvx`): `ClientServer-Service/<Area>` (servizio + `Models`), `pages/infrastructure/<pagina>` (+ moduli), `shared/components/infrastructure`, `Utility/infrastructure`.
- **Pagine con un form** (modifica, registrazione, login, richieste, utility) derivano da `BasePageConfirmCancelComponent<T>`; i dialog con form da `BaseDialogConfirmCancelComponent<T>`. Si implementano `LoadData()`, `SaveData()`, `Title`, `EditForm`; i comportamenti diversi si ottengono con `btnConfirm/btnCancel`, `ButtonCancelClickEv`, `AfterSave(saved)`, `buttonbar` (tabella in `PATTERN_AND_CONVENTIONS.md`, "Pagine con form che non sono di modifica"). Esenti: pagine senza inserimento di dati (conferma email, logout, elenchi, calendari).
- **Elenchi**: componente `nvx-list` (colonne `NvxListColumn`, azioni di riga `NvxListAction`, celle `nvxCell`, riga mobile `nvxMobile`; tabella su desktop, `ion-item-sliding` su mobile), con `app-page-toolbar` e `app-page-buttonbar`. I documenti parlano di `app-generic-list`: nome superato (§12.1).
- **Pulsanti**: prima i predefiniti di `UserInterfaceService.Btn_*` (`Btn_Aggiungi`, `Btn_Modifica`, `Btn_Cancella`, `Btn_Approva`…); se manca se ne valuta uno nuovo nella collezione.
- **"Aggiungi"**: voce `+` nel FAB (`FabMenuService.fabMenuItem`, azzerata all'uscita dalla pagina); con più aggiunte un menu dal basso (action sheet) per scegliere il tipo; si prende spunto da `customer-list-page` e dalle richieste.
- **Lingua**: ogni etichetta con Transloco (`TranslocoPipe`/`TranslocoService`); file `src/assets/i18n/<cartella>/<lingua>.json` (it/en) con chiavi sotto il prefisso del ramo; `?v=<build>` nell'URL delle traduzioni.
- **Servizi generati**: un servizio per area con `HttpClient.post(environment.remoteData.apiUri + 'Controller/Metodo', GenericRequest<T>)` e modelli TypeScript speculari ai modelli server. Il pacchetto `schema-generator/nvx-generator` è uno **schematic vuoto** (restituisce l'albero invariato): la generazione di tabelle/pagine oggi segue `GENERAZIONE_TABELLE_GUIDA_COMPLETA.md` e i modelli di pagina `CONTRIBUTING_nvxapp_client_Template_*` [V].
- **Rotte e menu**: nuove voci in `route-<ramo>.service.ts` e `main-menu-<ramo>.service.ts` con la guardia di ruolo; per un modulo si usano i suoi stub (§7.2).
- **Chiamate in background lunghe**: l'interceptor mostra `background-working`; per le chiamate che gestiscono da sole l'errore `NVX_SENZA_DIALOG_ERRORI`.

---

## 9. Infrastruttura tecnica

### 9.1 Hangfire [V]
Pacchetti presenti e `InstallHangFire` pronto (schema `hangfire` della stessa connessione), ma `Hangfire:UseHangfire` è `false` e **nessun codice accoda job** (`BackgroundJob`/`RecurringJob` non compaiono). I lavori periodici sono `BackgroundService` (§6). Se attivato espone la dashboard `/hangfire` (filtro di autorizzazione predefinito di Hangfire, non configurato qui) e un `MapGet("/")` di prova.

### 9.2 RabbitMQ [V]
`InstallRabbitMq` registra come hosted service ogni `IRabbitMqListenerService` dell'assembly service: oggi solo `RabbitMqListenerService_Demo` (legge `IWeatherForecastService`), quindi **parte sempre** e tenta la connessione (`RabbitQm:Connection:*`; l'errore è loggato e non ferma l'avvio). Nessun'altra funzione pubblica o consuma messaggi (la pubblicazione in `ChatAIService` è commentata). Il `docker-compose.yml` però avvia RabbitMQ e fa dipendere l'app dal suo healthcheck.

### 9.3 SignalR [V]
Hub `/chathub` (`SignalRHub`, namespace `HubAI`), attivo con `SignalR:UseSignalR`. Ogni connessione entra nel gruppo `UserGroup-<useridfirstconnection>`. I servizi inviano con `IHubContext<SignalRHub>` (eventi in §7.2). Con più istanze serve un backplane (non presente; non necessario con un'istanza sola, `…_DOCKER.md` punto 11). Caddy inoltra i WebSocket.

### 9.4 Email [V]
`EmailService` (MailKit, SMTP): sezione `Email` (`Host`, `Port`, `Security` = `StartTls|SslOnConnect|None`, `User`, `FromAddress`, `FromName`; password in `NVXAPP_Email__Password`). Senza `Host` il servizio risulta non configurato (`Configured`) e `NotificationSender` non manda email. Link nelle email: `App:PublicUrl`, altrimenti l'origine della richiesta; `App:ApiPublicUrl` per il feed `.ics`.

### 9.5 Docker, Caddy, ambienti
- `Dockerfile` a tre stadi: build client (`node:22`, `ng build --configuration production --define NVX_BUILD_VERSION`), publish server (`sdk:11.0`, client in `wwwroot`, `-p:AngularBuilt=true`), runtime `aspnet:11.0` (utente non root, volumi `/app/Logs` e `/app/Files`).
- `docker-compose.yml` (app + `postgres:15` + `rabbitmq:3-management`, nessuna porta pubblicata) con override `docker-compose.local.yml` (PC), `docker-compose.prod.yml` (VPS, Caddy con HTTPS Let's Encrypt, limiti di memoria), `docker-compose.devdb.yml` (usa il PostgreSQL del PC). Quale usare lo dice `COMPOSE_FILE` nel file di ambiente (`DockerLocale.env`, `.env` sul server: non in git). Volumi: `nvxapp_logs`, `nvxapp_files`, `pg_data`, `rabbit_data`.
- Una sola istanza dell'app: lo stato in memoria (sessioni della chat `InMemoryChatSessionStore`, cache degli inizializzatori, `ExportFolder` nella cartella temporanea) non è condiviso (`…_DOCKER.md` punti 10-12).

### 9.6 Configurazione (solo nomi delle chiavi)
Origine: `appsettings.json` → `appsettings.<Ambiente>.json` → variabili d'ambiente; i segreti con prefisso `NVXAPP_` e `__` al posto di `:` (script `docs/imposta-variabili-nvxapp.ps1`). Obbligatorie: `ConnectionStrings:nvxappDbContext`, `JwtParameter:Key`.
Chiavi presenti: `Logging:LogLevel:*`, `Logging:LogType:LogOnFile|LogOnDB`; `JwtParameter:Key|Issuer|Audience|ExpireMinutes`; `DbParameter:MultiTenant`; `Hangfire:UseHangfire`; `SignalR:UseSignalR`; `Export:RetentionHours`; `FileStorage:Root` (solo in compose); `AI:UseLLM|MaxHistoryTurns|VisionLLM|ImportMaxChars|TimeoutSeconds|ExportLLM|ExportMaxChars|ExportMaxTokens|TicketLLM|TicketMaxChars|TicketMaxTokens|DigestLLM|DigestMaxTokens` e i provider `AI:AI_local|AI_openrouter|AI_groq|AI_deepseek` (`Url`, `model`, `method`, `VisionModel`, `ResponseFormat`, `ApiKey`); `RabbitQm:Connection:HostName|Port|UserName|Password`; `RunTime:Port|UseHttps|Culture|TimeZone`; `ReverseProxy:Enabled|KnownProxies|KnownNetworks`; `Cors:AllowedOrigins`; `Swagger:Enabled|RoutePrefix`; `Mcp:Enabled|MaxLevel|TokenMaxDays|MaxCallsPerMinute|MaxCallsPerDay|MaxRows|ConfirmMinutes|MaxWriteDays|ImpersonationHours|DownloadLinkMinutes`; `Email:*`; `App:PublicUrl|ApiPublicUrl`; `Payments:Debug` e `Payments:PayPal:Mode|ClientId|Secret|BrandName|SandboxBuyer:*`; `Digest:Weekly|Hour`.
Con `Payments:Debug = true` (impostato così ovunque, anche in Docker) i pagamenti sono simulati: da rimettere a `false` con i pagamenti reali.

### 9.7 Log e cultura
Serilog: console sempre; file `Logs/nvxapp.log` (10 MB, 200 file) con `LogOnFile`; tabella `log."Logs"` con `LogOnDB`; livello da `Logging:LogLevel:Serilog` (default Information). Il log passa da `Helpers/LogSanitizer`: `ServiceBase.ExecuteAction` scrive richiesta (Information) e risultato (Verbose) con i valori riservati mascherati (`***`), per nome di proprietà (parole intere, senza maiuscole: `Pw`, `NewPw`, `Password`, `Pin`, `Token`, `RefreshToken`, `KioskToken`, `ClientSecret`, `ApiKey`, `ConnectionString`, `Badge`, `Iban`...; non `Mapping`, `Pinned`, `Keywords`, `StorageKey`, `Key` da solo) o per l'attributo `[LogSensitive]` (usato su `CalendarFeedModel.Url`/`WebcalUrl`, che contengono il token del feed). Si mascherano testo e numeri (`{"pin":1234}`) e gli elementi degli elenchi sotto un nome riservato; i contatori numerici (`MaxTokens`, `TokenCount`, `PromptTokens`) e i bool restano visibili; con `Password`/`Pw`/`Pin`/`Secret`/`Otp` le parole neutre in coda (`PasswordType`) non smascherano. Stesso trattamento per il JSON dentro una stringa e per il testo libero (`NewPw=...`, `"pw":"due parole"`, `Bearer ...`). Stringhe lunghe tagliate prima delle regex (con la lunghezza originale) e `byte[]` abbreviati, output limitato a 32.000 caratteri, cache dei nomi con tetto (4.000 voci, nomi fino a 64 caratteri), mai eccezioni. Anche i messaggi di errore, i corpi di risposta PayPal e i testi della chat AI scritti nel log passano da `LogSanitizer.MaskText`. Limite: l'eccezione passata a `Log.Error(e, ...)` (e `{Exception}` nel template del file) è scritta da Serilog così com'è, senza mascheratura; `ExecuteAction` mette nel messaggio solo `ToErrorString` mascherato. I file `Logs/nvxapp.log` scritti PRIMA di questa modifica (e la tabella `log."Logs"`) possono contenere password: vanno trattati come riservati ed eventualmente cancellati a mano [V, §12.5]. Cultura e fuso: `Installers.InstallCultureAndTimeZone` (`RunTime:*`); su Windows il fuso resta quello della macchina.

### 9.8 Capacitor
**Non integrato** [V]: `capacitor.config.ts` è quello predefinito (`io.ionic.starter`), non esistono le cartelle `android`/`ios`, nel codice non si importa `@capacitor/*`. `environment.prod.ts` usa URL relative (`/api/`, `/`) e ricorda che con un'app nativa servirà un indirizzo assoluto.

---

## 10. Regole e casi limite per argomento

| Argomento | Regola | Dove |
|---|---|---|
| Dati | La modalità singola/multi si decide al primo avvio e non cambia; un valore di configurazione diverso ferma l'avvio | `DatabaseInitializer.ResolveTenancyModeAsync` |
| Dati | Un nome di tabella in due contesti ferma l'avvio (in singola stanno tutte in `public`) | `DatabaseInitializer.CheckTableNames` |
| Dati | Il nome dello schema è sempre validato (`^[a-z_][a-z0-9_]{0,62}$`) e quotato: mai concatenato in SQL | `TenantSchemaName.Validate/Quote` |
| Dati | Common esiste per ogni azienda, non si attiva, si migra per primo; `DropApplicationSchemaAsync` rifiuta Common | `TenantSchemaAccessor.GetSchema`, `TenantProvisioningService` |
| Dati | In multi-azienda un errore di migration di uno schema non blocca gli altri (log); in singola sì | `TenantProvisioningService.MigrateAllAsync` |
| Dati | Fuori richiesta si imposta l'azienda con `TenantScope.Use`; senza, i contesti rifiutano (`ApplicationNotActiveException`) | `TenantScope`, `ServiceBase.RunInBackground` |
| Dati | Le scritture di Common solo da repository comuni; il modulo legge con `ReadRepository` | `CONTRIBUTING_nvxapp_data.md` |
| Identità | Ruolo e nodo (centro/studio/azienda) si leggono dal token, mai dal client; "chi ha fatto il login" = `UserIdFirstConnection` | `ServiceBase.Current*`, `ApplicationActivationAuthorizer`, `DeletionService` |
| Identità | "Lavora come" solo verso ruoli più bassi e nel proprio ramo, valutato sull'utente del login | `AccountService.PuoLavorareCome` |
| Identità | Un utente può avere più ruoli: usare la lista (o il più alto), mai `roles[0]` | `AccountService.LivelloRuolo` (corretto), `UserLoad`/`IdentitaDi` (usano `roles[0]`) |
| Cancellazione | Eliminando un nodo si elimina tutto il ramo; prima l'anteprima, poi l'esecuzione digitando il nome | `DeletionService` (`DeletionPreview/Execute`) |
| Cancellazione | I PowerAdmin non si eliminano da soli (escono col nodo); il PowerAdmin globale mai; Dealer e utenti solo il SuperUser | `DeletionService` (`RuoliPowerAdmin`) |
| Cancellazione | I deleter girano prima della transazione e devono essere ripetibili; Common per ultimo; in multi-azienda si fa `DROP SCHEMA` | `DeletionService.DeleterAzienda`, `IApplicationDataDeleter` |
| Licenze | Con `Licenze_Attive` spento la copertura è "neutra" (nessun limite) | `LicenseGuard.EnabledAsync/CoverageAsync` |
| Licenze | Copertura: abbonamento proprio valido (anche Cumulativo), altrimenti dello studio; vince il livello più alto; scaduta ma entro i giorni di grace vale ancora | `LicenseGuard` (`LicenseState.Grace`) |
| Licenze | Limiti: utenti solo con la licenza Presenze, clienti con la CRM (contano attivi non solo-fornitori), spazio con la Documentale | `LicenseGuard.CanAddUserAsync/EnsureCanAddClientAsync/StorageQuotaAsync`, `ContaComeCliente` |
| Licenze | Con licenze accese un applicativo si attiva solo se l'azienda è coperta; disdetta = `DROP SCHEMA` e notifica | `CompanyApplicationService` (chiama `DropApplicationSchemaAsync`) |
| Licenze | Dopo la cancellazione di un soggetto i pagamenti restano per i resoconti | `Lic_Pagamento` |
| Spazio | Prenotazione atomica (`UPDATE … WHERE usato + prenotato + n <= quota`), poi conferma o rilascio; riallineamento notturno con lo spazio reale | `ICompanyStorageRepository.ReserveAsync/ConfirmAsync`, `StorageQuotaService` |
| Spazio | Chiave file `<IdAzienda>/<aaaa>/<mm>/<guid32>` validata con regex e controllo di percorso; scrittura su `.tmp` poi rinomina | `FileSystemFileStorage` |
| Richieste | Si usano solo i gestori dell'applicativo attivo; strategie `Singola/Tutti/Gerarchia` dalla cronologia degli approvatori; permessi ricontrollati nelle azioni | `RequestService`, `IRichiestaTipoHandler` |
| Richieste | Lo stato mostrato e i pulsanti seguono `RichiestaRegole` (ritira, chiedi annullamento, ritira annullamento) | `RichiestaRegole.PuoAnnullare`/`TestoStato` |
| Scadenze | Chiave azienda+applicativo+fonte+riferimento; nuova data = riapre e azzera i promemoria; testi tagliati a 200/500, preavviso 0-365 | `DeadlineRegistry.UpsertAsync` |
| Notifiche | La notifica si conserva sempre; popup e email seguono le preferenze della categoria; email solo se `EmailAllowed` e SMTP configurato | `NotificationSender.SendAsync` |
| Calendario | Il feed `.ics` è anonimo (token nell'indirizzo); i provider non hanno utente collegato | `CalendarFeedController.Ics`, `ICalendarEventProvider` |
| File dei job | Estensione solo da lista; download solo al proprietario (utente del login); cancellati dopo `Export:RetentionHours` | `NVXSystem`, `Account/Download`, `JobFileCleanupService` |
| Servizi | Errore previsto = `UserMessageException` (messaggio all'utente), altro = messaggio generico con codice | `ServiceBase.ExecuteAction` |
| Servizi | Token rigenerato a ogni risposta, non nei sotto-processi; `RunInBackground` cattura il token e `TenantScope` | `ServiceBase.OkResponse/RunInBackground` |
| Ticket | Aprono Dealer, Studio, Azienda, Utente e SuperUser (verso il basso); allegati max 5, 10 MB l'uno, 20 MB in tutto, estensioni in lista; oggetto 200, testo 4000 | `TicketRules`, `TicketService` |
| Registrazione | Due tempi: richiesta + conferma via link valido 24 h; crea nel ramo `DEMO`; non usa `EmailConfirmed` | `RegistrationService` |
| Spostamento azienda | Solo SuperUser; basta cambiare `Company.IdFinancialAdvisor` | `CompanyMoveService` |
| Chat e MCP | Un comando senza conferma è pubblicato come lettura; con conferma come modifica amministrativa; navigazione esclusa; `McpAccess` esplicito per i casi diversi | `IntentDefinition.EffectiveMcpAccess` |
| MCP | Modifiche amministrative in due passi (riepilogo + codice firmato, usabile una volta), periodo massimo `Mcp:MaxWriteDays`, limiti di chiamate per token | `McpToolService`, `McpConfirmation`, `McpOptions` |
| Lingua | Nessun testo per l'utente scritto nel codice: resx (server) e Transloco (client); i nomi dei comandi MCP sono contratto con i client AI | `InfrastructureMessages`, `McpToolNaming` |
| Enum | I valori di enum salvati come interi non si rinumerano (`ApplicationType`, `AmbitoLicenza`, `StatoScadenza`, `TipoRiepilogo`…) | commenti nei file di enum |

---

## 11. Guida agli impatti

### 11.1 "Se tocchi X, controlla Y"

| Se tocchi… | Controlla… |
|---|---|
| Un valore di `ApplicationType` o un nuovo applicativo | nome degli schemi (irreversibile); `user-load-model.ts`; `AmbitoLicenza` e liste in `LicenseGuard`/`LicenseService`; `McpToolNaming`; mappa dei manuali in `user-manual-page`; costruttori di menu/rotte/widget/vetrina/i18n (§7.3); `CLAUDE.md` |
| Una **entità o tabella Common** (colonna, tabella, repository) | migration `Common` e snapshot; `CheckTableNames` in tutti i contesti; i contesti dei moduli che la mappano con `ExcludeFromMigrations`; i `ReadRepository` dei moduli; `CommonDataDeleter`; `ICommonEntityUsage` dei moduli; modalità singola (filtro azienda); il merge sui rami AT (`AT-COM`) e sugli altri moduli |
| `Dip_GG_Richiesta`, `RichiestaRegole`, `RequestService` | `AtRichiestaTipoHandler`, `AstRichiestaTipoHandler`; pagine `/requests` e `/requestsadmin`; widget richieste; strumenti MCP di approvazione; `RequestNotifier` lato client; `APPLICATIVI_RICHIESTE` |
| `TenantSchemaAccessor`, `TenantSearchPathInterceptor`, `TenantScope` | tutti i repository; hosted service; deleter e provider (usano la factory); `RunInBackground`; `SignalRHub` |
| `ServiceBase` (`ExecuteAction`, `OkResponse`, proprietà `Current*`, `RunInBackground`) | **ogni servizio di ogni modulo**; il token rinnovato a ogni risposta; i job; il logging dei parametri |
| Claim del token (`UtilToken`, `TokenProperty`) | `ServiceBase`, `SignalRHub.GetTokenProperty`, `McpTokenAuthenticator`, interceptor del client, `UserNavigationService` |
| `RoleCode` / nomi dei ruoli | guardie del client, `TicketRules`, `switch` di `AccountService`, `DeletionService`, `Usable4Role` dei comandi, ruoli tra graffe nei manuali, `roles` dei widget, `RoleRoute` |
| Una **interfaccia di §7.1** (firma, nuovi metodi) | tutti gli implementatori nei rami moduli (tabella §7.1): un metodo nuovo non compila su AllFunction finché ognuno non lo implementa; costruire sempre `AllFunction_Dev` dopo il merge |
| `IntentDefinition` / `ICommandHandler` / `CommandSlotProcessor` / `SlotNormalizers` | tutti gli handler dei moduli; nomi degli strumenti MCP; manuale dei comandi vocali (generato dal catalogo); `tools/ChatAiPhraseTest` |
| `ILicenseGuard`, licenze | `AccountService` (limite utenti), `Az_ClienteService` (limite CRM), `StorageQuotaService`, `UserLoad` → `AuthService.License`, guardie e menu del client, `ILicenseCatalogSeed` dei rami |
| `MainMenuService`, `RouteService`, `WidgetService`, `ShowcaseService`, `i18n-folders.ts` | gli stub di **tutti** i rami (un nuovo parametro del costruttore deve arrivare con un import in ogni ramo); conflitti di merge su AllFunction |
| Chiavi di traduzione `infrastructure/it.json` / `en.json` | i rami che ridefiniscono la stessa chiave (vince il ramo del modulo); chiavi usate dalle vetrine |
| `DeletionService`, ordine dei deleter | `IApplicationDataDeleter` dei moduli (relazioni `Restrict`), `FileStorage.DeleteCompanyAsync`, `DropCompanySchemasAsync` |
| `NVXSystem` (estensioni, naming, `.meta`) | tutti gli export dei moduli, `JobFileCleanupService`, link di download MCP (`McpDownloadLink`), client `background-working` |
| `FileSystemFileStorage` (formato della chiave) | file già salvati dal documentale (la chiave è nel loro database), `StorageMaintenanceService`, `DeleteCompanyAsync` |
| Scanner dei servizi (`Installers.InstallServices`) | ogni classe pubblica con interfaccia diventa Scoped: collisioni se due classi implementano la stessa interfaccia "singola"; hosted service fuori dal service |
| `Repository<TDbContext,T>` | tutti i repository di tutti i contesti; `IValidator<T>`; `CurrentUserId` impostato per riflessione da `ServiceBase.SetUpRepo` |
| Chiavi di `appsettings` | variabili in `docker-compose*.yml` e nei file di ambiente, `docs/imposta-variabili-nvxapp.ps1`, `ANALISI_INFRASTRUCTURE_DOCKER.md` §8 |
| Modello di `UserLoad` | `AuthService`, menu, guardie, ogni pagina che usa `hasApplication`/`licenseLevel` |
| Un evento SignalR | servizio notifier del client che lo ascolta; i moduli che lo ripubblicano |
| Registrazione self-service | esistenza del ramo `DEMO` (`PopolateDB`), creazione azienda (`AccountService.CompanyPut`) e inizializzatori dei moduli |

### 11.2 Trappole note
- Un comando di scrittura con `RequiresConfirmation = false` e senza `McpAccess` esplicito viene pubblicato come **lettura** (nessuna conferma a un'AI esterna): impostare sempre `McpAccess`.
- `BackgroundService` dentro `nvxapp.server.service` diventa Scoped: va in `nvxapp.server/Utility`.
- `Task.Run` perde utente e azienda: usare `RunInBackground`; nei job che creano contesti serve comunque `TenantScope`.
- Non si inietta mai un `DbContext` in un servizio; fuori richiesta si usa la factory e `new XRepository(...)`.
- Le liste scritte a mano nel client (`APPLICATIVI_RICHIESTE`, `RequestUiService`, `APPLICATIVO` dei manuali, `APPLICATIVI_MODULI`) non si aggiornano da sole con un nuovo modulo.
- `UserLoad`/`IdentitaDi` ricavano il contesto da `roles[0]`: con un utente che avesse ruoli di nodi diversi il risultato dipenderebbe dall'ordine [S].
- In modalità singola ogni query dei dati comuni deve filtrare per `Az_Anagrafica`: il codice scritto pensando solo agli schemi lascia vedere i dati di tutte le aziende.
- Cache statiche degli inizializzatori e di `CommonCompanyUtility`: dopo il ripristino di un database serve il riavvio [S].
- Una sola istanza dell'app: sessioni della chat, cartella degli export e cache sono in memoria/locali.
- `Payments:Debug = true` ovunque: i pagamenti non sono reali.
- Le restrizioni di menu e le guardie del client **non** proteggono le API.

---

## 12. Incongruenze, codice sospetto, lacune

Il codice fa fede. Marcatori: **[V]** verificato, **[S]** sospetto, **[?]** da verificare.

### 12.1 Documenti, manuale e codice

| # | Cosa dice il documento | Cosa fa il codice | |
|---|---|---|---|
| 1 | `ANALISI_INFRASTRUCTURE_TICKET.md`: "Documento di sola analisi: nessun codice è stato scritto"; §4.1 "predisposizione" della risoluzione AI | Sistema completo (`TicketService`, 7 entità, pagine, notifiche) e `TicketAiResolverService` chiamato da `TicketService` | [V] |
| 2 | `ANALISI_INFRASTRUCTURE_LICENZE.md`, intestazione: "mancano i pagamenti reali" | `PayPalPaymentGateway` esiste (§7.4 del documento); manca la carta | [V] |
| 3 | `ANALISI_INFRASTRUCTURE_NUOVI_MODULI.md` §11 "Da fare": fasi 1-3 ⏳ | La tabella §10.1 le dà quasi tutte ✅ e AllFunction contiene i moduli | [V] |
| 4 | `ANALISI_INFRASTRUCTURE_RICHIESTE.md`: `RICH-APP.3`, `RICH-M`, `RICH-INF.5` ⏳/🔄 | `AllFunction` contiene già `AtRichiestaTipoHandler` e `AstRichiestaTipoHandler` | [S] punti probabilmente superati |
| 5 | `CONTRIBUTING.md`: "2 branch logici", solo Infrastructure/AttendanceTracking | Otto rami (`CLAUDE.md`) | [V] |
| 6 | `CONTRIBUTING_nvxapp_data.md`: "in multi tenant ogni tenant ha il suo database"; cartelle `Entities\Tenant\GestionePresenze` come unica regola | Un solo database con **schemi** per azienda; Common; un contesto per modulo | [V] |
| 7 | `CONTRIBUTING_infrastruttura.md`, `CONTRIBUTING_nvxapp_client*.md`, `ANALISI_CRM_APPLICATIVO.md`, `ANALISI_DOCUMENTMANAGEMENT_*`: componente `app-generic-list` | Non esiste: il componente è `nvx-list` (`shared/components/infrastructure/nvx-list`), usato in 20 e più pagine | [V] |
| 8 | `GENERAZIONE_TABELLE_GUIDA_COMPLETA.md`: ".NET 9", ramo `AttendanceTracking_Dev`, tabelle `MyTableAI_*`; `QUICK_REFERENCE.md`, `PATTERN_AND_CONVENTIONS.md`: stessi esempi; molte emoji salvate come `??` | `net11.0`; esempi non più presenti; i file `CONTRIBUTING_*` sono salvati in codifica non UTF-8 (caratteri `�`) | [V] |
| 9 | `PATTERN_AND_CONVENTIONS.md`: "sempre filtrare per `IdAz_Anagrafica`" | Vale in modalità singola; in multi-azienda lo schema isola già (filtro ridondante, da non togliere) | [V] |
| 10 | `Note/Infrastuttura.txt`: "net core 10", "postgres 15"; `Note/Migrazioni.txt`: elenco applicativi fino a DM, `dotnet-ef 10` | `net11.0` con pacchetti EF Core 10.0.12; applicativi fino a 7; compose usa `postgres:15` mentre lo sviluppo risulta su PostgreSQL 14 [?] | [V]/[?] |
| 11 | `CommonDbContext` (commento) e `Note/Migrazioni.txt`: elencano nove tabelle Common | `CommonDbContext` ne mappa dieci (manca `Dip_GG_Richiesta` negli elenchi) | [V] |
| 12 | `PublicDbContext` (commento): "ogni modulo aggiunge le proprie tabelle condivise con `PublicDbContext_<Modulo>.cs`" | Nessun modulo lo fa (su AllFunction c'è solo `PublicDbContext_Infrastructure.cs`) | [V] |
| 13 | Manuale `infrastructure.it/en.md` e `UserManual.cs`: "(presenze, CRM, documentale)" | Sei applicativi; `user-manual-page` li conosce tutti | [V] |
| 14 | `ANALISI_CRM_APPLICATIVO.md` §2.3 (ramo CRM): sedi e reparti "restano nelle presenze (deciso)" | Sono in Common (`INF-COM.1`, `AT-COM`) | [V] decisione superata |
| 15 | `UserManual.cs`: schermate in `Manuals/img/` incorporate | La cartella non c'è nel worktree (`Manuals/` ha solo i `.md`) | [?] esclusa dal repository o da rigenerare |
| 16 | `piano-security.md` | vedi §12.2 | [V] |

### 12.2 `piano-security.md` contro il codice
Datazione e numeri non più allineati: "135 endpoint", "48 controller", riferimenti a righe di `AccountService` e `Installers`. Oggi: 111 azioni HTTP nei controller del ramo Infrastructure [V]. `UserLoad` non è più aperto (§4.4) ma il piano lo descrive come tale; il multi-azienda a schemi, che il piano dichiara "dormiente", è il modo di funzionamento predefinito. Le fasi 1, 3, 4, 5, 6 restano da fare (§4.5). Il piano non nomina `UserRoles` come ancora aperta oggi, né i punti §12.5.

### 12.3 Codice non usato o sospetto
- **Demo Moke**: `MokeDbContext`, `MyTable`, `MyTableService`, `MyMokeLongJobService` (+ controller), `WeatherForecastService` (+ controller), `RabbitMqListenerService_Demo`, `SignalRHub.SendMessage`. Non hanno pagine nel client ma i servizi client esistono; i due controller sono anonimi [V]. `ApplicationType.Moke` è attivabile e ha uno schema per azienda [V]; se la UI lo proponga all'attivazione: [?].
- **Hangfire**: pacchetti, `InstallHangFire` e dashboard senza alcun job (§9.1). **RabbitMQ**: solo il listener demo, ma avvio e compose lo richiedono (§9.2) [V].
- **Capacitor**: `capacitor.config.ts` predefinito, nessun `android/ios`, pacchetti `@capacitor/*` nel `package.json` non importati nel codice (§9.8) [V]. **`schema-generator/nvx-generator`**: schematic vuoto (§8.2) [V].
- **`NvxControllerBase`**: codice commentato, filtri vuoti. **`ServiceBase.Initialize` e `TransactionRollback`**: vuoti [V]. `ICurrentUser` e la riflessione in `ServiceBase.SetUpRepo`: meccanismo implicito (il repository riceve `CurrentUserId` per riflessione sui campi del servizio) [V].
- **Claim `tenant`**: sempre vuoto dal login (§4.1) [V]. **`IUserCompanyListExtension`**: nessun modulo la implementa [V].
- **`IValidator<T>`**: nessuna implementazione in Infrastructure; non è noto se i moduli ne abbiano [?]. `CanBeEliminated` dell'interfaccia: [?] chi la chiama.
- **Messaggi di errore**: `CompanyInitializerRegistry` e `TenantProvisioningService` inghiottono gli errori dei singoli applicativi/schemi per scelta (log): conseguenza operativa in §3.5.
- **Inconsistenza di menu**: in `MainMenuService.Pages4FinancialAdvisorAdmin` le voci delle Presenze usano `forApplication` (attivo per l'azienda), quelle degli altri moduli `forFinancialAdvisorApplication` (abilitato per lo studio) [V]; probabilmente voluto (le Presenze non hanno pagine di studio), ma non è scritto [S].
- **`app.MapGet("/")`** con Hangfire attivo e `UseDefaultFiles`: ordine dei middleware da provare se si riattiva Hangfire [S].
- **EF Core 10.0.12** su `net11.0` e Swagger/OpenApi 11: combinazione di versioni da tenere d'occhio negli aggiornamenti [S].

### 12.4 Riferimenti impropri da Infrastructure ai moduli
Infrastructure dovrebbe non conoscere i moduli; oggi li conosce in questi punti [V]:
- **Licenze**: `LicenseGuard`, `LicenseService`, `LicenseDefaults`, `LicenseSettingsProvider`, `UserLicenseModel`, `auth.service.ts`: limite utenti = licenza Presenze, limite clienti = CRM, spazio = Documentale (progetto voluto dal documento licenze, ma è conoscenza dei moduli dentro le funzioni base).
- `AccountService`: il limite utenti dell'azienda è quello delle Presenze (`CanAddUserAsync`); `Az_ClienteService` applica il limite CRM; `StorageService` cerca `ApplicationType.DocumentManagement`.
- `McpToolNaming` (prefissi e sigle di Crm, Doc, Hr, Cmp, Ast, AT).
- Client: `parameter-loader` → `SharedParameterGestionePresenzeService` e `ApplicationType.AttendanceTracking`; `data-changed-notifier.service.ts` (esempio con AT); `main-menu-infrastructure.service.ts` (`APPLICATIVI_MODULI`), `request-ui.model.ts`/`request-ui.service.ts` (AT e AST); `user-manual-page` (mappa dei manuali); `site-name.ts`, `i18n-folders.ts`, `widget.service.ts`, `route.service.ts`, `main-menu.service.ts`, `showcase.service.ts` (un parametro per ramo).
- Commenti con esempi dei moduli in `ICompanyInitializer`, `IRouteProvider`, `ChatAIModel`.
- Server `SiteName.Value` considera solo `SiteNameAttendanceTracking` e Infrastructure, mentre il client considera anche CRM, DM e AllFunction [V]; come sul server si ottenga il nome di AllFunction: [?].

### 12.5 Sicurezza e robustezza: verificato e sospetto
Verificato [V]:
1. `MyMokeLongJob/StartJob`, `MyMokeLongJob/ExportJob`, `WeatherForecast/Get` senza `[Authorize]`: un anonimo può avviare job in background.
2. [corretto in `Infrastructure_Dev_log_password`, FB-SEC.4] `ServiceBase.ExecuteAction` scriveva la richiesta intera nel log a livello Information (default, con `LogOnFile = true`): password di login e campi `Pw` finivano in `Logs/nvxapp.log`. Ora usa `LogSanitizer` (regola e falsi positivi in §9.7). Altri punti verificati: nessun Serilog request logging, middleware o filtro che scriva corpi, nessun `EnableSensitiveDataLogging`, gli hub SignalR e `EmailService` non scrivono parametri; `PayPalPaymentGateway` (corpi di risposta) e `ChatAIService` (risposta LLM, slot, frase) ora mascherati. Controllati gli altri campi con indirizzi o codici segreti: link di reset/conferma email e `DownloadUrl` non sono mai scritti nel log o sono riferimenti sotto controllo del proprietario; `RedirectUrl`/`ReturnUrl` dei pagamenti contengono solo l'id d'ordine. Restano fuori: le eccezioni scritte da Serilog con `{Exception}` (messaggio e stack senza mascheratura) (es. dettaglio dei vincoli di unicità di Npgsql con i valori della chiave), `Log.Error(ex.Message, entity)` in `Repository` (il template è il messaggio), le chiavi nominate genericamente `Key`/`Chiave` e le coppie nome/valore (`AppSetting.Value`), il campo `Ticket` di `TimbraturaEvoluta_OpzioniOutModel`, i dati personali non segreti (codice fiscale, email) che compaiono nei log di recupero password e registrazione.
3. `Account/UserRoles` con `IdAspNetUsers` arbitrario senza controllo di perimetro.
4. `/chathub` senza autenticazione; `UserLoad` fa `Clients.All.SendAsync("ReceiveMessage", "<utente> è entrato")` a tutti i client.
5. Nessun lockout dei login (`lockoutOnFailure: false`), password minima di 3 caratteri, password predefinita nel codice per le creazioni senza password.
6. `UtilToken`: `AddSeconds(ExpireMinutes)` (20 minuti invece di 20 ore con la configurazione attuale).
7. `Account/Download` senza `.meta`: nessun controllo di proprietario; `ExportFolder` nella cartella temporanea.
8. Nessun `[Authorize(Roles/Policy)]`, nessuna fallback policy: un nuovo controller senza attributo è anonimo.
Sospetto [S]: token di un utente eliminato valido fino alla scadenza (documentato in `CANCELLAZIONE`); `FileSystemFileStorage.PathOf` confronta il percorso con `StartsWith(_root)` senza separatore finale (la chiave è comunque validata con regex); messaggi diversi per utente inesistente o password errata in `Login` [?].

### 12.6 Aree senza documentazione dedicata
Account, gerarchia e ruoli (solo riferimenti nei documenti MCP, CANCELLAZIONE e TICKET); registrazione self-service e ramo `DEMO`; spostamento azienda; menu utenti (restrizioni); parametri; notifiche, scadenzario, calendario, presa visione e riepilogo (descritti solo come aree `INF-SVC/INF-CAL/INF-AI` in `NUOVI_MODULI`); spazio file e archiviazione lato Infrastructure (il dettaglio è nel ramo DM); manuali e job dei PDF (solo commenti del codice e `README` degli script); e-mail; log e cultura; test automatici (non esistono per il server). Questo documento ne dà la mappa (§6) ma non il dettaglio.

### 12.7 Non verificato
Esecuzione di qualunque codice, build e test; stato del database di sviluppo e versione di PostgreSQL; effettiva rotazione della chiave JWT; esito dell'ultima catena dei merge; contenuto dei rami `Infrastructure` e `AllFunction` rispetto a `Infrastructure_Dev`; comportamento a runtime dei punti segnati [S]; moduli letti solo per i punti di estensione (non nelle loro regole); se la UI propone Moke all'attivazione; presenza delle schermate dei manuali nel repository.

---

## 13. Decisioni aperte

| # | Decisione | Note |
|---|---|---|
| FB-D1 | Riprendere `piano-security.md` (e con quali fasi) oppure riscriverlo sullo stato attuale | Fasi 1, 3, 4, 5, 6 non fatte; `UserLoad` già chiuso con altra regola; §4.5 |
| FB-D2 | Codice demo (Moke, `MyTable`, `WeatherForecast`, `MyMokeLongJob`, listener demo): togliere, proteggere con `[Authorize]` o tenere come modello di nuovo applicativo | I due controller anonimi sono il rischio più immediato |
| FB-D3 | Hangfire e RabbitMQ: tenerli per usi futuri o toglierli (pacchetti, compose, listener demo sempre attivo) | Oggi nessuna funzione li usa |
| FB-D4 | Scadenza vera del token (`ExpireMinutes`): correggere `AddSeconds` cambia 20 minuti in 20 ore con la configurazione attuale | Decidere il valore insieme alla correzione |
| FB-D5 | Registro unico degli applicativi (client e server) al posto delle liste scritte a mano (§7.3), oppure accettare di toccare quei file a ogni nuovo modulo | Riduce i conflitti di merge e i punti dimenticati |
| FB-D6 | Versioni: PostgreSQL (compose 15 / sviluppo [?]), .NET 11 con EF Core 10, `dotnet-ef` | Allineare e scrivere nei documenti |
| FB-D7 | Capacitor e `schema-generator`: pianificarli o eliminare config, pacchetti e cartella | Oggi non fanno nulla |
| FB-D8 | Cosa fare dei limiti per modulo dentro le licenze (conoscenza dei moduli in Infrastructure) | Possibile interfaccia "limite del modulo" implementata dai rami, simile a `IStorageUsageProvider` |
| FB-D9 | Decisioni già aperte altrove, **non ripetute**: `MCP-D7` (MCP incluso o a licenza), `ANA.10` (livelli di licenza dei nuovi moduli), `INF-CAL.4`/D9 (OAuth calendari), `ADM.1` (intermediario SdI), `PERS-D.1` (D1-D10 personalizzazioni), `ANA.5`/`ANA.6` (whistleblowing) | rimando ai rispettivi documenti |

---

## 14. Checklist

Legenda: ✅ svolto · ⏳ da fare · 🔄 in corso · 🔍 da decidere
Codici `FB-…` (Funzioni Base): non collidono con quelli degli altri documenti (`INF-*`, `AT-*`, `CRM-*`, `DM-*`, `HR-*`, `CMP-*`, `AST-*`, `RICH-*`, `PERS-*`, `MCP-*`, `ANA`, `ADM`).

#### `FB-DOC` – Documenti
- ✅ `FB-DOC.1` Documento d'insieme delle funzioni base (questo file), 25/11/2026
- ⏳ `FB-DOC.2` Aggiornare `CONTRIBUTING.md` (otto rami, cartelle) e `CONTRIBUTING_nvxapp_data.md` (schemi per azienda, Common, `Dip_GG_Richiesta`, nuovi contesti); salvarli in UTF-8
- ⏳ `FB-DOC.3` Aggiornare o sostituire `GENERAZIONE_TABELLE_GUIDA_COMPLETA.md`, `QUICK_REFERENCE.md`, `TROUBLESHOOTING.md`, `PATTERN_AND_CONVENTIONS.md` (.NET 11, esempi reali, emoji corrotte, regola del filtro per azienda)
- ⏳ `FB-DOC.4` Sostituire `app-generic-list` con `nvx-list` in `CONTRIBUTING_infrastruttura.md` e nei documenti dei rami (CRM, DM, ciascuno sul proprio ramo)
- ⏳ `FB-DOC.5` Riallineare le intestazioni: `ANALISI_INFRASTRUCTURE_TICKET.md`, `…_LICENZE.md`, `…_NUOVI_MODULI.md` §11, punti `RICH-*` della catena
- ⏳ `FB-DOC.6` Riscrivere lo stato di `piano-security.md` (cosa è già fatto, cosa no; §4.5)
- ⏳ `FB-DOC.7` Aggiornare `Note/Migrazioni.txt`, `Note/Infrastuttura.txt`, il commento di `CommonDbContext` e di `PublicDbContext`
- ⏳ `FB-DOC.8` Manuale `infrastructure.it/en.md` e `infrastructure.guide.*`: introduzione ai sei applicativi; rigenerare le schermate (`Tools/ManualScreenshots`)
- ⏳ `FB-DOC.9` Documenti di dettaglio mancanti (§12.6): account/gerarchia/ruoli, notifiche-scadenzario-calendario-presa visione-riepilogo (stato reale), manuali e job PDF
- ⏳ `FB-DOC.10` Mantenere questo documento: aggiornare §6, §7 e §12 quando cambiano estensioni o aree

#### `FB-SEC` – Sicurezza (collegata a `piano-security.md`)
- ⏳ `FB-SEC.1` Mettere `[Authorize]` ai controller demo (`MyMokeLongJob`, `WeatherForecast`) o rimuoverli (`FB-D2`)
- ⏳ `FB-SEC.2` `Account/UserRoles`: applicare la regola di perimetro (`PuoLavorareCome`) a `IdAspNetUsers`
- ⏳ `FB-SEC.3` `/chathub` con autenticazione; togliere il broadcast `Clients.All` di `UserLoad`
- ✅ `FB-SEC.4` Non scrivere password e campi `Pw` nel log di `ExecuteAction` (mascheratura o esclusione): `Helpers/LogSanitizer` + `[LogSensitive]`, usato anche per errori, PayPal e chat AI (ramo `Infrastructure_Dev_log_password`, da mergiare; limiti: eccezioni di Serilog non mascherate, i vecchi file di log possono contenere password)
- ⏳ `FB-SEC.5` Lockout dei login, politica password, password predefinita nelle creazioni
- ⏳ `FB-SEC.6` Correggere `UtilToken` (`AddSeconds`) con il valore deciso in `FB-D4`
- ⏳ `FB-SEC.7` `Account/Download`: richiedere il proprietario anche per i file senza `.meta`
- ⏳ `FB-SEC.8` Ruoli nel token, policy e fallback policy (Fasi 1 e 3 del piano)
- 🔍 `FB-SEC.9` Filtro per azienda/IDOR (Fase 5) e progetto di test sulle regole (Fase 6): serve `FB-D1`
- ⏳ `FB-SEC.10` Verificare la rotazione della chiave JWT e cambiare le password degli utenti iniziali in ogni installazione reale [?]

#### `FB-PUL` – Pulizia e coerenza
- 🔍 `FB-PUL.1` Codice demo (`FB-D2`) e listener demo RabbitMQ sempre attivo
- 🔍 `FB-PUL.2` Hangfire e RabbitMQ: usare o togliere (`FB-D3`)
- 🔍 `FB-PUL.3` Capacitor e `schema-generator` (`FB-D7`)
- ⏳ `FB-PUL.4` Spostare nel punto di estensione i riferimenti AT di `parameter-loader` (`SharedParameterGestionePresenzeService`) e i commenti con esempi dei moduli
- ⏳ `FB-PUL.5` Chiarire (o uniformare) `Pages4FinancialAdvisorAdmin` delle Presenze (`forApplication` contro `forFinancialAdvisorApplication`)
- ⏳ `FB-PUL.6` Decidere e documentare `IUserCompanyListExtension` (nessun modulo la usa) e `IValidator<T>` (chi la usa)

#### `FB-EST` – Estensioni
- 🔍 `FB-EST.1` Registro unico degli applicativi (`FB-D5`): `McpToolNaming`, mappa dei manuali, `APPLICATIVI_*`, ambiti delle licenze, costruttori di menu/rotte/widget/vetrina
- ⏳ `FB-EST.2` `IDigestProvider` mancante per CRM, DM, HR, CMP, AST (lavoro nei rispettivi rami, su richiesta)
- ⏳ `FB-EST.3` `IUserInitializer` e `INotificationCategoryProvider` dove un modulo ne avrebbe bisogno (AT, CRM, DM senza categorie proprie): verificare nei rami
- 🔍 `FB-EST.4` Limiti per modulo delle licenze come interfaccia (`FB-D8`)
- ⏳ `FB-EST.5` Aggiornare `NUOVI_MODULI` §7 con i punti scritti a mano di §7.3

#### `FB-CFG` – Configurazione e test
- 🔍 `FB-CFG.1` Allineare le versioni (`FB-D6`) e documentarle
- ⏳ `FB-CFG.2` Progetto di test del server per tenancy (`TenantSchemaAccessor`, provisioning), regole di autorizzazione, `ServiceBase` e `RichiestaRegole`
- ⏳ `FB-CFG.3` Provare la riattivazione di Hangfire (dashboard, ordine dei middleware) se si decide di tenerlo [S]

#### Punti ancora aperti negli altri documenti di Infrastructure (rimando, senza copiarli)
- `ANALISI_INFRASTRUCTURE_CANCELLAZIONE.md`: deleter delle Presenze, validità del token di un utente eliminato, cancellazione in background
- `…_CHAT_AI.md`: C10 (giustificativo su intervallo e operazioni massive), prova della voce nel browser
- `…_INVIO_VOCALE_MANUALE.md`: prova su Chrome desktop e Android
- `…_LICENZE.md`: eliminazione dei file su disco dei moduli prima del `DROP SCHEMA`, prova con le chiavi sandbox PayPal, webhook/rinnovi automatici/gateway carta
- `…_MCP.md`: `MCP-37`, `MCP-42`, `MCP-44`, `MCP-45`, `MCP-49`, `MCP-50`, `MCP-51`, `MCP-52` e la fase 5
- `…_PAGAMENTI_CENTRI.md`: schermate del manuale, trattenuta reale, `DataPagamento` con i pagamenti reali
- `…_RICHIESTE.md`: `RICH-INF.5`, `RICH-AT.7`, `RICH-APP.3`, `RICH-M`
- `…_SHOWCASE.md`: verifica visiva su ogni ramo
- `…_NUOVI_MODULI.md`: `ANA.5`, `ANA.6`, `ANA.10`, `INF-CAL.4`, `DM-PEC`, `DM-CONS`, `CMP-WB`, `ADM`
- `…_PERSONALIZZAZIONI.md`: tutta la checklist (`PERS-*`), compresa la conferma delle decisioni D1-D10
- `…_CONFRONTO_BIZNEO.md`: le 30 voci da verificare
- `…_DOCKER.md`: preparazione del VPS (sezione 9)
- `piano-security.md`: fasi 1, 3, 4, 5, 6
