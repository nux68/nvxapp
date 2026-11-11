# Analisi – Sistema di ticket di assistenza

Branch di sviluppo: **Infrastructure_Dev** (funzione condivisa: tabelle in `public`, cartelle `Infrastructure`).
Documento di sola analisi: nessun codice è stato scritto.

---

## 1. Premessa: i livelli

| Livello | Ruoli | Nodo (entità) | `RoleCode` |
|---|---|---|---|
| 0 – SuperUser | SuperUser (riceve dal livello DEV e apre ticket verso il livello DEV) | nessuno (globale) | 100000 |
| 1 – DEV | PowerAdmin, Admin | nessuno (globale) | 10001 / 10000 |
| 2 – Dealer | DealerPowerAdmin, DealerAdmin | `Dealer` (via `UserDealer`) | 1001 / 1000 |
| 3 – FinancialAdvisor | FinancialAdvisorPowerAdmin, FinancialAdvisorAdmin | `FinancialAdvisor` (via `UserFinancialAdvisor`) | 101 / 100 |
| 4 – Company | CompanyPowerAdmin, CompanyAdmin | `Company` (via `UserCompany`) | 11 / 10 |
| 5 – User | User | `Company` (via `UserCompany`) | 0 |

Note ricavate dal codice:
- Gerarchia: `Dealer` → `FinancialAdvisor.IdDealer` → `Company.IdFinancialAdvisor` → `UserCompany`.
- **Un utente può avere più ruoli.** A livello Company un utente può essere contemporaneamente `User` e `CompanyAdmin` (o `CompanyPowerAdmin`): la modifica di un utente company salva una lista di ruoli. Il login legge `roles[0]` solo per ricavare il contesto (azienda, FA, dealer), che per i ruoli company è lo stesso: per i ticket **non** si può usare `roles[0]`, vanno letti tutti i ruoli dell'utente (vedi §2.1).
- Il contesto dell'utente collegato (dealer / financialadvisor / company) è nel token ed è esposto da `ServiceBase` (`CurrentDealer`, `CurrentFinancialAdvisor`, `CurrentCompany`).
- Il valore di `RoleCode` cresce salendo nella gerarchia: il confronto "livello più alto/più basso" si fa direttamente sul codice.
- **Ogni nodo ha sempre un PowerAdmin**, tranne il livello User e il SuperUser: non serve gestire il caso "nodo senza PowerAdmin".

---

## 2. Regole funzionali (requisiti consolidati)

1. Il ticket può essere aperto dai livelli **2–5** (Dealer, FinancialAdvisor, Company, User) e dal **SuperUser**. Il livello DEV riceve soltanto. Il ticket del SuperUser va **verso il basso**: viene assegnato direttamente a un utente del livello DEV (vedi §2.2).
2. All'apertura il ticket va **al padre** di chi lo apre.
3. Il ticket sale con l'azione "Inoltra al padre", ma **solo nei livelli abilitati dal tipo di richiesta** (vedi §4): passa al primo livello abilitato sopra, saltando quelli non abilitati. Se sono abilitati tutti i livelli, la catena termina al SuperUser.
4. Il **PowerAdmin** di un nodo vede i ticket del suo nodo e quelli dei nodi figli; all'ingresso vede i "propri", ma può passare a quelli dei figli.
5. L'**Admin** (non Power) vede **solo i ticket che gli sono stati assegnati** dal suo PowerAdmin. Non vede i ticket non assegnati del nodo né quelli dei figli, salvo assegnazione.
6. Il **SuperUser vede tutti i ticket**.
7. A ogni passaggio (apertura, assegnazione, inoltro, richiesta informazioni, risposta, risoluzione, riapertura, chiusura) si possono aggiungere **testo e allegati**.
8. Il livello che gestisce può **chiedere informazioni** senza risolvere il ticket, a chi l'ha aperto **oppure a uno dei livelli intermedi** da cui il ticket è passato. Esempio: User apre → Company inoltra → FinancialAdvisor può chiedere informazioni a User o a Company. La risposta riporta il ticket al livello che le ha chieste (vedi §5.1).
9. Quando il ticket è **Risolto** la visibilità torna a chi l'ha aperto, che può **chiuderlo** o **riaprirlo** (torna al livello che l'aveva risolto).
10. Le notifiche sono previste **già nella prima versione**, in push con SignalR.
11. A ogni passaggio verso l'alto, se il tipo lo prevede, si tenta prima una **risoluzione automatica con AI**. Nella prima versione è solo una predisposizione e non risolve mai (vedi §4.1).

### 2.1 Utenti con più ruoli

Un utente company può avere sia il ruolo `User` sia un ruolo di amministrazione (`CompanyAdmin` o `CompanyPowerAdmin`). Regole:

1. **I permessi sono l'unione dei ruoli.** Ogni controllo è "l'utente *ha* il ruolo X tra i suoi", mai "il ruolo dell'utente *è* X". Chi è `User` + `CompanyPowerAdmin` vede e gestisce tutto ciò che vede un CompanyPowerAdmin, oltre ai ticket aperti come dipendente.
2. **In apertura si sceglie "a nome di quale ruolo".** Il ruolo determina il destinatario:
   - come **dipendente** (`User`) → va al CompanyPowerAdmin della sua azienda;
   - come **amministratore aziendale** (`CompanyAdmin` / `CompanyPowerAdmin`) → va al FinancialAdvisorPowerAdmin.

   La scelta compare solo se l'utente ha più ruoli che possono aprire. Il valore proposto è il ruolo di amministrazione, cioè il più alto. Il server verifica che il ruolo scelto sia davvero tra quelli dell'utente.
3. **Il ruolo di apertura resta sul ticket** (`OpenerRoleId`). Quando il ticket torna a chi l'ha aperto (Risolto, Richiesta informazioni), torna a quell'utente in quel ruolo.
4. **Un CompanyPowerAdmin che apre come dipendente** manda il ticket al livello Company della sua azienda, cioè anche a sé stesso. È ammesso, ma non può gestirlo lui: assegnazione, risoluzione e inoltro di un ticket aperto da sé stessi sono riservati agli altri PowerAdmin del nodo. Se è l'unico PowerAdmin dell'azienda, la scelta "come dipendente" non gli viene proposta.
5. **Assegnatari e destinatari delle notifiche** sono gli utenti del nodo che hanno *tra i loro ruoli* `…Admin` o `…PowerAdmin`. Non bisogna escludere chi ha anche il ruolo `User`.
6. **Tipi proposti in apertura:** il controllo dei livelli abilitati (§4) si fa sul livello del ruolo scelto, non sul ruolo più alto dell'utente.

### 2.2 Ticket aperti dal SuperUser

Il SuperUser può aprire un ticket e **assegnarlo direttamente** a un utente con ruolo `PowerAdmin` o `Admin` (livello DEV, immediatamente sotto di lui). È l'unico caso in cui il ticket scende invece di salire.

1. **Apertura:** il SuperUser sceglie tipo, oggetto, testo, allegati e l'**assegnatario** (obbligatorio), tra gli utenti attivi con ruolo PowerAdmin o Admin.
2. **Cronologia generata:**
   - una riga di origine: RoleId SuperUser, autore e UserId = il SuperUser;
   - una riga al livello DEV: RoleId PowerAdmin, UserId = l'assegnatario scelto. Lo stato è subito **InCarico**.
3. **Percorso di origine** (`IdDealer`, `IdFinancialAdvisor`, `IdCompany`): resta vuoto, il ticket non nasce da un nodo della gerarchia.
4. **Gestione al livello DEV:** valgono le regole normali del nodo DEV.
   - I PowerAdmin DEV lo vedono tra quelli "in carico" del livello anche se è assegnato a un Admin, e possono riassegnarlo.
   - Un Admin lo vede solo se è lui l'assegnatario.
5. **Inoltro:** "Inoltra al padre" non è disponibile, perché il padre del livello DEV è il SuperUser, cioè chi l'ha aperto. Per ottenere chiarimenti si usa **Richiedi informazioni**, che riporta il ticket al SuperUser.
6. **Chiusura:** come per gli altri ticket. Risolto torna al SuperUser, che lo chiude o lo riapre (la riapertura torna all'utente DEV che l'aveva risolto).
7. **Tipo di richiesta:** il SuperUser può scegliere solo i tipi attivi che abilitano sia il livello SuperUser sia il livello DEV (per esempio "Attività di sviluppo").

### Catena dei padri

| Chi apre | Primo destinatario | Come si individua il nodo |
|---|---|---|
| User | CompanyPowerAdmin | `CurrentCompany` |
| Company (Power/Admin) | FinancialAdvisorPowerAdmin | `Company.IdFinancialAdvisor` |
| FinancialAdvisor | DealerPowerAdmin | `FinancialAdvisor.IdDealer` |
| Dealer | PowerAdmin (DEV) | globale |
| DEV (in inoltro) | SuperUser | globale |
| SuperUser (verso il basso, §2.2) | PowerAdmin o Admin scelto | globale, assegnazione diretta |

---

## 3. Modello dati

Tutte le tabelle in `public` (`PublicDbContext_Infrastructure`), entità in `nvxapp.server.data\Entities\Public`, derivate da `BaseEntity` (Id, CreationDate, ModifiedDate, ChangeUser).

**Regola importante:** nessuna navigation da `Company`, `Dealer`, `FinancialAdvisor` o `ApplicationUser` verso le tabelle dei ticket: solo chiavi dal ticket verso di loro. È lo stesso accorgimento usato per `CompanyRegistrationRequest`, perché i DbContext degli applicativi mappano `Company` e scoprirebbero le nuove tabelle seguendo le relazioni. Nessuna cancellazione in cascata: la cronologia va conservata.

### 3.1 TicketType (tipo di richiesta – tabella configurabile)

| Campo | Note |
|---|---|
| Id | |
| ApplicationType | `ApplicationType?` (enum esistente: Moke, AttendanceTracking). **Null = Generale**, cioè non legato a un applicativo |
| Codice | univoco, breve |
| AiResolution | `bool`, default false: a ogni passaggio verso l'alto si tenta la risoluzione automatica AI (§4.1) |
| Ordine | ordinamento nelle liste |
| Active | i tipi disattivati non sono proponibili in apertura ma restano sui ticket esistenti |

**Livelli abilitati: TicketTypeLevel.** Tabella figlia (`IdTicketType`, `Level`), una riga per ogni livello in cui il tipo è abilitato, univoca per coppia tipo/livello. `Level` è un enum con i sei livelli: SuperUser, DEV, Dealer, FinancialAdvisor, Company, User. Le regole sono al §4.

**Descrizione.** Una sola, nel campo `Descrizione` di `TicketType` (max 100), mostrata uguale in tutte le lingue: come gli altri dati delle tabelle (aziende, studi, centri), che non vengono tradotti. Nell'applicazione non c'è una traduzione automatica: i file it/en riguardano solo le etichette dell'interfaccia.

### 3.2 Ticket (la richiesta)

| Campo | Note |
|---|---|
| Id | |
| IdTicketType | |
| Oggetto | testo breve per gli elenchi |
| Text | descrizione iniziale |
| State | enum: vedi §5 |
| IdAspNetUsersOpener | chi ha aperto |
| OpenerRoleId | ruolo **scelto** in apertura (l'utente può averne più di uno, vedi §2.1) |
| IdDealer, IdFinancialAdvisor, IdCompany | **percorso di origine**, salvato all'apertura e valorizzato fino al livello di chi apre (un Dealer valorizza solo `IdDealer`; per i ticket del SuperUser restano tutti vuoti). Serve a individuare il nodo corrente e a filtrare "i ticket dei figli" con un semplice filtro (es. tutti i ticket con `IdFinancialAdvisor` = il mio) |
| CurrentRoleId | livello corrente (copia dell'ultima riga di cronologia) |
| CurrentIdAspNetUsers | assegnatario corrente, null se in attesa al livello |
| Version | cresce a ogni passaggio, per il controllo di concorrenza |

I campi "Current…" sono una copia dell'ultima riga di `TicketCronology`, aggiornata nella stessa transazione: evitano di cercare ogni volta l'ultima riga per costruire gli elenchi.

### 3.3 TicketCronology (cronologia dei passaggi)

| Campo | Note |
|---|---|
| Id | |
| TicketId | |
| RoleId | livello di visibilità dopo questo passaggio (come da specifica) |
| UserId | **assegnatario**: utente che ha in carico il ticket; null = in attesa al livello (visibile ai PowerAdmin del nodo) |
| IdAspNetUsersAuthor | **autore** del passaggio: chi ha eseguito l'azione e scritto il testo. Null per i passaggi eseguiti dall'AI |
| Action | enum: Apertura, Assegnazione, Inoltro, RichiestaInfo, RispostaInfo, Nota, Risoluzione, **RisoluzioneAI**, Riapertura, Chiusura, Riclassificazione, Spostamento (azienda passata a un altro studio, §9) |
| ReturnRoleId, ReturnUserId | **dove tornare**: valorizzati sulle righe RichiestaInfo e Risoluzione con il livello e l'utente che hanno chiesto o risolto. La risposta (o la riapertura) riporta lì il ticket |
| State | stato del ticket dopo il passaggio |
| Text | testo del passaggio (facoltativo per alcune azioni) |

Nella specifica originale `UserId` era usato sia per l'autore (punto 1.2) sia per l'assegnatario (punto 1.6): i due significati vanno separati.

### 3.4 TicketAttach (allegati)

| Campo | Note |
|---|---|
| Id | |
| IdTicketCronology | il passaggio a cui appartiene |
| FileName | nome originale |
| ContentType | |
| Size | byte |
| File | contenuto, `bytea` |

Il contenuto si legge **solo** dall'endpoint di download, mai negli elenchi né nel dettaglio.

### 3.5 Inizializzazione dei tipi di richiesta

Inserimento iniziale nella migration (o in `PopolateDB`, come per gli utenti di base). Proposta:

| Applicativo | Codice | Descrizione | Livelli abilitati |
|---|---|---|---|
| Generale | ACCESSO | Accesso e credenziali | Company, User |
| Generale | UTENTI | Gestione utenti | FinancialAdvisor, Company, User |
| Generale | CONTRATTO | Contratto e fatturazione | Dealer, FinancialAdvisor, Company |
| Generale | BUG | Malfunzionamento | tutti |
| Generale | NUOVA | Richiesta nuova funzionalità | tutti |
| Generale | SVILUPPO | Attività di sviluppo | SuperUser, DEV |
| AttendanceTracking | TIMBRATURE | Timbrature | Company, User |
| AttendanceTracking | GIUSTIFICATIVI | Giustificativi e causali | Company, User |
| AttendanceTracking | NOTESPESE | Note spese | FinancialAdvisor, Company, User |
| AttendanceTracking | BUGPRESENZE | Malfunzionamento Presenze | tutti |

Tutti i tipi iniziali hanno `AiResolution` = false: il SuperUser la attiva dalla pagina dei tipi quando servirà.

---

## 4. Livelli abilitati per tipo di richiesta

Non esiste un livello massimo. Per ogni tipo il SuperUser abilita i livelli **uno per uno** (`TicketTypeLevel`). Per esempio, "Timbrature" abilitato solo per Company e User resta interno all'azienda. Un tipo esiste e si muove **solo** nei livelli abilitati.

- **Chi può aprire:** un tipo è proposto in apertura solo se:
  - è attivo;
  - il livello di chi apre (il ruolo scelto, §2.1) è abilitato;
  - esiste **almeno un livello abilitato sopra** quello di chi apre, altrimenti nessuno potrebbe riceverlo. Per il SuperUser serve il livello DEV abilitato, perché il suo ticket scende (§2.2);
  - per i livelli User e Company, riguarda un applicativo **attivo per l'azienda** di chi apre (`CompanyApplication`). I livelli superiori vedono tutti i tipi.
- **Destinatario e inoltro:** il ticket va al **primo livello abilitato sopra** il livello corrente, saltando quelli non abilitati. Per esempio, con abilitati User e FinancialAdvisor, il ticket di un dipendente va direttamente al FinancialAdvisorPowerAdmin e il livello Company non lo vede. Il nodo si individua comunque dal percorso di origine (§3.2). "Inoltra" è disponibile solo se esiste un livello abilitato più in alto; al livello abilitato più alto restano Risolvi, Richiedi informazioni e Assegna. Il server ripete tutti i controlli.
- **Visibilità:** i livelli non abilitati non vedono il ticket, neanche nella vista "Dei livelli figli". Chi ha aperto lo vede sempre. Il SuperUser vede tutti i ticket (§2, regola 6).
- **Configurazione valida:** la pagina del SuperUser non salva un tipo che non permette a nessuno di aprire e a nessuno di ricevere, cioè con meno di due livelli abilitati.
- **Modifica dei livelli di un tipo già in uso:** vale per i passaggi successivi. Un ticket che si trova a un livello appena disabilitato resta lì finché non viene risolto o inoltrato; da quel momento segue la nuova configurazione.
- **Riclassificazione:** il livello che gestisce può cambiare il tipo, se è stato scelto male (azione `Riclassificazione` in cronologia). Il cambio è ammesso solo se il nuovo tipo abilita sia il livello di chi ha aperto sia il livello in cui si trova il ticket.

### 4.1 Risoluzione automatica AI (predisposizione)

Se il tipo ha `AiResolution` attivo, **prima** di passare il ticket al livello superiore si chiama un metodo di risoluzione automatica.

- **Quando:** in apertura e in ogni "Inoltra al padre", cioè in ogni passaggio verso l'alto. Il passaggio Dealer → DEV è compreso. **Esclusi** solo il passaggio DEV → SuperUser e i ticket aperti dal SuperUser, che scendono.
- **Esito positivo:** il ticket **non** passa al padre. Torna a chi l'ha aperto come **Risolto**, con una riga di cronologia di tipo `RisoluzioneAI`: testo della risposta, eventuali allegati, autore null, RoleId/UserId di chi ha aperto.
- **Esito negativo:** il passaggio prosegue normalmente verso il padre. Lo stesso vale se il metodo va in errore o va in timeout: l'errore si registra nel log ma non blocca mai il passaggio.
- **Dopo una risoluzione AI:** chi ha aperto chiude o riapre come per qualunque ticket risolto. Se riapre, il ticket va al livello che l'AI aveva "sostituito", e **non** si ritenta l'AI per quel passaggio.
- **Prima versione:** il metodo esiste ma **restituisce sempre "non risolto"**, quindi il ticket passa sempre al padre. Serve solo come punto di aggancio.
- **Forma del metodo:** un servizio dedicato (es. `TicketAiResolver`, derivato da `ServiceBase`, con l'interfaccia nello stesso file). Riceve il ticket, la cronologia, gli allegati e il livello di destinazione. Restituisce l'esito, il testo della risposta ed eventuali allegati. Così l'implementazione reale si potrà agganciare in seguito senza toccare `TicketService`, magari appoggiandosi al servizio `ChatAI` già presente.
- **Per il futuro:** una risposta AI reale può richiedere tempo. In quel caso conviene eseguirla in background (Hangfire) con uno stato temporaneo, invece di tenere ferma la richiesta del client. Nella prima versione la chiamata è sincrona, dato che restituisce subito.

---

## 5. Stati e azioni

**Stati:**

| Stato | Significato |
|---|---|
| Aperto | in attesa al livello corrente, senza assegnatario |
| InCarico | assegnato a un utente del livello corrente |
| InAttesaInfo | rimandato a chi ha aperto o a un livello intermedio per chiedere informazioni (§5.1) |
| Risolto | risposta data, visibilità a chi ha aperto |
| Chiuso | finale |

L'inoltro non è uno stato: cambia solo il livello e il ticket torna Aperto.

**Azioni:**

| Azione | Chi | Stato risultante | Effetto sulla cronologia |
|---|---|---|---|
| Apri | livelli 2–5 | Aperto | riga di origine (ruolo scelto e utente di chi apre) + allegati + riga al livello padre del ruolo scelto, senza assegnatario |
| Apri e assegna (§2.2) | SuperUser | InCarico | riga di origine (SuperUser) + allegati + riga al livello DEV con UserId = PowerAdmin o Admin scelto |
| Assegna / Riassegna | PowerAdmin del nodo corrente | InCarico | stesso RoleId, nuovo UserId (Admin o PowerAdmin attivi dello stesso nodo, anche sé stesso) |
| Inoltra al padre | PowerAdmin del nodo o Admin assegnatario | Aperto | RoleId del Power del primo livello abilitato sopra (§4), UserId null. Non ammesso se non ci sono livelli abilitati sopra, né sui ticket aperti dal SuperUser |
| Richiedi informazioni | PowerAdmin del nodo o Admin assegnatario | InAttesaInfo | RoleId/UserId del destinatario scelto (chi ha aperto o un livello intermedio, §5.1); ReturnRoleId/ReturnUserId = livello e utente che chiedono |
| Rispondi | il destinatario della richiesta (in InAttesaInfo) | InCarico (o Aperto se chi chiedeva non era assegnatario) | torna a ReturnRoleId/ReturnUserId |
| Aggiungi nota | chi ha il ticket in carico | invariato | solo testo/allegati |
| Risolvi | PowerAdmin del nodo o Admin assegnatario | Risolto | RoleId/UserId di chi ha aperto |
| Risoluzione AI (§4.1) | sistema, in apertura/inoltro | Risolto | riga `RisoluzioneAI` verso chi ha aperto; il ticket non sale. Nella prima versione non avviene mai |
| Riapri | chi ha aperto (in Risolto) | InCarico (Aperto dopo una risoluzione AI) | torna al livello e all'utente che avevano risolto; dopo una risoluzione AI va al livello destinatario, senza assegnatario e senza ritentare l'AI |
| Chiudi | chi ha aperto (in Risolto) | Chiuso | — |
| Riclassifica | PowerAdmin del nodo o Admin assegnatario | invariato | cambio di tipo (vedi §4) |

### 5.1 Richiesta di informazioni

**A chi si può chiedere.** L'elenco dei destinatari si ricava dalla cronologia del ticket:
- chi ha aperto il ticket, sempre;
- ogni **livello intermedio** da cui il ticket è passato, sotto il livello corrente. Si propone un livello per ogni passaggio.

Esempio: User apre → Company → FinancialAdvisor. Il FinancialAdvisor può chiedere a User (chi ha aperto) o a Company.

Per i ticket del SuperUser, il livello DEV può chiedere solo al SuperUser, perché non ci sono livelli intermedi.

**A quale utente del livello.** Per un livello intermedio la richiesta va a **chi aveva in carico il ticket a quel livello** (l'ultimo assegnatario trovato in cronologia). Chi chiede può anche scegliere di mandarla al livello senza assegnatario, cioè ai suoi PowerAdmin. Se l'assegnatario precedente non è più attivo, la richiesta va al livello senza assegnatario.

**Cosa può fare il destinatario.**
- Può **rispondere** con testo e allegati.
- Se è un PowerAdmin di un livello intermedio, può anche **assegnare** la richiesta a un altro utente del suo nodo.
- Non può risolvere, inoltrare, riclassificare né fare una nuova richiesta di informazioni: nella prima versione non sono ammesse richieste a catena. Se Company ha bisogno di un dato dal dipendente, lo dice nella risposta, e sarà il FinancialAdvisor a chiederlo a User.

**Risposta.** Il ticket torna a `ReturnRoleId` / `ReturnUserId` della richiesta, cioè al livello e all'utente che avevano chiesto. Il ritorno verso il livello che aveva chiesto **non** è un passaggio verso l'alto: la risoluzione AI (§4.1) non si attiva.

**Visibilità.** Mentre aspetta la risposta, il ticket compare nella vista "In carico a me" del destinatario. Il livello che ha chiesto lo vede in sola lettura, con l'indicazione "In attesa di informazioni da …".

**Concorrenza:** ogni azione invia la versione del ticket vista (`Version`); se nel frattempo il ticket è cambiato, il server rifiuta l'azione con un messaggio e il client ricarica il dettaglio.

---

## 6. Visibilità

Tutti i controlli si fanno **sul server**, nel servizio, compreso il download degli allegati. Il client non decide mai il livello: il `RoleId` arriva sempre dal ruolo reale dell'utente e dal contesto del token (coerente con `piano-security.md`).

Un utente con più ruoli vede l'**unione** di quanto previsto per ciascuno dei suoi ruoli. I ruoli si leggono sempre tutti, mai solo `roles[0]`.

| Chi | Cosa vede |
|---|---|
| User | i ticket che ha aperto |
| Admin di un nodo | i ticket che ha aperto + quelli **assegnati a lui** (correntemente o in passato, in sola lettura) |
| PowerAdmin di un nodo | i ticket che ha aperto o aperti dal suo nodo + quelli al livello del suo nodo + quelli dei nodi figli (sola lettura se non sono al suo livello) |
| PowerAdmin DEV | i ticket arrivati al livello DEV e tutti quelli dei livelli sottostanti |
| SuperUser | tutti (compresi quelli che ha aperto lui verso il livello DEV) |

**Viste dell'elenco** (con filtro per stato e per tipo):
1. **In carico a me** – vista iniziale. Per il PowerAdmin: ticket al livello del suo nodo, anche non assegnati. Per l'Admin: solo quelli assegnati a lui.
2. **Aperti da me** – i ticket aperti da me, visibili anche dopo l'inoltro per seguirne l'avanzamento.
3. **Dei livelli figli** – solo PowerAdmin e SuperUser, in sola lettura.

---

## 7. Notifiche SignalR (prima versione)

Il progetto ha già l'hub (`Hub/SignalRHub.cs`), l'invio mirato a un utente (`Clients.User(userId)`, usato da `JobNotifier`) e il servizio client `signalr.service.ts`.

| Evento | Destinatari |
|---|---|
| Ticket aperto o inoltrato a un livello | tutti i PowerAdmin attivi del nodo di destinazione |
| Assegnazione (anche quella diretta del SuperUser) | l'utente assegnatario; per i ticket del SuperUser anche i PowerAdmin DEV |
| Richiesta informazioni | il destinatario scelto: chi ha aperto, oppure l'utente o i PowerAdmin del livello intermedio |
| Risolto (anche dall'AI) | chi ha aperto il ticket; dopo una risoluzione AI nessuna notifica al padre |
| Risposta, Riapertura | l'utente / i PowerAdmin del livello che riceve il ticket |

Lato client:
- un messaggio a comparsa con il link al ticket;
- un **contatore** dei ticket "in carico a me" sulla voce di menu;
- se l'elenco o il dettaglio sono aperti, si aggiornano.

Le notifiche partono **dopo** il salvataggio, così un errore di invio non annulla il passaggio.

Nota sull'impersonificazione: i gruppi dell'hub usano `UserIdFirstConnection` (utente reale), mentre `Clients.User` usa l'utente del token. Il destinatario della notifica è l'utente della gerarchia (non chi lo sta impersonando).

---

## 8. Allegati

- Trasporto: base64 dentro il solito `GenericRequest` in POST, coerente con tutti gli altri endpoint.
- Limiti: 10 MB per file, numero massimo di file per passaggio (es. 5), elenco di estensioni ammesse (pdf, immagini, documenti Office, txt, zip).
- Controllare il limite di dimensione della richiesta sul server: il base64 aumenta il peso di circa il 33%.
- Download con un endpoint dedicato che verifica la visibilità del ticket.
- Lato client serve un **componente condiviso** per scegliere/mostrare gli allegati: oggi nel progetto non esiste nessun upload.

---

## 9. Casi particolari

- **CompanyMove:** se un'azienda passa a un altro FinancialAdvisor, i ticket non chiusi dell'azienda vanno riallineati (`IdFinancialAdvisor`, `IdDealer`) all'interno del servizio di spostamento. Se un ticket si trova al livello del vecchio FA o del vecchio Dealer, passa al nuovo nodo corrispondente, senza assegnatario.
- **Utenti disabilitati** (`Disabled` sui collegamenti `UserX`): non compaiono tra gli assegnatari e non ricevono notifiche. Se un assegnatario viene disabilitato, il ticket torna "senza assegnatario" al suo livello (verifica all'apertura del dettaglio o al momento della disabilitazione).
- **Utente collegato a più aziende:** il ticket si lega al contesto corrente del token.
- **Impersonificazione:** chi impersona opera come l'utente impersonato; l'utente reale (`UserIdFirstConnection`) si registra in `ChangeUser` per tracciabilità.
- **Cancellazione di aziende e utenti:** nessuna cascade sui ticket.

---

## 10. Pagine e componenti

| Pagina | Chi | Note |
|---|---|---|
| Elenco ticket | livelli 1–5, SuperUser | tre viste (§6), filtri per stato e tipo, `nvx-list`. Voce di menu nella seconda sezione del menu di ogni ruolo (Generale per l'azienda, Centro, Studio, Admin, SuperUser); il dipendente senza menu di ruolo la trova nella seconda sezione del menu utente se c'è (es. Presenze), altrimenti in Accesso. Accanto alla voce, il numero dei ticket in carico |
| Nuovo ticket | livelli 2–5, SuperUser | per il SuperUser al posto del ruolo di apertura c'è l'assegnatario obbligatorio (PowerAdmin o Admin, §2.2). Deriva da `BasePageConfirmCancelComponent`: ruolo di apertura (solo se l'utente ne ha più di uno, §2.1), tipo, oggetto, testo, allegati. Cambiando ruolo si aggiorna l'elenco dei tipi |
| Dettaglio ticket | chi può vederlo | cronologia (passaggi con testo e allegati) + pulsanti delle azioni consentite in quel momento + "Torna all'elenco" (sempre presente, non tocca il ticket), distinto da "Chiudi ticket" (chiusura definitiva, con avviso nel dialog) |
| Dialog azione | — | deriva da `BaseDialogConfirmCancelComponent`: testo + allegati (+ utente per l'assegnazione, + tipo per la riclassificazione, + destinatario per la richiesta di informazioni: chi ha aperto o un livello intermedio, con l'opzione "all'utente che l'aveva in carico" / "al livello") |
| Elenco tipi di richiesta | SuperUser | nel menu SuperUser (`Pages4SuperUser`), `RoleGuard4SuperUser` |
| Modifica tipo di richiesta | SuperUser | deriva da `BasePageConfirmCancelComponent`: applicativo, codice, descrizione, livelli abilitati (una casella per ognuno dei sei livelli, almeno due selezionati), risoluzione automatica AI (casella), ordine, attivo |

Da verificare: i pulsanti necessari (Assegna, Inoltra, Risolvi, Riapri, Chiudi, Richiedi informazioni) esistono già nella collezione di `UserInterfaceService`? Quelli mancanti vanno aggiunti lì.

Tutte le etichette nuove vanno nei file di traduzione `assets/i18n/infrastructure/it.json` ed `en.json`; i messaggi del server in `InfrastructureMessages.resx` / `.en.resx`. Stati e azioni viaggiano come codici e sono tradotti sul client.

---

## 11. Dove andranno le modifiche (tutto in Infrastructure)

**nvxapp.server.data**
- `Entities/Public`: `TicketType`, `TicketTypeLevel`, `Ticket`, `TicketCronology`, `TicketAttach` + enum di stato e azione
- `Infrastructure/PublicDbContext_Infrastructure.cs`: DbSet e configurazione (indici, chiavi esterne senza cascade)
- `Repositories/Public`: un repository per entità (interfaccia nello stesso file)
- `Migrations/Public`: nuova migration con i tipi iniziali
- `Localization/InfrastructureMessages*.resx`: messaggi

**nvxapp.server.service**
- `ClientServer-Service/infrastructure/SupportTicket` (non `Ticket`: il namespace andrebbe in conflitto con l'entità): `TicketService`, `TicketTypeService`, `TicketRules`, `TicketNotifier` + `Models`
- `ClientServer-Service/infrastructure/SupportTicket`: `TicketAiResolverService`, il metodo di risoluzione automatica predisposto che restituisce sempre "non risolto" (§4.1)
- `ClientServer-Service/infrastructure/TicketType`: `TicketTypeService` + `Models`
- `Mappers/Public`: profili AutoMapper
- `ClientServer-Service/infrastructure/CompanyMove`: riallineamento dei ticket
- notifiche: un notifier sul modello di `JobNotifier`

**nvxapp.server**
- `Controllers/Infrastructure/TicketController.cs` (solo POST): `TicketList`, `TicketCount`, `TicketGet` (con le azioni consentite, gli assegnatari e i destinatari), `TicketOpenInfo`, `TicketOpen`, `TicketExecute` (un solo endpoint per tutti i passaggi: assegnazione, inoltro, richiesta informazioni, risposta, nota, risoluzione, riapertura, chiusura, riclassificazione), `TicketAttachGet`
- `Controllers/Infrastructure/TicketTypeController.cs`: elenco, dettaglio, salvataggio

**nvxapp.client**
- `ClientServer-Service/Infrastructure/Ticket` e `TicketType`: servizi e modelli
- `pages/infrastructure`: le pagine del §10
- `shared/components`: componente allegati
- `Utility/infrastructure`: route, menu, contatore e gestione dei messaggi SignalR
- `assets/i18n/infrastructure`: traduzioni

---

## 12. Da fare in un secondo tempo

- **Chiusura automatica** dei ticket Risolti dopo N giorni senza riapertura (job Hangfire giornaliero, N configurabile in `AppSetting`).
- Notifiche anche via email (il servizio `EmailService` esiste già).
- Statistiche: tempi medi di risoluzione per livello e per tipo.
