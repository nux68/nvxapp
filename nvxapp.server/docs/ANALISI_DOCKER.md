# Analisi: distribuzione della solution su Docker

> Analisi del 26/10/2026, branch `AttendanceTracking_Dev`. Nessuna modifica al codice: solo valutazione.

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

| # | Problema | Dove | Intervento |
|---|---|---|---|
| 1 | L'URL delle API è scritto nella build del client: `apiUri: 'http://192.168.0.227:8146/api/'` | `nvxapp.client/src/environments/environment.prod.ts` | Usare URL relative (`/api/`, e `/` per SignalR): frontend e API hanno la stessa origine. Una sola build vale per tutti gli ambienti ed è il prerequisito per avere più profili. |
| 2 | Fuso orario e cultura: nei container Linux il default è UTC con cultura invariante. Nel `.service` ci sono circa 28 usi di `DateTime.Now` / timezone. `Program.cs` lascia date e numeri alla cultura della macchina. | `Program.cs`, servizi | `TZ=Europe/Rome` più un'immagine con ICU e `tzdata` (Debian, non Alpine o chiseled), oppure cultura e fuso impostati da configurazione. |
| 3 | Il log su file scrive su `Logs\\nvxapp.log`: su Linux crea un file che si chiama letteralmente `Logs\nvxapp.log`. | `nvxapp.server/Utility/Installers.cs:247` | Usare `Path.Combine("Logs", "nvxapp.log")`, oppure solo log su console o DB. |
| 4 | Le chiavi DataProtection (che firmano i token di conferma email e reset password di Identity) restano nel filesystem del container: si perdono a ogni redeploy e i link già inviati smettono di funzionare. | Configurazione servizi | Salvarle su un volume o su DB (`PersistKeysToFileSystem` / `PersistKeysToDbContext`). |
| 5 | HTTPS dietro reverse proxy: manca `UseForwardedHeaders()`, quindi l'app crede di girare in http e i link generati e i redirect escono sbagliati. | `Program.cs` | Terminare TLS su nginx, Traefik o Caddy, lasciare `RunTime:UseHttps=false` nel container, aggiungere `UseForwardedHeaders()` e impostare `App:PublicUrl`. |

### 3.2 Necessari

| # | Intervento |
|---|---|
| 6 | **Dockerfile multi-stage**: uno stage Node per `ng build`, uno stage SDK per `dotnet publish -c Release -p:AngularBuilt=true` (così salta il target MSBuild) e uno stage `aspnet` di runtime. Il target `CleanWwwrootDebug` usa PowerShell ma scatta solo in Debug: nessun problema. |
| 7 | **Segreti fuori dagli appsettings** (password Postgres, chiave JWT, password RabbitMQ): passarli come variabili d'ambiente (`ConnectionStrings__nvxappDbContext`, `JwtParameter__Key`, …) o come Docker secrets. I file `groq.key`, `openrouter.key` e `brevo.key`, letti da `ContentRootPath`, vanno montati come secret o volume, oppure letti da variabile d'ambiente come fa già `Email__Password`. |
| 8 | **`.dockerignore`**: escludere `node_modules`, `bin`, `obj`, `.angular`, i file `.key` e `Logs`. |
| 9 | **`docker-compose.yml`** con app, postgres (con volume), rabbitmq e, se serve, il reverse proxy. |

### 3.3 Da valutare

| # | Tema | Nota |
|---|---|---|
| 10 | Migrazioni all'avvio | `DatabaseInitializer` migra a ogni avvio. Con una sola istanza va bene; con più repliche c'è il rischio di migrazioni concorrenti, quindi meglio un job separato o un lock. `DbParameter:MultiTenant` si fissa al primo avvio su DB vuoto: ogni profilo deve averlo coerente con il proprio database. |
| 11 | SignalR con più repliche | Servono sticky session o un backplane (Redis). Con una sola istanza non serve nulla. |
| 12 | Cartella export | È in `Path.GetTempPath()/nvxapp_exports`: va bene per file temporanei, ma con più repliche il download può arrivare a un'istanza diversa da quella che ha creato il file. |
| 13 | Sicurezza | Swagger esposto in produzione su `/docs` e CORS che accetta qualsiasi origine (`SetIsOriginAllowed(origin => true)`). Con stessa origine dietro proxy il CORS si può restringere. |

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

| # | Intervento | Beneficio indipendente da Docker | Sforzo | Rischio |
|---|---|---|---|---|
| 1 | URL API relative in `environment.prod.ts` | Oggi IP e porta sono scritti nella build: se cambia server, porta o dominio, o si passa a https, va ricompilato Angular. Con URL relative una build vale ovunque. | 10 min | Basso. Va verificato che nessuno serva Angular separatamente dalle API in produzione. |
| 3 | Percorso dei log con `Path.Combine` | Elimina un bug latente: il percorso oggi funziona solo su Windows. | 5 min | Nullo |
| 4 | Chiavi DataProtection persistenti | Anche su IIS, se l'app pool non carica il profilo utente, le chiavi restano in memoria: a ogni riciclo i link di conferma email e reset password già inviati non funzionano più. | 30 min | Basso |
| 7 | Segreti sovrascrivibili da variabili d'ambiente e tolti da `appsettings.json` (password DB, chiave JWT, password RabbitMQ) | Oggi la chiave JWT e la password RabbitMQ sono nel repository. È un punto del piano sicurezza. | 1–2 h | Medio: chi avvia l'app deve avere le variabili impostate (IIS, Visual Studio). |
| 7b | File `.key` leggibili anche da variabile d'ambiente (`AI__GroqKey`, …), come già `Email__Password` | Gestione dei segreti uniforme. | 30 min | Basso |
| 13 | Swagger disattivabile in produzione da configurazione e CORS ristretto | Hardening: oggi `/docs` è pubblico e il CORS accetta qualsiasi origine. Anche questo è nel piano sicurezza. | 1 h | Basso/medio: va verificata l'app PWA o mobile, se chiama da un'altra origine. |

### 5.2 Utili, da valutare

| # | Intervento | Nota |
|---|---|---|
| 2 | Cultura e fuso orario espliciti (`it-IT` / `Europe/Rome` da configurazione) | Rende l'app indipendente dalle impostazioni del server, cosa preziosa per un'app di rilevazione presenze. La versione minima (cultura da config in `Program.cs`) costa poco. Rivedere i ~28 `DateTime.Now` è un lavoro a parte, da fare solo con test. |
| 5 | `UseForwardedHeaders` | Serve solo con un proxy davanti (nginx, IIS reverse proxy, Cloudflare). Non fa danni. |
| 12 | Cartella export configurabile | Permette di indicare un disco o una cartella dedicata invece di `%TEMP%`. |
| 10 | Flag per disattivare la migrazione automatica all'avvio | Permette di controllare quando si migra in produzione. |

### 5.3 Senza effetti sull'app attuale

- Dockerfile, `.dockerignore`, `docker-compose` con override per test e produzione (punti 6, 8, 9): file in più che non toccano il funzionamento attuale. Utili subito anche solo per avere un ambiente di test con Postgres e RabbitMQ in un comando.

### 5.4 Solo se si scala su più istanze

- Backplane SignalR e migrazioni come job separato (punti 10 e 11 nella forma completa).

## 6. Proposta operativa

1. **Primo blocco** (circa mezza giornata, rischio basso): punti 1, 3, 4, 7b, 13 e versione minima del 2.
2. **Secondo blocco**: punto 7 (segreti in variabili d'ambiente), da fare separatamente perché cambia come si avvia l'app su IIS e in Visual Studio.
3. **Terzo blocco**: Dockerfile, `.dockerignore`, compose con override per test e produzione, verifica dell'avvio su Linux.

Seguendo il flusso dei branch, gli interventi infrastrutturali (3, 4, 7, 13) vanno fatti su **Infrastructure_Dev** e poi mergiati verso AttendanceTracking.

## 7. Stato di avanzamento

| # | Punto | Stato | Data | Branch | Note |
|---|---|---|---|---|---|
| 1 | URL API relative nel client | ✅ Risolto | 27/10/2026 | `Infrastructure_Dev`, mergiato fino ad `AttendanceTracking` | In `nvxapp.client/src/environments/environment.prod.ts`: `apiUri: '/api/'` e `signalrUri: '/'`. Tutti i servizi concatenano il percorso a queste basi, così chiamate HTTP, download (`page-toolbar`) e hub SignalR (`/chathub`) usano l'origine della pagina. `environment.ts` (sviluppo con `ng serve` su `https://localhost:7146`) è invariato. Capacitor è presente solo come configurazione del template Ionic, senza piattaforme native: se un giorno si crea l'app nativa servirà di nuovo un indirizzo assoluto. Verificato con build Angular di produzione: nel bundle `apiUri` vale `/api/` e l'IP non compare più. |
| 2 | Cultura e fuso orario | Da fare | | | |
| 3 | Percorso log con `Path.Combine` | ✅ Risolto | 27/10/2026 | `Infrastructure_Dev`, mergiato fino ad `AttendanceTracking` | In `nvxapp.server/Utility/Installers.cs` (`InstallLog`) il file di log è `Path.Combine(builder.Environment.ContentRootPath, "Logs", "nvxapp.log")` al posto di `"Logs\\nvxapp.log"`: corretto su Windows e Linux e indipendente dalla cartella corrente del processo. Su IIS e in Visual Studio la cartella resta la stessa di prima (`<cartella app>/Logs`). In Docker basta montare un volume su `/app/Logs`. |
| 4 | Chiavi DataProtection persistenti | Da fare | | | |
| 5 | `UseForwardedHeaders` | Da fare | | | |
| 6 | Dockerfile multi-stage | Da fare | | | |
| 7 | Segreti in variabili d'ambiente / file `.key` | Da fare | | | |
| 8 | `.dockerignore` | Da fare | | | |
| 9 | `docker-compose` con override per ambiente | Da fare | | | |
| 10 | Migrazioni all'avvio | Da valutare | | | |
| 11 | SignalR con più repliche | Da valutare | | | |
| 12 | Cartella export configurabile | Da valutare | | | |
| 13 | Swagger in produzione e CORS | Da fare | | | |
