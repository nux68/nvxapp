# Analisi – Applicativo CRM

Branch: **CRM_Dev** (riceve i merge da Infrastructure, si mergia in CRM).
La parte sulle **tabelle condivise** (§2) è lavoro di **Infrastructure_Dev** e va fatta *prima* di iniziare il CRM: vale anche per il Documentale (`ANALISI_DOCUMENTALE.md`, che rimanda qui).

Stato: analisi, nessuna implementazione. Le scelte da confermare sono raccolte al §9.

---

## 1. Situazione attuale (cosa c'è già)

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
| `Dip_Anagrafica` | presenze | **Common** (vedi `ANALISI_DOCUMENTALE.md` §2) | presenze, documentale |
| `Az_Sedi`, `Az_SediReparto` | presenze | restano nelle presenze per ora (legate ad approvazioni/attività) – decisione §9 | presenze |
| `Az_Commessa` e figli | presenze | **restano nelle presenze** (sottocommesse, attività, reparti, utenti sono logica presenze) | presenze; il CRM le collega in modo "debole" (§5.3) |
| `Az_Cfg`, `Par_*` | presenze | restano | presenze |

Campo `Az_Cliente.Default`: oggi serve al wizard presenze per il cliente della commessa predefinita. Si può tenere (innocuo) oppure spostare in `Az_Cfg` delle presenze come `IdAz_ClienteDefault`. Consiglio: spostarlo, per rispettare la regola "nessun campo di un solo applicativo".

### 2.4 Spostamento dei dati esistenti (presenze → Common)

È il punto più delicato: in produzione ci sono già `Az_Anagrafica`, `Az_Cliente`, `Dip_Anagrafica` con dati e FK.

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
- **Tutti** i nuovi valori di `ApplicationType` (Common, CRM, DocumentManagement) si aggiungono **subito** in Infrastructure_Dev (anche nel gemello TypeScript in `company-model.ts`), così CRM_Dev e DocumentManagement_Dev non litigano sull'enum al merge in AllFunction. Proposta: `Common = 3`, `Crm = 4`, `DocumentManagement = 5` (valori **definitivi**: finiscono nei nomi degli schemi).
- Su AttendanceTracking_Dev, dopo il merge da Infrastructure: rimuovere le copie delle entità/servizi spostati, mappare le comuni come escluse, migration "vuota" (§2.4).
- I rami CRM_Dev e DocumentManagement_Dev ancora non esistono: si creano da **Infrastructure** *dopo* questo passo.

### 2.6 Integrazioni tra applicativi

Un applicativo non referenzia le tabelle di un altro. Quando serve un'integrazione (es. "ore lavorate per il cliente" nel CRM, prese dalle presenze):
- l'**interfaccia** si definisce in Infrastructure (es. `ICustomerActivityProvider`), con i modelli di scambio;
- l'**implementazione** sta nel ramo dell'applicativo che ha i dati (presenze);
- chi la usa (CRM) la chiede alla DI e la chiama solo se registrata **e** l'applicativo è attivo per l'azienda; altrimenti la sezione semplicemente non si mostra.
- Eventuali collegamenti a righe di un altro applicativo si salvano come **ID senza FK** (es. `Crm_Opportunita.IdCommessaPresenze`), con controllo di esistenza via interfaccia.

**Cancellazione di un record comune** (es. un cliente): le FK tra schemi la bloccano se è usato, ma il messaggio sarebbe tecnico. Serve un registro `ISharedEntityUsageChecker` (stesso schema dei deleter): ogni applicativo dice "questo cliente è usato da 3 commesse / 2 opportunità" e il servizio Clienti mostra un messaggio chiaro prima di cancellare. In alternativa (consigliato in aggiunta) disattivazione logica del cliente.

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

Regola di visibilità configurabile per azienda (`Crm_Cfg.VisibilitaUtenti`: *tutti vedono tutto* / *solo i propri*). Le voci di menu si possono nascondere per ruolo con `CompanyMenuRestriction` (già esistente).

---

## 5. Modello dati CRM (schema `tenant_<id>_4`, prefisso `Crm_`)

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
| `Crm_Allegato` | file della trattativa/offerta (stesso schema di `Dip_GG_NotaSpesa_Allegato`). Se il documentale è attivo, vedi §5.3 | |

### 5.3 Integrazioni (facoltative, §2.6)

- **Presenze attive**: dalla scheda cliente, sezione "Commesse e ore" (lettura tramite interfaccia: commesse del cliente, ore del periodo dalle statistiche attività). Da un'opportunità **vinta** si può proporre la creazione della commessa nelle presenze (`ICommessaCreator` implementato su AttendanceTracking_Dev) e se ne salva l'ID in `IdCommessaPresenze`.
- **Documentale attivo**: la scheda cliente mostra il **fascicolo del cliente** del documentale; gli allegati CRM possono essere archiviati lì invece che in `Crm_Allegato` (decisione §9).
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
- **Cancellazione fisica**: `CrmDataDeleter : IApplicationDataDeleter` (conteggi per l'anteprima; dati in modalità singola; per l'utente rimosso: riassegna o svuota il responsabile, non cancella i clienti).
- **GDPR**: consenso marketing con data e fonte; esportazione dei dati di un contatto; cancellazione di un contatto su richiesta (diritto all'oblio) senza perdere le statistiche (anonimizzazione).
- **Pagine**: elenchi con `app-generic-list`, modifiche con `BasePageConfirmCancelComponent`, dialog con `BaseDialogConfirmCancelComponent`.
- **Sicurezza**: ogni servizio verifica il ruolo e la visibilità (§4) lato server, non solo nel menu (vedi `piano-security.md`).

---

## 8. Piano di lavoro

**Fase 0 – Infrastructure_Dev (prerequisito comune a CRM e documentale)**
1. `ApplicationType`: `Common = 3`, `Crm = 4`, `DocumentManagement = 5` (C# e TypeScript).
2. `CommonDbContext`, accessor "sempre attivo", provisioning per tutte le aziende, search_path a tre schemi, ordine dei deleter.
3. Spostamento entità `Az_Anagrafica`, `Az_Cliente`, `Dip_Anagrafica` + nuova `Az_ClienteContatto`; servizi, controller, pagine Clienti/Contatti, menu "Anagrafiche".
4. Procedura di spostamento dati (§2.4), `ISharedEntityUsageChecker`.
5. Merge Infrastructure → AttendanceTracking_Dev: adeguamento presenze (rimozione copie, mappature escluse, migration vuota) e prova completa su DB copia, in modalità singola **e** multi-tenant.

**Fase 1 – CRM_Dev (creato da Infrastructure)**
6. Contesto, configurazione, wizard di attivazione.
7. Scheda cliente CRM, lead, opportunità + pipeline, attività + agenda + promemoria.
8. Offerte con PDF ed email.
9. Report, import/export.
10. Chat AI CRM, deleter, multilingua.

**Fase 2 – integrazioni**: commesse/ore presenze, fascicolo documentale, AI avanzata.

---

## 9. Decisioni da prendere

1. **Schema Common** (§2.1 opzione D) – confermare l'approccio.
2. **Valori dell'enum** `Common = 3`, `Crm = 4`, `DocumentManagement = 5`.
3. **`Az_Cliente.Default`** – spostarlo in `Az_Cfg` delle presenze (consigliato) o tenerlo.
4. **`Az_Sedi`/`Az_SediReparto`** – restano nelle presenze ora, o vanno subito in Common (servirebbero al documentale per distribuire documenti per reparto)?
5. **Visibilità predefinita** dei commerciali: tutti vedono tutto, o solo i propri?
6. **Allegati CRM** con documentale attivo: archiviati nel documentale o sempre in `Crm_Allegato`?
7. **Clienti disattivati** (campo `Attivo` in `Az_Cliente`) invece della sola cancellazione: consigliato.
8. Se in futuro il CRM dovrà gestire anche **fornitori**: `Az_Cliente` con un campo Tipo (Cliente/Fornitore/Entrambi), da decidere ora perché la tabella diventa comune.
