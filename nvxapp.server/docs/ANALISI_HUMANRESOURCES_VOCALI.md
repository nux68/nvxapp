# Analisi: comandi vocali (chat AI) per Risorse Umane

> Analisi del 12/11/2026, branch `HumanResources_Dev` (lavoro su `HumanResources_Dev_vocali`, worktree `wt_hr_vocali`).
> Documenti gemelli: `ANALISI_CRM_VOCALI.md` e `ANALISI_DOCUMENTMANAGEMENT_VOCALI.md` (rami CRM e DocumentManagement).
> Il motore della chat e la voce sono descritti in `ANALISI_INFRASTRUCTURE_CHAT_AI.md`.
>
> **Obiettivo**: portare a Risorse Umane i comandi in linguaggio naturale (chat e voce) per l'ufficio del personale
> (scadenze, corsi da fare, onboarding, scheda HR) e per il dipendente ("I miei cedolini"), rispettando la decisione
> `HR-D6` di `ANALISI_HUMANRESOURCES_APPLICATIVO.md` (cedolini e CU esclusi dalla chat, salvo i propri).

Legenda: ✅ svolto · ⏳ da fare · 🔍 da valutare.

## 1. Situazione di partenza

- Il motore (`ChatAIService`, catalogo intent, normalizzatori di date, voce nel client) è in Infrastructure ed è già
  presente su `HumanResources_Dev`, con il filtro per applicativo (`IntentDefinition.Application`,
  `IOpenableEntity.Application`), le pagine dei moduli nel prompt di Navigate (`IRouteProvider.PromptPages`) e il
  manuale "Comandi vocali". Un comando è una classe `ICommandHandler`, registrata dallo scanner DI (`Installers.cs`)
  come `IOpenableEntity` e `IRouteProvider`: **nessuna modifica a Infrastructure né al client**.
- Sul ramo la cartella `ChatAiHandler/` contiene solo i comandi comuni (Help, Navigate, OpenEntity, Tema, Lingua).
- `ANALISI_HUMANRESOURCES_APPLICATIVO.md` (`HR-BASE.3`, `HR-D6`): "comandi vocali: nessuno per ora"; proposta di
  escludere cedolini e CU e ammettere corsi e checklist per i ruoli che li vedono.
- Permessi del modulo (`HrUtility`): ufficio del personale = `CompanyPowerAdmin`, `CompanyAdmin` (tutti i dipendenti);
  `User` = solo l'area del dipendente ("I miei cedolini"). Lo studio paghe lavora solo sui caricamenti dei cedolini.

## 2. Servizi usati

| Servizio | Metodi | Visibilità |
|---|---|---|
| `IDeadlineService` (scadenzario comune) | `DeadlineList` (aperte, entro N giorni) filtrata su `Application = HumanResources` (fonti `hr.corso`, `hr.checklist`) | Come la pagina Scadenze: l'amministratore vede tutte quelle dell'azienda |
| `IHr_FormazioneService` | `Hr_PianoList`, `Hr_PianoGet`, `Hr_PianoPut`, `Hr_CorsoList` | Amministratori (`AmministratoreAsync`) |
| `IHr_OnboardingService` | `Hr_ChecklistList`, `Hr_ChecklistGet`, `Hr_ChecklistVocePut` | Amministratori |
| `IHr_DipendenteService` | `Hr_DipendenteList` | Amministratori |
| `IHr_CedolinoService` | `Hr_MieiDocumenti` | Il dipendente collegato all'utente, solo i propri |
| `IHr_ConfigService` | `Hr_CfgGet` (giorni di preavviso come periodo predefinito delle scadenze) | Utenti dell'azienda |

## 3. Comandi di Risorse Umane

Cartella `nvxapp.server.service/ClientServer-Service/ChatAiHandler/HumanResources/` (`ChatHrHelper.cs`,
`HrQueryHandlers.cs`, `HrWriteHandlers.cs`, `HrOpenableEntities.cs`, `HumanResourcesRouteProvider.cs`). Tutti con
`Application = HumanResources`, sezione dell'help "Risorse umane" (`CategoryOrder` 680, tra CRM e Documentale).
Ruoli come il menu del modulo: `CompanyPowerAdmin`, `CompanyAdmin` per l'ufficio del personale; anche `User` per
"I miei cedolini". Testi in `HumanResourcesMessages*.resx` (chiavi `Chat*`, italiano e inglese). Nomi degli intent con
prefisso `Hr`.

### 3.1 Interrogazioni (sola lettura, nessuna conferma)

| # | Intent | Esempi | Servizio e risposta |
|---|---|---|---|
| R1 | `HrExpiries` | "scadenze hr dei prossimi 60 giorni", "quali corsi sono in scadenza?", "scadenze dei dipendenti di Rossi" | `DeadlineList` (aperte, entro N giorni; default i giorni di preavviso del modulo, altrimenti 30) solo di Risorse Umane: prima le scadute, poi le prossime, data e descrizione ("Corso X - Rossi Mario", voce di checklist). Filtro facoltativo sul dipendente |
| R2 | `HrTrainingToDo` | "che corsi deve ancora fare Rossi?", "chi deve fare il corso sicurezza?", "com'è il piano di formazione di quest'anno?" | `Hr_PianoList` + `Hr_PianoGet` dell'anno (default corrente): righe non svolte, ordinate per "entro il", con quelle in ritardo segnate; filtri su dipendente e corso; svolte su totali |
| R3 | `HrOnboarding` | "come vanno gli onboarding?", "a che punto è la checklist di Bianchi?" | `Hr_ChecklistList` (aperte): dipendente, tipo, data, voci fatte su totali, prossima voce (in ritardo). Con il dipendente: `Hr_ChecklistGet` e le voci da completare con data e responsabile |
| R4 | `HrEmployeeInfo` | "scheda hr di Verdi", "che matricola ha Rossi?", "quando è stato assunto Bianchi?" | `Hr_DipendenteList`: in forza o no, matricola, email, assunzione, cessazione, account; checklist aperte. **Senza codice fiscale** (resta nella pagina) |
| R5 | `HrMyPayslips` | "ho cedolini da confermare?", "i miei cedolini del 2025", "do I have new payslips?" | `Hr_MieiDocumenti`: numero di cedolini e CU, ultimo periodo, mesi dell'anno chiesto, documenti da confermare. Solo periodi e date, **mai contenuti o importi** (che il servizio non espone) |

### 3.2 Scritture (riepilogo e conferma, anche a voce)

| # | Intent | Esempi | Servizio |
|---|---|---|---|
| W1 | `HrCompleteChecklistItem` | "segna come fatta la voce consegna badge della checklist di Rossi", "completa la voce visita medica di Bianchi" | Voce da completare tra le checklist aperte del dipendente (`Hr_ChecklistGet`), poi `Hr_ChecklistVocePut` con la voce com'è (responsabile, data prevista, note) e la sola spunta: come la casella nella pagina (chiude la scadenza, aggiorna lo stato). **Rifiutate le voci con un'azione sull'account** (creazione/disattivazione): si completano dalla pagina facendo l'azione |
| W2 | `HrAddPlanCourse` | "aggiungi al piano di formazione il corso primo soccorso per Verdi entro il 30 giugno", "metti nel piano 2027 il corso antincendio per Verdi" | Piano dell'anno (dalla data "entro" o dall'anno detto; se più piani, si chiede quale), corso attivo e dipendente attivo dalle liste di `Hr_PianoGet`; `Hr_PianoPut` con tutte le righe esistenti (con l'id) più la nuova. Rifiutato se la riga c'è già |

### 3.3 Apertura e navigazione

| # | Elemento | Esempi |
|---|---|---|
| O1 | Entità per `OpenEntity` (ufficio del personale): `dipendente hr` (`/hremployeeedit`, parole "scheda hr", "anagrafica hr", "dipendenti hr"; matricola come codice), `corso` (`/hrcourseedit`), `piano di formazione` (`/hrplanedit`, anno come codice), `checklist hr` (`/hrchecklistedit`, "checklist onboarding", "onboarding"; dipendente come codice) | "apri la scheda hr di Rossi", "apri il corso primo soccorso", "apri il piano di formazione 2026", "apri la checklist di onboarding di Bianchi" |
| O2 | `HumanResourcesRouteProvider`: risorse umane, dipendenti hr, cedolini (per ruolo: ufficio `/hrpayslips`, studio `/hrstudiopayslips`, dipendente `/hrmypayslips`), carica cedolini, i miei cedolini, corsi, formazione, piani di formazione, checklist hr, onboarding, modelli checklist, impostazioni hr, manuale (italiano e inglese) | "portami ai piani di formazione", "vai alla formazione", "take me to my payslips" |

**Chiavi condivise con gli altri moduli** (in "Applicativo completo" vince il primo provider registrato):
"dipendente"/"dipendenti", "clienti", "attività", "impostazioni", "competenze", "calendario hr" restano delle presenze
(e del CRM); per HR si usano "dipendente hr", "scheda hr", "dipendenti hr", "impostazioni hr". "formazione" e "corsi"
sono assegnati a Risorse Umane e si usano senza qualificatore. "scadenze", "documenti", "checklist" non si usano da
soli né come chiavi né come parole chiave: "scadenze hr", "scadenze dipendenti", "checklist hr", "checklist di".

## 4. Scelte

- **Dati personali mai al LLM**: il LLM riceve solo il testo dell'utente e il catalogo dei comandi; le risposte
  (descrizioni delle scadenze, nomi, matricole, periodi dei cedolini) si compongono nel codice dai servizi del modulo,
  con la stessa visibilità delle pagine.
- **Cedolini (HR-D6)**: nessun comando sui cedolini altrui né sui caricamenti (anche per l'ufficio del personale e lo
  studio: si usano le pagine). Al dipendente "I miei cedolini" dice solo quali periodi sono pubblicati e cosa è da
  confermare, cioè ciò che la pagina già mostra; il download e la conferma restano nella pagina.
- **Ricerca all'esecuzione, non nei validatori**: dipendente, corso, voce e piano si cercano tra quelli visibili
  (uguale → contenuto → tutte le parole); con più risultati la chat elenca i candidati. Così la prova delle frasi
  funziona con i servizi finti.
- **Scadenze dallo scadenzario comune**: è l'unico punto con le scadenze dei corsi e delle voci (il modulo non ha un
  elenco proprio); si filtra su `Application = HumanResources`, quindi non si mescolano con quelle di altri moduli.
- **Niente cancellazioni** né annullamenti di checklist, né scritture su corsi, edizioni, esiti, attestati, account o
  anagrafica: hanno effetti su competenze, scadenze, account e prese visione e vanno fatte dalle pagine.
  🔍 Da valutare: iscrizione di un dipendente a un'edizione ("iscrivi Rossi al corso sicurezza del 10 dicembre"),
  richiesta di presa visione di una voce.
- **Contratti, visite mediche, documenti in scadenza** non sono dati di Risorse Umane: i documenti in scadenza sono
  del Documentale (`DocExpiring`), visite e sicurezza della Compliance.

## 5. Checklist

- [x] Studio del modulo (servizi, permessi, pagine, menu) e delle chiavi già usate da presenze, CRM, Documentale, Infrastructure
- [x] R1 Scadenze HR (`HrExpiries`)
- [x] R2 Corsi da fare dai piani di formazione (`HrTrainingToDo`)
- [x] R3 Onboarding in corso (`HrOnboarding`)
- [x] R4 Scheda HR del dipendente (`HrEmployeeInfo`)
- [x] R5 I miei cedolini (`HrMyPayslips`)
- [x] W1 Completa voce di checklist (`HrCompleteChecklistItem`)
- [x] W2 Corso nel piano di formazione (`HrAddPlanCourse`)
- [x] O1 Entità apribili di Risorse Umane
- [x] O2 Pagine di Risorse Umane per Navigate (con le pagine per ruolo di "cedolini")
- [x] Testi `HumanResourcesMessages.resx` / `.en.resx` (chiavi `Chat*`)
- [x] Frasi di prova `tools/ChatAiPhraseTest/frasi/risorseumane.json` (servizi finti: nessuna aggiunta necessaria)
- [x] `HR-D6` e `HR-BASE.3` aggiornati in `ANALISI_HUMANRESOURCES_APPLICATIVO.md`
- [x] Build del server e del programma di prova delle frasi
- [ ] Prova delle frasi con il LLM e prova nell'app (anche a voce in Chrome/Edge), da fare in "Applicativo completo" per le chiavi condivise ("cedolini" con il Documentale, "formazione"/"corsi")
- [ ] 🔍 Iscrizione a un'edizione e richiesta di presa visione a voce; "I miei corsi" quando ci sarà la pagina (`HR-SEG.1`)
