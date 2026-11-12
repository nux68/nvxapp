# Analisi: comandi vocali (chat AI) per Risorse aziendali

> Analisi del 12/11/2026, branch `Assets_Dev` (lavoro su `Assets_Dev_vocali`, worktree `wt_ast_vocali`).
> Documenti gemelli: `ANALISI_CRM_VOCALI.md`, `ANALISI_DOCUMENTMANAGEMENT_VOCALI.md`. Il motore della chat e la
> voce sono descritti in `ANALISI_INFRASTRUCTURE_CHAT_AI.md`; il modulo in `ANALISI_ASSETS_APPLICATIVO.md`.
>
> **Obiettivo**: portare a Risorse aziendali (`ApplicationType.Assets`) i comandi in linguaggio naturale (chat e
> voce): le mie prenotazioni, le risorse libere, prenotare e annullare a voce ("prenota la sala riunioni domani
> dalle 10 alle 11"), le scadenze della flotta, i beni in dotazione. `ANALISI_ASSETS_APPLICATIVO.md` (`AST-BASE.3`)
> diceva "comandi vocali: nessuno per ora": questa analisi li introduce.

Legenda: ✅ svolto · ⏳ da fare · 🔍 da valutare.

## 1. Situazione di partenza

- Il motore (`ChatAIService`, catalogo intent, normalizzatori di date e orari, voce nel client) è in
  Infrastructure ed è già presente su `Assets_Dev`, compresi il filtro per applicativo attivo
  (`IntentDefinition.Application`, `IOpenableEntity.Application`) e le pagine dei moduli nel prompt di Navigate
  (`IRouteProvider.PromptPages`). Un nuovo comando è una classe `ICommandHandler`: la DI la registra da sola
  (scanner dell'assembly in `Installers.cs`), come `IOpenableEntity` e `IRouteProvider`. **Nessuna modifica a
  Infrastructure né al client.**
- Il manuale "Comandi vocali" (`VoiceCommandsUserManualProvider`) si costruisce dal catalogo: i comandi del modulo
  compaiono da soli sotto "Risorse aziendali" (`VoiceManualApp_7`).
- Servizi del modulo (`ClientServer-Service/Assets/`):
  - `Ast_PrenotazioniService`: risorse prenotabili (postazione, sala, auto in pool) e prenotazioni. Ogni utente
    prenota e annulla le proprie, gli amministratori vedono i nomi e annullano tutte; gli altri vedono le
    prenotazioni altrui solo come "occupato". Regole in `Ast_PrenotazionePut`: risorsa attiva, fine dopo
    l'inizio, non nel passato, durata massima e anticipo massimo (`Ast_Cfg`), conflitti con
    `SalvaSeLiberaAsync` (lock sulla risorsa, vale anche per richieste concorrenti). `Ast_PrenotazioneAnnulla`.
  - `Ast_FlottaService`: veicoli, scadenze (bollo, assicurazione, revisione, tagliando, altro), contratto di
    noleggio/leasing; solo amministratori, l'utente vede il proprio veicolo (`Ast_MieiVeicoliGet`).
  - `Ast_BeniService`: beni, consegne e restituzioni con verbale; solo amministratori, l'utente vede i propri
    (`Ast_MieiBeniGet`).
- Menu (`main-menu-assets.service.ts`): amministratori (`CompanyPowerAdmin`, `CompanyAdmin`) Veicoli, Beni,
  Prenotazioni, Le mie risorse (+ Risorse prenotabili e Impostazioni per il `CompanyPowerAdmin`); utenti
  (`User`) Prenotazioni e Le mie risorse.

## 2. Comandi

Cartella `nvxapp.server.service/ClientServer-Service/ChatAiHandler/Assets/`. Tutti con
`Application = Assets`; nomi degli intent con prefisso `Ast` (in "Applicativo completo" convivono con `Crm*`,
`Doc*`, `Hr*`, `Cmp*` e con i comandi delle presenze). Ruoli come il menu: prenotazioni e "le mie risorse" per
`CompanyPowerAdmin`, `CompanyAdmin`, `User`; flotta e beni altrui per `CompanyPowerAdmin`, `CompanyAdmin`.
La visibilità resta nei servizi. Testi nella lingua dell'utente in `AssetsMessages*.resx` (chiavi `Chat*`).

### 2.1 Interrogazioni (sola lettura, nessuna conferma)

| # | Intent | Esempi | Servizio |
|---|---|---|---|
| R1 | `AstMyBookings` | "quali sono le mie prenotazioni?", "cosa ho prenotato domani?", "le mie prenotazioni della prossima settimana" | `Ast_PrenotazioneList` (`SoloMie`; senza periodo i prossimi 14 giorni; escluse quelle già finite) |
| R2 | `AstFreeResources` | "che sale sono libere domani dalle 10 alle 11?", "c'è una postazione libera oggi alle 14?", "le auto sono libere venerdì?" | `Ast_PrenotazioneList` sulla fascia (solo l'inizio = un'ora); senza orario la disponibilità del giorno per risorsa (fasce occupate) |
| R3 | `AstFleetDeadlines` | "scadenze della flotta", "quando scade il bollo della Panda?", "revisioni nei prossimi 60 giorni" | `Ast_VeicoloList` + `Ast_VeicoloGet` per veicolo: scadenze non eseguite (scadute ed entro N giorni, default 30), filtri per tipo e veicolo (targa o modello); anche la fine del contratto di noleggio/leasing. Solo amministratori |
| R4 | `AstAssignedGoods` | "che beni ho in dotazione?", "cosa ha in dotazione Rossi?" | i miei: `Ast_MieiBeniGet` + `Ast_MieiVeicoliGet` (con "verbale da confermare"); di un dipendente (solo amministratori): `Ast_BeneList` + `Ast_VeicoloList` per assegnatario |

### 2.2 Scritture (riepilogo e conferma, anche a voce)

| # | Intent | Esempi | Servizio |
|---|---|---|---|
| W1 | `AstBookResource` | "prenota la sala riunioni domani dalle 10 alle 11", "prenotami una postazione lunedì alle 9", "prenota un'auto aziendale venerdì dalle 8 alle 18 per la visita al cliente" | `Ast_PrenotazionePut` a nome dell'utente |
| W2 | `AstCancelBooking` | "annulla la prenotazione della sala riunioni di domani", "disdici la postazione di lunedì" | `Ast_PrenotazioneAnnulla`, solo prenotazioni **mie**, confermate e non finite |

- **Risorsa per nome o per tipo**: "la sala riunioni", "la Panda" (anche dal veicolo dell'auto in pool) indicano una
  risorsa; "una sala", "una postazione", "un'auto" indicano il tipo e si prenota **la prima libera** nella fascia
  (ordine alfabetico). Più risorse con lo stesso nome: la chat le elenca e chiede quale.
- **Fascia**: data (default oggi), inizio obbligatorio, fine facoltativa (= inizio + 1 ora). Il `CrossValidator`
  scrive la fine calcolata negli slot, così il riepilogo e la conferma vocale la dicono ("prenotazione di sala
  riunioni il 13/11/2026 dalle 10:00 alle 11:00"), e la ricalcola se l'utente cambia l'inizio; una fine non
  successiva all'inizio si richiede subito.
- **Regole del servizio**: conflitti (anche concorrenti), durata e anticipo massimi, passato, risorsa attiva sono
  controllati da `Ast_PrenotazionePut` all'esecuzione: il messaggio d'errore del servizio arriva in chat.
- **Annullamento**: tra le mie prenotazioni dei prossimi 62 giorni si filtra per risorsa, giorno e ora; se ne
  restano più d'una si elencano e si chiede di precisare. Gli amministratori annullano quelle degli altri dalla
  pagina (con la notifica all'interessato), non a voce.

### 2.3 Apertura e navigazione

| # | Elemento | Esempi |
|---|---|---|
| O1 | Entità per `OpenEntity` (amministratori): `veicolo` (`/astvehicleedit`, targa come codice), `bene` (`/astgoodedit`, codice di inventario come codice). Le pagine leggono `history.state.id` | "apri il veicolo AB123CD", "apri il veicolo Panda", "apri il bene INV-001" |
| O2 | `AssetsRouteProvider` (italiano e inglese): prenotazioni, flotta/veicoli/auto aziendali/parco auto, beni/cespiti, risorse prenotabili/sale/postazioni, le mie risorse, impostazioni e manuale "risorse aziendali" | "portami alle prenotazioni", "vai alla flotta", "take me to my resources" |

Risorse prenotabili e prenotazioni non sono entità apribili: le loro pagine di modifica ricevono l'elemento intero
nello stato di navigazione, non l'id.

### 2.4 Collisioni con gli altri moduli

- Chiavi già prese e non riusate: "impostazioni", "calendario", "sedi", "richieste", "attività", "clienti",
  "agenda", "documenti". Qui i nomi sono qualificati ("impostazioni risorse aziendali", "calendario prenotazioni").
- Parole del modulo: "prenotazioni", "flotta", "veicoli", "auto aziendali", "beni", "cespiti", "sale".
  "scadenze" da sola resta al Documentale (`DocExpiring`): qui "scadenze flotta/veicoli/auto", "bollo",
  "revisione", "tagliando", "assicurazione auto". "formazione"/"corsi" (Risorse Umane) e "attrezzature"/DPI
  (Compliance) non sono usate.
- Prenotare non è chiedere un'assenza: le descrizioni di `AstBookResource` e `AstMyBookings` dicono "sala,
  postazione, auto aziendale ... non ferie, permessi o richieste di assenza", le parole chiave non contengono
  "richiesta".

## 3. Scelte

- **Ricerca all'esecuzione, non nel validatore**: risorse, veicoli e dipendenti si cercano nei servizi quando il
  comando si esegue; i validatori controllano solo i formati (data, ora, tipo, giorni). Così la prova delle frasi
  funziona con i servizi finti e la visibilità resta nei servizi.
- **Nessun dato del modulo al LLM**: il LLM riceve solo il testo dell'utente e il catalogo; elenchi e risposte si
  compongono nel codice.
- **Niente cancellazioni di anagrafiche** (veicoli, beni, risorse) né consegne/restituzioni a voce: hanno verbali,
  scadenzario e autovettura in Common, e restano nelle pagine.
- **Scadenze della flotta lette per veicolo** (`Ast_VeicoloGet`, al massimo 200 veicoli): il servizio non ha un
  elenco delle scadenze; con flotte grandi 🔍 un metodo dedicato (`Ast_VeicoloScadenzaList`) eviterebbe N letture.
- 🔍 Da valutare: durata detta a voce ("per due ore"), spostare una prenotazione ("sposta la sala di domani alle
  15"), prenotazione ricorrente, "chi ha l'auto AB123CD?".

## 4. Checklist

- [x] Studio del modulo (servizi, entità, menu e route del client, manuale, ruoli)
- [x] Helper comune `ChatAstHelper` (ruoli, formati, slot data/orario/risorsa/tipo, ricerca risorse, giorni)
- [x] R1 Le mie prenotazioni (`AstMyBookings`)
- [x] R2 Risorse libere (`AstFreeResources`)
- [x] R3 Scadenze della flotta (`AstFleetDeadlines`)
- [x] R4 Beni in dotazione (`AstAssignedGoods`)
- [x] W1 Prenota una risorsa (`AstBookResource`) con fine calcolata e controllo della fascia
- [x] W2 Annulla una mia prenotazione (`AstCancelBooking`)
- [x] O1 Entità apribili: veicolo, bene
- [x] O2 Pagine del modulo per Navigate (`AssetsRouteProvider`, italiano e inglese, `PromptPages`)
- [x] Testi `Chat*` in `AssetsMessages.resx` / `.en.resx` (XML valido, nessuna chiave duplicata)
- [x] Frasi di prova `tools/ChatAiPhraseTest/frasi/risorseaziendali.json` (nessun servizio finto aggiuntivo necessario)
- [x] Build del server e del tool `ChatAiPhraseTest` senza errori nuovi; prova locale degli helper e dei comandi di prenotazione con servizi finti
- [ ] Prova delle frasi con il LLM (DeepSeek) e prova nell'app, anche a voce in Chrome/Edge
- [ ] Prova in "Applicativo completo" (AllFunction) delle frasi contese con presenze, CRM, Documentale, HR e Compliance
- [ ] 🔍 Durata a voce, spostamento di una prenotazione, elenco scadenze della flotta dal servizio
