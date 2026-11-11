# Analisi: comandi vocali (chat AI) per il Documentale

> Analisi dell'11/11/2026, branch `DocumentManagement_Dev` (lavoro su `DocumentManagement_Dev_vocali`, worktree
> `wt_dm_vocali`). Documento gemello: `ANALISI_VOCALI_CRM.md` (branch CRM). Motore e voce: `ANALISI_INFRASTRUCTURE_CHAT_AI.md`.
>
> **Obiettivo**: comandi in linguaggio naturale (chat e voce) per il Documentale, come previsto da
> `ANALISI_DOCUMENTMANAGEMENT_APPLICATIVO.md` §7.6 e §11 ("trova il contratto di Rossi", "documenti in scadenza",
> "apri l'ultimo cedolino"), nel rispetto della decisione del 04/11/2026: **nessun contenuto dei documenti
> viene inviato al LLM**.

Legenda: ✅ svolto · ⏳ da fare · 🔍 da valutare.

## 1. Situazione di partenza

- Il motore della chat e la voce (client) sono in Infrastructure; sul ramo Documentale `ChatAiHandler/` ha solo
  i comandi comuni. Nessun comando del Documentale.
- Le letture passano da `IDoc_DocumentoService.Doc_DocumentoList`, che applica già la visibilità
  (`DocumentManagementUtility.Visibili`: HR, amministrazione con abilitazione al personale, dipendente solo sui
  propri documenti e sulle cartelle visibili). La riga dell'elenco porta già i nomi di tipo, cartella, dipendente
  e cliente: i filtri per nome si fanno in memoria senza leggere le tabelle.
- `Doc_DocumentoGet` e il download scrivono il registro accessi e la data di prima apertura: la chat non li usa
  per informare, solo quando l'utente apre davvero il documento (pagina `/docedit`).
- `Doc_SetupGet` allinea l'anagrafica dipendenti (scrive): la chat non lo usa.
- Prerequisiti nel motore (Infrastructure_Dev, comuni al CRM): **I1** comandi filtrati per applicativo attivo
  (`IntentDefinition.Application`), **I2** pagine dei moduli nel prompt di Navigate (`IRouteProvider.PromptPages`).
  Dettagli in `ANALISI_VOCALI_CRM.md` §2.

## 2. Comandi

Cartella `nvxapp.server.service/ClientServer-Service/ChatAiHandler/DocumentManagement/`. Tutti con
`Application = DocumentManagement`, ruoli `CompanyPowerAdmin`, `CompanyAdmin`, `User`: il servizio restringe i
documenti secondo il ruolo, quindi lo stesso comando vale per l'HR (tutto l'archivio) e per il dipendente (i suoi
documenti). Testi in `DocumentManagementMessages*.resx` (chiavi `Chat*`).

### 2.1 Filtri comuni

| Slot | Significato | Come si applica |
|---|---|---|
| `documentType` | tipo detto dall'utente ("cedolino", "busta paga", "CU", "contratto") | confronto con il nome del tipo della riga, con sinonimi italiano/inglese dei tipi predefiniti; "contratto" vale per tutti i tipi che lo contengono |
| `person` | dipendente o cliente ("di Rossi"); `@me` = "i miei" | `@me` → `Miei = true`; altrimenti confronto con dipendente e cliente della riga |
| `query` | parole del titolo o del testo ("fattura Enel") | filtro `Testo` del servizio (ricerca full-text **nel database**, il testo non va al LLM) |
| `year` | anno ("del 2025") | filtro `Anno` del servizio |

### 2.2 Interrogazioni (sola lettura, nessuna conferma)

| # | Intent | Esempi |
|---|---|---|
| R1 | `DocSearch` | "trova il contratto di Rossi", "cedolini del 2025 di Bianchi", "documenti con scritto Enel", "i miei cedolini" |
| R2 | `DocExpiring` | "documenti in scadenza", "cosa scade nei prossimi 60 giorni?" (default 30 giorni; prima i già scaduti) |
| R3 | `DocToRead` | "ho documenti da leggere?", "cosa devo firmare per presa visione?" (nuovi mai aperti e prese visione mancanti) |

### 2.3 Apertura e scritture

| # | Intent | Esempi | Note |
|---|---|---|---|
| O1 | `DocOpen` (navigazione) | "apri l'ultimo cedolino", "apri il contratto di Rossi" | apre `/docedit` con `history.state.id` il documento più recente tra quelli trovati e dice quanti ce n'erano |
| W1 | `DocConfirmRead` (conferma) | "conferma la presa visione del cedolino di ottobre" | `Doc_DocumentoPresaVisione`; **solo per documenti già aperti** (la presa visione attesta la lettura: se il documento non è mai stato aperto la chat propone di aprirlo) |
| N1 | `DocumentManagementRouteProvider` | "portami all'archivio", "i miei documenti", "cartelle", "registro accessi" | "documenti" porta all'archivio per HR e amministrazione, ai "miei documenti" per l'utente (`RoleRoutes`) |

Le anagrafiche del Documentale (tipi, cartelle, dipendenti) non sono entità apribili con `OpenEntity`: le loro
pagine di modifica ricevono l'intero modello (`state.item` + `state.setup`), non l'id. Si raggiungono dalla
navigazione. 🔍 Da valutare se aggiungere l'apertura per id a quelle pagine.

## 3. Scelte

- **Niente caricamenti, eliminazioni o spostamenti a voce**: richiedono un file o hanno effetti sul cestino e
  sullo spazio; restano nelle pagine.
- **Il LLM non vede i documenti**: riceve solo la frase dell'utente e il catalogo; titoli e nomi nelle risposte
  li compone il codice. La ricerca nel testo dei documenti avviene nel database.
- **Elenco limitato**: il servizio restituisce al massimo 500 righe (le più recenti); la chat mostra le prime 10
  e lo segnala se l'elenco è stato troncato.

## 4. Checklist

- [x] I1, I2 Prerequisiti nel motore (Infrastructure_Dev)
- [x] R1 Ricerca documenti
- [x] R2 Documenti in scadenza
- [x] R3 Documenti da leggere / presa visione mancante
- [x] O1 Apertura del documento
- [x] W1 Presa visione a voce (solo documenti già aperti)
- [x] N1 Pagine del Documentale per Navigate
- [x] Frasi di prova `tools/ChatAiPhraseTest/frasi/documentale.json`
- [ ] Prova delle frasi con il LLM (DeepSeek) e prova nell'app, anche a voce in Chrome/Edge
- [ ] 🔍 Apertura per id di tipi, cartelle e dipendenti; richieste di documenti (`Doc_Richiesta`, fase 2) quando esisteranno
