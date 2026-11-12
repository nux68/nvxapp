# Analisi – Applicativo Risorse aziendali (Assets)

Branch: **Assets_Dev** (riceve i merge da Infrastructure, si mergia in Assets).
Documento generale: `ANALISI_INFRASTRUCTURE_NUOVI_MODULI.md` (§4.3, aree `AST-*` in §10.2), punto `ANA.9`.

Stato (11/11/2026): **modulo base (`AST-BASE`) e aree funzionali `AST-FLO`, `AST-BENI`, `AST-PREN` fatti** (ramo di lavoro `Assets_Dev_f3`, provati su DB usa e getta, da mergiare); dettagli in §7.1, scelte provvisorie in §8.

---

## 1. Obiettivo

**Cosa possiede l'azienda e a chi lo assegna** (ufficio acquisti, logistica, IT):
- **Flotta** (`AST-FLO`): veicoli, scadenze (bollo, assicurazione, revisione, tagliando), assegnazione ai dipendenti, fringe benefit, multe e costi. L'assegnazione dell'auto al dipendente è `Dip_RapportoAutovetture`, spostata in Common (D5): Assets la gestisce, le Presenze continuano a leggerla per l'export paghe.
- **Beni assegnati** (`AST-BENI`): PC, telefoni, badge, attrezzature, con consegna e restituzione (verbale tramite presa visione/firma) e l'elenco dei beni da restituire alla cessazione.
- **Prenotazioni** (`AST-PREN`): postazioni, sale e auto in pool, con calendario, controllo dei conflitti e sincronizzazione facoltativa con il calendario personale (`INF-CAL`). Approvazione facoltativa per risorsa (`AST-RICH`): le richieste si decidono nelle pagine comuni delle richieste di Infrastructure (`ANALISI_INFRASTRUCTURE_RICHIESTE.md`).

Il modulo funziona da solo; HR, Presenze e Documentale sono integrazioni facoltative (§6).

## 2. Applicativo e schema

| Voce | Valore |
|---|---|
| `ApplicationType` | `Assets = 7` |
| Schema | `tenant_<IdAzienda>_7` (in modalità singola `public`) |
| Prefisso tabelle | `Ast_` |
| Contesto | `AssetsDbContext` (migration in `Migrations/Assets`, storico `__EFMigrationsHistory_Assets`) |
| Licenza | `AmbitoLicenza.Assets = 7`, nessun limite (`INF-APP.3`); listini con prezzi segnaposto (§8, D8) |
| Codice del manuale | `assets` (`Manuals/assets.<lingua>.md`) |
| Cartella i18n del client | `assets/i18n/assets/`, chiavi sotto `ast.` |
| Cartelle | server `Controllers/Assets`, `ClientServer-Service/Assets`, `Entities/Tenant/Assets`, `Repositories/Tenant/Assets`; client `ClientServer-Service/Assets`, `pages/Assets` |

## 3. Ruoli e visibilità

| Ruolo | Cosa fa |
|---|---|
| **CompanyPowerAdmin** | Configurazione del modulo e delle risorse prenotabili; tutto il resto |
| **CompanyAdmin** (acquisti, logistica, IT) | Anagrafica di veicoli e beni, assegnazioni, consegne e restituzioni, scadenze, multe e costi; vede tutte le prenotazioni |
| **User** (dipendente) | I beni e il veicolo assegnati a lui (con presa visione dei verbali), le proprie multe; prenota postazioni, sale e auto in pool e gestisce le proprie prenotazioni |
| **Studio** | Nessuna funzione per ora (eventuale lettura dei fringe benefit per le paghe: da decidere, §8) |

I permessi li controlla il server in ogni servizio (`AstUtility.ContestoAsync`), come nel CRM.

## 4. Modello dati proposto (schema `tenant_<id>_7`, prefisso `Ast_`)

Tutte le tabelle hanno `IdAz_Anagrafica` (FK verso `Az_Anagrafica` di Common) e i campi di `BaseEntity`. Dipendenti, sedi e autovetture del dipendente sono in Common (`Dip_Anagrafica`, `Az_Sedi` dopo `INF-COM.1`, `Dip_RapportoAutovetture` dopo `INF-COM.3`), letti in sola lettura nel contesto e scritti con i servizi/repository di Common.

### 4.1 Configurazione (fatta in `AST-BASE`)
- `Ast_Cfg` (una riga per azienda, creata all'attivazione): `GiorniPreavviso`, `NotificheAttive`, `Note`. Le aree aggiungono i propri parametri (es. durata massima di una prenotazione, anticipo massimo).

### 4.2 Flotta (`AST-FLO`)
- `Ast_Veicolo`: targa, marca/modello, alimentazione, immatricolazione, proprietà/noleggio/leasing (contratto, scadenza, km inclusi), sede, uso (assegnato / pool), valore ACI per il fringe benefit, stato (attivo / dismesso).
- Assegnazione al dipendente: **`Dip_RapportoAutovetture` in Common** (D5), con un riferimento facoltativo a `Ast_Veicolo` (da aggiungere in `INF-COM.3` o con una tabella ponte `Ast_VeicoloAssegnazione` se Common non deve conoscere Assets — decisione `AST-D3`).
- `Ast_VeicoloScadenza`: bollo, assicurazione, revisione, tagliando, con data e km; registrate nello scadenzario comune.
- `Ast_VeicoloCosto`: carburante, manutenzione, pedaggi, con importo e documento.
- `Ast_Multa`: data, verbale, importo, conducente al momento (dal periodo di assegnazione), stato (da notificare / notificata al conducente / pagata / contestata).

### 4.3 Beni assegnati (`AST-BENI`)
- `Ast_Categoria`: categorie (PC, telefono, badge, attrezzatura…) con campi propri facoltativi.
- `Ast_Bene`: codice inventario, categoria, descrizione, numero di serie, data di acquisto e valore, garanzia (scadenza), sede, stato (disponibile / assegnato / in riparazione / dismesso).
- `Ast_BeneAssegnazione`: bene, dipendente, data di consegna, data di restituzione, condizioni, verbale di consegna e di restituzione (presa visione).

### 4.4 Prenotazioni (`AST-PREN`)
- `Ast_Risorsa`: risorsa prenotabile (tipo: postazione / sala / auto in pool con riferimento a `Ast_Veicolo`), sede, capienza, attiva, `RichiedeApprovazione` (`AST-RICH`).
- `Ast_Prenotazione`: risorsa, utente, inizio/fine, note, stato (confermata / annullata / in attesa di approvazione / rifiutata); indice per il controllo dei conflitti (sovrapposizione degli intervalli sulla stessa risorsa: occupano la fascia le confermate e le in attesa). Per l'approvazione (`AST-RICH`, variante `RICH-AST.6`): `IdDip_GG_Richiesta`, la richiesta collegata in `Dip_GG_Richiesta` (schema Common `tenant_<id>_0`, tipo `PrenotazioneRisorsa`, `Dati = {"idAst_Prenotazione": n}`), senza navigazione perché la tabella è di un altro schema; decisione, approvatore e motivo del rifiuto stanno nella cronologia della richiesta. Lo stato della prenotazione segue l'esito (serve ai conflitti e alle pagine). Tutto in `InitAssets` (rigenerata, squash).

## 5. Uso dei servizi comuni (Infrastructure)

| Servizio | Dove | Uso in Risorse aziendali |
|---|---|---|
| Scadenzario `IDeadlineRegistry` | `ClientServer-Service/infrastructure/Deadline` | Scadenze dei veicoli (`ast.veicolo`), contratti di noleggio/leasing, garanzie dei beni (`ast.bene`), restituzioni attese alla cessazione (`ast.restituzione`). Preavviso predefinito da `Ast_Cfg.GiorniPreavviso` |
| Notifiche `INotificationSender` | `ClientServer-Service/infrastructure/Notification` | Multa da notificare al conducente, bene da restituire, prenotazione confermata o annullata. Rispettare `Ast_Cfg.NotificheAttive` |
| Presa visione `IAcknowledgementService` | `ClientServer-Service/infrastructure/Acknowledgement` | Verbali di consegna e restituzione di beni e veicoli (`ast.consegna`, `ast.restituzione`). Con il Documentale (`DM-FIRMA.2`) diventano firma FES |
| Dati Common | `Dip_Anagrafica`, `Az_Sedi` (`INF-COM.1`), `Dip_RapportoAutovetture` (`INF-COM.3`), data di cessazione (`INF-COM.4`) | Sola lettura con `Ast_<Entità>ReadRepository`; scritture delle assegnazioni dell'auto con i repository di `Repositories/Tenant/Common` |
| Calendari (`INF-CAL`) | `ICalendarEventProvider` | `AstCalendarEventProvider`: le prenotazioni confermate dell'utente nel suo feed .ics (`AST-PREN.3`) |
| Cancellazione `IApplicationDataDeleter` | `AstDataDeleter` | Conteggi (veicoli, beni, prenotazioni), cancellazione per azienda in modalità singola; utente rimosso: prenotazioni future annullate. Le assegnazioni sono del dipendente (Common), non dell'utente: restano fino alla restituzione |

## 6. Integrazioni facoltative

- **Presenze**: export paghe del fringe benefit da `Dip_RapportoAutovetture` (Common, D5), invariato.
- **Risorse Umane**: all'offboarding (`HR-ONB`) l'elenco dei beni e del veicolo da restituire (`AST-BENI.3`).
- **Documentale**: firma dei verbali di consegna (`DM-FIRMA.2`).
- **Compliance**: i DPI restano in Compliance (proposta `CMP` §6).

## 7. Fatto in `AST-BASE` (11/11/2026)

- Server: `AssetsDbContext` registrato in `Installers4Assets`, design-time factory in un file proprio (`AssetsDbContextDesignTimeFactory.cs`), migration `InitAssets` con `Ast_Cfg`, repository `Ast_CfgRepository`, `Ast_Az_AnagraficaReadRepository`, `AstUtility`, `AstCompanyInitializer` (priorità 70), `AstDataDeleter`, `Ast_ConfigService` + `Ast_ConfigController` (`api/Ast_Config/Ast_CfgGet` amministratori, `Ast_CfgPut` CompanyPowerAdmin), testi `AssetsMessages` (it/en), manuale `assets` (it/en), listini `AstLicenseCatalogSeed`.
- Client: pagina `/astsettings` (`AstSettingsPageComponent`), voce di menu "Risorse aziendali › Impostazioni" (CompanyPowerAdmin, gruppo 70), manuale nella sezione Assistenza, rotta, cartella i18n `assets`.
- Widget: nessuno per ora. Comandi vocali: aggiunti dopo, vedi `ANALISI_ASSETS_VOCALI.md`.
- Prova su DB usa e getta: azienda, attivazione come SuperUser (non bloccata dalle licenze), schema `tenant_<id>_7` con `Ast_Cfg`, Get/Put del CompanyPowerAdmin, Put rifiutata al CompanyAdmin (12 controlli, tutti ok).

### 7.1 Fatto in `AST-FLO`, `AST-BENI`, `AST-PREN` (11/11/2026, ramo `Assets_Dev_f3`)

- **Flotta** (`AST-FLO`, migration `AstFlotta`): `Ast_Veicolo` (targa unica per azienda), `Ast_VeicoloAssegnazione`, `Ast_VeicoloScadenza`, `Ast_VeicoloCosto`, `Ast_Multa`; `Dip_Anagrafica`, `Az_Sedi`, `Dip_RapportoAutovetture` in sola lettura nel contesto (`Ast_*ReadRepository`). Servizio `Ast_FlottaService` / `api/Ast_Flotta/*`.
  - Assegnazione (un dipendente per volta, non sui veicoli in pool): crea o aggiorna l'autovettura del dipendente in Common (`Dip_RapportoAutovettureCommonRepository`: descrizione, targa, rimborso al km) usata dalle note spese delle Presenze; il riferimento sta nella tabella ponte (`IdDip_RapportoAutovetture`). A fine assegnazione l'autovettura resta (note spese già registrate).
  - Fringe benefit informativo: 15.000 km × costo ACI × percentuale (da emissioni CO2 se non indicata: 25/30/50/60%), annuo e quota dell'anno in corso.
  - Scadenzario: `ast.veicolo.scadenza` (chiusa con la data di esecuzione o con il veicolo dismesso) e `ast.veicolo.contratto` (noleggio/leasing). Multa: conducente proposto dal periodo di assegnazione, notifica al conducente (`Ast_MultaNotifica`).
- **Beni** (`AST-BENI`, migration `AstBeni`): `Ast_Categoria` (5 predefinite all'attivazione), `Ast_Bene` (codice di inventario unico), `Ast_BeneAssegnazione` (indice unico filtrato: una sola consegna aperta per bene). Servizio `Ast_BeniService` / `api/Ast_Beni/*`.
  - Consegna e restituzione con presa visione al dipendente (`ast.beni.consegna`, `ast.beni.restituzione`: la fonte contiene "beni"/"consegna", con il documentale attivo diventa il verbale di consegna beni da firmare); dipendente senza utente: solo registrazione. Garanzia nello scadenzario (`ast.bene.garanzia`).
  - `AST-BENI.3`: `AstRestituzioni` trova beni e veicoli ancora assegnati a dipendenti con `DataCessazione` (Common) entro il preavviso: scadenza `ast.restituzione` / `ast.restituzione.veicolo` alla data di cessazione (chiusa alla restituzione o a fine assegnazione) e una notifica agli amministratori; `AstRestituzioniMaintenanceService` (registrato in `Installers4Assets`) la esegue ogni giorno, la pagina la lancia a richiesta.
- **Prenotazioni** (`AST-PREN`, migration `AstPrenotazioni`): `Ast_Risorsa` (postazione, sala con capienza, auto in pool legata a un veicolo con uso "pool"), `Ast_Prenotazione`; `Ast_Cfg` con durata massima (10 h) e anticipo massimo (60 giorni). Servizio `Ast_PrenotazioniService` / `api/Ast_Prenotazioni/*`.
  - Conflitti: `Ast_PrenotazioneRepository.SalvaSeLiberaAsync` (transazione + `pg_advisory_xact_lock` sulla risorsa): vale anche per richieste concorrenti. Gli utenti vedono le prenotazioni altrui solo come "occupato"; gli amministratori vedono i nomi e annullano (con notifica all'interessato).
  - `AstCalendarEventProvider`: prenotazioni dell'utente nel feed .ics (contesto dalla factory, senza utente collegato).
- **Client**: pagine `/astvehicles`, `/astvehicleedit`, `/astvehicleitemedit`, `/astgoods` (beni / da restituire / categorie), `/astgoodedit`, `/astgooditemedit`, `/astbookings`, `/astbookingedit`, `/astresources`, `/astresourceedit`, `/astmine` (le mie risorse: beni e verbali, veicolo, multe); impostazioni con i limiti delle prenotazioni; menu (amministratori: Veicoli, Beni, Prenotazioni, Risorse prenotabili, Le mie risorse, Impostazioni; utenti: Prenotazioni, Le mie risorse), i18n it/en, manuale.
- **Notifiche**: categorie proprie (`AST-SEG.1`, `INF-SVC.7`): `ast.flotta` (multe al conducente), `ast.beni` (beni e veicoli da restituire), `ast.prenotazioni` (prenotazione annullata); rispettano `Ast_Cfg.NotificheAttive`.
- **Prova** su DB usa e getta multi-tenant (60 controlli, tutti ok): azienda, attivazione Assets, CRUD e permessi, autovettura in Common, scadenze nello scadenzario (aperte/chiuse), multa notificata, verbale di consegna creato, restituzioni alla cessazione (scadenze e notifica), conflitti di prenotazione rifiutati anche con 6 richieste concorrenti, prenotazione nel feed .ics dell'utente, conteggi nell'anteprima di cancellazione.

### 7.2 Fatto in `AST-RICH` (12/11/2026, ramo `Assets_Dev_richieste`)

- **Dati** (`InitAssets` rigenerata, squash; il DB si ricrea): `Ast_Risorsa.RichiedeApprovazione`; `AstStatoPrenotazione` + `InAttesa = 2`, `Rifiutata = 3`; `Ast_Prenotazione.IdDip_GG_Richiesta`. Variante `RICH-AST.6` (12/11/2026): la richiesta non sta più nei campi della prenotazione ma in `Dip_GG_Richiesta` (Common), con il ciclo del motore comune `RichiestaCommonService` (approvatori = amministratori dell'azienda, notifiche "richieste", SignalR `RequestNotifier`).
- **Prenotazioni** (`Ast_PrenotazioniService`): su una risorsa con approvazione la prenotazione (o la modifica) di chi non è amministratore nasce in attesa; un amministratore che prenota (o modifica una richiesta) conferma subito. Conflitti (`Ast_PrenotazioneRepository.SalvaSeLiberaAsync`) con confermate e in attesa; l'elenco mostra le in attesa come occupate e, tra le proprie, anche le rifiutate con il motivo; feed .ics solo con le confermate.
- **Creazione e modifica** (`Ast_PrenotazioniService`, `AstApprovazioni`): la prenotazione si salva in attesa (controllo dei conflitti), poi `RichiestaCommonService.CreaAsync`; se la richiesta non si crea (es. nessun amministratore oltre al richiedente) si compensa: la nuova prenotazione si toglie, una modificata torna com'era. Modifica di una prenotazione ancora in attesa da parte del richiedente: si aggiorna il periodo della stessa richiesta (`Data`/`DataA`), così gli amministratori non ricevono una seconda notifica; modifica di una prenotazione confermata: nuova richiesta e annullamento di quella approvata. Annullamento dalla pagina prenotazioni: il richiedente annulla anche la richiesta; un amministratore che annulla la prenotazione in attesa di un altro la rifiuta.
- **Richieste comuni** (`AstRichiestaTipoHandler : IRichiestaTipoHandler`, tipo `PrenotazioneRisorsa`, strategia Singola): testi delle righe (risorsa, veicolo, titolo, note, stato con il motivo del rifiuto dalla cronologia, url `/astbookings`), con la prenotazione come fonte dello stato mostrato; approva (prima la prenotazione con il ricontrollo dei conflitti, poi il motore; se il motore rifiuta la prenotazione torna in attesa), rifiuta (prenotazione rifiutata, fascia libera), annulla del richiedente.
- **Scadenza** (`AstApprovazioni.ScadiAsync`): le prenotazioni ancora in attesa con inizio passato diventano annullate e la richiesta scade (`RichiestaCommonService.ScadiAsync`, notifica al richiedente). Si applica in modo pigro all'inizio di elenchi, salvataggi e azioni delle prenotazioni e in `AstRichiestaTipoHandler.StrategiaAsync`, che `RequestService` chiama a ogni conteggio ed elenco delle richieste (i contatori del menu si ricalcolano a ogni login e a ogni evento delle richieste).
- **Motivo del rifiuto** nelle pagine delle prenotazioni: dalla cronologia della richiesta collegata (`AstApprovazioni.MotiviAsync`).
- **Client**: spunta "Richiede approvazione" nella risorsa, colonna stato nelle prenotazioni, avviso "richiesta inviata" dopo il salvataggio, la pagina di modifica carica da sola risorse e limiti (aperta da "Nuova prenotazione" nelle richieste comuni), widget "Le mie prenotazioni" con le in attesa indicate (le rifiutate no), `request-ui-assets.service.ts`.
- **Comandi vocali**: "prenota..." risponde "richiesta inviata, in attesa di approvazione"; le mie prenotazioni indicano le in attesa; le risorse libere considerano occupate anche le in attesa.

## 8. Decisioni aperte

| # | Decisione | Note |
|---|---|---|
| AST-D1 | Livelli Free/Standard/Pro e prezzi (D8, `ANA.10`) | Oggi listini con la struttura del CRM e **prezzi segnaposto**, nessun limite |
| AST-D2 | Fringe benefit: calcolo in Assets o solo dati per le Presenze | **Scelta provvisoria** (la proposta): Assets tiene costo ACI, percentuale e periodi e mostra il valore a titolo informativo; l'export resta nelle Presenze |
| AST-D3 | Legame tra `Dip_RapportoAutovetture` (Common) e `Ast_Veicolo` | **Scelta provvisoria**: tabella ponte in Assets (`Ast_VeicoloAssegnazione.IdDip_RapportoAutovetture`); l'assegnazione crea/aggiorna l'autovettura in Common, che a fine assegnazione resta |
| AST-D4 | Auto in pool: prenotazione con o senza approvazione | ✅ **Decisa il 12/11/2026** (`AST-RICH`): approvazione facoltativa per risorsa (`Ast_Risorsa.RichiedeApprovazione`); approvano gli amministratori dell'azienda (CompanyPowerAdmin, CompanyAdmin), ne basta uno; le prenotazioni degli amministratori sono confermate subito; il ciclo passa dalle richieste comuni di Infrastructure (`/requests`, `/requestsadmin`, `ANALISI_INFRASTRUCTURE_RICHIESTE.md`); una richiesta non decisa all'ora di inizio viene annullata con notifica. Restano durata massima e anticipo massimo in `Ast_Cfg` |
| AST-D5 | Inventario con codici a barre/QR | Utile con `AT-TIMB.1` (QR); da valutare |
| AST-D6 | Cartella i18n e `i18n-folders.ts` | Come per HR: riga aggiunta in `i18n-folders.ts` (file di Infrastructure), conflitto banale nel merge di AllFunction, oppure stub `i18n-assets.ts` in Infrastructure |

## 9. Checklist

Legenda: ✅ svolto · ⏳ da fare · 🔄 in corso · 🔍 da decidere

#### `AST-BASE` – Avvio del modulo
- ✅ `AST-BASE.1` `AssetsDbContext` registrato, migration `InitAssets` con `Ast_Cfg`, repository, inizializzatore e `AstDataDeleter`
- ✅ `AST-BASE.2` Cartelle del modulo, servizio/controller della configurazione, pagina `/astsettings`, ruoli (§3)
- ✅ `AST-BASE.3` Menu, rotta, manuale e i18n; widget: nessuno per ora; comandi vocali in `ANALISI_ASSETS_VOCALI.md`

#### `AST-FLO` – Flotta
- ✅ `AST-FLO.1` Veicoli, assegnazione e fringe benefit (su `Dip_RapportoAutovetture` in Common, D5)
- ✅ `AST-FLO.2` Scadenze (bollo, assicurazione, revisione, tagliando) nello scadenzario
- ✅ `AST-FLO.3` Multe e costi

#### `AST-BENI` – Beni assegnati
- ✅ `AST-BENI.1` Anagrafica dei beni (PC, telefoni, badge, attrezzature)
- ✅ `AST-BENI.2` Consegna e restituzione con verbale (`INF-SVC.6`)
- ✅ `AST-BENI.3` Elenco dei beni da restituire alla cessazione (data in Common)

#### `AST-PREN` – Prenotazioni
- ✅ `AST-PREN.1` Risorse prenotabili (postazioni, sale, auto in pool)
- ✅ `AST-PREN.2` Calendario delle prenotazioni e conflitti
- ✅ `AST-PREN.3` Sincronizzazione con il calendario personale (`INF-CAL`, facoltativa)

#### Seguiti
- ✅ `AST-SEG.1` Categorie di notifica proprie (`INF-SVC.7`) — fatta, da mergiare: `AstNotificationCategoryProvider` con `ast.flotta` (email predefinita), `ast.beni`, `ast.prenotazioni` al posto di `generale` (`AstUtility.NotificaAsync` riceve la categoria), titoli nei testi del modulo, visibili nelle preferenze solo con Risorse aziendali attivo; le scadenze restano `scadenze`, i verbali `presevisione`. Compilata (server e client); provata sul ramo HR la parte comune

#### `AST-RICH` – Approvazione delle prenotazioni (richieste comuni)
- ✅ `AST-RICH` Risorse con approvazione, prenotazioni in attesa/rifiutate, richiesta collegata in `Dip_GG_Richiesta` (Common) con `AstRichiestaTipoHandler` e il motore `RichiestaCommonService`, scadenza, client e comandi vocali (§7.2; variante `RICH-AST.6`, `InitAssets` rigenerata) — fatta il 12/11/2026 su `Assets_Dev_richieste`, da mergiare

#### Decisioni
- 🔍 `AST-D1`, `AST-D5`, `AST-D6` (§8)
- 🔍 `AST-D2`, `AST-D3`: scelte provvisorie applicate in `AST-FLO`/`AST-PREN`, da confermare
- ✅ `AST-D4` decisa il 12/11/2026: approvazione facoltativa per risorsa (`AST-RICH`)
