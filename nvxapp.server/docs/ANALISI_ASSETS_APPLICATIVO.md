# Analisi – Applicativo Risorse aziendali (Assets)

Branch: **Assets_Dev** (riceve i merge da Infrastructure, si mergia in Assets).
Documento generale: `ANALISI_INFRASTRUCTURE_NUOVI_MODULI.md` (§4.3, aree `AST-*` in §10.2), punto `ANA.9`.

Stato (11/11/2026): **analisi scritta, scheletro del modulo fatto** (`AST-BASE`, ramo di lavoro `Assets_Dev_ast-base`). Le aree funzionali (`AST-FLO`, `AST-BENI`, `AST-PREN`) sono da fare; decisioni aperte in §8.

---

## 1. Obiettivo

**Cosa possiede l'azienda e a chi lo assegna** (ufficio acquisti, logistica, IT):
- **Flotta** (`AST-FLO`): veicoli, scadenze (bollo, assicurazione, revisione, tagliando), assegnazione ai dipendenti, fringe benefit, multe e costi. L'assegnazione dell'auto al dipendente è `Dip_RapportoAutovetture`, spostata in Common (D5): Assets la gestisce, le Presenze continuano a leggerla per l'export paghe.
- **Beni assegnati** (`AST-BENI`): PC, telefoni, badge, attrezzature, con consegna e restituzione (verbale tramite presa visione/firma) e l'elenco dei beni da restituire alla cessazione.
- **Prenotazioni** (`AST-PREN`): postazioni, sale e auto in pool, con calendario, controllo dei conflitti e sincronizzazione facoltativa con il calendario personale (`INF-CAL`).

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
- `Ast_Risorsa`: risorsa prenotabile (tipo: postazione / sala / auto in pool con riferimento a `Ast_Veicolo`), sede, capienza, attiva.
- `Ast_Prenotazione`: risorsa, utente, inizio/fine, note, stato (confermata / annullata); indice per il controllo dei conflitti (sovrapposizione degli intervalli sulla stessa risorsa).

## 5. Uso dei servizi comuni (Infrastructure)

| Servizio | Dove | Uso in Risorse aziendali |
|---|---|---|
| Scadenzario `IDeadlineRegistry` | `ClientServer-Service/infrastructure/Deadline` | Scadenze dei veicoli (`ast.veicolo`), contratti di noleggio/leasing, garanzie dei beni (`ast.bene`), restituzioni attese alla cessazione (`ast.restituzione`). Preavviso predefinito da `Ast_Cfg.GiorniPreavviso` |
| Notifiche `INotificationSender` | `ClientServer-Service/infrastructure/Notification` | Multa da notificare al conducente, bene da restituire, prenotazione confermata o annullata. Rispettare `Ast_Cfg.NotificheAttive` |
| Presa visione `IAcknowledgementService` | `ClientServer-Service/infrastructure/Acknowledgement` | Verbali di consegna e restituzione di beni e veicoli (`ast.consegna`, `ast.restituzione`). Con il Documentale (`DM-FIRMA.2`) diventano firma FES |
| Dati Common | `Dip_Anagrafica`, `Az_Sedi` (`INF-COM.1`), `Dip_RapportoAutovetture` (`INF-COM.3`), data di cessazione (`INF-COM.4`) | Sola lettura con `Ast_<Entità>ReadRepository`; scritture delle assegnazioni dell'auto con i repository di `Repositories/Tenant/Common` |
| Calendari (`INF-CAL`) | da fare | Pubblicazione delle prenotazioni sul calendario personale (facoltativa, `AST-PREN.3`) |
| Cancellazione `IApplicationDataDeleter` | `AstDataDeleter` | Oggi solo `Ast_Cfg` in modalità singola. Con i dati: conteggi (veicoli, beni, prenotazioni), cancellazione per azienda in modalità singola; utente rimosso: prenotazioni future annullate, assegnazioni chiuse (i beni restano all'azienda) |

## 6. Integrazioni facoltative

- **Presenze**: export paghe del fringe benefit da `Dip_RapportoAutovetture` (Common, D5), invariato.
- **Risorse Umane**: all'offboarding (`HR-ONB`) l'elenco dei beni e del veicolo da restituire (`AST-BENI.3`).
- **Documentale**: firma dei verbali di consegna (`DM-FIRMA.2`).
- **Compliance**: i DPI restano in Compliance (proposta `CMP` §6).

## 7. Fatto in `AST-BASE` (11/11/2026)

- Server: `AssetsDbContext` registrato in `Installers4Assets`, design-time factory in un file proprio (`AssetsDbContextDesignTimeFactory.cs`), migration `InitAssets` con `Ast_Cfg`, repository `Ast_CfgRepository`, `Ast_Az_AnagraficaReadRepository`, `AstUtility`, `AstCompanyInitializer` (priorità 70), `AstDataDeleter`, `Ast_ConfigService` + `Ast_ConfigController` (`api/Ast_Config/Ast_CfgGet` amministratori, `Ast_CfgPut` CompanyPowerAdmin), testi `AssetsMessages` (it/en), manuale `assets` (it/en), listini `AstLicenseCatalogSeed`.
- Client: pagina `/astsettings` (`AstSettingsPageComponent`), voce di menu "Risorse aziendali › Impostazioni" (CompanyPowerAdmin, gruppo 70), manuale nella sezione Assistenza, rotta, cartella i18n `assets`.
- Widget e comandi vocali: nessuno per ora.
- Prova su DB usa e getta: azienda, attivazione come SuperUser (non bloccata dalle licenze), schema `tenant_<id>_7` con `Ast_Cfg`, Get/Put del CompanyPowerAdmin, Put rifiutata al CompanyAdmin (12 controlli, tutti ok).

## 8. Decisioni aperte

| # | Decisione | Note |
|---|---|---|
| AST-D1 | Livelli Free/Standard/Pro e prezzi (D8, `ANA.10`) | Oggi listini con la struttura del CRM e **prezzi segnaposto**, nessun limite |
| AST-D2 | Fringe benefit: calcolo in Assets o solo dati per le Presenze | Proposta: Assets tiene valore ACI e periodi, l'export resta nelle Presenze |
| AST-D3 | Legame tra `Dip_RapportoAutovetture` (Common) e `Ast_Veicolo` | Common non può riferire tabelle di un applicativo: targa come chiave logica o tabella ponte in Assets |
| AST-D4 | Auto in pool: prenotazione con o senza approvazione | Proposta: senza approvazione, con limiti in `Ast_Cfg` |
| AST-D5 | Inventario con codici a barre/QR | Utile con `AT-TIMB.1` (QR); da valutare |
| AST-D6 | Cartella i18n e `i18n-folders.ts` | Come per HR: riga aggiunta in `i18n-folders.ts` (file di Infrastructure), conflitto banale nel merge di AllFunction, oppure stub `i18n-assets.ts` in Infrastructure |

## 9. Checklist

Legenda: ✅ svolto · ⏳ da fare · 🔄 in corso · 🔍 da decidere

#### `AST-BASE` – Avvio del modulo
- ✅ `AST-BASE.1` `AssetsDbContext` registrato, migration `InitAssets` con `Ast_Cfg`, repository, inizializzatore e `AstDataDeleter`
- ✅ `AST-BASE.2` Cartelle del modulo, servizio/controller della configurazione, pagina `/astsettings`, ruoli (§3)
- ✅ `AST-BASE.3` Menu, rotta, manuale e i18n; widget e comandi vocali: nessuno per ora

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

#### Decisioni
- 🔍 `AST-D1`…`AST-D6` (§8)
