# ANALISI – Infrastructure – Personalizzazioni

Data: 22/11/2026 — Branch di lavoro: `Infrastructure_Dev_personalizzazioni` (da `Infrastructure_Dev`)

Stato: **analisi, nessun codice scritto**. Le scelte del §9 vanno confermate prima di iniziare.

---

## 1. Obiettivo

Le aziende e gli studi possono chiedere una **personalizzazione**: funzioni fatte su misura, a pagamento, visibili solo a chi le ha chieste.
Se la richiesta parte da uno studio, tutto quello che segue vale per **tutte le aziende dello studio**.

Il lavoro ha due parti:

- **amministrativa** (§3): menu riservato a chi ha una licenza Pro, richiesta verso il super user con testo, allegati e scambio di domande e risposte, licenza con pagamento una tantum e/o canone, attivazione a pagamenti effettuati;
- **tecnica** (§4–§8): codice univoco, menu e form riservati, tabelle in uno schema del cliente, integrazione nelle pagine esistenti, impatto sui DataContext, posizionamento delle classi, comandi chat e vocali.

## 2. Situazione attuale (cosa c'e' gia' e cosa manca)

| Tema | Oggi | Conseguenza |
|---|---|---|
| Licenza Pro | `LivelloLicenza {Free, Standard, Pro}` e' **per ambito** (un applicativo o Cumulativo), non per cliente. `ILicenseGuard.IsProAsync(app)` (`LicenseGuard.cs:197`) | "Avere una licenza Pro" va definito (decisione D1) |
| Client e Pro | `auth.service.ts:73` `licenseLevel` risponde **Pro** se le licenze sono spente o se l'applicativo non e' nell'elenco | Il menu non puo' usare `minLicense`: serve un dato esplicito da `UserLoad` |
| Pagamenti | `Lic_Pagamento` copre solo canoni mensili/annuali; `IdAbbonamento` e' gia' nullable. Gateway `IPaymentGateway` (PayPal, Debug) | **L'una tantum non esiste**: va aggiunta |
| Attivazione a pagamento | `LicenseSubscriptionManager.ConfirmPaymentAsync` (:301) → abbonamento attivo → `ActivateForLicenseAsync` → `RefreshLicense` sul client | Stesso giro riusabile per le personalizzazioni |
| Richiesta al super user | I **ticket** (`Ticket`, `TicketCronology`, `TicketAttach`, `TicketRules`) hanno testo, allegati e domanda/risposta fino al super user. Le "Richieste" (`Dip_GG_Richiesta`) sono approvazioni interne all'azienda | Il modello da copiare sono i ticket, non le Richieste |
| Allegati | `IFileStorage` richiede `idCompany > 0`; i ticket salvano gli allegati nel DB | Per gli studi (senza azienda) gli allegati vanno nel DB come nei ticket |
| Studio → aziende | Nessuna tabella di assegnazione: si ricava da `Company.IdFinancialAdvisor` (regola `DisponibileSuCompany` del guard) | Stessa regola per le personalizzazioni dello studio |
| Menu | Codice statico nel client (`main-menu-*.service.ts`), filtrato per ruolo, applicativo, `minLicense` | Le voci riservate richiedono un meccanismo nuovo |
| Controllo lato server | Nessun filtro tipo `[RequiresLicense]`; controlli nei servizi e in `TenantSchemaAccessor` | Va introdotto un controllo server per le personalizzazioni |
| Database | PostgreSQL, uno schema per azienda e per applicativo: `tenant_<IdCompany>_<ApplicationType>`. I modelli EF non contengono lo schema: lo sceglie `TenantSearchPathInterceptor` con `SET search_path`. Nessun `IModelCacheKeyFactory` | Uno schema in piu' per personalizzazione si innesta bene |
| DataContext | Un contesto per applicativo (`ApplicationDbContextBase`), registro `ApplicationDbContextRegistry`, migrazioni per contesto, ciclo `TenantProvisioningService.MigrateAllAsync` | Un contesto per personalizzazione segue lo stesso schema |
| Moduli | Separati per **branch git**, non per assembly ne' plugin. 3 soli assembly. DI a scansione (`Installers.cs:118-122`, repository per namespace) | Le classi dei clienti si possono mettere in cartelle dedicate senza toccare la registrazione |
| Estensione delle pagine | Esiste solo il catalogo dei widget (`WidgetDefinition` + `*ngComponentOutlet`). Nessun campo dinamico, nessuna sezione configurabile | Va creato un meccanismo di "punti di estensione" |
| Comandi chat/vocali | Un solo sistema: il vocale e' testo che arriva alla chat. Un comando = una classe `ICommandHandler`, chiave `IntentDefinition.Name`. Filtri: ruolo e applicativo attivo | Manca il filtro per cliente |
| Conflitti tra comandi | **Nessun controllo.** Due comandi con lo stesso nome fanno fallire la chat per tutti (`CommandRegistry.cs:14`, `ToDictionary`). Frasi sovrapposte: decide il modello linguistico | Vanno introdotte regole e un controllo all'avvio |

## 3. Gestione amministrativa

### 3.1 Menu "Personalizzazioni" (solo licenza Pro)

- Nuova voce nel menu di infrastruttura, per `CompanyPowerAdmin` e `FinancialAdvisorPowerAdmin` (chi oggi puo' comprare le licenze) e per il super user (lato gestione).
- Visibile se il soggetto ha **almeno un abbonamento attivo di livello Pro** (proposta D1): proprio, dello studio con `DisponibileSuCompany`, o Cumulativo.
- Server: nuovo metodo `ILicenseGuard.HasAnyProAsync()`; `UserLoad` restituisce `CanRequestCustomization` (bool). Il client usa quel dato, non `minLicense`.
- I servizi delle richieste ricontrollano la condizione lato server (il menu nascosto non e' una protezione).
- Chi ha gia' una personalizzazione e perde la Pro continua a vedere le proprie personalizzazioni e i pagamenti, ma non puo' aprire nuove richieste.

### 3.2 Richiesta di personalizzazione

Tabelle nuove nello schema `public`, modellate sui ticket:

| Tabella | Contenuto |
|---|---|
| `Pers_Richiesta` | Soggetto (`IdCompany` **oppure** `IdFinancialAdvisor`), utente richiedente, `Oggetto`, `Testo`, `Stato`, `IdDealer` di origine, `Version` |
| `Pers_RichiestaMessaggio` | Scambio di domande e risposte: autore, lato (cliente / super user), testo, data, tipo (Domanda, Risposta, Nota, Offerta, Accettazione, Rifiuto) |
| `Pers_RichiestaAllegato` | File nel DB (`byte[]`), legato alla richiesta o a un messaggio; stesse regole di `TicketRules` (5 file, 10 MB l'uno, 20 MB totali, estensioni ammesse) |

Stati della richiesta:

```
Bozza → Inviata → InValutazione ⇄ AttesaCliente → Offerta → Accettata → InSviluppo → Rilasciata
                         └→ Rifiutata              └→ RifiutataDalCliente
        (Annullata dal cliente fino a Offerta)
```

- **Offerta**: il super user indica descrizione, importo una tantum (facoltativo), canone mensile e/o annuale (facoltativo), validita' dell'offerta. Almeno uno dei due importi e' obbligatorio.
- **Accettata**: genera la personalizzazione e la sua licenza (§3.3).
- Notifiche in tempo reale e categoria di notifica dedicata, come `TicketNotifier`.
- Pagine client: elenco, nuova richiesta, dettaglio con conversazione e allegati (lato cliente); elenco e dettaglio con offerta (lato super user).

Perche' tabelle dedicate e non un tipo di ticket: il ticket ha livelli di inoltro (dealer, studio), risposta automatica dell'AI e stati da assistenza; qui servono offerta, prezzi e accettazione. Si riusano regole degli allegati, notifiche e impostazione delle pagine (alternativa in D2).

### 3.3 Licenza della personalizzazione

| Tabella (`public`) | Contenuto |
|---|---|
| `Pers_Personalizzazione` | **Definizione**: `Id` (mai riusato), `Codice` univoco (§4), nome, descrizione, `Dipendenze` (applicativi richiesti), `Stato` (InSviluppo, Rilasciata, Ritirata), `IdRichiesta` di origine |
| `Pers_Licenza` | **Assegnazione a un soggetto**: `IdPersonalizzazione`, `IdCompany` oppure `IdFinancialAdvisor`, `ImportoUnaTantum`, `CanoneMensile`, `CanoneAnnuale`, `Periodicita` scelta, `DataInizio`, `DataScadenza`, `Stato`, `RinnovoAutomatico` |

Definizione e assegnazione sono separate: la stessa personalizzazione potra' essere venduta a un secondo cliente senza rifare nulla (decisione D3).

Pagamenti: si riusa `Lic_Pagamento`, cosi' restano validi gateway, rendiconti dei dealer e manutenzione notturna. Modifiche:

- nuovo campo `Tipo` {Abbonamento, PersonalizzazioneUnaTantum, PersonalizzazioneCanone};
- nuovo campo `IdPersLicenza` (nullable);
- per l'una tantum `DataInizioCoperta`/`DataFineCoperta` non hanno significato: si valorizzano entrambe con la data del pagamento;
- nuovo valore `AmbitoLicenza.Personalizzazione` per i filtri dei rendiconti.

I due pagamenti possono coesistere: una tantum (una riga) e canone (una riga per periodo, con rinnovo come gli abbonamenti).

### 3.4 Attivazione

Una personalizzazione e' **attiva per un soggetto** quando valgono tutte:

1. `Pers_Personalizzazione.Stato = Rilasciata` (il codice e' in produzione; lo imposta il super user dopo il rilascio);
2. esiste una `Pers_Licenza` del soggetto (o del suo studio);
3. l'una tantum, se prevista, e' `Completato`;
4. il canone, se previsto, copre la data di oggi (con i giorni di tolleranza `Licenze_GraceGiorni`).

Regole collegate:

- **Licenze spente** (`Licenze_Attive = false`): oggi il guard diventa neutro e risponde "tutto permesso". Per le personalizzazioni questo **non deve valere**: resterebbero visibili a tutti. Con le licenze spente si ignorano i punti 3 e 4, ma i punti 1 e 2 restano obbligatori.
- **Studio**: la licenza intestata allo studio vale per tutte le aziende con `Company.IdFinancialAdvisor` = studio, comprese quelle aggiunte dopo.
- **Canone scaduto**: le funzioni si spengono, i dati restano nello schema; eliminazione dopo un periodo di conservazione o su richiesta (decisione D6).
- Unico punto di verita': nuovo servizio `ICustomizationGuard` (`IsActiveAsync(codice)`, `ActiveCodesAsync()`), con cache per richiesta. `UserLoad` restituisce `Customizations: string[]` (codici attivi) e il client lo aggiorna con lo stesso giro di `RefreshLicense`.
- La manutenzione notturna (`LicenseMaintenanceService`) gestisce scadenza, rinnovo e avvisi anche per `Pers_Licenza`.

## 4. Codice univoco

Ogni personalizzazione ha due identificativi:

- `Id` numerico, progressivo, **mai riusato**: entra nei nomi degli schemi;
- `Codice` leggibile: lettere maiuscole e cifre, inizia con una lettera, da 3 a 12 caratteri (esempio `ACME01`). Lo assegna il super user all'accettazione e **non cambia piu'**.

Dove si usa:

| Elemento | Forma | Esempio |
|---|---|---|
| Schema del database | `tenant_<IdCompany>_p<Id>` | `tenant_42_p7` |
| Tabelle | prefisso `P<Codice>_` | `PACME01_Commessa` |
| Cartelle e namespace | `Custom/<Codice>` | `Entities/Custom/ACME01` |
| DataContext | `Custom<Codice>DbContext` | `CustomACME01DbContext` |
| API | `api/custom/<codice>/[controller]` | `api/custom/acme01/Commessa` |
| Route client | `custom/<codice>/...` | `/custom/acme01/commesse` |
| Comandi chat/vocali | `P<Codice>_<Nome>` | `PACME01_NuovaCommessa` |
| Strumenti MCP | `p<codice>_<nome>` | `pacme01_nuova_commessa` |
| Testi | chiavi con prefisso `P<Codice>_` | `PACME01_Commessa_Titolo` |

Il prefisso sulle tabelle serve perche' `DatabaseInitializer.CheckTableNames` pretende nomi univoci tra tutti i contesti e perche' in modalita' singola tutto finisce in `public`.

## 5. Database e DataContext

### 5.1 Scelta: un DataContext per personalizzazione

| Alternativa | Giudizio |
|---|---|
| **A. Un contesto per personalizzazione** (`Custom<Codice>DbContext : ApplicationDbContextBase`), schema proprio, migrazioni proprie | **Proposta.** E' lo stesso schema usato per gli applicativi. I contesti standard non cambiano e non conoscono le tabelle dei clienti |
| B. Modello dinamico nei contesti esistenti (`IModelCacheKeyFactory` per cliente) | Scartata: un modello in cache per ogni combinazione, migrazioni diverse per cliente, rischio diretto sul codice standard |
| C. Colonne aggiunte alle tabelle standard solo per alcuni clienti | Scartata: le tabelle standard non si alterano mai (vedi §5.3) |

Con la scelta A la risposta al punto "il DataContext deve conoscere piu' tabelle della versione standard" e': **i contesti standard restano identici per tutti**; le tabelle in piu' stanno in un contesto separato, che esiste nel codice per tutti ma viene usato (e ha uno schema) solo per chi ha la personalizzazione attiva.

### 5.2 Impatti sul codice di tenancy

Oggi tutto e' indicizzato per `ApplicationType` (enum). Una personalizzazione non e' un applicativo, quindi serve una chiave piu' generale.

| File | Modifica |
|---|---|
| `ApplicationDbContextBase.cs` | Oltre ad `Application`, proprieta' virtuale `CustomizationId` (null per gli applicativi). I contesti custom dichiarano `Application = ApplicationType.Custom` (nuovo valore, es. 1000, mai usato nei nomi di schema) |
| `TenantSchemaName.cs` | Nuovo `ForCustomization(idCompany, idPers)` → `tenant_<id>_p<n>`; la regex `_tenant` (`^tenant_([0-9]+)_[0-9]+$`) va estesa a `p[0-9]+`, altrimenti `CommonFor` non riconosce lo schema |
| `TenantSchemaAccessor.cs` | Per i contesti custom verifica `ICustomizationGuard` invece di `CompanyApplication`; se non attiva lancia un'eccezione dedicata (`CustomizationNotActiveException`) |
| `TenantSearchPathInterceptor.cs` | `search_path` = schema custom, schemi degli applicativi dichiarati in `Dipendenze`, Common, public |
| `ApplicationDbContextFactory.cs` (registro) | Registrazione anche per personalizzazione: `AddCustomizationDbContext<T>(codice)` |
| `TenantProvisioningService.cs` | `MigrateAllAsync`: dopo gli applicativi, ciclo sulle licenze di personalizzazione attive (per lo studio: tutte le sue aziende). Nuovo `EnsureCustomizationAsync` chiamato all'attivazione |
| `DataLayerInstaller.cs` | Tabella storico `__EFMigrationsHistory_P<Codice>` |
| `DesignTimeDbContextFactories.cs` | Una factory per contesto custom |
| `DatabaseInitializer.cs` | `CheckTableNames` include i contesti custom e verifica il prefisso `P<Codice>_` |

Regole per i contesti custom (le stesse dei moduli verso Common):

- le tabelle proprie sono mappate senza schema;
- le tabelle standard lette dalla personalizzazione (Common o applicativi dichiarati) sono mappate senza schema e con `ExcludeFromMigrations()`, in **sola lettura**;
- le scritture sui dati standard passano dai servizi standard, mai dal contesto custom;
- una personalizzazione non referenzia mai un'altra personalizzazione.

Quando si crea lo schema:

- **all'attivazione** (pagamenti completati e stato Rilasciata): `EnsureCustomizationAsync` crea lo schema e applica le migrazioni, per l'azienda o per ogni azienda dello studio;
- **a ogni avvio**: il ciclo di `MigrateAllAsync` allinea gli schemi esistenti;
- **nuova azienda in uno studio con personalizzazione**: lo schema si crea alla creazione dell'azienda (`ICompanyInitializer`);
- **modalita' singola** (tutto in `public`): le tabelle custom si creano in `public` solo se esiste almeno una licenza attiva; da verificare in prova (PERS-DB.8).

### 5.3 Campi e dati aggiunti alle tabelle standard

Le tabelle standard **non si modificano**. Due strade:

1. **Tabella compagna** nello schema custom, relazione 1:1 con la riga standard (es. `PACME01_ClienteExt` con `IdCliente`). Riferimento **senza vincolo di chiave esterna tra schemi**: un vincolo bloccherebbe l'eliminazione dello schema dell'applicativo alla disdetta. Le righe orfane si puliscono con un `IApplicationDataDeleter` della personalizzazione.
2. **Campi extra senza codice** (fase successiva, D7): tabella generica in Common con definizione dei campi e valori (`jsonb`), per i casi semplici "aggiungi due campi a questa scheda" senza rilascio.

### 5.4 Eliminazione e spazio

- Cancellazione dell'azienda: un `IApplicationDataDeleter` elimina anche gli schemi `tenant_<id>_p*`.
- Spazio occupato: `IStorageUsageProvider` della personalizzazione, se salva file.

## 6. Posizionamento delle classi

### 6.1 Server

| Cosa | Dove |
|---|---|
| Entita' | `nvxapp.server.data/Entities/Custom/<Codice>/` |
| DataContext | `nvxapp.server.data/Infrastructure/Custom/Custom<Codice>DbContext.cs` |
| Migrazioni | `nvxapp.server.data/Migrations/Custom/<Codice>/` |
| Repository | `nvxapp.server.data/Repositories/Custom/<Codice>/` |
| Mapper (AutoMapper) | `nvxapp.server.data/Mappers/Custom/<Codice>/` |
| Testi | `nvxapp.server.data/Localization/Custom/<Codice>Messages(.en).resx` |
| Servizi e modelli | `nvxapp.server.service/ClientServer-Service/Custom/<Codice>/<X>Service/` |
| Comandi chat/vocali | `nvxapp.server.service/ClientServer-Service/Custom/<Codice>/ChatAiHandler/` |
| Descrittore | `nvxapp.server.service/ClientServer-Service/Custom/<Codice>/<Codice>Descriptor.cs` |
| Espositori (controller) | `nvxapp.server/Controllers/Custom/<Codice>/` |
| Registrazione contesti | `nvxapp.server/Utility/Installers4Custom.cs` (stub vuoto in Infrastructure) |

Le convenzioni esistenti coprono gia' queste cartelle: i servizi sono registrati per scansione dell'assembly, i repository per namespace `nvxapp.server.data.Repositories*`, i mapper su tutti gli assembly. Nessuna modifica alla registrazione.

**Descrittore** (`ICustomizationDescriptor`, uno per personalizzazione): dichiara `Codice`, `Dipendenze`, voci di menu, eventuali comandi sostituiti (§8). E' il punto unico da cui l'infrastruttura conosce la personalizzazione.

**Protezione lato server**:

- attributo/filtro `[RequiresCustomization("<Codice>")]` obbligatorio su ogni controller custom (controllo all'avvio: un controller sotto `Controllers/Custom/` senza attributo blocca la partenza);
- seconda barriera: `TenantSchemaAccessor` rifiuta il contesto custom se la personalizzazione non e' attiva.

**Attenzione alle interfacce a scansione** (`IDigestProvider`, `ICalendarEventProvider`, `INotificationCategoryProvider`, `IUserManualProvider`, ...): una classe custom che le implementa oggi girerebbe **per tutti i clienti**. Serve un'interfaccia marcatrice `ICustomizationScoped { string Customization }` e il filtro nei punti che consumano quegli elenchi.

### 6.2 Client

| Cosa | Dove |
|---|---|
| Servizi HTTP e modelli | `nvxapp.client/src/nvx/custom/<codice>/ClientServer-Service/` |
| Pagine (form) | `nvxapp.client/src/nvx/custom/<codice>/pages/` |
| Sezioni per le pagine esistenti | `nvxapp.client/src/nvx/custom/<codice>/extensions/` |
| Testi | `nvxapp.client/src/nvx/custom/<codice>/i18n/` |
| Aggregatori (stub vuoti in Infrastructure) | `Utility/infrastructure/route-custom.service.ts`, `extension-custom.service.ts`, `i18n-custom.service.ts`, `widget-custom.service.ts` |

Ogni personalizzazione e' un modulo Angular caricato solo quando serve (`loadChildren`): il suo codice non viene scaricato dagli altri clienti.

### 6.3 Branch

Una personalizzazione puo' appoggiarsi a uno o piu' applicativi (es. una sezione in piu' nella scheda cliente del CRM), quindi il suo codice deve stare dove quegli applicativi esistono. Proposta (D4):

- il **meccanismo** (richieste, licenze, guard, punti di estensione, filtro dei comandi) va in `Infrastructure_Dev`;
- i **punti di estensione nelle pagine dei moduli** vanno nel branch del modulo, generici e senza riferimenti al cliente;
- il **codice dei clienti** va in un nuovo gruppo `Customizations_Dev` / `Customizations`, che riceve i merge dall'Applicativo completo e contiene tutte le cartelle `Custom/<Codice>`. `CLAUDE.md` va aggiornato con il nuovo gruppo.

Conseguenze da accettare: il codice di tutti i clienti viaggia nello stesso rilascio (spento per chi non lo ha attivo); rilasciare una personalizzazione significa rilasciare l'applicativo.

## 7. Menu, form e pagine esistenti

### 7.1 Voci di menu riservate

- Le voci di una personalizzazione **arrivano dal server** in `UserLoad` (`CustomMenu[]`: etichetta, url, icona, sezione, ruoli), prese dal descrittore e solo per le personalizzazioni attive dell'utente. Nel codice del client degli altri clienti non compare nulla.
- Il client le aggiunge al menu in `main-menu.service.ts`, in una sezione dedicata o nella sezione di un applicativo.
- Restano valide le restrizioni di menu dell'azienda (`CompanyMenuRestriction`).

### 7.2 Nuovi form

- Route `custom/<codice>/...` registrate da `route-custom.service.ts`, con una nuova guardia `customizationGuard('<Codice>')` (accanto a `applicationGuard`) e `menuRestrictionGuard`.
- Pagine costruite con i modelli standard (`Template_Elenco_1`, `Template_Modifica_1`).
- `auth.service.ts`: nuovo `hasCustomization(codice)`.

### 7.3 Integrazione nelle pagine esistenti (punti di estensione)

Stesso principio dei widget del cruscotto (`dashboard-page.component.html:46`), applicato alle pagine: la pagina comune dichiara dei **punti di aggancio** vuoti, la personalizzazione dichiara "ho una sezione per quel punto". A pagina aperta, il punto di aggancio chiede al registro quali sezioni esistono per l'utente collegato e le disegna. **Senza personalizzazioni non disegna nulla** e la pagina resta identica a oggi.

La pagina comune va toccata **una sola volta**, con poche righe che non nominano nessun cliente.

#### 7.3.1 Componenti del meccanismo

- **Punto di aggancio** nella pagina comune: `<nvx-extension-slot slot="..." [contesto]="...">` (e `<nvx-extension-tabs>` per le linguette).
- **Registro** (`ExtensionSlotService`): raccoglie le sezioni dichiarate dalle personalizzazioni (`{ slot, customization, chiave, etichetta, ordine, ruoli, carica }`) e le filtra con `hasCustomization`.
- **Sezione**: componente nella cartella della personalizzazione, caricato solo quando serve (il suo codice non viene scaricato dagli altri clienti). Implementa un contratto semplice: `carica(contesto)`, `valida()`, `salva(contesto)`.
- **Dati**: la sezione legge e scrive con le **proprie API** (`api/custom/<codice>/...`), nelle **proprie tabelle** dello schema del cliente, usando l'Id del record standard. Il codice standard non sa che la sezione esiste e le tabelle standard non vengono toccate (§5.3).

#### 7.3.2 Esempio: sezione "Commesse" nella scheda cliente del CRM per il cliente ACME01

La scheda cliente (`pages/Crm/crm-customer-edit-page`) oggi ha le linguette Dati, Contatti, Opportunita', Attivita', Offerte, Assistenza, Note, Allegati.

1. **Nella pagina comune** (branch CRM, una volta sola) si aggiungono due agganci:

```html
<!-- tra le linguette -->
<nvx-extension-tabs slot="crm.cliente.sezioni" [(selezionata)]="sezione"></nvx-extension-tabs>

<!-- nel corpo della pagina -->
<nvx-extension-slot slot="crm.cliente.sezioni" [selezionata]="sezione"
                    [contesto]="{ id: id, modello: _editModel }"></nvx-extension-slot>
```

2. **Nella cartella della personalizzazione** (`custom/acme01/extensions/`) ci sono il componente della sezione e la sua dichiarazione:

```ts
{ slot: 'crm.cliente.sezioni', customization: 'ACME01',
  chiave: 'acme01.commesse', etichetta: 'Commesse', ordine: 100,
  carica: () => import('./commesse-section.component') }
```

3. **Risultato**: l'utente di ACME01 vede una nona linguetta "Commesse"; tutti gli altri vedono le otto di sempre.

#### 7.3.3 Due tipi di sezione

| Tipo | Come appare | Complessita' |
|---|---|---|
| **Linguetta o blocco autonomo** (elenco commesse, documenti, storico) | Nuova linguetta, o un riquadro in fondo alla pagina | Bassa: la sezione vive per conto suo, con i propri pulsanti |
| **Campi aggiuntivi dentro la scheda** (es. "Codice gestionale", "Categoria interna" sotto i dati fiscali) | Campi in piu' nel form, salvati con il pulsante Conferma della pagina | Media: richiede l'aggancio al salvataggio (§7.3.4) |

#### 7.3.4 Salvataggio dei campi aggiuntivi

Per salvare tutto con un solo Conferma si modifica una volta la base comune (`BasePageConfirmCancelComponent`, `ButtonConfirmClickEv`):

1. alla pressione di Conferma valida anche le sezioni presenti (se una non e' valida, non salva);
2. salva i dati standard come oggi (`SaveData`);
3. chiede a ogni sezione di salvare i propri dati, passando l'Id del record;
4. solo alla fine torna indietro (`AfterSave`).

Se non ci sono sezioni, i passi 1 e 3 non fanno nulla. `SaveData` oggi restituisce solo l'esito: le pagine con agganci devono rendere disponibile l'Id del record salvato (la scheda cliente lo fa gia', valorizzando `this.id`).

#### 7.3.5 Limiti

- **Due salvataggi, non una transazione unica.** Se il salvataggio standard riesce e quello della sezione fallisce, i dati standard restano salvati e la pagina mostra l'errore senza chiudersi.
- **Record nuovo.** L'Id esiste solo dopo il primo salvataggio. I campi aggiuntivi vengono salvati al passo 3 con l'Id appena creato; le linguette con elenchi restano disattivate finche' il record non e' salvato, come gia' fa la scheda cliente con le sue.
- **Gli agganci vanno messi pagina per pagina.** Una sezione si puo' aggiungere solo dove la pagina comune ha un punto di aggancio. Si aggiungono quando una personalizzazione li richiede, nel branch del modulo.
- **La sezione puo' aggiungere, non modificare.** Non puo' nascondere, spostare o cambiare il comportamento dei campi standard. Se un cliente chiede questo, e' una modifica alla pagina comune e va trattata come tale.
- **Elenchi, ricerche, esportazioni e stampe.** I campi aggiuntivi non vi compaiono da soli: servono agganci dedicati (colonne aggiuntive, pulsanti aggiuntivi nella barra), previsti come fase successiva.

## 8. Comandi chat e vocali

### 8.1 Come si aggiunge un comando di un cliente

- Classe `ICommandHandler` nella cartella della personalizzazione; nome obbligatorio `P<Codice>_<Nome>`.
- Nuova proprieta' `IntentDefinition.Customization` (null per i comandi standard).
- Nuovo filtro `IntentCatalog.Filter4Customizations(codiciAttivi)`, chiamato in `ChatAIService` accanto a `Filter4Applications` (:162). `IChatUserContext` espone i codici attivi.
- Lo stesso filtro va applicato in `McpToolService` (:111) e in `VoiceCommandsUserManualProvider` (:134), che costruiscono elenchi propri.
- Il motore non usa "tool" del modello: il catalogo filtrato finisce nel prompt. Basta quindi che il comando sia nel catalogo di quella richiesta; gli altri clienti non lo vedono e non pagano prompt piu' lunghi.

### 8.2 Conflitti

| Conflitto | Oggi | Regola proposta |
|---|---|---|
| Stesso nome (`Name`) | La chat fallisce per tutti | Prefisso obbligatorio `P<Codice>_` + controllo all'avvio. `CommandRegistry` costruito **dopo** i filtri e senza `ToDictionary`: un duplicato viene scritto nel log e vince lo standard |
| Stessa etichetta (`DisplayName`, usata dai pulsanti rapidi) | Vince il primo, in ordine non garantito | Univocita' verificata all'avvio nell'insieme "standard + quella personalizzazione" |
| Parole chiave o frasi d'esempio sovrapposte | Decide il modello linguistico | Di base **vince lo standard**: i comandi custom sono indicati nel prompt come specifici del cliente e da scegliere solo se la frase li richiama chiaramente |
| Il cliente vuole che una frase standard faccia altro | Non previsto | Sostituzione esplicita: il descrittore dichiara `Sostituisce = "CrmAgenda"`; per quel cliente il comando standard esce dal catalogo |
| Parole di controllo (conferma, annulla, invia, aiuto: `ChatAIKeywords` e parole d'invio vocale del client) | Nessun controllo | **Riservate**: un comando custom non puo' usarle come parole chiave (controllo all'avvio) |
| Nome dello strumento MCP | Il secondo riceve un prefisso, i successivi vengono scartati | Prefisso `p<codice>_` sempre |
| Rotte vocali (`RouteRegistry`: vince il primo) | Nessun controllo | Rotte custom solo sotto `custom/<codice>/`, filtrate per personalizzazione |
| Due personalizzazioni attive sullo stesso cliente | — | I prefissi evitano i conflitti di nome; le sovrapposizioni di frasi sono verificate dal controllo all'avvio tra le due |

Il **controllo all'avvio** (`CommandConflictValidator`) verifica nomi, etichette, parole riservate e sostituzioni dichiarate; in sviluppo e prova blocca la partenza con un messaggio chiaro, in produzione scrive nel log ed esclude il comando custom in errore. Lo stesso controllo va messo in una prova automatica.

## 9. Decisioni da confermare

| # | Decisione | Proposta |
|---|---|---|
| D1 | Cosa vuol dire "licenza Pro" per vedere il menu | Almeno un abbonamento attivo di livello Pro (proprio, dello studio, o Cumulativo) |
| D2 | Richiesta: tabelle dedicate o tipo di ticket | Tabelle dedicate `Pers_*`, riusando regole allegati e notifiche dei ticket |
| D3 | Stessa personalizzazione vendibile a piu' clienti | Si': definizione e licenza separate |
| D4 | Dove sta il codice dei clienti | Nuovo gruppo `Customizations_Dev` / `Customizations` alimentato dall'Applicativo completo |
| D5 | Quando si paga l'una tantum | All'accettazione dell'offerta (prima dello sviluppo); il canone parte dal rilascio |
| D6 | Canone scaduto: sorte dei dati | Funzioni spente, dati conservati 90 giorni, poi eliminazione dello schema previo avviso |
| D7 | Campi extra senza codice | Fase successiva, non nella prima versione |
| D8 | Dealer: vede la richiesta? Ha una quota sui pagamenti? | Vede in sola lettura; quota come per le licenze (stessi rendiconti) |
| D9 | Personalizzazione dello studio: i dati sono per azienda o anche dello studio | Per azienda (uno schema per ogni azienda dello studio); nessuno schema "di studio" nella prima versione |
| D10 | Chi puo' aprire la richiesta | `CompanyPowerAdmin` e `FinancialAdvisorPowerAdmin` |

## 10. Rischi

- **Crescita del rilascio unico**: ogni cliente aggiunge codice, migrazioni e prove allo stesso applicativo. Se le personalizzazioni diventano molte va valutato il passaggio ad assembly separati caricati all'avvio (oggi non esiste alcun caricamento di plugin).
- **Aggiornamenti dello standard**: una personalizzazione che legge tabelle standard si rompe se quelle cambiano. Servono prove automatiche per personalizzazione nella catena di rilascio.
- **Riservatezza**: il codice server di tutti i clienti e' nello stesso binario; quello client e' in pacchetti separati caricati solo da chi ne ha diritto. Le voci di menu arrivano dal server proprio per non lasciare tracce nel client comune.
- **Licenze spente**: senza la regola del §3.4 tutte le personalizzazioni diventerebbero visibili a tutti.
- **Interfacce a scansione**: senza il filtro del §6.1 un'estensione custom girerebbe per tutti.

## 11. Checklist

Legenda: ⏳ da fare · 🔄 in corso · ✅ fatto · 🔍 da decidere

#### `PERS-D` – Decisioni
- 🔍 `PERS-D.1` Conferma delle decisioni D1–D10 (§9)

#### `PERS-AMM` – Gestione amministrativa (Infrastructure)
- ⏳ `PERS-AMM.1` `ILicenseGuard.HasAnyProAsync`, `CanRequestCustomization` in `UserLoad`, voce di menu "Personalizzazioni"
- ⏳ `PERS-AMM.2` Tabelle `Pers_Richiesta`, `Pers_RichiestaMessaggio`, `Pers_RichiestaAllegato`, repository, migrazione Public
- ⏳ `PERS-AMM.3` Servizio e controller delle richieste: apertura, messaggi, allegati, stati, offerta, accettazione/rifiuto
- ⏳ `PERS-AMM.4` Notifiche in tempo reale e categoria di notifica, testi (it/en)
- ⏳ `PERS-AMM.5` Pagine client lato cliente: elenco, nuova richiesta, dettaglio con conversazione e allegati
- ⏳ `PERS-AMM.6` Pagine client lato super user: elenco, dettaglio, offerta, cambio stato, rilascio
- ⏳ `PERS-AMM.7` Tabelle `Pers_Personalizzazione` e `Pers_Licenza`, generazione all'accettazione, assegnazione del codice
- ⏳ `PERS-AMM.8` `Lic_Pagamento`: campi `Tipo` e `IdPersLicenza`, `AmbitoLicenza.Personalizzazione`, pagamento una tantum
- ⏳ `PERS-AMM.9` Pagamento del canone, rinnovo, scadenza e avvisi in `LicenseMaintenanceService`
- ⏳ `PERS-AMM.10` Rendiconti dei dealer con i pagamenti delle personalizzazioni (secondo D8)
- ⏳ `PERS-AMM.11` `ICustomizationGuard` (regole del §3.4, licenze spente, studio → aziende), `Customizations` in `UserLoad`, aggiornamento sul client
- ⏳ `PERS-AMM.12` Pagina "Le mie personalizzazioni": stato, pagamenti, scadenze

#### `PERS-DB` – Database e DataContext (Infrastructure)
- ⏳ `PERS-DB.1` `ApplicationType.Custom`, `CustomizationId` in `ApplicationDbContextBase`
- ⏳ `PERS-DB.2` `TenantSchemaName.ForCustomization` e regex estesa
- ⏳ `PERS-DB.3` `TenantSchemaAccessor` e `TenantSearchPathInterceptor` per i contesti custom (dipendenze nel `search_path`)
- ⏳ `PERS-DB.4` Registro dei contesti e `AddCustomizationDbContext`, storico migrazioni per personalizzazione
- ⏳ `PERS-DB.5` `TenantProvisioningService`: ciclo sulle personalizzazioni attive, `EnsureCustomizationAsync`, aziende dello studio, nuova azienda
- ⏳ `PERS-DB.6` `CheckTableNames` con prefisso `P<Codice>_`
- ⏳ `PERS-DB.7` Eliminazione degli schemi custom (cancellazione azienda, conservazione D6)
- ⏳ `PERS-DB.8` Prova in modalita' singola e multi su DB usa e getta

#### `PERS-COD` – Struttura del codice dei clienti
- ⏳ `PERS-COD.1` `ICustomizationDescriptor`, `Installers4Custom` (stub), cartelle `Custom/` nei tre progetti server
- ⏳ `PERS-COD.2` `[RequiresCustomization]` e controllo all'avvio sui controller custom
- ⏳ `PERS-COD.3` `ICustomizationScoped` e filtro nei consumatori delle interfacce a scansione
- ⏳ `PERS-COD.4` Client: stub `route-custom`, `extension-custom`, `i18n-custom`, `widget-custom`; `customizationGuard`; `hasCustomization`
- ⏳ `PERS-COD.5` Voci di menu dal server (`CustomMenu` in `UserLoad`) e composizione in `main-menu.service.ts`
- ⏳ `PERS-COD.6` Gruppo `Customizations_Dev` / `Customizations` e aggiornamento di `CLAUDE.md` (secondo D4)
- ⏳ `PERS-COD.7` Guida "come si crea una personalizzazione" (aggiornando anche `GENERAZIONE_TABELLE_GUIDA_COMPLETA.md`, oggi non allineata ai contesti per applicativo)

#### `PERS-EXT` – Integrazione nelle pagine esistenti
- ⏳ `PERS-EXT.1` `nvx-extension-slot`, `nvx-extension-tabs`, `ExtensionSlotService`, contratto della sezione (§7.3.1)
- ⏳ `PERS-EXT.2` Aggancio al salvataggio in `BasePageConfirmCancelComponent`: valida, salva standard, salva sezioni, esito (§7.3.4); Id del record disponibile dopo il salvataggio
- ⏳ `PERS-EXT.3` Primo aggancio in una pagina standard, come riferimento: linguetta aggiuntiva nella scheda cliente del CRM (§7.3.2), nel branch CRM
- ⏳ `PERS-EXT.4` Campi aggiuntivi dentro una scheda, salvati con Conferma (§7.3.3), compreso il caso del record nuovo
- ⏳ `PERS-EXT.6` Pulsanti aggiuntivi nella barra e colonne aggiuntive in elenchi, ricerche, esportazioni (fase successiva)
- 🔍 `PERS-EXT.5` Campi extra senza codice (D7)

#### `PERS-CMD` – Comandi chat e vocali
- ⏳ `PERS-CMD.1` `IntentDefinition.Customization`, `Filter4Customizations`, codici attivi in `IChatUserContext`
- ⏳ `PERS-CMD.2` `CommandRegistry` costruito dopo i filtri, senza `ToDictionary`
- ⏳ `PERS-CMD.3` Stesso filtro in `McpToolService` e `VoiceCommandsUserManualProvider`; prefisso MCP
- ⏳ `PERS-CMD.4` Sostituzione dichiarata di un comando standard
- ⏳ `PERS-CMD.5` `CommandConflictValidator` all'avvio e prova automatica
- ⏳ `PERS-CMD.6` Rotte vocali custom filtrate

#### `PERS-PILOTA` – Personalizzazione di prova
- ⏳ `PERS-PILOTA.1` Personalizzazione fittizia completa: una tabella, un form, una voce di menu, una sezione in una pagina esistente, un comando
- ⏳ `PERS-PILOTA.2` Prova dell'intero giro: richiesta → offerta → pagamenti → rilascio → attivazione → scadenza, per azienda e per studio
- ⏳ `PERS-PILOTA.3` Verifica che un altro cliente non veda nulla: menu, route, API, comandi, MCP, manuale

#### Merge
- ⏳ `PERS-M` Catena dei merge: Infrastructure → moduli → Applicativo completo
