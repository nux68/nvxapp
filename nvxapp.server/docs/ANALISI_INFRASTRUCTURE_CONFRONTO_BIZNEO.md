# ANALISI INFRASTRUCTURE – Confronto funzionale nvxapp vs Bizneo HR

Data: 20/11/2026 · Stato documento: **prima stesura**

## 1. Scopo e fonti

Confrontare le funzioni dell'applicativo completo nvxapp (branch `AllFunction`) con la suite **Bizneo HR**
(pagina prezzi https://www.bizneo.com/it/prezzi/), individuare cosa manca e pianificare i lavori.

Fonti:
- **Bizneo**: pagina prezzi letta il 20/11/2026. I prezzi per dipendente non sono pubblicati
  ("Piani a partire da 5 €", poi preventivo con "Contattaci"). La tabella "Confronta le funzionalità" non
  espone nel testo le spunte per piano, quindi l'attribuzione Standard/Pro delle singole voci non è nota.
  Le voci di Performance, Obiettivi, Compensation, ATS, LMS, Comunicazione interna, Canale segnalazioni e API
  vengono da una lettura parziale della pagina: **da confermare con una demo** (vedi BIZ-00).
- **nvxapp**: controller in `nvxapp.server/Controllers/<Modulo>/`, menu client
  `nvxapp.client/src/nvx/Utility/infrastructure/main-menu-<modulo>.service.ts`, documenti `ANALISI_*.md`
  (stato riassunto in `ANALISI_INFRASTRUCTURE_NUOVI_MODULI.md` §10.1).

### 1.1 Piani Bizneo

| Piano | Contenuto |
|---|---|
| **Standard** | Suite completa Tempo (assenze, presenze, turni, banca ore), Gestione del team (chat, progetti, helpdesk, 1:1, sondaggi, wiki), Talento (competenze, performance, obiettivi, compensation), Reclutamento, Formazione, Finanza (spese, buste paga, budget) |
| **Pro** | Standard + **firme digitali**, **API**, **report personalizzati**, **connettore paghe di terze parti** |

Prezzo: da 5 € (unità e minimi non pubblicati), preventivo su richiesta.

## 2. Tabella comparativa

Legenda: ✅ presente · 🟡 parziale o già pianificato · ❌ assente · 🔍 da verificare nel codice

### 2.1 Tempo – Assenze e ferie

| Funzione Bizneo | nvxapp | Dove / note |
|---|---|---|
| Richieste ferie, permessi, malattia con approvazione | ✅ | `Dip_GG_RichiestaController`, `/attendancerequests`, hub `/requestsadmin` |
| Piano ferie / calendario assenze | ✅ | `VacationPlanController`, `/vacationplan` |
| Assenze a ore, richieste multi-giorno con calcolo dell'orario | 🔍 | giustificativi e motore `TimeSheet_Engine` presenti, verificare il caso multi-giorno a ore |
| Maturazione per anzianità, riporto residui, liquidazione | 🔍 | contatori presenti, regole di maturazione da verificare |
| Approvazioni multilivello per tipo di assenza | 🟡 | workflow unico di approvazione, nessuna catena configurabile |
| Blocco date, limiti, giorni di preavviso | 🔍 | |
| Assenteismo predittivo (IA) | ✅ | Anomalie AI: previsione delle assenze (`Dip_AnomalieController`) |

### 2.2 Tempo – Rilevazione presenze

| Funzione Bizneo | nvxapp | Dove / note |
|---|---|---|
| Timbratura web | ✅ | `/timeclockuser` |
| Timbratura da app mobile iOS/Android | 🟡 | client Ionic+Capacitor responsive, ma app non pubblicata (appId `io.ionic.starter`), nessuna PWA |
| Kiosk con PIN o QR | ✅ | `/timbraturakiosk`, `/timbraturaqr`, QR dinamico firmato anti-replay |
| Geolocalizzazione restrittiva | ✅ | geofence per sede Nessuna/Segnala/Blocca (`Az_SediTimbratura`) |
| Timbratura offline | ✅ | coda offline con timbrature differite |
| Anomalie (15+) con escalation dipendente → manager | 🟡 | Anomalie AI presenti; soglie per azienda pianificate; escalation automatica da verificare |
| Tolleranze, arrotondamenti, puntualità | ✅ | `Par_*` arrotondamenti, profili orari |
| Pause manuali/automatiche/non retribuite | ✅ | Pause fase 1; stato "in pausa" e soglie a scaglioni pianificati (fase 2) |
| Smart working | ✅ | accordi SW con presa visione, `/lavoroagile` |
| Più politiche di timbratura | ✅ | profili orari / modalità per sede |
| Timbratura con carta NFC | 🟡 | badge sul kiosk; NFC da smartphone assente |
| Integrazione biometrici / controllo accessi (Suprema, ZKTeco) | ❌ | |
| Rapporto di audit PDF firmabile | 🟡 | PDF cartellino presente; firma FES esiste nel DM ma non collegata |
| Orari speciali (reperibilità, straordinari, notturni, festivi) | 🟡 | causali e formule personalizzate; reperibilità dedicata da verificare |
| Buoni pasto, ore contrattuali | 🟡 | buono pasto pianificato (Pause fase 2); ore contrattuali nel rapporto di lavoro |
| Report saldo ore e classifica anomalie | ✅ | quadratura mensile, contatori, statistiche |

### 2.3 Tempo – Turni e banca ore

| Funzione Bizneo | nvxapp | Dove / note |
|---|---|---|
| Agenda turni giorno/settimana/mese | ✅ | `/pianificazioneturni`, `/mieiturni` |
| Coperture minime / contatori personale | ✅ | coperture minime in pianificazione |
| Scambio/cessione turni con approvazione | ✅ | `Dip_TurnoScambioController` |
| Copia/incolla, azioni massive | 🔍 | |
| Turni rotativi programmabili | 🔍 | |
| Turni per luogo/progetto | 🔍 | sedi e commesse esistono, legame col turno da verificare |
| Istogramma di copertura, ottimizzatore conflitti | 🟡 | coperture minime sì, istogramma no |
| Generazione automatica turni (IA, smart planner) | ❌ | |
| Banca ore con maggiorazioni combinate e compensazione massiva | 🟡 | contatori e formule; compensazione massiva da verificare |

### 2.4 Gestione del team

| Funzione Bizneo | nvxapp | Dove / note |
|---|---|---|
| Chat interna tra dipendenti (canali, file, thread) | ❌ | c'è solo la chat AI (`ChatAIController`); SignalR già disponibile |
| Progetti e attività con ore e costo orario | 🟡 | commesse/sottocommesse con ore e marginalità (`/jobmargins`); niente task, kanban, deadline |
| Helpdesk interno (ticket dipendente → HR, SLA) | 🟡 | ticket di assistenza software e assistenza clienti CRM; manca l'helpdesk HR interno |
| 1:1 manager-dipendente | ❌ | |
| Sondaggi / clima (anonimi, heatmap per reparto) | ❌ | |
| Wiki aziendale | 🟡 | manuali utente e archivio DM; nessuna wiki con versioni e commenti |
| Comunicazione interna / bacheca | 🟡 | notifiche, prese visione, documenti con destinatari, digest AI; nessuna bacheca news |

### 2.5 Talento

| Funzione Bizneo | nvxapp | Dove / note |
|---|---|---|
| Dizionario competenze, livelli richiesti per posizione | 🟡 | anagrafica competenze (`CommonOrganizzazioneController`); niente posizioni/livelli/gap |
| Valutazione performance (360°, calibrazione, 9-box) | ❌ | non presente né pianificata |
| Obiettivi / OKR a cascata | ❌ | |
| Compensation, budget salariale, simulatore aumenti | ❌ | |
| Rischio turnover predittivo | ❌ | |
| Piani di carriera / PDI | ❌ | |
| Organigramma | 🟡 | albero sedi/reparti; nessun organigramma gerarchico grafico (responsabile diretto) |

### 2.6 Reclutamento, formazione, onboarding

| Funzione Bizneo | nvxapp | Dove / note |
|---|---|---|
| ATS: posizioni, candidature, pipeline, pagina carriere | ❌ | nessuna traccia né pianificazione |
| Agenti IA recruiting, video-interviste, psicometria, WhatsApp | ❌ | fuori perimetro (vedi §5) |
| Gestione corsi, edizioni, attestati con scadenza, piani formativi | ✅ | HR Formazione `/hrcourselist`, `/hrplanlist` |
| Corsi obbligatori sicurezza | ✅ | Compliance 81/08 |
| LMS e-learning (contenuti, quiz, corsi generati con IA) | ❌ | |
| "I miei corsi" lato dipendente | 🟡 | pianificato `HR-SEG.1` |
| Onboarding/offboarding con checklist e automazioni | ✅ | `/hrchecklistlist`, creazione/disattivazione account |

### 2.7 Finanza, documenti, piattaforma

| Funzione Bizneo | nvxapp | Dove / note |
|---|---|---|
| Note spese con approvazione | ✅ | `Dip_GG_NotaSpesaController`, con rimborsi chilometrici |
| Budget (spese/reparto) | ❌ | |
| Distribuzione buste paga | ✅ | portale cedolini e CU con divisione PDF e presa visione, anche per lo studio |
| Connettore paghe di terze parti (Pro) | 🟡 | export causali con tracciati configurabili e AI; nessun connettore diretto a software paghe |
| Scheda dipendente (People Central) | ✅ | `Dip_Anagrafica`, `Dip_RapportoLavoro`, scheda HR, import dipendenti |
| Archivio documenti, versioni, ricerca | ✅ | DM: cartelle, tipologie, conservazione, full-text, registro accessi |
| Firma digitale (Pro) | 🟡 | FES con un clic presente; OTP, firma avanzata/qualificata pianificate |
| Invio documenti massivo | 🟡 | cedolini sì; distribuzione massiva DM pianificata |
| Canale segnalazioni (whistleblowing) | 🟡 | pianificato `CMP-WB.1-3`, bloccato da `ANA.5`/`ANA.6` |
| Report predefiniti e dashboard | ✅ | report per modulo, dashboard a widget configurabili |
| Report personalizzati (Pro) | ❌ | nessun report builder |
| API pubblica (Pro) | 🟡 | server MCP e feed iCal; nessuna API REST pubblica con chiavi/webhook |
| SSO | ❌ | non rilevato |
| Automazioni / workflow configurabili | 🟡 | scadenzario e notifiche automatiche; nessun builder di workflow |
| App mobile dipendente e portale manager | 🟡 | portale web responsive; app store assente |
| Sincronizzazione calendari | 🟡 | feed .ics in uscita; OAuth Google/Microsoft pianificato (`INF-CAL.4`) |
| Multi-lingua | ✅ | it/en |

### 2.8 Riepilogo copertura

Conteggio sulle 70 righe delle tabelle §2.1–2.7:

| Stato | Righe | % |
|---|---|---|
| ✅ presente | 24 | 34% |
| 🟡 parziale/pianificato | 24 | 34% |
| ❌ assente | 16 | 23% |
| 🔍 da verificare | 6 | 9% |

Per area: **Tempo** e **documenti/buste paga** sono quasi alla pari; i buchi si concentrano in
**Talento** (performance, obiettivi, compensation), **Reclutamento**, **Gestione del team**
(chat, sondaggi, 1:1) e nella **piattaforma** (app mobile, API REST, SSO, report builder).

### 2.9 Distacco da Bizneo (copertura pesata)

Peso: ✅ = 1 · 🟡 = 0,5 · 🔍 = 0,5 · ❌ = 0, sulle 70 funzioni di §2.1–2.7.

| Area Bizneo | Righe | ✅ | 🟡 | 🔍 | ❌ | Copertura |
|---|---|---|---|---|---|---|
| Rilevazione presenze (§2.2) | 16 | 9 | 6 | 0 | 1 | **75%** |
| Assenze e ferie (§2.1) | 7 | 3 | 1 | 3 | 0 | **71%** |
| Turni e banca ore (§2.3) | 9 | 3 | 2 | 3 | 1 | **61%** |
| Finanza, documenti, piattaforma (§2.7) | 17 | 6 | 8 | 0 | 3 | **59%** |
| Reclutamento, formazione, onboarding (§2.6) | 7 | 3 | 1 | 0 | 3 | **50%** |
| Gestione del team (§2.4) | 7 | 0 | 4 | 0 | 3 | **29%** |
| Talento (§2.5) | 7 | 0 | 2 | 0 | 5 | **14%** |
| **Totale** | **70** | **24** | **24** | **6** | **16** | **≈ 56%** |

**Lettura**
- **Presenze, ferie e documenti:** il nucleo per cui di solito si compra un gestionale HR è quasi alla pari.
  Il distacco qui è fatto di rifiniture (buoni pasto, maturazione ferie, turni generati in automatico),
  non di moduli interi.
- **Talento, gestione del team e reclutamento:** qui sta il ritardo vero. Mancano moduli interi:
  valutazioni, obiettivi, sondaggi, chat, ATS.
- **Piattaforma:** per il cliente il segnale più visibile è l'assenza di un'app negli store.
  Bizneo la mette in primo piano.

**Proiezione della copertura completando la checklist (§7)**

| Traguardo | Copertura stimata |
|---|---|
| Oggi | ≈ 56% |
| Completati i punti P1 (BIZ-01…08) | ≈ 66% |
| Completati anche i punti P2 (BIZ-09…19) | ≈ 81% |
| Completati anche i punti P3 (BIZ-20…28) | ≈ 95–100% |

**Considerazione sul perimetro:** la percentuale misura solo le funzioni di Bizneo. Non conta ciò che
nvxapp ha e Bizneo no (§3): CRM, Compliance 81/08 e GDPR, risorse aziendali, gestione centro → studio
→ aziende, chat AI vocale, MCP, controllo di gestione. Confrontate come suite per **studi paghe e PMI**,
anziché come HR "puro", le due offerte risultano molto più equilibrate.

## 3. Dove nvxapp è più avanti di Bizneo

| Area | nvxapp | Bizneo |
|---|---|---|
| Multi-tenant centro → studio → aziende | lo studio paghe gestisce cedolini, GDPR e documenti per le sue aziende; rivendita tramite centri con ripartizione incassi | suite per singola azienda |
| CRM | clienti, lead, pipeline, preventivi PDF, assistenza clienti | assente |
| Compliance | sicurezza 81/08 (visite, DPI, nomine, DVR), registro GDPR con violazioni 72h | solo canale segnalazioni |
| Risorse aziendali | flotta, fringe benefit, multe, beni assegnati, prenotazioni sale/auto | assente |
| AI operativa | chat con comandi vocali su tutti i moduli, server MCP per AI esterne, formule ed export paghe con AI, digest settimanale | IA su assenteismo, turni, wiki, recruiting |
| Controllo di gestione | commesse, ore, marginalità | progetti con costo orario |
| Scadenzario unico | scadenze di tutti i moduli con promemoria e feed calendario | — |
| Fatturazione (futura) | modulo Administration con SdI pianificato | assente |

Questi punti vanno valorizzati nel posizionamento: nvxapp è una suite per **studi paghe/consulenti e PMI**,
Bizneo è una suite HR "pura" per l'azienda.

## 4. Gap analysis e priorità

Criterio: valore commerciale × base già presente nel codice. Branch di pertinenza indicato per ogni punto.

### P1 – Alta priorità (gap visibili, base già pronta o obbligo normativo)

| ID | Lavoro | Branch | Motivo |
|---|---|---|---|
| BIZ-01 | **App mobile pubblicata** (appId, icone, push, build Android/iOS) + manifest PWA e service worker | Infrastructure_Dev | Bizneo vende la timbratura da app; Capacitor è già nel client |
| BIZ-02 | **Whistleblowing** D.Lgs 24/2023 (canale anonimo, crittografia, termini 7/90 gg) | Compliance_Dev | obbligatorio per aziende ≥ 50 dipendenti; già pianificato `CMP-WB` |
| BIZ-03 | **Valutazione delle performance** (campagne, modelli, auto/manager/360°, 9-box base) | HumanResources_Dev | gap più evidente nell'area Talento |
| BIZ-04 | **Obiettivi/OKR** collegati alle valutazioni | HumanResources_Dev | completa BIZ-03 |
| BIZ-05 | **Firma OTP/avanzata + distribuzione massiva** documenti | DocumentManagement_Dev | firma è funzione Pro Bizneo; già pianificata |
| BIZ-06 | **Buoni pasto e banca ore** con compensazione | AttendanceTracking_Dev | già pianificato (Pause fase 2) |
| BIZ-07 | **Sondaggi e clima** (anonimato, eNPS, heatmap per reparto, sondaggi automatici onboarding/offboarding) | Infrastructure_Dev | trasversale a tutti i moduli |
| BIZ-08 | **Bacheca comunicazioni interne** con destinatari e presa visione | Infrastructure_Dev | riusa prese visione e notifiche |

### P2 – Media priorità

| ID | Lavoro | Branch |
|---|---|---|
| BIZ-09 | **Recruiting/ATS base**: posizioni, pagina carriere pubblica, candidature con CV, pipeline kanban, conversione in dipendente → onboarding | HumanResources_Dev |
| BIZ-10 | **Organigramma gerarchico** grafico (responsabile diretto, usato anche nelle approvazioni) | Infrastructure_Dev |
| BIZ-11 | **Approvazioni multilivello** configurabili per tipo di richiesta | Infrastructure_Dev |
| BIZ-12 | **Helpdesk HR interno** (richieste dipendente → HR con categorie, SLA, kanban) | Infrastructure_Dev |
| BIZ-13 | **Report builder** personalizzati + export Excel uniforme | Infrastructure_Dev |
| BIZ-14 | **API REST pubblica** con chiavi e webhook + **SSO** Microsoft/Google (OIDC) | Infrastructure_Dev |
| BIZ-15 | **Turni**: rotazioni programmabili, istogramma copertura, generazione automatica con AI | AttendanceTracking_Dev |
| BIZ-16 | **Regole ferie**: maturazione per anzianità, riporto, blocco date, preavviso | AttendanceTracking_Dev |
| BIZ-17 | **Competenze per posizione** (livelli richiesti, gap, collegamento a formazione) | HumanResources_Dev |
| BIZ-18 | **Sincronizzazione calendari OAuth** (già `INF-CAL.4`) | Infrastructure_Dev |
| BIZ-19 | **Audit PDF firmabile** del cartellino (PDF + FES del DM) | AttendanceTracking_Dev |

### P3 – Bassa priorità / da valutare

| ID | Lavoro | Branch |
|---|---|---|
| BIZ-20 | Chat interna tra dipendenti (canali, file) su SignalR | Infrastructure_Dev |
| BIZ-21 | Wiki aziendale con versioni e commenti | DocumentManagement_Dev |
| BIZ-22 | 1:1 manager-dipendente (agenda, note, impegni) | HumanResources_Dev |
| BIZ-23 | Attività/kanban sulle commesse | AttendanceTracking_Dev |
| BIZ-24 | LMS e-learning (contenuti, quiz, corsi generati con AI) | HumanResources_Dev |
| BIZ-25 | Compensation e budget salariale | HumanResources_Dev |
| BIZ-26 | Integrazione hardware (biometrici, controllo accessi, NFC) | AttendanceTracking_Dev |
| BIZ-27 | Rischio turnover predittivo con AI | HumanResources_Dev |
| BIZ-28 | Budget spese per reparto | AttendanceTracking_Dev (o Administration) |

## 5. Fuori perimetro (per ora)

Psicometria, video-interviste, agenti IA di recruiting, canale WhatsApp, calcolo delle paghe
(neanche Bizneo lo fa: usa un connettore verso il software paghe; nvxapp ha già l'export causali).

## 6. Decisioni aperte

- **D1**: Recruiting dentro HumanResources o nuovo applicativo dedicato (`ApplicationType` separato, schema proprio)?
- **D2**: Sondaggi/bacheca in Infrastructure (comuni) o in HumanResources?
- **D3**: Livello di licenza (Free/Standard/Pro) delle nuove funzioni, allineato a `ANA.10`.
- **D4**: App mobile: solo Android, o anche iOS (account Apple developer)?

## 7. Checklist lavori e avanzamento

Avanzamento: **1 / 31** punti completati (analisi redatta).

### Preliminari
- [x] Redazione del documento di confronto (20/11/2026)
- [ ] BIZ-00 Verificare nel codice le righe 🔍 (§2.1, §2.3) e confermare con una demo Bizneo le funzioni per piano
- [ ] Decisioni D1–D4 (§6)

### P1
- [ ] BIZ-01 App mobile pubblicata + PWA — Infrastructure_Dev
- [ ] BIZ-02 Whistleblowing D.Lgs 24/2023 — Compliance_Dev (sblocco `ANA.5`/`ANA.6`)
- [ ] BIZ-03 Valutazione delle performance — HumanResources_Dev
- [ ] BIZ-04 Obiettivi/OKR — HumanResources_Dev
- [ ] BIZ-05 Firma OTP/avanzata + distribuzione massiva — DocumentManagement_Dev
- [ ] BIZ-06 Buoni pasto e banca ore — AttendanceTracking_Dev
- [ ] BIZ-07 Sondaggi e clima — Infrastructure_Dev
- [ ] BIZ-08 Bacheca comunicazioni interne — Infrastructure_Dev

### P2
- [ ] BIZ-09 Recruiting/ATS base — HumanResources_Dev
- [ ] BIZ-10 Organigramma gerarchico — Infrastructure_Dev
- [ ] BIZ-11 Approvazioni multilivello — Infrastructure_Dev
- [ ] BIZ-12 Helpdesk HR interno — Infrastructure_Dev
- [ ] BIZ-13 Report builder + export Excel — Infrastructure_Dev
- [ ] BIZ-14 API REST pubblica + SSO — Infrastructure_Dev
- [ ] BIZ-15 Turni avanzati con AI — AttendanceTracking_Dev
- [ ] BIZ-16 Regole ferie avanzate — AttendanceTracking_Dev
- [ ] BIZ-17 Competenze per posizione — HumanResources_Dev
- [ ] BIZ-18 Sync calendari OAuth (`INF-CAL.4`) — Infrastructure_Dev
- [ ] BIZ-19 Audit PDF firmabile del cartellino — AttendanceTracking_Dev

### P3
- [ ] BIZ-20 Chat interna tra dipendenti — Infrastructure_Dev
- [ ] BIZ-21 Wiki aziendale — DocumentManagement_Dev
- [ ] BIZ-22 1:1 manager-dipendente — HumanResources_Dev
- [ ] BIZ-23 Attività/kanban sulle commesse — AttendanceTracking_Dev
- [ ] BIZ-24 LMS e-learning — HumanResources_Dev
- [ ] BIZ-25 Compensation e budget salariale — HumanResources_Dev
- [ ] BIZ-26 Integrazione hardware presenze — AttendanceTracking_Dev
- [ ] BIZ-27 Turnover predittivo AI — HumanResources_Dev
- [ ] BIZ-28 Budget spese per reparto — AttendanceTracking_Dev / Administration
