# Analisi – Export regole verso paghe con AI

Data: 29/10/2026 — ramo di analisi: `AttendanceTracking_Dev`
Stato: **solo analisi, nessuna modifica al codice**.

---

## 0. Sintesi e decisioni

### Decisioni prese (29/10/2026)

| # | Decisione | Esito |
|---|-----------|-------|
| D1 | File intero o ricetta | **Ricetta del formato** (§6.3): l'AI produce la specifica, il server scrive il file. L'opzione A (file intero) non si fa |
| D2 | Matricola e codice fiscale | **Si aggiungono** a `Dip_Anagrafica` (§10) |
| D3 | Unità Importo | **Nuovo Tipo (Orario / Importo) sulle causali**, e di conseguenza sui giustificativi; si adeguano tutti i punti in cui si inseriscono valori (§11) |
| D4 | Formati di output | Testo (TXT, CSV, tracciato fisso, XML, JSON) + tabella (XLSX con ClosedXML). PDF/DOC binari no |
| D5 | Dati personali al provider | Con la ricetta non si inviano: all'AI vanno documentazione e un esempio sintetico dei metadati |
| D6 | `Par_Export_Verso` | **Si implementa** (§9.5): colonna `Verso` su `Par_ExportCau_Causali`, il valore si somma o si sottrae nel codice di output |

### Decisioni ancora aperte

Nessuna. Comportamento scelto in assenza di indicazioni: totale negativo per effetto del Verso esportato **così com'è** (con il segno), con un avviso nel messaggio di fine job (§9.5).

---

## 1. Situazione attuale

### 1.1 Tabelle

- `Par_ExportCau` (`nvxapp.server.data/Entities/Tenant/GestionePresenze/Par_ExportCau.cs`): `IdAz_Anagrafica`, `Codice`, `Descrizione`, **`TipoFile`** (enum `Par_Export_TipoFile { CSV, TXT }`).
- `Par_ExportCau_Causali`: `IdPar_ExportCau`, `IdCausale` (→ `Par_Causali`), `Codice` (codice paghe), `TipoElaborazione` (`Gionaliera`, `Mensile` — refuso nel nome), `TipoUnita` (`Ore`, `Giorni`, `Importo`).
- L'enum `Par_Export_Verso { Sommatoria, Sottrazione }` è definito ma **non usato**.

### 1.2 Export attuale (`TimeSheet_ExportService.Export`)

1. Job in background (`RunInBackground`), notifiche `LongJobProgressAsync`, categoria `FileGeneration`.
2. `Get_Timesheet_AllData(Dal, Al, UsersId)` → anagrafiche, rapporti, `Dip_GG_Causali`.
3. `PrepareExportRows`: per dipendente e per causale del modello:
   - giornaliera: **una riga per ogni `Dip_GG_Causali`**, senza sommare;
   - mensile: somma per (anno, mese) **della singola causale di origine**.
4. Scrive CSV o TXT in base a `TipoFile` in `NVXSystem.ExportFolder` (`%TEMP%/nvxapp_exports`) e manda `DownloadUrl = "Account/Download/{file}"`.

Problemi già presenti (da correggere con questo lavoro):

- **c.4 non rispettato**: se due causali presenze sono mappate sullo **stesso codice paghe** escono righe distinte (anche in mensile, perché il raggruppamento è per causale di origine e non per codice di output).
- `await Task.Delay(10000)` prima della notifica finale (residuo di prova): 10 secondi di attesa inutili.
- **Nessun controllo di ruolo** in `Export`: gli `SelectedUserId` arrivano dal client e non vengono filtrati. `ExportPdf_4Admin` invece filtra (`HaRuolo`, `DipendentiAmministrati`): stesso schema da applicare.
- `ConvertValore` per `Importo` restituisce ore (non esiste un dato importo).

### 1.3 Infrastruttura job e download

| Pezzo | File | Comportamento |
|-------|------|---------------|
| Notifica job | `nvxapp.server.service/ClientServer-Service/infrastructure/Notifications/LongJobNotifier.cs` | `LongJobProgressUpdate { JobId, JobType, Payload, Category, ProgressPercentage, Message, IsFinished, DownloadUrl }` |
| Cartella | `nvxapp.server.service/Helpers/Sistema.cs` | `NVXSystem.ExportFolder`, `NVXSystem.DownloadURL` |
| Download | `nvxapp.server/Controllers/Infrastructure/Account.cs` → `Download/{fileName}` | **Content-Type fisso**: `.csv` → `text/csv`, tutto il resto → `text/plain` |
| Client | `nvxapp.client/src/nvx/shared/components/infrastructure/page-toolbar/page-toolbar.component.ts` → `onJobLabelClick` | GET blob, nome file = ultimo pezzo dell'URL |

Osservazioni:

- Il Content-Type fisso è **già sbagliato oggi** per i PDF (cartellini, report) che partono come `text/plain`; funziona solo perché il client salva il blob con l'estensione presa dall'URL.
- `fileName` arriva dalla rotta e va in `Path.Combine` senza controllo: va limitato a un nome semplice (`Path.GetFileName(fileName) == fileName`), altrimenti è possibile uscire dalla cartella (es. `..%5C` su Windows).
- Nessun controllo che il file sia del richiedente (basta conoscere il nome; il GUID del job lo rende difficile da indovinare, ma non impossibile da ottenere).
- I file in `nvxapp_exports` **non vengono mai cancellati**.
- Il nome scaricato è `{codice}_{guid}.ext`: brutto per l'utente.

### 1.4 AI disponibile

- `ILlmClient.CompleteJsonAsync(systemPrompt, userMessage, schema, schemaName, LlmImage? image)` (`infrastructure/ChatAI/LlmClient.cs`): una richiesta, **risposta testo JSON**, `temperature = 0`, al massimo **un'immagine**, nessun `max_tokens`, nessun file allegato.
- Provider configurati (`appsettings.json`, sezione `AI`): DeepSeek (in uso), Groq, OpenRouter, Ollama locale. Sono API chat compatibili OpenAI: **ricevono e producono testo**, non file.
- `ImportFileText.Estrai(nomeFile, byte[])` (DipendentiImportService) estrae già il testo da PDF (PdfPig), DOCX, ODT, XLSX (ClosedXML) e testo: **riusabile** per la documentazione.
- Limiti pratici: `AI:ImportMaxChars` (12000) in ingresso per i provider gratuiti; in uscita i modelli chat danno tipicamente 4–8K token (DeepSeek chat: 8K max) → poche centinaia di righe di file.

### 1.5 Precedente per gli allegati

`Dip_GG_NotaSpesa_Allegato`: file salvato **nel database** (`byte[] Contenuto`, `NomeFile`, `ContentType`, `Dimensione`), upload in base64 dal client (`FileReader.readAsDataURL`), apertura in una nuova scheda (`DipGGNotaSpesaService.ApriAllegato`: blob + `window.open`), limiti in costanti del servizio (5 MB per file, tipi ammessi). È il modello da seguire per `Par_ExportCau_Docs`.

---

## 2. (a) Tabelle

### a.1 – Eliminare `Par_ExportCau.TipoFile`

Da togliere/modificare:

| Livello | File |
|---------|------|
| Entità + enum `Par_Export_TipoFile` | `nvxapp.server.data/Entities/Tenant/GestionePresenze/Par_ExportCau.cs` |
| Migrazione | nuova migrazione AttendanceTracking (`DropColumn TipoFile`) |
| Modello server | `Par_ExportCauService/Models/Par_ExportCauModel.cs` |
| Default nuovo record | `Par_ExportCauService.cs` (Get, record "Nuovo Export") |
| Parametri di default | `GestionePresenze/_utility/GestionePresenzeParametriDefault.cs` (riga ~261) |
| Export | `TimeSheet_ExportService.cs` (scelta CSV/TXT → sostituita dal flusso §3) |
| Modello client | `ClientServer-Service/GestionePresenze/Par_ExportCau/Models/par-export-cau-model.ts` |
| Edit | `export-cau-edit-page.component.html/.ts` (select + validatore) |
| Lista | `export-cau-list-page.component.html/.ts` (colonna `tipoFile`) |
| Pipe | `shared/pipe/GestionePresenze/par-export-tipo-file-to-short-text.pipe.ts` + registrazione in `shared-component-gestione-presenze-module.module.ts` |
| i18n | `assets/i18n/attendance-tracking/it.json`, `en.json`: `tipoFile`, `tipoFileObbligatorio`, `columnFileType` (lasciare `erroreTipoFile`, è della nota spese) |

### a.2 – Nuova tabella `Par_ExportCau_Docs`

```csharp
// Documento che descrive il formato del file da produrre per le paghe (tracciato, manuale, esempio):
// si manda all'AI insieme ai dati. Il file e' salvato nel database.
public class Par_ExportCau_Docs : BaseEntity
{
    [Required] public required int IdPar_ExportCau { get; set; }
    [ForeignKey("IdPar_ExportCau")] public virtual Par_ExportCau? Par_ExportCauNavigation { get; set; }

    [Required, MaxLength(255)] public string NomeFile { get; set; } = string.Empty;
    [Required, MaxLength(100)] public string ContentType { get; set; } = string.Empty;
    public int Dimensione { get; set; }                       // byte
    [Required] public byte[] Contenuto { get; set; } = Array.Empty<byte>();

    // testo estratto al caricamento (ImportFileText): e' quello che va all'AI e che si mostra per DOC/XLSX
    public string? Testo { get; set; }

    [MaxLength(200)] public string? Descrizione { get; set; } // es. "tracciato record", "esempio file"
    public int Ordine { get; set; }                           // ordine di invio all'AI
}
```

- `Par_ExportCau`: `ICollection<Par_ExportCau_Docs>? Par_ExportCau_Docs`.
- `AttendanceTrackingDbContext`: `DbSet` + relazione con `OnDelete(Cascade)` come `Par_ExportCau_Causali`.
- Repository `IPar_ExportCau_DocsRepository`, mapper, registrazione DI come le altre tabelle (vedi `GENERAZIONE_TABELLE_GUIDA_COMPLETA.md`).
- **Testo estratto salvato al caricamento**: si estrae una volta sola, si sa subito se il file è leggibile (altrimenti errore all'utente), si conosce la lunghezza da mandare all'AI.
- Limiti proposti: 5 MB per file, 10 documenti per modello, tipi ammessi: PDF, DOCX, ODT, XLSX, TXT, CSV, XML, JSON, MD. Le immagini si potrebbero aggiungere (passano dal modello con visione) ma `LlmClient` oggi ne accetta una sola: fuori dalla prima versione.
- Proposta aggiuntiva su `Par_ExportCau`: campo `NoteAI` (max 1000) per le indicazioni libere dell'amministratore all'AI ("usa il separatore ;", "codifica ANSI"...), come le note di `DipendentiImportService`.

### a.3 – Sezione "Documentazione" in `export-cau-edit-page`

- Terzo `ion-segment-button` (`SEGMENT_sez3`) accanto a *Generale* e *Causali*.
- Elenco documenti: nome, descrizione, dimensione, data; pulsanti **Vedi**, **Scarica**, **Elimina**; pulsante **Carica** (`<input type="file" hidden multiple [accept]>` come la nota spese).
- **Vedi il contenuto**:
  - PDF, TXT, CSV, XML, JSON → nuova scheda con il blob e il `ContentType` (come `ApriAllegato`): il browser li mostra;
  - DOCX, ODT, XLSX → il browser non li mostra: modale con il **testo estratto** (`Testo`, in `<pre>`), cioè esattamente quello che l'AI riceverà. Utile anche per PDF scansionati: se il testo è vuoto l'utente capisce che l'AI non leggerà nulla.
- Salvataggio: endpoint propri, **non** dentro `Par_ExportCauPut`:
  - `Par_ExportCau_Docs_GetAll(IdPar_ExportCau)` → elenco senza contenuto;
  - `Par_ExportCau_Docs_Get(Id)` → contenuto base64 + testo;
  - `Par_ExportCau_Docs_Put` (upload base64, estrazione testo, controlli di dimensione/tipo);
  - `Par_ExportCau_Docs_Delete(Id)`.
  Così i file (anche di MB) non viaggiano a ogni salvataggio del modello. Per un modello nuovo (`Id = 0`) la sezione è disabilitata con il messaggio "salvare prima il modello".
- Controller `Par_ExportCau_DocsController` (`[Authorize]`), solo amministratori dell'azienda come il resto dei parametri.
- Servizio client `par-export-cau-docs.service.ts` + modelli; chiavi i18n in `it.json`/`en.json` (`attendance.pages.exportCauEdit.documentazione`, …).

---

## 3. (b, d) Flusso dell'export

```
Export (job in background)
 ├─ 0%   controlli: ruolo, dipendenti amministrati, modello export, periodo
 ├─ 5–60% b.1  preparazione metadati JSON (§4), un dipendente alla volta, notifica avanzamento
 ├─      modello SENZA documentazione ─────────────► file .json dei metadati ──► 100% download
 └─      modello CON documentazione
         ├─ 65%  b.2  invio all'AI: istruzioni + documentazione (testo estratto) + NoteAI + metadati (o esempio, §6)
         ├─ 85%  b.3  risposta dell'AI → validazione → scrittura file (estensione scelta dall'AI)
         └─ 100% download
```

- Il **tipo di file non è più noto a priori**: lo decide l'AI (o il fallback JSON). Il servizio scrive il file con l'estensione restituita, controllata contro una lista ammessa (§5.1).
- Errori AI (`LlmBusyException`, risposta non valida, JSON non conforme): il job termina con messaggio di errore chiaro e, **in aggiunta, il JSON dei metadati scaricabile** (l'utente non perde il lavoro di preparazione). Da decidere se il fallback è automatico o solo segnalato.
- Nome del file da scaricare: `{CodiceExport}_{Dal:yyyyMMdd}-{Al:yyyyMMdd}.{ext}` (nome per l'utente) separato dal nome su disco (`{jobId}.{ext}`), vedi §5.2.
- Togliere il `Task.Delay(10000)`.
- Nuovo `JobType` (es. `GestionePresenze_JobType.TimeSheet_Engine_ExportAI`) oppure riuso di `TimeSheet_Engine_Export`: basta il riuso, il client non distingue.

---

## 4. (c) Metadati JSON

### 4.1 Struttura proposta

```json
{
  "versione": 1,
  "generatoIl": "2026-10-29T10:15:00",
  "azienda": { "descrizione": "ACME srl", "codice": "..." },
  "modelloExport": { "codice": "ZUC01", "descrizione": "Paghe Zucchetti" },
  "periodo": { "dal": "2026-10-01", "al": "2026-10-31" },
  "causaliEsportate": [
    { "codice": "ORD", "tipoElaborazione": "Mensile",     "tipoUnita": "Ore" },
    { "codice": "FER", "tipoElaborazione": "Giornaliera", "tipoUnita": "Giorni" }
  ],
  "dipendenti": [
    {
      "anagrafica": {
        "idDipendente": 123,
        "cognome": "Rossi", "nome": "Mario",
        "email": "...", "username": "...",
        "matricola": null, "codiceFiscale": null,
        "rapporto": {
          "dataAssunzione": "2020-01-01", "dataLicenziamento": null,
          "tipoContratto": "FullTime", "oreSettimanaliContratto": 40,
          "ccnl": "...", "sede": "...", "reparto": "..."
        }
      },
      "causali": [
        { "chiave": "ORD|2026-10",    "codice": "ORD", "periodo": "2026-10",    "tipoElaborazione": "Mensile",     "tipoUnita": "Ore",     "valore": 168.50 },
        { "chiave": "FER|2026-10-03", "codice": "FER", "periodo": "2026-10-03", "tipoElaborazione": "Giornaliera", "tipoUnita": "Giorni",  "valore": 1 },
        { "chiave": "TRA|2026-10",    "codice": "TRA", "periodo": "2026-10",    "tipoElaborazione": "Mensile",     "tipoUnita": "Importo", "valore": 125.40 }
      ]
    }
  ]
}
```

Deciso (29/10/2026): **elenco con campo `chiave`**; **dipendenti senza causali esportate nel periodo esclusi**; ore e importi **arrotondati a 2 decimali** (importi in euro), separatore decimale punto.

- **c.1** – un unico file con tutti i dipendenti selezionati (filtrati per i reparti amministrati se l'utente è `CompanyAdmin`, come `ExportPdf_4Admin`).
- **c.2** – "tutti i dati anagrafici" **oggi disponibili**: `Dip_Anagrafica` (cognome, nome), `ApplicationUser` (email, username), `Dip_RapportoLavoro` (assunzione, licenziamento, tipo contratto, ore settimanali, CCNL), sede/reparto. **Mancano matricola e codice fiscale** (D2): nel JSON si prevedono già, a `null` finché non esistono.
- Più rapporti di lavoro nello stesso periodo: `rapporto` diventa un array; le causali si sommano comunque per dipendente (già oggi si usano tutti gli `IdDip_RapportoLavoro` del dipendente).
- **c.3 chiave** = `{codiceOutput}|{periodo}` con periodo `yyyy-MM-dd` (giornaliera) o `yyyy-MM` (mensile), in un **elenco** di oggetti con campo `chiave` (univoca per dipendente); ogni elemento ripete codice e periodo così chi lo legge non deve spezzare la chiave.
- Dipendenti senza causali esportate nel periodo: **non inclusi**.

### 4.2 Regole di calcolo (c.3, c.4)

1. Si leggono le `Dip_GG_Causali` del periodo per i dipendenti selezionati.
2. Si tengono **solo** quelle la cui `IdPar_Causali` è presente in `Par_ExportCau_Causali` del modello; le altre si ignorano.
3. Ogni causale prende il **codice di output** (`Par_ExportCau_Causali.Codice`), il `TipoElaborazione` e il `TipoUnita` della riga del modello.
4. Raggruppamento per **(dipendente, codice di output, periodo)** dove periodo = giorno o mese secondo il `TipoElaborazione`; **somma** dei valori convertiti nell'unità, ciascuno con il segno del `Verso` della riga del modello (Sommatoria +, Sottrazione −).
   - Ore: ore decimali arrotondate a **2 decimali** (es. 7.50). Si somma in minuti e si arrotonda una volta sola sul totale, per non accumulare errori.
   - Giorni: 1 per ogni giorno con valore > 0; in mensile la somma = numero di giorni.
   - Importo: somma di `Dip_GG_Causali.Importo`, euro a **2 decimali** (§11).
5. Mensile con periodo che non copre il mese intero (es. dal 15/10 al 14/11): due chiavi, `2026-10` e `2026-11`, ognuna con la parte nel periodo.

**Vincolo da aggiungere in `Par_ExportCau_Causali_PutAll_4Edit`**: righe con lo stesso `Codice` di output devono avere lo stesso `TipoElaborazione` e lo stesso `TipoUnita`, altrimenti la somma non ha senso (ore + giorni) e la chiave sarebbe ambigua. Messaggio di errore all'utente in salvataggio.

`Par_Export_Verso` (Sommatoria/Sottrazione): **si implementa** come colonna `Verso` di `Par_ExportCau_Causali`, vedi §9.5.

### 4.3 Codice

- Nuovo metodo `PrepareExportMetadati(...)` al posto di `PrepareExportRows`, con classi modello dedicate in `TimeSheet_ExportService/Models` (serializzazione `System.Text.Json`, camelCase).
- Il calcolo del raggruppamento è puro (input → output): metterlo in una classe statica testabile con unit test (casi: due causali sullo stesso codice, giornaliera/mensile, periodo a cavallo di due mesi, dipendente senza causali).

---

## 5. Infrastruttura: cosa cambiare (su `Infrastructure_Dev`)

> Da fare su **`Infrastructure_Dev`** e poi catena di merge `Infrastructure_Dev → Infrastructure → AttendanceTracking_Dev → AttendanceTracking`. Il resto (tabelle, servizio export, pagina edit) su `AttendanceTracking_Dev`.

### 5.1 Scrittura dei file generati — `nvxapp.server.service/Helpers/Sistema.cs`

Nuovo helper unico per tutti i job che producono file (export, cartellini, report):

```csharp
// salva il file del job e restituisce l'URL di download; l'estensione deve essere tra quelle ammesse
public static string SalvaFileJob(string jobId, string estensione, byte[] contenuto)
```

- Lista di estensioni ammesse (`csv, txt, json, xml, xlsx, pdf, dat, prn, …`): l'estensione proposta dall'AI passa da qui, mai usata così com'è.
- Nome su disco = `{jobId}.{ext}` (niente testo che arriva da utente/AI nel percorso).

### 5.2 Download — `nvxapp.server/Controllers/Infrastructure/Account.cs`

- **Content-Type dall'estensione** con `FileExtensionContentTypeProvider` (risolve anche il bug dei PDF serviti come `text/plain`).
- Controllo del nome: `Path.GetFileName(fileName) == fileName` e solo caratteri ammessi, altrimenti 400.
- Nome per l'utente: o un file `{jobId}.meta.json` accanto (nome visibile, utente proprietario, scadenza) o un piccolo registro in memoria `jobId → {owner, nomeVisibile}`. Con il proprietario si controlla anche che chi scarica sia chi ha lanciato il job (oggi manca).
- `Content-Disposition` con il nome visibile.

### 5.3 Notifica job — `LongJobNotifier.cs` + `long-job-notifier.service.ts`

- Aggiungere a `LongJobProgressUpdate` il campo opzionale **`FileName`** (nome visibile, con estensione): il client lo usa per `a.download` invece di prendere l'ultimo pezzo dell'URL. Non serve conoscere il tipo prima: arriva con la notifica di fine job.
- In alternativa leggere `Content-Disposition` nel client (`observe: 'response'`), ma richiede di esporre l'header in CORS: il campo nella notifica è più semplice.
- `page-toolbar.component.ts` → `onJobLabelClick`: `a.download = job.fileName || downloadUrl.split('/').pop() || 'export'`.
- Compatibile all'indietro: i job esistenti senza `FileName` funzionano come oggi.

### 5.4 Pulizia della cartella export

Servizio in background (o al primo uso del giorno) che cancella da `nvxapp_exports` i file più vecchi di N ore (`Export:RetentionHours`, proposto 24). Oggi i file restano per sempre, e con l'AI conterranno dati paghe.

### 5.5 Client AI — `infrastructure/ChatAI/LlmClient.cs`

`CompleteJsonAsync` basta per la **ricetta** (§6, risposta JSON piccola). Da aggiungere comunque:

- parametro opzionale **`maxTokens`** (oggi non passato: il provider usa il suo default, per DeepSeek 4K);
- **provider dedicato** opzionale `AI:ExportLLM` (come `AI:VisionLLM`): l'export può volere un modello con contesto/uscita più ampi della chat;
- **timeout**: le chiamate passano da `WebApiService` (HttpClient da factory, default 100 s); la generazione può superarlo. Timeout configurabile per la chiamata (`AI:TimeoutSeconds`).
- Limite del testo di documentazione: `AI:ExportMaxChars` (come `ImportMaxChars`), con avviso all'utente se troncato.

Se si sceglie il **file intero** (D1) servono in più: risposta molto più lunga (vedi limiti §1.4) e, per molti dipendenti, l'invio a blocchi con ricomposizione del file (intestazione/piede/totali diventano un problema).

### 5.6 Estrazione testo

`ImportFileText` è in `GestionePresenze/DipendentiImportService`: l'export è sempre GestionePresenze, quindi **si può usare dov'è**, nessuno spostamento obbligatorio. Spostarlo in `Helpers` (infrastruttura) solo se servirà ad altri argomenti.

---

## 6. Come far produrre il file all'AI (D1)

### 6.1 Perché l'AI non può "restituire un file" qualsiasi

I provider configurati sono modelli **chat**: restituiscono testo. Quindi:

- formati **testo** (TXT, CSV, tracciato a larghezza fissa, XML, JSON): l'AI può scriverli;
- **XLSX**: l'AI restituisce righe/colonne in JSON, il server scrive il file con ClosedXML (già nel progetto);
- **PDF/DOC binari**: non generabili in modo affidabile (base64 scritto da un LLM si corrompe). Per le paghe non servono; se servisse un PDF di riepilogo lo fa il server con MigraDoc come i cartellini.

### 6.2 Opzione A – L'AI scrive il file intero

Messaggio all'AI: documentazione + note + **metadati completi**. Risposta (JSON):

```json
{ "estensione": "txt", "mimeType": "text/plain", "codifica": "utf-8", "tipo": "testo",
  "contenuto": "....", "avvisi": ["causale XYZ non prevista dal tracciato"] }
```

`tipo = "tabella"` → `contenuto` è `[{ "foglio": "...", "righe": [[...]] }]` e il server scrive l'XLSX.

Contro:
- **limite di uscita** (§1.4): oltre qualche decina di dipendenti il file viene troncato;
- **numeri non garantiti**: l'LLM può sbagliare/riformattare valori di paga (temperature 0 riduce, non elimina) e non c'è modo generale di verificarlo;
- **dati personali** di tutti i dipendenti al provider esterno a ogni export;
- costo e tempi crescono con i dipendenti.

### 6.3 Opzione B – L'AI scrive la "ricetta", il server il file (proposta)

1. L'AI riceve documentazione + note + **un esempio sintetico** dei metadati (struttura reale, 1–2 dipendenti finti) e restituisce una **specifica di formattazione** JSON, ad esempio:

```json
{
  "estensione": "txt", "codifica": "windows-1252", "fineRiga": "CRLF", "tipo": "testo",
  "intestazione": [ { "campo": "costante", "valore": "H" }, { "campo": "periodo.dal", "formato": "ddMMyyyy" } ],
  "rigaPer": "causale",
  "campi": [
    { "campo": "anagrafica.matricola", "lunghezza": 8, "allinea": "destra", "riempi": "0" },
    { "campo": "causale.codice",       "lunghezza": 4 },
    { "campo": "causale.periodo",      "formato": "ddMMyyyy" },
    { "campo": "causale.valore",       "lunghezza": 7, "decimali": 2, "separatoreDecimali": "", "riempi": "0" }
  ],
  "separatore": null,
  "piede": [],
  "avvisi": []
}
```

2. Un **renderer deterministico** sul server applica la ricetta ai metadati reali: riga per dipendente o per causale, campi a larghezza fissa o separati, formati data/numero, intestazione e piede, codifica; `tipo = "tabella"` → XLSX con ClosedXML.
3. La ricetta si **salva** su `Par_ExportCau` (`RicettaAI` json + hash della documentazione/note): si rigenera solo quando cambiano documenti o note. L'amministratore può vederla e provarla ("anteprima" su un dipendente) dalla pagina del modello.

Pro: numeri esatti, nessun limite di dipendenti, nessun dato personale all'AI, una chiamata AI per modello invece che per export, risultato ripetibile.
Contro: il renderer copre i tracciati "a righe" (la grande maggioranza dei formati paghe); un formato molto particolare che la ricetta non sa esprimere richiederebbe di estenderla.

**Deciso (D1)**: si implementa B. A non si fa; se un formato non fosse esprimibile con la ricetta, si estende la ricetta.

---

## 7. Sicurezza e privacy

- Ruoli: `Export` solo per `CompanyAdmin`/`CompanyPowerAdmin`, dipendenti filtrati per reparti amministrati (oggi manca).
- Documentazione caricata = **testo non fidato** messo nel prompt: va racchiusa tra delimitatori come in `DipendentiImportService` (`<<<DOC ... DOC>>>`) e il prompt di sistema dice di trattarla solo come specifica del formato. Con l'opzione B la risposta è comunque solo una ricetta validata, non codice eseguito.
- Validazione della risposta AI: schema JSON, estensione in lista ammessa, lunghezze massime, campi riferiti solo a quelli esistenti nei metadati.
- Dati personali verso provider esterni: con B nessuno; con A informare l'azienda (DPA/GDPR), almeno con un avviso nella pagina.
- Download: controlli §5.2, pulizia §5.4.

---

## 8. Piano di lavoro

La lista numerata dei lavori è al **§12** (checklist).

**Prove**: modello senza documenti (JSON), con un tracciato TXT a larghezza fissa, con un CSV, con un XLSX; due causali sullo stesso codice; periodo a cavallo di due mesi; CompanyAdmin con dipendenti fuori dai suoi reparti; AI non raggiungibile/429.

---

## 9. Domande aperte

1. ~~D1~~ → ricetta. ~~D2~~ → matricola e CF sì. ~~D3~~ → Tipo su causali e giustificativi (§11).
2. ~~Chiavi JSON~~ → elenco con campo `chiave`.
3. ~~Dipendenti senza causali~~ → esclusi.
4. ~~Arrotondamento~~ → 2 decimali (ore e importi), separatore punto.
5. **`Par_Export_Verso`: cos'è.** È un enum già scritto in `Par_ExportCau_Causali.cs` (`Sommatoria`, `Sottrazione`) ma **non usato da nessuna parte**: nessuna colonna, nessuna pagina, nessun calcolo. Chi l'ha scritto pensava probabilmente a questo:

   Nel modello di export ogni riga dice "la causale presenze X va nel codice paghe Y". Se più righe hanno lo **stesso codice paghe**, i valori si **sommano** (c.4). Il Verso permetterebbe di dire, riga per riga, se il valore va **sommato** o **sottratto**.

   Esempio. Modello di export con tre righe:

   | Causale presenze | Codice paghe | Verso |
   |------------------|--------------|-------|
   | Ore ordinarie    | `ORE`        | Sommatoria |
   | Straordinario    | `ORE`        | Sommatoria |
   | Permesso non retribuito | `ORE` | **Sottrazione** |

   Dipendente nel mese: ordinarie 160, straordinario 10, permesso non retribuito 8.
   - **Oggi (senza Verso)**: `ORE` = 160 + 10 + 8 = **178**
   - **Con il Verso**: `ORE` = 160 + 10 − 8 = **162**

   Serve solo se le paghe vogliono un codice che è una **differenza** tra causali. Se ogni causale va su un suo codice (il caso normale: `ORD`, `STR`, `PNR` separati) e il calcolo lo fa il programma paghe, **non serve**.

   **Deciso (29/10/2026): si implementa.**
   - Colonna `Par_ExportCau_Causali.Verso` (`Par_Export_Verso`, default `Sommatoria`: i modelli esistenti non cambiano risultato) + migrazione.
   - Select *Verso* nel dialog delle causali dell'export (`edit-par-export-cau-causali-component`) e colonna/segno nell'elenco della sezione *Causali* di `export-cau-edit-page`; modelli server/client, i18n.
   - Raggruppamento (§4.2 punto 4): valore con segno.
   - Totale negativo: esportato con il segno (es. `-4.00`) e segnalato nel messaggio di fine job ("codice ORE negativo per N dipendenti"); la ricetta deve prevedere come scrivere il segno (§6.3, campo `segno`: `-` iniziale, `+/-` in colonna dedicata, ecc.).
   - Coerenza: righe con lo stesso codice di output possono avere Verso diversi (è lo scopo); restano obbligatori stesso tipo elaborazione e stessa unità.

---

## 10. (D2) Matricola e codice fiscale

### 10.1 Dati

`Dip_Anagrafica` (`nvxapp.server.data/Entities/Tenant/GestionePresenze/Dip_Anagrafica.cs`):

```csharp
[MaxLength(20)] public string? Matricola { get; set; }       // matricola paghe
[MaxLength(16)] public string? CodiceFiscale { get; set; }
```

- Entrambi **facoltativi** a database (i dipendenti esistenti non li hanno). L'export segnala nel messaggio finale i dipendenti senza matricola/CF; la ricetta può comunque usarli (campo vuoto).
- **Matricola univoca per azienda** (indice su `IdAz_Anagrafica` della company + `Matricola` dove non nulla, o controllo nel servizio: `Dip_Anagrafica` non ha `IdAz_Anagrafica` diretto, il controllo va fatto nel servizio tramite i dipendenti dell'azienda).
- **Codice fiscale**: maiuscolo, 16 caratteri, controllo del carattere di controllo (persone fisiche); 11 cifre accettate solo se si vuole gestire il CF numerico provvisorio. Univoco per azienda.
- Migrazione AttendanceTracking (`AddColumn` ×2).

### 10.2 Punti da adeguare

| Punto | File |
|-------|------|
| Modelli e mapper anagrafica | `Dip_AnagraficaService/Models`, mapper `Dip_Anagrafica_AND_...` |
| Salvataggio + controlli (univocità, CF) | `Dip_AnagraficaService` (`Dip_AnagraficaPut`) |
| Modifica dipendente | `pages/GestionePresenze/user-department-edit-page` |
| Wizard nuovo dipendente | `pages/GestionePresenze/user-department-wizard-page`, `shared/components/GestionePresenze/user-attendance-wizard-dialog` |
| Importazione da file | `DipendentiImportService` (schema e prompt AI: estrarre anche matricola e CF, con la stessa regola anti-allucinazione "solo se presenti nel testo"), `user-attendance-import-dialog` |
| Metadati export | `anagrafica.matricola`, `anagrafica.codiceFiscale` (§4.1) |
| Selettori/ricerche (facoltativo) | `dip-selector-modal`: ricerca anche per matricola |
| i18n | `it.json`, `en.json` |

---

## 11. (D3) Tipo della causale: Orario / Importo

### 11.1 Come nascono oggi i valori delle causali

Dal codice risulta che **le causali del giorno (`Dip_GG_Causali`) sono calcolate dal motore** (`TimeSheet_EngineService`):

- da **giustificativi** approvati/diretti: `Par_Giustificativi.IdCausale` ← `Dip_GG_Giustificativi.Hours` (riga ~1999–2016);
- da **ore lavorate** per fascia (`Par_OrarioIntervalloHH.IdCausale_HH_Lav`, `Par_Orario.IdCausale_HH_Lav_MonteOre`);
- da **eccedenze** (straordinario, notturno, festivo, supplementare, banca ore) con le causali del profilo (`Par_ProfiloOrario.IdCausale_*`).

`ScriviCausali` (riga ~2209) segna come da cancellare **tutte** le causali del giorno non prodotte dal calcolo. Quindi una causale inserita a mano (dialog `edit-dip-gg-causali-dialog` o chat `RegisterCausaleHandler`) viene salvata e poi, al ricalcolo del giorno che parte subito dopo (`Dip_GG_CausaliPut` → `CalculateGiorno`), **cancellata**. Da confermare con una prova, ma è il comportamento che si legge nel codice; va deciso prima di introdurre causali a importo inserite a mano (§11.7, domanda 1).

Conseguenza per il Tipo: un importo non può nascere da ore lavorate né da eccedenze; può nascere **da un giustificativo a importo** (la strada indicata: "stessa cosa nei giustificativi") ed eventualmente da inserimento manuale se si risolve il punto sopra.

### 11.2 Modello dati

```csharp
public enum TipoValore { Orario = 0, Importo = 1 }   // in un file condiviso di GestionePresenze
```

| Tabella | Modifica |
|---------|----------|
| `Par_Causali` | `TipoValore Tipo` (default `Orario`: i dati esistenti restano validi) |
| `Par_Giustificativi` | `TipoValore Tipo` (default `Orario`) |
| `Dip_GG_Giustificativi` | `decimal? Importo` (precisione 18,2), usato se il giustificativo è a importo; `Hours` resta per l'orario |
| `Dip_GG_Causali` | `decimal? Importo` (18,2); `Valore` (`TimeOnly`) resta per l'orario. In alternativa sostituire `Valore` con un unico `decimal` (minuti o importo): più pulito ma tocca tutto ciò che legge `Valore` — **proposta: aggiungere `Importo`** |
| `Par_ExportCau_Causali` | `TipoUnita` va coerente con il tipo della causale: `Importo` solo per causali a importo, `Ore`/`Giorni` solo per orarie |

Migrazione AttendanceTracking con default `Orario` / `null`.

### 11.3 Regole di coerenza (da controllare in salvataggio)

1. **Giustificativo a importo** → la causale collegata (`IdCausale`) deve essere a importo, e viceversa (un giustificativo orario non può puntare a una causale a importo).
2. Giustificativo a importo: niente effetti sulle ore → `Segno = Neutral`, `TipoContatore = NoContatore`, `BancaOre = false`, `AutorizzaStraordinario = false`, `VisualizzaInPianoFerie = false`; `TipoInput` non usato (giornata intera/intervallo non hanno senso per un importo).
3. Causali usate dal **profilo/orario** (`Par_ProfiloOrario.IdCausale_*`, `Par_Orario.IdCausale_HH_Lav_MonteOre`, `Par_OrarioIntervalloHH.IdCausale_HH_Lav`) → solo **orarie**: filtrare i selettori e controllare in salvataggio.
4. Cambio del Tipo di una causale/giustificativo **già usati** (esistono `Dip_GG_*` collegati): bloccarlo, altrimenti i valori storici cambiano significato.
5. Export: `TipoUnita = Importo` solo per causali a importo (§11.2).

### 11.4 Motore (`TimeSheet_EngineService`)

- `causaliAccumulate` oggi è `Dictionary<int, TimeSpan>`: aggiungere un secondo accumulatore `Dictionary<int, decimal>` per gli importi (o una struttura `(TimeSpan Ore, decimal Importo)`).
- Giustificativi a importo: si accumula `Importo` sulla causale; **nessun effetto** su `oreLavorabili`, `oreAssenza`, autorizzazioni, contatori, quadratura.
- `ScriviCausali`: scrive `Valore` per le orarie e `Importo` per quelle a importo; il confronto con le esistenti resta per `IdPar_Causali`.
- `AggiungiCausaliEccedenze`: invariato (solo orarie, garantito dalla regola 3).

### 11.5 Punti di inserimento e visualizzazione da adeguare

**Inserimento**

| Punto | File | Modifica |
|-------|------|----------|
| Parametri causale | `pages/GestionePresenze/causali-edit-page`, `causali-list-page`, `Par_CausaliService` | campo Tipo, colonna in lista, regola 4 |
| Parametri giustificativo | `pages/GestionePresenze/justification-edit-page`, `justification-list-page`, `Par_GiustificativiService` | campo Tipo; con Importo nascondere/forzare i campi della regola 2; selettore causale filtrato per tipo (regola 1) |
| Profilo orario / orario | pagine dei profili e orari, `Par_ProfiloOrarioService`, `Par_OrarioService` | selettori causale solo orarie (regola 3) |
| Giustificativo del giorno (admin) | `shared/components/GestionePresenze/edit-dip-gg-giustificativi-dialog` | con giustificativo a importo: campo importo al posto di ore/dalle/tipo input |
| Richiesta giustificativo (utente) | `pages/GestionePresenze/request-justification-user-page`, `Dip_GG_RichiestaService` | idem; la richiesta e l'approvazione portano l'importo |
| Causale del giorno (manuale) | `shared/components/GestionePresenze/edit-dip-gg-causali-dialog`, `Dip_GG_CausaliService` | ore o importo (uno solo) secondo il tipo; il comportamento del ricalcolo resta com'è (§11.7.1) |
| Chat AI | `ChatAiHandler/GestionePresenze/RegisterJustificationHandler.cs`, `RegisterJustificationPeriodHandler.cs`, `RegisterCausaleHandler.cs` (+ slot e normalizzatori) | slot "importo" in alternativa alle ore; messaggi di conferma con l'importo |
| Servizi | `Dip_GG_GiustificativiService` | controlli: importo > 0 obbligatorio per i giustificativi a importo, ore vuote |

**Visualizzazione / calcoli che leggono ore**

| Punto | File | Modifica |
|-------|------|----------|
| Calendari/timesheet | `time-sheet-user-page`, `time-sheet-admin-page`, `time-sheet-power-admin-page`, `Utility/GestionePresenze/time-sheet.service.ts`, `time-sheet-common-data.ts` | mostrare `€ 12,50` invece di ore per i giustificativi/causali a importo; escluderli dai totali ore |
| Piano ferie | `vacation-plan-page` | escludere i giustificativi a importo (regola 2 li toglie già con `VisualizzaInPianoFerie = false`) |
| Contatori | `ContatoriService` | escludere i giustificativi a importo (regola 2: `NoContatore`) |
| Report | `ReportService` | totali ore senza importi; eventuale colonna importi |
| Cartellino PDF | `TimeSheet_ExportService.PrepareCartellinoPdf` | giustificativo a importo mostrato con l'importo, non con le ore |
| Export | `TimeSheet_ExportService` (nuovo `PrepareExportMetadati`) | `valore` = importo per le causali a importo |
| Chat AI (letture/riepiloghi) | handler che leggono giustificativi/causali | formattazione importo |
| i18n | `it.json`, `en.json`, `AttendanceTrackingMessages` | etichette Tipo, Importo, messaggi di errore delle regole |

### 11.6 Nota spese

La nota spese ha già importi (`Dip_GG_NotaSpesa`, `Par_TipoSpesa`). Se le spese approvate devono andare in paghe (rimborsi), la strada naturale è collegare `Par_TipoSpesa` a una **causale a importo** e farle entrare nelle causali del giorno. **Fuori ambito** salvo indicazione, ma il Tipo Importo lo rende possibile senza altre modifiche al modello.

### 11.7 Decisioni sul Tipo Importo (29/10/2026)

1. **Causali inserite a mano / ricalcolo**: l'export **non interviene** sull'elaborazione: legge le `Dip_GG_Causali` così come sono, non lancia ricalcoli e non modifica dati. Il comportamento del ricalcolo sulle causali manuali (§11.1) resta com'è e non fa parte di questo lavoro.
2. **Un solo importo**: ogni causale e ogni giustificativo a importo prevedono l'inserimento di **un solo importo** per riga (campo `Importo`, al posto di ore/dalle/tipo input). Nessuna gestione di importi multipli nella stessa riga.
3. **Valuta**: sempre euro, 2 decimali (`decimal(18,2)`, input con 2 decimali, visualizzazione `€ 12,50`).
4. **Richiesta del dipendente**: sì, il dipendente può richiedere un giustificativo a importo (es. rimborso), con la stessa richiesta e approvazione degli altri giustificativi.

---

## 12. Lista lavori (checklist)

Numerazione unica, nell'ordine in cui farli. `[ ]` da fare, `[x]` fatto (con nota di cosa è stato fatto).

**Stato per sezione** (aggiornato a ogni lavoro completato)

| Sezione | Contenuto | Lavori | Fatti | Stato |
|---------|-----------|--------|-------|-------|
| 12.1 | Infrastruttura | 1–7 | 7/7 | **completata** (29/10/2026) |
| 12.2 | Anagrafica: matricola e codice fiscale | 8–12 | 5/5 | **completata** (29/10/2026) |
| 12.3 | Tipo Orario / Importo | 13–27 | 15/15 | **completata** (29/10/2026) |
| 12.4 | Modello di export | 28–32 | 0/5 | da iniziare |
| 12.5 | Export | 33–43 | 0/11 | da iniziare |
| **Totale** | | **1–43** | **27/43** | |

### 12.1 Infrastruttura (lavori 1–7) — ramo `Infrastructure_Dev`, poi catena dei merge (§5)

- [x] 1. `NVXSystem.SalvaFileJob(jobId, estensione, contenuto)` con lista di estensioni ammesse; nome su disco `{jobId}.{ext}` (§5.1)
  — Fatto: `SalvaFileJob(jobId, owner, nomeVisibile, estensione, contenuto)` e `RegistraFileJob` (per i file già scritti da un writer, es. PDF) in `Helpers/Sistema.cs`; estensioni ammesse `csv txt tsv json xml xlsx pdf dat prn zip`; jobId deve essere un GUID. Restituiscono `JobFile(DownloadUrl, FileName)`. `MyMokeLongJobService` usa già `SalvaFileJob`.
- [x] 2. `Account/Download`: Content-Type dall'estensione (`FileExtensionContentTypeProvider`), controllo del nome file, `Content-Disposition` (§5.2)
  — Fatto: `NVXSystem.NomeFileJobValido` rifiuta cartelle, `..`, caratteri non validi ed estensioni non ammesse (quindi anche i `.meta`); risposta 404 senza dire il perché. Il nome nel `Content-Disposition` è quello registrato.
- [x] 3. Registro dei file dei job (proprietario + nome visibile) e controllo che scarichi solo chi ha lanciato il job (§5.2)
  — Fatto: file `{file}.meta` (JSON: Owner, FileName, Creato) accanto al file. Owner = utente del login (`useridfirstconnection`, come le notifiche del job, quindi funziona anche in impersonazione). **I file non registrati** (cartellini PDF, report, export attuale: codice dei rami presenze) restano scaricabili come prima: si adeguano al lavoro 43.
- [x] 4. `LongJobProgressUpdate.FileName` lato server e client; `page-toolbar` usa `job.fileName` per il nome scaricato (§5.3)
  — Fatto: senza `fileName` il client usa l'ultimo pezzo dell'URL come prima.
- [x] 5. Pulizia automatica di `nvxapp_exports` (`Export:RetentionHours`, default 24) (§5.4)
  — Fatto: `nvxapp.server/Utility/JobFileCleanupService.cs` (BackgroundService, all'avvio e ogni ora), registrato in `Installers.InstallServices`. Sta nel progetto server perché lo scanner del service registra tutto come Scoped.
- [x] 6. `LlmClient`: parametro `maxTokens`, provider dedicato `AI:ExportLLM`, timeout `AI:TimeoutSeconds`, limite `AI:ExportMaxChars` (§5.5)
  — Fatto: `CompleteJsonAsync(..., provider, maxTokens)` con parametri facoltativi (chiamate esistenti invariate); `max_tokens` / Ollama `num_predict`; timeout da `AI:TimeoutSeconds` (180) con `LlmTimeoutException`; `WebApiService.Post` con `timeout` facoltativo. In appsettings: `AI:ExportLLM` (vuoto), `AI:ExportMaxChars` (30000), `AI:ExportMaxTokens` (4000): li leggerà l'export (lavori 37–39).
- [x] 7. Merge `Infrastructure_Dev → Infrastructure → AttendanceTracking_Dev → AttendanceTracking`
  — Fatto: commit `79ef15cc` su Infrastructure_Dev, merge `d09b596f` (Infrastructure), `e77c3543` (AttendanceTracking_Dev), AttendanceTracking allineato. Nessun conflitto; `dotnet build` e `ng build` ok.

### 12.2 Anagrafica: matricola e codice fiscale (lavori 8–12) — `AttendanceTracking_Dev` (§10)

- [x] 8. `Dip_Anagrafica.Matricola` e `CodiceFiscale` + migrazione
  — Fatto: colonne facoltative (20 e 16 caratteri), migrazione `20261029133216_Dip_Anagrafica_MatricolaCF` (solo due `AddColumn`); si applica all'avvio a tutti gli schemi attivi. Nessun indice univoco a database: in modalità singola lo schema `public` contiene più aziende, l'univocità la controlla il servizio.
- [x] 9. Modelli, mapper e controlli in `Dip_AnagraficaService` (CF valido, univocità per azienda di matricola e CF)
  — Fatto: `_utility/GestionePresenzeCodiceFiscale` (16 caratteri con carattere di controllo, anche omocodia; 11 cifre con controllo partita IVA). `Dip_AnagraficaPut` normalizza (CF maiuscolo senza spazi, vuoti = null) e controlla **prima** di creare/aggiornare l'utente. `MatricolaCodiceFiscaleGiaUsati` (riusato dall'importazione) cerca tra i dipendenti dell'azienda escluso il dipendente stesso. Nuovo endpoint `Dip_Anagrafica/Dip_AnagraficaCheck` per il wizard. Mapper invariati (stessi nomi).
- [x] 10. Pagine: modifica dipendente, wizard, dialog del wizard; ricerca per matricola nel selettore dipendenti
  — Fatto: `user-department-edit-page` e dialog `user-attendance-wizard-dialog` (allo step 1 controllo sul server, come per username/email: l'utente si crea prima dei dati presenze); validatore client `Utility/GestionePresenze/codice-fiscale.ts`; `dip-selector-modal` cerca e mostra la matricola. La pagina `user-department-wizard-page` non è collegata a nessuna rotta: non toccata.
- [x] 11. Importazione da file (`DipendentiImportService`): schema e prompt AI con matricola e CF, regola "solo se presenti nel testo"
  — Fatto: campi `matricola`/`codice_fiscale` nello schema e nel prompt; accettati solo se presenti nel testo (CF cercato senza spazi); in revisione segnalati CF non valido, già usati in azienda, ripetuti nell'elenco; all'inserimento controllo prima dell'avvio del job e, per ogni dipendente, prima di creare l'utente.
- [x] 12. i18n e messaggi
  — Fatto: `AttendanceTrackingMessages` (it/en) `AnagraficaMatricolaTroppoLunga`, `AnagraficaCodiceFiscaleNonValido`, `AnagraficaMatricolaGiaUsata`, `AnagraficaCodiceFiscaleGiaUsato`, `ImportMatricolaRipetuta`, `ImportCodiceFiscaleRipetuto`; chiavi client in `userDepartmentEdit`, `userAttendanceWizardDialog`, `userAttendanceImportDialog`.
  — Prove sul server di prova (5299, azienda di test 1): elenco con i campi nuovi, controllo CF, salvataggio rifiutato per CF non valido e per CF già di un altro dipendente (nessuna scrittura), salvataggio con normalizzazione e ripristino del dato; estrazione AI (DeepSeek) di matricola e CF da un CSV; inserimento da import rifiutato per CF non valido e matricola ripetuta prima di creare utenti. Interfaccia non provata nel browser.

### 12.3 Tipo Orario / Importo su causali e giustificativi (lavori 13–27) — `AttendanceTracking_Dev` (§11)

- [x] 13. Enum `TipoValore`; `Par_Causali.Tipo`, `Par_Giustificativi.Tipo`, `Dip_GG_Giustificativi.Importo`, `Dip_GG_Causali.Importo` (`decimal(18,2)`) + migrazione con default Orario
  — Fatto: enum `TipoValore` (in `Par_Causali.cs`), `Tipo` su `Par_Causali`/`Par_Giustificativi` (default Orario), `Importo` `numeric(10,2)` su `Dip_GG_Giustificativi` e `Dip_GG_Causali` (come la nota spese), migrazione `20261029134812_TipoValore_Importo` (solo `AddColumn`). Campi anche nei modelli server/client, `[HashField]` sugli importi.
- [x] 14. Parametri causale (pagina, lista, servizio): campo Tipo, blocco del cambio se la causale è già usata
  — Fatto: select Tipo e nota nella pagina, colonna nell'elenco; `Par_CausaliPut` rifiuta il cambio di tipo se la causale e' usata (giorni dei dipendenti, giustificativi, profili, orari, fasce, modelli di export).
- [x] 15. Parametri giustificativo: campo Tipo; con Importo campi forzati (segno neutro, niente contatore, banca ore, autorizzazione straordinario, piano ferie); causale collegata dello stesso tipo; blocco del cambio se usato
  — Fatto: select Tipo; con Importo i campi delle ore sono nascosti e forzati (anche dal server), causali filtrate per tipo; `Par_GiustificativiPut` controlla causale dello stesso tipo e blocca il cambio di tipo se il giustificativo e' usato (giorni o maturazioni).
- [x] 16. Profili orari e orari: selettori causale solo orarie + controllo in salvataggio
  — Fatto: selettori di profilo orario (causali e giustificativo di assenza), orario (monte ore) e fasce mostrano solo i tipi a ore; i servizi rifiutano un tipo a importo (`_utility/GestionePresenzeTipoValore`). **Corretto un bug esistente**: profilo orario e fasce prendevano le causali di default dalla tabella dei giustificativi.
- [x] 17. Motore (`TimeSheet_EngineService`): accumulo degli importi dai giustificativi a importo, nessun effetto su ore/quadratura; `ScriviCausali` scrive `Importo`
  — Fatto: in `GeneraCausali` il giustificativo a importo accumula l'importo sulla sua causale (`_importiGiorno`) e salta tutti gli effetti sulle ore; `ScriviCausali` scrive le causali a importo (Valore 00:00).
- [x] 18. Dialog giustificativo del giorno (admin): un solo importo al posto di ore/dalle/tipo input
  — Fatto: `edit-dip-gg-giustificativi-dialog` mostra l'importo al posto dell'ora per i giustificativi a importo (validazione ore/importo secondo il tipo). Corretto anche il messaggio di giustificativo obbligatorio (path sbagliato, non compariva mai).
- [x] 19. Richiesta giustificativo dell'utente + approvazione (`request-justification-user-page`, `Dip_GG_RichiestaService`): giustificativi a importo richiedibili dal dipendente
  — Fatto: `request-justification-user-page` chiede l'importo per giorno; `Dip_GG_Richiesta_Body_Giustificativo.Importo`; il server vuole un importo positivo e materializza un giustificativo per ogni giorno lavorativo del periodo con quell'importo (scelta: stesso criterio dei giorni delle ore). Approvazione invariata (cambia solo lo stato).
- [x] 20. Dialog causale del giorno: un solo importo per le causali a importo
  — Fatto: `edit-dip-gg-causali-dialog` con l'importo per le causali a importo (stesso schema del dialog dei giustificativi). Il ricalcolo del giorno resta com'e' (§11.7.1).
- [x] 21. Servizi `Dip_GG_GiustificativiService` / `Dip_GG_CausaliService`: importo > 0 obbligatorio per il tipo Importo, ore vuote
  — Fatto: `Dip_GG_GiustificativiPut` e `Dip_GG_CausaliPut` vogliono un importo > 0 per il tipo Importo e azzerano ore/importo secondo il tipo; i valori proposti per un nuovo record usano il primo tipo a ore.
- [x] 22. Chat AI: `RegisterJustificationHandler`, `RegisterJustificationPeriodHandler`, `RegisterCausaleHandler` con lo slot importo
  — Fatto: nuovo comando `RegisterJustificationAmount` ("rimborso di 25 euro a Rossi per oggi") con `ImportoSlotHelper`; slot `amount` facoltativo nelle richieste per periodo e self-service (obbligatorio se il giustificativo e' a importo); `RegisterJustification` e `RegisterCausale` rispondono con un messaggio se il tipo e' a importo (le causali a importo dalla chat non si registrano); testo delle richieste con l'importo; "chi e' presente" ignora i giustificativi a importo. **Chat non provata con il LLM.**
- [x] 23. Visualizzazioni: calendari/timesheet (user, admin, power admin), `time-sheet.service.ts`, piano ferie: importi mostrati come `€ 12,50` ed esclusi dai totali ore
  — Fatto: cartellini utente, admin e power admin, action sheet delle causali, testo delle richieste nelle liste: importo con la pipe `importoEuro` (lingua attiva). Piano ferie: i giustificativi a importo non compaiono (non sono "da piano ferie"), aggiunta solo la protezione su ore assenti.
- [x] 24. Contatori e report (`ContatoriService`, `ReportService`): importi esclusi dalle ore; colonna importi nei report se serve
  — Fatto/verificato: contatori e piano ferie/report li escludono gia' (niente contatore, niente piano ferie per i tipi a importo); nessuna modifica necessaria. Colonna importi nei report non aggiunta (non richiesta).
- [x] 25. Cartellino PDF: giustificativi a importo con l'importo
  — Fatto: nel cartellino PDF il giustificativo a importo mostra l'importo (es. `TRIMB € 25,50`).
- [x] 26. i18n e messaggi
  — Fatto: messaggi server it/en (regole dei parametri, importo obbligatorio, chat) e chiavi client: nuova sezione `attendance.common` (tipo, ore, importo), note delle pagine causali/giustificativi, importo per giorno della richiesta.
- [x] 27. Prova di non regressione: ricalcolo di giorni esistenti senza differenze sui dati orari
  — Fatto: **non regressione** sul server di prova (azienda di test 1, ottobre, 7 dipendenti): ricalcolo con il motore dell'ultimo commit (`0a7b97af`, worktree temporaneo) e con quello nuovo -> causali e risultati giornalieri **identici**. **Prove funzionali** (API): causale e giustificativo a importo con campi forzati; giustificativo a ore con causale a importo rifiutato; giustificativo del giorno senza importo rifiutato, con 25,50 salvato e causale `00:00|25.50` scritta dal motore con ore del giorno invariate; cambio di tipo rifiutato se usato; profilo con causale a importo rifiutato; richiesta a importo senza importo rifiutata e con importo materializzata; pulizia con il giorno tornato uguale. Interfaccia non provata nel browser.

### 12.4 Modello di export (lavori 28–32) — `AttendanceTracking_Dev` (§2)

- [ ] 28. Eliminazione di `Par_ExportCau.TipoFile` (entità, enum, migrazione, modelli, default, pagina edit, lista, pipe, i18n)
- [ ] 29. Tabella `Par_ExportCau_Docs` + campi `NoteAI`, `RicettaAI`, `RicettaHash` su `Par_ExportCau` + colonna `Verso` su `Par_ExportCau_Causali` (default Sommatoria) + migrazione
- [ ] 30. Repository, servizio (upload base64, estrazione testo, limiti), controller, servizio client di `Par_ExportCau_Docs`
- [ ] 31. Sezione *Documentazione* nella pagina edit: elenco, carica, vedi (file o testo estratto), scarica, elimina
- [ ] 32. Causali dell'export: select *Verso* nel dialog e segno nell'elenco (modelli, i18n); controllo in `Par_ExportCau_Causali_PutAll_4Edit`: stesso codice di output ⇒ stesso tipo elaborazione e unità; `Importo` solo per causali a importo

### 12.5 Export (lavori 33–43) — `AttendanceTracking_Dev` (§3, §4, §6.3)

- [ ] 33. Controllo ruoli e dipendenti amministrati in `Export`; tolto `Task.Delay(10000)`
- [ ] 34. `PrepareExportMetadati`: JSON con elenco `chiave`, raggruppamento per codice di output e periodo con il segno del Verso, 2 decimali, avviso per i totali negativi, esclusi i dipendenti senza causali
- [ ] 35. Unit test del raggruppamento (stesso codice da più causali, giornaliera/mensile, periodo a cavallo di due mesi, importi, Verso Sottrazione, totale negativo)
- [ ] 36. Export senza documentazione → file `.json` scaricabile
- [ ] 37. Generazione della ricetta AI (prompt, schema, validazione) e salvataggio su `Par_ExportCau` con hash di documenti e note
- [ ] 38. Renderer della ricetta: testo (separato / larghezza fissa, codifica, fine riga, intestazione/piede, formato del segno per i valori negativi) e XLSX con ClosedXML
- [ ] 39. Export con documentazione: ricetta (rigenerata solo se cambiano documenti o note) + renderer + download; errore AI ⇒ JSON comunque scaricabile
- [ ] 40. Anteprima della ricetta nella pagina del modello (su un dipendente)
- [ ] 41. i18n e messaggi
- [ ] 42. Prove finali (§8): JSON, TXT a larghezza fissa, CSV, XLSX, causali a importo, CompanyAdmin, AI non raggiungibile/429
- [ ] 43. Cartellini PDF (`ExportPdf_4User`, `ExportPdf_4Admin`) e report (`ReportService`) registrano il file con `NVXSystem.RegistraFileJob` e mandano `FileName` nella notifica (download solo per chi ha lanciato il job, nome leggibile)

---

## 13. Lavori fatti (registro)

Una riga per ogni lavoro completato, nell'ordine in cui è stato fatto. Il dettaglio di cosa è stato fatto va anche nella nota del lavoro nella checklist (§12).

| Data | Sezione | Lavoro | Ramo | Commit | Note |
|------|---------|--------|------|--------|------|
| 29/10/2026 | 12.1 | 1–6 | Infrastructure_Dev | `79ef15cc` | file dei job, download, nome file nella notifica, pulizia, LlmClient |
| 29/10/2026 | 12.1 | 7 | Infrastructure → AttendanceTracking_Dev → AttendanceTracking | `d09b596f`, `e77c3543` | catena dei merge, build server e client ok |
| 29/10/2026 | 12.2 | 8–12 | AttendanceTracking_Dev → AttendanceTracking | `c374c41c` | matricola e codice fiscale: dati, controlli, pagine, importazione, i18n; prove API ok |
| 29/10/2026 | 12.3 | 13–27 | AttendanceTracking_Dev → AttendanceTracking | `ce0ca1cd` | tipo Orario/Importo: dati, regole, motore, dialog, richieste, chat, visualizzazioni; non regressione e prove API ok |
