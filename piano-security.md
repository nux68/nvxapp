# Piano — Messa in sicurezza di autenticazione e autorizzazione (nvxapp)

## Context

Dall'analisi della soluzione è emerso che **l'autorizzazione esiste solo lato client**. Il frontend Angular ha 20 guard di ruolo (`nvxapp.client/src/nvx/pages/RoleGuard/infrastructure/`) e un modello di ruoli articolato, ma il server non ha **un solo `[Authorize(Roles = ...)]`** in tutta la soluzione: tutti i 135 endpoint sono protetti dal solo `[Authorize]`, cioè da "sei autenticato in qualche modo".

Conseguenza concreta, verificata sul codice: un dipendente autenticato con ruolo `User` diventa amministratore con due chiamate HTTP.

```
POST /api/Account/UserList   → elenco id di tutti gli Admin/PowerAdmin   (AccountService.cs:1020)
POST /api/Account/UserLoad   {"Data":{"Id":"<id>"}} → JWT firmato per quell'utente  (AccountService.cs:181)
```

`UserLoad` accetta un id arbitrario e restituisce un token con ruoli, company e tenant del bersaglio, senza verificare che il chiamante sia autorizzato. Percorsi equivalenti esistono via `UserPut` (crea un utente con ruolo arbitrario, password default `"1234"`) e `CompanyPut`.

In parallelo, ogni entità è leggibile/scrivibile/cancellabile per id numerico senza verifica di proprietà (IDOR sistematico su ~45 entità), il che rende i dati di un'azienda accessibili agli utenti di un'altra.

**Esito atteso**: l'autorizzazione diventa una proprietà del server, verificabile e testata; il client resta com'è nella sostanza (le guard restano, ma come UX, non come sicurezza).

**Decisioni prese con l'utente**
- Perimetro: auth/authz **+ IDOR**. Escluso il rifacimento del multi-tenant a schema (`SharedSchema` statico), che resta dormiente perché `MultiTenant: false`.
- Nessun ambiente di produzione: si può rompere la retrocompatibilità, ruotare la chiave JWT e resettare le password senza finestre di transizione.
- IDOR: **denormalizzare `IdAz_Anagrafica`** su tutte le entità del modulo presenze, poi un unico global query filter EF.
- Test: **sì**, progetto xUnit sulle regole di autorizzazione.

---

## Due vincoli scoperti che determinano l'ordine dei lavori

**1. Il JWT non contiene claim di ruolo.** `UtilToken.GenerateJwtToken` (`nvxapp.server.service/Helpers/UtilToken.cs:30-38`) emette solo `sub`, `useridfirstconnection`, `tenant`, `dealer`, `financialadvisor`, `company`. Aggiungere `[Authorize(Roles=...)]` oggi **respingerebbe tutti gli utenti, inclusi i legittimi**. La Fase 1 è quindi abilitante e non rinviabile.

**2. La catena di impersonificazione vive solo in memoria del browser.** `UserNavigationService._userCronology` (`nvxapp.client/src/nvx/Utility/infrastructure/user-navigation.service.ts`) mantiene lo stack; `UserGoBack`/`UserGoTo` richiamano `UserLoad` con l'id precedente. Il server non ha memoria della catena, quindi non può distinguere una risalita legittima da una scalata. Serve una regola server-side che non dipenda dallo stack del client.

**Nota positiva**: la specifica di autorizzazione **non va inventata**. È già scritta nelle guard e nelle route Angular, con un modello coerente *"il livello N gestisce il livello N-1"*. Il lavoro è portarla sul server, non progettarla.

---

## Modello di autorizzazione target

`RoleCode` (`nvxapp.server.data/Entities/Public/ApplicationRole.cs:17`) è già un enum numerico gerarchico: `User=0`, `CompanyAdmin=10`, `CompanyPowerAdmin=11`, `FinancialAdvisor*=100/101`, `Dealer*=1000/1001`, `Admin=10000/10001`, `SuperUser=100000`.

Attenzione: **non è una gerarchia per ereditarietà**. Un `SuperUser` non accede alle pagine company: deve *scendere* via `UserLoad`. Questo è il design, e va preservato — `UserLoad` è il meccanismo legittimo di cambio scope, non solo un "impersona".

### Matrice endpoint → ruoli (derivata dalle guard esistenti)

| Gruppo di endpoint | Guard client di riferimento | Ruoli ammessi lato server |
|---|---|---|
| `Account/Dealer{List,Get,Put}`, `Account/User{List,Get,Put}` | `RoleGuard4DealerList/Edit`, `RoleGuard4Admin` | `SuperUser`, `Admin`, `PowerAdmin` |
| `Account/FinancialAdvisor{List,Get,Put}`, `Account/UserDealer*` | `RoleGuard4FinancialAdvisorList/Edit`, `RoleGuard4DealerAdmin` | `DealerAdmin`, `DealerPowerAdmin` (+ `SuperUser` su *Edit*) |
| `Account/Company{List,Get,Put}`, `Account/UserFinancialAdvisor*` | `RoleGuard4CompanyList/Edit`, `RoleGuard4FinancialAdvisorAdmin` | `FinancialAdvisorAdmin`, `FinancialAdvisorPowerAdmin` |
| `Account/UserCompany{List,Get,Put}` | `RoleGuard4UserCompanyList` | `CompanyAdmin`, `CompanyPowerAdmin` |
| Configurazione presenze: `Par_*`, `Az_*`, `TimeSheet_Export`, `ActivityStatistics`, `PresentStaff`, `VacationPlan` (~30 controller) | `RoleGuard4CompanyPowerAdmin` | `CompanyPowerAdmin` |
| `Az_SediRepartoUser`, timesheet admin, `Dip_GG_Richiesta` (approvazioni) | `RoleGuard4CompanyAdmin` | `CompanyAdmin`, `CompanyPowerAdmin` |
| Self-service dipendente: timbrature, richieste proprie, timesheet proprio | `RoleGuard4User`, `RoleGuard4JustClockRequest` | `User` (+ gruppo CompanyAdmin per le richieste) |
| `Account/Login` | — | anonimo |
| `Account/UserRoles`, `Account/UserLoad` | — | autenticato + **regola dedicata** (Fase 2) |

La mappatura completa è ricavabile meccanicamente da `route-infrastructure.service.ts` e `route-attendance-tracking.service.ts`: ogni `path` ha già il suo `canActivate`.

### Regola di autorizzazione per `UserLoad`

Il server è stateless e non deve fidarsi dello stack del client. Regola:

> Una `UserLoad(target)` è consentita se e solo se **l'utente radice** — quello identificato dal claim `useridfirstconnection`, già presente nel token e conservato lungo tutte le impersonificazioni — è autorizzato a gestire `target`.

"Autorizzato a gestire" = ruolo della radice compatibile con il livello del bersaglio **e** bersaglio contenuto nel perimetro organizzativo della radice, secondo la catena già modellata nel DB:

```
Dealer 1—N FinancialAdvisor 1—N Company 1—N UserCompany
   ↑ UserDealer      ↑ UserFinancialAdvisor      ↑ IdAspNetUsers
```

Casi coperti:
- **Login** (`login-page.component.ts:95`, target = sé stesso) → consentito, `target == radice`.
- **Discesa** (`company-list-page.component.ts:94`, drill-down su un'azienda) → consentito se l'azienda appartiene al perimetro della radice.
- **Risalita** (`UserGoBack`, target intermedio) → consentito, perché resta nel perimetro della radice. Questa è la ragione per cui la regola si àncora alla radice e non al ruolo corrente: valutata sul ruolo corrente, la risalita fallirebbe.
- **Scalata** (`User` → `PowerAdmin`) → **negata**: la radice è un `User`, che non gestisce nessuno.

`SuperUser` come radice ha perimetro totale — comportamento invariato rispetto a oggi.

---

## Fasi

### Fase 0 — Prerequisiti (rapida, indipendente)

- Spostare i segreti fuori dal repository: password PostgreSQL (`appsettings.json:20`), **chiave JWT** (`:24`), credenziali RabbitMQ (`:87`) → User Secrets in dev, variabili d'ambiente in deploy. Lasciare in `appsettings.json` solo chiavi non sensibili.
- **Generare una nuova chiave JWT**: quella attuale è nella history di git e va considerata compromessa. Va sostituita, non solo spostata.
- Rimuovere i due progetti vuoti `nvxapp.server.Controllers` e `nvxapp.schema` (non sono in solution, sono solo rumore).

### Fase 1 — Ruoli nel token *(abilitante: nulla del resto funziona senza)*

**File**: `nvxapp.server.service/Helpers/UtilToken.cs`, `nvxapp.server/Utility/Installers.cs`, `nvxapp.server.service/Base/ServiceBase.cs`, `AccountService.cs`

1. Estendere `TokenProperty` con `List<string> Roles` e `int MaxRoleCode`.
2. In `GenerateJwtToken`, emettere un claim `ClaimTypes.Role` per ruolo, più un claim `rolecode` con il codice numerico massimo (utile alla regola di Fase 2 senza query aggiuntive).
3. In `InstallAuthentication` (`Installers.cs:336`), impostare esplicitamente `RoleClaimType = ClaimTypes.Role` e `NameClaimType = ClaimTypes.NameIdentifier` in `TokenValidationParameters`, per non dipendere dal mapping implicito di `JwtSecurityTokenHandler`.
4. Popolare i ruoli nei due punti che generano token: `AccountService.Login` (`:111`) e `AccountService.UserLoad` (`:251`) — quest'ultimo già chiama `_userManager.GetRolesAsync`, il valore è a disposizione.
5. `ServiceBase.OkResponse` (`ServiceBase.cs:132-169`) rigenera il token **a ogni risposta**. Va esteso per riportare i ruoli, ma **leggendoli dai claim del token corrente**, non ri-interrogando il DB: altrimenti si aggiunge una query per ogni richiesta HTTP dell'applicazione. I ruoli cambiano solo in `Login` e `UserLoad`.
6. `RunInBackground` (`ServiceBase.cs:347`) cattura il `TokenProperty`: va incluso `Roles`, o i job in background perdono il contesto di ruolo.

**Bug collaterale da correggere qui**: `UtilToken.cs:44` usa `DateTime.UtcNow.AddSeconds(ExpireMinutes)` su un parametro chiamato *Minutes*. Con `ExpireMinutes: 1200` la scadenza reale è 20 minuti, non 20 ore. Oggi è mascherato dal refresh a ogni risposta; va allineato al nome (`AddMinutes`) e il valore in configurazione rivisto di conseguenza.

> Punto di controllo: a fine fase l'app deve funzionare **esattamente come prima**. Nessun `[Authorize(Roles)]` è ancora attivo; si è solo arricchito il token. Se qualcosa si rompe qui, si rompe prima di introdurre restrizioni — ed è molto più facile da diagnosticare.

### Fase 2 — Blindare `UserLoad` *(chiude l'escalation)*

**File**: nuovo `nvxapp.server.service/ClientServer-Service/infrastructure/Account/UserScopeAuthorizer.cs`; modifica a `AccountService.UserLoad`

- Nuovo servizio `IUserScopeAuthorizer` con `Task<bool> CanImpersonate(string rootUserId, string targetUserId)`, che implementa la regola descritta sopra risolvendo il perimetro tramite i repository già esistenti: `IUserDealerRepository`, `IUserFinancialAdvisorRepository`, `IUserCompanyRepository`, `ICompanyRepository`, `IFinancialAdvisorRepository`.
- In `UserLoad`, prima di generare il token: se `CanImpersonate(UserIdFirstConnection, model.Data.Id)` è falso → nessun token, messaggio d'errore generico, **log a livello Warning con id chiamante e id bersaglio** (un tentativo fallito qui è un segnale di attacco, non un errore ordinario).
- Applicare la stessa verifica a `UserRoles` (`AccountService.cs:144`), che accetta anch'esso un `IdAspNetUsers` arbitrario ed espone i ruoli altrui.

> Questa fase da sola chiude la vulnerabilità più grave. Se serve un rilascio d'emergenza, Fase 1 + Fase 2 sono il taglio minimo sensato.

### Fase 3 — `[Authorize]` con ruoli su tutti gli endpoint

**File**: nuovo `nvxapp.server/Utility/NvxPolicies.cs`; tutti i 48 controller in `nvxapp.server/Controllers/`

- Definire le policy **in un unico punto**, non disseminando stringhe di ruolo nei controller:

  ```csharp
  public static class NvxPolicy
  {
      public const string DealerManage   = "Dealer.Manage";
      public const string CompanyConfig  = "Company.Config";
      // ...
  }
  ```
  registrate in `Installers` con `AddAuthorizationBuilder().AddPolicy(..., p => p.RequireRole(...))`.
- Applicare `[Authorize(Policy = ...)]` **a livello di controller** dove tutti i metodi condividono lo stesso requisito (la larga maggioranza), e a livello di action solo nelle eccezioni.
- Impostare una **fallback policy** che richieda l'autenticazione, così un controller nuovo è protetto per default anche se ci si dimentica l'attributo. `Account/Login` va marcato `[AllowAnonymous]`.
- Per il modulo presenze, applicare la matrice: `CompanyPowerAdmin` sui ~30 controller di configurazione, `CompanyAdmin`+`CompanyPowerAdmin` su reparti-utente e approvazioni richieste, `User` sul self-service.

Attenzione al caso `Dip_GG_Richiesta`: lo stesso controller serve sia il dipendente (crea la propria richiesta) sia l'amministratore (approva). Vanno separati i requisiti per action, e l'action del dipendente deve comunque verificare che la richiesta sia **sua** — la Fase 5 non copre questo caso, che è scoping per utente, non per azienda.

### Fase 4 — Irrobustimento dell'autenticazione

**File**: `nvxapp.server/Utility/Installers.cs`, `AccountService.cs`, `Program.cs`

- Password policy (`Installers.cs:158-163`): `RequiredLength` da 3 a 10, riattivare i requisiti di complessità.
- Lockout: `AccountService.cs:101` usa `CheckPasswordSignInAsync(user, pwd, lockoutOnFailure: false)` → portare a `true` e configurare `options.Lockout` (5 tentativi, 15 minuti).
- Rate limiting sull'endpoint `Login` (middleware `RateLimiter` nativo di .NET 9).
- Eliminare le password di default `"1234"` in `UserPut` (`:1086`) e `CompanyPut` (`:746`): generare una password casuale e forzare il cambio al primo accesso.
- Unificare i messaggi di login: oggi `"Nome utente non trovato"` (`:127`) e `"Password errata"` (`:104`) sono distinti e permettono di enumerare gli username.
- CORS (`Installers.cs:383-398`): sostituire `AllowAnyOrigin()` / `SetIsOriginAllowed(_ => true)` con una whitelist da configurazione. Il ramo SignalR combina origine libera e `AllowCredentials()`, combinazione che va corretta in ogni caso.
- Swagger: limitarlo all'ambiente di sviluppo (`Program.cs:164-173` lo espone anche in produzione su `/docs`).

### Fase 5 — Chiusura dell'IDOR: `IdAz_Anagrafica` denormalizzato + global query filter

Analisi svolta sul modello: delle 39 entità del modulo presenze, **solo 13 hanno `IdAz_Anagrafica` diretto** (`Az_Cfg`, `Az_Cliente`, `Az_Commessa`, `Az_Sedi`, `My_template1`, `Par_Arrotondamenti`, `Par_Attivita`, `Par_Causali`, `Par_Competenza`, `Par_ExportCau`, `Par_Giustificativi`, `Par_Orario`, `Par_ProfiloOrario`). Le altre 26 sono agganciate in modo transitivo, fino a 4 livelli:

```
Az_Sedi → Az_SediReparto → Az_SediRepartoAttivita / Az_SediRepartoUser
Az_Commessa → Az_SubCommessa → Az_SubCommessaAttivita → Dip_RapportoLavoro → Dip_GG_*
Par_Orario → Par_OrarioIntervalloHH ; Par_ProfiloOrario → Par_ProfiloOrarioGG
```

**Criticità da segnalare**: `Dip_Anagrafica` ha come unica chiave esterna `IdAspNetUsers` — **nessun ancoraggio all'azienda**. L'anagrafica di un dipendente è collegata alla sua azienda solo passando per `Dip_RapportoLavoro → Az_SubCommessaAttivita → Az_SubCommessa → Az_Commessa`. Non è una scomodità: è una lacuna del modello. La denormalizzazione qui non è ottimizzazione, è correzione.

Passi:

1. **Migration + backfill**: aggiungere `IdAz_Anagrafica` (con FK e indice) alle 26 entità che ne sono prive. Il backfill risale la catena sopra con UPDATE...FROM; va scritto e verificato entità per entità nell'ordine topologico, partendo dalle radici.
2. **Scope accessor**: servizio scoped `ICurrentAzAnagraficaAccessor` che risolve `CurrentCompany` (claim `company`) → `Az_Anagrafica.Id`, con cache per richiesta. Va iniettato in `ApplicationDbContext` e memorizzato in un **campo d'istanza** del context: il global query filter deve referenziare un campo d'istanza perché EF lo tratti come parametro di query e non lo congeli nel modello compilato.
3. **Global query filter** in `Define_Table_DbContext_GestionePresenze` (`ApplicationDbContext_GestionePresenze.cs:60`), uniforme su tutte le entità tenant:
   ```csharp
   modelBuilder.Entity<T>().HasQueryFilter(e => e.IdAz_Anagrafica == _currentAzAnagraficaId);
   ```
   Introdurre un'interfaccia marker `IAzAnagraficaScoped` per applicarli in ciclo anziché uno per uno.
4. **Valorizzazione in scrittura**: impostare `IdAz_Anagrafica` in `Repository.CreateAsync` (`Repository.cs:130`) dallo stesso accessor, così una insert non può nascere fuori scope. È l'occasione per sostituire la ricerca via reflection di `Id`/`id`/`ID`, `ModifiedDate`, `CreationDate`, `ChangeUser` con `BaseEntity`, che tutte le entità già estendono.
5. **Percorsi di bypass espliciti**: gli inizializzatori (`GestionePresenzeCompanyInitializer`, `InfrastructureCompanyInitializer`, invocati da `CompanyGet`) e la migrazione all'avvio (`Program.cs:84-124`) operano legittimamente fuori scope e devono usare `IgnoreQueryFilters()` o un accessor in modalità elevata. Sono **pochi e vanno elencati**: ogni bypass è una potenziale falla e va motivato nel codice.
6. **Entità Public** (`Company`, `Dealer`, `FinancialAdvisor`, `UserCompany`, …): non hanno `IdAz_Anagrafica` e vanno protette diversamente — con la verifica di perimetro del `IUserScopeAuthorizer` della Fase 2, applicata in `CompanyGet`, `UserCompanyGet`, `DealerGet`, `FinancialAdvisorGet` e nei rispettivi `Put`.

### Fase 6 — Test

Nuovo progetto `nvxapp.server.tests` (xUnit) aggiunto alla solution.

- **Matrice ruolo → endpoint**: test parametrico che, per ogni policy, verifica accesso consentito ai ruoli previsti e `403` a tutti gli altri.
- **`UserScopeAuthorizer`**: i quattro casi della Fase 2 — sé stesso, discesa lecita, discesa illecita (`User` → `PowerAdmin`, il caso dell'exploit), risalita a nodo intermedio.
- **Query filter**: un contesto con scope azienda A non vede né modifica righe dell'azienda B.
- **Regressione dedicata** sull'exploit `UserList` → `UserLoad`, come test nominato: è la vulnerabilità che ha motivato tutto il lavoro e deve restare visibilmente coperta.

---

## Impatto sulla soluzione

### Cosa si rompe di proposito

| Ambito | Impatto |
|---|---|
| **Token esistenti** | Tutti invalidati (nuova chiave JWT + nuovi claim). Tutti devono rifare login. Nessun problema: non c'è produzione. |
| **Password esistenti** | Le password sotto i 10 caratteri restano valide al login (Identity non rivalida in ingresso) ma non sono più reimpostabili a valori deboli. Consigliato un reset generale in dev, dove le password sono `"1234"`. |
| **Chiamate API oggi lecite** | Qualunque chiamata fuori ruolo che oggi "funziona" inizierà a restituire `403`. Se il client si appoggia inconsapevolmente a una di queste, emerge in questa fase — è l'effetto desiderato, ma va messo in conto in collaudo. |
| **Dati fuori scope** | Dopo la Fase 5 le righe orfane o con `IdAz_Anagrafica` non risolvibile diventano **invisibili**. Il backfill va verificato con conteggi prima/dopo, per riga e per tabella. |

### Impatto sul client Angular

Sorprendentemente contenuto, perché la logica di ruolo esiste già:

- **Gestione del `403`**: `NvxHttpInterceptor` (`http-interceptor.ts:58-67`) gestisce solo il `401`. Va aggiunto il `403` con un messaggio comprensibile, altrimenti l'utente vede un errore muto.
- **Guard invariate**: restano come sono. Cambia il loro significato — da controllo di sicurezza a scorciatoia di UX — ma non il codice.
- **`UserNavigationService`**: nessuna modifica necessaria. Le navigazioni che oggi esegue restano tutte lecite sotto la nuova regola. Va però gestito il caso in cui `UserLoad` fallisce (oggi il ramo `else` è vuoto in `login-page.component.ts:114` e in `company-list-page.component.ts:106`).
- **`role-guard-4-user-impersonate.ts`**: verifica solo `UserCanGoBack`, nessun ruolo. Ora è coperto dal server, ma vale la pena allinearlo.

### Dimensione del lavoro

| Fase | Ampiezza | Rischio di regressione |
|---|---|---|
| 0 — Segreti e chiave | Pochi file | Nullo |
| 1 — Ruoli nel token | 4 file, chirurgico | Basso (comportamento invariato) |
| 2 — Gate su `UserLoad` | 1 servizio nuovo + 2 metodi | Basso, alto valore |
| 3 — Policy sugli endpoint | 48 controller, ripetitivo | **Medio** — è qui che si scopre chi usava cosa |
| 4 — Hardening auth | ~5 file | Basso |
| 5 — IDOR + denormalizzazione | Migration su 26 tabelle + data layer | **Alto** — il backfill tocca i dati |
| 6 — Test | Progetto nuovo | Nullo |

Le Fasi 0-4 sono a rischio contenuto e chiudono l'escalation di privilegio. La Fase 5 è il grosso del lavoro e l'unica che tocca i dati: va affrontata separatamente, con backup e verifica dei conteggi.

### Cosa resta fuori (consapevolmente)

- **Multi-tenant a schema**: `SharedSchema.CurrentSchema` è uno `static` mutabile condiviso tra richieste concorrenti (`SharedSchema.cs:16`), e `Repository.FindAll(predicate)` restituisce un `IQueryable` dopo che il `finally` ha già rimesso lo schema a `public` (`Repository.cs:320-333`). Con `MultiTenant: false` è dormiente e non aggiunge rischio oggi. **Ma va risolto prima di accendere il multi-tenant**, altrimenti si mescolano dati tra clienti in modo non riproducibile.
- Refactoring del boilerplate CRUD (~179 metodi quasi identici), rimozione dei `Task.Delay(DelayAsyncMethod)`, semantica HTTP dei codici di stato. Debito reale ma non di sicurezza.

---

## Verifica

**Dopo ogni fase**: `dotnet build nvxapp.sln` deve restare a **0 warning** — è il livello attuale e va difeso.

**Fase 1** — regressione funzionale completa: login con un utente per ruolo, navigazione, drill-down, `UserGoBack`. Comportamento identico a prima. Ispezionare un token su jwt.io e verificare la presenza dei claim di ruolo.

**Fase 2** — riprodurre l'exploit e verificare che fallisca:
```bash
# 1. login come utente 'User' → TOKEN
# 2. enumerare gli admin (deve continuare a funzionare finché non è attiva la Fase 3)
curl -X POST .../api/Account/UserList -H "Authorization: Bearer $TOKEN" -d '{"Data":{}}'
# 3. tentare l'escalation → deve fallire, senza token in risposta
curl -X POST .../api/Account/UserLoad -H "Authorization: Bearer $TOKEN" \
     -d '{"Data":{"Id":"<id-di-un-PowerAdmin>"}}'
```
Verificare inoltre che il Warning compaia nei log Serilog, e che login, drill-down e risalita di un `FinancialAdvisorAdmin` continuino a funzionare.

**Fase 3** — per ogni ruolo, un giro completo dell'applicazione dalla UI: nessun `403` su percorsi legittimi. In parallelo, chiamata diretta a un endpoint di livello superiore con token di livello inferiore → `403`.

**Fase 5** — prima della migration, salvare i conteggi per tabella; dopo il backfill, verificare che nessuna riga abbia `IdAz_Anagrafica` nullo o non risolvibile. Poi: login come azienda A, verifica che `Par_CausaliGet` con un id dell'azienda B non restituisca nulla; idem per `Put` e `Delete`.

**Fase 6** — `dotnet test` verde, con il test di regressione sull'exploit incluso.

**Verifica finale**: rilettura degli endpoint privi di policy esplicita (la fallback policy li copre, ma vanno elencati e giustificati) e dei punti di `IgnoreQueryFilters()` introdotti in Fase 5.
