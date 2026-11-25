# Analisi – Sicurezza difensiva di nvxapp (revisione statica di tutti i moduli)

Branch: **Infrastructure_Dev** (worktree `wt_infra_sicurezza`, ramo `Infrastructure_Dev_sicurezza`). Le correzioni di infrastruttura si fanno qui; quelle dei moduli sul rispettivo ramo `_Dev` (§7).
Documento di sintesi di tre revisioni parallele (accessi e dati, server/file/integrazioni, client/browser/mobile), scritto il 25/11/2026. **Nessuna modifica al codice.** Collegati: `piano-security.md` (radice), `ANALISI_INFRASTRUCTURE_APPLICATIVO.md` (codici `FB-SEC`), `ANALISI_ATTENDANCETRACKING_APPLICATIVO.md` (codici `ATT-SEC`, ramo AttendanceTracking).

Legenda dei marcatori: **[V]** verificato nel codice (file e metodo citati) · **[S]** sospetto, letto ma non provato · **[?]** da verificare (serve esecuzione, database, configurazione reale dell'ambiente).
Chi può sfruttarlo: **anonimo** (da Internet, senza account) · **autenticato** (qualunque utente) · **ruolo X** (serve quel ruolo) · **locale** (chi ha accesso al PC o al repository).
Stime: **S** fino a mezza giornata · **M** da una a tre giornate · **G** oltre tre giornate. Rischio di rottura: nullo / basso / medio / alto.

Regola di scrittura: il documento sta in un repository. Non contiene password, chiavi, token o dati personali, nemmeno la password predefinita (si dice "password predefinita nota nel codice"), e non descrive procedure di attacco: solo difetto, impatto, riferimenti e correzione. Dei riferimenti si cita file e metodo, mai la riga.

---

## 1. Scopo, perimetro, metodo e limiti

**Scopo.** Avere un quadro unico e verificabile dello stato di sicurezza dell'applicativo completo, un elenco di correzioni ordinato per rischio e indipendente a blocchi piccoli, e le decisioni che servono per iniziare.

**Perimetro.**
- Server: `nvxapp.server` (controller, `Program.cs`, `Installers.cs`), `nvxapp.server.service` (servizi, hub, chat AI, MCP), `nvxapp.server.data` (entità, migration, tenancy).
- Client: `nvxapp.client` (Angular/Ionic) e la preparazione a Capacitor.
- Distribuzione: `Dockerfile`, `docker-compose*.yml`, `docker/Caddyfile`.
- Dipendenze NuGet e npm.
- Tutti i sette applicativi, letti sul ramo **AllFunction** al commit di riferimento **13e0386a** ("Merge branch 'AllFunction_Dev' into AllFunction"). I rilievi sui moduli valgono per il rispettivo ramo, che eredita le funzioni base.

**Metodo.** Revisione statica, in sola lettura. Tre aree lette in parallelo, poi unite e **ricontrollate nel codice** dal redattore per ogni rilievo CRITICO e ALTO e per gli elementi marcati "già noto / nuovo". Per ogni rilievo si è cercato attivamente ciò che lo smentisce (filtri globali, policy, middleware, controlli nei servizi): dove non c'era, lo si scrive.

**Limiti (cosa non è stato fatto).**
- Nessuna esecuzione: né build, né server, né database, né rete, né prove dal vivo. Tutto ciò che dipende dal comportamento a runtime è marcato [S] o [?].
- Non aperti, per istruzione: file `.env`, `*.segreti.ps1`, log, configurazione locale di git. Non so quali valori sono realmente in uso negli ambienti (chiave JWT, password, `App:PublicUrl`, `Payments:Debug` sul VPS).
- Non letti uno per uno: i restanti servizi `Par_*`/`Az_*`/`Dip_*` delle Presenze (letti a campione, stesso schema atteso), gli handler di chat e MCP per singolo ruolo, i singoli servizi di CRM, HR, Documentale e Assets oltre al campione e ai punti di upload/download.
- Non verificati: firewall, SSH, aggiornamenti, backup e log del VPS; visibilità del repository remoto; comportamento della libreria ClosedXML con testi che iniziano per "=".
- Non analizzato a parte: il `package.json` di `nvxapp.server` (rimanda al client con `file:`).

---

## 2. Sintesi

### 2.1 Giudizio

La parte **nuova** dell'applicativo (HR, Compliance, CRM, Documentale, Assets, Licenze, Pagamenti, Cancellazione, Storage, Ticket, Notifiche, MCP, restrizioni di menu, spostamento azienda) è costruita bene: contesto con ruolo e azienda presi dal token, filtro per azienda, lato studio con verifica del perimetro. L'isolamento tra aziende in modalità multi-azienda è garantito dallo schema del database, non dai servizi.

La parte **vecchia** no: il gruppo `Account` (utenti, aziende, studi, centri: 27 azioni) e quasi tutto il modulo Presenze non controllano né il ruolo né il perimetro del chiamante. In più i database nuovi nascono con utenti iniziali con password predefinita nota nel codice, compreso il SuperUser, e non c'è nessuna protezione contro i tentativi ripetuti di accesso.

**Se l'applicazione fosse esposta a Internet oggi, non sarebbe sicura**: un anonimo può arrivare a controllare tutti i tenant per due strade indipendenti (utenti iniziali, §4 SIC-03; registrazione pubblica più creazione utenti con ruolo arbitrario, SIC-01). Dal codice non si ricava se esistono installazioni raggiungibili da fuori con dati reali: se esistono è un'emergenza (§3), altrimenti va chiusa prima di qualunque installazione esposta (decisione SIC-D1).

### 2.2 Numeri (dopo unione dei doppioni e rivalutazione)

| Gravità | N. | Codici |
|---|---|---|
| CRITICA | 3 | SIC-01, SIC-02, SIC-03 |
| ALTA | 4 | SIC-04, SIC-05, SIC-06, SIC-07 |
| MEDIA | 20 | SIC-08 … SIC-11, SIC-13 … SIC-19, SIC-21 … SIC-27, SIC-34, SIC-35 |
| BASSA | 11 | SIC-12, SIC-20, SIC-28 … SIC-33, SIC-36 … SIC-38 |
| INFO | 2 | SIC-39, SIC-40 |
| Totale | 40 | |

Le fonti avevano 19 rilievi numerati (area accessi, di cui 1 informativo), 26 (server, di cui 2 informativi) e 16 (client, di cui 4 informativi): 61 voci, 40 dopo l'unione dei doppioni.

### 2.3 Rilievi critici e alti

| Codice | Gravità | Titolo | Chi | Origine | Stima | Rischio rottura |
|---|---|---|---|---|---|---|
| SIC-01 | CRITICA | Creazione di utenti con ruolo arbitrario da parte di qualunque utente (anche partendo dalla registrazione pubblica) | anonimo (via registrazione) / autenticato | SA-01 | M | medio |
| SIC-02 | CRITICA | Lettura e modifica per id di utenti, aziende, studi e centri di altri tenant | autenticato | SA-02, SC-13 | G | medio |
| SIC-03 | CRITICA | Utenti iniziali con password predefinita su ogni database nuovo, SuperUser compreso | anonimo | SB-01 | M | alto per dev/test |
| SIC-04 | ALTA | Presenze: nessun controllo di ruolo e proprietà sui dati dei dipendenti | autenticato (stessa azienda) | SA-05 | G | medio-alto |
| SIC-05 | ALTA | "Disabilitato" non blocca l'accesso | utente disabilitato o eliminato | SA-06 | M | medio |
| SIC-06 | ALTA | Nessun limite ai tentativi su login, cambio password, recupero, registrazione | anonimo | SA-04, SB-03 | M | basso-medio |
| SIC-07 | ALTA | Chat AI chiamabile senza autenticazione | anonimo | SA-03, SB-02 | S+M | nullo |

### 2.4 Rivalutazioni rispetto alle fonti (motivate)

- **Token e logout** (SC-01 era MEDIA, SA-06 era ALTA): la durata corretta è **20 minuti per emissione** (`UtilToken` usa `AddSeconds` con il valore di configurazione 1200), non 20 ore come scritto in SC-01. Il token si rinnova a ogni risposta, quindi la sessione attiva non scade mai. Ho separato il difetto in due: "disabilitato non blocca" (SIC-05, ALTA: il login funziona normalmente per chi dovrebbe essere escluso) e "token non revocabile e sessione senza limite assoluto" (SIC-13, MEDIA: un token rubato vale 20 minuti dall'ultima richiesta, non 20 ore).
- **Token in console** (SC-03 era MEDIA per le "20 ore"): con 20 minuti passa a **BASSA** (SIC-36); torna MEDIA se si pubblica l'app mobile (registro del WebView).
- **Pagamenti in modalità debug** (SA-14 e SB-08 erano MEDIA/MEDIA con ALTA al go-live): **MEDIA** oggi, blocco obbligatorio prima della prima vendita (SIC-17).
- **Hub SignalR** (SA-08 e SB-04 MEDIA, SC-02 MEDIA sul lato client): una sola voce MEDIA lato server (SIC-14); il lato client è in SIC-34.
- **Seed degli utenti** (SB-01 CRITICA, assente nell'area accessi): confermata CRITICA, verificata dal redattore (§4, SIC-03).
- **Segreti nella storia git** (SB-05 MEDIA, "ALTA se riusati"): resta MEDIA; sale se il repository è o diventa pubblico o se le chiavi storiche sono in uso (SIC-18).
- **Isolamento in modalità singola** (SA-07 MEDIA): MEDIA solo se si installa in modalità singola; in multi-azienda (predefinita) BASSA per quella voce.

### 2.5 Cosa è già fatto bene (da non rompere)

- **JWT**: emittente, destinatario, scadenza e firma validati, nessuna tolleranza sull'orologio, algoritmo HS256 (nessun `none`), avvio rifiutato con chiave sotto i 32 caratteri o senza connessione al database (`CheckRequiredSecrets`); segreti fuori dai file tracciati (variabili `NVXAPP_*`).
- **Isolamento tra aziende**: la claim `company` del token firmato decide lo schema `tenant_<id>_<app>`; `search_path` impostato per connessione con nome schema validato e quotato; il client non sceglie mai lo schema; i job in background catturano l'azienda del token.
- **SQL**: nessuna concatenazione di input utente (`FromSql*`/`ExecuteSql*` solo in provisioning con nomi validati, in `CompanyStorageRepository` e in `Ast_PrenotazioneRepository` con parametri).
- **"Lavora come"** (`PuoLavorareCome`): valuta l'utente del login, solo ruolo più basso e solo nel ramo; rifiuti registrati con Warning. Ha sostituito la Fase 2 del piano.
- **Recupero password**: risposta uguale per utente noto e ignoto, token a 24 ore, chiavi su database. **Registrazione**: token a 256 bit salvato come hash, scadenza 24 ore, password solo come hash, utenti creati alla conferma.
- **MCP**: token personale a 256 bit salvato come hash, scadenza, limiti al minuto e al giorno, conferma in due passi firmata, registro delle chiamate, impersonificazione a tempo, spento per azienda finché l'amministratore non lo attiva.
- **Client**: token solo in memoria (mai in storage del browser), nessun `bypassSecurityTrust*`, nessun `eval`, `[innerHTML]` sempre sanificato, testo della chat con escape prima della formattazione, allegati scaricati come file e mai navigati, URL relativi e nessun segreto in `environment.prod.ts`, nessuna source map in produzione, nessun service worker.
- **Server**: errori tecnici mascherati verso il client, `LogSanitizer` per i campi segreti (`FB-SEC.4` fatto), Swagger e Hangfire spenti, CORS chiuso (stessa origine), formule con Jint a risorse limitate, XXE non sfruttabile, file con nome casuale e controllo del percorso, crittografia con `RandomNumberGenerator` e confronti a tempo costante, container applicativo non root, Postgres e RabbitMQ senza porte pubbliche in produzione, nessuna vulnerabilità NuGet nota.
- **Kiosk**: PIN come HMAC, QR firmato e a uso unico, token del dispositivo confrontato a tempo costante.

---

## 3. Cosa fare subito se l'app è esposta a Internet

Misure rapide, in ordine, **senza codice dove possibile**. Servono solo se esiste (o esisterà a breve) un'installazione raggiungibile da fuori.

1. **Cambiare le password degli utenti iniziali** su ogni database già creato (SuperUser per primo, poi tutti gli amministratori di centro, studio e azienda e le utenze DEMO), o ricreare il database dopo SIC-03. Finché non è fatto, nessun'altra misura basta. (SIC-03)
2. **Spegnere la registrazione pubblica**: non c'è un interruttore dedicato, ma la registrazione risponde "non disponibile" se manca la configurazione dell'invio email o se mancano centro e studio DEMO. Lasciare `Email:Host` vuoto sull'istanza esposta, oppure togliere dal reverse proxy il percorso `Registration/` finché SIC-01 non è chiuso. (SIC-01, SIC-15)
3. **Bloccare nel reverse proxy** i percorsi che non servono dall'esterno: l'invio dei messaggi della chat AI, `Parameter/Roles`, i controller dimostrativi (`MyMokeLongJob`, `WeatherForecast`) e `/chathub` se non si usa la comunicazione in tempo reale. In Caddy è una regola di risposta 403 per percorso. (SIC-07, SIC-09, SIC-14, SIC-33)
4. **Limitare la frequenza** delle richieste ai percorsi di login, cambio password, recupero password e registrazione direttamente in Caddy (plugin di rate limit) o sul firewall, in attesa del limitatore nativo. (SIC-06)
5. **Verificare le variabili dell'ambiente**: `APP_PUBLIC_URL` valorizzato (il compose di produzione lo impone, gli altri no); `NVXAPP_Payments__Debug` impostato a falso; `AI:UseLLM` su un provider scelto consapevolmente; `Mcp` spento se non serve. (SIC-16, SIC-17, SIC-25, SIC-26)
6. **Ruotare** chiave JWT, password PostgreSQL e RabbitMQ in ogni ambiente che abbia mai usato i valori di sviluppo storici (la chiave JWT vecchia compare nella storia git fino al 27/10/2026). Ruotare la chiave invalida tutte le sessioni: va bene. (SIC-18)
7. **Rigenerare il token personale** incorporato nell'URL del remote git locale e rimetterlo senza credenziali (Git Credential Manager o chiave SSH). Operazione dell'utente, nessun codice. (SIC-19)
8. **Header di sicurezza e HSTS** in Caddy: sono poche righe di configurazione, senza codice (la CSP prima in sola segnalazione). (SIC-21)
9. **Chiudere l'accesso diretto alla porta 8080** (già non pubblicata in produzione) e, se si espone il compose locale, mantenerla su 127.0.0.1. (SIC-32)
10. **Cifrare disco e backup** del VPS prima di caricare dati reali. (SIC-24)

Non bastano: queste misure riducono la superficie ma **non chiudono** SIC-01, SIC-02, SIC-04 (utente già autenticato di una qualunque azienda). Quelle richiedono codice (§7, blocchi B1-B4).

---

## 4. Rilievi per area

Per ogni rilievo: gravità, chi lo sfrutta, impatto, riferimenti, correzione, stima, rischio di rottura, rami, stato. "Origine" rimanda alle relazioni di partenza (SA = accessi, SB = server, SC = client), "Piano" alle fasi di `piano-security.md`, "Doc" ai codici `FB-SEC`/`ATT-SEC`.

### 4.1 Accessi, ruoli e perimetro (server)

#### SIC-01 – CRITICA – Creazione di utenti con ruolo arbitrario, anche partendo dalla registrazione pubblica
- **Origine**: SA-01. **Piano**: Fase 3. **Doc**: `FB-SEC.8`. Il documento applicativo (§4.3) lo presenta come mitigato da `UserLoad`: **non lo è**.
- **Chi**: qualunque utente autenticato, anche con ruolo `User`, di qualunque azienda. **Anonimo** per catena (vedi sotto, verificata).
- **Difetto**: i metodi di creazione utente di `AccountService` (`UserPut`, `UserCompanyPut` ramo creazione, `UserDealerPut`, `UserFinancialAdvisorPut`) assegnano il ruolo ricevuto dal client (`RoleId` oppure `Roles`) senza leggere il ruolo del chiamante e senza un elenco di ruoli ammessi; la password può essere scelta dal chiamante o, se manca, è la password predefinita nota nel codice. Il controller `Account.cs` ha solo `[Authorize]` senza ruolo. L'unico controllo esistente è `IsSuperUserAsync` in `DealerPut`, per la sola percentuale della piattaforma. `UserPut` non lega l'utente a nessun nodo (centro, studio, azienda).
- **Impatto**: un utente creato con un ruolo alto (SuperUser, Admin, PowerAdmin) può fare login e, con la regola "solo chi sta sotto" di `UserLoad`, lavorare come qualunque utente di livello inferiore di qualunque tenant: lettura e scrittura di tutti i dati, cedolini, pagamenti, cancellazione fisica di aziende. Gli id dei ruoli sono leggibili anche senza autenticazione (SIC-09).
- **Registrazione anonima (verificata nel codice, `RegistrationService`)**: la registrazione pubblica (`CompanyRegister` seguita da `CompanyRegisterConfirm`) è senza login. La "conferma" è il solo clic su un link inviato **a un indirizzo email scelto da chi si registra**, senza verifica di dominio e senza CAPTCHA (SIC-15). Alla conferma si crea una nuova azienda nel ramo DEMO (studio DEMO del centro DEMO) con gli utenti principali CompanyPowerAdmin e CompanyAdmin, oppure uno studio nel centro DEMO con FinancialAdvisorPowerAdmin e FinancialAdvisorAdmin; la password è quella scelta in registrazione (minimo 8 caratteri); la registrazione "non usa EmailConfirmed". Non esiste un interruttore per disattivarla: risponde "non disponibile" solo se l'invio email non è configurato o se mancano centro e studio DEMO. Quindi **un anonimo, con una casella email qualunque, ottiene in pochi minuti un account autenticato con ruolo di amministratore di un'azienda DEMO, e da lì SIC-01 diventa raggiungibile**. Per questo SIC-01 è sfruttabile da anonimo, ma in due passi e non su un'installazione con la registrazione spenta (§3, punto 2).
- **Smentita cercata**: filtri globali, middleware, policy, interceptor, controlli in `ServiceBase`, nei repository o in `UserManager` (`Program.cs`, `Installers.cs`, `NvxControllerBase`): nessuno. `PuoLavorareCome` protegge solo `UserLoad` e MCP, non la creazione.
- **Correzione**: in ogni `Put` di Account leggere il ruolo più alto del chiamante **dal database** (non da `roles[0]`, vedi la nota nei ricordi sugli utenti con più ruoli) e verificare che il ruolo assegnato sia strettamente inferiore e che il nodo sia nel perimetro (stessa funzione di `PuoLavorareCome`); elenco chiuso di ruoli assegnabili per livello (tabella in §7.2); password casuale con cambio al primo accesso invece della password predefinita; `[Authorize(Policy)]` per livello come difesa in profondità. Non toccare `RegistrationService`, `Dip_AnagraficaService` e l'importazione dipendenti: chiamano questi metodi come sottoprocesso (`isSubProcess`) e devono continuare a funzionare.
- **Stima** M · **Rischio** medio (il client usa questi `Put` con i flussi attuali) · **Rami** Infrastructure (Account); Presenze crea utenti dall'anagrafica · **Stato** [V].

#### SIC-02 – CRITICA – Lettura e modifica per id di utenti, aziende, studi e centri di altri tenant
- **Origine**: SA-02, SC-13 (lato client: id da `history.state` e query string, non difendibile dal client). **Piano**: Fasi 5 e 6. **Doc**: `FB-SEC.9`.
- **Chi**: qualunque utente autenticato.
- **Difetto**: `UserCompanyGet/Put`, `UserGet`, `CompanyGet/Put`, `FinancialAdvisorGet/Put`, `DealerGet/Put`, `UserDealerPut`, `UserFinancialAdvisorPut` cercano per `FindByIdAsync(id)` senza verificare che il nodo sia nel perimetro del token. Queste entità stanno nello schema `public`, condiviso da tutti i tenant: lo schema non le protegge. `DealerPut` senza id crea un nuovo centro con due amministratori con password scelta dal chiamante. Gli elenchi (`DealerList`, `FinancialAdvisorList`, `CompanyList`, `UserList`) non filtrano per ruolo e per ramo del chiamante: ad esempio `DealerList` restituisce tutti i centri con gli id degli utenti principali; un utente di azienda vede le aziende dello studio con i loro amministratori.
- **Impatto**: cambio dell'email di un utente altrui (poi recupero password = presa dell'account), aggiunta e rimozione di ruoli, disabilitazione di utenti e aziende, lettura di email e ruoli altrui, creazione di centri.
- **Smentita cercata**: filtro per azienda nel repository (`FindById` non ne ha), global query filter sul contesto `public` (assente), controlli in `ExecuteAction`: nessuno.
- **Correzione**: una sola funzione di perimetro ("nodo gestibile dal chiamante": ruolo più ruolo-più-ramo, la stessa catena di `PuoLavorareCome`) applicata a ogni `Get`/`Put`/`Delete` per id e agli elenchi; ruolo minimo per ogni gruppo di endpoint secondo la matrice del piano. Stima G (27 azioni, da fare a gruppi: B2 in §7) · Rischio medio · Rami Infrastructure · Stato [V]. Sottostimato nei documenti precedenti.

#### SIC-03 – CRITICA – Utenti iniziali con password predefinita su ogni database nuovo
- **Origine**: SB-01 (assente nell'area accessi). **Piano**: la Fase 4 elimina la password predefinita solo in `UserPut`/`CompanyPut`, **non nel seed**. **Doc**: `FB-SEC.10`.
- **Chi**: anonimo (i nomi utente sono nel codice, la password è nota nel codice).
- **Difetto**: `nvxapp.server.data/Migrations/Public/PopolateDB.cs` (`PopolateDB_Save_User`) crea utenti per tutti i ruoli (SuperUser, PowerAdmin, Admin, amministratori di centro, studio e azienda, utente semplice, utenze DEMO) con la password predefinita, email già confermata e blocco per tentativi disattivato. È invocata da `Migrations/Public/20261112054221_InitPublic.cs` (`PopolateDB_InitDB_UP`) e quindi gira all'avvio (`DatabaseInitializer`) su ogni database nuovo, **anche con Docker e sul VPS**. Il seed è incondizionato: nessun test sull'ambiente, nessun passo successivo che cambi o disattivi quegli utenti.
- **Impatto**: con SIC-06 (nessun blocco dei tentativi) e il SuperUser che scende in qualunque azienda con `UserLoad`, presa totale di tutti i tenant da Internet.
- **Smentita cercata**: altre migration, `DatabaseInitializer`, `ANALISI_INFRASTRUCTURE_DOCKER.md` (§7 e §9), compose di produzione: nessun passo di bonifica.
- **Correzione**: seed degli utenti solo con ambiente Development oppure con una variabile esplicita (`Seed:DemoUsers`); in Production un solo SuperUser creato al primo avvio con password casuale o da variabile d'ambiente e cambio obbligatorio al primo accesso; utenze DEMO solo nel ramo DEMO e con password casuale; chi ha già un database in uso cambia le password una volta (§3.1).
- **Stima** M · **Rischio** alto per il primo avvio di sviluppo e collaudo e per gli script di prova che assumono la password predefinita (da aggiornare) · **Rami** Infrastructure (migration), poi tutti · **Stato** [V].

#### SIC-04 – ALTA – Presenze: nessun controllo di ruolo e di proprietà sui dati dei dipendenti
- **Origine**: SA-05. **Piano**: Fasi 3 e 5. **Doc**: `ATT-SEC.1`, `ATT-D1`.
- **Chi**: utente normale (`User`) della stessa azienda.
- **Difetto** [V sui metodi citati]: `TimeSheet_EngineService.Get_Timesheet_AllData` accetta `UsersId` arbitrari (cartellino, assenze, malattie: dati particolari); `Dip_GG_TimbraturaService` (`GetAll`, `Dip_GG_TimbraturaPut`) crea e modifica timbrature di qualunque rapporto di lavoro senza richiesta né approvazione e senza controllo del mese approvato (le `Delete` sono invece controllate dal 25/11, `VerificaCancellazione`); `Dip_PeriodoService.Dip_Periodo_SetStato` approva e riapre i mesi per chiunque (nessun controllo di ruolo all'inizio del metodo); `ContatoriService.Riporto_Upsert` cambia i saldi di ferie e permessi; `Par_CausaliService.Par_CausaliPut` e gli altri `Par_*`/`Az_*` sono modificabili da un `User`, e il modello del client (con `IdAz_Anagrafica`) è mappato per intero sull'entità nel ramo di aggiornamento; il ricalcolo (`Calculate`) è aperto a chiunque.
- **Impatto**: falsificazione dell'orario e dei saldi, lettura di dati sanitari di colleghi (GDPR), approvazione del proprio mese.
- **Già fatto bene** (coincide con `ATT-SEC` §3.3): `Az_SediReparto`, `Dip_Anagrafica`, `Dip_Anomalie`, `NotaSpesa`, `Richiesta` (`VerificaPermesso`), `Report`, `TimbraturaEvoluta` (`VerificaAdmin`), `TimeSheet_Export`, `Turni`, `Formule`.
- **Isolamento tra aziende**: in multi-azienda lo schema impedisce di uscire dall'azienda; in modalità singola no (SIC-10).
- **Correzione**: `RichiediAmministratore()` e `RichiediPowerAdmin()` condivisi (come `AmministratoreAsync` di HR), regola "stesso utente oppure amministra il reparto" (`DipendentiAmministrati`), divieto di scrivere nei mesi approvati; per i sottoprocessi del motore usare `isSubProcess` come in `VerificaCancellazione`. Priorità interne: `Get_Timesheet_AllData`, `Dip_GG_TimbraturaPut`, `Dip_Periodo_SetStato`, `Riporto_Upsert`, `Par_*`, poi il resto (circa 30 servizi).
- **Stima** G · **Rischio** medio-alto (il motore e MCP/chat chiamano questi servizi) · **Rami** AttendanceTracking_Dev (con ricaduta su MCP e chat in Infrastructure) · **Stato** [V].

#### SIC-05 – ALTA – "Disabilitato" non blocca l'accesso
- **Origine**: SA-06 (parte "disabilitato"). **Doc**: parzialmente `ANALISI_INFRASTRUCTURE_CANCELLAZIONE.md` (token di un utente eliminato).
- **Chi**: ex dipendente o utente disabilitato o eliminato; è un caso di uscita dall'azienda, non di attacco esterno.
- **Difetto** [V]: il flag `Disabled` di `UserCompany`, `UserDealer`, `UserFinancialAdvisor` (e dei nodi, con `MainUser`) è solo un attributo di elenco: `AccountService.Login` e `UserLoad` non lo controllano e nessun validatore del JWT lo guarda (`Installers.InstallAuthentication`, nessun `OnTokenValidated`). È controllato solo da MCP (`McpTokenAuthenticator.IdentitaValidaAsync`), dal feed calendario e da pochi elenchi. Anche l'eliminazione dell'utente non invalida un token già emesso.
- **Impatto**: chi è stato "disabilitato" continua ad accedere con la stessa password; un'azienda disabilitata (per esempio per mancato pagamento) resta pienamente utilizzabile.
- **Correzione**: rifiutare in `Login` e `UserLoad` l'utente con tutti i collegamenti disabilitati; `OnTokenValidated` che verifica l'utente esistente e almeno un collegamento attivo (con cache breve) e lo stato di sicurezza dell'utente. Valutare il caso dell'impersonificazione (`UserIdFirstConnection`).
- **Stima** M · **Rischio** medio · **Rami** Infrastructure (tutti i moduli ne beneficiano) · **Stato** [V].

#### SIC-06 – ALTA – Nessun limite ai tentativi su login, cambio password, recupero e registrazione
- **Origine**: SA-04, SB-03. **Piano**: Fase 4. **Doc**: `FB-SEC.5`.
- **Chi**: anonimo.
- **Difetto** [V]: `Login` usa `CheckPasswordSignInAsync(..., false)` (nessun blocco per tentativi falliti); `Installers` imposta lunghezza minima 3 e nessun requisito di complessità e non configura `Lockout`; nessun `AddRateLimiter`; `PasswordRecoveryService.PasswordChange` è **anonimo** e usa `ChangePasswordAsync`, quindi è un secondo oracolo di password senza limite che scavalcherebbe anche un futuro blocco del login (il messaggio, però, è unico: bene); `PasswordRecoveryRequest` e `CompanyRegister` inviano email senza limite per indirizzo di destinazione o per IP (il solo limite è di 3 richieste in attesa per indirizzo); i messaggi di login distinguono "utente non trovato" da "password errata", e `CompanyRegisterUserNamesCheck` è anonimo: i nomi utente sono enumerabili. Il ritardo `DelayAsyncMethod` vale 0.
- **Impatto**: indovinare la password di qualunque utente (soprattutto gli iniziali di SIC-03 e quelli creati con la password predefinita), consumo del relay email (costi, reputazione del mittente).
- **Correzione**: `lockoutOnFailure: true` con opzioni `Lockout` (5 tentativi, 15 minuti) e lo stesso conteggio in `PasswordChange`; limitatore nativo per IP e per nome utente su `Login`, `PasswordChange`, `PasswordRecovery*`, `CompanyRegister*` (partizionare anche per utente: dietro NAT aziendale l'IP è condiviso; richiede `ForwardedHeaders` corretto, SIC-32); messaggio unico; password minima 10; tetto giornaliero di email per destinatario e per IP; ritardo costante sul recupero.
- **Stima** M · **Rischio** basso-medio (utenti iniziali di demo da aggiornare; NAT) · **Rami** Infrastructure · **Stato** [V].

#### SIC-07 – ALTA – Chat AI chiamabile senza autenticazione
- **Origine**: SA-03, SB-02.
- **Chi**: anonimo.
- **Difetto** [V]: in `ChatAIController` l'attributo `[Authorize]` di `SendMessage` è commentato e la classe non ne ha; `ChatAIService.SendMessage` tratta l'utente assente come ruoli "*" invece di rifiutare; non c'è limite di lunghezza del messaggio né di richieste per utente; le sessioni in memoria (`InMemoryChatSessionStore`, singleton) si rimuovono solo alla rilettura.
- **Impatto**: chiamate illimitate al provider LLM a pagamento con le chiavi del proprietario (costo, esaurimento della quota che blocca la chat a tutti), crescita della memoria. Con utente vuoto il catalogo è ridotto a pochi comandi, ma il costo è reale. [S a runtime: dedotto dal codice].
- **Correzione**: ripristinare `[Authorize]` (una riga), rifiutare esplicitamente `CurrentUserId` vuoto, tetto di caratteri per messaggio, limite di richieste per utente e per azienda, pulizia periodica delle sessioni.
- **Stima** S (attributo e rifiuto) + M (limiti) · **Rischio** nullo (il client manda già il token) · **Rami** Infrastructure · **Stato** [V].

#### SIC-08 – MEDIA – Chat AI: i ruoli filtrano solo il catalogo mostrato al modello
- **Origine**: SA-12.
- **Difetto** [V]: `ChatAIService.ExecuteCommandAsync` risolve l'intento con `_commandRegistry.Resolve` senza ricontrollare `Usable4Role`/`Application`; il filtro è in `IntentCatalog.Fuilter4UserRoles` e agisce solo sull'elenco proposto al modello. Un modello che non rispetta lo schema, o un dialogo manipolato, può ottenere un intento di ruolo superiore; i servizi sottostanti non controllano a loro volta (SIC-04). MCP invece ricontrolla a ogni chiamata (`McpToolService`) ed è fatto bene.
- **Correzione**: stesso controllo di MCP in `ExecuteCommandAsync`. **Stima** S · **Rischio** basso · **Rami** Infrastructure · **Stato** [V].

#### SIC-09 – MEDIA – Informazioni a utenti non autorizzati (ruoli, utenti, id)
- **Origine**: SA-09. **Doc**: `FB-SEC.2` per `UserRoles`; il resto è nuovo.
- **Difetto** [V]: `Parameter/Roles` è anonimo (`[Authorize]` commentato in `ParameterController`) e restituisce id e codici di tutti i ruoli (utile a SIC-01); `Account/UserRoles` accetta un `IdAspNetUsers` qualunque senza `PuoLavorareCome`; `Account/UserCheck` dice a chiunque se uno username o un'email esiste; `UserList` restituisce gli id degli amministratori; `CompanyApplicationList` accetta qualunque `IdCompany` (`CompanyApplicationService`).
- **Correzione**: `[Authorize]` su `Parameter/Roles`, perimetro su tutti gli altri. **Stima** S · **Rischio** basso · **Rami** Infrastructure · **Stato** [V].

#### SIC-10 – MEDIA – Isolamento tra aziende in modalità singola affidato ai servizi
- **Origine**: SA-07. **Doc**: `FB-SEC.9`.
- **Difetto** [V sui campioni]: con installazione in modalità singola (decisa al primo avvio, immutabile) tutti i dati stanno in `public`; i servizi Presenze fanno `FindByIdAsync`/`Delete` per id senza filtro `IdAz_Anagrafica` (esempi: `Par_CausaliGet/Delete`, `Dip_GG_TimbraturaPut`) e i modelli in ingresso contengono `IdAz_Anagrafica` (mappato con AutoMapper): un utente di un'azienda legge, modifica e cancella dati di un'altra. I servizi nuovi (HR, CRM, Documentale, Compliance, Assets) filtrano sempre per azienda (verificato a campione).
- **Gravità**: MEDIA solo per installazioni in modalità singola; in multi-azienda (predefinita) BASSA.
- **Correzione**: `IdAz_Anagrafica` mai dal client (sovrascritto dal contesto in ogni `Put`), filtro per azienda nei `Get` e `Delete` (Fase 5 del piano). **Stima** G · **Rischio** alto (migration e dati) · **Rami** AttendanceTracking_Dev · **Stato** [V].

#### SIC-11 – MEDIA – Assegnazione di massa e approvazione della propria richiesta
- **Origine**: SA-13. **Stato** [V in parte, resto S].
- **Difetto**: mappature client→entità senza elenco di campi (`Par_CausaliPut`, `Dip_GG_TimbraturaPut`, `UserCompanyEdit.Roles`); in `VerificaPermesso` delle richieste un amministratore che è anche approvatore può approvare la propria (non c'è il controllo richiedente diverso da approvatore). Nessun campo prezzo o licenza arriva dal client: i prezzi sono dal catalogo del server.
- **Correzione**: DTO senza campi di sistema; regola "richiedente diverso da approvatore" configurabile. **Stima** M · **Rischio** medio · **Rami** Infrastructure e AttendanceTracking_Dev.

#### SIC-12 – BASSA – Kiosk, badge, PIN
- **Origine**: SA-15, SB-20. **Doc**: `ATT-SEC.3`.
- **Difetto** [V]: tentativi del kiosk e dello sblocco in un dizionario statico (non condiviso tra istanze, azzerato al riavvio); badge salvato e confrontato in chiaro; sblocco del terminale con la password di un qualunque CompanyPowerAdmin; PIN breve con 5 errori al minuto; se manca `Timbratura:PinKey` l'HMAC del PIN usa la chiave JWT (SIC-20). Si richiede comunque un amministratore autenticato e il token del kiosk è confrontato a tempo costante.
- **Correzione**: chiave dedicata obbligatoria per il PIN, salatura per dipendente, badge come hash, contatore dei tentativi condivisibile. **Stima** S-M · **Rischio** basso · **Rami** AttendanceTracking_Dev.

### 4.2 Autenticazione, sessione e canali in tempo reale

#### SIC-13 – MEDIA – Token non revocabile e sessione senza limite assoluto
- **Origine**: SA-06 (parte token), SC-01 **corretto**. **Piano**: Fase 1, bug collaterale. **Doc**: `FB-SEC.6`, `FB-D4`.
- **Fatto verificato**: `UtilToken` somma `ExpireMinutes` con `AddSeconds`; con il valore di configurazione 1200 la scadenza è di **20 minuti per emissione**, non 20 ore. Ma `ServiceBase.OkResponse` rilascia un token nuovo a ogni risposta: una sessione in uso **non scade mai** (nessun limite assoluto).
- **Difetto**: nessun endpoint di logout o revoca, nessun `jti`, nessun numero di versione per utente; il logout del client (`auth.service.ts`, `user-navigation.service.ts`, `logout-page.component.ts`) azzera solo variabili in memoria e non chiama il server. Anche la reimpostazione della password non invalida i token già emessi.
- **Impatto**: un token copiato resta valido finché chi lo ha continua a usarlo (rinnovo a catena); dopo "Esci" resta valido per 20 minuti dall'ultima richiesta.
- **Correzione**: limite assoluto di vita della sessione; endpoint di logout che registra la revoca (o numero di versione dell'utente nella claim, controllato in `OnTokenValidated`, insieme a SIC-05); richiamarlo dal client prima di azzerare. **Attenzione**: correggere `AddSeconds` in `AddMinutes` cambia 20 minuti in 20 ore con il valore attuale: decidere prima il valore (SIC-D2). **Stima** M · **Rischio** medio (tocca ogni richiesta autenticata) · **Rami** Infrastructure · **Stato** [V].

#### SIC-14 – MEDIA – Hub SignalR senza autenticazione e broadcast dei nomi utente
- **Origine**: SA-08, SB-04, SC-08 (token in query string). **Doc**: `FB-SEC.3`.
- **Chi**: anonimo.
- **Difetto** [V]: `/chathub` è mappato senza `RequireAuthorization` (`Installers`) e `SignalRHub` non ha `[Authorize]`; `OnConnectedAsync` accetta un utente nullo; `AccountService.UserLoad` invia a **tutti i client** (`Clients.All`) "<nome utente> è entrato" quando `SignalR:UseSignalR` è vero (lo è in `appsettings.json`): chi si connette riceve in tempo reale i nomi utente validi di tutte le aziende (alimenta SIC-06). Il metodo `SendMessage` dell'hub è una prova (Moke) che segnala errori con il testo dell'eccezione. Il token arriva da `access_token` in query string solo su `/chathub` (meccanismo standard del browser); se si attiva un access log del proxy con la query, il token finisce nei log. Un utente autenticato non può scegliere il gruppo di un altro (claim validata): bene.
- **Correzione**: `[Authorize]` sull'hub e `RequireAuthorization()` sulla mappatura, eliminare il broadcast `Clients.All` (o inviarlo al solo utente), eliminare il metodo di prova, rifiutare connessioni senza `useridfirstconnection`; non attivare access log con query e non mettere CDN davanti; valutare un ticket monouso. **Stima** S · **Rischio** basso (il client si collega dopo il login: da provare) · **Rami** Infrastructure · **Stato** [V].

#### SIC-15 – MEDIA – Registrazione pubblica senza anti-automazione
- **Origine**: SA-10. Collegato a SIC-01 e SIC-06.
- **Chi**: anonimo con molti indirizzi email.
- **Difetto** [V]: `CompanyRegister` e `CompanyRegisterConfirm` creano aziende e studi (con schemi PostgreSQL e applicativi attivi) in numero illimitato: disco, tempo di migrazione all'avvio (`MigrateAllAsync` per ogni azienda), costo email. Nessun CAPTCHA, nessun limite per IP, nessun tetto di aziende DEMO, nessuna pulizia delle DEMO inattive. **Fatto bene**: token a 256 bit memorizzato come hash, scadenza 24 ore, password solo come hash, utenti creati alla conferma, link con `PublicUrl`.
- **Correzione**: CAPTCHA o limite per IP, tetto giornaliero di aziende DEMO, pulizia delle DEMO inattive, interruttore di configurazione per spegnere la registrazione (oggi manca). **Stima** M · **Rischio** basso · **Rami** Infrastructure · **Stato** [V].

#### SIC-16 – MEDIA – Link nelle email costruiti con l'intestazione `Origin` se `App:PublicUrl` è vuoto
- **Origine**: SA-11, SB-09. **Stato** [V].
- **Chi**: anonimo.
- **Difetto**: `Site/SiteUrl.cs` (`Base`) usa `App:PublicUrl` e, se vuoto, l'intestazione `Origin` della richiesta. Usato da `PasswordRecoveryService`, `RegistrationService`, `NotificationSender`, `LicenseService`. Chi chiede il recupero password per un utente vittima con un `Origin` a proprio dominio fa recapitare alla vittima un'email vera (mittente legittimo) con il link di reset che punta al sito dell'attaccante, che incassa utente e token al clic. `AllowedHosts` è `*`.
- **Attenuante**: `docker-compose.prod.yml` rende `APP_PUBLIC_URL` obbligatoria; il compose base e `appsettings.json` la lasciano vuota.
- **Correzione**: fuori da Development rifiutare l'avvio se vuota (aggiungere a `CheckRequiredSecrets`); togliere il ripiego su `Origin` (o ammetterlo solo in Development e solo se in `Cors:AllowedOrigins`); `AllowedHosts` uguale al dominio pubblico. **Stima** S · **Rischio** nullo/basso · **Rami** Infrastructure.

#### SIC-17 – MEDIA – Pagamenti: modalità di debug accesa ovunque
- **Origine**: SA-14, SB-08. **Doc**: `ANALISI_INFRASTRUCTURE_LICENZE.md` §7.2.
- **Chi**: ruolo CompanyPowerAdmin (o chiunque si sia registrato).
- **Difetto** [V]: `appsettings.json` ha `Payments:Debug` vero; `License/Payment/PaymentGateway.cs` dichiara il gateway di debug "acceso ovunque, anche in Production", e `LicenseService.LicensePaymentConfirm` tratta lo stato `Debug` come pagato: si attivano licenze gratis. Nessun blocco in Production, solo un avviso nel log. Decisione dichiarata nel codice, valida finché non ci sono pagamenti reali. Il compose non imposta `NVXAPP_Payments__Debug`.
- **Correzione**: predefinito falso; in Production rifiutare l'avvio con `Debug` vero salvo `Payments:AllowDebugInProduction`. **Stima** S · **Rischio** nullo (i collaudi delle licenze impostano la variabile) · **Rami** Infrastructure. **Gravità**: MEDIA oggi, **da chiudere prima della prima vendita**.

#### SIC-18 – MEDIA – Segreti nella storia git
- **Origine**: SB-05. **Piano**: Fase 0 (da completare). **Doc**: `FB-SEC.10`.
- **Difetto** [V]: fino al commit 0ca482a3 (27/10/2026) `nvxapp.server/appsettings.json` conteneva la chiave JWT di sviluppo, la password PostgreSQL e quella RabbitMQ, e la storia è su `origin` in tutti i rami. I file tracciati oggi sono puliti (`CheckRequiredSecrets` rifiuta l'avvio senza chiave di almeno 32 byte); `.gitignore` e `.dockerignore` escludono `*.key`, `*.env`, `*.segreti.ps1`, `*.crt`; mai aggiunti file di chiavi alla storia; mai presenti pattern di chiavi dei provider (i valori storici erano segnaposto).
- **Impatto**: con la chiave JWT storica si firmano token di qualunque utente in un ambiente che la usi ancora. **MEDIA, ALTA se i valori sono riusati in produzione o se il repository è o diventa pubblico.**
- **Correzione**: ruotare comunque chiave JWT, password DB e RabbitMQ in ogni ambiente; decidere se riscrivere la storia solo se il repository è o sarà pubblico (la riscrittura invalida tutti i cloni e i worktree: SIC-D4). **Stima** S (rotazione) / G (riscrittura) · **Rischio** nullo (rotazione).

#### SIC-19 – MEDIA – Credenziale nell'URL del remote git locale
- **Origine**: SB-06. **Stato** [V].
- **Difetto**: l'URL del remote `origin` nella configurazione git **locale** contiene una credenziale personale. Non è nel repository né nella storia (verificato); è però leggibile da ogni processo, backup e strumento dell'utente locale e compare in `git remote -v`. Chi lo legge ha accesso in scrittura al repository, nei limiti degli scope della credenziale.
- **Correzione**: revocare e rigenerare la credenziale, usare Git Credential Manager o una chiave SSH, rimettere l'URL senza credenziali. **Stima** S · **Rischio** nullo. Azione dell'utente, nessun codice.

#### SIC-20 – BASSA – Una sola chiave per firme e PIN
- **Origine**: SA-17, SB-20. La chiave JWT firma anche le conferme MCP, i link di download (HMAC derivato con prefisso, corretto) e, senza `Timbratura:PinKey`, il PIN: compromessa una, cadono tutte. I file `*.key` dei provider stanno nella cartella `nvxapp.server` (non letti): verificare che siano ignorati da git e fuori dall'immagine. **Correzione**: chiavi separate e obbligatorie in produzione. **Stima** S.

### 4.3 Server, file, integrazioni e infrastruttura

#### SIC-21 – MEDIA – Nessuna intestazione di sicurezza, nessun HSTS
- **Origine**: SB-07, SC-04. **Stato** [V].
- **Chi**: sito terzo o attaccante di rete.
- **Difetto**: `Program.cs` non imposta intestazioni né HSTS; `docker/Caddyfile` ha solo compressione e reverse proxy; `index.html` non ha CSP. Conseguenze: nessun contenimento di un eventuale XSS futuro, pagina incorniciabile (clickjacking, anche pagina pagamenti e kiosk), `X-Content-Type-Options` assente, primo accesso in http intercettabile, nessun `Referrer-Policy` (i link di reset e conferma hanno utente e token nella query e le pagine `confirm-email` e `reset-password` non ripuliscono l'URL).
- **Correzione**: in Caddy `Strict-Transport-Security`, `X-Content-Type-Options`, `X-Frame-Options` (o `frame-ancestors 'none'`), `Referrer-Policy`, `Permissions-Policy` (posizione, fotocamera, microfono solo same-origin), `Cache-Control: no-store` sulle API; CSP **prima in sola segnalazione** (Ionic usa stili inline, blob per i download, icone da `svg/`); dopo l'uso dei link, `history.replaceState` per togliere i token dall'URL. Trusted Types solo dopo la CSP. **Stima** S (header) / M (CSP) · **Rischio** medio solo per la CSP · **Rami** Infrastructure/Docker, client.

#### SIC-22 – MEDIA – File: lettura in memoria, bomba di decompressione, nessun controllo del contenuto
- **Origine**: SB-10. **Chi**: utente autenticato con accesso al Documentale (e amministratori per import HR, CRM, dipendenti). **Stato** [V].
- **Difetto**: `Doc_DocumentoService.LeggiFileAsync` copia l'intero upload in un `MemoryStream` e poi `ToArray()` (due copie; limite di richiesta 512 MB; la quota si prenota dopo la lettura); `Helpers/FileText.Estrai` apre `.docx/.odt/.xlsx` senza limite di dimensione decompressa; stessa logica in `DipendentiImportService/ImportFileText.cs`; PdfPig sui cedolini (`HrCedoliniCore.MaxPdf` 100 MB). Content-Type preso dal client; estensioni ammesse senza controllo dei magic bytes né antivirus. **Bene**: XXE non sfruttabile, nome casuale con controllo del percorso, download come allegato, `.svg` e `.html` non ammessi.
- **Correzione**: lettura in streaming su file temporaneo con tetto per file e quota prima della lettura; limite di byte decompressi e di voci prima dell'estrazione del testo; timeout; `nosniff` (SIC-21); controllo dei magic bytes lato server (vale anche per `AllegatoTipiAmmessi` della nota spese, il cui tipo è oggi quello dichiarato dal client); antivirus opzionale per utenti non amministratori. **Stima** M · **Rischio** medio (limiti troppo stretti bloccano file legittimi) · **Rami** DocumentManagement_Dev, HumanResources_Dev, CRM_Dev, AttendanceTracking_Dev (import).

#### SIC-23 – MEDIA – Dati personali nei log e nessuna conservazione
- **Origine**: SB-11. **Doc**: `FB-SEC.4` (fatto per i segreti). **Stato** [V].
- **Difetto**: `ServiceBase.ExecuteAction` scrive a livello Information (predefinito, `LogOnFile` vero) i parametri di ogni chiamata; `LogSanitizer` maschera password, PIN, token, IBAN, badge e simili ma **non** codice fiscale, nomi, email, indirizzi, note di assenza (dati sanitari nei giustificativi). `Logs/nvxapp.log` ruota a 10 MB con 200 file (circa 2 GB) senza scadenza nel volume `nvxapp_logs`; `ChatAIService` registra le frasi non capite con pagina e ruoli; `Console.WriteLine` in `SignalRHub.OnConnectedAsync` e `ErrorResponse`. Restano fuori dalla mascheratura le eccezioni scritte da Serilog (dettaglio dei vincoli di unicità con i valori).
- **Correzione**: livello Warning in produzione (o solo nome del metodo e GUID), estendere `ReservedWords` a codice fiscale, partita IVA, telefono, indirizzo, note; ritenzione massima (per esempio 30 giorni); voce nel registro dei trattamenti. **Stima** S · **Rischio** basso.

#### SIC-24 – MEDIA – Nessuna cifratura a riposo dei dati particolari
- **Origine**: SB-14. **Stato** [V].
- **Difetto**: codice fiscale (`Dip_Anagrafica`, `Az_Cliente`, `CmpGdprEntities`), IBAN dei centri (`Pag_DealerConfig`), cedolini, CU e documenti HR sono in chiaro nel database e nel volume `nvxapp_files` (`FileSystemFileStorage`); dati sanitari nei giustificativi; le chiavi di DataProtection sono in chiaro in `public."DataProtectionKeys"` (`PersistKeysToDbContext` senza `ProtectKeysWith*`): chi legge il database può forgiare token di recupero password. I backup `pg_dump` della documentazione non sono cifrati. **Mitigazioni**: Postgres e RabbitMQ senza porte pubbliche in produzione.
- **Correzione**: cifratura del disco o volume (LUKS sul VPS) e dei backup (age o gpg) [piccola]; protezione delle chiavi di DataProtection con certificato o chiave esterna; cifratura dei file HR e Documentale con chiave per azienda; valutare la cifratura per colonna di codice fiscale e IBAN [grande, rischio medio per la migrazione dei dati esistenti]. **Rami** tutti.

#### SIC-25 – MEDIA – Dati personali verso provider LLM esterni
- **Origine**: SB-12. **Stato** [V il flusso, [?] i "fatti" del riepilogo].
- **Difetto**: `AI:UseLLM` predefinito è un provider extra-UE. Verso il modello vanno le frasi della chat (con nomi di colleghi), testo e immagini dell'import dipendenti (cognome, nome, codice fiscale, email, documenti d'identità fotografati: `DipendentiImportService`, `ImportMaxChars` 12000), testo e allegati dei ticket con la documentazione (`TicketAiResolverService`), i fatti strutturati del riepilogo settimanale (`DigestWriter`: da verificare se contengono nominativi). L'export paghe non invia dati personali (decisione D5). **Mitigazioni**: chiave da variabile d'ambiente, import solo agli amministratori, i campi sono accettati solo se presenti nel testo e l'amministratore rivede prima di inserire.
- **Correzione**: provider UE o locale (Ollama già previsto, `AI_local`) come predefinito per le funzioni con dati personali; avviso e consenso dell'azienda nella pagina di import; voce nell'informativa e nel registro dei trattamenti del modulo Compliance; possibilità di disattivare per azienda; accordo di trattamento col provider. **Stima** M · **Rischio** qualità inferiore con i modelli locali · **Rami** Infrastructure, AttendanceTracking_Dev (import).

#### SIC-26 – MEDIA – MCP livello 3: il codice di conferma torna al modello
- **Origine**: SB-13. **Doc**: `ANALISI_INFRASTRUCTURE_MCP.md` §8. **Stato** [V il codice, [S] lo scenario].
- **Chi**: chiunque possa scrivere testo poi letto dall'assistente di un amministratore (richieste, note CRM, titoli, ticket).
- **Difetto**: la conferma in due passi (`McpToolService`, `ConfirmationParam`) protegge dall'errore del modello ma non da un modello manipolato da un testo ("ignora le istruzioni…") quando il client MCP non chiede il permesso (approvazione automatica): il `confirmationCode` torna al modello, che può ripetere la chiamata.
- **Correzione**: conferma legata all'hash degli argomenti e su un canale diverso dal modello (conferma nell'interfaccia web o notifica da approvare); `Mcp:MaxLevel` a 2 predefinito. **Stima** M · **Rischio** cambia l'esperienza d'uso · **Rami** Infrastructure.

#### SIC-27 – MEDIA – Firma elettronica semplice del Documentale
- **Origine**: SB-15. **Doc**: `ANALISI_DOCUMENTMANAGEMENT_FIRMA_DIGITALE` §2/§4 (l'impersonificazione non tracciata è nuova). **Stato** [V].
- **Difetto**: `Doc_DocumentoService.Doc_DocumentoFirma` è un clic sulla sessione: registra hash, data, IP, user agent e `IdAspNetUsersFirma`, ma nessun secondo fattore, e con "lavora come" il firmatario registrato è l'impersonato senza traccia di chi ha agito (`UserIdFirstConnection` non è salvato); `Doc_Log` è modificabile da chi ha accesso al database (nessun sigillo); i `.p7m` si caricano ma non si verificano.
- **Correzione**: salvare `UserIdFirstConnection`, richiedere nuova autenticazione o OTP per firmare, concatenare gli hash del registro, non qualificare come firma elettronica finché non c'è l'OTP. **Stima** M · **Rischio** basso · **Rami** DocumentManagement_Dev.

#### SIC-28 – BASSA – Il database di produzione è usato come superutente
- **Origine**: SB-16. `docker-compose.yml` usa `postgres` con la stessa password dell'immagine. Nessun SQL con input utente trovato, ma un'iniezione futura avrebbe diritti totali. **Correzione**: ruolo applicativo con soli diritti sugli schemi (`CREATE` sugli schemi tenant per le migrazioni, ruolo separato per l'esecuzione). **Stima** M · **Rischio** medio (le migration multi-tenant creano schemi).

#### SIC-29 – BASSA – Download dei file dei job senza registrazione
- **Origine**: SA-18, SB-17. **Doc**: `FB-SEC.7`. `Account/Download` salta il controllo del proprietario se manca il file `.meta`; la cartella è `nvxapp_exports` nella temporanea di sistema. **Bene**: nome `{guid}.{estensione ammessa}`, `NomeFileJobValido` impedisce i percorsi, scadenza 24 ore (`JobFileCleanupService`); `Account/DownloadLink` è anonimo ma firmato (HMAC, scadenza, confronto a tempo costante). **Correzione**: rifiutare i file senza registrazione. **Stima** S.

#### SIC-30 – BASSA – Pagamento PayPal: la conferma non confronta importo, valuta e riferimento
- **Origine**: SB-18, SC-14 (chiuso lato server). `PayPalPaymentGateway.ConfirmAsync` dichiara pagato su `COMPLETED` l'ordine il cui id è nel database, creato dal server con importo di listino; in `LicensePaymentConfirm` il pagamento è cercato per riferimento ma **l'abbonamento deve appartenere al soggetto chiamante** (`AbbonamentiSoggetto`), quindi il caso temuto in SC-14 (riferimento scelto da un attaccante) è coperto [V]. Manca il confronto di `amount.value`, `currency_code` e `custom_id` con `Lic_Pagamento`. **Correzione**: confrontarli prima di `Completato`. **Stima** S.

#### SIC-31 – BASSA – CSV ed Excel senza protezione dalle formule
- **Origine**: SB-19. **Stato** [S]. `ExportRicettaRenderer.Csv()` non neutralizza i testi che iniziano per `=`, `+`, `-`, `@`; le esportazioni ClosedXML (`Crm_ClienteService`, `Cmp_GdprService`, `PaymentReportPdf`) da verificare con la versione in uso. **Correzione**: anteporre un apice ai testi che iniziano con quei caratteri (non per i file paghe a tracciato fisso). **Stima** S · **Rami** AttendanceTracking_Dev, CRM_Dev, Compliance_Dev.

#### SIC-32 – BASSA – Container, immagini e intestazioni inoltrate
- **Origine**: SB-21, SB-22. **Bene**: app non root, segreti da variabili, `.dockerignore` coerente, Postgres e RabbitMQ non pubblicati, solo Caddy su 80 e 443, limiti di memoria. **Da migliorare**: `caddy:2` e `rabbitmq:3-management` non fissati (la linea 3.x è a fine vita e l'interfaccia di gestione non serve), `postgres:15` senza digest, nessun `no-new-privileges`, `cap_drop`, `read_only` con `tmpfs`, nessun healthcheck dell'app né `pids_limit`; `RabbitMqConnection` ha `guest/guest` come predefinito se manca la configurazione (in Docker la password è obbligatoria). `Installers.InstallForwardedHeaders` accetta le intestazioni `X-Forwarded-*` da chiunque se `ReverseProxy:KnownProxies/KnownNetworks` è vuoto (avviso nel log): oggi la porta 8080 non è pubblicata in produzione, ma IP e schema (usati nella firma del Documentale e nei limiti di SIC-06) sarebbero falsificabili. **Correzione**: indicare la rete del compose nella configurazione `ReverseProxy` del compose di produzione e provare `read_only` (servono `/tmp`, `/app/Logs`, `/app/Files`). **Stima** S.

#### SIC-33 – BASSA – Endpoint dimostrativi e Hangfire
- **Origine**: SA-16, SB-23, SB-24. **Doc**: `FB-SEC.1`, `FB-D2`. `MyMokeLongJob` (`StartJob`, `ExportJob`) e `WeatherForecast/Get` sono anonimi (nessuna policy di fallback): l'anonimo avvia job in background (CPU e memoria). Il metodo `SendMessage` dell'hub (SIC-14) e `MyTableService` sono di prova. Hangfire è spento; se acceso `UseHangfireDashboard()` ha solo il filtro locale predefinito: aggiungere un filtro di autorizzazione per SuperUser prima di abilitarlo. **Correzione**: rimuovere o proteggere (decisione `FB-D2`); **fallback policy** che richieda l'autenticazione con `[AllowAnonymous]` esplicito su `Login`, `Registration`, `PasswordRecovery`, calendario, listino pubblico, `DownloadLink`. Nessun controller ha oggi `Roles=` o `Policy=`: i controlli stanno solo nei servizi. **Stima** S · **Rischio** nullo/basso.

### 4.4 Client, browser e mobile

#### SIC-34 – MEDIA – Dopo il logout restano la connessione in tempo reale e dati in memoria e nel browser
- **Origine**: SC-02, SC-05. **Chi**: chi usa dopo di lui lo stesso browser (PC condiviso, postazione, tablet, kiosk). **Stato** [V].
- **Difetto**: al logout l'hub SignalR resta connesso con il vecchio token (`signalr.service.ts`, `onTokenChanged` con token nullo esce senza chiudere; `onclose` rilancia `startConnection()` ogni 3 secondi all'infinito, rumore di 401); chat AI (`FabMenuComponent.messages`), notifiche, cronologia utenti (`_userCronology$`, mai azzerato) e job restano visibili fino al ricaricamento. Nel `localStorage` non toccato da `LogOut()` stanno: `costiDipendenti` e ricavi della commessa (`az-commessa-cdg.service.ts`: dato retributivo), coda offline delle timbrature con posizione GPS (`timbratura-offline.service.ts`), `kioskToken` e sede del kiosk.
- **Correzione**: al logout `hubConnection.stop()` e nessuna riconnessione senza token; `location.reload()` dopo il logout (soluzione piccola e robusta) o evento "sessione chiusa" che azzeri i servizi con stato; cancellare le chiavi con dati personali (lasciare lingua e tema); non salvare i costi per dipendente (tenerli in pagina o chiederli al server); coda offline: cancellare solo a logout volontario e chiedere conferma se non vuota; il token del kiosk resta una credenziale di dispositivo, ma va revocabile dal server e legata alla sede. **Stima** S · **Rischio** basso · **Rami** Infrastructure (client), AttendanceTracking_Dev (`Az_CommessaCdg`, timbratura).

#### SIC-35 – MEDIA – Preparazione a Capacitor (mobile)
- **Origine**: SC-06. Non è un difetto di oggi, è ciò che blocca la pubblicazione. Stato [V] per la configurazione, il resto è lista di controllo. Dettaglio in §6.3.

#### SIC-36 – BASSA – Token e testo dettato scritti in console; 82 `console.log` in produzione
- **Origine**: SC-03, SC-09 (parte), SC-10. **Rivalutata** (§2.4). `signalr.service.ts` scrive il token a ogni connessione (`accessTokenFactory`); `speech.service.ts` scrive il testo riconosciuto; il progetto non toglie i `console.log` dal build (82 occorrenze nel client, alcune con dati di lavoro). **Impatto**: console visibile a utente, supporto, estensioni; in futuro il registro del WebView. **Correzione**: eliminare la riga del token; regola ESLint `no-console` o `drop: ['console']` nel build. **Stima** S · **Rischio** nullo · **Rami** Infrastructure (client).

#### SIC-37 – BASSA – L'interceptor allega il token a qualunque richiesta
- **Origine**: SC-07. `http-interceptor.ts` non limita l'host e invia anche `Bearer null` alle chiamate pubbliche. Oggi tutte le chiamate vanno a `apiUri`, quindi nessuna fuga; il rischio cresce con l'indirizzo assoluto di Capacitor. **Correzione**: aggiungere il token solo se l'URL inizia con `apiUri` e il token non è nullo; gestire anche il `403` (oggi solo il `401`). **Stima** S · **Rischio** basso.

#### SIC-38 – BASSA – Voce, nuove schede e appunti
- **Origine**: SC-09, SC-11, SC-12. `SpeechRecognition` invia l'audio ai server del produttore del browser: serve informativa, interruttore per azienda, plugin nativo con consenso sul mobile (SC-09). `window.open('', '_blank')` seguito da un `blob:` in `dip-gg-nota-spesa.service.ts` (`ApriAllegato`) e `export-cau-edit-page.component.ts` (`vedi`) mantiene `window.opener`: non sfruttabile oggi perché il tipo è limitato a PDF e immagini (ma il tipo è quello dichiarato dal client: vedi SIC-22); usare `noopener` o un visualizzatore interno (SC-11). Il token MCP e il link del calendario si copiano in chiaro negli appunti (voluto, mostrati una sola volta): aggiungere l'avviso di cancellarli (SC-12). **Stima** S.

#### SIC-39 – INFO – Cose fatte bene lato server e client
Vedi §2.5. Da non rompere: token solo in memoria, nessuna esecuzione di contenuto, `PuoLavorareCome`, `LogSanitizer`, CORS chiuso.

#### SIC-40 – INFO – Superata o chiusa
- Il ritorno dal pagamento (`pagamento-ritorno.html`, SC-14): origine controllata nel `postMessage`, conferma fatta dal server sul soggetto chiamante [V].
- Nessun service worker, nessuna cache offline di dati; il manifest PWA non cambia il modello di sicurezza (SC-15). Fotocamera e posizione solo nelle pagine che le usano (SC-16).

---

## 5. Rapporto con `piano-security.md`

| Fase | Contenuto | Stato (verificato nel codice) | Cosa cambia |
|---|---|---|---|
| 0 | Segreti fuori dal repository, nuova chiave JWT | **Fatta in parte**: file tracciati puliti, avvio rifiutato senza chiave; resta la rotazione e la storia git (SIC-18) | Aggiunge SIC-18, SIC-19, SIC-20 |
| 1 | Ruoli nel token (`Roles`, `rolecode`) e `AddSeconds` | **Non fatta**: il token non ha claim di ruolo; il bug `AddSeconds` è ancora presente | Resta prerequisito delle policy (SIC-01, SIC-33); decisione sul valore (SIC-D2) |
| 2 | Blindare `UserLoad` con regola per radice | **Fatta con altra regola**: `PuoLavorareCome` ("solo chi sta sotto", ruolo del login); resta aperto `UserRoles` (SIC-09) | Il piano va riscritto: non serve `IUserScopeAuthorizer` della radice; la stessa funzione va **riusata** per SIC-01 e SIC-02 |
| 3 | `[Authorize(Policy)]` su tutti gli endpoint e fallback policy | **Non fatta**: nessun `Roles=`/`Policy=`, nessuna fallback policy | Sale a priorità alta (SIC-01 ne è l'effetto più grave, SIC-33) |
| 4 | Password, blocco tentativi, limitatore, password predefinita, messaggi di login, CORS, Swagger | **Fatta in parte**: CORS chiuso, Swagger spento; **non fatti** lunghezza minima, blocco, limitatore, password predefinita nei `Put` (SIC-06) | Aggiunge il **seed** (SIC-03), il cambio password anonimo (SIC-06), registrazione (SIC-15) |
| 5 | `IdAz_Anagrafica` denormalizzato e filtro globale | **Non fatta**; in multi-azienda l'isolamento regge per schema | Serve soprattutto per la modalità singola (SIC-10) e per le entità `public` (SIC-02: per queste non si può usare il filtro per azienda, serve la funzione di perimetro) |
| 6 | Progetto di test sulle regole | **Non fatta** | Resta; i test nominati: creazione utenti con ruolo alto, id altrui, `UserLoad` fuori ramo |
| (nuove) | Nulla nel piano su: canali in tempo reale, sessione e revoca, file, log, privacy, intestazioni, client e mobile | — | Coperte da questo documento (SIC-13, SIC-14, SIC-21 … SIC-38) |

**Giudizio sul piano.** L'analisi iniziale resta valida nel metodo, ma lo stato descritto è in parte superato (Fase 2) e non vede due problemi più gravi dell'escalation via `UserLoad`: la creazione di utenti con ruolo arbitrario (SIC-01) e il seed di produzione (SIC-03). Il piano presuppone "nessun ambiente di produzione": vero oggi, ma il Docker e il VPS sono già predisposti, quindi i blocchi di §7 vanno chiusi prima della prima installazione esposta (aggiorna `FB-D1` e `FB-DOC.6`).

---

## 6. Privacy/GDPR, dipendenze, preparazione mobile

### 6.1 Privacy e GDPR
- **Dati particolari trattati**: assenze e malattie nei giustificativi, cedolini, CU, documenti d'identità dell'import, codice fiscale, IBAN, dati disciplinari e di formazione, posizione GPS delle timbrature. Rilevanti per SIC-04 (accesso ai dati dei colleghi), SIC-23 (log), SIC-24 (cifratura), SIC-25 (provider esterni).
- **Trasferimenti extra-UE**: provider LLM predefinito (SIC-25); riconoscimento vocale del browser (SIC-38); relay email Brevo (verificare accordo di trattamento).
- **Conservazione**: log senza scadenza (SIC-23); i dati di aziende e utenti hanno la cancellazione fisica (`ANALISI_INFRASTRUCTURE_CANCELLAZIONE.md`), ma restano aperti i deleter delle Presenze e la validità del token di un utente eliminato (SIC-05).
- **Registro e informativa**: il modulo Compliance ha registro dei trattamenti e delle violazioni; vanno aggiunte le voci dei flussi sopra, la DPIA per la posizione GPS e la voce, e una procedura per le violazioni (SIC-01 e SIC-02 sono, se sfruttati, violazioni notificabili).
- **Diritti degli interessati**: esportazione e cancellazione sono parzialmente coperte dai moduli; non è stato verificato il percorso completo [?].

### 6.2 Dipendenze
- **NuGet** (server, data, service, transitive, interrogando il registro con copie dei `.csproj` fuori dal repository): nessuna vulnerabilità nota. Versioni: .NET 11, EF Core 10.0.12, MailKit 5.0.0, Jint 4.17.0, ClosedXML 0.105.1, PdfPig 0.1.16, PDFsharp 6.2.4, RabbitMQ.Client 7.2.2, Newtonsoft.Json 13.0.4, ModelContextProtocol.AspNetCore 2.4.0, Hangfire.AspNetCore 1.8.25. Da tenere d'occhio: PdfPig 0.1.x e Serilog.Sinks.PostgreSQL 2.3.0 (poco manutenuti).
- **npm del client** (`npm audit --package-lock-only` su copia neutra): 20 avvisi (5 alti, 14 moderati, 1 basso). Solo sviluppo e build (`@angular/cli`, `@angular/build`, `vite`, `body-parser`, `follow-redirects`, `glob`, `minimatch`, `qs`, `uuid`, `fast-uri`, `@typescript-eslint/*`, `@capacitor/cli`, `rimraf`, `xcode`): girano sul PC dello sviluppatore e nello stadio `web` del Dockerfile, non nell'immagine finale. In esecuzione resta `zod` sotto la 4.6.6 (alto, consumo di memoria con collezioni enormi), transitiva di `@angular/forms`, nel browser: rischio pratico basso, si risolve aggiornando Angular.
- **Pratica**: aggiungere `dotnet list package --vulnerable` e `npm audit --omit=dev` a un controllo periodico (mensile) e prima di ogni rilascio.

### 6.3 Preparazione mobile (Capacitor)
Stato: `capacitor.config.ts` ha ancora l'identificativo del modello iniziale (`io.ionic.starter`), nessun progetto `android/` o `ios/`, installati solo `app`, `core`, `haptics`, `keyboard`, `status-bar` (nessun archivio sicuro, biometria o schermata privata). Il token vive solo in memoria: a ogni chiusura si rifà il login. Punti da chiudere **prima dello store**, in ordine:
1. **Sessione**: prima SIC-13 (revoca, limite assoluto) e un refresh token con durata e revoca; solo dopo, se si vuole la sessione persistente, Keychain/Keystore (mai Preferences o `localStorage`) con sblocco biometrico locale.
2. **Indirizzo API**: `environment.prod.ts` usa URL relativi; in Capacitor l'origine è locale e le chiamate non raggiungerebbero il server. Serve un `environment` mobile con URL assoluto https e `Cors:AllowedOrigins` con le origini di Capacitor (mai `*`: con `*` il codice risponde con `SetIsOriginAllowed(true)` più `AllowCredentials`; l'app usa Bearer, non cookie).
3. **WebView**: `allowNavigation` vuoto, niente traffico in chiaro né contenuto misto, backup disattivato per non copiare `localStorage`, debug del WebView spento in release.
4. **Schermate**: schermata privata nel selettore delle app (cedolini, dati HR).
5. **Collegamenti profondi**: se si aggiungono (conferma email, recupero password) solo App Links e Universal Links verificati, mai schemi personalizzati per token di reset.
6. **Pinning del certificato**: valutare per un'app con dati HR (costo: rotazione dei certificati; usare la chiave pubblica con copia di riserva).
7. **Permessi**: posizione (timbratura), fotocamera (QR), microfono (comandi vocali), con descrizioni chiare; la voce nel WebView va sostituita da un plugin nativo con consenso.
8. **Privacy degli store**: dati raccolti (posizione precisa, identificativi, dati di lavoro, audio) nelle etichette di Apple e Google; informativa e DPIA già nel modulo Compliance.
9. **Caddy con dominio**: con `:80` (prova senza dominio) l'app mobile non funzionerebbe e sarebbe in chiaro.
10. Rilevamento di root o jailbreak: solo nota, non basare la sicurezza su di esso.
Prima SIC-36, SIC-34, SIC-37 e SIC-21: sono piccoli e servono anche al browser.

---

## 7. Ordine di lavoro proposto

### 7.1 Blocchi piccoli e indipendenti

Regola: un blocco, un ramo `X_Dev_<etichetta>` in un worktree dedicato, una prova, una conferma. Nessun commit verso `origin`. La catena dei merge parte solo su conferma (Infrastructure_Dev → Infrastructure → moduli → AllFunction_Dev). I file di Infrastructure si modificano solo su Infrastructure_Dev.

| Blocco | Contenuto | Rilievi | Ramo | Dipende da | Prove | Stima |
|---|---|---|---|---|---|---|
| **B0** | Operazioni senza codice: password iniziali, rotazione chiavi, token del remote, spegnere la registrazione, Caddy (header, limiti, percorsi) | SIC-03 (tampone), 18, 19, 21 | nessuno (+ Docker in Infrastructure_Dev) | nulla | verifica manuale dell'ambiente esposto | S |
| **B1** | `[Authorize]` su chat AI e `Parameter/Roles`; rifiuto utente vuoto; fallback policy con `[AllowAnonymous]` espliciti; rimozione demo; hub autenticato e senza broadcast | SIC-07, 09 (parte), 14, 33 | Infrastructure_Dev | nulla | accesso anonimo respinto; login, hub e chat funzionano dopo il login | S |
| **B2** | Funzione di perimetro unica (estratta da `PuoLavorareCome`) e controllo del ruolo nei `Put` di creazione; poi `Get`/`Put`/elenchi per id | SIC-01, 02, 09 | Infrastructure_Dev | B1 | test nominati: utente semplice non crea ruoli alti né legge id altrui; flussi attuali (wizard, registrazione, importazione dipendenti) invariati | M + G a gruppi |
| **B3** | Seed solo in sviluppo; primo SuperUser con password casuale; password predefinita eliminata dai `Put` | SIC-03, SIC-01 (parte) | Infrastructure_Dev | nulla | primo avvio su database vuoto in Development e in Production; aggiornare gli script di prova | M |
| **B4** | Blocco tentativi, limitatore, messaggio unico, password minima, `PasswordChange` | SIC-06, 15, 16 | Infrastructure_Dev | B3 | cinque errori bloccano; recupero e registrazione limitati; utente dietro NAT | M |
| **B5** | `Disabled` in `Login`/`UserLoad`/`OnTokenValidated`; revoca e logout; limite assoluto; valore della durata | SIC-05, 13 | Infrastructure_Dev | B2 | disabilitato respinto subito; logout invalida; sessione attiva scade al limite | M |
| **B6** | Client: logout completo, token fuori dalla console, interceptor per host, `403` | SIC-34, 36, 37 | Infrastructure_Dev (client) | nulla | logout con hub chiuso; nessun `console.log` del token nel build | S |
| **B7** | Presenze: ruolo e proprietà, mesi approvati, a gruppi (prima i quattro metodi di SIC-04) | SIC-04, 10, 11, 12 | AttendanceTracking_Dev | B2 (regole condivise) | `User` non legge né scrive per altri; motore e MCP invariati | G |
| **B8** | Chat AI e MCP: ricontrollo del ruolo, conferma legata agli argomenti | SIC-08, 26 | Infrastructure_Dev | B2 | intento non ammesso rifiutato | M |
| **B9** | Pagamenti: debug spento, confronto PayPal; `PublicUrl` obbligatorio; chiavi separate | SIC-16, 17, 20, 30 | Infrastructure_Dev | nulla | avvio rifiutato in Production con debug o senza URL | S |
| **B10** | File: streaming, zip, magic bytes, CSV | SIC-22, 31 | DocumentManagement_Dev, HumanResources_Dev, CRM_Dev, AttendanceTracking_Dev | nulla | file legittimi grandi ancora accettati | M |
| **B11** | Privacy: log, LLM, cifratura disco e backup, firma | SIC-23, 24, 25, 27 | Infrastructure_Dev, DocumentManagement_Dev | decisioni D5, D6 | verifica dei log; backup ripristinabile | M-G |
| **B12** | Docker: ruolo applicativo, immagini fisse, hardening, `ForwardedHeaders` | SIC-28, 32 | Infrastructure_Dev | nulla | avvio del compose di prova | M |
| **B13** | Mobile (§6.3) | SIC-35 | Infrastructure_Dev (client) | B5, B6, B9 | progetto nativo di prova | G |
| **B14** | Progetto di test del server (regole di autorizzazione e perimetro) | trasversale | Infrastructure_Dev | B2 | `dotnet test` verde | M |

Nota di ordine: **B0 e B1 subito** (rischio massimo, sforzo minimo); **B2 e B3** sono i veri chiusori delle tre critiche; B7 è il più grande e può procedere a servizi.

### 7.2 Chi può creare e modificare quali utenti (tabella proposta)

Ricavata dal client (pagine di modifica e guardie in `route-infrastructure.service.ts`, ordine dei ruoli in `RoleCode`, `IsInGroup*` di `auth.service.ts`) e dalla regola di `PuoLavorareCome` ("solo chi sta sotto, nel proprio ramo", ruolo del login). I livelli numerici di `RoleCode`: User 0, CompanyAdmin 10, CompanyPowerAdmin 11, FinancialAdvisorAdmin 100, FinancialAdvisorPowerAdmin 101, DealerAdmin 1000, DealerPowerAdmin 1001, Admin 10000, PowerAdmin 10001, SuperUser 100000. **Il ruolo del chiamante si legge dal database (il più alto), non dal primo ruolo.**

| Ruolo del chiamante | Utenti che può creare/modificare (pagina del client) | Ruoli assegnabili | Perimetro |
|---|---|---|---|
| SuperUser | tutti | qualunque ruolo, ma **SuperUser solo da procedura di installazione**, non da API | globale |
| PowerAdmin / Admin | utenti di piattaforma (`useredit`), centri (`dealeredit`) e loro utenti | Admin, PowerAdmin (utenti di piattaforma); DealerAdmin, DealerPowerAdmin (centri) | globale; **non** può creare SuperUser; PowerAdmin da Admin: decisione SIC-D3 |
| DealerPowerAdmin / DealerAdmin | utenti del proprio centro (`userdealeredit`), studi (`financialadvisoredit`) | DealerAdmin, DealerPowerAdmin; per gli studi i due utenti principali FinancialAdvisorPowerAdmin e FinancialAdvisorAdmin | solo i propri centri e gli studi sotto di essi |
| FinancialAdvisorPowerAdmin / FinancialAdvisorAdmin | utenti del proprio studio (`userfinancialadvisoredit`), aziende (`companyedit`) | FinancialAdvisorAdmin, FinancialAdvisorPowerAdmin; per le aziende i due utenti principali CompanyPowerAdmin e CompanyAdmin | solo i propri studi e le aziende sotto di essi |
| CompanyPowerAdmin / CompanyAdmin | utenti della propria azienda (`usercompanyedit`) | in creazione solo **User** (il client offre solo quello a un nuovo utente); in modifica User e CompanyAdmin; CompanyPowerAdmin non assegnabile dal client | solo la propria azienda (`IdCompany` del token) |
| User | nessuno (cambia la propria password) | — | — |

Regole trasversali da imporre sul server:
1. Il ruolo assegnato deve essere **strettamente inferiore** a quello del chiamante, salvo i casi di pari livello della tabella (utenti dello stesso centro o studio) da decidere in SIC-D3.
2. Il nodo di destinazione deve essere nel perimetro del chiamante (stessa catena Centro → Studio → Azienda di `PuoLavorareCome`), con i collegamenti disabilitati del chiamante che non contano.
3. Il chiamante non modifica il proprio ruolo né si disabilita.
4. Password: casuale o scelta ma conforme alla politica (SIC-06), cambio al primo accesso per le utenze create da altri.
5. Chi "lavora come" un altro è valutato come utente del login (`UserIdFirstConnection`), come già in `PuoLavorareCome`.
6. I sottoprocessi interni (`isSubProcess`: registrazione, creazione azienda, importazione dipendenti) agiscono per conto del sistema: hanno un contesto esplicito, non saltano il controllo in modo generico.
7. Un utente con più ruoli (per esempio User più CompanyAdmin) ha il livello del ruolo più alto.

---

## 8. Decisioni aperte

| # | Decisione | Note |
|---|---|---|
| SIC-D1 | Esposizione: l'app va su Internet (VPS) prima dei blocchi B0-B5? | Se sì, B0 è obbligatorio e la registrazione pubblica resta spenta fino a B2 e B4 |
| SIC-D2 | Durata vera del token e limite assoluto della sessione (oggi 20 minuti rinnovabili senza fine; con `AddMinutes` diventerebbero 20 ore) | Collegata a `FB-D4`; scegliere anche se serve un refresh token per il mobile |
| SIC-D3 | Regole di pari livello: un Admin può creare Admin o PowerAdmin? un CompanyAdmin può creare altri CompanyAdmin? | La tabella §7.2 è una proposta; il client oggi lo permette in alcune pagine |
| SIC-D4 | Riscrivere la storia git per togliere la chiave storica, oppure solo ruotare | La riscrittura invalida cloni e worktree; ha senso solo se il repository è o sarà pubblico |
| SIC-D5 | Provider LLM predefinito per le funzioni con dati personali (UE o locale) e consenso per azienda | Qualità inferiore con i modelli locali (SIC-25) |
| SIC-D6 | Cifratura dei dati: solo disco e backup, oppure anche file e colonne per azienda | Costo e migrazione dei dati (SIC-24) |
| SIC-D7 | Registrazione pubblica: tenerla, con CAPTCHA, verifica del dominio e tetto di aziende DEMO, oppure farla approvare | Collegata a SIC-01 e SIC-15 |
| SIC-D8 | Modalità singola: ancora supportata? | Se no, SIC-10 scende e il filtro per azienda diventa un semplice controllo di coerenza |
| SIC-D9 | Utenti iniziali e DEMO: restano nel seed per lo sviluppo? Con quale meccanismo si crea il primo SuperUser in Production | Collegata a SIC-03 |
| SIC-D10 | `FB-D1`: riscrivere `piano-security.md` sullo stato attuale (Fase 2 superata, Fasi 1/3/4/5/6 aperte) oppure assorbirlo in questo documento | Raccomandato: riscrivere lo stato e rimandare qui per i dettagli |
| SIC-D11 | Hangfire e RabbitMQ: usare o togliere | Collegata a `FB-D3` e a SIC-32, SIC-33 |

---

## 9. Checklist

Legenda: ✅ svolto · ⏳ da fare · 🔄 in corso · 🔍 da decidere
Priorità: **P0** subito, prima di qualunque esposizione · **P1** prima della prima installazione reale · **P2** prima del rilascio ai clienti · **P3** miglioramento.
Codici `SIC-…`: non collidono con gli altri documenti (`FB-*`, `ATT-*`, `INF-*`, `MCP-*`, `PERS-*`). Tra parentesi i codici di origine.

#### `SIC-DOC` – Documenti
- ✅ `SIC-DOC.1` Documento di sintesi della revisione di sicurezza (questo file), 25/11/2026
- ⏳ `SIC-DOC.2` Riscrivere lo stato di `piano-security.md` (Fase 2 superata; §5) e correggere `ANALISI_INFRASTRUCTURE_APPLICATIVO.md` §4.3 (`FB-DOC.6`) · P1
- ⏳ `SIC-DOC.3` Aggiornare `FB-SEC.*` e `ATT-SEC.*` con i rimandi a questo documento · P2
- ⏳ `SIC-DOC.4` Aggiungere voci al registro dei trattamenti (provider LLM, voce, GPS, log) nel modulo Compliance · P2

#### `SIC-B0` – Operazioni senza codice (blocco B0)
- ⏳ `SIC-B0.1` Cambiare le password degli utenti iniziali in ogni database esistente · P0 (SB-01)
- ⏳ `SIC-B0.2` Spegnere la registrazione pubblica sull'istanza esposta (o regola nel proxy) · P0 (SA-01)
- ⏳ `SIC-B0.3` Header di sicurezza e HSTS in Caddy; CSP prima in sola segnalazione · P1 (SB-07, SC-04)
- ⏳ `SIC-B0.4` Limiti di frequenza e blocco dei percorsi non necessari in Caddy · P0 (SA-03, SA-04)
- ⏳ `SIC-B0.5` Ruotare chiave JWT, password DB e RabbitMQ negli ambienti che usarono i valori storici · P1 (SB-05)
- ⏳ `SIC-B0.6` Rigenerare la credenziale nell'URL del remote git locale e ripulire l'URL · P0 (SB-06)
- ⏳ `SIC-B0.7` Verificare sul VPS: `APP_PUBLIC_URL`, `Payments__Debug` falso, MCP spento se non serve, porte, cifratura disco e backup · P1 (SA-11, SB-08, SB-14)

#### `SIC-ACC` – Accessi, ruoli e perimetro
- ⏳ `SIC-ACC.1` (SIC-01) Controllo di ruolo e di perimetro nei `Put` di creazione utente, elenco chiuso di ruoli, password casuale · P0
- ⏳ `SIC-ACC.2` (SIC-02) Funzione di perimetro unica e controllo su `Get`/`Put`/elenchi per id di utenti, aziende, studi, centri · P0
- ⏳ `SIC-ACC.3` (SIC-09) `[Authorize]` su `Parameter/Roles`; perimetro su `UserRoles`, `UserCheck`, `UserList`, `CompanyApplicationList` · P1
- ⏳ `SIC-ACC.4` (SIC-33) Fallback policy con `[AllowAnonymous]` espliciti; rimozione o protezione dei controller demo · P1
- ⏳ `SIC-ACC.5` (SIC-04) Presenze: ruolo e proprietà in `Get_Timesheet_AllData`, `Dip_GG_TimbraturaPut`, `Dip_Periodo_SetStato`, `Riporto_Upsert`, `Par_*` · P1
- ⏳ `SIC-ACC.6` (SIC-04) Presenze: i rimanenti circa 30 servizi, mesi approvati bloccati, sottoprocessi con `isSubProcess` · P2
- ⏳ `SIC-ACC.7` (SIC-10) `IdAz_Anagrafica` mai dal client e filtro per azienda (modalità singola) · P2, 🔍 SIC-D8
- ⏳ `SIC-ACC.8` (SIC-11) DTO senza campi di sistema; richiedente diverso da approvatore · P2
- ⏳ `SIC-ACC.9` (SIC-08) Chat AI: ricontrollo del ruolo in `ExecuteCommandAsync` · P2
- ⏳ `SIC-ACC.10` (SIC-12) Kiosk: chiave del PIN dedicata, badge come hash, tentativi condivisibili · P3
- ⏳ `SIC-ACC.11` Policy per livello `[Authorize(Policy)]` come difesa in profondità; ruoli nel token (Fasi 1 e 3) · P2

#### `SIC-AUT` – Autenticazione e sessione
- ⏳ `SIC-AUT.1` (SIC-03) Seed degli utenti solo in sviluppo; primo SuperUser con password casuale e cambio obbligatorio; aggiornare gli script di prova · P0
- ⏳ `SIC-AUT.2` (SIC-06) Blocco tentativi, limitatore per IP e utente, messaggio unico, password minima 10, `PasswordChange` incluso · P0
- ⏳ `SIC-AUT.3` (SIC-07) `[Authorize]` su `ChatAI/SendMessage`, rifiuto dell'utente vuoto, tetti e pulizia delle sessioni · P0
- ⏳ `SIC-AUT.4` (SIC-05) `Disabled` ed eliminazione in `Login`, `UserLoad` e `OnTokenValidated` · P1
- ⏳ `SIC-AUT.5` (SIC-13) Logout con revoca, limite assoluto della sessione, correzione di `AddSeconds` · P1, 🔍 SIC-D2
- ⏳ `SIC-AUT.6` (SIC-14) Hub autenticato, niente `Clients.All`, metodo di prova rimosso · P1
- ⏳ `SIC-AUT.7` (SIC-15) Registrazione: interruttore, CAPTCHA o limite per IP, tetto DEMO, pulizia · P1, 🔍 SIC-D7
- ⏳ `SIC-AUT.8` (SIC-16) `App:PublicUrl` obbligatorio fuori da Development; via il ripiego su `Origin`; `AllowedHosts` · P1
- ⏳ `SIC-AUT.9` (SIC-20) Chiavi separate per JWT, MCP, link e PIN · P2

#### `SIC-SRV` – Server, file, integrazioni, infrastruttura
- ⏳ `SIC-SRV.1` (SIC-17) `Payments:Debug` falso di default e blocco in Production · P1 (prima di ogni vendita)
- ⏳ `SIC-SRV.2` (SIC-30) PayPal: confronto di importo, valuta e riferimento · P2
- ⏳ `SIC-SRV.3` (SIC-21) Intestazioni di sicurezza nel server o nel proxy; token fuori dall'URL dopo l'uso · P1
- ⏳ `SIC-SRV.4` (SIC-22) Upload in streaming, limiti agli zip, magic bytes, antivirus opzionale · P2
- ⏳ `SIC-SRV.5` (SIC-23) Log: livello, campi personali, conservazione · P2
- ⏳ `SIC-SRV.6` (SIC-25) Provider LLM UE o locale, consenso per azienda, voce nell'informativa · P2, 🔍 SIC-D5
- ⏳ `SIC-SRV.7` (SIC-26) MCP: conferma legata agli argomenti e fuori dal modello; `Mcp:MaxLevel` 2 · P2
- ⏳ `SIC-SRV.8` (SIC-24) Cifratura di disco e backup; protezione delle chiavi di DataProtection; valutare file e colonne · P1 (disco e backup), P2 (resto), 🔍 SIC-D6
- ⏳ `SIC-SRV.9` (SIC-27) Firma: utente reale registrato, OTP, catena di hash · P2
- ⏳ `SIC-SRV.10` (SIC-28) Ruolo applicativo di database non superutente · P2
- ⏳ `SIC-SRV.11` (SIC-29) `Account/Download`: rifiutare i file senza registrazione · P3
- ⏳ `SIC-SRV.12` (SIC-31) Neutralizzare le formule in CSV ed Excel · P3
- ⏳ `SIC-SRV.13` (SIC-32) Container: immagini fisse, `no-new-privileges`, `cap_drop`, `read_only`, healthcheck, `ForwardedHeaders` con la rete del compose · P3
- ⏳ `SIC-SRV.14` (SIC-33) Hangfire: filtro di autorizzazione prima di accenderlo · P3, 🔍 SIC-D11
- 🔍 `SIC-SRV.15` (SIC-18) Decidere sulla riscrittura della storia git · SIC-D4

#### `SIC-CLI` – Client, browser e mobile
- ⏳ `SIC-CLI.1` (SIC-34) Logout: chiusura dell'hub, niente riconnessione, ricarica o azzeramento dello stato, pulizia delle chiavi con dati personali · P1
- ⏳ `SIC-CLI.2` (SIC-36) Togliere il token dalla console; `no-console` o `drop` nel build · P1
- ⏳ `SIC-CLI.3` (SIC-37) Interceptor: token solo verso `apiUri` e se non nullo; gestione del `403` · P2
- ⏳ `SIC-CLI.4` (SIC-38) Informativa e interruttore per la voce; `noopener`; avviso sugli appunti · P3
- ⏳ `SIC-CLI.5` (SIC-35) Capacitor: i dieci punti di §6.3, in ordine · P2 (prima dello store)
- ⏳ `SIC-CLI.6` (SC-05) Non salvare `costiDipendenti` nel browser; coda offline con conferma · P2

#### `SIC-TST` – Prove
- ⏳ `SIC-TST.1` Progetto di test del server: matrice ruolo→azione, perimetro, creazione utenti con ruolo alto (test nominato), `UserLoad` fuori ramo · P1
- ⏳ `SIC-TST.2` Prove a mano per ruolo dopo B1-B5: nessun `403` su percorsi legittimi (wizard utenti, registrazione, importazione dipendenti, chat, hub) · P1
- ⏳ `SIC-TST.3` Ripetere questa revisione sui punti "non verificati" di §1 (servizi `Par_*`/`Az_*`/`Dip_*` restanti, handler per ruolo, CRM/HR/Documentale/Assets, Docker e VPS) · P2

#### Decisioni (rimando a §8)
- 🔍 `SIC-D1` … `SIC-D11`
