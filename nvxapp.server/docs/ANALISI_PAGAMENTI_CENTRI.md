# Analisi – Ripartizione degli incassi delle licenze ai centri

Data: 11/11/2026. Ramo: **Infrastructure** (funzioni base). Si appoggia al sistema licenze (`ANALISI_LICENZE.md`).

**Scopo.** Le aziende e gli studi pagano le licenze al **SuperUser** (la piattaforma), che incassa tutto e poi **gira ai centri** (`Dealer`) la loro quota. Il SuperUser deve vedere i pagamenti per mese e per anno suddivisi per centro, liquidare i centri e mandare loro riepiloghi mensili e annuali via email; ogni centro vede le stesse informazioni limitate a sé.

---

## 1. Situazione di partenza

- **Centro = `Dealer`** (gerarchia Dealer → FinancialAdvisor/studio → Company/azienda → User, `ANALISI_LICENZE.md` §1.1).
- **`Lic_Pagamento`** registra già ogni incasso (una riga per sottoscrizione o rinnovo, scritta da `LicenseSubscriptionManager.PagaAsync`) e resta anche dopo la cancellazione del soggetto, "per i resoconti degli incassi".
- Mancavano:
  - il **centro** sul pagamento: si poteva ricavare solo risalendo azienda → studio → centro, impossibile dopo una cancellazione. `IdDealerListino` è il proprietario del listino (null = listino globale), non il centro del cliente;
  - la **data di incasso** (c'era solo la data di creazione della riga);
  - qualsiasi elenco trasversale per il SuperUser, i totali per periodo e per centro, i rendiconti e le email.
- Si riusano: `EmailService` (MailKit, con allegati), MigraDoc (PDF, come `UserManualPdf`), ClosedXML (Excel), il modello `BackgroundService` + `PeriodicTimer` (`LicenseMaintenanceService`, `StorageMaintenanceService`), ruoli `SuperUser` / `DealerAdmin` / `DealerPowerAdmin` con `CurrentDealer` dal token, `nvx-list`, `MonthNavigatorService`.

## 2. Flusso e regole

```
Azienda/Studio ──paga la licenza──▶ SuperUser (Lic_Pagamento, con il centro del cliente)
                                        │  giorno N del mese: rendiconto del mese precedente per ogni centro (Bozza)
                                        ▼
                                 Pag_Rendiconto ──conferma──▶ email al centro (Confermato)
                                        │  bonifico del SuperUser
                                        ▼
                                   Liquidato ──▶ email di conferma al centro
```

- **A chi va la quota**: al centro del cliente che ha pagato, **fotografato al momento del pagamento** (`Lic_Pagamento.IdDealer`, `DealerDescrizione`). Se il cliente passa a un altro centro, i pagamenti già fatti restano al centro di prima. Cliente senza centro → l'incasso resta tutto alla piattaforma.
- **Criterio di cassa**: conta `DataPagamento` (quando il pagamento è andato a buon fine). Un annuale si liquida per intero nel mese in cui è incassato.
- **Liquidazione mensile**: l'unità operativa è il mese. Il **riepilogo annuale** è la somma dei 12 rendiconti, con una sua vista e una sua email, ma senza liquidazione propria.
- **Nessun doppio conteggio**: da liquidare = `Stato = Completato`, `Debug = false`, `IdDealer` valorizzato, `IdRendiconto` null, `DataPagamento` entro la fine del mese del rendiconto. Un pagamento confermato in ritardo entra nel primo rendiconto aperto.
- **Rimborsi**: un pagamento già liquidato e poi rimborsato (`Stato = Rimborsato`) entra come rettifica negativa nel primo rendiconto successivo (`Lic_Pagamento.IdRendicontoRettifica`, `Pag_Rendiconto.TotaleRettifiche`). Un pagamento rimborsato prima della liquidazione semplicemente non entra.
- **Pagamenti in debug esclusi** dai rendiconti (oggi `Payments:Debug = true` ovunque: finché non arrivano i pagamenti reali i rendiconti risultano vuoti, ma i pagamenti debug si vedono negli elenchi, segnati come tali, per le prove).
- **Calcolo**: `Base = TotaleIncassato − TotaleRettifiche`; `QuotaPiattaforma = round(Base × Percentuale / 100, 2)`; `QuotaCentro = Base − QuotaPiattaforma`. Una base negativa (rimborsi superiori agli incassi del mese) resta negativa: è un credito della piattaforma verso il centro, che si compensa nei mesi successivi.

## 3. Modello dati (schema `public`)

| Entità | Campi | Note |
|---|---|---|
| **`Lic_Pagamento`** (estesa) | `IdDealer`, `DealerDescrizione`, `DataPagamento`, `IdRendiconto`, `IdRendicontoRettifica` | `IdDealer` senza chiave esterna (come `IdCompany`); FK verso il rendiconto con `SetNull`; indice `(IdDealer, DataPagamento)` |
| **`Pag_Rendiconto`** | IdDealer, DealerDescrizione, Anno, Mese, NumeroPagamenti, TotaleIncassato, TotaleRettifiche, PercentualePiattaforma, QuotaPiattaforma, QuotaCentro, Valuta, Stato, DataConferma, DataLiquidazione, RiferimentoBonifico, NumeroFatturaCentro, DataFatturaCentro, Note | un solo rendiconto non annullato per centro e mese (indice univoco filtrato); importi e percentuale fotografati; resta anche se il centro viene cancellato |
| **`Pag_DealerConfig`** | IdDealer (univoco, cascade), PercentualePiattaforma (null = generale), Iban, Intestatario, EmailAmministrazione | dati per la liquidazione |
| **`Pag_InvioEmail`** | IdDealer, Anno, Mese (null = annuale), Tipo (Mensile / Annuale / Liquidazione), IdRendiconto, Automatico, Destinatari, Esito, Errore | storico e idempotenza degli invii automatici |

**Stati del rendiconto** (`StatoRendiconto`):
- `Bozza`: generato, rigenerabile ed eliminabile; il centro non lo vede; i pagamenti vengono agganciati solo alla conferma.
- `Confermato`: il SuperUser l'ha controllato; i pagamenti sono agganciati (`IdRendiconto`), parte l'email al centro, che ora lo vede.
- `Liquidato`: bonifico fatto (data, riferimento); email di conferma.
- `Annullato`: solo da `Confermato`; i pagamenti tornano da liquidare (`IdRendiconto = null`).

**Impostazioni** (`AppSetting`): `Pagamenti_PercentualePiattaforma` (0, da impostare), `Pagamenti_RiepilogoAutomatico` (false), `Pagamenti_GiornoInvioMensile` (5), `Pagamenti_MeseInvioAnnuale` (1).

**Fattura del centro**: `NumeroFatturaCentro` / `DataFatturaCentro` registrano la fattura che il centro emette alla piattaforma per la sua quota. Per ora informative (non bloccano la liquidazione). Gli aspetti fiscali dell'incasso per conto terzi vanno verificati con il commercialista.

## 4. Servizio, API, permessi

`ClientServer-Service/infrastructure/Payments/PaymentReportService.cs` + `Controllers/Infrastructure/PaymentReportController.cs`, stesso schema di `LicenseService` (`GenericRequest<XxxInModel>`, `ExecuteAction`, `UserMessageException` con testi in `InfrastructureMessages`).

Il calcolo e i cambi di stato stanno in `PaymentSettlementManager` (non dipende dall'utente: lo usa anche la manutenzione); i permessi in `PaymentReportService`.

| API (`api/PaymentReport/...`) | Chi | Cosa |
|---|---|---|
| `PaymentReportGet(anno, mese?)` | SuperUser | per centro (anche quelli senza movimenti e i clienti senza centro): pagamenti, incassato, rimborsato, debug, trattenuta, quota del centro, liquidato, da liquidare, rendiconto del mese; totali |
| `PaymentDealerGet(idDealer?, anno, mese?)` | SuperUser, centro | riepilogo del centro, pagamenti del periodo, rendiconti dell'anno (il centro non vede le bozze), storico delle email |
| `PaymentSettlementGenerate(anno, mese, idDealer?)` | SuperUser | crea o aggiorna le bozze (un centro o tutti quelli con qualcosa da liquidare) |
| `PaymentSettlementConfirm` / `Cancel` / `Pay(data, riferimento, note)` | SuperUser | cambi di stato; conferma e liquidazione inviano l'email (se non parte, il cambio di stato resta e l'esito è nello storico) |
| `PaymentSettlementInvoicePut(numero, data)` | SuperUser, DealerPowerAdmin del centro | fattura del centro sul rendiconto confermato o liquidato |
| `PaymentSummarySend(idDealer?, anno, mese?)` | SuperUser (qualsiasi centro), centro (il proprio) | email HTML con il PDF del riepilogo, registrata in `Pag_InvioEmail` |
| `PaymentExport(idDealer?, anno, mese?, formato)` | SuperUser, centro | PDF del riepilogo di un centro; Excel di un centro o (SuperUser) di tutti |
| `PaymentDealerConfigGet/Put` | SuperUser (tutto), DealerPowerAdmin (IBAN, intestatario, email del proprio centro; non la trattenuta); DealerAdmin legge | |
| `PaymentSettingsGet/Put` | SuperUser | impostazioni `Pagamenti_*` |

**Permessi** nel servizio (come `LicenseService.cs` §12 delle licenze): SuperUser vede e gestisce tutto; DealerAdmin / DealerPowerAdmin vedono **solo** il proprio centro, con `idDealer` **forzato da `CurrentDealer`** (dal token, mai dal client); gli altri ruoli nessun accesso. Il SuperUser che impersona un utente del centro, senza indicare un centro, lavora sul centro impersonato. I guard Angular sono solo un aiuto all'interfaccia.

## 5. Email e automatismi

- Corpo HTML costruito dai testi resx (come `LicenseSubscriptionManager`), PDF del rendiconto o del riepilogo annuale in allegato (MigraDoc).
- Destinatari: `Pag_DealerConfig.EmailAmministrazione`, altrimenti i DealerPowerAdmin del centro (via `UserDealer`).
- Ogni invio (riuscito o no) finisce in `Pag_InvioEmail`.
- **`PaymentMaintenanceService`** (`nvxapp.server/Utility/`, come `StorageMaintenanceService`), una volta al giorno dopo le 4, attivo solo con `Pagamenti_RiepilogoAutomatico = true`:
  - dal giorno `Pagamenti_GiornoInvioMensile`: genera le **bozze** del mese precedente, una volta per mese (`AppSetting` `Pagamenti_UltimaGenerazione` = "aaaa-mm", così una bozza eliminata dal SuperUser non ricompare); il SuperUser le conferma (e la conferma invia l'email);
  - dal mese `Pagamenti_MeseInvioAnnuale`: invia a ogni centro con movimenti il riepilogo dell'anno precedente, una volta sola (controllo su `Pag_InvioEmail`).
- Email di liquidazione quando il SuperUser segna il rendiconto come liquidato.

## 6. Interfaccia

- Pagina **`payment-report`** (`pages/infrastructure/`), che si adatta al ruolo:
  - filtri Anno, Mese (con "Tutto l'anno"), Centro (solo SuperUser);
  - riga di totali: Incassato, Trattenuta, Quote centri, Da liquidare;
  - `nvx-list` per centro con azioni Dettaglio, Genera/Conferma, Liquida, Invia email, PDF, Excel.
- Pagina **dettaglio** del centro e del periodo: righe dei pagamenti (debug evidenziati) e rendiconti.
- Pagina **dati di liquidazione** del centro (`Pag_DealerConfig`, `BasePageConfirmCancelComponent`).
- Menu: `Pages4SuperUser` e `Pages4DealerAdmin` in `main-menu-infrastructure.service.ts`; route con guard in `route-infrastructure.service.ts`.
- Testi in `assets/i18n/infrastructure/{it,en}.json`, manuale `Manuals/infrastructure.*.md`.

## 7. Decisioni (11/11/2026)

1. Il SuperUser incassa dalle aziende e dagli studi e gira la quota ai centri.
2. Trattenuta della piattaforma: percentuale per centro, con un valore generale per i centri senza una propria. Non per funzione.
3. I rendiconti si generano in bozza; il SuperUser li conferma prima dell'invio.
4. Fattura del centro: campi informativi sul rendiconto, nessun blocco.
5. Pagamenti in debug esclusi dai rendiconti.
6. Criterio di cassa; liquidazione mensile; riepilogo annuale come somma.

## 8. Fasi

1. **Modello dati** (fatta): campi su `Lic_Pagamento` valorizzati in `PagaAsync`, entità `Pag_*`, impostazioni, migration `PagamentiCentri` che riempie i pagamenti già presenti.
2. **Consultazione** (fatta): servizio, API e pagina con elenco e totali per mese/anno e per centro, dettaglio.
3. **Rendiconti** (fatta): generazione, conferma, liquidazione, annullamento, fattura, dati di liquidazione del centro.
4. **Email e documenti** (fatta): invio manuale con PDF, esportazione PDF ed Excel.
5. **Automatismi** (fatta): `PaymentMaintenanceService`.
6. Manuale (fatto); **schermate** del manuale da rigenerare con `nvxapp.server/Tools/ManualScreenshots`.

Con i pagamenti reali (Fase 3 delle licenze) la conferma del provider via webhook dovrà valorizzare `DataPagamento` come fa oggi `PagaAsync`.

## 9. Stato dell'implementazione

### Fase 1 (11/11/2026)

| Parte | Dove |
|---|---|
| Entità ed enum | `Entities/Public/Lic_Pagamento.cs` (campi nuovi), `Pag_Rendiconto.cs`, `Pag_DealerConfig.cs`, `Pag_InvioEmail.cs`, `PaymentEnums.cs` |
| Tabelle, indici, vincoli, impostazioni | `PublicDbContext_Infrastructure.cs` (`Gen_Payments`, indici nuovi in `Gen_License`) |
| Repository | `Repositories/Public/Pag_*Repository.cs` |
| Centro e data di incasso sul pagamento | `LicenseSubscriptionManager.PagaAsync` / `CentroDelSoggettoAsync` |
| Migration | `Migrations/Public/*_PagamentiCentri.cs` (con l'aggiornamento dei pagamenti esistenti) |

### Fasi 2-5 (11/11/2026)

| Parte | Dove |
|---|---|
| Calcoli, rendiconti, documenti, email | `ClientServer-Service/infrastructure/Payments/PaymentSettlementManager.cs` |
| PDF (MigraDoc, font del manuale) ed Excel (ClosedXML) | `Payments/PaymentReportPdf.cs` (`PaymentReportPdf`, `PaymentReportExcel`) |
| Impostazioni | `Payments/PaymentSettingsProvider.cs` |
| API e permessi | `Payments/PaymentReportService.cs`, `Payments/Models/PaymentModel.cs`, `Controllers/Infrastructure/PaymentReportController.cs` |
| Manutenzione | `nvxapp.server/Utility/PaymentMaintenanceService.cs` (registrata in `Installers.InstallServices`) |
| Client | `ClientServer-Service/Infrastructure/Payment/` (servizio e modelli); pagine `payment-report` (SuperUser), `payment-dealer` (SuperUser e centro), `payment-dealer-config`, `payment-settings`; route in `route-infrastructure.service.ts`, voci in `Pages4SuperUser` e `Pages4DealerAdmin` |
| Testi | `InfrastructureMessages*.resx` (chiavi `Payment*`), `assets/i18n/infrastructure/*.json` (`pages.payment*`, `menu.payments`, `menu.dealerPayments`), manuale `Manuals/infrastructure*.md` |

**Verifica.** Build di server e client; prova completa delle API su un database PostgreSQL usa e getta con un server SMTP finto (49 controlli): riepiloghi e totali per centro (con debug, senza centro, centri senza movimenti), permessi (SuperUser, DealerPowerAdmin, DealerAdmin, studio, SuperUser che impersona un centro), mese non concluso, bozze, conferma con ricalcolo e aggancio dei pagamenti, doppia conferma, liquidazione (data futura rifiutata), fattura, annullamento, rimborso dopo la liquidazione come rettifica negativa, PDF ed Excel, email con PDF ai DealerPowerAdmin e all'email dell'amministrazione, dati di liquidazione (IBAN non valido, trattenuta solo dal SuperUser), sottoscrizione reale in debug che fotografa centro e data di incasso, giro della manutenzione (una bozza generata, marcatore del mese scritto).
