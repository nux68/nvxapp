# Analisi: distribuzione della solution su Docker

> Analisi del 26/10/2026, branch `AttendanceTracking_Dev`. Interventi in corso dal 27/10/2026 su `Infrastructure_Dev`.
>
> **Server di destinazione**: Contabo Cloud VPS Core 4 con Ubuntu 24.04, un'unica istanza dell'app (sezione 9).

**Stato al 27/10/2026: 8 punti su 14 risolti, 3 da fare, 3 non necessari sul VPS.**

| Stato | Punti |
|---|---|
| ✅ Risolti | 1 URL API relative · 2 cultura e fuso (versione minima) · 3 percorso log · 4 chiavi DataProtection · 5 reverse proxy · 7 segreti in variabili d'ambiente · 7b chiavi AI ed email senza file `.key` · 13 Swagger e CORS |
| ⏳ Da fare | 6 Dockerfile · 8 `.dockerignore` · 9 `docker-compose` con profili per ambiente |
| ➖ Non necessari sul VPS | 10 migrazioni all'avvio · 11 SignalR con più repliche · 12 cartella export: servono solo con più istanze dell'app |

Legenda: ✅ risolto · ⏳ da fare · ➖ non necessario con un'istanza sola (da riprendere se si scala) · 🔍 da valutare. Dettagli di ogni intervento nella sezione 7, segreti nella sezione 8, server di destinazione nella sezione 9.

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
| 6 | ⏳ Da fare | **Dockerfile multi-stage**: uno stage Node per `ng build`, uno stage SDK per `dotnet publish -c Release -p:AngularBuilt=true` (così salta il target MSBuild) e uno stage `aspnet` di runtime. Il target `CleanWwwrootDebug` usa PowerShell ma scatta solo in Debug: nessun problema. |
| 7 | ✅ | **Segreti fuori dagli appsettings** (password Postgres, chiave JWT, password RabbitMQ): passarli come variabili d'ambiente (`ConnectionStrings__nvxappDbContext`, `JwtParameter__Key`, …) o come Docker secrets. I file `groq.key`, `openrouter.key` e `brevo.key`, letti da `ContentRootPath`, vanno montati come secret o volume, oppure letti da variabile d'ambiente come fa già `Email__Password`. |
| 8 | ⏳ Da fare | **`.dockerignore`**: escludere `node_modules`, `bin`, `obj`, `.angular`, i file `.key` e `Logs`. |
| 9 | ⏳ Da fare | **`docker-compose.yml`** con app, postgres (con volume), rabbitmq e, se serve, il reverse proxy. |

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
| File `.env` | `.env.test` e `.env.prod` con porta, tag dell'immagine e URL pubblico. |
| Visual Studio | Profili `.pubxml` di tipo *Container Registry* accanto a `IISProfile`, oppure `dotnet publish /t:PublishContainer` con `ContainerImageTag` diversi. Il profilo IIS attuale può restare. |
| Angular | Le configurazioni di `angular.json` (`production`, `development`, `ci`) restano. Con le URL relative del punto 1 non servono build diverse per ambiente. |

### Schema di Dockerfile di partenza

```dockerfile
FROM node:22 AS web
WORKDIR /src/nvxapp.client
COPY nvxapp.client/package*.json ./
RUN npm ci
COPY nvxapp.client/ ./
RUN npx ng build

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
COPY --from=web /src/nvxapp.client/www nvxapp.server/wwwroot
RUN dotnet publish nvxapp.server/nvxapp.server.csproj -c Release -p:AngularBuilt=true -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0
ENV TZ=Europe/Rome RunTime__Port=8080 RunTime__UseHttps=false
RUN apt-get update && apt-get install -y tzdata && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENTRYPOINT ["dotnet", "nvxapp.server.dll"]
```

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

- ⏳ Dockerfile, `.dockerignore`, `docker-compose` con override per test e produzione (punti 6, 8, 9): file in più che non toccano il funzionamento attuale. Utili subito anche solo per avere un ambiente di test con Postgres e RabbitMQ in un comando.

### 5.4 Solo se si scala su più istanze

- ➖ Backplane SignalR e migrazioni come job separato (punti 10 e 11 nella forma completa). Non servono sul VPS, dove gira un'istanza sola.

## 6. Proposta operativa

1. ✅ **Primo blocco** (circa mezza giornata, rischio basso): punti 1, 3, 4, 7b, 13 e versione minima del 2. Completato, insieme al punto 5.
2. ✅ **Secondo blocco**: punto 7 (segreti in variabili d'ambiente), da fare separatamente perché cambia come si avvia l'app su IIS e in Visual Studio.
3. ⏳ **Terzo blocco**: Dockerfile, `.dockerignore`, compose con override per test e produzione, verifica dell'avvio su Linux.
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
| 6 | Dockerfile multi-stage | Da fare | | | |
| 7 | Segreti in variabili d'ambiente | ✅ Risolto | 27/10/2026 | `Infrastructure_Dev`, mergiato fino ad `AttendanceTracking` | Connessione DB, chiave JWT e password RabbitMQ tolte da `appsettings.json`, `appsettings.Development.json` e `appsettings.Development.DEV1.json`: valori `""` con commento che indica la variabile. In `InstallSettings` le variabili con prefisso `NVXAPP_` (`__` al posto di `:`) sono aggiunte per ultime e vincono sugli appsettings; il prefisso evita che le leggano altre app .NET della macchina. All'avvio `CheckRequiredSecrets` ferma l'app se mancano connessione DB o chiave JWT (almeno 32 byte, requisito di HMAC-SHA256), indicando la variabile. Anche la factory di design-time di `dotnet ef` legge `NVXAPP_ConnectionStrings__nvxappDbContext`. Elenco e modalità d'uso nella sezione 8. Verificato su server di prova: senza variabili l'app si ferma con il messaggio; con le variabili login 200 e API protetta 401 senza token / 200 con token. **I valori tolti restano nella storia di git e su origin: vanno cambiati** (chiave JWT, password DB e RabbitMQ). |
| 7b | Chiavi AI e password SMTP senza file `.key` | ✅ Risolto | 27/10/2026 | `Infrastructure_Dev`, mergiato fino ad `AttendanceTracking` | `ChatAIService` non legge più `groq.key` e `openrouter.key`: le chiavi sono `AI:AI_groq:ApiKey` e `AI:AI_openrouter:ApiKey` (variabili `NVXAPP_AI__AI_groq__ApiKey`, `NVXAPP_AI__AI_openrouter__ApiKey`). È obbligatoria **solo quella del provider in uso** (`AI:UseLLM`): prima mancava un file qualsiasi e la chat si bloccava. `EmailService` non legge più `brevo.key`: password solo da `Email:Password` (`NVXAPP_Email__Password`), con errore esplicito se manca. Rimosse dagli appsettings le chiavi mai lette `OpenRouterKey` e `AI_local:Key` e il segnaposto `TUO_BREVO_KEY`. Verificato: senza la chiave Groq la chat risponde 500 con "impostare la variabile d'ambiente NVXAPP_AI__AI_groq__ApiKey". I file `.key` in `nvxapp.server/` e in `C:\inetpub\wwwroot\nvxapp\` non servono più e si possono eliminare, dopo aver conservato le chiavi altrove. |
| 8 | `.dockerignore` | Da fare | | | |
| 9 | `docker-compose` con override per ambiente | Da fare | | | |
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

**Docker**: le variabili di sistema di Windows non arrivano nei container. Si usa un file `.env` accanto al compose, escluso da git, letto con `env_file`, con gli stessi nomi; nel repository solo un `.env.example` con i segnaposto (punti 6–9). In Docker gli host di DB e RabbitMQ sono i nomi dei servizi del compose, non `localhost`.

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
- **App**: `ASPNETCORE_ENVIRONMENT=Production`, `RunTime__Port=8080`, `RunTime__UseHttps=false`, `NVXAPP_ReverseProxy__Enabled=true` (sicuro perché la porta 8080 la raggiunge solo Caddy), `NVXAPP_App__PublicUrl=https://<dominio>`. Segreti dal file `.env` (sezione 8), con `Host=postgres` e host RabbitMQ `rabbitmq`.
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
| Senza registry (consigliato per iniziare) | `docker save nvxapp:<tag> \| gzip` → `scp` → `docker load` sul VPS | Nessun servizio esterno; tag con data o versione per il rollback. |
| Registry privato | GitHub Container Registry o Docker Hub privato: `docker push` dal PC, `docker compose pull` sul VPS | Più comodo quando i rilasci sono frequenti. |
| Build sul VPS | `git pull` e `docker compose build` | Possibile ma lento, e serve il codice sul server. |

Rilascio: snapshot o dump del DB → carica la nuova immagine → `docker compose up -d` (le migrazioni partono all'avvio) → controllo del log. Rollback: si riavvia il tag precedente (e si ripristina il dump se la migrazione ha cambiato lo schema).

### 9.3 Preparazione del VPS (una volta)

1. Accesso SSH con chiave, login root con password disattivato, utente non root nel gruppo `docker`.
2. `ufw`: aperte solo 22, 80 e 443. Più `fail2ban` e `unattended-upgrades` per gli aggiornamenti di sicurezza.
3. Docker Engine e plugin compose dal repository ufficiale di Docker (non il pacchetto `docker.io` di Ubuntu).
4. Fuso del sistema `Europe/Rome` (`timedatectl`), comodo per leggere i log. L'app non ne dipende: usa `RunTime:TimeZone` (punto 2).
5. Cartella `/opt/nvxapp` con `docker-compose.yml`, `docker-compose.prod.yml`, `Caddyfile` e `.env` (`chmod 600`, mai in git).
6. Dominio: record A verso l'IP del VPS **prima** del primo avvio di Caddy, altrimenti il certificato non viene emesso.

### 9.4 Backup

- **Database**: `pg_dump` giornaliero via cron (`docker compose exec postgres pg_dump ...`) in una cartella del VPS, con rotazione (es. 14 giorni) e **copia fuori dal VPS** (PC, storage esterno). Senza copia esterna, se si perde il VPS si perdono anche i backup.
- **Chiavi DataProtection**: stanno nel DB (punto 4), quindi sono già nel dump.
- **File `.env`**: copia sicura fuori dal server (gestore di password), non in git.
- **Snapshot Contabo**: prima di ogni rilascio (ne è incluso 1). Il backup automatico a €1,65/mese è un'ulteriore sicurezza, ma non sostituisce il dump: ripristina tutta la macchina, non il singolo database.
- Prova di ripristino del dump su un database di test almeno una volta, prima di andare in produzione.

### 9.5 Da decidere

- Nome di dominio dell'app.
- Registry o `docker save`/`scp` per portare le immagini.
- Dove copiare i backup fuori dal VPS.
- Se attivare il backup automatico Contabo.
- Postgres e RabbitMQ di produzione con password nuove, diverse da quelle di sviluppo (quelle vecchie sono nella storia di git, punto 7).
