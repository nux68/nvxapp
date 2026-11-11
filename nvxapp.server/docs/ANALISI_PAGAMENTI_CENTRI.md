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

| API | Chi | Cosa |
|---|---|---|
| `RiepilogoPerCentro(anno, mese?)` | SuperUser | per centro: n. pagamenti, incassato, rettifiche, trattenuta, quota centro, stato rendiconto, ultimo invio |
| `RiepilogoCentro(anno, mese?)` | Centro | lo stesso per il proprio centro, suddiviso per studio/azienda |
| `ElencoPagamenti(anno, mese?, idDealer?, stato?)` | entrambi | le righe di `Lic_Pagamento` |
| `ElencoRendiconti(anno, idDealer?)` | entrambi (il centro non vede le bozze) | |
| `GeneraRendiconti(anno, mese, idDealer?)` | SuperUser | crea/rigenera le bozze |
| `ConfermaRendiconto`, `AnnullaRendiconto`, `LiquidaRendiconto(data, riferimento)`, `AggiornaFatturaCentro` | SuperUser | cambi di stato |
| `InviaRiepilogo(idDealer, anno, mese?)` | SuperUser (a qualsiasi centro), Centro (a sé stesso) | email HTML + PDF allegato, registrata in `Pag_InvioEmail` |
| `EsportaPdf` / `EsportaExcel` | entrambi | |
| `DealerConfigGet/Put` | SuperUser (tutti), DealerPowerAdmin (Iban, intestatario, email del proprio centro; non la percentuale) | |

**Permessi** nel servizio (come `LicenseService.cs` §12 delle licenze): SuperUser vede e gestisce tutto; DealerAdmin / DealerPowerAdmin vedono **solo** il proprio centro, con `idDealer` **forzato da `CurrentDealer`** (dal token, mai dal client); gli altri ruoli nessun accesso. I guard Angular sono solo un aiuto all'interfaccia.

## 5. Email e automatismi

- Corpo HTML costruito dai testi resx (come `LicenseSubscriptionManager`), PDF del rendiconto o del riepilogo annuale in allegato (MigraDoc).
- Destinatari: `Pag_DealerConfig.EmailAmministrazione`, altrimenti i DealerPowerAdmin del centro (via `UserDealer`).
- Ogni invio (riuscito o no) finisce in `Pag_InvioEmail`.
- **`PaymentSummaryMaintenanceService`** (`nvxapp.server/Utility/`, come `StorageMaintenanceService`), attivo solo con `Pagamenti_RiepilogoAutomatico = true`:
  - dal giorno `Pagamenti_GiornoInvioMensile`: genera le **bozze** del mese precedente; il SuperUser le conferma (e la conferma invia l'email);
  - nel mese `Pagamenti_MeseInvioAnnuale`: invia a ogni centro il riepilogo dell'anno precedente, una volta sola (controllo su `Pag_InvioEmail`).
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
2. **Consultazione**: servizio, API e pagina con elenco e totali per mese/anno e per centro, dettaglio.
3. **Rendiconti**: generazione, conferma, liquidazione, annullamento, dati di liquidazione del centro.
4. **Email e documenti**: invio manuale con PDF, esportazione Excel.
5. **Automatismi**: `PaymentSummaryMaintenanceService`.
6. Manuale e schermate (`nvxapp.server/Tools/ManualScreenshots`).

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
