# Analisi – Documentazione per le risposte AI ai ticket

Branch di sviluppo: **Infrastructure_Dev** (funzione condivisa: tabelle in `public`, cartelle `Infrastructure`).
Documento di sola analisi: nessun codice è stato scritto.
Riferimento: `nvxapp.server.service/ANALISI_TICKET.md` (livelli, passaggi, §4.1 risoluzione AI).

---

## 1. Obiettivo

1. Ai livelli **SuperUser, Dealer, FinancialAdvisor e Company** si carica della documentazione, con descrizione e contenuto, facoltativamente legata a uno o più **tipi di richiesta** (`TicketType`).
2. Quando un ticket arriva a un nuovo livello (apertura o inoltro), si cercano i documenti di **quel nodo** sull'argomento del ticket. Se ce ne sono, si passano all'AI insieme al testo della domanda. Con la risposta si genera una **risposta automatica** a chi ha aperto.
3. Ogni nodo personalizza, per ogni tipo di richiesta, **se** usare l'AI e **quante volte** tentare prima di passare all'operatore. Il campo `TicketType.AiResolution` viene eliminato.
4. Il ticket conta le risposte AI date al livello corrente. Raggiunto il limite, a quel livello il ticket non va più all'AI ma all'operatore.

Oggi esiste solo la predisposizione: `TicketAiResolverService.TicketAiResolve` risponde sempre "non risolto"; `TicketService.SalitaRows` la chiama se `TicketType.AiResolution` è attivo e il passaggio non è DEV → SuperUser.

---

## 2. Scelta 1: una tabella per livello o una tabella unica?

| | 4 tabelle (una per nodo) | **Tabella unica con i riferimenti al nodo** |
|---|---|---|
| Struttura | `DealerDocument`, `FinancialAdvisorDocument`, `CompanyDocument`, + una "globale" senza padre per il SuperUser | una tabella con `Level` + `IdDealer` / `IdFinancialAdvisor` / `IdCompany` (tutti null per il SuperUser) |
| Integrità | FK "vera" e obbligatoria verso il padre | FK facoltative; serve un **vincolo di controllo** (vedi sotto) |
| Codice | 4 entità, 4 repository, 4 mapping, query in `UNION` per la ricerca | un'entità, un repository, una pagina, una query |
| Pagina client | stessa pagina con 4 servizi diversi, o 4 pagine | una pagina sola: il nodo arriva dal contesto dell'utente |
| Coerenza col progetto | — | è lo schema già usato da `Ticket` (percorso `IdDealer` / `IdFinancialAdvisor` / `IdCompany`) e dalla visibilità "dei livelli figli" |

**Raccomandazione: tabella unica.** I quattro casi hanno gli stessi campi e lo stesso comportamento; cambia solo il "proprietario". Con quattro tabelle si moltiplica il codice senza guadagno funzionale, e il caso SuperUser (senza padre) resta comunque un'eccezione da gestire.

Il punto debole della tabella unica (l'integrità) si copre così:
- FK reali (nullable) verso `Dealer`, `FinancialAdvisor`, `Company`, **senza navigation sul lato padre**: è la regola già seguita per i ticket, perché i DbContext degli applicativi mappano `Company` e scoprirebbero la tabella seguendo le relazioni.
- Un **check constraint** che lega il livello ai campi valorizzati:
  - `Level = SuperUser` → tutti e tre null;
  - `Level = Dealer` → solo `IdDealer`;
  - `Level = FinancialAdvisor` → solo `IdFinancialAdvisor`;
  - `Level = Company` → solo `IdCompany`.

  A differenza di `Ticket` si salva **solo** l'id del nodo proprietario, non tutto il percorso: se l'azienda passa a un altro studio (`CompanyMove`) i documenti la seguono senza riallineamenti.
- Cancellazione del nodo → cascade sui suoi documenti (sono "suoi", non c'è storia da conservare). Vedi anche `ANALISI_CANCELLAZIONE.md`.

---

## 3. Scelta 2: struttura dedicata o tabella "documenti" generica?

Due strade:
- **A – dedicata:** `TicketAiDocument`, pensata solo per le risposte AI.
- **B – generica:** `Document` con un campo `Section` (enum) che dice a cosa serve il documento. Oggi un solo valore (`TicketAi`), domani altri (manuali da scaricare, informative, modelli…).

**Raccomandazione: B, generica, ma solo nella parte "contenitore".**
- In `Document` vanno i campi che ogni documento ha comunque: proprietario (livello + nodo), sezione, titolo, descrizione, contenuto, file originale, attivo, ordine.
- Ciò che è **specifico** della sezione va in tabelle figlie dedicate. Per l'AI: il legame con i tipi di richiesta (`DocumentTicketType`). Così la tabella generica non si riempie di colonne valide solo per una sezione.
- Costo rispetto ad A: quasi nullo (un campo e un filtro in più). Vantaggio: il caricamento, l'estrazione del testo, la pagina di gestione e la cancellazione si riusano per ogni uso futuro.
- Rischio da evitare: non usare `Section` per mettere in tabella cose che non sono documenti. Se un uso futuro ha regole molto diverse, merita la sua tabella.

Se invece non si prevede **nessun** altro uso concreto, A è altrettanto valida. La differenza di lavoro oggi è minima, quindi conviene B.

---

## 4. Modello dati

Tutte le tabelle in `public` (`PublicDbContext_Infrastructure`), entità in `nvxapp.server.data\Entities\Public`, derivate da `BaseEntity`.

### 4.1 Document

| Campo | Tipo | Note |
|---|---|---|
| Id | int | |
| Section | enum `DocumentSection` | `TicketAi = 0` (altri in futuro). I valori finiscono nel database: non si cambiano |
| Level | `TicketLevel` | SuperUser, Dealer, FinancialAdvisor, Company. Si riusa l'enum dei ticket (vedi §9, DEV) |
| IdDealer / IdFinancialAdvisor / IdCompany | int? | proprietario, con check constraint (§2) |
| Titolo | string(200) | mostrato nell'elenco |
| Descrizione | string(1000)? | a cosa serve; **va anche all'AI** per scegliere il documento |
| Contenuto | text | **testo** del documento: è quello che si manda all'AI |
| FileName / ContentType / Size / File | facoltativi | file originale caricato (pdf, docx, txt, md). Come per `TicketAttach`, `File` si legge solo dal download |
| ApplicationType | `ApplicationType?` | facoltativo, come su `TicketType` (null = Generale): filtro grossolano utile quando non si indicano i tipi |
| IncludeSubLevels | bool | vedi §6.2: il documento vale anche per i nodi sotto |
| Ordine | int | priorità quando i documenti superano il limite di testo (§7.3) |
| Active | bool | i documenti disattivati non vanno all'AI ma restano |

Indice su (`Section`, `Level`, `IdDealer`, `IdFinancialAdvisor`, `IdCompany`, `Active`).

**Contenuto testuale.** All'AI va testo, non file. Due modi di riempire `Contenuto`, entrambi da prevedere:
1. scritto/incollato nella pagina (editor di testo semplice o Markdown);
2. estratto dal file caricato: pdf (es. PdfPig), docx (OpenXml), txt/md (diretto). L'estrazione avviene al salvataggio; l'utente vede il testo estratto e può correggerlo. I PDF scansionati (immagini) non hanno testo: si segnala e si chiede di incollarlo (in seguito si potrebbe usare il modello con visione già gestito da `LlmClient`).

Limite consigliato per documento: ~100.000 caratteri, configurabile.

### 4.2 DocumentTicketType (argomenti del documento, sezione TicketAi)

| Campo | Note |
|---|---|
| IdDocument | FK, cascade |
| IdTicketType | FK, cascade (eliminare un tipo toglie solo il legame) |

Univoco per coppia. **Nessuna riga = il documento vale per tutti i tipi** (documentazione generale del nodo). La pagina propone solo i tipi abilitati al livello del nodo (`TicketTypeLevel`).

### 4.3 TicketTypeAiSetting (personalizzazione dei tipi per nodo)

Sostituisce `TicketType.AiResolution`. Stessa forma "livello + nodo" di `Document`.

| Campo | Note |
|---|---|
| IdTicketType | FK, cascade |
| Level | livello a cui si applica |
| IdDealer / IdFinancialAdvisor / IdCompany | nodo; **tutti null = valore predefinito del livello**, deciso dal SuperUser |
| AiEnabled | bool: a questo livello si tenta la risposta AI |
| AiMaxAttempts | int (≥ 1): risposte AI al massimo, a questo livello, prima dell'operatore |

Univoco per (`IdTicketType`, `Level`, `IdDealer`, `IdFinancialAdvisor`, `IdCompany`) — attenzione: in PostgreSQL i null non sono uguali tra loro in un indice univoco, quindi serve `NULLS NOT DISTINCT` (PG 15+) o un indice parziale per le righe di default.

**Impostazione effettiva** per un ticket che arriva al nodo N di livello L:
1. riga del nodo N, se c'è;
2. altrimenti riga di default del livello L (nodo null), definita dal SuperUser;
3. altrimenti AI disattivata.

Così il SuperUser può dire "per BUG, a livello Company, AI attiva con 2 tentativi" una volta per tutte le aziende, e la singola azienda può cambiarlo.

### 4.4 Ticket: contatore delle risposte AI

Campi nuovi su `Ticket`:

| Campo | Note |
|---|---|
| AiAnswerCount | int: tentativi AI fatti **al livello corrente** (con o senza risposta, decisione 4 del §9) |
| AiLevel | `TicketLevel?`: il livello a cui si riferisce il contatore |
| AiPendingSince | `DateTime?`: risposta AI in corso dal…; null = nessuna (§5.3) |

Il contatore si **azzera** quando il ticket sale a un nuovo livello (il limite è per livello: "non verrà più sottoposta ad AI **su quel livello**"). Lo storico completo resta nella cronologia (righe `RisoluzioneAI`), da cui il contatore si potrebbe anche ricalcolare: il campo è una copia di comodo, come i `Current*`.

Se si vuole anche il totale complessivo (statistiche), si aggiunge `AiAnswerTotal`, che non si azzera.

### 4.5 TicketCronology: documenti usati

Sulle righe `RisoluzioneAI` conviene sapere **quali documenti** ha usato l'AI (per capire perché ha risposto così e migliorare la documentazione). Tabella figlia `TicketCronologyDocument` (IdTicketCronology, IdDocument nullable con `SET NULL`, Titolo copiato) oppure, più semplice, un campo testo con i titoli. Consigliata la tabella: il titolo copiato resta leggibile anche se il documento viene eliminato.

### 4.6 TicketType

- Si elimina `AiResolution` (migration, entità, `TicketTypeModel`, `PopolateTicketType`, pagina di modifica del tipo).
- Migrazione dei dati: per i tipi che oggi hanno `AiResolution = true` si creano le righe di default (§4.3) per i livelli abilitati, con `AiMaxAttempts = 1`. Oggi i tipi iniziali hanno tutti `false`, quindi in pratica non si crea nulla.

---

## 5. Flusso

### 5.1 Arrivo a un nuovo livello (apertura o inoltro)

Al posto del controllo attuale `type.Type.AiResolution` in `SalitaRows`:

1. Livello di destinazione L e nodo N (dal percorso di origine del ticket, come oggi).
2. Esclusioni invariate: nessuna AI nel passaggio DEV → SuperUser né sui ticket aperti dal SuperUser.
3. Impostazione effettiva (§4.3). Se `AiEnabled` è falso → operatore.
4. Contatore: se `AiLevel = L` e `AiAnswerCount >= AiMaxAttempts` → operatore. Se `AiLevel ≠ L` si azzera e `AiLevel = L`.
5. Documenti (§6): se **non ce ne sono** → operatore, **senza** chiamare l'AI (risposta senza fonti = rischio di risposte inventate).
6. Chiamata all'AI (§7) con domanda, cronologia utile e documenti.
7. Esito:
   - **risposta trovata** → riga `RisoluzioneAI` verso chi ha aperto (stato Risolto, autore null, `ReturnLevel = L` come oggi), `AiAnswerCount + 1`, notifica a chi ha aperto. Il ticket **non** arriva agli operatori di L;
   - **l'AI dichiara che i documenti non coprono la domanda** → il ticket resta a L, Aperto, per l'operatore, con una riga di cronologia `RisoluzioneAINonTrovata` (autore null). **Il tentativo conta** (`AiAnswerCount + 1`, decisione 4).
   - **errore o timeout** → come sopra ma senza riga di cronologia e senza consumare il tentativo (nessuna risposta è stata data): si registra nel log e non blocca mai il passaggio.

### 5.2 Riapertura dopo una risposta AI (il "ritentativo")

Oggi la riapertura dopo `RisoluzioneAI` porta il ticket al livello sostituito **senza** ritentare l'AI. Con i tentativi multipli:

- chi ha aperto riapre scrivendo **perché** la risposta non va bene (testo obbligatorio in questo caso);
- se `AiAnswerCount < AiMaxAttempts` e ci sono ancora documenti → nuovo tentativo AI, che riceve anche la risposta precedente e l'obiezione di chi ha aperto;
- altrimenti → il ticket va al livello L per l'operatore, senza assegnatario (comportamento attuale).

Esempio con `AiMaxAttempts = 2` a livello Company: User apre → AI risponde (1) → User riapre → AI risponde (2) → User riapre → va al CompanyPowerAdmin. Se poi Company inoltra al FinancialAdvisor, il contatore riparte da 0 per quel livello.

Il secondo tentativo ha senso perché riceve l'obiezione di chi ha aperto. Valore predefinito della pagina: `AiMaxAttempts = 1`.

### 5.3 Esecuzione in background (necessaria)

Una chiamata reale al LLM dura da qualche secondo a oltre un minuto (`AI:TimeoutSeconds`). Non si può tenere ferma l'apertura del ticket, né la transazione del salvataggio.

1. Il passaggio si salva subito: ticket al livello L, Aperto, con **`AiPendingSince`** valorizzato e una riga di cronologia normale (`Inoltro`, o `Riapertura` per un nuovo tentativo).
2. Dopo il salvataggio si avvia il lavoro con `ServiceBase.RunInBackground` (il meccanismo già usato dal progetto: scope nuovo e contesto utente propagato), passando id ticket e `Version`.
3. Il lavoro rilegge il ticket; se la `Version` è cambiata (qualcuno ha già agito) non fa nulla; altrimenti chiama l'AI e registra l'esito (§5.1, punto 7), azzera `AiPendingSince` e notifica via `TicketNotifier`.
4. Mentre la risposta è in corso, gli operatori di L vedono il ticket con l'etichetta "risposta automatica in corso" e **non** ricevono la notifica di nuovo ticket (arriva solo se l'AI non risponde). **Possono comunque prenderlo in carico** (decisione 5): ogni azione azzera `AiPendingSince` e cambia la `Version`, e il lavoro in background si ferma.
5. Sicurezza: se il lavoro non termina (server riavviato), dopo `AI:TimeoutSeconds` + 2 minuti l'indicatore è considerato scaduto e il ticket si comporta come un ticket normale. Il riavvio del server perde il lavoro in corso: è il limite accettato di `RunInBackground`, che non ha una coda persistente.

Chi ha aperto vede subito il ticket inviato e, poco dopo, la risposta automatica (notifica SignalR + aggiornamento della pagina, come già avviene).

---

## 6. Quali documenti

### 6.1 Ricerca

Documenti con `Section = TicketAi`, `Active`, del nodo N di destinazione, e:
- legati al tipo del ticket (`DocumentTicketType`), **oppure** senza nessun tipo (generali);
- con `ApplicationType` null o uguale a quello del tipo.

L'AI risponde nella lingua della domanda, qualunque sia la lingua dei documenti (il campo Lingua del documento è rimandato).

Per il passaggio **Dealer → DEV** (unico passaggio verso un livello globale con AI) si usano i documenti del **SuperUser** (livello globale, nodo null). Vedi §9.

### 6.2 Ereditarietà (proposta)

La richiesta dice "si cerca se **in company** ci sono documenti". Ma ci sono documenti che vale la pena scrivere una volta sola: il SuperUser che carica il manuale dell'applicativo Presenze dovrebbe servire anche a ogni azienda, altrimenti ognuna lo deve ricaricare.

Proposta: il flag `IncludeSubLevels`. Un documento con il flag vale anche per i nodi sotto: SuperUser → tutti; Dealer → i suoi FA e le loro aziende; FA → le sue aziende. La ricerca allora prende i documenti del nodo N **più** quelli con il flag sul percorso sopra N. Default: falso (comportamento della richiesta).

### 6.3 Chi li gestisce

| Livello | Chi modifica | Chi vede l'elenco |
|---|---|---|
| SuperUser | SuperUser | SuperUser |
| Dealer | DealerPowerAdmin | + DealerAdmin, in sola lettura |
| FinancialAdvisor | FinancialAdvisorPowerAdmin | + FinancialAdvisorAdmin, in sola lettura |
| Company | CompanyPowerAdmin | + CompanyAdmin, in sola lettura |

Il nodo è **sempre** quello del token (`CurrentDealer`, `CurrentFinancialAdvisor`, `CurrentCompany`), mai un id mandato dal client. Utenti con più ruoli: valgono i permessi del ruolo più alto (`multi-role`, mai `roles[0]`). Il SuperUser può anche consultare (sola lettura) i documenti dei nodi, utile per il supporto.

Domanda aperta: i PowerAdmin di un nodo devono vedere i documenti dei nodi figli (come per i ticket)? Proposta: no nella prima versione.

---

## 7. Chiamata all'AI

### 7.1 Servizio

Si implementa `TicketAiResolverService.TicketAiResolve` (oggi stub), usando `ILlmClient.CompleteJsonAsync`, come già fanno l'import dipendenti e l'export. Provider facoltativo dedicato `AI:TicketLLM` (vuoto = `AI:UseLLM`), sul modello di `AI:ExportLLM`.

### 7.2 Prompt

- **Sistema:** sei l'assistenza di <applicativo>; rispondi **solo** con le informazioni dei documenti forniti; se i documenti non contengono la risposta dichiaralo (`resolved = false`) e non inventare; rispondi nella lingua di chi ha scritto; niente dati che non siano nei documenti o nella domanda.
- **Utente:** tipo di richiesta, oggetto, testo, eventuali risposte AI precedenti e obiezioni (riaperture), poi i documenti (titolo, descrizione, contenuto), ognuno con un identificativo.
- **Schema della risposta (JSON):** `resolved` (bool), `answer` (testo per chi ha aperto), `usedDocumentIds` (array), facoltativo `confidence`.

Gli allegati del ticket nella prima versione **non** si mandano (solo il testo).

### 7.3 Limiti

- Budget di testo dei documenti per chiamata (es. `AI:TicketAi:MaxDocumentChars`, default ~60.000 caratteri): oltre, si prendono i documenti per `Ordine` (prima quelli legati al tipo, poi i generali) e si tronca l'ultimo.
- Prima versione **senza** ricerca semantica: si mandano i documenti filtrati per nodo e tipo. Se la documentazione crescerà molto, il passo successivo è dividere i documenti in parti con embedding (pgvector su PostgreSQL) e mandare solo le parti pertinenti. Il modello dati del §4 non cambia: si aggiunge una tabella figlia delle parti.

### 7.4 Riservatezza

Testo del ticket e documenti dell'azienda escono verso il provider LLM esterno. Va detto nella pagina di caricamento ("il contenuto viene inviato al servizio AI per le risposte automatiche") e tenuto presente nel contratto con i clienti. Con `AI_local` (Ollama) restano in casa.

---

## 8. Pagine e menu

| Pagina | Chi | Note |
|---|---|---|
| **Documentazione AI** – elenco | SuperUser, Dealer, FA, Company (PowerAdmin e Admin) | voce di menu **subito sotto "Assistenza"** (`TicketListPageComponent`) nel menu di ogni ruolo; `nvx-list` con titolo, tipi collegati, attivo, ordine |
| Documentazione AI – modifica | PowerAdmin del nodo, SuperUser | deriva da `BasePageConfirmCancelComponent`: titolo, descrizione, tipi di richiesta (multi-selezione tra quelli abilitati al livello; nessuno = tutti), lingua, "vale anche per i livelli sotto", file (con estrazione del testo), contenuto, ordine, attivo |
| **Impostazioni AI dei tipi di richiesta** | PowerAdmin del nodo; SuperUser per i default | elenco dei tipi abilitati al livello con: AI attiva, numero di tentativi, e se la riga è "personalizzata" o "predefinita". Comando "torna al predefinito" (cancella la riga del nodo). Proposta: seconda voce nello stesso gruppo, oppure scheda nella pagina della documentazione |
| Modifica tipo di richiesta (esistente) | SuperUser | si toglie la casella "risoluzione automatica AI" |
| Dettaglio ticket (esistente) | | le righe `RisoluzioneAI` mostrano "Risposta automatica" e, a PowerAdmin e SuperUser, i documenti usati; etichetta "risposta automatica in corso" con `AiPendingSince` |

Componente allegati: il caricamento del file riusa quello già fatto per i ticket.

Traduzioni in `assets/i18n/infrastructure/it.json` / `en.json`, messaggi del server in `InfrastructureMessages*.resx`.

---

## 9. Decisioni (30/10/2026)

1. **Livello DEV:** nel passaggio verso il DEV si usano i documenti del **SuperUser**. Le impostazioni AI del livello DEV sono quelle di default decise dal SuperUser (il DEV non ha nodi).
2. **Ereditarietà:** sì, con il flag `IncludeSubLevels` (§6.2).
3. **Livello User:** l'AI agisce all'arrivo al livello Company (e ai livelli sopra); il dipendente non carica documenti.
4. **Tentativo non risolto:** **conta** come tentativo (riga `RisoluzioneAINonTrovata` in cronologia). Errori e timeout non contano.
5. **Operatori durante la risposta AI:** possono prendere il ticket; la risposta AI in corso viene abbandonata.
6. **Tabella generica** `Document` con `Section`.

Decisioni tecniche prese nell'implementazione:
- il lavoro in background usa `RunInBackground` (già presente) e non Hangfire (§5.3);
- il campo Lingua del documento è rimandato: l'AI risponde nella lingua della domanda;
- l'estrazione del testo dai file passa a un helper comune `Helpers/FileText` (stessa logica di `ImportFileText` delle presenze, che in seguito potrà usarlo);
- i documenti usati da una risposta AI si vedono nel dettaglio del ticket da tutti tranne chi ha aperto.

---

## 10. Dove andranno le modifiche (tutto in Infrastructure)

**nvxapp.server.data**
- `Entities/Public`: `Document`, `DocumentTicketType`, `TicketTypeAiSetting`, `TicketCronologyDocument`, enum `DocumentSection`, azione `RisoluzioneAINonTrovata`; `Ticket` (+`AiAnswerCount`, `AiLevel`, `AiPendingSince`); `TicketType` (−`AiResolution`)
- `Infrastructure/PublicDbContext_Infrastructure.cs`: DbSet, check constraint, indici, FK senza navigation sui padri
- `Repositories/Public`: un repository per entità
- `Migrations/Public`: una migration (con il passaggio dei dati di `AiResolution`); aggiornare `PopolateTicketType`

**nvxapp.server.service**
- `ClientServer-Service/infrastructure/Document`: `DocumentService` (elenco, dettaglio, salvataggio, eliminazione, estrazione del testo, download) + `Models`
- `ClientServer-Service/infrastructure/SupportTicket`: `TicketTypeAiSettingService`; `TicketAiResolverService` reale; `TicketService` (`SalitaRows`, riapertura, lavoro in background, contatore); `TicketRules` (impostazione effettiva); `TicketNotifier`
- `ChatAI/LlmClient`: nessuna modifica prevista (si usa il parametro `provider`)
- `Mappers/Public`

**nvxapp.server**
- `Controllers/Infrastructure/DocumentController.cs`, `TicketTypeAiSettingController.cs` (solo POST)
- `appsettings.json`: `AI:TicketLLM`, `AI:TicketAi:MaxDocumentChars`

**nvxapp.client**
- `ClientServer-Service/Infrastructure/Document`, `TicketTypeAiSetting`
- `pages/infrastructure`: pagine del §8; modifica di `ticket-type-edit-page` e del dettaglio ticket
- `Utility/infrastructure`: route e voci di menu sotto "Assistenza"
- `assets/i18n/infrastructure`

**Documentazione:** aggiornare `ANALISI_TICKET.md` §3.1, §3.5, §4.1, §5 (riapertura) quando l'analisi è approvata.

---

## 11. Checklist dei lavori

| Sezione | Lavori | Stato |
|---|---|---|
| 11.1 Dati | 1–6 | fatto |
| 11.2 Server: documenti e impostazioni | 7–12 | fatto |
| 11.3 Server: flusso AI dei ticket | 13–19 | fatto |
| 11.4 Client | 20–27 | da fare |
| 11.5 Verifica | 28–30 | da fare |

### 11.1 Dati
- [x] 1. Entità `Document`, `DocumentTicketType`, enum `DocumentSection`
- [x] 2. Entità `TicketTypeAiSetting`, `TicketCronologyDocument`
- [x] 3. `Ticket`: `AiAnswerCount`, `AiLevel`, `AiPendingSince`; `TicketAction.RisoluzioneAINonTrovata`
- [x] 4. `TicketType`: tolto `AiResolution`
- [x] 5. `PublicDbContext_Infrastructure`: tabelle, check constraint, indici, FK senza navigation sui nodi; repository — unicità delle impostazioni con indici parziali: `NULLS NOT DISTINCT` richiede PostgreSQL 15
- [x] 6. Migration (con il passaggio di `AiResolution` alle impostazioni di default) — `20261030..._TicketAiDocuments`

### 11.2 Server: documenti e impostazioni
- [x] 7. `Helpers/FileText` (estrazione del testo) + pacchetto PdfPig
- [x] 8. Livelli gestibili dall'utente (`Documentation/LevelScope`): SuperUser, Dealer, FinancialAdvisor, Company; modifica solo PowerAdmin e SuperUser
- [x] 9. `DocumentService` + `DocumentController`: elenco, dettaglio, salvataggio, eliminazione, testo dal file, download (cartella `Documentation`: `Document` andrebbe in conflitto con l'entità)
- [x] 10. `TicketTypeAiSettingService` + controller: elenco con valori effettivi e di default, salvataggio, ritorno al default
- [x] 11. `TicketTypeService`, modelli e mapper senza `AiResolution`
- [x] 12. Messaggi `InfrastructureMessages` it/en, `appsettings` (`AI:TicketLLM`, `AI:TicketMaxChars`, `AI:TicketMaxTokens`)

### 11.3 Server: flusso AI dei ticket
- [x] 13. Impostazione effettiva e documenti per livello di destinazione, con ereditarietà (`TicketAiKnowledge`)
- [x] 14. `SalitaRows`: contatore per livello, `AiPendingSince` al posto della chiamata sincrona
- [x] 15. Lavoro in background (`TicketAiRun`): versione, esito, cronologia, documenti usati, notifiche
- [x] 16. `TicketAiResolverService` reale con `LlmClient` (prompt, schema JSON, budget dei documenti)
- [x] 17. Riapertura dopo una risposta AI: nuovo tentativo se il limite lo consente
- [x] 18. Ogni azione azzera la risposta in corso; nessuna notifica agli operatori mentre è in corso
- [x] 19. Dettaglio: `AiPending`, documenti usati sulle righe AI

### 11.4 Client
- [ ] 20. Servizi e modelli `Document`, `TicketTypeAiSetting`
- [ ] 21. Pagina elenco documentazione AI (scelta del livello se l'utente ne ha più di uno)
- [ ] 22. Pagina modifica documento (tipi, flag livelli sotto, file con estrazione del testo, contenuto)
- [ ] 23. Pagina impostazioni AI dei tipi di richiesta
- [ ] 24. Route e voci di menu sotto "Assistenza" (SuperUser, Dealer, FinancialAdvisor, Company)
- [ ] 25. Tipi di richiesta: tolti colonna e casella AI
- [ ] 26. Dettaglio ticket: "risposta automatica in corso", azione `RisoluzioneAINonTrovata`, documenti usati
- [ ] 27. Traduzioni it/en

### 11.5 Verifica
- [ ] 28. Build server e client
- [x] 29. Migration applicata al database di sviluppo (30/10)
- [ ] 30. Prova del flusso con il server di prova (apertura → risposta AI, riapertura → secondo tentativo, limite → operatore)

---

## 12. Lavori fatti

| Data | Sezione | Lavori | Ramo | Commit |
|---|---|---|---|---|
| 30/10/2026 | 11.1 | 1–6, 29 | Infrastructure_Dev | b5dabcd5 |
| 30/10/2026 | 11.2 | 7–12 | Infrastructure_Dev | 303597d2 |
