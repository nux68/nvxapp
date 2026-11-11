# Analisi: assistenza ai clienti dell'azienda (CRM-ASS)

> Analisi dell'11/11/2026, branch `CRM_Dev` (lavoro su `CRM_Dev_f4`, worktree `wt_crm-f4`).
> Area `CRM-ASS` di `ANALISI_INFRASTRUCTURE_NUOVI_MODULI.md` (§3.3, §10.2: punti `CRM-ASS.1`–`CRM-ASS.2`).
> Modello generale del CRM: `ANALISI_CRM_APPLICATIVO.md`.
>
> **Obiettivo**: dare all'azienda uno strumento per gestire le **richieste di assistenza dei propri clienti**
> (es. "il cliente Rossi Srl segnala un guasto", "chiede un intervento"): registrazione, priorità, assegnazione a un
> collega, tempi di presa in carico e di risoluzione, note interne, notifica all'assegnatario, storico nella scheda
> del cliente.

Legenda: ✅ svolto · ⏳ da fare · 🔍 da valutare.

## 1. Cosa NON è

- **Non sono i ticket di Infrastructure** (`SupportTicket`, `TicketService`): quelli sono le richieste degli utenti
  verso la piattaforma (centro/superuser). Qui i "clienti" sono i clienti dell'azienda (`Az_Cliente` comune) e chi
  lavora la richiesta è un utente dell'azienda.
- Nessun portale per il cliente finale e nessuna email al cliente in questa fase (*scelta provvisoria*): le richieste
  le registra l'azienda (telefonata, email ricevuta, ...); i messaggi sono **interni**.

## 2. Modello dati (schema `tenant_<id>_3`, migration incrementale `Crm_Assistenza` del contesto Crm)

### `Crm_Richiesta`

| Campo | Tipo | Note |
|---|---|---|
| `IdAz_Anagrafica` | int | azienda (come le altre tabelle del CRM) |
| `Anno`, `Numero` | int | numerazione per anno (indice unico azienda+anno+numero), mostrata come `N/AAAA` |
| `IdAz_Cliente` | int | cliente comune (FK, Restrict: un cliente con richieste non si cancella, `ICommonEntityUsage`) |
| `IdAz_ClienteContatto` | int? | chi ha chiesto (FK, SetNull) |
| `Oggetto` | string(200) | obbligatorio |
| `Descrizione` | string(4000)? | |
| `Canale` | enum `Crm_CanaleRichiesta` | Telefono = 0, Email = 1, DiPersona = 2, Sito = 3, Altro = 4 |
| `Priorita` | enum `Crm_PrioritaRichiesta` | Bassa = 0, Normale = 1 (predefinita), Alta = 2, Urgente = 3 |
| `Stato` | enum `Crm_StatoRichiesta` | Nuova = 0, InCarico = 1, InAttesaCliente = 2, Risolta = 3, Chiusa = 4, Annullata = 5 |
| `IdAspNetUsersAssegnatario` | string? | utente dell'azienda (FK su `AspNetUsers`, SetNull) |
| `DataApertura` | DateTime | quando il cliente ha chiesto (predefinita adesso) |
| `DataPresaInCarico` | DateTime? | prima volta che la richiesta esce da "Nuova" verso uno stato di lavorazione |
| `DataRisoluzione` | DateTime? | passaggio a Risolta (o Chiusa se non era già risolta) |
| `DataChiusura` | DateTime? | passaggio a Chiusa o Annullata |
| `Soluzione` | string(2000)? | cosa si è fatto |
| `IdCrm_Opportunita` | int? | facoltativa (es. la richiesta genera una trattativa), FK SetNull |

### `Crm_RichiestaMessaggio`

Note e messaggi **interni** della richiesta (con data e autore), più gli eventi registrati dal sistema
(`Sistema = true`: assegnazione, cambio di stato) che formano lo storico della richiesta.

| Campo | Tipo |
|---|---|
| `IdCrm_Richiesta` | int (FK Cascade) |
| `Data` | DateTime |
| `IdAspNetUsersAutore` | string? |
| `Testo` | string(4000) |
| `Sistema` | bool |

## 3. Stati e tempi

```
Nuova ──► In carico ◄──► In attesa del cliente
  │           │
  │           ▼
  └──────► Risolta ──► Chiusa          (Annullata da qualsiasi stato aperto)
              ▲   │
              └───┘ (riapertura: torna In carico)
```

- **Aperte** = Nuova, In carico, In attesa del cliente. **Chiuse** = Risolta, Chiusa, Annullata.
- Tempi: **presa in carico** = `DataPresaInCarico − DataApertura`, **risoluzione** = `DataRisoluzione − DataApertura`
  (in ore, calcolati dal server e mostrati nella scheda e nell'elenco). Una riapertura svuota risoluzione e chiusura
  (la presa in carico resta).
- Nessun SLA configurabile per priorità (*scelta provvisoria*); 🔍 in futuro tempi obiettivo per priorità e
  scadenza nello scadenzario.

## 4. Assegnazione e notifica

- Assegnatario scelto tra gli utenti dell'azienda (stessa lista di responsabili/assegnatari del CRM).
- A ogni **cambio di assegnatario** (nuova richiesta compresa) verso un utente diverso da chi salva: notifica con
  `INotificationSender` (categoria `generale`, *scelta provvisoria* in attesa di `INF-SVC.7` per le categorie dei
  moduli), titolo "Richiesta di assistenza N/AAAA assegnata", testo con cliente, oggetto e priorità, link
  `/crmrequestedit` con `{ id }`. La notifica resta nella pagina Notifiche e arriva a comparsa/email secondo le
  preferenze dell'utente.
- L'evento finisce anche tra i messaggi della richiesta (storico).

## 5. Ruoli e visibilità

| Ruolo | Cosa vede | Cosa fa |
|---|---|---|
| CompanyPowerAdmin, CompanyAdmin | tutte le richieste | crea, modifica, assegna a chiunque, cancella |
| User | le richieste dei clienti che vede (regola di visibilità del CRM) e quelle assegnate a lui | crea richieste per i clienti che vede; prende in carico quelle senza assegnatario o assegnate a lui (si assegna a sé o le lascia libere, come per i responsabili); lavora (stato, soluzione, note) quelle assegnate a lui o libere; non le riassegna ad altri né le cancella |

Controlli tutti lato server (`CrmUtility.RichiesteVisibili`), le pagine li rispecchiano soltanto.

## 6. Pagine (client)

- **Richieste clienti** (menu del CRM, amministratori e commerciali): elenco con filtri *aperte* / *tutte* / *mie*,
  colonne numero, cliente, oggetto, priorità, stato, assegnatario, apertura, tempi; **+** nel FAB per una nuova
  richiesta.
- **Richiesta** (`BasePageConfirmCancelComponent`): cliente, contatto, oggetto, descrizione, canale, priorità, stato,
  assegnatario, data apertura, soluzione, opportunità; sezione **Messaggi** (note interne con autore e data, eventi
  di sistema) con l'aggiunta di una nota; cancellazione per gli amministratori.
- **Scheda cliente del CRM**: nuova sezione **Assistenza** con lo storico delle richieste del cliente (aperte e chiuse),
  e voce "Richiesta di assistenza" nel menu dal basso del **+**. La sezione **Offerte** (preventivi) c'era già.
- Lingue it/en; manuale del CRM aggiornato.

## 7. Integrazione con i meccanismi comuni

- `CrmCommonEntityUsage`: "usato da N richieste di assistenza" (blocca la cancellazione del cliente comune).
- `CrmDataDeleter`: conteggio per l'anteprima, cancellazione in modalità singola, utente rimosso → richieste senza
  assegnatario, messaggi senza autore.

## 8. Decisioni (scelte provvisorie da confermare)

| # | Tema | Scelta provvisoria |
|---|---|---|
| A1 | Chi apre le richieste | solo gli utenti dell'azienda (nessun portale/email in ingresso dal cliente) |
| A2 | Messaggi | solo interni, nessuna email al cliente |
| A3 | Categoria della notifica | `generale` (in attesa di `INF-SVC.7`) |
| A4 | SLA | nessun tempo obiettivo per priorità; i tempi sono solo misurati |
| A5 | Visibilità del commerciale | come i clienti del CRM + le richieste assegnate a lui |

## 9. Checklist

- [ ] `CRM-ASS.1` Entità `Crm_Richiesta`, `Crm_RichiestaMessaggio`, enum, migration `Crm_Assistenza`, repository
- [ ] `CRM-ASS.1` Servizio e API (`Crm_Richiesta`: elenco, scheda, salvataggio, nota, cancellazione) con stati, tempi, visibilità per ruolo
- [ ] `CRM-ASS.1` Notifica all'assegnatario (`INotificationSender`) ed eventi nello storico
- [ ] `CRM-ASS.1` Uso del cliente comune e cancellazione fisica (`CrmCommonEntityUsage`, `CrmDataDeleter`)
- [ ] `CRM-ASS.1` Client: elenco e pagina della richiesta, menu, rotte, i18n it/en
- [ ] `CRM-ASS.2` Sezione Assistenza nella scheda cliente e "+" con la nuova richiesta
- [ ] Manuale del CRM aggiornato (it/en)
- [ ] Build server e client
- [ ] Prova su DB usa e getta (assegnazione con notifica, tempi, storico nella scheda cliente, permessi per ruolo)
