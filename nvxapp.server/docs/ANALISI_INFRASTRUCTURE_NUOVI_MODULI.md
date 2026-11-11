# Analisi – Nuovi moduli e raggruppamento logico delle funzioni

Data: 11/11/2026. Ramo: **Infrastructure_Dev** (lavoro su `Infrastructure_Dev_moduli`, worktree `wt_infra_moduli`).
Stato: **solo analisi, nessuna modifica al codice**. **Decisioni D1–D8 prese l'11/11/2026** (§9): si creano subito Risorse Umane, Compliance e Risorse aziendali; Amministrazione più avanti e solo con la fatturazione. I livelli di licenza dei nuovi moduli si definiranno con i listini.

**Scopo.** Raccogliere le funzioni che possono interessare le aziende clienti e assegnarle a un gruppo logico, cioè a un applicativo con un proprio branch. Le funzioni vanno nei gruppi esistenti (Presenze, Documentale, CRM) quando ci stanno; altrimenti si propongono gruppi nuovi. Le parti comuni vanno nelle funzioni base (Infrastructure).
Il documento sta in Infrastructure perché ogni gruppo nuovo richiede lavoro di base: valore dell'enum, DbContext e schema, licenze, menu, dati spostati in Common. Il dettaglio di ogni modulo andrà poi in un'analisi del suo ramo (`ANALISI_<RAMO>_APPLICATIVO.md`).

---

## 1. Situazione di partenza

- Gli applicativi sono definiti in `nvxapp.server.data/Infrastructure/Tenancy/ApplicationType.cs`:
  `Common = 0`, `Moke = 1`, `AttendanceTracking = 2`, `Crm = 3`, `DocumentManagement = 4`.
  Il valore numerico entra nel nome dello schema (`tenant_<IdAzienda>_<valore>`, `TenantSchemaName.For`) e nella tabella `__EFMigrationsHistory_<applicativo>`: **non si cambia mai** per un applicativo esistente.
- Ogni azienda attiva gli applicativi che usa (`CompanyApplication`). Gli studi hanno l'equivalente in `FinancialAdvisorApplication`. Le licenze (`ANALISI_INFRASTRUCTURE_LICENZE.md`) e `ILicenseGuard` lavorano per applicativo.
- Lo schema **Common** (`tenant_<id>_0`) esiste sempre e contiene `Az_Anagrafica`, `Az_Cliente`, `Az_ClienteContatto` e `Dip_Anagrafica` (`Entities/Tenant/Common`). Nelle Presenze restano la parte specifica del dipendente (`Dip_Anagrafica.Presenze.cs`), le sedi e i reparti (`Az_Sedi*`), le commesse (`Az_Commessa`, `Az_SubCommessa*`) e le competenze (`Dip_Competenza`, `Par_Competenza`).
- Sono già comuni a tutti i moduli: chat AI e comandi vocali, widget della dashboard, ticket, manuali, storage, cancellazione a cascata (`DeletionService`), licenze.
- Gerarchia commerciale: SuperUser → Dealer (centro) → FinancialAdvisor (studio) → Company (azienda) → User. Lo **studio** è il canale principale per vendere i moduli alle aziende.

## 2. Criteri di raggruppamento

1. Un gruppo è un'area che **un'azienda acquista come prodotto** e di cui ha un responsabile chiaro (ufficio del personale, RSPP/DPO, amministrazione, ufficio acquisti e logistica).
2. Una funzione va nel gruppo **di cui usa i dati propri**. I dati che servono a più gruppi vanno in Common, non vengono duplicati.
3. Un modulo **non dipende** da un altro modulo opzionale. Le integrazioni tra moduli sono facoltative e si accendono solo se entrambi sono attivi (come in `ANALISI_CRM_APPLICATIVO.md` §2.6).
4. I servizi tecnici usati da più moduli (scadenze, notifiche, calendari, riepiloghi AI) stanno in **Infrastructure**.

## 3. Funzioni nei gruppi esistenti

### 3.1 Presenze (AttendanceTracking, valore 2)
| Funzione | Descrizione | Dati usati |
|---|---|---|
| Turni e pianificazione | Turni per reparto e sede, controllo delle coperture minime, scambio tra colleghi, confronto tra turni previsti e timbrature | Profili orario, `Az_Sedi*`, piano ferie |
| Timbratura evoluta | QR o NFC per sede, geofencing, postazione condivisa in modalità kiosk, PWA che timbra anche offline | `Dip_GG_Timbratura`, sedi |
| Smart working | Accordi individuali (con firma nel Documentale, se attivo), calendario del lavoro agile, giorni massimi | Causali, richieste |
| Analisi AI | Timbrature anomale, previsione delle assenze, giornate da sistemare prima dell'export paghe | `Dip_GG_*` |
| Controllo di gestione sulle commesse (base) | Ore lavorate per commessa per costo orario, più le note spese: costo e marginalità | Timesheet, `Az_Commessa`, `Dip_RapportoLavoro`, note spese |

### 3.2 Documentale (DocumentManagement, valore 4)
| Funzione | Descrizione |
|---|---|
| Acquisizione delle PEC | Lettura di una casella PEC, archiviazione come documenti, classificazione e assegnazione con AI |
| Conservazione a norma | Invio dei documenti a un conservatore accreditato, con stato e ricevute |
| Prese visione e verbali | Servizio di firma o presa visione usato dagli altri moduli (cedolini, consegna dei beni, DPI, accordi), basato sulla FES già realizzata |

### 3.3 CRM (Crm, valore 3)
| Funzione | Descrizione |
|---|---|
| Preventivi e offerte | Righe, listini, PDF e invio. Il preventivo accettato diventa un ordine in Amministrazione, se attivo |
| Sincronizzazione degli appuntamenti | Con Google e Outlook, tramite il motore comune (§5) |
| Assistenza ai clienti dell'azienda | Richieste dei clienti finali, diverse dai ticket di Infrastructure che riguardano la piattaforma |

## 4. Gruppi nuovi (decisi: D1, D3, D4)

| Gruppo | `ApplicationType` | Branch | Schema | Prefisso tabelle | Responsabile in azienda |
|---|---|---|---|---|---|
| Risorse Umane | `HumanResources = 5` | `HumanResources_Dev` → `HumanResources` | `tenant_<id>_5` | `Hr_` | Ufficio del personale, studio paghe |
| Compliance | `Compliance = 6` | `Compliance_Dev` → `Compliance` | `tenant_<id>_6` | `Cmp_` | RSPP, DPO, responsabile delle segnalazioni |
| Risorse aziendali | `Assets = 7` | `Assets_Dev` → `Assets` | `tenant_<id>_7` | `Ast_` | Ufficio acquisti, logistica, IT |
| Amministrazione | `Administration = 8` | `Administration_Dev` → `Administration` | `tenant_<id>_8` | `Adm_` | Amministrazione |

Valori dell'enum e nomi dei branch confermati (D4). Una volta in produzione **non si cambiano più**. HumanResources, Compliance e Assets si creano subito (D1). Amministrazione arriva più avanti, ma il valore 8 resta riservato.

### 4.1 Risorse Umane (HumanResources)
Il fascicolo del dipendente al di là delle presenze.
- **Portale del dipendente con cedolini e CU**: lo studio (o l'azienda) carica il PDF unico. Il sistema lo divide per dipendente (codice fiscale o matricola, con l'AI come aiuto), lo pubblica nell'area del dipendente e chiede la presa visione (servizio del Documentale se attivo, altrimenti una semplice conferma di lettura).
- **Onboarding e offboarding**: checklist per assunzione e cessazione, documenti da consegnare o firmare, creazione e disattivazione dell'account.
- **Formazione e competenze**: corsi, attestati, piani di formazione. Parte da `Dip_Competenza` e `Par_Competenza`, che si spostano in Common (§6).
- **Integrazioni facoltative**: Presenze (cessazione e chiusura del rapporto), Documentale (archiviazione e firma), Compliance (corsi obbligatori).

### 4.2 Compliance
Adempimenti normativi. È il modulo che lo studio può proporre a tutte le sue aziende.
- **Sicurezza sul lavoro (D.Lgs 81/08)**: scadenzario di corsi obbligatori, visite mediche e idoneità, consegna dei DPI (con verbale), nomine (RSPP, RLS, preposti, addetti alle emergenze), documenti DVR.
- **Whistleblowing (D.Lgs 24/2023)**, compreso nel modulo senza una licenza separata (D7): canale di segnalazione con anonimato, codice per seguire la propria segnalazione, scadenze di legge (avviso di ricezione entro 7 giorni, risposta entro 3 mesi) e accesso limitato al gestore. Serve una **verifica legale** prima del rilascio.
- **Registro GDPR**: registro dei trattamenti, nomine e responsabili esterni, registro delle violazioni. Lo studio può gestirlo per conto delle aziende.
- **Note**: i dati delle segnalazioni vanno protetti in modo particolare, con cifratura, accesso solo al gestore ed esclusione dalla chat AI. Serve un'analisi della sicurezza dedicata, che si aggancia a `piano-security.md`.

### 4.3 Risorse aziendali (Assets)
Cosa possiede l'azienda e a chi lo assegna.
- **Flotta**: veicoli, scadenze (bollo, assicurazione, revisione, tagliando), assegnazione, fringe benefit e multe. `Dip_RapportoAutovetture` si sposta in Common (D5): Assets lo gestisce e le Presenze continuano a leggerlo per le note spese (rimborsi chilometrici, `Dip_GG_NotaSpesa.IdDip_RapportoAutovetture`).
- **Beni assegnati**: PC, telefoni, badge e attrezzature, con consegna e restituzione (verbale firmato tramite il Documentale) e collegamento all'offboarding di HR.
- **Prenotazioni**: postazioni, sale e auto in pool, con calendario e sincronizzazione (§5).

### 4.4 Amministrazione (Administration) – più avanti (D1)
- **Solo fatturazione (D2)**: dal preventivo del CRM si passa a ordine, fattura, invio allo SdI tramite un intermediario e incassi.
- Il **controllo di gestione** resta nelle Presenze (§3.1). Collegarlo ai ricavi delle fatture sarà un'integrazione facoltativa, da valutare quando si avvia il modulo.
- **Note**: è il gruppo più costoso (fiscalità, intermediario SdI, conservazione). Si avvia dopo aver scelto l'intermediario SdI.

## 5. Funzioni base da aggiungere in Infrastructure

| Servizio | Usato da | Descrizione |
|---|---|---|
| Scadenzario comune | Compliance, Assets, Documentale, HR | Ogni modulo registra le proprie scadenze (fonte, data, responsabile). Un'unica pagina e un widget "In scadenza" le mostrano tutte |
| Notifiche | Tutti | Email (Brevo), notifiche nell'app (SignalR, `Hub/SignalRHub.cs`), in futuro push. Preferenze per utente |
| Sincronizzazione calendari | CRM, Presenze, Assets | Collegamento a Google e Microsoft (OAuth). Ogni modulo pubblica i propri eventi |
| Riepilogo AI periodico | Tutti | Riepilogo settimanale per l'azienda e per lo studio. Ogni modulo fornisce i propri dati, il motore usa il provider AI già configurato |
| Registrazione di un nuovo applicativo | Gruppi nuovi | Procedura documentata (§7) |

## 6. Dati da spostare in Common

Seguono la regola del §2.2: se un dato serve a più moduli, va in Common e lo sposta Infrastructure prima che i moduli lo usino. Si fa come per `Dip_Anagrafica` (`ANALISI_CRM_APPLICATIVO.md` §2.4).
Le entità delle Presenze esistono solo nel branch AttendanceTracking: lo spostamento si divide in `INF-COM` (nuove entità in Common, su Infrastructure_Dev) e `AT-COM` (rimozione dal contesto delle Presenze, migrazione dei dati, adeguamento del codice, su AttendanceTracking_Dev), §10.

| Dato | Oggi | Serve a | Scelta |
|---|---|---|---|
| Sedi e reparti (`Az_Sedi`, `Az_SediReparto`) | Presenze | HR, Compliance (sicurezza per sede), Assets (beni per sede) | Spostarli in Common. Le tabelle di collegamento proprie delle Presenze (`Az_SediRepartoUser`, `Az_SediAttivita`…) restano nelle Presenze |
| Commesse (`Az_Commessa`, `Az_SubCommessa`) | Presenze | CRM (opportunità vinta che diventa commessa), in futuro Amministrazione | Restano nelle Presenze, dove sta il controllo di gestione (D2). Da rivalutare quando si avvia Amministrazione |
| Competenze (`Par_Competenza`, `Dip_Competenza`) | Presenze | HR, Compliance | Spostarle prima di avviare HR |
| Autovetture del dipendente (`Dip_RapportoAutovetture`) | Presenze | Assets (flotta), Presenze (note spese, rimborsi km) | **Deciso (D5)**: in Common, legata a `Dip_Anagrafica` (`IdDip_Anagrafica`) invece che al rapporto di lavoro, che resta nelle Presenze |
| Rapporto di lavoro | Presenze (`Dip_RapportoLavoro`) | HR (onboarding e cessazione) | **Deciso (D6)**: in Common vanno **solo le date di assunzione e cessazione**, come campi di `Dip_Anagrafica` (`DataAssunzione`, `DataCessazione`). Il resto del rapporto resta nelle Presenze |

## 7. Cosa serve per aggiungere un gruppo nuovo

Questa è la procedura base, da ripetere per ogni gruppo:
1. **Infrastructure_Dev**: nuovo valore in `ApplicationType`, voce nel listino e nelle licenze, testo del menu e della pagina di attivazione dell'applicativo, cancellazione (`DeletionService`, eliminazione dello schema), manuali.
2. **Nuovo branch** `X_Dev`, creato da Infrastructure dopo il punto 1, e il branch `X`.
3. In `X_Dev`: `XDbContext` registrato con `AddApplicationDbContext<XDbContext>(ApplicationType.X)` (vedi `DataLayerInstaller.cs`), migration iniziale, controller in `Controllers/X`, servizi in `ClientServer-Service/X`, pagine Angular, widget (`widget-x.service.ts`), comandi vocali, voci di `ILicenseGuard`.
4. **CLAUDE.md**: aggiungere il gruppo all'elenco dei branch e alla catena di AllFunction (Infrastructure per primo, poi AttendanceTracking, CRM, DocumentManagement e i gruppi nuovi).
5. **AllFunction_Dev**: merge e verifica di build e migration con tutti gli applicativi attivi.

## 8. Ordine di lavoro e parallelismo

Il lavoro è diviso in **aree** con un'etichetta (es. `INF-COM`) e punti numerati (es. `INF-COM.3`), elencati nel §10. Ogni area si può portare avanti in un **worktree dedicato**. Le aree della stessa fase non dipendono l'una dall'altra e si possono fare in parallelo. Una fase comincia quando le aree da cui dipende sono state mergiate.

| Fase | Aree (in parallelo) | Prerequisiti |
|---|---|---|
| **1** | `ANA`, `INF-APP`, `INF-COM`, `INF-SVC` · `AT-TURNI`, `AT-TIMB`, `AT-SW`, `AT-AI`, `AT-CDG` · `DM-PEC`, `DM-CONS` · `CRM-PREV`, `CRM-ASS` | Nessuno |
| **2** | `AT-COM`, `INF-CAL`, `DM-FIRMA`, `CMP-BASE`, `HR-BASE`, `AST-BASE` | `AT-COM` ← `INF-COM` · `DM-FIRMA` ← `INF-SVC` · `*-BASE` ← `INF-APP` |
| **3** | `CMP-81`, `CMP-WB`, `CMP-GDPR` · `HR-CED`, `HR-FOR`, `HR-ONB` · `AST-FLO`, `AST-BENI`, `AST-PREN` · `CRM-CAL`, `INF-AI` | Vedi la colonna "Dipende da" del §10.1 |
| Più avanti | `ADM` | Scelta dell'intermediario SdI |

Amministrazione arriva più avanti, solo con la fatturazione (D1, D2).

## 9. Decisioni (prese l'11/11/2026)

| # | Decisione | Esito |
|---|---|---|
| D1 | Quali gruppi nuovi creare | **HR, Compliance e Assets** si creano subito. **Amministrazione più avanti** |
| D2 | Tenere Amministrazione separata o lasciare il controllo di gestione nelle Presenze | **Amministrazione solo con la fatturazione**. Il controllo di gestione resta nelle Presenze |
| D3 | Unire Assets dentro HR (meno branch) | **No, gruppi separati**: chi compra è diverso (logistica e IT contro personale) |
| D4 | Valori dell'enum e nomi dei branch | **Confermati quelli del §4** |
| D5 | Dove tenere `Dip_RapportoAutovetture` | **In Common**: lo usano Assets e Presenze |
| D6 | Quali dati del rapporto di lavoro spostare in Common | **Solo le date di assunzione e cessazione** |
| D7 | Whistleblowing nel modulo Compliance o venduto a parte | **Nel modulo Compliance**, senza una licenza separata |
| D8 | Livelli Free/Standard/Pro per i nuovi moduli | **Da definire con i listini**. Se serve si aggiungeranno in seguito; per ora nessun livello specifico |
| D9 | Sincronizzazione dei calendari: OAuth con Google/Microsoft o feed iCalendar | **Da confermare con l'utente.** Per ora **feed iCalendar (.ics) personale in sola lettura**, sottoscritto via URL da Google Calendar, Outlook e Apple Calendar: OAuth richiede la registrazione di app e credenziali che non abbiamo. La sincronizzazione bidirezionale via OAuth resta un punto futuro (`INF-CAL.4`) |

---

## 10. Aree di lavoro e checklist

Legenda: ✅ svolto · ⏳ da fare · 🔄 in corso · 🔍 da decidere

### 10.0 Regole per lavorare su più worktree

1. **Un'area per worktree.** Ramo di lavoro `<Ramo>_Dev_<etichetta>` creato dal `_Dev` di competenza, worktree `../wt_<etichetta>` (es. `git worktree add -b Infrastructure_Dev_inf-com ../wt_inf-com Infrastructure_Dev`).
2. **Citare il punto.** Nei messaggi di commit si indica il punto (es. `INF-COM.1: sedi e reparti in Common`).
3. **Prima di iniziare** un'area si controlla che le aree della colonna "Dipende da" siano ✅ e mergiate nel `_Dev` di partenza. Se no, si fa prima il merge da Infrastructure (le aree INF arrivano agli altri rami solo attraverso la catena).
4. **Migration.** Due aree che aggiungono migration allo **stesso DbContext** (es. `INF-COM` e `INF-SVC` sul contesto Common) non vanno mergiate insieme senza controllo: dopo il secondo merge si rigenera la migration o si verifica lo snapshot.
5. **Checklist.** Questo documento sta in Infrastructure e i merge vanno solo da Infrastructure verso gli altri rami. Lo stato dei punti si aggiorna quindi **solo su `Infrastructure_Dev`**, con un piccolo commit dopo il merge dell'area, anche quando l'area è di un altro ramo. Il dettaglio di un modulo va nella sua analisi (`ANALISI_<RAMO>_*`).
6. **Fine area.** Catena dei merge (su conferma), poi si rimuovono il worktree e il ramo di lavoro.

### 10.1 Elenco delle aree

| Etichetta | Area | Branch (`_Dev`) | Worktree | Dipende da | Fase | Stato |
|---|---|---|---|---|---|---|
| `ANA` | Analisi di dettaglio e decisioni | Infrastructure_Dev (+ rami dei moduli) | `wt_ana` | – | 1 | 🔄 |
| `INF-APP` | Registrazione dei nuovi applicativi | Infrastructure_Dev | `wt_inf-app` | – | 1 | ✅ mergiata l'11/11/2026 |
| `INF-COM` | Dati spostati in Common (lato Infrastructure) | Infrastructure_Dev | `wt_inf-com` | – | 1 | ✅ mergiata l'11/11/2026 insieme ad `AT-COM` |
| `INF-SVC` | Scadenzario, notifiche, interfaccia di presa visione | Infrastructure_Dev | `wt_inf-svc` | – | 1 | ✅ mergiata l'11/11/2026 (server provato su DB usa e getta, client compilato) |
| `INF-CAL` | Sincronizzazione dei calendari | Infrastructure_Dev | `wt_inf-cal` | `INF-SVC` | 2 | ✅ mergiata l'11/11/2026 (feed .ics; OAuth = `INF-CAL.4`, decisione D9 da confermare) |
| `INF-AI` | Riepilogo AI periodico | Infrastructure_Dev | `wt_inf-ai` | `INF-SVC` | 3 | ✅ mergiata l'11/11/2026 |
| `AT-COM` | Dati spostati in Common (lato Presenze) | AttendanceTracking_Dev | `wt_at-com` | `INF-COM` | 2 | ✅ mergiata l'11/11/2026 (provata su DB nuovo, esistente multi-tenant e nuovo single-tenant) |
| `AT-TURNI` | Turni e pianificazione | AttendanceTracking_Dev | `wt_at-turni` | – | 1 | ⏳ |
| `AT-TIMB` | Timbratura evoluta | AttendanceTracking_Dev | `wt_at-timb` | – | 1 | ⏳ |
| `AT-SW` | Smart working | AttendanceTracking_Dev | `wt_at-sw` | – | 1 | ⏳ |
| `AT-AI` | Analisi AI delle timbrature | AttendanceTracking_Dev | `wt_at-ai` | – | 1 | ⏳ |
| `AT-CDG` | Controllo di gestione sulle commesse | AttendanceTracking_Dev | `wt_at-cdg` | – | 1 | ⏳ |
| `DM-PEC` | Acquisizione delle PEC | DocumentManagement_Dev | `wt_dm-pec` | – | 1 | ⏳ |
| `DM-CONS` | Conservazione a norma | DocumentManagement_Dev | `wt_dm-cons` | – | 1 | ⏳ |
| `DM-FIRMA` | Presa visione e verbali per gli altri moduli | DocumentManagement_Dev | `wt_dm-firma` | `INF-SVC` | 2 | ✅ svolta l'11/11/2026 nel ramo `DocumentManagement_Dev_dm-firma`, da mergiare (server provato su DB usa e getta con 35 controlli API, client compilato senza modifiche) |
| `CRM-PREV` | Preventivi e offerte | CRM_Dev | `wt_crm-prev` | – | 1 | ⏳ |
| `CRM-ASS` | Assistenza ai clienti dell'azienda | CRM_Dev | `wt_crm-ass` | – | 1 | ⏳ |
| `CRM-CAL` | Sincronizzazione degli appuntamenti | CRM_Dev | `wt_crm-cal` | `INF-CAL` | 3 | ✅ mergiata l'11/11/2026 |
| `CMP-BASE` | Avvio del modulo Compliance | Compliance_Dev (nuovo) | `wt_cmp-base` | `INF-APP` | 2 | ✅ fatta l'11/11/2026 (ramo `Compliance_Dev_cmp-base`, provata su DB usa e getta), da mergiare |
| `CMP-81` | Sicurezza sul lavoro 81/08 | Compliance_Dev | `wt_cmp-f3` | `CMP-BASE`, `INF-COM`, `INF-SVC` | 3 | ✅ fatta l'11/11/2026 (ramo `Compliance_Dev_f3`, provata su DB usa e getta), da mergiare |
| `CMP-WB` | Whistleblowing | Compliance_Dev | `wt_cmp-wb` | `CMP-BASE`, `ANA.5`, `ANA.6` | 3 | 🔍 bloccata: serve la verifica legale (`ANA.5`) e l'analisi di sicurezza (`ANA.6`) |
| `CMP-GDPR` | Registro GDPR | Compliance_Dev | `wt_cmp-f3` | `CMP-BASE` | 3 | ✅ fatta l'11/11/2026 (ramo `Compliance_Dev_f3`, con lato studio ed export Excel via job), da mergiare |
| `HR-BASE` | Avvio del modulo Risorse Umane | HumanResources_Dev (nuovo) | `wt_hr-base` | `INF-APP` | 2 | ✅ fatta l'11/11/2026 (ramo `HumanResources_Dev_hr-base`, provata su DB usa e getta), da mergiare |
| `HR-CED` | Portale con cedolini e CU | HumanResources_Dev | `wt_hr-ced` | `HR-BASE`, `INF-SVC` | 3 | ✅ fatta l'11/11/2026 (ramo `HumanResources_Dev_f3`, worktree `wt_hr-f3`, provata su DB usa e getta: 80 controlli), da mergiare |
| `HR-FOR` | Formazione e competenze | HumanResources_Dev | `wt_hr-for` | `HR-BASE`, `INF-COM`, `INF-SVC` | 3 | ✅ fatta l'11/11/2026 (ramo `HumanResources_Dev_f3`, worktree `wt_hr-f3`, provata su DB usa e getta: 80 controlli), da mergiare |
| `HR-ONB` | Onboarding e offboarding | HumanResources_Dev | `wt_hr-onb` | `HR-BASE`, `INF-COM` | 3 | ✅ fatta l'11/11/2026 (ramo `HumanResources_Dev_f3`, worktree `wt_hr-f3`, provata su DB usa e getta: 80 controlli), da mergiare |
| `AST-BASE` | Avvio del modulo Risorse aziendali | Assets_Dev (nuovo) | `wt_ast-base` | `INF-APP` | 2 | ✅ fatta l'11/11/2026 (ramo `Assets_Dev_ast-base`, provata su DB usa e getta), da mergiare |
| `AST-FLO` | Flotta | Assets_Dev | `wt_ast-flo` | `AST-BASE`, `INF-COM`, `INF-SVC` | 3 | ✅ mergiata l'11/11/2026 |
| `AST-BENI` | Beni assegnati | Assets_Dev | `wt_ast-beni` | `AST-BASE`, `INF-SVC` | 3 | ✅ mergiata l'11/11/2026 |
| `AST-PREN` | Prenotazioni | Assets_Dev | `wt_ast-pren` | `AST-BASE` (`INF-CAL` facoltativo) | 3 | ✅ mergiata l'11/11/2026 |
| `ADM` | Amministrazione (fatturazione) | Administration_Dev (più avanti) | `wt_adm` | `INF-APP`, scelta dell'intermediario SdI | – | ⏳ |

I branch `Compliance_Dev`, `HumanResources_Dev` e `Assets_Dev` (con `Compliance`, `HumanResources`, `Assets`) nascono in `INF-APP.6`, creati da Infrastructure dopo il merge di `INF-APP`.

### 10.2 Punti di ogni area

#### `ANA` – Analisi e decisioni
- ✅ `ANA.1` Raggruppamento delle funzioni nei gruppi esistenti e nuovi (§3, §4)
- ✅ `ANA.2` Funzioni base e dati da spostare in Common (§5, §6)
- ✅ `ANA.3` Decisioni D1–D8 (§9), prese l'11/11/2026
- ✅ `ANA.4` Divisione in aree con etichetta e punti numerati per il lavoro su più worktree (§8, §10)
- ⏳ `ANA.5` Verifica legale del whistleblowing (D.Lgs 24/2023)
- ⏳ `ANA.6` Analisi della sicurezza dei dati riservati (segnalazioni, dati sanitari delle visite), collegata a `piano-security.md`
- ✅ `ANA.7` `ANALISI_COMPLIANCE_APPLICATIVO.md` (branch Compliance_Dev) — scritta l'11/11/2026 con modello dati, uso dei servizi comuni, decisioni aperte e checklist
- ✅ `ANA.8` `ANALISI_HUMANRESOURCES_APPLICATIVO.md` (branch HumanResources_Dev) — scritta l'11/11/2026 con modello dati, uso dei servizi comuni, decisioni aperte e checklist
- ✅ `ANA.9` `ANALISI_ASSETS_APPLICATIVO.md` (branch Assets_Dev) — scritta l'11/11/2026 con modello dati, uso dei servizi comuni, decisioni aperte e checklist
- 🔍 `ANA.10` Livelli di licenza dei nuovi moduli, con i listini (D8)

#### `INF-APP` – Registrazione dei nuovi applicativi (§7)
- ✅ `INF-APP.1` `ApplicationType`: `HumanResources = 5`, `Compliance = 6`, `Assets = 7` (`Administration = 8` riservato, solo in commento), anche nella copia del client (`user-load-model.ts`). Un valore senza DbContext registrato non si può attivare e la creazione degli schemi lo salta: le app diventano attivabili quando il loro ramo registra il contesto in `Installers4<App>`
- ✅ `INF-APP.2` Attivazione per azienda e studio: nessuna modifica necessaria (i servizi filtrano le app registrate). Testi: `LicenseAmbito_5..7`, `VoiceManualApp_5..7` (resx it/en) e nel client `menu.application`, `ticket.application`, `widgets.application`, `pages.license.function5..7`
- ✅ `INF-APP.3` Licenze: `AmbitoLicenza` 5..7 (server e client), le tre app in `LicenseGuard.Funzioni` (coperte anche dal Cumulativo) e in `AmbitiOfferta`, **nessun limite** (`LimiteFunzione` = null). I listini propri arrivano con i rami (`ILicenseCatalogSeed`), i livelli con `ANA.10`
- ✅ `INF-APP.4` Stub vuoti per i tre rami: server `Installers4HumanResources/Compliance/Assets` (in `Program.cs`); client `main-menu-*`, `route-*`, `widget-*` collegati a `MainMenuService` (con `WorkingMode` 4..6), `RouteService`, `app-routing.module`, `WidgetService`; manuali `humanresources`, `compliance`, `assets` nella pagina dei manuali. Showcase: nessuno stub (lo ridefiniscono i rami e AllFunction)
- ✅ `INF-APP.5` Cancellazione: nessuna modifica, `DropCompanySchemasAsync` trova gli schemi `tenant_<id>_<n>` per nome; ogni ramo aggiungerà il proprio `IApplicationDataDeleter` se tiene dati in `public` o Common
- ✅ `INF-APP.6` (11/11/2026) Creazione dei branch `Compliance_Dev`/`Compliance`, `HumanResources_Dev`/`HumanResources`, `Assets_Dev`/`Assets` da Infrastructure
- ✅ `INF-APP.7` CLAUDE.md: nuovi gruppi nell'elenco dei branch e nella catena di AllFunction (Infrastructure, AttendanceTracking, CRM, DocumentManagement, HumanResources, Compliance, Assets)

#### `INF-COM` – Dati in Common, lato Infrastructure (§6)
- ✅ `INF-COM.1` Entità Common `Az_Sedi`, `Az_SediReparto` e migration del contesto Common — entità partial in `Entities/Tenant/Common` senza le collection verso le Presenze, configurate in `CommonDbContext`; migration `CommonSediCompetenzeAutovetture` (solo le nuove tabelle e `DataAssunzione`); repository `*CommonRepository`
- ✅ `INF-COM.2` Entità Common `Par_Competenza`, `Dip_Competenza` — indice univoco (`IdDip_Anagrafica`, `IdPar_Competenza`); la collection `Dip_Anagrafica.Dip_Competenza` è nella classe comune (AT-COM la toglie da `Dip_Anagrafica.Presenze.cs`)
- ✅ `INF-COM.3` Entità Common `Dip_RapportoAutovetture` (D5) — legata a `Dip_Anagrafica` (FK Cascade) invece che a `Dip_RapportoLavoro`; AT-COM converte i dati
- ✅ `INF-COM.4` Date di assunzione e cessazione in Common (campi di `Dip_Anagrafica` o tabella dedicata) (D6) — aggiunto `Dip_Anagrafica.DataAssunzione` (`DataCessazione` c'era già)
- ✅ `INF-COM.5` Servizi e pagine comuni di sola lettura per i moduli che non sono le Presenze (sedi, reparti, competenze) — `CommonOrganizzazioneService`/`CommonOrganizzazioneController` (`SediGetAll` con albero dei reparti, `CompetenzeGetAll`) e servizio client in `ClientServer-Service/Infrastructure/Common`; nessuna pagina
- ✅ `INF-COM.6` `CommonDataDeleter` aggiornato per le nuove tabelle — tutte in cascata da `Az_Anagrafica` (documentato); prima si scollegano i reparti dal padre (FK Restrict); conteggi invariati

#### `INF-SVC` – Servizi comuni (§5)
- ✅ `INF-SVC.1` Scadenzario: modello dati (fonte, modulo, riferimento, data, responsabile, stato), da decidere se in `public` o in Common — server fatto e provato su DB usa e getta con 24 controlli API (tabella `Scadenza` in `public`, migration `ServiziComuni`); client: servizi e modelli `ClientServer-Service/Infrastructure/Deadline`
- ✅ `INF-SVC.2` Scadenzario: servizio con cui i moduli registrano e chiudono le proprie scadenze — server fatto e provato su DB usa e getta con 24 controlli API (`DeadlineRegistry`); lato client nulla da fare (le scadenze dei moduli compaiono nello scadenzario con il link all'oggetto)
- ✅ `INF-SVC.3` Scadenzario: pagina e widget "In scadenza" — server fatto e provato su DB usa e getta con 24 controlli API; client: pagina `/deadlines` (filtri stato / entro n giorni / testo / solo mie, scadute e in scadenza evidenziate, apri oggetto, completa/riapri, + nel fab per gli amministratori), modifica della scadenza manuale `/deadlineedit` con cancellazione, voce di menu "Scadenzario" (ruoli dell'azienda), widget `infrastructure.deadlines` (non predefinito)
- ✅ `INF-SVC.4` Notifiche: servizio unico (email con Brevo, notifiche nell'app con SignalR) e preferenze per utente — server fatto e provato su DB usa e getta con 24 controlli API; client: pagina `/notifications` (paginata, non lette, apri, segna tutte lette, cancella lette), preferenze `/notificationpreference` (App/Email per categoria), voce di menu "Notifiche" per tutti con badge delle non lette (`NotificationNotifierService`, evento SignalR `NotificationNew`)
- ✅ `INF-SVC.5` Promemoria automatici delle scadenze (job pianificato) — server fatto e provato su DB usa e getta con 24 controlli API; client: il promemoria apre `/deadlines` (o l'oggetto della scadenza)
- ✅ `INF-SVC.6` Interfaccia di presa visione (`IAcknowledgementService`): implementazione base con semplice conferma di lettura, sostituita dal Documentale quando è attivo (`DM-FIRMA`) — server fatto e provato su DB usa e getta con 24 controlli API; client: pagina `/acknowledgements` (da confermare / tutte, "Confermo di aver letto" o apertura dell'oggetto per gli altri canali), voce di menu "Prese visione" (ruoli dell'azienda) con badge di quelle da confermare
- ⏳ `INF-SVC.7` Titolo delle categorie di notifica fornito dal modulo (oggi sta nei testi di Infrastructure: Compliance, Risorse Umane e Risorse aziendali usano per ora `generale`/`scadenze`)
- ⏳ `INF-SVC.8` Alla creazione dell'account di un dipendente gia' presente, collegare l'utente alla `Dip_Anagrafica` esistente invece di crearne una nuova (oggi lo corregge Risorse Umane, `HR-ONB.4`)

#### `INF-CAL` – Sincronizzazione dei calendari
- ✅ `INF-CAL.1` (ridefinito con D9) Feed iCalendar personale per utente e azienda: tabella `CalendarioFeed` in `public` (token casuale di 32 byte base64url, date di creazione e ultimo accesso), migration `CalendarioFeed`
- ✅ `INF-CAL.2` Modello comune degli eventi pubblicati dai moduli: `ICalendarEventProvider` + `CalendarEvent` (registrati dallo scanner, più implementazioni); per Infrastructure `DeadlineCalendarEventProvider` (scadenze aperte visibili all'utente, stesse regole dello scadenzario, come eventi tutto il giorno). I moduli (`CRM-CAL`, `AST-PREN.3`, Presenze) aggiungono il proprio provider
- ✅ `INF-CAL.3` Feed anonimo `GET api/CalendarFeed/{token}.ics` (RFC 5545, eventi da oggi-90 a oggi+365 giorni, 404 per token sconosciuto, calendario vuoto se l'utente non lavora più nell'azienda), gestione del proprio feed (crea, nuovo indirizzo, elimina; indirizzo da `App:ApiPublicUrl` / `App:PublicUrl`), pagina client `/calendarfeed` (menu "Calendario" e bottone nello Scadenzario) — server provato su DB usa e getta con 52 controlli API e parser `icalendar`; client compilato
- ⏳ `INF-CAL.4` (futuro, D9) Sincronizzazione bidirezionale via OAuth con Google e Microsoft (richiede app registrate e credenziali)

#### `INF-AI` – Riepilogo AI periodico
- ✅ `INF-AI.1` Interfaccia con cui ogni modulo fornisce i dati del riepilogo — `IDigestProvider` (`Application`, `SectionAsync(idCompany, idAspNetUsers?, dal, al)`) con `DigestSection` (codice, titolo, critica, righe) e `DigestFact` (testo, numero, critico, dettagli), registrati dallo scanner; idAspNetUsers null = vista dell'azienda (studio). Provider di Infrastructure: `DeadlineDigestProvider` (scadute ancora aperte = critica, nei prossimi 14 giorni, chiuse nel periodo), `AcknowledgementDigestProvider` (in attesa nell'azienda, critica se oltre 7 giorni, e proprie), `NotificationDigestProvider` (non lette dell'utente per categoria). I moduli con tabelle negli schemi delle aziende leggono con `IApplicationDbContextFactory`
- ✅ `INF-AI.2` Generazione settimanale con il provider AI configurato, per azienda e per studio — `DigestMaintenanceService` (lunedì dopo `Digest:Hour` = 7, stato `Riepilogo_UltimaSettimana` in `AppSetting`, `Digest:Weekly` = false lo spegne); `DigestGenerator`: un riepilogo per ogni amministratore dell'azienda (solo applicativi attivi) e uno per studio con le sole aziende che hanno sezioni critiche; `DigestWriter`: testo dall'AI (`ILlmClient`, `AI:DigestLLM`, vuoto = `UseLLM`, `none` = senza AI) solo dai fatti JSON con prompt che vieta di inventare, ripiego sul testo semplice generato dai fatti; categoria di notifica `riepilogo` (app ed email di default)
- ✅ `INF-AI.3` Invio tramite `INF-SVC.4` e consultazione nell'app — tabella `public.Riepilogo` (tipo azienda/studio, IdCompany, IdFinancialAdvisor, IdAspNetUsers, periodo, testo, fatti JSON, AI sì/no, manuale; migration `Riepilogo`), notifica `riepilogo` con link a `/digestdetail`; `DigestService`/`DigestController` (`DigestList`, `DigestGet`, `DigestGenerate`); client: pagine `/digests` (elenco, "Genera ora" per CompanyPowerAdmin nell'azienda e FinancialAdvisorPowerAdmin nello studio) e `/digestdetail` (testo e fatti), voce di menu "Riepiloghi" per amministratori dell'azienda e utenti dello studio, testi it/en

#### `AT-COM` – Dati in Common, lato Presenze
- ✅ `AT-COM.1` Rimozione di `Az_Sedi`, `Az_SediReparto`, `Par_Competenza`, `Dip_Competenza`, `Dip_RapportoAutovetture` dal contesto delle Presenze; collegamenti verso Common. Le classi Presenze hanno lo stesso nome di quelle Common: vanno cancellate nello stesso merge in cui arriva `INF-COM` (altrimenti il ramo non compila); va tolta anche la collection `Dip_Competenza` da `Dip_Anagrafica.Presenze.cs` (ora e' nella classe Common). Nel contesto Presenze le tabelle Common si mappano con `ExcludeFromMigrations()` come `Dip_Anagrafica`; le collection verso le tabelle Presenze (Az_SediAttivita, Az_SediRepartoUser, Dip_GG_NotaSpesa...) vanno nei partial Presenze — fatto: cancellate le 5 entità e i repository Presenze omonimi; partial `Az_Sedi.Presenze.cs`, `Az_SediReparto.Presenze.cs`, `Dip_RapportoAutovetture.Presenze.cs`; tabelle mappate in `AttendanceTrackingDbContext.Gen_PublicReferences_GestionePresenze` con `ExcludeFromMigrations()` e la stessa configurazione di `CommonDbContext`. Repository: i servizi Presenze (`Az_SediService`, `Az_SediRepartoService`, `Par_CompetenzaService`, `Dip_CompetenzaService`, `Dip_RapportoAutovettureService`, utility, report, note spese) leggono e scrivono con i `*CommonRepository`: nessuna query unisce queste tabelle a quelle Presenze (gli Id si materializzano prima), quindi non servono repository `*ReadRepository`; le `Include` dalle tabelle Presenze (es. `Az_SediRepartoUser` → reparto) funzionano con la mappatura esclusa
- ✅ `AT-COM.2` Migrazione dei dati esistenti dallo schema `tenant_<id>_2` al `tenant_<id>_0`. Attenzione: all'avvio la migration Common (`CommonSediCompetenzeAutovetture`) crea gia' le tabelle **vuote** in `tenant_<id>_0`, prima delle Presenze, quindi non si puo' usare `ALTER TABLE ... SET SCHEMA`: si copiano le righe mantenendo gli Id (e si riallineano le sequenze identity), si ripuntano le FK delle tabelle Presenze e poi si eliminano le vecchie tabelle. `Dip_RapportoAutovetture`: `IdDip_Anagrafica` preso dal rapporto di lavoro. `Dip_Competenza`: eliminare i doppioni (ora indice univoco dipendente+competenza). Su un database ricreato da zero non serve nulla — fatto: `AttendanceTracking_Init` non crea più le 5 tabelle (le FK Presenze, senza schema, si risolvono col search_path sulle tabelle comuni: serve anche alla modalità single-tenant, dove la migration Common le crea già in `public`); migration `AttendanceTracking_CommonTables` con un blocco `DO` condizionale (esce se lo schema è `public` o le vecchie tabelle non ci sono; si ferma con errore se le tabelle comuni hanno già righe con gli stessi Id) che copia con `OVERRIDING SYSTEM VALUE`, deduplica `Dip_Competenza` (tiene l'Id minore), converte le autovetture, riallinea le sequenze, ricrea le FK (stesso nome e ON DELETE) sulle tabelle comuni e toglie le vecchie. Provato: DB nuovo multi-tenant e single-tenant, DB multi-tenant creato con il codice originale e poi migrato (dati, doppioni, FK, sequenze, CRUD e note spese dopo la migrazione). **Limite**: database single-tenant **già esistenti** (tabelle Presenze già in `public`) non sono gestiti: la migration Common `CommonSediCompetenzeAutovetture` (ramo Infrastructure) fallisce su `CREATE TABLE "Az_Sedi"` perché la tabella esiste. Procedura manuale prima dell'avvio con il nuovo codice: in `public` aggiungere a `Dip_RapportoAutovetture` la colonna `IdDip_Anagrafica` (valorizzata dal rapporto di lavoro) e togliere `IdDip_RapportoLavoro`, eliminare i doppioni di `Dip_Competenza` e creare l'indice univoco, aggiungere `Dip_Anagrafica.DataAssunzione`, poi inserire in `__EFMigrationsHistory_Common` la riga di `CommonSediCompetenzeAutovetture`; in alternativa rendere tollerante quella migration su Infrastructure (da decidere)
- ✅ `AT-COM.3` Date di assunzione e cessazione lette da Common, il resto di `Dip_RapportoLavoro` resta nelle Presenze (D6) — `GestionePresenzeUserUtility.AllineaDateDipendente`: `DataAssunzione` = `DataAss` minima, `DataCessazione` = `DataLic` del rapporto più recente (null se un rapporto è aperto); chiamata da `Dip_RapportoLavoroPut` (salvataggio e cancellazione dei rapporti, anche dall'import dei dipendenti, che passa da `Dip_AnagraficaPut`) e alla creazione del rapporto iniziale; la migration allinea i dati esistenti con lo stesso calcolo. Prima `DataCessazione` la impostava solo `CommonDataDeleter` (resta); `Dip_AnagraficaPut` conserva le due date salvate
- ✅ `AT-COM.4` Adeguamento di servizi, pagine, import dei dipendenti, note spese e rimborsi km (autovetture) ai dati in Common (l'export paghe legge invece le date del rapporto) — autovetture del dipendente: `Dip_RapportoAutovettureModel.IdDip_Anagrafica`, Get/Put per dipendente (una volta, fuori dal ciclo dei rapporti), `GetAll4User` e rimborsi km dal dipendente del rapporto; client: modello, anagrafica dipendente (la sezione Spesa di ogni rapporto mostra le autovetture del dipendente), testo "nessuna autovettura"; export paghe invariato (date del rapporto)
- ✅ `AT-COM.5` Prova completa: calcolo delle giornate, report ed export paghe invariati — `dotnet build` e `ng build` ok; prova API su DB usa e getta: creazione azienda, CRUD sedi/reparti (anche figlio)/competenze, anagrafica con competenze e autovetture, nota spese con rimborso km (100 km × 0,40 = 40,00), autovettura usata non cancellabile, `TimeSheet_Engine/Calculate`, cancellazioni a cascata; dati verificati con psql in `tenant_<id>_0`. Export paghe non provato (job; il codice non legge le tabelle spostate)

#### `AT-TURNI` – Turni e pianificazione
- ⏳ `AT-TURNI.1` Modello dei turni (modelli, assegnazioni per reparto e sede, coperture minime)
- ⏳ `AT-TURNI.2` Pagina di pianificazione con controllo delle coperture e delle ferie
- ⏳ `AT-TURNI.3` Scambio di turni tra colleghi con approvazione
- ⏳ `AT-TURNI.4` Confronto tra turni previsti e timbrature nel calcolo della giornata

#### `AT-TIMB` – Timbratura evoluta
- ⏳ `AT-TIMB.1` Timbratura con QR o NFC per sede
- ⏳ `AT-TIMB.2` Geofencing: controllo della posizione rispetto alla sede
- ⏳ `AT-TIMB.3` Postazione condivisa in modalità kiosk
- ⏳ `AT-TIMB.4` PWA che timbra offline e sincronizza dopo

#### `AT-SW` – Smart working
- ⏳ `AT-SW.1` Accordi individuali (con firma tramite `INF-SVC.6` se disponibile)
- ⏳ `AT-SW.2` Calendario del lavoro agile e limite di giorni
- ⏳ `AT-SW.3` Causale o giustificativo dedicato nel calcolo della giornata

#### `AT-AI` – Analisi AI delle timbrature
- ⏳ `AT-AI.1` Segnalazione delle timbrature anomale
- ⏳ `AT-AI.2` Giornate da sistemare prima dell'export paghe
- ⏳ `AT-AI.3` Previsione delle assenze
- ⏳ `AT-AI.4` Widget e comandi vocali

#### `AT-CDG` – Controllo di gestione sulle commesse (D2)
- ⏳ `AT-CDG.1` Costo orario del dipendente dal rapporto di lavoro
- ⏳ `AT-CDG.2` Costi della commessa: ore del timesheet per costo orario, più le note spese
- ⏳ `AT-CDG.3` Report di marginalità e widget

#### `DM-PEC` – Acquisizione delle PEC
- ⏳ `DM-PEC.1` Configurazione della casella PEC per azienda (credenziali protette)
- ⏳ `DM-PEC.2` Lettura periodica e archiviazione come documenti, con ricevute
- ⏳ `DM-PEC.3` Classificazione e assegnazione con AI

#### `DM-CONS` – Conservazione a norma
- ⏳ `DM-CONS.1` Scelta del conservatore accreditato e della sua API
- ⏳ `DM-CONS.2` Invio dei pacchetti, stato e ricevute
- ⏳ `DM-CONS.3` Regole per tipologia di documento

#### `DM-FIRMA` – Presa visione e verbali per gli altri moduli
- ✅ `DM-FIRMA.1` Implementazione di `IAcknowledgementService` (`INF-SVC.6`) con la FES già realizzata — canale `doc.fes` (`DocumentManagementAcknowledgementChannel`, priorità 10): disponibile con documentale attivo e inizializzato e tipologia `PRESAVIS` "Verbali e prese visione" non disattivata (disattivarla = tornare al canale base; nelle aziende già inizializzate la crea il canale alla prima richiesta, nelle nuove è tra le predefinite); crea un documento del personale pubblicato (file del modulo se c'è, altrimenti verbale PDF) con il destinatario (e il suo dipendente, se collegato), presa visione richiesta e firma FES proposta dalla tipologia (accesa di default); riferimento nel canale `"<IdCompany>:<Doc_Destinatario.Id>"` (gli id del documentale sono per schema), url della richiesta `/docmine` ("I miei documenti"); la firma — o la presa visione se la firma non è richiesta — chiama `ConfirmFromChannelAsync` con data, IP e browser (`IAcknowledgementService` risolto in modo lazy in `Doc_DocumentoService`); se il documento non si può creare (es. senza licenza/spazio) la richiesta passa al canale base. Provato su DB usa e getta: creazione, firma prima del download rifiutata, conferma con IP e browser, presa visione senza firma, tipologia disattivata/eliminata, documentale disattivato, senza licenza
- ✅ `DM-FIRMA.2` Modelli di verbale (consegna di beni, consegna di DPI, accordi) — basta un testo predefinito: `DocumentManagementVerbale` genera il PDF (azienda, titolo, destinatario, data, testo del modulo riga per riga, dichiarazione e modalità di conferma); la dichiarazione dipende dal modello scelto dalla fonte della richiesta (`dpi` → consegna DPI con richiamo al D.Lgs. 81/2008 artt. 20 e 78, `beni`/`consegna`/`asset` → consegna di beni, `accordo`/`agreement` → accordo, altrimenti presa visione generica; testi in `DocumentManagementMessages`, italiano e inglese). L'elenco dei beni o il testo dell'accordo li scrive il modulo nel testo; un modulo che vuole un modello proprio passa il file (`Contenuto`/`NomeFile`)
- ✅ `DM-FIRMA.3` Archiviazione del documento firmato e delle prove — verificato: il documento resta nel documentale (tipologia `PRESAVIS`, categoria Personale, conservazione 10 anni, versioni non potate perché con presa visione/firma); le prove sono su `Doc_Destinatario` (`DataFirma`, `FirmaVersione`, `FirmaHash` SHA-256 della versione, `FirmaIp`, `FirmaUserAgent`, `IdAspNetUsersFirma`, `DataPresaVisione`) e nel registro `Doc_Log` (caricamento con la fonte della richiesta, download, presa visione, `Firma` con `FES sha256:<hash>`); in `public.PresaVisione` restano data di conferma, IP e browser. Nessun attestato PDF separato (resta nel piano della firma, `ANALISI_DOCUMENTMANAGEMENT_FIRMA_DIGITALE.md` §13 F1)

#### `CRM-PREV` – Preventivi e offerte
- ⏳ `CRM-PREV.1` Modello del preventivo (righe, listini, sconti, stati)
- ⏳ `CRM-PREV.2` PDF e invio al cliente
- ⏳ `CRM-PREV.3` Collegamento con le opportunità della pipeline

#### `CRM-ASS` – Assistenza ai clienti dell'azienda
- ⏳ `CRM-ASS.1` Richieste dei clienti (stati, assegnazione, tempi)
- ⏳ `CRM-ASS.2` Storico nella scheda del cliente

#### `CRM-CAL` – Sincronizzazione degli appuntamenti
- ✅ `CRM-CAL.1` Pubblicazione delle attività del CRM sul calendario (`INF-CAL`): `CrmCalendarEventProvider` (`Crm/_utility`), letto dal feed senza utente collegato con lo schema aperto da `CrmSchema.ContextAsync` e i repository di `CrmRepositorySet`. Solo con il CRM attivo e un ruolo dell'azienda; solo le attività **assegnate all'utente** (sempre visibili a lui nel CRM), da fare o fatte (titolo "(fatta)"), non le annullate. Titolo "Tipo: oggetto", descrizione con cliente, contatto, opportunità, lead e descrizione dell'attività; senza orario (inizio a mezzanotte, fine assente o a mezzanotte) = tutto il giorno; UID `crm-attivita-<IdAzienda>-<Id>@nvxapp`; link `/crmactivityedit?id=<Id>` (assoluto con `App:PublicUrl`; la pagina dell'attività ora legge anche l'`id` dall'indirizzo). Provato su DB usa e getta (20 controlli)

#### `CMP-BASE` – Avvio del modulo Compliance
- ✅ `CMP-BASE.1` `ComplianceDbContext` registrato in `DataLayerInstaller` e migration iniziale — `ComplianceDbContext` registrato in `Installers4<App>` (design-time factory in un file proprio), migration `InitCompliance` con la tabella di configurazione, repository, inizializzatore e `IApplicationDataDeleter`
- ✅ `CMP-BASE.2` Cartelle del modulo (controller, servizi, pagine) e ruoli — servizio/controller della configurazione (Get amministratori, Put CompanyPowerAdmin), pagina `/cmpsettings`, manuale it/en, listini con prezzi segnaposto (livelli: `ANA.10`); dettagli in `ANALISI_COMPLIANCE_APPLICATIVO.md`
- ✅ `CMP-BASE.3` Menu, widget e comandi vocali di base — voce di menu, rotta, cartella i18n; widget e comandi vocali: nessuno per ora (arrivano con le aree)

#### `CMP-81` – Sicurezza sul lavoro (D.Lgs 81/08)
- ✅ `CMP-81.1` Corsi obbligatori per mansione e scadenze — mansioni, corsi per mansione, assegnazioni, corsi svolti con rinnovo e situazione della formazione
- ✅ `CMP-81.2` Visite mediche e idoneità (dati sanitari protetti, `ANA.6`) — esito e scadenza agli amministratori, dati sanitari cifrati solo agli abilitati (scelta provvisoria in attesa di `ANA.6`)
- ✅ `CMP-81.3` Consegna dei DPI con verbale (`INF-SVC.6`) — presa visione `cmp.dpi` (con il documentale: verbale DPI)
- ✅ `CMP-81.4` Nomine (RSPP, RLS, preposti, addetti alle emergenze) e documenti DVR — presa visione del nominato; allegati solo come riferimento
- ✅ `CMP-81.5` Scadenze registrate nello scadenzario (`INF-SVC.2`) — `cmp.formazione`, `cmp.visita`, `cmp.nomina`, `cmp.documento` a ogni salvataggio/cancellazione

#### `CMP-WB` – Whistleblowing (D7: nel modulo, senza una licenza separata)
- ⏳ `CMP-WB.1` Canale di segnalazione con anonimato e codice per seguire la segnalazione
- ⏳ `CMP-WB.2` Gestore, accesso limitato, cifratura, esclusione dalla chat AI
- ⏳ `CMP-WB.3` Scadenze di legge (7 giorni, 3 mesi) e registro

#### `CMP-GDPR` – Registro GDPR
- ✅ `CMP-GDPR.1` Registro dei trattamenti — con export Excel tramite job
- ✅ `CMP-GDPR.2` Nomine e responsabili esterni — scadenza del contratto `cmp.responsabile`
- ✅ `CMP-GDPR.3` Registro delle violazioni — 72 ore nello scadenzario (`cmp.violazione`), notifica agli amministratori
- ✅ `CMP-GDPR.4` Gestione da parte dello studio per conto delle aziende — pagina `/cmpstudio` (registro GDPR e scadenze del modulo)

#### `HR-BASE` – Avvio del modulo Risorse Umane
- ✅ `HR-BASE.1` `HumanResourcesDbContext` registrato in `DataLayerInstaller` e migration iniziale — `HumanResourcesDbContext` registrato in `Installers4<App>` (design-time factory in un file proprio), migration `InitHumanResources` con la tabella di configurazione, repository, inizializzatore e `IApplicationDataDeleter`
- ✅ `HR-BASE.2` Cartelle del modulo (controller, servizi, pagine) e ruoli, compresa l'area del dipendente — servizio/controller della configurazione (Get amministratori, Put CompanyPowerAdmin), pagina `/hrsettings`, manuale it/en, listini con prezzi segnaposto (livelli: `ANA.10`); area del dipendente prevista nei ruoli, pagine con `HR-CED`; dettagli in `ANALISI_HUMANRESOURCES_APPLICATIVO.md`
- ✅ `HR-BASE.3` Menu, widget e comandi vocali di base — voce di menu, rotta, cartella i18n; widget e comandi vocali: nessuno per ora (arrivano con le aree)

#### `HR-CED` – Portale con cedolini e CU
- ✅ `HR-CED.1` Caricamento del PDF unico da parte dello studio o dell'azienda — pagine lato studio (`/hrstudiopayslips`, scelta dell'azienda dello studio) e dell'azienda; origine registrata; divisione in un job
- ✅ `HR-CED.2` Divisione per dipendente (codice fiscale o matricola, con l'AI come aiuto) e verifica degli scarti — PdfPig/PDFsharp; codice fiscale, poi matricola; scarti con proposta per cognome e nome (niente AI, scelta provvisoria), assegnati o ignorati a mano
- ✅ `HR-CED.3` Pubblicazione nell'area del dipendente e notifica (`INF-SVC.4`) — pubblicazione in un job, "I miei cedolini", notifica "nuovo documento"
- ✅ `HR-CED.4` Presa visione (`INF-SVC.6`) e report delle conferme — fonte `hr.cedolino` con il PDF per il canale del documentale; report nel dettaglio del caricamento

#### `HR-FOR` – Formazione e competenze
- ✅ `HR-FOR.1` Corsi, attestati e piani di formazione sulle competenze in Common — corso superato: `Dip_Competenza` in Common; catalogo dei corsi in HR (HR-D5, scelta provvisoria)
- ✅ `HR-FOR.2` Scadenze dei corsi nello scadenzario (`INF-SVC.2`) — fonte `hr.corso`, chiusura della precedente al rinnovo

#### `HR-ONB` – Onboarding e offboarding
- ✅ `HR-ONB.1` Checklist di assunzione e cessazione, basate sulle date in Common (D6) — modelli predefiniti, generazione e spostamento sulle date di `Dip_Anagrafica`, scadenze `hr.checklist`
- ✅ `HR-ONB.2` Documenti da consegnare o firmare (`INF-SVC.6`) — fonte `hr.checklist.voce` con il file; la conferma completa la voce
- ✅ `HR-ONB.3` Creazione e disattivazione dell'account — con `IAccountService.UserCompanyGet/UserCompanyPut` (vedi i problemi aperti in `ANALISI_HUMANRESOURCES_APPLICATIVO.md` §7.1)

#### `AST-BASE` – Avvio del modulo Risorse aziendali
- ✅ `AST-BASE.1` `AssetsDbContext` registrato in `DataLayerInstaller` e migration iniziale — `AssetsDbContext` registrato in `Installers4<App>` (design-time factory in un file proprio), migration `InitAssets` con la tabella di configurazione, repository, inizializzatore e `IApplicationDataDeleter`
- ✅ `AST-BASE.2` Cartelle del modulo (controller, servizi, pagine) e ruoli — servizio/controller della configurazione (Get amministratori, Put CompanyPowerAdmin), pagina `/astsettings`, manuale it/en, listini con prezzi segnaposto (livelli: `ANA.10`); dettagli in `ANALISI_ASSETS_APPLICATIVO.md`
- ✅ `AST-BASE.3` Menu, widget e comandi vocali di base — voce di menu, rotta, cartella i18n; widget e comandi vocali: nessuno per ora (arrivano con le aree)

#### `AST-FLO` – Flotta
- ⏳ `AST-FLO.1` Veicoli, assegnazione e fringe benefit (su `Dip_RapportoAutovetture` in Common, D5)
- ⏳ `AST-FLO.2` Scadenze (bollo, assicurazione, revisione, tagliando) nello scadenzario
- ⏳ `AST-FLO.3` Multe e costi

#### `AST-BENI` – Beni assegnati
- ⏳ `AST-BENI.1` Anagrafica dei beni (PC, telefoni, badge, attrezzature)
- ⏳ `AST-BENI.2` Consegna e restituzione con verbale (`INF-SVC.6`)
- ⏳ `AST-BENI.3` Elenco dei beni da restituire alla cessazione (data in Common)

#### `AST-PREN` – Prenotazioni
- ⏳ `AST-PREN.1` Risorse prenotabili (postazioni, sale, auto in pool)
- ⏳ `AST-PREN.2` Calendario delle prenotazioni e conflitti
- ⏳ `AST-PREN.3` Sincronizzazione con il calendario personale (`INF-CAL`, facoltativa)

#### `ADM` – Amministrazione (più avanti, D1, D2)
- 🔍 `ADM.1` Scelta dell'intermediario SdI
- ⏳ `ADM.2` Registrazione dell'applicativo `Administration = 8` (come `INF-APP`)
- ⏳ `ADM.3` Ordini, a partire dai preventivi del CRM (integrazione facoltativa)
- ⏳ `ADM.4` Fatture elettroniche, invio allo SdI e incassi

---

## 11. Lavori fatti e da fare (riepilogo)

### Fatti
- ✅ 11/11/2026 – Ricognizione dell'applicativo: branch, controller per modulo, analisi esistenti, enum `ApplicationType`, contenuto dello schema Common
- ✅ 11/11/2026 – Proposta delle funzioni utili alle aziende e loro raggruppamento logico (`ANA.1`, `ANA.2`)
- ✅ 11/11/2026 – Stesura di questo documento e prima catena dei merge (solo locale, nessun push); worktree `wt_infra_moduli` rimosso
- ✅ 11/11/2026 – Decisioni D1–D8 registrate (`ANA.3`)
- ✅ 11/11/2026 – Divisione in aree con etichetta e punti numerati per il lavoro su più worktree (`ANA.4`), seconda catena dei merge

- ✅ 11/11/2026 – Fase 1 (`INF-APP`, `INF-SVC`) mergiata; fase 2 (`INF-COM`+`AT-COM`, `INF-CAL`, `DM-FIRMA`, `CMP-BASE`, `HR-BASE`, `AST-BASE`, analisi `ANA.7`–`ANA.9`) mergiata dopo la prova completa della catena (8 rami compilati, AllFunction avviato con tutte le app: 15 controlli)

- ✅ 11/11/2026 – Fase 3 mergiata: `CMP-81`, `CMP-GDPR`, `HR-CED`, `HR-FOR`, `HR-ONB`, `AST-FLO`, `AST-BENI`, `AST-PREN`, `INF-AI`, `CRM-CAL` (prova completa: 8 rami compilati, AllFunction avviato con tutte le app). `CMP-WB` fermo in attesa di `ANA.5`/`ANA.6`

### Da fare (per fase, §8)
- ⏳ **Fase 1**: `ANA.5`–`ANA.10`, `INF-APP`, `INF-COM`, `INF-SVC`; in parallelo, quando si vuole, `AT-TURNI`, `AT-TIMB`, `AT-SW`, `AT-AI`, `AT-CDG`, `DM-PEC`, `DM-CONS`, `CRM-PREV`, `CRM-ASS`
- ⏳ **Fase 2**: `AT-COM`, `INF-CAL`, `DM-FIRMA`, `CMP-BASE`, `HR-BASE`, `AST-BASE`
- ⏳ **Fase 3**: `CMP-81`, `CMP-WB`, `CMP-GDPR`, `HR-CED`, `HR-FOR`, `HR-ONB`, `AST-FLO`, `AST-BENI`, `AST-PREN`, `CRM-CAL`, `INF-AI`
- ⏳ **Più avanti**: `ADM`
