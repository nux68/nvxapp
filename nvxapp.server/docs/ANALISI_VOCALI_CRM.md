# Analisi: comandi vocali (chat AI) per il CRM

> Analisi dell'11/11/2026, branch `CRM_Dev` (lavoro su `CRM_Dev_vocali`, worktree `wt_crm_vocali`).
> Documento gemello: `ANALISI_VOCALI_DOCUMENTALE.md` (branch DocumentManagement). Il motore della chat e la voce
> sono descritti in `ANALISI_CHAT_AI.md`.
>
> **Obiettivo**: portare al CRM i comandi in linguaggio naturale (chat e voce) che oggi esistono solo per le
> presenze. `ANALISI_CRM.md` §6.3 e §10 li prevedevano ("apri il cliente X", "attività di oggi",
> "opportunità in chiusura questo mese", creazione di un'attività a voce).

Legenda: ✅ svolto · ⏳ da fare · 🔍 da valutare.

## 1. Situazione di partenza

- Il motore (`ChatAIService`, catalogo intent, normalizzatori di date e orari, voce nel client) è in
  Infrastructure ed è già presente su `CRM_Dev`. Un nuovo comando è una classe `ICommandHandler`: la DI la
  registra da sola (scanner dell'assembly in `Installers.cs`), come `IOpenableEntity` e `IRouteProvider`.
- La voce (riconoscimento, sintesi, conferma vocale) è tutta nel client e vale per qualsiasi comando: per il
  CRM non servono modifiche al client.
- Sul ramo CRM la cartella `ChatAiHandler/` contiene solo i comandi comuni (Help, Navigate, OpenEntity, Tema,
  Lingua): **nessun comando CRM**.
- Il motore filtra i comandi solo per ruolo: in "Applicativo completo" un'azienda senza CRM vedrebbe i comandi
  CRM (e la loro esecuzione fallirebbe con `ApplicationNotActiveException`). Il prompt di `Navigate` elenca a
  mano le pagine di Infrastructure e presenze, non quelle del CRM.

## 2. Prerequisiti nel motore (Infrastructure_Dev)

| # | Intervento | Stato |
|---|---|---|
| I1 | `IntentDefinition.Application` e `IOpenableEntity.Application` (`ApplicationType?`): il comando o l'entità compare solo se l'applicativo è attivo per l'azienda (`ICompanyApplicationRepository.ActiveApplications`, claim `company`). `null` = comune. `ChatAIService` filtra il catalogo (`IIntentCatalog.Filter4Applications`) e passa gli applicativi a `IChatUserContext` (`Applications`, `HasApplication`); `OpenEntityHandler` cerca solo tra le entità degli applicativi attivi. Senza azienda (centro, studio, superuser) restano i comandi comuni | ✅ |
| I2 | `IRouteProvider.PromptPages`: ogni modulo dichiara i nomi delle proprie pagine, che `NavigateHandler` aggiunge al prompt ("portami alla pipeline") | ✅ |

## 3. Comandi CRM

Cartella `nvxapp.server.service/ClientServer-Service/ChatAiHandler/Crm/`. Tutti con `Application = Crm`,
ruoli `CompanyPowerAdmin`, `CompanyAdmin`, `User` (come il menu del CRM). La visibilità dei dati resta nei
servizi (`CrmUtility.*Visibili`): con visibilità "solo propri" l'utente vede dalla chat esattamente ciò che
vede nelle pagine. Testi nella lingua dell'utente in `CrmMessages*.resx` (chiavi `Chat*`).

### 3.1 Interrogazioni (sola lettura, nessuna conferma)

| # | Intent | Esempi | Servizio |
|---|---|---|---|
| R1 | `CrmAgenda` | "cosa ho in agenda oggi?", "attività di domani", "appuntamenti della prossima settimana" | `Crm_AttivitaList` (mie, da fare; con il periodo che parte da oggi anche le scadute) |
| R2 | `CrmOpportunities` | "opportunità in chiusura questo mese", "trattative aperte con Rossi" | `Crm_OpportunitaList` (aperte, filtro su chiusura prevista e cliente) |
| R3 | `CrmPipeline` | "com'è la pipeline?", "quanto vale la pipeline?" | `Crm_Report` (pipeline per fase: numero, valore, valore pesato; vinte e perse del mese) |
| R4 | `CrmCustomerInfo` | "dimmi del cliente Rossi", "che telefono ha Bianchi srl?" | `Crm_ClienteList` (stato, responsabile, contatti, opportunità aperte, prossima attività) |

### 3.2 Scritture (riepilogo e conferma, anche a voce)

| # | Intent | Esempi | Servizio |
|---|---|---|---|
| W1 | `CrmNewActivity` | "fissa una chiamata con Rossi domani alle 10", "appuntamento con Bianchi srl lunedì alle 15 per la demo" | `Crm_AttivitaPut` (tipo dal nome, assegnata a me, fine = inizio + durata del tipo) |
| W2 | `CrmCompleteActivity` | "segna come fatta la chiamata con Rossi, esito: richiamare a gennaio" | `Crm_AttivitaCompleta` (attività da fare del cliente; se più d'una si chiede tipo o data) |
| W3 | `CrmAddNote` | "aggiungi una nota al cliente Rossi: vuole il listino nuovo" | `Crm_NotaPut` |

### 3.3 Apertura e navigazione

| # | Elemento | Esempi |
|---|---|---|
| O1 | Entità per `OpenEntity`: cliente CRM (`/crmcustomeredit`), opportunità (`/crmopportunityedit`), lead (`/crmleadedit`), offerta (`/crmofferedit`, per numero o oggetto) | "apri la scheda cliente Rossi", "apri l'opportunità rinnovo contratto", "apri l'offerta 12/2026" |
| O2 | `CrmRouteProvider`: clienti, lead, pipeline, agenda, offerte, report, impostazioni, tabelle, riassegna (italiano e inglese) | "portami alla pipeline", "apri l'agenda del CRM" |

**Cliente presenze e cliente CRM**: le presenze hanno già l'entità `cliente` (`/customeredit`). In
"Applicativo completo" "apri il cliente X" resta quello delle presenze (chiave uguale, provider registrato per
primo); per la scheda CRM si dice "scheda cliente" o "cliente crm". Se le presenze non sono attive, "cliente"
porta alla scheda CRM. Lo stesso vale per le pagine: "clienti" resta l'anagrafica delle presenze, "clienti crm"
è la lista del CRM.

## 4. Scelte

- **Ricerca del cliente all'esecuzione, non nel validatore**: il nome si cerca tra i clienti visibili
  (uguale → contenuto → tutte le parole); con più risultati la chat elenca i candidati e la sessione resta
  aperta per correggere ("Rossi Mario srl"). Così i validatori non chiamano servizi e la prova delle frasi
  funziona con i servizi finti.
- **Niente cancellazioni né cambi di fase a voce** in questa fase: sono operazioni che nel CRM hanno effetti
  sullo storico (motivo di perdita, storico fase/valore) e vanno confermate dalle pagine. 🔍 Da valutare
  `CrmChangePhase` ("porta l'opportunità X in negoziazione").
- **Nessun dato delle note o delle descrizioni inviato oltre il necessario**: il LLM riceve solo il testo
  dell'utente e il catalogo, mai il contenuto del CRM (le risposte si compongono nel codice).

## 5. Checklist

- [x] I1 Filtro dei comandi per applicativo attivo (Infrastructure_Dev)
- [x] I2 Pagine dei moduli nel prompt di Navigate (Infrastructure_Dev)
- [x] R1 Agenda
- [x] R2 Opportunità in chiusura
- [x] R3 Pipeline
- [x] R4 Scheda cliente a voce
- [x] W1 Nuova attività
- [x] W2 Completa attività
- [x] W3 Nota sul cliente
- [x] O1 Entità apribili del CRM
- [x] O2 Pagine del CRM per Navigate
- [x] Frasi di prova `tools/ChatAiPhraseTest/frasi/crm.json`
- [ ] Prova delle frasi con il LLM (DeepSeek) e prova nell'app, anche a voce in Chrome/Edge
- [ ] 🔍 Cambio di fase delle opportunità, nuovo lead a voce, "stessa cosa per Bianchi" sulle attività (memoria M6)
