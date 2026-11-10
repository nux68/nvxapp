# Widget della dashboard — Documentale

Dettaglio del ramo **DocumentManagement**. Il funzionamento generale (catalogo per ramo, configurazione per
utente, tabella `UserDashboard`, stili comuni) è in `ANALISI_WIDGET.md` (Infrastructure).

## 1. Widget

| Chiave | Titolo | Dimensione | Ruoli | Predefinito | Dati |
|---|---|---|---|---|---|
| `documents.storage` | Spazio documenti | small (1/3) | CompanyPowerAdmin, CompanyAdmin | sì | `Storage/CompanyStorageGet` (Infrastructure) |
| `documents.typeSpace` | Spazio per tipo documento | medium (1/2) | CompanyPowerAdmin, CompanyAdmin | sì | `Doc_Widget/Doc_SpazioPerTipoGet` (nuovo) |

- Compaiono solo se il **documentale è attivo** per l'azienda (applicativo attivo e licenza: ANALISI_LICENZE.md §15.6).
- **Ruoli**: gli stessi delle voci di menu "Archivio" e "Spazio documenti". L'utente semplice (User) non li vede:
  lo spazio dell'azienda non lo riguarda e la ripartizione per tipo rivelerebbe i documenti del personale
  (es. il numero e il peso delle buste paga dei colleghi).
- Dichiarazione: `Utility/infrastructure/widget-document-management.service.ts` (in Infrastructure è vuoto).

## 2. Spazio documenti (`documents.storage`)

Indicatori **Occupato** e **Disponibile**, barra con la percentuale della quota, pacchetto e numero di file;
collegamento **Dettagli** alla pagina "Spazio documenti" (`/docstorage`).

| Dato | Origine (`CompanyStorageModel`) |
|---|---|
| Occupato | `usedBytes` |
| Quota | `quotaMB` × 1 MB: quota della **licenza** del documentale se c'è, altrimenti del pacchetto di spazio |
| Disponibile | quota − occupato (minimo 0); con quota illimitata (`quotaMB` = `int.MaxValue`) "Illimitato" e nessuna barra |
| Colore | rosso oltre la quota (`overQuota`), arancione oltre la soglia di avviso (`overWarning`), altrimenti primary |

`CompanyStorageGet` con `IdCompany = 0` legge l'azienda corrente ed è già disponibile a chi vi lavora: nessuna
modifica al server.

## 3. Spazio per tipo documento (`documents.typeSpace`)

Grafico a **ciambella** (SVG disegnato dal widget, nessuna libreria di grafici) con il totale al centro e la
legenda (tipo, spazio, percentuale); collegamento **Apri l'archivio** (`/doclist`).

- Fette: le tipologie ordinate per spazio; oltre la sesta confluiscono in **Altri**; il **Cestino** è una fetta a parte.
- Colori del tema in ordine `primary`, `warning`, `success`, `tertiary`, `danger`, `secondary` (fette vicine con toni lontani; grigi per Altri e
  Cestino), quindi leggibili anche nel tema scuro.

### 3.1 Endpoint `Doc_Widget/Doc_SpazioPerTipoGet`

| | |
|---|---|
| Servizio | `Doc_WidgetService` (`ClientServer-Service/DocumentManagement/Doc_WidgetService/`) |
| Controller | `Doc_WidgetController` |
| Permessi | `DocContesto.AmministraArchivio` (CompanyPowerAdmin o CompanyAdmin), altrimenti `NotAllowed` |
| Uscita | `Tipi[]` (`IdDoc_Par_Tipo`, `Descrizione`, `Bytes`, `Documenti`), `CestinoBytes`, `TotaleBytes` |

Calcolo, sullo schema dell'azienda:

- `Bytes` = somma di `Doc_Versione.Dimensione` di **tutte le versioni** dei documenti della tipologia, fuori dal cestino;
- `CestinoBytes` = la stessa somma per i documenti nel cestino;
- `Documenti` = documenti della tipologia fuori dal cestino;
- `TotaleBytes` = tipi + cestino.

È lo stesso criterio del ricalcolo notturno dello spazio (`DocumentManagementStorageUsage.UsageAsync`): dopo il
ricalcolo il totale coincide con lo spazio occupato del widget "Spazio documenti". Durante il giorno lo spazio
occupato si aggiorna a ogni caricamento o cancellazione e può differire di poco.

## 4. File

| Livello | File |
|---|---|
| Servizio | `nvxapp.server.service/ClientServer-Service/DocumentManagement/Doc_WidgetService/Doc_WidgetService.cs` (+ `Models/`) |
| Controller | `nvxapp.server/Controllers/DocumentManagement/Doc_WidgetController.cs` |
| Client, API | `ClientServer-Service/DocumentManagement/doc-widget.service.ts`, `Models/doc-widget-model.ts` |
| Client, widget | `shared/components/DocumentManagement/widgets/` (`doc-storage-widget`, `doc-type-space-widget`, `doc-widget-format.ts`) |
| Catalogo | `Utility/infrastructure/widget-document-management.service.ts` |
| Traduzioni | `assets/i18n/document-management/{it,en}.json`, sezione `documents.widgets` |

## 5. Sviluppi possibili

- Widget per l'utente semplice: "I miei documenti da leggere" (presa visione richiesta e non ancora data).
- Documenti in scadenza nei prossimi 30 giorni (tipologie con scadenza).
