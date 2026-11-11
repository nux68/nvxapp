# Analisi – Applicativo CRM

Branch: **CRM_Dev** (riceve i merge da Infrastructure, si mergia in CRM).
La parte sulle **tabelle condivise** (§2) è lavoro di **Infrastructure_Dev** e va fatta *prima* di iniziare il CRM: vale anche per il Documentale (`ANALISI_DOCUMENTALE.md`, che rimanda qui).

Stato: analisi completa, **tutte le decisioni prese** (§9, 03-04/11/2026). **Fase 0 e Fase 1 implementate il 04/11/2026** (CRM_Dev); restano i punti aperti della checklist §10 (import, AI, GDPR completo, Fase 2). Lavori fatti e da fare: §10.

**Predisposizione fatta il 03/11/2026** (dettaglio nella checklist §10) (decisioni dell'utente): schema Common con valore **`Common = 0`**, sempre creato per ogni azienda;
in Common sono state travasate `Az_Anagrafica` e `Az_Cliente` ed è stata aggiunta `Az_ClienteContatto`; **`Dip_Anagrafica` restava nelle presenze**; il 04/11/2026 è stato deciso di spostare anche lei in Common (vedi `ANALISI_DOCUMENTALE.md` §2.1, da implementare). Migration azzerate: una iniziale per contesto (Public, Common, Moke,
AttendanceTracking) e database ricreato. Le pagine client dei clienti restano per ora nelle presenze (stesse API `api/Az_Cliente/*`, ora in
Infrastructure); i contatti hanno solo le API (`api/Az_ClienteContatto/*`). I valori di CRM e documentale nell'enum sono decisi (`Crm = 3`, `DocumentManagement = 4`) ma non ancora aggiunti.

---

## 1. Situazione di partenza (prima della predisposizione del 03/11/2026)

- **Applicativi e schemi.** `ApplicationType` (Moke = 1, AttendanceTracking = 2). In multi-tenant ogni azienda ha uno schema per applicativo **attivo**: `tenant_<IdAzienda>_<IdApp>`; in modalità singola tutto sta in `public`. L'attivazione è in `public.CompanyApplication`; `TenantSchemaAccessor` blocca l'accesso a un applicativo non attivo.
- **Regola dei contesti** (`ApplicationDbContextBase`): un contesto può leggere le tabelle di `public` (mappate con schema esplicito ed `ExcludeFromMigrations()`), ma **non** può riferirsi alle tabelle di un altro applicativo.
- **search_path**: `TenantSearchPathInterceptor` imposta `SET search_path TO "<schema app>", public`.
- **Clienti oggi.** `Az_Cliente` sta nelle presenze (schema `tenant_<id>_2`), figlia di `Az_Anagrafica` (l'ancora dell'azienda nei dati tenant, 1-1 con `Company`). La usano:
  - `Az_Commessa.IdAz_Cliente` (FK obbligatoria) → sottocommesse → `Dip_RapportoLavoro.IdAz_SubCommessaAttivita`;
  - `ActivityStatisticsService` (ore per cliente), export causali, pagine `customer-list/edit`, pipe `az-cliente-to-long-text`, chat AI (`OpenableEntities`).
- Campi di `Az_Cliente`: ragione sociale, P.IVA, CF, indirizzo, contatti (telefono, email, PEC, referente), codice destinatario SDI, note, `Default`.

**Il problema:** un'azienda che attiva **solo il CRM** non ha lo schema presenze, quindi non ha `Az_Cliente`. E se ha entrambi, i clienti non possono essere due copie diverse.

---

## 2. Tabelle condivise tra applicativi

### 2.1 Opzioni valutate

| Opzione | Descrizione | Giudizio |
|---|---|---|
| A. In `public` con `IdCompany` | Clienti nello schema comune | **No**: dati aziendali fuori dallo schema dell'azienda, si perde l'isolamento del multi-tenant, `public` cresce con i dati di tutti |
| B. Copia in ogni applicativo + sincronizzazione | Ogni app ha la sua `Az_Cliente` | **No**: doppia verità, conflitti, ID diversi |
| C. Il CRM "possiede" i clienti, le presenze li leggono | | **No**: un cliente con solo presenze non avrebbe i clienti |
| **D. "Dati comuni dell'azienda": nuovo schema per azienda, sempre attivo** | Le anagrafiche condivise stanno in uno schema `tenant_<id>_<Common>` creato per **ogni** azienda, indipendente dagli applicativi attivati | **Consigliata** |

### 2.2 Il contesto dei dati comuni (Common)

- Nuovo valore `ApplicationType.Common` e nuovo contesto `CommonDbContext : ApplicationDbContextBase`, in **Infrastructure** (`nvxapp.server.data/Infrastructure/CommonDbContext.cs`, entità in `Entities/Tenant/Common/`, migration in `Migrations/Common`).
- **Sempre attivo**: non compare in `CompanyApplication` né nella pagina di attivazione degli applicativi.
  - `TenantSchemaAccessor.GetSchema(Common)`: richiede l'azienda selezionata ma **salta** il controllo di attivazione.
  - `TenantProvisioningService`: lo schema Common si crea alla creazione dell'azienda e in `MigrateAllAsync` per **tutte** le aziende (non solo quelle con applicativi attivi), e va migrato **prima** degli schemi degli applicativi (questi hanno FK verso di lui).
  - `DropCompanySchemasAsync` lo elimina già (prende tutti i `tenant_<id>_<numero>`); in modalità singola serve un `IApplicationDataDeleter` del Common da chiamare **per ultimo** (gli altri hanno FK verso le sue tabelle) → aggiungere un ordine ai deleter.
- **search_path** dei contesti degli applicativi: `"<schema app>", "<schema common>", public`. Così il modello resta senza nome di schema anche per le tabelle comuni (in modalità singola sono tutte in `public` e non cambia nulla).
- **Riferimenti dagli applicativi alle tabelle comuni**: stessa regola già usata per `Company`/`AspNetUsers`:
  - entità mappata nel contesto dell'applicativo con `ExcludeFromMigrations()` e **senza** schema esplicito (la risolve il search_path);
  - FK reali dal database (PostgreSQL ammette FK tra schemi dello stesso database; la migration dell'app viene eseguita con il search_path che include Common);
  - **sola lettura** dal contesto dell'applicativo: le scritture passano dai servizi Common;
  - le entità comuni **non hanno collection di navigazione** verso le tabelle degli applicativi (Infrastructure non le conosce): la relazione si configura dal lato dell'applicativo con `.WithMany()` senza navigazione. Verificato: nelle presenze non ci sono `Include` sulle collection di `Az_Anagrafica`, quindi rimuoverle non rompe query (restano 27 usi di `Az_AnagraficaNavigation`, che è il lato "uno" e resta).
  - `DatabaseInitializer.CheckTableNames` deve continuare a ignorare le tabelle escluse dalle migration (verificare).
- **Regola nuova**: la tabella comune **non contiene campi di un solo applicativo**. Se il CRM (o il documentale) ha bisogno di dati propri su un cliente usa una **tabella di estensione 1-1 nel proprio schema** (es. `Crm_ClienteDati.IdAz_Cliente` unico). Così un cliente senza CRM non vede colonne vuote o logiche che non lo riguardano.

### 2.3 Quali tabelle diventano comuni

| Tabella | Oggi | Proposta | Chi la usa |
|---|---|---|---|
| `Az_Anagrafica` | presenze | **Common** (ancora dell'azienda; serve in modalità singola come discriminante `IdCompany`) | tutti |
| `Az_Cliente` | presenze | **Common** | presenze (commesse, statistiche), CRM, documentale (fascicolo cliente) |
| `Az_ClienteContatto` | – (oggi un solo campo `Referente`) | **Common, nuova**: persone di riferimento del cliente | CRM, documentale (destinatari esterni) |
| `Dip_Anagrafica` | presenze | **Common** (deciso il 04/11/2026, vedi `ANALISI_DOCUMENTALE.md` §2.1; da fare) | presenze, documentale |
| `Az_Sedi`, `Az_SediReparto` | presenze | **restano nelle presenze** (deciso) | presenze |
| `Az_Commessa` e figli | presenze | **restano nelle presenze** (sottocommesse, attività, reparti, utenti sono logica presenze) | presenze; il CRM le collega in modo "debole" (§5.3) |
| `Az_Cfg`, `Par_*` | presenze | restano | presenze |

Campi di `Az_Cliente` decisi il 04/11/2026:
- `Default` (serve solo al wizard presenze per il cliente della commessa predefinita) **si sposta** in `Az_Cfg` delle presenze come `IdAz_ClienteDefault`, per rispettare la regola "nessun campo di un solo applicativo";
- nuovo campo **`Attivo`** (predefinito sì): un cliente disattivato resta nello storico (commesse, opportunità, documenti) ma non viene più proposto nelle scelte; gli elenchi lo mostrano con l'opzione "Mostra disabilitati" già presente nelle liste;
- nuovo campo **`Tipo`** (Cliente / Fornitore / Entrambi, predefinito Cliente): predispone i fornitori senza doverli aggiungere dopo in una tabella comune già piena; le scelte "cliente" (commesse, opportunità) propongono Cliente ed Entrambi.

### 2.4 Spostamento dei dati esistenti (presenze → Common)

È il punto più delicato: in produzione ci sono già `Az_Anagrafica`, `Az_Cliente`, `Dip_Anagrafica` con dati e FK.

*Non è servita*: il 03/11/2026 il database di sviluppo è stato ricreato con le migration iniziali. Va realizzata solo prima del primo rilascio su un database **con dati** (installazioni già in uso).

**Multi-tenant** (per ogni azienda con presenze):
1. crea lo schema Common (`CREATE SCHEMA`), **senza** applicare la sua migration iniziale;
2. `ALTER TABLE "tenant_<id>_2"."Az_Cliente" SET SCHEMA "tenant_<id>_<Common>"` (e così per le altre): conserva righe, ID, sequenze identity, indici e le FK che arrivano dalle presenze;
3. registra la migration iniziale di Common come già applicata in `__EFMigrationsHistory_Common` dello schema;
4. le aziende **senza** presenze ricevono invece la migration iniziale normale (tabelle vuote) e un `Az_Anagrafica` creato.

**Modalità singola**: le tabelle sono già in `public`; serve solo il passo 3 (storico Common) e il passo AT sotto.

**Lato presenze (AttendanceTracking_Dev)**: la migration che toglie le tabelle dal modello presenze **non deve fare `DropTable`** (EF la genererebbe): va scritta a mano vuota/condizionale, perché le tabelle a quel punto appartengono a Common.

Ordine di esecuzione all'avvio: provisioning Common (con lo spostamento) → migration presenze. Va fatto con una procedura dedicata nel `DatabaseInitializer`, idempotente (se la tabella è già in Common non fa nulla), testata su una copia del DB di sviluppo (PG 14) prima di qualsiasi rilascio.

### 2.5 Branch e merge

- Entità, contesto, servizi, controller, pagine **Clienti** (e Contatti) passano in **Infrastructure_Dev**: le pagine `customer-list/edit`, il servizio Angular `Az_Cliente`, la pipe e l'handler AI dei clienti si spostano dalle cartelle `GestionePresenze` a quelle infrastrutturali.
- Il menu "Clienti" va in un gruppo **Anagrafiche** dell'infrastruttura, visibile se l'azienda ha almeno un applicativo che usa i clienti (presenze, CRM, documentale) – non legato a un singolo applicativo.
- **Tutti** i nuovi valori di `ApplicationType` (Common, CRM, DocumentManagement) si aggiungono **subito** in Infrastructure_Dev (anche nel gemello TypeScript in `company-model.ts`), così CRM_Dev e DocumentManagement_Dev non litigano sull'enum al merge in AllFunction. Decisi: `Common = 0` (già presente), `Crm = 3`, `DocumentManagement = 4` (valori **definitivi**: finiscono nei nomi degli schemi).
- Su AttendanceTracking_Dev, dopo il merge da Infrastructure: rimuovere le copie delle entità/servizi spostati, mappare le comuni come escluse, migration "vuota" (§2.4).
- I rami CRM_Dev e DocumentManagement_Dev ancora non esistono: si creano da **Infrastructure** *dopo* questo passo.

### 2.6 Integrazioni tra applicativi

Un applicativo non referenzia le tabelle di un altro. Quando serve un'integrazione (es. "ore lavorate per il cliente" nel CRM, prese dalle presenze):
- l'**interfaccia** si definisce in Infrastructure (es. `ICustomerActivityProvider`), con i modelli di scambio;
- l'**implementazione** sta nel ramo dell'applicativo che ha i dati (presenze);
- chi la usa (CRM) la chiede alla DI e la chiama solo se registrata **e** l'applicativo è attivo per l'azienda; altrimenti la sezione semplicemente non si mostra.
- Eventuali collegamenti a righe di un altro applicativo si salvano come **ID senza FK** (es. `Crm_Opportunita.IdCommessaPresenze`), con controllo di esistenza via interfaccia.

**Cancellazione di un record comune** (es. un cliente): le FK tra schemi la bloccano se è usato, ma il messaggio sarebbe tecnico. Registro `ICommonEntityUsage` (stesso schema dei deleter, già fatto in Infrastructure con l'implementazione delle presenze): ogni applicativo dice "questo cliente è usato da 3 commesse / 2 opportunità" e il servizio Clienti mostra un messaggio chiaro prima di cancellare. In aggiunta (deciso) il campo `Attivo` per disattivare il cliente senza cancellarlo.

---

## 3. Obiettivo del CRM

Gestire, per l'azienda, il rapporto con i propri **clienti e potenziali clienti**: anagrafiche e contatti (comuni), trattative (opportunità), attività commerciali (chiamate, appuntamenti, promemoria), offerte, storico. Integrato, quando attivi, con le presenze (commesse, ore) e il documentale (fascicolo del cliente).

Fuori dall'ambito della prima versione: fatturazione, magazzino, campagne email massive, telefonia (CTI), integrazione con caselle di posta (IMAP).

---

## 4. Ruoli e visibilità

Riusa i ruoli dell'azienda (ricordare: un utente può essere **User + Admin** insieme; mai dedurre il ruolo da `roles[0]`).

| Ruolo | Cosa fa nel CRM |
|---|---|
| CompanyPowerAdmin | configurazione (fasi della pipeline, tipi attività, prodotti, motivi di perdita, regole di visibilità), tutto il resto |
| CompanyAdmin | vede tutti i clienti/opportunità, assegna responsabili, report |
| User (commerciale) | vede i clienti/opportunità di cui è **responsabile** o in cui è coinvolto, più i clienti "condivisi"; gestisce le proprie attività |
| Studio / Dealer | nessun accesso ai dati CRM (se non impersonando, come oggi) |

Regola di visibilità configurabile per azienda (`Crm_Cfg.VisibilitaUtenti`: *tutti vedono tutto* / *solo i propri*), **predefinita "solo i propri"** (deciso il 04/11/2026). Le voci di menu si possono nascondere per ruolo con `CompanyMenuRestriction` (già esistente).

---

## 5. Modello dati CRM (schema `tenant_<id>_3`, prefisso `Crm_`)

Tutte le tabelle derivano da `BaseEntity` (Id, CreationDate, ModifiedDate, ChangeUser). Prefisso `Crm_` per evitare collisioni di nomi tra contesti (verificate all'avvio).

### 5.1 Configurazione

| Tabella | Contenuto |
|---|---|
| `Crm_Cfg` | 1-1 con `Az_Anagrafica`: visibilità, valuta, giorni di preavviso promemoria, numerazione offerte |
| `Crm_Par_Fase` | fasi della pipeline (Descrizione, Ordine, Probabilità % predefinita, Tipo: Aperta/Vinta/Persa) |
| `Crm_Par_TipoAttivita` | Chiamata, Email, Appuntamento, Attività, … (icona, colore, durata predefinita) |
| `Crm_Par_MotivoPerdita` | motivi di perdita di una trattativa |
| `Crm_Par_Fonte` | provenienza (sito, passaparola, fiera, …) |
| `Crm_Par_Prodotto` | prodotti/servizi offribili (codice, descrizione, prezzo di listino, UM, IVA) |
| `Crm_Par_Tag` | etichette libere per segmentare i clienti |

Il wizard di attivazione (come `company-attendance-wizard`) crea valori predefiniti multilingua.

### 5.2 Dati

| Tabella | Contenuto | Relazioni |
|---|---|---|
| `Crm_ClienteDati` | estensione 1-1 di `Az_Cliente`: Stato (Prospect / Cliente / Ex cliente / Non interessato), IdFonte, **responsabile** (IdAspNetUsers), settore, fatturato stimato, n. dipendenti, sito, consenso marketing + data | `Az_Cliente` (comune, FK), `AspNetUsers` (public) |
| `Crm_ClienteTag` | tag del cliente | `Az_Cliente`, `Crm_Par_Tag` |
| `Crm_Lead` | contatti non ancora qualificati (nome, azienda, email, telefono, fonte, note, stato). **Conversione** → crea `Az_Cliente` + `Az_ClienteContatto` + opzionale opportunità | dopo la conversione `IdAz_Cliente` |
| `Crm_Opportunita` | trattativa: titolo, cliente, contatto, fase, valore, probabilità, data chiusura prevista/effettiva, responsabile, motivo perdita, `IdCommessaPresenze` (ID debole) | `Az_Cliente`, `Az_ClienteContatto`, `Crm_Par_Fase` |
| `Crm_OpportunitaStorico` | cambi di fase/valore (chi, quando, da→a): base per i report di conversione | `Crm_Opportunita` |
| `Crm_Attivita` | tipo, oggetto, descrizione, data/ora inizio-fine, scadenza, assegnatario, stato (Da fare / Fatta / Annullata), esito; collegata a cliente e/o contatto e/o opportunità e/o lead | FK facoltative |
| `Crm_AttivitaPartecipante` | altri utenti coinvolti (appuntamenti) | `AspNetUsers` |
| `Crm_Offerta` + `Crm_OffertaRiga` | numero/anno, data, validità, stato (Bozza/Inviata/Accettata/Rifiutata), righe con prodotto, quantità, prezzo, sconto, IVA; totali | `Crm_Opportunita`, `Az_Cliente`, `Crm_Par_Prodotto` |
| `Crm_Nota` | note libere sul cliente/opportunità (testo, autore, data, fissata in alto) | |
| `Crm_Allegato` | file della trattativa/offerta (stesso schema di `Dip_GG_NotaSpesa_Allegato`). **Sempre qui**, anche con il documentale attivo (deciso il 04/11/2026) | |

### 5.3 Integrazioni (facoltative, §2.6)

- **Presenze attive**: dalla scheda cliente, sezione "Commesse e ore" (lettura tramite interfaccia: commesse del cliente, ore del periodo dalle statistiche attività). Da un'opportunità **vinta** si può proporre la creazione della commessa nelle presenze (`ICommessaCreator` implementato su AttendanceTracking_Dev) e se ne salva l'ID in `IdCommessaPresenze`.
- **Documentale attivo**: la scheda cliente mostra (in lettura) il **fascicolo del cliente** del documentale; gli allegati del CRM restano comunque in `Crm_Allegato` (deciso).
- **Ticket/assistenza**: nessuna dipendenza.

---

## 6. Funzioni (pagine)

### 6.1 Utente / commerciale
- **Clienti** (pagina comune, §2.5) con, se il CRM è attivo, le colonne/filtri CRM (stato, responsabile, tag, ultima attività) e la **scheda cliente** a schede: dati, contatti, opportunità, attività, offerte, note, allegati, (commesse/ore), (documenti).
- **Lead**: elenco, modifica, conversione in cliente.
- **Pipeline**: vista kanban delle opportunità per fase (trascinamento = cambio fase, con storico) + vista elenco.
- **Agenda attività**: calendario e lista "da fare oggi / scadute / prossime" (riuso del pattern calendario delle presenze).
- **Offerte**: elenco, modifica righe, **PDF** dell'offerta (stessa libreria già usata per gli export), invio per email (servizio email esistente).
- **Promemoria**: notifica (SignalR, `job-notifier`/`ticket-notifier` come modello) per attività in scadenza.

### 6.2 Admin
- Configurazione (§5.1), **riassegnazione** in blocco dei clienti di un commerciale (es. dimissioni).
- **Report**: valore pipeline per fase/responsabile, tasso di conversione, opportunità vinte/perse per periodo e motivo, attività per utente. Grafici con le stesse componenti delle statistiche presenze.
- **Importazione** clienti/contatti da Excel/CSV: riuso del modello dell'importazione dipendenti (lettura file con `FileText`, mappatura colonne assistita dall'AI), con controllo duplicati per P.IVA/CF/email.
- **Esportazione** Excel degli elenchi.

### 6.3 AI (riuso della chat AI e del provider già configurato)
- handler chat `ChatAiHandler/Crm`: "apri il cliente X", "attività di oggi", "opportunità in chiusura questo mese", creazione attività a voce;
- **riepilogo del cliente** (note + attività + opportunità) e **suggerimento della prossima azione**;
- **bozza email** di follow-up a partire dall'attività;
- lettura biglietto da visita/firma email → lead (facoltativo, fase 2).

---

## 7. Aspetti trasversali

- **Multilingua**: risorse `CrmMessages.resx/.en.resx` sul server, cartella i18n `crm` sul client (`i18n-folders.ts`), come per le presenze.
- **Menu e rotte**: `main-menu-crm.service.ts`, `route-crm.service.ts`, `site-name-crm.ts`, `showcase-crm.service.ts`, registrati in `main-menu.service.ts`/`route.service.ts` con `forApplication(..., ApplicationType.Crm)`.
- **Backend**: `Installers4Crm` registra `CrmDbContext` con `AddApplicationDbContext<CrmDbContext>(ApplicationType.Crm)`; `CrmDbContextDesignTimeFactory` per `dotnet ef`; cartelle `Crm` in entità, repository, servizi, mapper (AutoMapper), controller, seguendo `CONTRIBUTING_infrastruttura.md`.
- **Uso dei clienti comuni**: `CrmCommonEntityUsage : ICommonEntityUsage` ("opportunità: 2, offerte: 1") per bloccare con un messaggio chiaro la cancellazione di un cliente usato.
- **Cancellazione fisica**: `CrmDataDeleter : IApplicationDataDeleter` (conteggi per l'anteprima; dati in modalità singola; per l'utente rimosso: riassegna o svuota il responsabile, non cancella i clienti).
- **GDPR**: consenso marketing con data e fonte; esportazione dei dati di un contatto; cancellazione di un contatto su richiesta (diritto all'oblio) senza perdere le statistiche (anonimizzazione).
- **Pagine**: elenchi con `app-generic-list`, modifiche con `BasePageConfirmCancelComponent`, dialog con `BaseDialogConfirmCancelComponent`.
- **Sicurezza**: ogni servizio verifica il ruolo e la visibilità (§4) lato server, non solo nel menu (vedi `piano-security.md`).

---

## 8. Piano di lavoro

Le fasi sono quelle della checklist al §10: **Fase 0** in Infrastructure_Dev (prerequisito comune a CRM e documentale, poi merge fino alle presenze), **Fase 1** su CRM_Dev, **Fase 2** integrazioni con presenze e documentale.

---

## 9. Decisioni

1. **Schema Common** (§2.1 opzione D) – deciso e predisposto il 03/11/2026.
2. **Valori dell'enum**: `Common = 0`, `Crm = 3`, `DocumentManagement = 4`, da aggiungere subito in Infrastructure – deciso il 04/11/2026.
3. **`Az_Cliente.Default`** – si sposta in `Az_Cfg` delle presenze (`IdAz_ClienteDefault`) – deciso il 04/11/2026.
4. **`Az_Sedi`/`Az_SediReparto`** – restano nelle presenze – deciso il 04/11/2026.
5. **Visibilità dei commerciali** – configurabile per azienda, predefinita "solo i propri" – deciso il 04/11/2026.
6. **Allegati CRM** – sempre in `Crm_Allegato`, anche con il documentale attivo – deciso il 04/11/2026.
7. **Clienti disattivati** – nuovo campo `Attivo` in `Az_Cliente` – deciso il 04/11/2026.
8. **Fornitori** – nuovo campo `Tipo` (Cliente/Fornitore/Entrambi) in `Az_Cliente` – deciso il 04/11/2026.
9. **Attivazione** – solo SuperUser e DealerPowerAdmin del centro, per tutti gli applicativi – deciso il 03/11/2026 (`ANALISI_DOCUMENTALE.md` §4.1, S2).
10. **`Dip_Anagrafica` in Common** – deciso il 04/11/2026 (`ANALISI_DOCUMENTALE.md` §2.1).

---

## 10. Lavori fatti e da fare

Legenda: [x] fatto · [ ] da fare.

### Fase 0 – Dati comuni e prerequisiti (Infrastructure_Dev, poi merge fino ad AttendanceTracking)

Questa fase è comune a CRM e documentale: la checklist del documentale (`ANALISI_DOCUMENTALE.md` §11) rimanda qui.

**Schema Common** (fatto il 03/11/2026, commit f4b99d2c su Infrastructure_Dev e 2b5e63b3 su AttendanceTracking_Dev)
- [x] `ApplicationType.Common = 0`, escluso dall'attivazione degli applicativi (server e client)
- [x] `CommonDbContext`, accessor senza controllo di attivazione, provisioning per tutte le aziende (Common migrato per primo)
- [x] search_path dei contesti degli applicativi: schema app, schema Common, public
- [x] `Az_Anagrafica` e `Az_Cliente` spostate in Common; nuova `Az_ClienteContatto`
- [x] repository in `Repositories/Tenant/Common`, utilità `ICommonCompanyUtility`, inizializzatore `CommonCompanyInitializer`
- [x] API comuni `api/Az_Cliente/*` (filtrate sull'azienda corrente) e `api/Az_ClienteContatto/*`
- [x] `ICommonEntityUsage` (cliente usato non cancellabile) con l'implementazione delle presenze (commesse)
- [x] `CommonDataDeleter` (eseguito per ultimo) nella cancellazione fisica
- [x] presenze adeguate (AttendanceTracking_Dev): tabelle comuni mappate escluse, servizi clienti tolti, "azienda nuova" = senza `Az_Cfg`
- [x] migration azzerate (InitPublic, InitCommon, InitMoke, AttendanceTracking_Init) e database di sviluppo ricreato
- [x] prove via API in multi-tenant (clienti, contatti, isolamento tra aziende, attivazione presenze, blocco della cancellazione)
- [ ] prova in **modalità singola** (tutto in public)

**Decisioni del 03-04/11/2026 da realizzare** (spostamento e adattamento dei dati fatti il 04/11/2026, commit 71978b5c e seguenti su Infrastructure_Dev; migration azzerate e database ricreato)
- [x] `ApplicationType`: `Crm = 3`, `DocumentManagement = 4` (C# e TypeScript); si attivano solo gli applicativi con contesto registrato nel ramo
- [x] `Dip_Anagrafica` in Common, con `IdAspNetUsers` facoltativo e conservazione dopo la rimozione dell'utente (Infrastructure_Dev + AttendanceTracking_Dev; dettaglio in `ANALISI_DOCUMENTALE.md` §11)
- [x] `Az_Cliente.Default` → `Az_Cfg.IdAz_ClienteDefault` (Infrastructure_Dev + AttendanceTracking_Dev)
- [x] `Az_Cliente.Attivo` (campo)
- [x] clienti disattivati esclusi dalle scelte e "Mostra disabilitati" negli elenchi (client; 04/11)
- [x] `Az_Cliente.Tipo` (Cliente / Fornitore / Entrambi, campo)
- [x] scelte "cliente" limitate a Cliente ed Entrambi nel client (commessa delle presenze, opportunità e offerte del CRM; 04/11) – i servizi delle presenze non lo ricontrollano ancora
- [x] migration azzerate (InitPublic, InitCommon, InitMoke, AttendanceTracking_Init) e database di sviluppo ricreato il 04/11/2026; prove via API (attivazione presenze, dipendenti nello schema Common, cliente predefinito in `Az_Cfg`, blocco della cancellazione, CRM non attivabile finché non ha il suo contesto)
- [x] attivazione degli applicativi solo da SuperUser e DealerPowerAdmin del centro (`ApplicationActivationAuthorizer`, fatto con il documentale)
- [x] `FinancialAdvisorApplication` (abilitazione degli applicativi per studio) e applicativi dello studio in `UserLoad` (fatto con il documentale)

**Client comune**
- [x] pagine Clienti (`customer-list/edit`) e servizio Angular nell'infrastruttura, copie tolte dalle presenze; la cache delle presenze si ricarica con `CommonDataEventsService` (04/11). La pipe e l'handler AI dei clienti restano nelle presenze (li usano solo loro)
- [x] menu **Anagrafiche** dell'infrastruttura, visibile con almeno un applicativo che usa i clienti (`MainMenuItem.anyApplication`)
- [x] contatti del cliente nella pagina di modifica del cliente (elenco + dialog)
- [x] campi `Attivo` e `Tipo` nelle pagine clienti; modifiche all'anagrafica comune solo per CompanyPowerAdmin e CompanyAdmin

**Prima del primo rilascio su un database con dati**
- [ ] procedura di spostamento dei dati presenze → Common (§2.4), idempotente, provata su una copia del database

### Fase 1 – CRM (CRM_Dev, creato da Infrastructure dopo la Fase 0)
Implementata il 04/11/2026 (commit 846a9400 e seguenti su CRM_Dev; in Infrastructure: punti di aggancio, `IUserNotifier`, email con allegati).
- [x] creazione del ramo CRM_Dev da Infrastructure
- [x] `CrmDbContext` (`ApplicationType.Crm`, schema `tenant_<id>_3`), design-time factory, `Installers4Crm`, migration `Crm_Init` (17 tabelle)
- [x] configurazione (§5.1) con valori predefiniti multilingua creati all'attivazione (`CrmCompanyInitializer`; nessun wizard dedicato: si modificano dalle pagine Tabelle e Impostazioni)
- [x] `Crm_ClienteDati` e scheda cliente CRM (stato, responsabile, fonte, tag, settore, fatturato, consenso) con contatti, opportunità, attività, offerte, note e allegati
- [x] visibilità dei commerciali (`Crm_Cfg.Visibilita`, predefinita "solo i propri": clienti propri, senza responsabile o in cui sono coinvolti), controllata in ogni servizio (`CrmUtility`)
- [x] lead e conversione in cliente (nuovo o esistente, contatto, opportunità; attività e note passano al cliente)
- [x] opportunità, pipeline kanban (trascinamento HTML5, scelta della fase su mobile, motivo per le perse), storico delle fasi e del valore
- [x] attività, agenda (oggi / scadute / prossime / fatte), promemoria SignalR (`CrmReminderService` + `IUserNotifier` di Infrastructure)
- [x] offerte con righe, totali calcolati dal server, numerazione per anno, PDF (MigraDoc) e invio email con il PDF allegato
- [x] note (fissate, autore) e allegati (`Crm_Allegato`, max 20 MB, su disco; non contano nello spazio del documentale)
- [x] report (pipeline per fase/responsabile con valore pesato, vinte/perse nel periodo, conversione dei lead, attività per utente)
- [ ] import clienti/contatti da Excel/CSV con controllo duplicati
- [x] export Excel dei clienti (ClosedXML, con i filtri della pagina)
- [x] riassegnazione in blocco di clienti, opportunità, lead e attività da fare di un commerciale
- [x] `CrmCommonEntityUsage` (cliente usato da opportunità/offerte/attività/allegati non cancellabile) e `CrmDataDeleter` (utente rimosso: dati senza responsabile, attività annullate)
- [x] menu, rotte, multilingua (`CrmMessages`, i18n `crm`) – site-name e showcase non servono per ora
- [x] handler chat AI CRM (11/11/2026: vedi ANALISI_CRM_VOCALI.md)
- [x] GDPR: consenso marketing con data (clienti e lead)
- [ ] GDPR: esportazione e anonimizzazione di un contatto
- [x] prove via API (42 controlli: visibilità, totali, PDF, conversione, riassegnazione, blocco della cancellazione) e nel browser sull'applicativo completo

### Fase 2 – Integrazioni
- [ ] presenze: sezione "Commesse e ore" nella scheda cliente (interfaccia in Infrastructure, implementazione su AttendanceTracking_Dev)
- [ ] presenze: da opportunità vinta a commessa (`ICommessaCreator`, `IdCommessaPresenze`)
- [ ] documentale: fascicolo del cliente in lettura nella scheda cliente
- [ ] AI avanzata (riepilogo cliente, prossima azione, bozza email)

---

## 11. Guida all'uso (come si usa il CRM)

Spiegazione pratica del funzionamento, dal punto di vista di chi lo usa. Il dettaglio pagina per pagina è nel manuale utente (`nvxapp.server.service/Manuals/crm.it.md`).

### 11.1 A cosa serve

Il CRM tiene traccia di **chi sono i clienti (o potenziali clienti), cosa si sta trattando con loro e cosa bisogna fare dopo**. Ruota tutto attorno a cinque oggetti:

| Oggetto | Cos'è | Esempio |
|---|---|---|
| **Lead** | contatto non ancora qualificato | "Mario Rossi, ha lasciato il biglietto in fiera" |
| **Cliente** | azienda con cui si ha (o si vuole avere) un rapporto; è l'anagrafica clienti **comune**, la stessa di presenze e documentale | "Rossi Srl" |
| **Opportunità** | una trattativa con un cliente, con valore e probabilità | "Fornitura software, 15.000 €, 60%" |
| **Attività** | cosa fare / fatta: telefonata, email, appuntamento | "Richiamare Rossi giovedì alle 10" |
| **Offerta** | il preventivo con righe e prezzi, PDF e invio per email | "Offerta 2026/014" |

### 11.2 Il flusso tipico

```
Lead ──(Converti)──► Cliente ──► Opportunità ──► Offerta ──► Vinta / Persa
                        │            │
                        └── Attività (chiamate, appuntamenti…) lungo tutto il percorso
```

1. **Entra un contatto nuovo** (sito, fiera, passaparola): **Lead** → **+** → **Nuovo lead** (nome, azienda, email, telefono, fonte). Lo si contatta e si registra un'attività ("chiamata fatta").
2. **Il contatto è interessato**: dal lead → **Converti**. Diventa un **Cliente**, la persona diventa un suo **Contatto** e si può creare subito un'**Opportunità**. Se il cliente è già noto si salta il lead: **Clienti** → **+** → **Nuovo cliente**.
3. **Si lavora la trattativa**:
   - dalla **scheda cliente**, con il **+** del FAB si sceglie (menu dal basso) cosa aggiungere: Contatto, Opportunità, Attività, Offerta, già collegati al cliente;
   - nella **Pipeline** le opportunità sono in colonne per fase: quando la trattativa avanza si trascina la scheda nella colonna successiva (se va in "persa" viene chiesto il motivo).
4. **Si fa il preventivo**: **Offerta** → righe (scegliendo un prodotto si compilano prezzo e IVA) → **Conferma** (riceve il numero) → **PDF** per scaricarla, **Invia** per mandarla per email (diventa "Inviata"). Poi si aggiorna lo stato: Accettata / Rifiutata.
5. **Si chiude**: l'opportunità va in una fase "vinta" o "persa". Lo storico registra ogni passaggio ed è la base dei **Report**.

### 11.3 Uso quotidiano del commerciale

La pagina principale è l'**Agenda**: Oggi, **Scadute** (numero in rosso), Prossime, Fatte.

- Finita una chiamata: **Fatta** e si scrive l'esito ("richiamare a fine mese").
- Si crea subito l'attività successiva.
- Prima di ogni attività arriva un promemoria a comparsa (predefinito 30 minuti prima), se si è collegati all'applicazione.

### 11.4 Prima di iniziare (amministratore)

1. **Impostazioni CRM**: visibilità (ogni commerciale vede solo i suoi clienti / tutti), valuta, preavviso promemoria, prefisso e condizioni delle offerte.
2. **Tabelle CRM** (create con valori predefiniti, da adattare): **fasi** della pipeline con la probabilità, **tipi di attività**, **prodotti** con prezzi e IVA, **fonti**, **motivi di perdita**, **etichette**.
3. **Ruoli**: *User* = commerciale; *CompanyAdmin* = vede tutto, assegna i clienti, report; *CompanyPowerAdmin* = anche la configurazione.

### 11.5 Prova veloce (10 minuti)

1. Creare un lead di prova e convertirlo in cliente creando anche l'opportunità.
2. Dalla scheda cliente aggiungere (con il **+**) un'attività per domani e un'offerta.
3. Andare nella pipeline e spostare l'opportunità di fase.
4. Controllare come cambiano Agenda e Report.
