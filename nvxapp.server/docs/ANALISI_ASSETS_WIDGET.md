# Widget della dashboard — Risorse aziendali

Dettaglio del ramo **Assets**. Il funzionamento generale (catalogo per ramo, configurazione per utente, tabella
`UserDashboard`, stili comuni) è in `ANALISI_INFRASTRUCTURE_WIDGET.md` (Infrastructure). Il modulo è descritto in
`ANALISI_ASSETS_APPLICATIVO.md`.

## 1. Widget

| Chiave | Titolo | Dimensione | Ruoli | Predefinito | Dati |
|---|---|---|---|---|---|
| `assets.myBookings` | Le mie prenotazioni | small (1/3) | CompanyPowerAdmin, CompanyAdmin, User | sì | `Ast_Prenotazioni/Ast_PrenotazioneList` (esistente, `SoloMie`) |
| `assets.mine` | Le mie risorse | small (1/3) | CompanyPowerAdmin, CompanyAdmin, User | no | `Ast_Beni/Ast_MieiBeniGet` + `Ast_Flotta/Ast_MieiVeicoliGet` (esistenti) |
| `assets.fleet` | Scadenze della flotta | small (1/3) | CompanyPowerAdmin, CompanyAdmin | sì | `Ast_Widget/Ast_WidgetFlottaGet` (nuovo) |
| `assets.toReturn` | Da restituire | small (1/3) | CompanyPowerAdmin, CompanyAdmin | no | `Ast_Beni/Ast_DaRestituireList` (esistente) |

- Compaiono solo se **Risorse aziendali è attivo** per l'azienda (applicativo attivo e licenza), raggruppati sotto
  "Risorse aziendali" nella configurazione della dashboard (`widgets.application.7`, già in Infrastructure).
- **Ruoli**: gli stessi delle pagine. Flotta e restituzioni sono degli amministratori dell'azienda (`AstContesto.Amministratore`,
  ricontrollato dal server); prenotazioni e risorse proprie di chiunque lavori nell'azienda.
- **Predefiniti**: "Le mie prenotazioni" per tutti e "Scadenze della flotta" per gli amministratori (il flag è unico per il widget,
  il ruolo filtra). Valgono solo per chi non ha mai configurato la dashboard: chi l'ha già salvata li aggiunge dalle Preferenze.
- Dichiarazione: `Utility/infrastructure/widget-assets.service.ts` (in Infrastructure è vuoto). Zorder nella fascia 700000, come il menu.

## 2. Le mie prenotazioni (`assets.myBookings`)

Indicatori **Oggi** (prenotazioni che iniziano oggi) e **Prossimi 14 giorni**; le prime 4 con risorsa, titolo e orario
(la fine con la sola ora se nello stesso giorno); collegamento **Vai alle prenotazioni** (`/astbookings`).

- Richiesta: `Ast_PrenotazioneList` con `Dal` = adesso, `Al` = fra 15 giorni (mezzanotte), `SoloMie` = true. Il servizio restituisce
  le prenotazioni **confermate** che si sovrappongono al periodo: comprese quelle in corso.
- Il servizio limita il periodo a 62 giorni: i 14 giorni del widget ci stanno.

## 3. Le mie risorse (`assets.mine`)

Indicatori **Beni in carico** e **Veicoli assegnati**, e se ci sono **Verbali da confermare** (arancione, apre le prese visione
`/acknowledgements`); riga "Multe notificate da pagare"; elenco dei primi 4 (prima i veicoli, poi i beni con codice inventario);
collegamento **Le mie risorse** (`/astmine`).

| Dato | Origine |
|---|---|
| Beni in carico | `Ast_MieiBeniGet.Assegnazione` senza `DataRestituzione` |
| Verbali da confermare | stessa regola della pagina: verbale di consegna o restituzione richiesto e senza `DataConferma` |
| Veicoli assegnati | `Ast_MieiVeicoliGet.Veicolo` con assegnazione in corso oggi (il servizio dà anche quelle chiuse nell'anno, per il fringe benefit) |
| Multe da pagare | `Ast_MieiVeicoliGet.Multa` in stato **Notificata** (1) |

Un utente non collegato a un dipendente riceve elenchi vuoti (comportamento dei servizi): il widget mostra "Nessun bene o veicolo in carico".

## 4. Scadenze della flotta (`assets.fleet`)

Indicatori **Scadute** (rosso, verde se zero) e **Entro N giorni** (arancione), con N = `Ast_Cfg.GiorniPreavviso` (lo stesso
preavviso dello scadenzario comune); riga "Multe da notificare ai conducenti"; le prime 6 scadenze (data, targa, tipo), ognuna apre
il veicolo (`/astvehicleedit`); collegamento **Vai ai veicoli** (`/astvehicles`). Senza veicoli attivi: "Nessun veicolo attivo".

### 4.1 Endpoint `Ast_Widget/Ast_WidgetFlottaGet`

| | |
|---|---|
| Servizio | `Ast_WidgetService` (`ClientServer-Service/Assets/Ast_WidgetService.cs`, modelli in `Models/AstWidgetModels.cs`) |
| Controller | `Ast_WidgetController` (`nvxapp.server/Controllers/Assets/AstWidgetController.cs`) |
| Permessi | `IAstUtility.RichiediAmministratore` (CompanyPowerAdmin o CompanyAdmin), altrimenti `NotAllowed` |
| Ingresso | nessuno (`Ast_EmptyInModel`) |
| Uscita | `GiorniPreavviso`, `VeicoliAttivi`, `Scadute`, `InArrivo`, `MulteDaNotificare`, `Scadenze[]` (`IdAst_Veicolo`, `Targa`, `Veicolo`, `Descrizione`, `DataScadenza`, `Scaduta`, `Contratto`) |

Calcolo, sullo schema dell'azienda e sui soli veicoli **attivi**:

- scadenze del veicolo (`Ast_VeicoloScadenza`) **non eseguite** (`DataEsecuzione` null) con data fino a oggi + preavviso:
  `Scaduta` se la data è passata; descrizione come nella flotta (`Ast_FlottaService.DescrizioneScadenza`, ora pubblica);
- contratti di noleggio/leasing (`Ast_Veicolo.ScadenzaContratto`) da oggi a oggi + preavviso, descritti "Contratto di noleggio/leasing"
  (nuovo testo `WidgetContractDeadline` in `AssetsMessages`). Quelli già scaduti non si contano: il contratto non ha una data di
  esecuzione e resterebbe in elenco per sempre;
- `MulteDaNotificare` = multe dell'azienda in stato **Da notificare** (0);
- elenco ordinato per data (prima le scadute), al massimo 6.

## 5. Da restituire (`assets.toReturn`)

Indicatori **Beni** e **Veicoli** ancora assegnati a dipendenti cessati (rosso se ci sono, verde se zero); i primi 4 per data di
cessazione (dipendente e oggetto), ognuno apre il bene (`/astgoodedit`) o il veicolo (`/astvehicleedit`) come la pagina dei beni;
collegamento **Vai ai beni** (`/astgoods`, dove c'è la sezione "Da restituire"). Dati da `Ast_DaRestituireList` (AST-BENI.3), senza
lanciare la verifica: quella gira ogni notte o dalla pagina.

## 6. File

| Livello | File |
|---|---|
| Servizio | `nvxapp.server.service/ClientServer-Service/Assets/Ast_WidgetService.cs`, `Models/AstWidgetModels.cs`; `Ast_FlottaService.DescrizioneScadenza` resa pubblica |
| Controller | `nvxapp.server/Controllers/Assets/AstWidgetController.cs` |
| Testi server | `nvxapp.server.data/Localization/AssetsMessages{,.en}.resx`: `WidgetContractDeadline` |
| Client, API | `ClientServer-Service/Assets/ast.service.ts` (`WidgetFlottaGet`), `Models/ast-widget-model.ts` |
| Client, widget | `shared/components/Assets/widgets/` (`ast-my-bookings-widget`, `ast-mine-widget`, `ast-fleet-widget`, `ast-to-return-widget`, `_ast-widget.scss`) |
| Catalogo | `Utility/infrastructure/widget-assets.service.ts` |
| Traduzioni | `assets/i18n/assets/{it,en}.json`, sezione `ast.widgets` |

Il servizio si registra da solo (scansione dell'assembly dei servizi in `Installers.cs`); nessuna migrazione.

## 7. Sviluppi possibili

- Garanzie dei beni in scadenza (`Ast_Bene.ScadenzaGaranzia`, già nello scadenzario comune) nel widget della flotta o in uno proprio.
- Occupazione delle risorse oggi (sale e postazioni libere/occupate) per chi prenota spesso.
- Costi della flotta del mese (`Ast_VeicoloCosto`) per il CompanyPowerAdmin.

## 8. Checklist

| # | Punto | Stato |
|---|---|---|
| AST-W.1 | Endpoint `Ast_Widget/Ast_WidgetFlottaGet` (servizio, modelli, controller, testo `WidgetContractDeadline` it/en) | ✅ fatta, da mergiare |
| AST-W.2 | Widget `assets.myBookings` (Le mie prenotazioni) | ✅ fatta, da mergiare |
| AST-W.3 | Widget `assets.mine` (Le mie risorse) | ✅ fatta, da mergiare |
| AST-W.4 | Widget `assets.fleet` (Scadenze della flotta) | ✅ fatta, da mergiare |
| AST-W.5 | Widget `assets.toReturn` (Da restituire) | ✅ fatta, da mergiare |
| AST-W.6 | Registrazione nel catalogo `widget-assets.service.ts` (ruoli, predefiniti, zorder 700000) | ✅ fatta, da mergiare |
| AST-W.7 | Traduzioni `ast.widgets` it/en | ✅ fatta, da mergiare |
| AST-W.8 | Build server (`dotnet build`) e client (`ng build`) sul ramo Assets | ✅ fatta |
| AST-W.9 | Merge nella catena (Assets_Dev → Assets → AllFunction_Dev → AllFunction) | ⏳ da fare (su richiesta) |
| AST-W.10 | Prova nell'applicativo con dati reali (azienda con Risorse aziendali attivo, utente e amministratore) | ⏳ da fare |
