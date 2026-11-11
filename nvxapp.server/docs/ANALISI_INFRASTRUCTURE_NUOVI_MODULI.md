# Analisi – Nuovi moduli e raggruppamento logico delle funzioni

Data: 11/11/2026. Ramo: **Infrastructure_Dev** (lavoro su `Infrastructure_Dev_moduli`, worktree `wt_infra_moduli`).
Stato: **solo analisi, nessuna modifica al codice**. Le scelte segnate "proposta" sono indicazioni di partenza, non decisioni (§9).

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

## 4. Gruppi nuovi (proposta)

| Gruppo | `ApplicationType` | Branch | Schema | Prefisso tabelle | Responsabile in azienda |
|---|---|---|---|---|---|
| Risorse Umane | `HumanResources = 5` | `HumanResources_Dev` → `HumanResources` | `tenant_<id>_5` | `Hr_` | Ufficio del personale, studio paghe |
| Compliance | `Compliance = 6` | `Compliance_Dev` → `Compliance` | `tenant_<id>_6` | `Cmp_` | RSPP, DPO, responsabile delle segnalazioni |
| Risorse aziendali | `Assets = 7` | `Assets_Dev` → `Assets` | `tenant_<id>_7` | `Ast_` | Ufficio acquisti, logistica, IT |
| Amministrazione | `Administration = 8` | `Administration_Dev` → `Administration` | `tenant_<id>_8` | `Adm_` | Amministrazione |

Le assegnazioni dei valori dell'enum sono una proposta. Una volta in produzione **non si cambiano più**.

### 4.1 Risorse Umane (HumanResources)
Il fascicolo del dipendente al di là delle presenze.
- **Portale del dipendente con cedolini e CU**: lo studio (o l'azienda) carica il PDF unico. Il sistema lo divide per dipendente (codice fiscale o matricola, con l'AI come aiuto), lo pubblica nell'area del dipendente e chiede la presa visione (servizio del Documentale se attivo, altrimenti una semplice conferma di lettura).
- **Onboarding e offboarding**: checklist per assunzione e cessazione, documenti da consegnare o firmare, creazione e disattivazione dell'account.
- **Formazione e competenze**: corsi, attestati, piani di formazione. Parte da `Dip_Competenza` e `Par_Competenza`, che si spostano in Common (§6).
- **Integrazioni facoltative**: Presenze (cessazione e chiusura del rapporto), Documentale (archiviazione e firma), Compliance (corsi obbligatori).

### 4.2 Compliance
Adempimenti normativi. È il modulo che lo studio può proporre a tutte le sue aziende.
- **Sicurezza sul lavoro (D.Lgs 81/08)**: scadenzario di corsi obbligatori, visite mediche e idoneità, consegna dei DPI (con verbale), nomine (RSPP, RLS, preposti, addetti alle emergenze), documenti DVR.
- **Whistleblowing (D.Lgs 24/2023)**: canale di segnalazione con anonimato, codice per seguire la propria segnalazione, scadenze di legge (avviso di ricezione entro 7 giorni, risposta entro 3 mesi) e accesso limitato al gestore. Serve una **verifica legale** prima del rilascio.
- **Registro GDPR**: registro dei trattamenti, nomine e responsabili esterni, registro delle violazioni. Lo studio può gestirlo per conto delle aziende.
- **Note**: i dati delle segnalazioni vanno protetti in modo particolare, con cifratura, accesso solo al gestore ed esclusione dalla chat AI. Serve un'analisi della sicurezza dedicata, che si aggancia a `piano-security.md`.

### 4.3 Risorse aziendali (Assets)
Cosa possiede l'azienda e a chi lo assegna.
- **Flotta**: veicoli, scadenze (bollo, assicurazione, revisione, tagliando), assegnazione, fringe benefit e multe. Oggi esiste `Dip_RapportoAutovetture` nelle Presenze, che resta lì per l'export paghe (§9, D5).
- **Beni assegnati**: PC, telefoni, badge e attrezzature, con consegna e restituzione (verbale firmato tramite il Documentale) e collegamento all'offboarding di HR.
- **Prenotazioni**: postazioni, sale e auto in pool, con calendario e sincronizzazione (§5).

### 4.4 Amministrazione (Administration)
- **Ordini e fatture elettroniche**: dal preventivo del CRM si passa a ordine, fattura, invio allo SdI tramite un intermediario e incassi.
- **Controllo di gestione avanzato**: ricavi (dalle fatture) messi a confronto con i costi delle commesse (dalle Presenze). Richiede le commesse in Common (§6).
- **Note**: è il gruppo più costoso (fiscalità, intermediario SdI, conservazione). Si propone di **rimandarlo** dopo HR e Compliance (§9, D2).

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

| Dato | Oggi | Serve a | Proposta |
|---|---|---|---|
| Sedi e reparti (`Az_Sedi`, `Az_SediReparto`) | Presenze | HR, Compliance (sicurezza per sede), Assets (beni per sede) | Spostarli in Common. Le tabelle di collegamento proprie delle Presenze (`Az_SediRepartoUser`, `Az_SediAttivita`…) restano nelle Presenze |
| Commesse (`Az_Commessa`, `Az_SubCommessa`) | Presenze | Amministrazione, CRM (opportunità vinta che diventa commessa) | Spostarle quando si avvia Amministrazione (D2) |
| Competenze (`Par_Competenza`, `Dip_Competenza`) | Presenze | HR, Compliance | Spostarle quando si avvia HR |
| Rapporto di lavoro (date e tipo) | Presenze (`Dip_RapportoLavoro`) | HR (onboarding e cessazione) | Da valutare: in Common basterebbero le date di assunzione e cessazione (D6) |

## 7. Cosa serve per aggiungere un gruppo nuovo

Questa è la procedura base, da ripetere per ogni gruppo:
1. **Infrastructure_Dev**: nuovo valore in `ApplicationType`, voce nel listino e nelle licenze, testo del menu e della pagina di attivazione dell'applicativo, cancellazione (`DeletionService`, eliminazione dello schema), manuali.
2. **Nuovo branch** `X_Dev`, creato da Infrastructure dopo il punto 1, e il branch `X`.
3. In `X_Dev`: `XDbContext` registrato con `AddApplicationDbContext<XDbContext>(ApplicationType.X)` (vedi `DataLayerInstaller.cs`), migration iniziale, controller in `Controllers/X`, servizi in `ClientServer-Service/X`, pagine Angular, widget (`widget-x.service.ts`), comandi vocali, voci di `ILicenseGuard`.
4. **CLAUDE.md**: aggiungere il gruppo all'elenco dei branch e alla catena di AllFunction (Infrastructure per primo, poi AttendanceTracking, CRM, DocumentManagement e i gruppi nuovi).
5. **AllFunction_Dev**: merge e verifica di build e migration con tutti gli applicativi attivi.

## 8. Ordine di lavoro proposto

1. **Infrastructure**: scadenzario e notifiche (§5), sedi e reparti in Common (§6), procedura del §7.
2. **Compliance**: prima la sicurezza 81/08 (usa lo scadenzario), poi il whistleblowing, poi il registro GDPR.
3. **Risorse Umane**: portale con cedolini e CU (usa la FES del Documentale), formazione, onboarding.
4. **Gruppi esistenti**, in parallelo quando si vuole: turni e timbratura evoluta (Presenze), PEC (Documentale), preventivi (CRM).
5. **Risorse aziendali**.
6. **Amministrazione**, dopo aver scelto l'intermediario SdI.

## 9. Decisioni da prendere

| # | Decisione | Proposta |
|---|---|---|
| D1 | Quali gruppi nuovi creare | HR e Compliance subito. Assets e Amministrazione più avanti |
| D2 | Tenere Amministrazione separata o lasciare il controllo di gestione nelle Presenze | Lasciare la parte base nelle Presenze (§3.1) e creare Amministrazione solo con la fatturazione |
| D3 | Unire Assets dentro HR (meno branch) | No: chi compra è diverso (logistica e IT contro personale) |
| D4 | Valori dell'enum e nomi dei branch | Quelli del §4 |
| D5 | Dove tenere `Dip_RapportoAutovetture` | Resta nelle Presenze (serve all'export paghe). Assets lo legge se attivo |
| D6 | Quali dati del rapporto di lavoro spostare in Common | Solo le date di assunzione e cessazione |
| D7 | Whistleblowing nel modulo Compliance o venduto a parte | Nel modulo Compliance, ma con una licenza attivabile da sola |
| D8 | Livelli Free/Standard/Pro per i nuovi moduli | Da definire con i listini (`ANALISI_INFRASTRUCTURE_LICENZE.md`) |

---

## 10. Checklist

Legenda: ✅ svolto · ⏳ da fare · 🔍 da decidere

### Analisi
- ✅ Raggruppamento delle funzioni nei gruppi esistenti e nuovi (§3, §4)
- ✅ Funzioni base e dati da spostare in Common (§5, §6)
- 🔍 Decisioni D1–D8 (§9)
- ⏳ Analisi di dettaglio per ogni gruppo nuovo (`ANALISI_HUMANRESOURCES_APPLICATIVO.md`, `ANALISI_COMPLIANCE_APPLICATIVO.md`, …)

### Infrastructure_Dev
- ⏳ Scadenzario comune (entità in `public` o Common, servizio, pagina, widget "In scadenza")
- ⏳ Servizio di notifica (email, nell'app) con preferenze per utente
- ⏳ Sedi e reparti spostati in Common, con migrazione dei dati delle Presenze
- ⏳ Sincronizzazione dei calendari (Google e Microsoft)
- ⏳ Riepilogo AI periodico
- ⏳ Valori `HumanResources`, `Compliance` (poi `Assets`, `Administration`) in `ApplicationType`, con licenze, menu, cancellazione e manuali
- ⏳ Aggiornamento di CLAUDE.md con i nuovi branch e la catena dei merge

### Compliance_Dev
- ⏳ Creazione del branch, `ComplianceDbContext` e migration iniziale
- ⏳ Sicurezza 81/08: corsi, visite, DPI, nomine
- ⏳ Whistleblowing (dopo la verifica legale e l'analisi della sicurezza)
- ⏳ Registro GDPR
- ⏳ Widget e comandi vocali

### HumanResources_Dev
- ⏳ Creazione del branch, `HumanResourcesDbContext` e migration iniziale
- ⏳ Competenze spostate in Common (Infrastructure, prima del modulo)
- ⏳ Portale del dipendente con cedolini e CU (divisione del PDF, pubblicazione, presa visione)
- ⏳ Formazione
- ⏳ Onboarding e offboarding
- ⏳ Widget e comandi vocali

### Gruppi esistenti
- ⏳ Presenze: turni e pianificazione
- ⏳ Presenze: timbratura evoluta (QR/NFC, geofencing, kiosk, offline)
- ⏳ Presenze: smart working
- ⏳ Presenze: analisi AI delle timbrature
- ⏳ Presenze: controllo di gestione di base sulle commesse
- ⏳ Documentale: acquisizione delle PEC
- ⏳ Documentale: conservazione a norma
- ⏳ Documentale: servizio di presa visione e verbali per gli altri moduli
- ⏳ CRM: preventivi e offerte
- ⏳ CRM: sincronizzazione degli appuntamenti
- ⏳ CRM: assistenza ai clienti dell'azienda

### Più avanti
- ⏳ Assets: flotta, beni assegnati, prenotazioni
- ⏳ Amministrazione: commesse in Common, ordini, fatture elettroniche e SdI, controllo di gestione avanzato
