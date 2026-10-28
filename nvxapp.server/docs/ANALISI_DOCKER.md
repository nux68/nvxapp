# Analisi: distribuzione della solution su Docker

> Analisi del 26/10/2026, branch `AttendanceTracking_Dev`. Interventi in corso dal 27/10/2026 su `Infrastructure_Dev`.
>
> **Server di destinazione**: Contabo Cloud VPS Core 4 con Ubuntu 24.04, un'unica istanza dell'app (sezione 9).

**Stato al 27/10/2026: 11 punti su 14 risolti, 3 non necessari sul VPS. Docker pronto: manca solo la preparazione del VPS (sezione 9).**

| Stato | Punti |
|---|---|
| ✅ Risolti | 1 URL API relative · 2 cultura e fuso (versione minima) · 3 percorso log · 4 chiavi DataProtection · 5 reverse proxy · 6 Dockerfile · 7 segreti in variabili d'ambiente · 7b chiavi AI ed email senza file `.key` · 8 `.dockerignore` · 9 `docker-compose` con profili per ambiente · 13 Swagger e CORS |
| ➖ Non necessari sul VPS | 10 migrazioni all'avvio · 11 SignalR con più repliche · 12 cartella export: servono solo con più istanze dell'app |

Legenda: ✅ risolto · ⏳ da fare · ➖ non necessario con un'istanza sola (da riprendere se si scala) · 🔍 da valutare. Dettagli di ogni intervento nella sezione 7, segreti nella sezione 8, server di destinazione nella sezione 9, come far partire l'app (PC e VPS) nella sezione 10.

## 1. Situazione attuale

- **Backend .NET 10**: `nvxapp.server` più `nvxapp.server.data` e `nvxapp.server.service`.
  - Kestrel ascolta sulla porta definita in `RunTime:Port`.
  - Il server serve anche il client Angular da `wwwroot`: frontend e API stanno nello stesso processo, quindi per Docker basta **un solo container applicativo**.
- **Servizi esterni**:
  - PostgreSQL: dati, log Serilog (opzionale) e Hangfire (opzionale).
  - RabbitMQ: chat AI.
  - SMTP Brevo.
  - API Groq / OpenRouter.
- **Pubblicazione**:
  - C'è solo `Properties/PublishProfiles/IISProfile.pubxml`, cioè MSDeploy su IIS locale.
  - La build Angular la lancia il target MSBuild `BuildAngular` (`npx ng build`), solo in Release.

## 2. Pro e contro

### Pro

- **Ambienti riproducibili**: la stessa immagine va in test e in produzione, senza IIS né Hosting Bundle sul server.
- **Si può girare su Linux**: costa meno e ci sono più opzioni di hosting (VPS, Azure Container Apps, Kubernetes).
- **Un solo comando**: con `docker compose` si avviano insieme app, Postgres e RabbitMQ. Utile anche per lo sviluppo e i server di prova.
- **Rollback immediato**: basta rilanciare il tag dell'immagine precedente.
- **Configurazione separata dai segreti**: diventa naturale usare variabili d'ambiente. Oggi i segreti stanno in `appsettings.json`, quindi questo aiuta anche il piano sicurezza (`piano-security.md`).

### Contro

- **Competenze e infrastruttura in più**: Docker, un registry per le immagini e un reverse proxy per HTTPS.
- **Il container è effimero**: tutto ciò che oggi viene scritto su disco va spostato su volumi (log, export, file `.key`, chiavi DataProtection).
- **Differenze Linux / Windows**: fuso orario, cultura e percorsi con `\`.
- **Database in container**: va bene per test e sviluppo. In produzione servono volumi e backup curati, oppure Postgres gestito o installato fuori dal container.
- **Debug di produzione**: meno immediato rispetto a IIS sulla stessa macchina.

## 3. Lavoro da fare

### 3.1 Bloccanti o quasi

| # | Stato | Problema | Dove | Intervento |
|---|---|---|---|---|
| 1 | ✅ | L'URL delle API è scritto nella build del client: `apiUri: 'http://192.168.0.227:8146/api/'` | `nvxapp.client/src/environments/environment.prod.ts` | Usare URL relative (`/api/`, e `/` per SignalR): frontend e API hanno la stessa origine. Una sola build vale per tutti gli ambienti ed è il prerequisito per avere più profili. |
| 2 | ✅ (minima) | Fuso orario e cultura: nei container Linux il default è UTC con cultura invariante. Nel `.service` ci sono circa 28 usi di `DateTime.Now` / timezone. `Program.cs` lascia date e numeri alla cultura della macchina. | `Program.cs`, servizi | `TZ=Europe/Rome` più un'immagine con ICU e `tzdata` (Debian, non Alpine o chiseled), oppure cultura e fuso impostati da configurazione. |
| 3 | ✅ | Il log su file scrive su `Logs\\nvxapp.log`: su Linux crea un file che si chiama letteralmente `Logs\nvxapp.log`. | `nvxapp.server/Utility/Installers.cs:247` | Usare `Path.Combine("Logs", "nvxapp.log")`, oppure solo log su console o DB. |
| 4 | ✅ | Le chiavi DataProtection (che firmano i token di conferma email e reset password di Identity) restano nel filesystem del container: si perdono a ogni redeploy e i link già inviati smettono di funzionare. | Configurazione servizi | Salvarle su un volume o su DB (`PersistKeysToFileSystem` / `PersistKeysToDbContext`). |
| 5 | ✅ | HTTPS dietro reverse proxy: manca `UseForwardedHeaders()`, quindi l'app crede di girare in http e i link generati e i redirect escono sbagliati. | `Program.cs` | Terminare TLS su nginx, Traefik o Caddy, lasciare `RunTime:UseHttps=false` nel container, aggiungere `UseForwardedHeaders()` e impostare `App:PublicUrl`. |

### 3.2 Necessari

| # | Stato | Intervento |
|---|---|---|
| 6 | ✅ | **Dockerfile multi-stage**: uno stage Node per `ng build`, uno stage SDK per `dotnet publish -c Release -p:AngularBuilt=true` (così salta il target MSBuild) e uno stage `aspnet` di runtime. Il target `CleanWwwrootDebug` usa PowerShell ma scatta solo in Debug: nessun problema. |
| 7 | ✅ | **Segreti fuori dagli appsettings** (password Postgres, chiave JWT, password RabbitMQ): passarli come variabili d'ambiente (`ConnectionStrings__nvxappDbContext`, `JwtParameter__Key`, …) o come Docker secrets. I file `groq.key`, `openrouter.key` e `brevo.key`, letti da `ContentRootPath`, vanno montati come secret o volume, oppure letti da variabile d'ambiente come fa già `Email__Password`. |
| 8 | ✅ | **`.dockerignore`**: escludere `node_modules`, `bin`, `obj`, `.angular`, i file `.key` e `Logs`. |
| 9 | ✅ | **`docker-compose.yml`** con app, postgres (con volume), rabbitmq e, se serve, il reverse proxy. |

### 3.3 Da valutare

| # | Stato | Tema | Nota |
|---|---|---|---|
| 10 | ➖ Non necessario sul VPS | Migrazioni all'avvio | `DatabaseInitializer` migra a ogni avvio. Con una sola istanza va bene; con più repliche c'è il rischio di migrazioni concorrenti, quindi meglio un job separato o un lock. `DbParameter:MultiTenant` si fissa al primo avvio su DB vuoto: ogni profilo deve averlo coerente con il proprio database. |
| 11 | ➖ Non necessario sul VPS | SignalR con più repliche | Servono sticky session o un backplane (Redis). Con una sola istanza non serve nulla. |
| 12 | ➖ Non necessario sul VPS | Cartella export | È in `Path.GetTempPath()/nvxapp_exports`: va bene per file temporanei, ma con più repliche il download può arrivare a un'istanza diversa da quella che ha creato il file. |
| 13 | ✅ | Sicurezza | Swagger esposto in produzione su `/docs` e CORS che accetta qualsiasi origine (`SetIsOriginAllowed(origin => true)`). Con stessa origine dietro proxy il CORS si può restringere. |

**Stima indicativa**: i punti 1–9 richiedono circa 2–4 giorni, test compresi. I punti 10–13 dipendono da quanto si vuole scalare.

## 4. Più profili di pubblicazione

Si possono avere. L'approccio consigliato è **una sola immagine**, con la configurazione per ambiente iniettata al momento del deploy.

| Livello | Come |
|---|---|
| Ambiente .NET | `ASPNETCORE_ENVIRONMENT=Test` / `Production`, che carica `appsettings.Test.json`, `appsettings.Production.json`, …. Il meccanismo esiste già: vedi `appsettings.Development.DEV1.json`. |
| Valori specifici e segreti | Variabili d'ambiente nel compose, che sovrascrivono gli appsettings. |
| Compose per ambiente | `docker-compose.yml` di base più `docker-compose.test.yml` e `docker-compose.prod.yml` (`docker compose -f docker-compose.yml -f docker-compose.prod.yml up -d`), oppure i `profiles:` di compose per attivare servizi opzionali (Hangfire, RabbitMQ, Postgres locale). |
| File di ambiente | `DockerLocale.env` (PC) e `DockerVps.env` (server), copie di `.env.example` escluse da git, con profilo, tag dell'immagine, segreti e URL pubblico. Per un altro ambiente basta un altro file, passato con `--env-file`. |
| Visual Studio | Profilo **`DockerLocale`** accanto a `IISProfile`: pubblica su Docker Desktop lanciando `docker compose --env-file DockerLocale.env up -d --build` (sezione 10.1). Il profilo IIS resta invariato. |
| Angular | Le configurazioni di `angular.json` (`production`, `development`, `ci`) restano. Con le URL relative del punto 1 non servono build diverse per ambiente. |

### Dockerfile

Il `Dockerfile` è nella cartella della solution (punto 6, dettagli nella sezione 7), insieme al `.dockerignore` (punto 8). Si costruisce da quella cartella:

```
docker build -t nvxapp:<tag> .
```

L'immagine non contiene configurazione d'ambiente né segreti: è la stessa per test e produzione, e quello che cambia si passa all'avvio con le variabili `NVXAPP_*` (sezione 8).

## 5. Interventi che migliorano l'applicazione anche senza Docker

Molti punti sono utili anche con il deploy IIS attuale e rendono la migrazione futura quasi gratuita.

### 5.1 Da fare subito

| # | Stato | Intervento | Beneficio indipendente da Docker | Sforzo | Rischio |
|---|---|---|---|---|---|
| 1 | ✅ | URL API relative in `environment.prod.ts` | Oggi IP e porta sono scritti nella build: se cambia server, porta o dominio, o si passa a https, va ricompilato Angular. Con URL relative una build vale ovunque. | 10 min | Basso. Va verificato che nessuno serva Angular separatamente dalle API in produzione. |
| 3 | ✅ | Percorso dei log con `Path.Combine` | Elimina un bug latente: il percorso oggi funziona solo su Windows. | 5 min | Nullo |
| 4 | ✅ | Chiavi DataProtection persistenti | Anche su IIS, se l'app pool non carica il profilo utente, le chiavi restano in memoria: a ogni riciclo i link di conferma email e reset password già inviati non funzionano più. | 30 min | Basso |
| 7 | ✅ | Segreti sovrascrivibili da variabili d'ambiente e tolti da `appsettings.json` (password DB, chiave JWT, password RabbitMQ) | Oggi la chiave JWT e la password RabbitMQ sono nel repository. È un punto del piano sicurezza. | 1–2 h | Medio: chi avvia l'app deve avere le variabili impostate (IIS, Visual Studio). |
| 7b | ✅ | File `.key` leggibili anche da variabile d'ambiente (`AI__GroqKey`, …), come già `Email__Password` | Gestione dei segreti uniforme. | 30 min | Basso |
| 13 | ✅ | Swagger disattivabile in produzione da configurazione e CORS ristretto | Hardening: oggi `/docs` è pubblico e il CORS accetta qualsiasi origine. Anche questo è nel piano sicurezza. | 1 h | Basso/medio: va verificata l'app PWA o mobile, se chiama da un'altra origine. |

### 5.2 Utili, da valutare

| # | Stato | Intervento | Nota |
|---|---|---|---|
| 2 | ✅ (minima) | Cultura e fuso orario espliciti (`it-IT` / `Europe/Rome` da configurazione) | Rende l'app indipendente dalle impostazioni del server, cosa preziosa per un'app di rilevazione presenze. La versione minima (cultura da config in `Program.cs`) costa poco. Rivedere i ~28 `DateTime.Now` è un lavoro a parte, da fare solo con test. |
| 5 | ✅ | `UseForwardedHeaders` | Serve solo con un proxy davanti (nginx, IIS reverse proxy, Cloudflare). Non fa danni. |
| 12 | ➖ Non necessario sul VPS | Cartella export configurabile | Permette di indicare un disco o una cartella dedicata invece di `%TEMP%`. |
| 10 | ➖ Non necessario sul VPS | Flag per disattivare la migrazione automatica all'avvio | Permette di controllare quando si migra in produzione. |

### 5.3 Senza effetti sull'app attuale

- ✅ Dockerfile, `.dockerignore` e `docker-compose` con override per PC e server (punti 6, 8 e 9). Sono file in più che non toccano il funzionamento attuale. Utili subito anche solo per avere un ambiente di test con Postgres e RabbitMQ in un comando.

### 5.4 Solo se si scala su più istanze

- ➖ Backplane SignalR e migrazioni come job separato (punti 10 e 11 nella forma completa). Non servono sul VPS, dove gira un'istanza sola.

## 6. Proposta operativa

1. ✅ **Primo blocco** (circa mezza giornata, rischio basso): punti 1, 3, 4, 7b, 13 e versione minima del 2. Completato, insieme al punto 5.
2. ✅ **Secondo blocco**: punto 7 (segreti in variabili d'ambiente), da fare separatamente perché cambia come si avvia l'app su IIS e in Visual Studio.
3. ✅ **Terzo blocco**: Dockerfile, `.dockerignore` e compose con override per PC e server, verificati su Linux (Docker Desktop).
4. ⏳ **Quarto blocco**: preparazione del VPS Contabo e primo rilascio (sezione 9).

Seguendo il flusso dei branch, gli interventi infrastrutturali (3, 4, 7, 13) vanno fatti su **Infrastructure_Dev** e poi mergiati verso AttendanceTracking.

## 7. Stato di avanzamento

| # | Punto | Stato | Data | Branch | Note |
|---|---|---|---|---|---|
| 1 | URL API relative nel client | ✅ Risolto | 27/10/2026 | `Infrastructure_Dev`, mergiato fino ad `AttendanceTracking` | In `nvxapp.client/src/environments/environment.prod.ts`: `apiUri: '/api/'` e `signalrUri: '/'`. Tutti i servizi concatenano il percorso a queste basi, così chiamate HTTP, download (`page-toolbar`) e hub SignalR (`/chathub`) usano l'origine della pagina. `environment.ts` (sviluppo con `ng serve` su `https://localhost:7146`) è invariato. Capacitor è presente solo come configurazione del template Ionic, senza piattaforme native: se un giorno si crea l'app nativa servirà di nuovo un indirizzo assoluto. Verificato con build Angular di produzione: nel bundle `apiUri` vale `/api/` e l'IP non compare più. |
| 2 | Cultura e fuso orario | ✅ Risolto (versione minima) | 27/10/2026 | `Infrastructure_Dev`, mergiato fino ad `AttendanceTracking` | Nuove chiavi `RunTime:Culture` (`it-IT`) e `RunTime:TimeZone` (`Europe/Rome`) in `appsettings.json`, applicate da `Installers.InstallCultureAndTimeZone` (chiamato in `Program.cs` subito dopo `InstallLog`). **Cultura**: `CultureInfo.DefaultThreadCurrentCulture`, quindi vale anche fuori dalle richieste HTTP (job, RabbitMQ, export). `UseRequestLocalization` usa la stessa cultura: prima era quella della macchina. **Fuso**: su Linux/Docker imposta la variabile `TZ` del processo, così `DateTime.Now` è l'ora italiana anche in un container che parte in UTC. Su Windows il fuso resta quello della macchina e, se diverso da quello configurato, scrive un avviso nel log. All'avvio il log riporta cultura e fuso in uso. Chiavi vuote = impostazioni della macchina, come prima. Verificato su Windows: `Cultura it-IT, fuso orario W. Europe Standard Time`, e avviso corretto con un fuso diverso. **Non verificato su Linux** (Docker Desktop non avviato): da provare con il primo container. Per Docker serve un'immagine con ICU e `tzdata` (Debian/Ubuntu, non Alpine o chiseled). **Escluso**: la revisione dei ~28 `DateTime.Now` (es. passaggio a UTC o a un servizio orario), lavoro a parte da fare con i test. |
| 3 | Percorso log con `Path.Combine` | ✅ Risolto | 27/10/2026 | `Infrastructure_Dev`, mergiato fino ad `AttendanceTracking` | In `nvxapp.server/Utility/Installers.cs` (`InstallLog`) il file di log è `Path.Combine(builder.Environment.ContentRootPath, "Logs", "nvxapp.log")` al posto di `"Logs\\nvxapp.log"`: corretto su Windows e Linux e indipendente dalla cartella corrente del processo. Su IIS e in Visual Studio la cartella resta la stessa di prima (`<cartella app>/Logs`). In Docker basta montare un volume su `/app/Logs`. |
| 4 | Chiavi DataProtection persistenti | ✅ Risolto | 27/10/2026 | `Infrastructure_Dev`, mergiato fino ad `AttendanceTracking` | Chiavi salvate su DB nella nuova tabella `public."DataProtectionKeys"` (`PublicDbContext` implementa `IDataProtectionKeyContext`, pacchetto `Microsoft.AspNetCore.DataProtection.EntityFrameworkCore` 10.0.12, migrazione `AddDataProtectionKeys`). In `Installers.cs` (`InstallEntityContex`): `AddDataProtection().SetApplicationName("nvxapp").PersistKeysToDbContext<PublicDbContext>()`. `ApplicationName` fisso perché di default dipende dalla cartella dell'app (IIS, Docker e sviluppo sono diverse). Scelto il DB invece di una cartella perché il profilo IIS (`SkipExtraFilesOnServer=false`) cancella a ogni pubblicazione i file non presenti nel pacchetto. Su DB funziona anche in Docker senza volumi e con più istanze. Verificato su un database di prova: al primo avvio la migrazione crea la tabella e salva la chiave; al riavvio la chiave viene riletta (nessuna nuova chiave). **Nota:** al primo avvio i link di conferma email / reset password inviati prima dell'aggiornamento non valgono più (cambia la chiave). Le chiavi sono salvate in chiaro nel DB (avviso `XmlKeyManager[35]` al primo avvio): accettabile con un DB protetto, cifrabile in futuro con `ProtectKeysWithCertificate`. |
| 5 | `UseForwardedHeaders` | ✅ Risolto (disattivato di default) | 27/10/2026 | `Infrastructure_Dev`, mergiato fino ad `AttendanceTracking` | Nuova sezione `ReverseProxy` in `appsettings.json` (`Enabled: false`, `KnownProxies`, `KnownNetworks`). `Installers.InstallForwardedHeaders` configura `X-Forwarded-For` e `X-Forwarded-Proto`; in `Program.cs` `app.UseForwardedHeaders()` è il primo middleware, ma solo con `ReverseProxy:Enabled=true`. **Spento non cambia nulla in debug né su IIS in-process**: il codice non viene eseguito. Con proxy dichiarati si accettano gli header solo da loro (più localhost); con liste vuote da chiunque, con avviso nel log: va bene solo se la porta dell'app è raggiungibile unicamente dal proxy (rete interna di docker compose). In Docker: `ReverseProxy__Enabled=true`. Verificato con `UseHttpsRedirection` e `X-Forwarded-Proto: https`: spento = 307 verso https (header ignorato); acceso = nessun redirect; acceso senza header = 307. **Valore oggi limitato**: il server non usa schema, host o IP della richiesta (i link delle email usano `App:PublicUrl` o l'header `Origin`); serve a IP reale nei log, a `UseHttpsRedirection` e a usi futuri dietro proxy. |
| 6 | Dockerfile multi-stage | ✅ Risolto | 27/10/2026 | `Infrastructure_Dev`, mergiato fino ad `AttendanceTracking` | `Dockerfile` nella cartella della solution, tre stage. **web** (`node:22-bookworm-slim`): `npm ci` e `ng build --configuration production` → `nvxapp.client/www`. **build** (`dotnet/sdk:10.0`): restore dei soli `.csproj` (resta in cache finché i pacchetti non cambiano), client copiato in `nvxapp.server/wwwroot`, `dotnet publish -c Release -p:AngularBuilt=true` (salta il target `BuildAngular` del csproj). **final** (`dotnet/aspnet:10.0`, Ubuntu 24.04): immagine completa con ICU e `tzdata`, più `libgssapi-krb5-2` (Npgsql la cerca alla prima connessione: senza, scrive `Cannot load library libgssapi_krb5.so.2`); gira con l'utente non root `app`; cartella `/app/Logs` scrivibile, da montare come volume. Variabili fisse: `ASPNETCORE_ENVIRONMENT=Production`, `NVXAPP_RunTime__Port=8080`, `NVXAPP_RunTime__UseHttps=false`, `TZ=Europe/Rome`; `ASPNETCORE_HTTP_PORTS` svuotata perché la porta la apre l'app. Nessun segreto nell'immagine: tutto il resto all'avvio con le variabili `NVXAPP_*`. Immagine di circa **285 MB**; prima build circa 4 minuti, le successive riusano la cache. **Verificato** con Docker Desktop (linux/amd64, come il VPS) su una rete con Postgres 15 e RabbitMQ in container: primo avvio su DB vuoto con migrazioni e modalità multi-tenant salvata; log `Cultura it-IT, fuso orario Europe/Rome` e ora del container in CET; client Angular servito (`/`, `/login`, `/reset-password` → 200); login 200; API protetta 401 senza token e 200 con token; negoziazione SignalR riuscita; connessione a RabbitMQ attiva; Swagger spento (`swagger.json` 404); log su file in `/app/Logs`. Secondo avvio sullo stesso DB: nessuna migrazione e chiave DataProtection riletta. PDF (AttendanceTracking): già indipendenti dai font installati, perché `PdfFontResolver` usa i font Segoe WP inclusi in PDFsharp. |
| 7 | Segreti in variabili d'ambiente | ✅ Risolto | 27/10/2026 | `Infrastructure_Dev`, mergiato fino ad `AttendanceTracking` | Connessione DB, chiave JWT e password RabbitMQ tolte da `appsettings.json`, `appsettings.Development.json` e `appsettings.Development.DEV1.json`: valori `""` con commento che indica la variabile. In `InstallSettings` le variabili con prefisso `NVXAPP_` (`__` al posto di `:`) sono aggiunte per ultime e vincono sugli appsettings; il prefisso evita che le leggano altre app .NET della macchina. All'avvio `CheckRequiredSecrets` ferma l'app se mancano connessione DB o chiave JWT (almeno 32 byte, requisito di HMAC-SHA256), indicando la variabile. Anche la factory di design-time di `dotnet ef` legge `NVXAPP_ConnectionStrings__nvxappDbContext`. Elenco e modalità d'uso nella sezione 8. Verificato su server di prova: senza variabili l'app si ferma con il messaggio; con le variabili login 200 e API protetta 401 senza token / 200 con token. **I valori tolti restano nella storia di git e su origin: vanno cambiati** (chiave JWT, password DB e RabbitMQ). |
| 7b | Chiavi AI e password SMTP senza file `.key` | ✅ Risolto | 27/10/2026 | `Infrastructure_Dev`, mergiato fino ad `AttendanceTracking` | `ChatAIService` non legge più `groq.key` e `openrouter.key`: le chiavi sono `AI:AI_groq:ApiKey` e `AI:AI_openrouter:ApiKey` (variabili `NVXAPP_AI__AI_groq__ApiKey`, `NVXAPP_AI__AI_openrouter__ApiKey`). È obbligatoria **solo quella del provider in uso** (`AI:UseLLM`): prima mancava un file qualsiasi e la chat si bloccava. `EmailService` non legge più `brevo.key`: password solo da `Email:Password` (`NVXAPP_Email__Password`), con errore esplicito se manca. Rimosse dagli appsettings le chiavi mai lette `OpenRouterKey` e `AI_local:Key` e il segnaposto `TUO_BREVO_KEY`. Verificato: senza la chiave Groq la chat risponde 500 con "impostare la variabile d'ambiente NVXAPP_AI__AI_groq__ApiKey". I file `.key` in `nvxapp.server/` e in `C:\inetpub\wwwroot\nvxapp\` non servono più e si possono eliminare, dopo aver conservato le chiavi altrove. |
| 8 | `.dockerignore` | ✅ Risolto | 27/10/2026 | `Infrastructure_Dev`, mergiato fino ad `AttendanceTracking` | Fatto insieme al punto 6, perché senza la build non funziona: le cartelle `obj` di Windows contengono percorsi locali che rompono `dotnet restore`. Esclude `.git`, `.vs`, `.claude`, `*.user`, `node_modules`, `bin`, `obj`, `.angular`, `dist`, `www`, `wwwroot`, i segreti (`*.key`, `*.segreti.ps1`, `.env`, `.env.*` tranne `.env.example`), `Logs` e `nvxapp.server/Note`. Il contesto inviato a Docker passa da oltre 400 MB a circa 5 MB e nell'immagine non finisce nessun file `.key`. |
| 9 | `docker-compose` con override per ambiente | ✅ Risolto | 27/10/2026 | `Infrastructure_Dev`, mergiato fino ad `AttendanceTracking` | File nella cartella della solution. **`docker-compose.yml`** (base): app, `postgres:15` e `rabbitmq:3-management` con volumi (`pg_data`, `rabbit_data`, `nvxapp_logs`), healthcheck (l'app parte solo con DB e RabbitMQ pronti), `restart: unless-stopped`, log Docker limitati a 5 × 10 MB, nessuna porta pubblicata. La stringa di connessione la compone il compose da `POSTGRES_PASSWORD`; host `postgres` e `rabbitmq`. **`docker-compose.local.yml`** (PC): costruisce l'immagine, pubblica solo su `127.0.0.1` app (8080), console RabbitMQ (15672) e Postgres (5434, perché 5432 e 5433 sono già usate). **`docker-compose.prod.yml`** (VPS): Caddy su 80/443 con `docker/Caddyfile` (HTTPS automatico, compressione, WebSocket), `ReverseProxy:Enabled=true`, `App:PublicUrl` obbligatorio, `mem_limit` (app 2 GB, Postgres 3 GB con `shared_buffers=1GB`, RabbitMQ 1 GB, Caddy 256 MB). **`.env.example`** in git, `.env` escluso da git (`.gitignore`) e dal contesto di build: il profilo si sceglie con `COMPOSE_FILE` nel `.env`, poi basta `docker compose up -d`; i segreti obbligatori mancanti fermano compose con un messaggio. **Verificato** su Docker Desktop: profilo locale = avvio ordinato dopo i healthcheck, client e login 200, API 401/200, connessione RabbitMQ, console RabbitMQ, dati e chiave DataProtection conservati dopo `down`/`up`; profilo server (porte 8081/8443, dominio `localhost` con certificato interno di Caddy) = HTTPS con HTTP/2 e compressione zstd, redirect http → https, login 200, WebSocket SignalR attraverso Caddy (101), solo Caddy con porte pubblicate, limiti di memoria applicati. **Non verificato**: certificato Let's Encrypt vero e separatore `:` di `COMPOSE_FILE` su Linux, da provare sul VPS. **28/10/2026**: nel profilo locale aggiunto Caddy con `docker/Caddyfile.local` per l'https sulla rete locale (https://`LAN_IP`:8443, certificati dell'autorità locale di Caddy; sezione 10.1); verificato con il certificato radice: https su `192.168.0.227:8443` e `localhost:8443` valido, HTTP/2, zstd, login 200, WebSocket SignalR 101. |
| 10 | Migrazioni all'avvio | ➖ Non necessario sul VPS | 27/10/2026 | | Un'istanza sola: la migrazione all'avvio va bene. Prima di ogni rilascio, backup del DB (sezione 9). |
| 11 | SignalR con più repliche | ➖ Non necessario sul VPS | 27/10/2026 | | Un'istanza sola: nessun backplane né sticky session. |
| 12 | Cartella export configurabile | ➖ Non necessario sul VPS | 27/10/2026 | | Un'istanza sola: i file temporanei nel container vanno bene; si perdono al riavvio, ma sono temporanei. |
| 13 | Swagger in produzione e CORS | ✅ Risolto | 27/10/2026 | `Infrastructure_Dev`, mergiato fino ad `AttendanceTracking` | **CORS**: rimossi le policy `AllowAllOrigins` / `DynamicOrigins` (quest'ultima mai usata) e il middleware `UseDynamicCors`, che con SignalR attivo rimandava indietro qualsiasi `Origin` con le credenziali. Ora c'è un'unica policy da `Cors:AllowedOrigins` (`Installers.InstallCors`, `app.UseCors(Installers.CorsPolicyName)`). Vuoto = nessun CORS (`appsettings.json`: il client è servito dal server, stessa origine). Elenco = solo quelle origini. `"*"` = qualsiasi, con avviso nel log (`appsettings.Development.json`, per `ng serve`). Sempre `AllowCredentials`, richiesto da SignalR. **Swagger**: non dipende più dal nome dell'ambiente ma da `Swagger:Enabled` e `Swagger:RoutePrefix`: spento in `appsettings.json`, acceso su `/swagger` in sviluppo (ora anche con `Development.DEV1`, dove prima `IsDevelopment()` era falso). **Attenzione:** sul server IIS di test, che usa `appsettings.json`, dopo la prossima pubblicazione `/docs` non risponde più. Se serve: `Swagger__Enabled=true`. Verificato su server di prova: produzione = swagger 404 e nessun `Access-Control-Allow-Origin` per API e `/chathub` da un sito esterno; sviluppo e DEV1 = swagger 200 e CORS come prima; origine dichiarata = passa solo quella. |

## 8. Segreti: variabili d'ambiente

I segreti non stanno negli appsettings (che vanno in git) ma in variabili d'ambiente con prefisso `NVXAPP_` e `__` al posto di `:`. Le legge solo nvxapp e vincono sugli appsettings.

| Variabile | Chiave di configurazione | Obbligatoria |
|---|---|---|
| `NVXAPP_ConnectionStrings__nvxappDbContext` | `ConnectionStrings:nvxappDbContext` | Sì: senza, l'app non parte |
| `NVXAPP_JwtParameter__Key` | `JwtParameter:Key` | Sì, almeno 32 caratteri: senza, l'app non parte |
| `NVXAPP_RabbitQm__Connection__Password` | `RabbitQm:Connection:Password` | Per la chat AI via RabbitMQ |
| `NVXAPP_Email__Password` | `Email:Password` | Per inviare email (conferma registrazione, reset password) |
| `NVXAPP_AI__AI_groq__ApiKey` | `AI:AI_groq:ApiKey` | Solo con `AI:UseLLM = AI_groq` |
| `NVXAPP_AI__AI_openrouter__ApiKey` | `AI:AI_openrouter:ApiKey` | Solo con `AI:UseLLM = AI_openrouter` |

Qualsiasi altra chiave di configurazione si può sovrascrivere allo stesso modo (es. `NVXAPP_Swagger__Enabled=true`).

**Windows, Visual Studio e IIS sulla stessa macchina**: variabili di sistema, impostate una volta sola.
- `docs/imposta-variabili-nvxapp.ps1` (in git, senza segreti): tiene le variabili già impostate, chiede le mancanti senza mostrarle. Con `-Prova` mostra solo lo stato, con `-Sovrascrivi` richiede anche quelle già impostate.
- `docs/imposta-variabili-nvxapp.segreti.ps1` (copia locale, esclusa da git con `*.segreti.ps1`): stesso effetto con i valori scritti dentro.
- Da PowerShell aperto come amministratore: `powershell -ExecutionPolicy Bypass -File <script>`. Lo script fa `iisreset`; poi chiudere e riaprire Visual Studio. Va rilanciato solo per cambiare un valore o su un'altra macchina.
- Controllo: `(Get-ChildItem Env:NVXAPP_*).Name` in una nuova finestra.

**Docker**: le variabili di sistema di Windows non arrivano nei container. Si usa un file di ambiente accanto al compose, copia di `.env.example` ed escluso da git (`*.env`): `DockerLocale.env` sul PC, `DockerVps.env` per il server, dove si copia come `.env`. Il compose ne prende i valori e li passa all'app con i nomi `NVXAPP_*`. La connessione DB la compone il compose da `POSTGRES_PASSWORD` (host `postgres`), RabbitMQ usa `RABBITMQ_PASSWORD` (host `rabbitmq`); chiave JWT, email e AI hanno gli stessi nomi della tabella. Come far partire l'app nella sezione 10.

**Linux senza Docker**: `EnvironmentFile=/etc/nvxapp/nvxapp.env` nel servizio systemd (file con `chmod 600`).

## 9. Server di destinazione: Contabo Cloud VPS Core 4

Il compose di produzione deve funzionare su questo server ([scheda Contabo](https://contabo.com/en/vps/cloud-vps-core-4?image=ubuntu.332&qty=1&contract=24&storage-type=ssd-storage-cloud-vps-core-4-default)).

| Caratteristica | Valore | Conseguenza |
|---|---|---|
| CPU | 4 vCPU x86-64 | Immagini `linux/amd64`: quelle costruite su un PC Windows x64 vanno bene così. |
| RAM | 8 GB | Bastano per app, Postgres, RabbitMQ e proxy. Non conviene costruire lì l'immagine (vedi sotto). |
| Disco | 100 GB SSD (+200 GB a €1,50/mese) | Sufficiente; va limitata la crescita dei log Docker. |
| Rete | 200 Mbit/s, traffico illimitato (fair use), 1 IPv4 | Serve un dominio con un record A verso l'IP per il certificato HTTPS. |
| Sistema | Ubuntu 24.04 | Docker Engine e plugin compose dal repository ufficiale di Docker. |
| Regione | Unione Europea | Va bene per il GDPR (dati di presenza dei dipendenti). |
| Backup | 1 snapshot incluso; backup automatico giornaliero a €1,65/mese | Lo snapshot prima di ogni rilascio, più il dump del DB (vedi sotto). |
| Contratto | 24 mesi, circa €6/mese | |

### 9.1 Architettura sul VPS

Tutto su una macchina, con **un'istanza sola** dell'app: per questo i punti 10, 11 e 12 non servono.

```
Internet ──443/80──> caddy ──8080──> nvxapp ──> postgres
                     (HTTPS)                 └──> rabbitmq
```

| Servizio | Immagine | Porte pubblicate | Volume |
|---|---|---|---|
| `caddy` | `caddy:2` | 80 e 443 | `caddy_data` (certificati) |
| `nvxapp` | immagine dell'app (punto 6) | nessuna, solo rete interna | `nvxapp_logs` su `/app/Logs` |
| `postgres` | `postgres:15` (stessa versione dello sviluppo) | nessuna | `pg_data` |
| `rabbitmq` | `rabbitmq:3-management` | nessuna | `rabbit_data` |

- **Reverse proxy: Caddy**. Ottiene e rinnova da solo il certificato Let's Encrypt e il `Caddyfile` è di due righe (`dominio { reverse_proxy nvxapp:8080 }`). WebSocket di SignalR inclusi senza configurazione. In alternativa nginx con certbot, più lavoro.
- **App**: `ASPNETCORE_ENVIRONMENT=Production`, `RunTime__Port=8080`, `RunTime__UseHttps=false`, `NVXAPP_ReverseProxy__Enabled=true` (sicuro perché la porta 8080 la raggiunge solo Caddy), `NVXAPP_App__PublicUrl=https://<dominio>`. Segreti dal `.env` del server, copia di `DockerVps.env` (sezione 8), con `Host=postgres` e host RabbitMQ `rabbitmq`.
- **Postgres e RabbitMQ senza porte pubblicate**: la console di RabbitMQ e Postgres si raggiungono, quando serve, con un tunnel SSH (`ssh -L 5432:...`) o pubblicando la porta solo su `127.0.0.1`.
- **Attenzione: le porte pubblicate da Docker scavalcano `ufw`**. Per questo non si pubblica nulla tranne 80 e 443 di Caddy.
- **Riavvio automatico**: `restart: unless-stopped` su tutti i servizi, così ripartono dopo un riavvio del VPS.
- **Log di Docker**: `logging: { driver: json-file, options: { max-size: "10m", max-file: "5" } }`, altrimenti riempiono il disco.
- **Memoria**: `shared_buffers=1GB` per Postgres e un limite di memoria (`mem_limit`) per app e RabbitMQ, così un servizio non toglie memoria agli altri.
- **Email**: Brevo usa la porta 587, che Contabo non blocca (di solito blocca solo la 25).

### 9.2 Come arriva l'immagine sul VPS

Il build Angular più `dotnet publish` richiede parecchia RAM e tempo: meglio **costruire l'immagine sul PC di sviluppo** (Docker Desktop) e spostarla.

| Modo | Come | Nota |
|---|---|---|
| Senza registry (consigliato per iniziare) | `docker build -t nvxapp:<tag> .` nella cartella della solution, poi `docker save nvxapp:<tag> \| gzip` → `scp` → `docker load` sul VPS | Nessun servizio esterno; tag con data o versione per il rollback. |
| Registry privato | GitHub Container Registry o Docker Hub privato: `docker push` dal PC, `docker compose pull` sul VPS | Più comodo quando i rilasci sono frequenti. |
| Build sul VPS | `git pull` e `docker compose build` | Possibile ma lento, e serve il codice sul server. |

Rilascio: snapshot o dump del DB → carica la nuova immagine → `docker compose up -d` (le migrazioni partono all'avvio) → controllo del log. Rollback: si riavvia il tag precedente (e si ripristina il dump se la migrazione ha cambiato lo schema).

### 9.3 Preparazione del VPS (una volta)

1. Accesso SSH con chiave, login root con password disattivato, utente non root nel gruppo `docker`.
2. `ufw`: aperte solo 22, 80 e 443. Più `fail2ban` e `unattended-upgrades` per gli aggiornamenti di sicurezza.
3. Docker Engine e plugin compose dal repository ufficiale di Docker (non il pacchetto `docker.io` di Ubuntu).
4. Fuso del sistema `Europe/Rome` (`timedatectl`), comodo per leggere i log. L'app non ne dipende: usa `RunTime:TimeZone` (punto 2).
5. Cartella `/opt/nvxapp` con `docker-compose.yml`, `docker-compose.prod.yml`, `docker/Caddyfile` e `.env` (copia di `DockerVps.env`, `chmod 600`, mai in git) con `COMPOSE_FILE=docker-compose.yml:docker-compose.prod.yml`.
6. Dominio: record A verso l'IP del VPS **prima** del primo avvio di Caddy, altrimenti il certificato non viene emesso.

### 9.4 Backup

- **Database**: `pg_dump` giornaliero via cron (`docker compose exec postgres pg_dump ...`) in una cartella del VPS, con rotazione (es. 14 giorni) e **copia fuori dal VPS** (PC, storage esterno). Senza copia esterna, se si perde il VPS si perdono anche i backup.
- **Chiavi DataProtection**: stanno nel DB (punto 4), quindi sono già nel dump.
- **File `DockerVps.env`** (il `.env` del server): copia sicura fuori dal server (gestore di password), non in git.
- **Snapshot Contabo**: prima di ogni rilascio (ne è incluso 1). Il backup automatico a €1,65/mese è un'ulteriore sicurezza, ma non sostituisce il dump: ripristina tutta la macchina, non il singolo database.
- Prova di ripristino del dump su un database di test almeno una volta, prima di andare in produzione.

### 9.5 Da decidere

- Nome di dominio dell'app.
- Registry o `docker save`/`scp` per portare le immagini.
- Dove copiare i backup fuori dal VPS.
- Se attivare il backup automatico Contabo.
- Postgres e RabbitMQ di produzione con password nuove, diverse da quelle di sviluppo (quelle vecchie sono nella storia di git, punto 7).

## 10. Come far partire l'app con Docker

File nella cartella della solution (`C:\REPO\ANGULAR-2025\Merge\nvxapp`): `Dockerfile`, `.dockerignore`, `docker-compose.yml`, `docker-compose.local.yml`, `docker-compose.prod.yml`, `docker/Caddyfile` (VPS), `docker/Caddyfile.local` (PC), `.env.example`. Tutti i comandi si lanciano da quella cartella (sul server da `/opt/nvxapp`).

**File di ambiente** (copie di `.env.example`, con segreti, esclusi da git):

| File | Dove | Come viene letto |
|---|---|---|
| `DockerLocale.env` | PC, cartella della solution | dal profilo di pubblicazione **DockerLocale**; nei comandi a mano con `docker compose --env-file DockerLocale.env ...` |
| `DockerVps.env` | PC, cartella della solution (si prepara e si conserva qui) | si copia sul VPS come `/opt/nvxapp/.env`: lì `docker compose` lo legge da solo |

Sul PC, un comando `docker compose` senza `--env-file DockerLocale.env` si ferma con "impostare POSTGRES_PASSWORD nel file di ambiente".

### 10.1 Sul PC (Docker Desktop)

1. **Avviare Docker Desktop** e aspettare che sia pronto.
2. **Creare il file `DockerLocale.env`** (solo la prima volta), in PowerShell nella cartella della solution:
   ```powershell
   Copy-Item .env.example DockerLocale.env
   notepad DockerLocale.env
   ```
   Compilare `POSTGRES_PASSWORD`, `RABBITMQ_PASSWORD` e `NVXAPP_JwtParameter__Key` con valori casuali, generabili con:
   ```powershell
   -join ((1..32) | ForEach-Object { '{0:x2}' -f (Get-Random -Maximum 256) })
   ```
   Per l'https sulla rete locale: `LAN_IP` con l'IP del PC (es. `192.168.0.227`, da `ipconfig`) e `APP_PUBLIC_URL=https://192.168.0.227:8443` (indirizzo dei link nelle email). Facoltativi: `NVXAPP_Email__Password` e la chiave AI (senza, email e chat AI non funzionano). `COMPOSE_FILE` è già impostato sul profilo locale.
3. **Avviare** (oppure da Visual Studio, sotto):
   ```powershell
   docker compose --env-file DockerLocale.env up -d --build
   ```
   La prima volta servono circa 5 minuti (scarica le immagini e compila Angular e .NET); le volte successive molto meno.
4. **Aprire** https://192.168.0.227:8443 (da questo PC, dagli altri PC e dai telefoni della rete; prima installare il certificato radice, sotto) oppure http://localhost:8080 (solo da questo PC). Al primo avvio il database è nuovo, con gli utenti iniziali (es. `SuperUser`).
5. **Dopo una modifica al codice**: di nuovo `docker compose --env-file DockerLocale.env up -d --build` (ricrea solo l'app; i dati restano), oppure da Visual Studio (sotto).
6. **Fermare**: `docker compose --env-file DockerLocale.env down`. I dati restano nei volumi e al prossimo `up` si ritrovano. Con `down -v` invece **cancella** anche il database.

Database e code di Docker sono separati da quelli di sviluppo usati da Visual Studio: le due cose possono girare insieme.

**Da Visual Studio**: tasto destro su `nvxapp.server` → **Pubblica** → profilo **DockerLocale** → **Pubblica** (`Properties/PublishProfiles/DockerLocale.pubxml`). Compila in Release senza Angular, poi lancia `docker compose --env-file DockerLocale.env up -d --build` nella cartella della solution e apre http://localhost:8080; l'avanzamento di Docker compare nella finestra Output. Circa 2 minuti se è cambiato solo il server, di più se è cambiato il client. Si ferma con un messaggio chiaro se Docker Desktop non è avviato o se manca il file `DockerLocale.env`. Per fermare l'app resta `docker compose --env-file DockerLocale.env down`.

#### HTTPS sulla rete locale

Il profilo locale avvia anche **Caddy**, con `docker/Caddyfile.local`, sulla porta **8443**. È l'unica porta raggiungibile dagli altri PC della rete; app (8080), Postgres e RabbitMQ restano solo su `127.0.0.1`. Caddy passa le richieste all'app (`ReverseProxy:Enabled=true`, come sul VPS) con HTTP/2, compressione e WebSocket di SignalR. Serve perché su `http://<IP>` il browser non considera la pagina sicura e blocca geolocalizzazione, fotocamera, notifiche e installazione come app (PWA).

Per un IP privato nessuna autorità pubblica rilascia certificati: li firma l'**autorità locale di Caddy**, creata al primo avvio e conservata nel volume `caddy_data` (radice valida 10 anni, certificati rinnovati da Caddy). Il certificato vale per `localhost` e per `LAN_IP`. Perché il browser non mostri "connessione non sicura", il **certificato radice** va installato **una volta su ogni dispositivo**:

1. **Esportarlo** (dalla cartella della solution; il file è escluso da git):
   ```powershell
   docker compose --env-file DockerLocale.env cp caddy:/data/caddy/pki/authorities/local/root.crt nvxapp-caddy-root.crt
   ```
2. **Windows** (Chrome, Edge), senza diritti di amministratore:
   ```powershell
   certutil -user -addstore Root nvxapp-caddy-root.crt
   ```
   Confermare l'avviso di sicurezza e riavviare il browser. **Firefox** ha un archivio suo: Impostazioni → Privacy e sicurezza → Certificati → Mostra certificati → Autorità → Importa, spuntando "identificare siti web".
3. **Android**: copiare il file sul telefono → Impostazioni → Sicurezza → Crittografia e credenziali → Installa un certificato → **Certificato CA** (le voci cambiano un po' da marca a marca).
4. **iPhone/iPad**: mandare il file sul dispositivo (es. email o AirDrop) e aprirlo → Impostazioni → Profilo scaricato → Installa; poi Impostazioni → Generali → Info → **Impostazioni attendibilità certificati** → attivare "Caddy Local Authority".

Da sapere:
- **IP fisso**: conviene una prenotazione DHCP sul router per il PC. Se l'IP cambia: aggiornare `LAN_IP` e `APP_PUBLIC_URL` in `DockerLocale.env` e rilanciare `up -d`; Caddy firma da solo il certificato per il nuovo IP con la stessa radice, quindi sui dispositivi non va reinstallato nulla.
- **Firewall di Windows**: con la rete impostata come **Privata** non serve nessuna regola (la regola "Docker Desktop Backend" consente già l'ingresso); con rete Pubblica Windows blocca la porta.
- Se si cancella il volume `caddy_data` (es. `down -v`) nasce una nuova autorità e il certificato radice va reinstallato ovunque.
- Nei log di Caddy l'avviso "Caddyfile input is not formatted" è solo estetico.

### 10.2 Sul VPS (primo rilascio)

Prerequisiti: VPS preparato come nella sezione 9.3 (Docker installato, `ufw`, cartella `/opt/nvxapp`) e, per l'HTTPS, il dominio con il record A verso l'IP del VPS.

1. **Sul PC: costruire e salvare l'immagine** (il tag identifica la versione, serve per il rollback):
   ```powershell
   docker build -t nvxapp:2026-10-27 .
   docker save -o nvxapp-2026-10-27.tar nvxapp:2026-10-27
   ```
   Usare `-o`: in PowerShell 5.1 una pipe o un `>` rovinano i file binari.
2. **Copiare sul VPS** immagine e file del compose:
   ```powershell
   scp nvxapp-2026-10-27.tar docker-compose.yml docker-compose.prod.yml utente@IP_VPS:/opt/nvxapp/
   scp docker/Caddyfile utente@IP_VPS:/opt/nvxapp/docker/
   ```
3. **Sul VPS: caricare l'immagine**:
   ```bash
   cd /opt/nvxapp
   docker load -i nvxapp-2026-10-27.tar
   ```
4. **Il `.env` del server** (la prima volta e quando cambia): compilare sul PC `DockerVps.env`, che ha già il profilo del server e password e chiave JWT nuove, poi copiarlo come `.env` e proteggerlo:
   ```powershell
   scp DockerVps.env utente@IP_VPS:/opt/nvxapp/.env
   ssh utente@IP_VPS chmod 600 /opt/nvxapp/.env
   ```
   In `DockerVps.env` controllare:
   - `COMPOSE_FILE=docker-compose.yml:docker-compose.prod.yml` (separatore `:` di Linux);
   - `NVXAPP_TAG=2026-10-27`;
   - password e chiave JWT **nuove**, diverse da quelle di sviluppo (`openssl rand -hex 32`);
   - chiave Brevo e chiave AI;
   - `APP_DOMAIN=app.miodominio.it`, `APP_PUBLIC_URL=https://app.miodominio.it`, `ACME_EMAIL=` un indirizzo reale.
5. **Avviare e controllare il log**:
   ```bash
   docker compose up -d
   docker compose logs -f nvxapp
   ```
   Il primo avvio crea il database ed esegue le migrazioni; Caddy ottiene il certificato in pochi secondi.
6. **Aprire** `https://app.miodominio.it` e **cambiare subito le password degli utenti iniziali**: quelle predefinite sono note.

**Senza dominio, per una prima prova**: `APP_DOMAIN=:80` e `APP_PUBLIC_URL=http://IP_VPS` (`ACME_EMAIL` va comunque compilato). L'app risponde su `http://IP_VPS`, senza HTTPS: solo per prova, perché password e token viaggiano in chiaro.

### 10.3 Rilasci successivi sul VPS

1. Backup: snapshot Contabo e/o dump del DB (tabella sotto).
2. Sul PC: `docker build` e `docker save` con un nuovo tag, poi `scp` del `.tar` (i file del compose solo se sono cambiati).
3. Sul VPS: `docker load -i ...`, aggiornare `NVXAPP_TAG` nel `.env` (e in `DockerVps.env` sul PC, per tenerli allineati), `docker compose up -d`: ricrea solo l'app, le migrazioni partono all'avvio.
4. **Rollback**: rimettere il tag precedente in `NVXAPP_TAG` e `docker compose up -d`; se la nuova versione aveva cambiato lo schema del DB, ripristinare anche il dump.

### 10.4 Comandi utili

Scritti come sul server. **Sul PC** aggiungere `--env-file DockerLocale.env` subito dopo `docker compose` (es. `docker compose --env-file DockerLocale.env ps`).

| Cosa | Comando |
|---|---|
| Stato dei servizi | `docker compose ps` |
| Log dell'app | `docker compose logs -f nvxapp` |
| Riavviare solo l'app | `docker compose restart nvxapp` |
| Fermare (i dati restano) | `docker compose down` |
| Fermare e **cancellare** i dati | `docker compose down -v` |
| Dump del database | `docker compose exec -T postgres pg_dump -U postgres -Fc -f /tmp/nvxapp.dump nvxapp` e poi `docker compose cp postgres:/tmp/nvxapp.dump .` |
| Ripristino di un dump | `docker compose cp nvxapp.dump postgres:/tmp/` e poi `docker compose exec -T postgres pg_restore -U postgres -d nvxapp --clean --if-exists /tmp/nvxapp.dump` (con l'app ferma: `docker compose stop nvxapp`) |
| Console RabbitMQ (solo PC) | http://localhost:15672, utente `rabbit` |
| Certificato radice di Caddy (solo PC) | `docker compose cp caddy:/data/caddy/pki/authorities/local/root.crt nvxapp-caddy-root.crt` (installazione nella sezione 10.1) |
| Postgres da pgAdmin/Navicat (solo PC) | host `127.0.0.1` (non `localhost`: la porta è solo IPv4), porta `5434`, utente `postgres`, password `POSTGRES_PASSWORD` di `DockerLocale.env` |
