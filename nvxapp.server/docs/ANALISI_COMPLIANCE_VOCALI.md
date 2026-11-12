# Analisi: comandi vocali (chat AI) per Compliance

> Analisi del 12/11/2026, branch `Compliance_Dev` (lavoro su `Compliance_Dev_vocali`, worktree `wt_cmp_vocali`).
> Documenti gemelli: `ANALISI_CRM_VOCALI.md` (branch CRM) e `ANALISI_DOCUMENTMANAGEMENT_VOCALI.md` (branch DocumentManagement).
> Il motore della chat e la voce sono descritti in `ANALISI_INFRASTRUCTURE_CHAT_AI.md`.
>
> **Obiettivo**: portare a Compliance i comandi in linguaggio naturale (chat e voce): scadenze della sicurezza sul lavoro,
> formazione obbligatoria, data breach, stato del registro GDPR, i propri adempimenti, poche scritture semplici con
> conferma, apertura delle sezioni. `ANALISI_COMPLIANCE_APPLICATIVO.md` §5 prevede la chat "solo per sicurezza e GDPR,
> segnalazioni escluse in modo esplicito".

Legenda: ✅ svolto · ⏳ da fare · 🔍 da valutare.

## 1. Situazione di partenza

- Il motore (`ChatAIService`, catalogo intent, normalizzatori di date, voce nel client) è in Infrastructure ed è già
  presente su `Compliance_Dev`, compresi il filtro per applicativo attivo (`IntentDefinition.Application`,
  `IOpenableEntity.Application`), le pagine dei moduli nel prompt di `Navigate` (`IRouteProvider.PromptPages`) e il
  manuale "Comandi vocali" (capitolo `VoiceManualApp_6`). Nessun intervento in Infrastructure.
- Un nuovo comando è una classe `ICommandHandler`: la DI la registra da sola (scanner dell'assembly in `Installers.cs`),
  come `IRouteProvider`. La voce è tutta nel client e vale per qualsiasi comando: **il client non cambia**.
- Sul ramo Compliance la cartella `ChatAiHandler/` contiene solo i comandi comuni (Help, Navigate, OpenEntity, Tema,
  Lingua): **nessun comando di Compliance**.
- Servizi del modulo: `Cmp_SicurezzaService` (CMP-81: formazione e situazione, visite, DPI e consegne, nomine, documenti,
  `Cmp_Lookup`, `Cmp_Mine`), `Cmp_GdprService` (trattamenti, responsabili esterni, violazioni), `Cmp_ConfigService`.
  Permessi in `CmpUtility.ContestoAsync`: amministratori (CompanyPowerAdmin, CompanyAdmin) per sicurezza e GDPR,
  abilitati ai dati sanitari per le visite, dipendente per i propri adempimenti, studio per il GDPR delle sue aziende.
- **Richieste degli interessati (artt. 15-22)**: il modulo non le gestisce (non ci sono tabella né servizio); il comando
  "richieste GDPR aperte e loro termini" non si può fare ora (§5).
- **Whistleblowing** (`CMP-WB`): non ancora fatto e comunque escluso dalla chat.

## 2. Ruoli, visibilità e dati personali

- Comandi della sicurezza e del GDPR: ruoli `CompanyPowerAdmin`, `CompanyAdmin` (come il menu "Sicurezza sul lavoro" e
  "Registro GDPR"). "I miei adempimenti": anche `User` (come la voce "I miei adempimenti").
- Lo studio (FinancialAdvisor*) non ha azienda nella chat: vede solo i comandi comuni (filtro per applicativo del motore).
- **Visibilità**: ogni comando legge con i servizi delle pagine (`Cmp_*List`, `Cmp_FormazioneStato`, `Cmp_Mine`,
  `Cmp_Lookup`), che ricontrollano i permessi: dalla chat si vede esattamente ciò che si vede nelle pagine.
- **Dati personali**: il LLM riceve solo il testo dell'utente e il catalogo dei comandi; le risposte si compongono nel
  codice (`ChatCmpHelper`, testi `ComplianceMessages`). Inoltre le risposte non contengono mai:
  - esito di idoneità, limitazioni, medico, note sanitarie delle visite (solo "visita medica di X" e la scadenza);
  - descrizione, categorie di dati, conseguenze di una violazione (solo data di scoperta, rischio, termine delle 72 ore);
  - nulla delle segnalazioni del whistleblowing.
- ⚠️ **Limite del motore**: `ChatAIService` aggiunge la risposta di ogni comando alla cronologia della sessione
  (`session.AddToHistory("assistant", message)`) e la cronologia (ultimi `AI:MaxHistoryTurns` turni) torna al LLM nei
  turni successivi. Per questo le risposte di Compliance sono già "povere" (vedi sopra), ma nomi dei dipendenti e scadenze
  possono arrivare al LLM. Proposta per Infrastructure (§5, `CMP-VOC.I1`): `CommandResult.HistoryText` (testo ridotto da
  mettere in cronologia) o un flag `IntentDefinition.SensitiveResult` che sostituisce la risposta con "[risultato del
  comando X]".

## 3. Comandi di Compliance

Cartella `nvxapp.server.service/ClientServer-Service/ChatAiHandler/Compliance/`. Tutti con
`Application = ApplicationType.Compliance`, nomi con prefisso `Cmp`, testi nella lingua dell'utente in
`ComplianceMessages*.resx` (chiavi `Chat*`). Sezioni dell'help: "Sicurezza sul lavoro" (ordine 690) e "Registro GDPR" (695): 680 è di Risorse Umane, 700 di Documentale e Risorse aziendali.

### 3.1 Interrogazioni (sola lettura, nessuna conferma)

| # | Intent | Esempi | Servizio | Ruoli |
|---|---|---|---|---|
| R1 | `CmpSafetyExpiries` | "quali sono le scadenze della sicurezza?", "visite mediche in scadenza nei prossimi 60 giorni", "il DVR è scaduto?" | `Cmp_FormazioneStato`, `Cmp_VisitaList`, `Cmp_NominaList`, `Cmp_DocumentoList`: scaduti e in scadenza entro N giorni (default: giorni di preavviso del modulo); filtro facoltativo per argomento (corsi, visite, nomine, documenti); conteggio dei corsi obbligatori mai svolti. Per nomine e documenti conta l'ultima nomina/revisione | amministratori |
| R2 | `CmpSafetyTraining` | "com'è la situazione della formazione sicurezza?", "chi non ha fatto i corsi obbligatori?", "corsi sicurezza di Rossi" | `Cmp_FormazioneStato`: validi, in scadenza, scaduti, mancanti ed elenco da sistemare; con il dipendente, i suoi corsi richiesti | amministratori |
| R3 | `CmpDataBreaches` | "ci sono data breach aperti?", "violazioni da notificare al Garante" | `Cmp_ViolazioneList`: violazioni con notifica dovuta, termine delle 72 ore e ore mancanti o ritardo; violazioni dell'anno | amministratori |
| R4 | `CmpGdprRegister` | "com'è messo il registro GDPR?", "contratti dei responsabili esterni in scadenza?" | `Cmp_TrattamentoList`, `Cmp_ResponsabileList`, `Cmp_ViolazioneList`: trattamenti attivi (con dati particolari), trattamenti incompleti (voci dell'art. 30 mancanti), contratti dei responsabili scaduti/in scadenza, violazioni dell'anno | amministratori |
| R5 | `CmpMyCompliance` | "quando scade la mia visita medica?", "i miei corsi sicurezza sono in regola?", "che DPI ho ricevuto?" | `Cmp_Mine`: corsi con scadenza, scadenza della prossima visita (mai l'esito), DPI ricevuti, nomine, prese visione da confermare | amministratori e `User` |

### 3.2 Scritture (riepilogo e conferma, anche a voce)

Solo inserimenti; nessuna modifica, nessuna cancellazione.

| # | Intent | Esempi | Servizio | Effetti |
|---|---|---|---|---|
| W1 | `CmpRegisterTraining` | "registra il corso antincendio di Rossi fatto ieri", "Bianchi ha fatto il corso di primo soccorso il 3 novembre" | `Cmp_FormazionePut` (tipo di corso dal nome, anche con sinonimi italiani/inglesi; descrizione = tipo; data non futura) | La scadenza la calcola il servizio dalle mansioni; il nuovo corso rinnova i precedenti dello stesso tipo nello scadenzario (comportamento della pagina) |
| W2 | `CmpPpeDelivery` | "consegna due paia di guanti a Rossi", "ho dato le scarpe antinfortunistiche taglia 42 a Verdi ieri" | `Cmp_DpiConsegnaPut` (DPI attivo del catalogo dal nome, quantità, taglia, data non futura) | Il dipendente con utente riceve la richiesta di presa visione del verbale (comportamento della pagina) |

Dipendente e DPI si cercano **all'esecuzione** (come il cliente nel CRM) tra quelli di `Cmp_Lookup`: uguale → contenuto
a parole intere → tutte le parole; con più risultati la chat elenca i candidati. Così i validatori non chiamano servizi e
la prova delle frasi funziona con i servizi finti.

### 3.3 Apertura e navigazione

| # | Elemento | Esempi |
|---|---|---|
| O1 | `CmpOpen` (navigazione, senza conferma): apre una sezione dell'elenco (`/cmp81` o `/cmpgdpr` con `history.state = { sezione }`) oppure un elemento nella pagina di modifica (`/cmpedit` con `{ sezione, item }`). Elementi apribili: documenti (anche "il DVR": la revisione più recente), nomine (per tipo o persona), mansioni, catalogo DPI, trattamenti, responsabili esterni. Corsi, situazione formazione, visite, consegne e violazioni si aprono sull'elenco | "apri le visite mediche", "apri il DVR", "apri la nomina dell'RSPP", "open the data breaches" |
| O2 | `ComplianceRouteProvider` per `Navigate`: sicurezza sul lavoro, registro GDPR, i miei adempimenti, impostazioni, manuale (italiano e inglese) | "portami al registro gdpr", "take me to workplace safety" |

**Perché non `IOpenableEntity`**: `OpenEntity` apre la pagina di modifica con `history.state = { id }`, ma le pagine di
Compliance sono uniche per tutte le sezioni e leggono `{ sezione }` (elenco) e `{ sezione, item }` con l'elemento
completo (modifica, come fa la pagina elenco). Senza toccare il client serve quindi un comando proprio, che passa lo
stato giusto. Se in futuro `cmp-edit-page` caricherà l'elemento da `{ sezione, id }`, i casi semplici potranno passare a
`IOpenableEntity` (§5).

**Parole contese** (in "Applicativo completo" vince il primo provider registrato):
- proprie del modulo: "sicurezza", "gdpr", "privacy", "data breach", "dpi", "dvr";
- "formazione" e "corsi" sono di Risorse Umane: qui sempre "corsi sicurezza", "formazione sicurezza", "corsi obbligatori";
- "scadenze", "documenti", "prenotazioni" mai da sole: "scadenze sicurezza", "scadenze compliance", "documenti sicurezza";
- nessuna chiave di route uguale a quelle di Infrastructure, Presenze, CRM, Documentale (verificato sul ramo AllFunction).
- "consegna" da sola non è una parola chiave (Risorse aziendali consegna i beni): "consegna dpi", "guanti", "scarpe antinfortunistiche"…

## 4. Scelte

- **Niente cancellazioni né modifiche a voce**: le scritture sono solo inserimenti con riepilogo e conferma.
- **Niente notifica al Garante a voce** (segnare una violazione come notificata): ha valore legale (data, motivo del
  ritardo) e chiude la scadenza delle 72 ore; si fa dalla pagina. 🔍 Da valutare con il DPO.
- **Niente visite mediche a voce** (registrazione): esito e limitazioni sono dati sanitari, da scrivere solo nella pagina
  dagli abilitati.
- **Niente comandi lato studio**: la chat dello studio non ha un'azienda; il registro GDPR delle aziende resta in `/cmpstudio`.
- **Gli abilitati ai dati sanitari non amministratori** non hanno comandi propri (come nel menu: aprono le visite da `/cmp81`).

## 5. Punti aperti

- `CMP-VOC.I1` (Infrastructure) testo ridotto in cronologia per i comandi con dati personali (§2).
- `CMP-VOC.O1` (client Compliance) `cmp-edit-page` che carica l'elemento da `{ sezione, id }`, per usare `IOpenableEntity`.
- `CMP-VOC.G1` Richieste degli interessati (accesso, cancellazione…) con i termini di legge (1 mese, prorogabile): prima
  vanno fatte nel modulo (tabella, servizio, pagina), poi il comando `CmpGdprRequests`.
- `CMP-VOC.W1` Segnalazioni del whistleblowing: escluse dalla chat anche quando `CMP-WB` sarà fatto.

## 6. Checklist

- [x] Studio del modulo (servizi, permessi, pagine, menu, manuale) e dei modelli CRM/Documentale
- [x] Helper comune `ChatCmpHelper` (ruoli, formati, slot dipendente/giorni/data, ricerca dipendenti, DPI, tipi di corso)
- [x] R1 Scadenze della sicurezza (`CmpSafetyExpiries`)
- [x] R2 Situazione della formazione sicurezza (`CmpSafetyTraining`)
- [x] R3 Data breach da notificare (`CmpDataBreaches`)
- [x] R4 Stato del registro GDPR (`CmpGdprRegister`)
- [x] R5 I miei adempimenti (`CmpMyCompliance`)
- [x] W1 Registra corso di sicurezza (`CmpRegisterTraining`)
- [x] W2 Consegna DPI (`CmpPpeDelivery`)
- [x] O1 Apertura di sezioni ed elementi (`CmpOpen`)
- [x] O2 Pagine di Compliance per Navigate (`ComplianceRouteProvider`, italiano e inglese)
- [x] Testi `Chat*` in `ComplianceMessages.resx` / `.en.resx` (XML valido, nessuna chiave duplicata)
- [x] Frasi di prova `tools/ChatAiPhraseTest/frasi/compliance.json` (nessun servizio finto da aggiungere)
- [x] Build del server e del tool `ChatAiPhraseTest` senza errori nuovi
- [ ] Prova delle frasi con il LLM (DeepSeek) e prova nell'app, anche a voce in Chrome/Edge
- [ ] Prova su DB usa e getta dei comandi (permessi CompanyAdmin/User, apertura `/cmpedit` con l'elemento)
- [ ] 🔍 `CMP-VOC.I1`, `CMP-VOC.O1`, `CMP-VOC.G1` (§5)
