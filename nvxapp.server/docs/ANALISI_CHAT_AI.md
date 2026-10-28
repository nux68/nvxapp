# Analisi: chat AI e comandi vocali

> Analisi del 28/10/2026, branch `AttendanceTracking_Dev`. Interventi iniziati il 28/10/2026.
>
> **Obiettivo**: dialogo meno macchinoso, più comandi disponibili, linguaggio naturale (chat e voce).

**Stato al 28/10/2026: 8 punti su 41 svolti (Fase 1 completa, da provare con l'LLM), 1 parziale (M2: più valori in una risposta).**

Legenda: ✅ svolto · ⏳ da fare · 🔍 da valutare. Lo stato di ogni punto si aggiorna nella checklist (sezione 9); la tabella seguente riassume per area.

| Area | Punti | Stato |
|---|---|---|
| 3. Correzioni rapide | Q1–Q8 | ✅ 8/8 |
| 4. Motore di dialogo | M1–M7 | ⏳ 0/7 (M2 parziale) |
| 5. Linguaggio naturale | L1–L4 | ⏳ 0/4 |
| 6. Voce | V1–V4 | ⏳ 0/4 |
| 7. Nuovi comandi | C1–C14 | ⏳ 0/14 |
| 8. Costo di aggiunta dei comandi | E1–E4 | ⏳ 0/4 |

**Dove si interviene**: il motore (sezioni 3, 4, 5, 6 ed E2–E4) sta in **Infrastructure_Dev**; i comandi delle presenze (C1–C11, E1) in **AttendanceTracking_Dev**, dopo il merge del motore.

## 1. Situazione attuale

### 1.1 Componenti

| Componente | File | Ramo |
|---|---|---|
| Servizio di dialogo | `nvxapp.server.service/ClientServer-Service/infrastructure/ChatAI/ChatAIService.cs` | Infrastructure |
| Modelli, catalogo intent, prompt | `.../infrastructure/ChatAI/Models/ChatAIModel.cs` | Infrastructure |
| Testi e parole di controllo | `.../infrastructure/ChatAI/ChatAIMessages.cs` (`ChatAIKeywords`) | Infrastructure |
| Sessioni in memoria (Singleton) | `.../infrastructure/ChatAI/ChatSessionStore.cs` | Infrastructure |
| Registro comandi (auto-costruito dalla DI) | `.../infrastructure/ChatAI/CommandRegistry.cs` | Infrastructure |
| Comandi `Help`, `Navigate` | `.../ChatAiHandler/infrastructure/` | Infrastructure |
| Comandi presenze | `.../ChatAiHandler/GestionePresenze/` | AttendanceTracking |
| Riconoscimento vocale | `nvxapp.client/src/nvx/Utility/infrastructure/speech.service.ts` | Infrastructure |
| Finestra chat e FAB | `nvxapp.client/src/nvx/shared/components/infrastructure/fab-menu/` | Infrastructure |

### 1.2 Flusso di un messaggio

1. **Filtro per ruolo**: restano i comandi con `Usable4Role` compatibile con i ruoli dell'utente.
2. **Reset / help**: se il testo *contiene* una parola di `ChatAIKeywords.Reset` o `.Help`, la conversazione si azzera.
3. **Pre-filtro parole chiave**: se nessuna parola chiave di nessun comando compare nel testo, risposta "non ho capito" senza chiamare l'LLM.
4. **Primo turno**: l'LLM (Ollama, OpenRouter o Groq, scelto da `AI:UseLLM`) restituisce un piano JSON `{ actions: [{ intent, slots, confidence }] }`.
5. **Filtro anti-allucinazione** (`SafeMergeSlotsForAction`): un valore si tiene solo se compare nel testo, è il default, oppure `HasRelevantContent` dello slot dice che il messaggio è pertinente.
6. **Validazione** degli slot (`Validator`, `CrossValidator`), con eventuale normalizzazione (es. nome canonico del dipendente).
7. **Turni successivi**: si richiama l'LLM per estrarre **un solo** slot mancante per volta.
8. **Conferma**: riepilogo del piano, poi "sì"/"no" a corrispondenza esatta; esecuzione e cancellazione della sessione.

### 1.3 Comandi disponibili

| Ramo | Comandi | Ruolo |
|---|---|---|
| Infrastructure | `Help`, `Navigate` | tutti |
| AttendanceTracking | `RegisterClocking`, `RegisterJustification`, `RegisterCausale`, `CalcolaGiorno`, `OpenTimeSheetPowerAdmin` | solo CompanyPowerAdmin |
| AttendanceTracking | 10 comandi `Open*` (Attività, Causale, Cliente, Commessa, Competenza, Giustificativo, ModelloExport, Orario, ProfiloOrario, Reparto) | solo CompanyPowerAdmin |

Un utente con ruolo User ha quindi a disposizione solo `Help` e `Navigate`.

### 1.4 Client

- `SpeechService`: Web Speech API, ascolto continuo, solo riconoscimento (nessuna sintesi vocale).
- `fab-menu`: in modalità vocale con la finestra chiusa, i risultati vanno in un toast, le domande aprono la finestra e le **conferme partono da sole** dopo 600 ms.

## 2. Perché il dialogo è macchinoso

| # | Problema | Effetto |
|---|---|---|
| P1 | Il pre-filtro parole chiave fa da cancello | "Rossi è arrivato alle 9", "segna 2 ore di permesso a Mario" falliscono se manca la parola giusta |
| P2 | Reset e help cercano sottostringhe (`nuovo`, `cancella`, `stop`, `basta`, `menu`, `comandi`) | "cancella la timbratura di Rossi" o "inserisci un nuovo permesso" azzerano la conversazione |
| P3 | L'anti-allucinazione confronta il valore già normalizzato col testo | "il 15 maggio" → "15/05/2026" non compare nel testo e non è "oggi/ieri/domani": viene scartato e si usa la data di oggi, senza avviso |
| P4 | Un solo slot per turno | "Rossi alle 9" come risposta riempie solo il dipendente; niente correzioni ("no, alle 10") né cambio di comando |
| P5 | Conferma a corrispondenza esatta | "sì grazie", "sì, vai" non sono riconosciuti; dal riepilogo non si può modificare un valore |
| P6 | Errore → sessione cancellata | l'utente perde quanto già detto |
| P7 | Esecuzione → sessione cancellata | impossibile "e anche per Bianchi", "stessa cosa domani" |
| P8 | Nessuna voce in uscita | in vocale le domande aprono la finestra: niente mani libere |
| P9 | Conferma automatica in vocale | una scrittura parte senza conferma reale |
| P10 | Ricerca dipendente per sottostringa | errori di trascrizione ("Russi" per "Rossi") non recuperati |
| P11 | Suggerimenti = elenco di tutti i comandi | i chip non dipendono dal contesto |
| P12 | Solo comandi "fai qualcosa" | nessuna domanda di sola lettura ("chi è presente oggi?") |

## 3. Correzioni rapide

| # | Intervento | Risolve |
|---|---|---|
| Q1 | Reset e help riconosciuti solo a frase intera (o quasi), non come sottostringa | P2 |
| Q2 | Anti-allucinazione: per date e orari confrontare il testo originale, non il valore normalizzato; in alternativa allargare `HasRelevantContent` di date a numeri e nomi di mesi/giorni | P3 |
| Q3 | Conferma tollerante: "sì…", "ok…", "va bene…" all'inizio frase valgono come conferma | P5 |
| Q4 | Su errore di validazione o dell'LLM non cancellare la sessione, ripetere la domanda | P6 |
| Q5 | Niente conferma automatica in vocale per i comandi che scrivono (solo per navigazione) | P9 |
| Q6 | `RegisterClockingHandler`: la descrizione dell'orario viene scritta in `sb_Date`, quindi l'LLM non riceve le regole sull'orario | bug |
| Q7 | `RegisterClockingHandler`: il verso è forzato a `SenzaVerso`; usare quello detto dall'utente (o decidere esplicitamente di no) e togliere `.Result` sincrono | bug |
| Q8 | Impostare `ChatSession.UserId` e verificare che la sessione appartenga all'utente corrente; ripristinare il messaggio "Dipendente agganciato" (`info = ""` in `DipendenteLookupHelper`) | sicurezza, chiarezza |

## 4. Motore di dialogo

| # | Intervento | Risolve |
|---|---|---|
| M1 | **Tool calling nativo**: ogni `IntentDefinition` diventa un tool con schema JSON (tipi, enumerazioni, obbligatori). L'LLM risponde con una chiamata a un tool o con una domanda. Il server continua a validare e confermare | P1, P4 |
| M2 | **Stato completo a ogni turno**: si passano all'LLM comando, slot raccolti e slot mancanti, e lui aggiorna tutti gli slot insieme. Abilita risposte con più valori, correzioni, cambio di comando | P4 |
| M3 | **Parole chiave come suggerimento, non cancello**: scelta del comando dall'LLM sull'elenco completo, oppure preselezione dei 5 più simili con embedding | P1 |
| M4 | **Frasi di esempio per comando** (5–10 per intent), usate nel prompt e nei test | P1 |
| M5 | **Date e orari risolti in C#**: l'LLM restituisce il testo originale ("lunedì scorso", "dalle 9 alle 13", "mezz'ora", "il 15"), un risolutore deterministico lo converte | P3 |
| M6 | **Memoria tra comandi**: dopo l'esecuzione si conserva l'ultima azione come contesto ("anche per Bianchi", "stessa cosa domani", "annulla l'ultima") | P7 |
| M7 | **Conferma proporzionata al rischio**: lettura e navigazione senza conferma; scritture con riepilogo modificabile ("cambia l'ora in 10"); operazioni massive con doppia conferma | P5, P9 |

## 5. Linguaggio naturale

| # | Intervento |
|---|---|
| L1 | "Io/me" = utente corrente (dipendente collegato); pronomi dal contesto ("lui", "lo stesso") |
| L2 | Data di oggi e giorno della settimana nel prompt comune (oggi `BuildSystemPrompt_Append_Data_Oggi` è usato solo negli slot data) |
| L3 | Risposte formulate bene: l'esecuzione restituisce dati strutturati, un modello di testo con varianti (o l'LLM) produce la frase; campi distinti per testo mostrato e testo breve da pronunciare |
| L4 | Ricerca dipendente tollerante (fonetica / distanza tra parole) con chip di disambiguazione ("Rossi Mario o Rossi Marco?") — risolve P10 |

## 6. Voce

| # | Intervento |
|---|---|
| V1 | Sintesi vocale delle risposte (`speechSynthesis`) nella lingua dell'interfaccia — risolve P8 |
| V2 | Conferma vocale vera ("Registro l'entrata di Rossi alle 9, confermi?") al posto della conferma automatica |
| V3 | Le domande si fanno a voce senza aprire la finestra |
| V4 | Interruzione della voce parlando; pulsante "tieni premuto per parlare" come alternativa all'ascolto continuo |

## 7. Nuovi comandi

### 7.1 Interrogazioni (sola lettura, nessuna conferma) — risolve P12

| # | Comando | Esempio |
|---|---|---|
| C1 | Presenti / assenti | "chi è presente oggi?", "chi manca in produzione?" |
| C2 | Ore lavorate | "quante ore ha fatto Rossi a ottobre?" |
| C3 | Saldi | "quante ferie restano a Bianchi?" |
| C4 | Timbrature | "che timbrature ha Rossi ieri?" |
| C5 | Anomalie | "ci sono timbrature mancanti oggi?" |
| C6 | Richieste in attesa | "quali richieste devo approvare?" |
| C7 | Piano ferie | "chi è in ferie la prossima settimana?" |

### 7.2 Flussi e scritture

| # | Comando | Esempio |
|---|---|---|
| C8 | Approva / rifiuta richieste | "approva tutte le richieste di Rossi" |
| C9 | Correggi / elimina timbratura | "sposta l'uscita di Rossi di ieri alle 18" |
| C10 | Giustificativo su intervallo e operazioni massive | "ferie a tutto il reparto produzione il 24/12" (doppia conferma) |
| C11 | Self-service ruolo User | "timbra la mia entrata", "chiedo ferie dal 3 al 7", "quante ore ho fatto questa settimana" |

### 7.3 Altro

| # | Comando | Esempio |
|---|---|---|
| C12 | Stampe | "stampa il cartellino di Rossi di ottobre" |
| C13 | Infrastructure | cambio lingua/tema, logout, impersonate |
| C14 | Help contestuale | "cosa posso fare in questa pagina?" |

Per tutti i comandi il controllo dei permessi resta nei servizi, non solo in `Usable4Role`.

## 8. Costo di aggiunta dei comandi

| # | Intervento |
|---|---|
| E1 | Un unico comando "Apri entità" guidato da metadati (servizio, campi di ricerca, pagina) al posto dei 10 `Open*Handler` duplicati; "apri / lista / filtra" nascono da soli per ogni nuova anagrafica |
| E2 | Raccolta di frasi di test (frase → comando e slot attesi) eseguita a ogni modifica di prompt o modello |
| E3 | Log delle frasi non capite, da cui ricavare sinonimi e comandi nuovi |
| E4 | Suggerimenti (chip) contestuali: comandi pertinenti alla pagina e al ruolo, non l'elenco completo — risolve P11 |

## 9. Checklist

Ordine consigliato: correzioni rapide → motore → interrogazioni e self-service → voce → metadati e test.

### Fase 1 — Correzioni rapide (Infrastructure_Dev, Q6–Q7 su AttendanceTracking_Dev)
- [x] Q1 Reset e help a frase intera (`ChatAIKeywords.Reset`/`Help` con `IsExactMatch`; testo normalizzato senza punteggiatura)
- [x] Q2 Data non più scartata dall'anti-allucinazione (`ChatSlotWords.HasDateContent` sugli slot data; data letta con formati espliciti e cultura invariante in `ChatSlotWords.ParseDateOrToday`: prima con interfaccia inglese "15/05/2026" diventava oggi)
- [x] Q3 Conferma tollerante (`ChatAIKeywords.IsConfirm`/`IsCancel`: "sì grazie", "no lascia stare" sì, "sì ma alle 10" no; dopo la richiesta di chiarimento si resta in attesa di conferma)
- [x] Q4 Sessione mantenuta in caso di errore (errore del LLM a metà dialogo: si ripete la domanda; sessione scaduta: il messaggio si elabora in una sessione nuova invece di essere perso)
- [x] Q5 Niente conferma automatica in vocale per le scritture (`ChatAIOutModel.AutoConfirm` vero solo per piani di sola navigazione; altrimenti si apre la chat col riepilogo)
- [x] Q6 Descrizione orario in `RegisterClockingHandler`
- [x] Q7 `RegisterClockingHandler`: `await` al posto di `.Result`, esito del salvataggio controllato (prima un errore risultava comunque "registrata"), rapporto di lavoro verificato. Il verso resta `SenzaVerso` come nel resto dell'app: lo assegna il motore di calcolo
- [x] Q8 Sessione legata all'utente (`IChatSessionStore.TryGet/Create` con `userId`). Il messaggio "Dipendente agganciato" non è stato ripristinato: il nome canonico compare già nel riepilogo e nelle domande

### Fase 2 — Motore di dialogo (Infrastructure_Dev)
- [ ] M1 Tool calling nativo
- [ ] M2 Stato completo a ogni turno
  - [x] Più valori in una risposta dal secondo turno (28/10/2026, `ChatAIService.Call_LLM_OtherTurnAsync`; da provare con l'LLM)
  - [ ] Correzioni di valori già dati ("no, alle 10")
  - [ ] Cambio di comando a metà dialogo
  - [ ] Valori per le azioni successive del piano
- [ ] M3 Parole chiave come suggerimento
- [ ] M4 Frasi di esempio per comando
- [ ] M5 Date e orari risolti in C#
- [ ] M6 Memoria tra comandi
- [ ] M7 Conferma proporzionata al rischio
- [ ] L1 "Io/me" e pronomi
- [ ] L2 Data e giorno della settimana nel prompt comune
- [ ] L3 Risposte formulate (testo mostrato + testo da pronunciare)
- [ ] L4 Ricerca dipendente tollerante

### Fase 3 — Nuovi comandi (AttendanceTracking_Dev, C13–C14 su Infrastructure_Dev)
- [ ] C1 Presenti / assenti
- [ ] C2 Ore lavorate
- [ ] C3 Saldi ferie e permessi
- [ ] C4 Timbrature
- [ ] C5 Anomalie
- [ ] C6 Richieste in attesa
- [ ] C7 Piano ferie
- [ ] C8 Approva / rifiuta richieste
- [ ] C9 Correggi / elimina timbratura
- [ ] C10 Giustificativo su intervallo e operazioni massive
- [ ] C11 Self-service ruolo User
- [ ] C12 Stampe
- [ ] C13 Comandi Infrastructure (lingua, tema, logout, impersonate)
- [ ] C14 Help contestuale

### Fase 4 — Voce (Infrastructure_Dev)
- [ ] V1 Sintesi vocale delle risposte
- [ ] V2 Conferma vocale vera
- [ ] V3 Domande a voce senza aprire la finestra
- [ ] V4 Interruzione e "tieni premuto per parlare"

### Fase 5 — Scalabilità (E1 su AttendanceTracking_Dev, il resto su Infrastructure_Dev)
- [ ] E1 Comando "Apri entità" da metadati
- [ ] E2 Frasi di test
- [ ] E3 Log delle frasi non capite
- [ ] E4 Suggerimenti contestuali
