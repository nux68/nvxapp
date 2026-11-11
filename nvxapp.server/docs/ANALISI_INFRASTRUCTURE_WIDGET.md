# Widget della dashboard — analisi generale

Documento generale (ramo **Infrastructure**). Ogni ramo applicativo ha il proprio documento di dettaglio:

| Ramo | Documento | Widget |
|---|---|---|
| Infrastructure | questo documento (§6) | Chiedi all'assistente |
| AttendanceTracking (Presenze) | `ANALISI_WIDGET_PRESENZE.md` | Dipendenti presenti/assenti, Richieste approvate/da approvare |
| DocumentManagement (Documentale) | `ANALISI_WIDGET_DOCUMENTALE.md` | Spazio occupato/disponibile, Spazio per tipo documento |
| CRM | `ANALISI_CRM_WIDGET.md` | nessuno per ora (punto di estensione pronto) |

---

## 1. Obiettivo

La **dashboard** (pagina iniziale dopo il login, `/dashboard`) mostra il saluto e una griglia di **widget**:
piccoli riquadri con informazioni o comandi rapidi.

- Ogni **applicativo** dichiara una **collezione di widget**.
- Ogni utente **sceglie** quali widget vedere e in che **ordine**, dalla pagina del proprio ruolo:

| Ruolo | Pagina di configurazione | Voce di menu |
|---|---|---|
| User | `/user` | Accesso → **Preferenze** |
| CompanyAdmin | `/companyadmin` | Generale → **Preferenze** |
| CompanyPowerAdmin | `/companypoweradmin` | Generale → **Preferenze** |

Le tre voci (e il titolo delle pagine) usano la chiave `menu.preferences` ("Preferenze"), con l'icona `options`.

- Chi non ha ancora configurato la dashboard vede i **widget predefiniti**.
- Le pagine dei ruoli prima erano segnaposto; nella pagina User c'erano i **pulsanti demo** dei job lunghi ("Esegui", "Export"): sono stati **eliminati**.

## 2. Sensibilità al ramo

Il catalogo dei widget dipende dal **ramo compilato**, con lo stesso meccanismo già usato per menu, rotte,
showcase e traduzioni: Infrastructure contiene un file per ogni applicativo, **vuoto**; il ramo dell'applicativo lo riempie.

| File (client, `src/nvx/Utility/infrastructure/`) | In Infrastructure | Lo riempie |
|---|---|---|
| `widget-infrastructure.service.ts` | widget delle funzioni base | — |
| `widget-attendance-tracking.service.ts` | vuoto | AttendanceTracking |
| `widget-crm.service.ts` | vuoto | CRM |
| `widget-document-management.service.ts` | vuoto | DocumentManagement |

Risultato, seguendo la catena dei merge (Infrastructure → rami → AllFunction):

| Ramo compilato | Widget nel catalogo |
|---|---|
| Infrastructure | Infrastructure |
| AttendanceTracking | Infrastructure + Presenze |
| CRM | Infrastructure + CRM |
| DocumentManagement | Infrastructure + Documentale |
| AllFunction | tutti |

I file dei rami non entrano in conflitto nei merge: ognuno modifica solo il proprio.

## 3. Quali widget vede un utente

`WidgetService.Disponibili` filtra il catalogo del ramo:

1. **Applicativo attivo**: un widget di un applicativo compare solo se l'applicativo è attivo per l'azienda dell'utente
   (`AuthService.hasApplication`, che tiene già conto delle **licenze**: ANALISI_INFRASTRUCTURE_LICENZE.md §15.6).
   I widget di Infrastructure ci sono sempre.
2. **Ruoli**: ogni widget può limitare i ruoli che lo usano (`roles`); senza indicazione vale per tutti.

I dati mostrati dai widget arrivano dai servizi degli applicativi, che **ricontrollano i permessi** come per le loro
pagine: il filtro del client serve a non proporre widget inutili, non a proteggere i dati.

## 4. Modello

### 4.1 Client — definizione di un widget (`widget.model.ts`)

| Campo | Significato |
|---|---|
| `key` | chiave unica e stabile, salvata sul server: `<applicativo>.<nome>` (es. `documents.storage`) |
| `application` | applicativo (assente = Infrastructure); il servizio la assegna da solo ai widget dei rami |
| `title`, `description` | chiavi di traduzione (titolo nella dashboard, descrizione nella configurazione) |
| `icon` | icona ionicons |
| `roles` | ruoli che possono usarlo (assente = tutti) |
| `size` | `small` (1/3), `medium` (1/2), `large` (riga intera) della griglia a 12 colonne; sotto 768px ogni widget occupa la riga |
| `default` | presente nella dashboard di chi non l'ha ancora configurata |
| `zorder` | ordine nel catalogo e tra i predefiniti |
| `frameless` | senza cornice: titolo e bordo li disegna il widget |
| `component` | componente **standalone** che mostra il widget e carica da solo i propri dati |

La dashboard crea i componenti con `NgComponentOutlet`; a ogni ingresso nella pagina i widget vengono ricreati e
ricaricano i dati.

### 4.2 Server — scelte dell'utente (tabella `public.UserDashboard`)

| Colonna | Significato |
|---|---|
| `IdAspNetUsers` | utente (cancellazione a cascata) |
| `IdCompany` | azienda in cui lavora (cancellazione a cascata): la stessa persona può avere dashboard diverse per azienda |
| `Widgets` | chiavi dei widget in JSON, nell'ordine (max 50, formato `[A-Za-z0-9._-]`, max 100 caratteri) |

- Indice univoco (`IdAspNetUsers`, `IdCompany`).
- **Nessuna riga** = dashboard mai configurata → widget predefiniti.
- **Riga con elenco vuoto** = dashboard volutamente vuota (solo il saluto).
- Il server **non conosce il catalogo** (lo dichiara il client di ogni ramo): controlla solo il formato delle chiavi.
- Le chiavi che il ramo non conosce o che oggi non sono disponibili (applicativo disattivato, licenza scaduta, ruolo)
  **non si mostrano ma restano salvate**: il client le rimette in coda quando salva, così il widget torna da solo
  quando ridiventa disponibile.
- Entità senza navigation (come `CompanyMenuRestriction`): i contesti degli applicativi mappano `Company` e
  `AspNetUsers` e non devono scoprire la tabella.
- Migration del contesto Public: `UserDashboard` (aggiunta dopo `InitPublic`, senza ricreare il database).

### 4.3 API (`UserDashboardController`)

| Endpoint | Ingresso | Uscita |
|---|---|---|
| `UserDashboard/UserDashboardGet` | — | `Configurabile`, `Configurata`, `Widgets` (chiavi) |
| `UserDashboard/UserDashboardPut` | `Widgets` (chiavi nell'ordine) | come sopra, dopo il salvataggio |

- `Configurabile` = l'utente lavora in un'azienda (claim `company`) con ruolo User, CompanyAdmin o CompanyPowerAdmin.
- Il salvataggio da parte di chi non è configurabile è rifiutato (`DashboardNotConfigurable`).
- Messaggi: `DashboardNotConfigurable`, `DashboardTooManyWidgets`, `DashboardWidgetInvalid` (`InfrastructureMessages`).

## 5. Interfaccia

### 5.1 Dashboard (`dashboard-page`)

- In alto il saluto e, per chi può configurarla, il pulsante **Personalizza** che apre la pagina del proprio ruolo.
- Sotto, i widget scelti, ognuno nella sua cornice (icona e titolo), tranne i widget `frameless`.
- Nessun widget: messaggio "Nessun widget nella dashboard".

**Disposizione** (`Utility/infrastructure/widget-layout.ts`), in funzione del numero e delle dimensioni:

1. nell'ordine scelto i widget riempiono **righe da 12 colonne** (small 4, medium 6, large 12);
2. una riga con **più widget** che non arriva a 12 colonne si **riempie** allargandoli in proporzione
   (due small → metà e metà; small + medium → 40% e 60%): niente buchi;
3. un widget **da solo** nella riga (non large) è largo **almeno metà pagina** ed è **centrato**, allineato al campo
   dell'assistente: a tutta pagina indicatori e grafici diventerebbero dispersivi;
4. i widget della stessa riga hanno la **stessa altezza**.

| Esempio | Disposizione |
|---|---|
| assistente + 1 small (es. User con "Richieste") | assistente; sotto "Richieste" centrato, largo metà |
| 3 small | una riga piena, un terzo ciascuno |
| 3 small + 1 medium (CompanyPowerAdmin predefinito) | riga di tre; sotto il medium centrato, largo metà |
| 2 small | metà e metà |
| 1 small + 1 medium | 40% e 60% |

Sotto i 992px nessun widget è più stretto di metà riga (i small vanno due per riga); sotto i 768px un widget per riga.

### 5.2 Configurazione (`app-widget-config`, nelle pagine dei ruoli)

- **Nella dashboard**: i widget scelti, nell'ordine; per ognuno *Sposta su*, *Sposta giù*, *Togli*;
  pulsante **Ripristina predefiniti**.
- **Widget disponibili**: quelli non ancora scelti, raggruppati per applicativo (Funzioni base, Presenze, CRM,
  Documentale; solo i gruppi con widget), ognuno con **Aggiungi**.
- Barra dei pulsanti: **Conferma** salva e apre la dashboard; **Annulla** ricarica la configurazione salvata.
- Classe base comune delle tre pagine: `DashboardConfigPageBase`.

### 5.3 Stile

Colori e misure del tema (`--nvx-*`, `--ion-color-*`), quindi anche nel tema scuro. I widget dei rami includono
gli stili comuni `shared/components/infrastructure/widgets/_widget-common.scss` (`@include wc.widget-common`):
indicatori (`.nvx-w-stats`, `.nvx-w-stat` con varianti `primary`/`success`/`warning`/`danger`), barra
(`.nvx-w-bar`), testo secondario, messaggio vuoto, caricamento, collegamento.

## 6. Widget di Infrastructure

| Chiave | Titolo | Dimensione | Ruoli | Predefinito | Note |
|---|---|---|---|---|---|
| `infrastructure.askAssistant` | Chiedi all'assistente | large, senza cornice | tutti | sì | campo a pillola che apre la chat dell'assistente (FAB); è il contenuto della vecchia dashboard |

## 7. Aggiungere un widget (in un ramo)

1. **Componente standalone** nel ramo (es. `shared/components/<Ramo>/widgets/<nome>-widget/`): carica i dati nel
   costruttore o in `ngOnInit`, gestisce caricamento ed errori, include gli stili comuni.
2. **Dichiarazione** nel file del ramo (`widget-<ramo>.service.ts`): chiave `<prefisso>.<nome>`, testi, icona,
   dimensione, ruoli, predefinito, `zorder`.
3. **Testi** nel file di traduzione del ramo, sotto il suo prefisso (`attendance.widgets.*`, `documents.widgets.*`, `crm.widgets.*`).
4. **Dati**: se serve, un endpoint del servizio dell'applicativo, con i controlli dei permessi delle pagine.
5. **Documento di dettaglio** del ramo aggiornato.

Una chiave, una volta rilasciata, **non si cambia**: è salvata nelle dashboard degli utenti.

## 8. File

| Livello | File |
|---|---|
| Entità | `nvxapp.server.data/Entities/Public/UserDashboard.cs` |
| Contesto | `PublicDbContext_Infrastructure.cs` (`Gen_UserDashboard`) |
| Migration | `Migrations/Public/*_UserDashboard.cs` |
| Repository | `Repositories/Public/UserDashboardRepository.cs` |
| Servizio | `ClientServer-Service/infrastructure/UserDashboard/UserDashboardService.cs` |
| Controller | `Controllers/Infrastructure/UserDashboardController.cs` |
| Client, servizio API | `ClientServer-Service/Infrastructure/UserDashboard/user-dashboard.service.ts` |
| Client, catalogo | `Utility/infrastructure/widget.model.ts`, `widget.service.ts`, `widget-*.service.ts` |
| Client, disposizione | `Utility/infrastructure/widget-layout.ts` |
| Client, componenti | `shared/components/infrastructure/widgets/` (assistente, configurazione, stili comuni) |
| Pagine | `dashboard-page`, `user-page`, `company-admin-page`, `company-power-admin-page` |
| Traduzioni | `assets/i18n/infrastructure/{it,en}.json`, sezione `widgets` |

## 9. Sviluppi possibili

- Trascinamento per riordinare i widget; dimensione scelta dall'utente.
- Dashboard predefinita decisa dall'azienda (CompanyPowerAdmin) per i propri utenti.
- Aggiornamento in tempo reale dei widget con le notifiche SignalR già presenti.
