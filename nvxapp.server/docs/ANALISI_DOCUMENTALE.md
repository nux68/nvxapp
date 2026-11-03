# Analisi – Applicativo Documentale (gestione documentale)

Branch: **DocumentManagement_Dev** (riceve i merge da Infrastructure, si mergia in DocumentManagement).
Il modello delle **tabelle condivise** (schema "dati comuni dell'azienda", `Common`) è descritto in `ANALISI_CRM.md` §2 e va realizzato in **Infrastructure_Dev** prima di entrambi gli applicativi. Qui si riprendono solo i punti specifici del documentale.

Stato: analisi, nessuna implementazione. Le scelte da confermare sono al §10.

---

## 1. Situazione attuale (cosa c'è già)

- **Gerarchia**: Dealer → FinancialAdvisor (studio) → Company (azienda) → utenti (`UserDealer`, `UserFinancialAdvisor`, `UserCompany`). Il documentale è il punto d'incontro naturale **studio ↔ azienda ↔ dipendente**.
- **File oggi**: sempre nel database come `byte[]` (`TicketAttach.File`, `Document.File`, `Dip_GG_NotaSpesa_Allegato.Contenuto`, `Par_ExportCau_Docs.Contenuto`). Va bene per pochi allegati, non per un archivio (cedolini mensili × dipendenti × anni).
- **Estrazione testo**: `Helpers/FileText.Estrai` (PdfPig per PDF, docx, odt, xlsx, html, testo). Le scansioni non hanno testo (niente OCR).
- **Tabella `public.Document`**: è la documentazione per le risposte AI ai ticket (`ANALISI_DOCUMENTAZIONE_AI.md`), **non** c'entra con il documentale. Le tabelle nuove usano il prefisso `Doc_` per non collidere (il controllo dei nomi all'avvio lo bloccherebbe).
- **Dipendenti**: `Dip_Anagrafica` (presenze) con `IdAspNetUsers` **obbligatorio**, Cognome, Nome, Matricola, CodiceFiscale (unici per azienda).
- **Cancellazione fisica** (`IApplicationDataDeleter`): togliendo un utente dall'azienda, le presenze cancellano il dipendente con i suoi dati.
- **Infrastruttura utile**: RabbitMQ (lavori lunghi), SignalR (`job-notifier`, `long-job-notifier`, `ticket-notifier`), servizio email, chat AI con handler per applicativo, `CompanyMenuRestriction`.

---

## 2. Tabelle condivise che riguardano il documentale

Riferimento: `ANALISI_CRM.md` §2 (schema `tenant_<id>_<Common>` sempre presente, entità comuni in sola lettura dagli applicativi, estensioni 1-1 nello schema dell'applicativo, integrazioni via interfacce in Infrastructure).

| Tabella comune | Perché serve al documentale |
|---|---|
| `Az_Anagrafica` | ancora dell'azienda |
| `Dip_Anagrafica` | **destinatario** dei documenti del personale (cedolini, CU, contratti, attestati); abbinamento per **codice fiscale / matricola** |
| `Az_Cliente`, `Az_ClienteContatto` | **fascicolo del cliente** (contratti, offerte firmate, corrispondenza); destinatari esterni |
| `Az_Sedi`, `Az_SediReparto` (se spostate, `ANALISI_CRM.md` §9 punto 4) | distribuzione e permessi per sede/reparto |

### 2.1 Modifiche a `Dip_Anagrafica` richieste dal documentale

Sono il motivo principale per cui `Dip_Anagrafica` deve diventare comune e non restare delle presenze:

1. **Dipendente senza account.** Un'azienda che usa solo il documentale archivia i cedolini anche di chi non accede all'applicazione (o non ha ancora ricevuto l'invito). → `IdAspNetUsers` **facoltativo** nella tabella comune. Le presenze lavorano solo sui dipendenti con utente (filtro nei servizi e nel wizard presenze); il controllo "un utente = un dipendente per azienda" resta (indice unico parziale `WHERE IdAspNetUsers IS NOT NULL`).
2. **Conservazione dopo l'uscita.** I documenti del personale vanno conservati per anni (obblighi di legge) anche dopo la cessazione o la rimozione dell'utente. → con il documentale attivo, togliere l'utente dall'azienda **non** cancella `Dip_Anagrafica`: si scollega l'utente (`IdAspNetUsers = null`) e si valorizza `DataCessazione`. Cambia il comportamento del deleter presenze: cancella i propri dati (rapporti, timbrature, …) ma la riga anagrafica appartiene a Common e la cancella il deleter Common solo se nessun applicativo la usa più (`ISharedEntityUsageChecker`).
3. Campi anagrafici utili a entrambi (in Common, non specifici del documentale): `Email` personale (per invio/avvisi a chi non ha account), `DataCessazione`, `Attivo`.
4. Invitare un dipendente senza account: dalla scheda si crea l'utente (stesso flusso di registrazione esistente) e lo si collega alla riga già presente.

---

## 3. Obiettivo

Archivio documentale per azienda, con tre usi principali:
1. **Documenti del personale** – lo studio (o l'azienda) carica cedolini, CU, contratti, comunicazioni; il dipendente li vede/scarica dall'app, con **presa visione** tracciata.
2. **Scambio studio ↔ azienda** – documenti inviati dallo studio all'azienda (F24, DURC, prospetti, circolari) e **richieste di documenti** dallo studio all'azienda/dipendente con scadenza (es. documenti per un'assunzione).
3. **Archivio aziendale** – cartelle, tipologie, ricerca nel testo, scadenzario (DURC, visure, certificati, visite mediche, attestati di formazione), fascicolo cliente e dipendente.

Fuori dall'ambito della prima versione: **conservazione sostitutiva a norma** (richiede un conservatore accreditato: eventualmente integrazione futura), firma digitale qualificata/FEA (integrazione con provider, fase successiva), OCR delle scansioni (fase 2), editing collaborativo dei file.

---

## 4. Ruoli e accessi

| Ruolo | Accesso |
|---|---|
| Utenti dello **studio** (FinancialAdvisor) | carica e consulta i documenti delle **proprie** aziende (quelle con `Company.IdFinancialAdvisor` = studio), anche in blocco su più aziende; vede prese visione e richieste |
| CompanyPowerAdmin | configurazione (tipologie, cartelle, permessi, conservazione), tutto l'archivio |
| CompanyAdmin | archivio aziendale secondo i permessi; documenti del personale **solo se** abilitato (dati sensibili: buste paga) |
| User (dipendente) | **solo i propri** documenti personali + cartelle condivise a cui ha accesso; presa visione, caricamento su richiesta |
| Dealer / SuperUser | nessun accesso ai contenuti (solo impersonando, come oggi) |

Accesso dello studio a più aziende: il servizio lavora azienda per azienda con `TenantScope.Use(idCompany)` (già usato da job e inizializzatori), dopo aver verificato che l'azienda appartenga allo studio dell'utente. Nessun accesso diretto agli schemi di altre aziende senza questo controllo.

I permessi si verificano **sempre lato server** su ogni lettura/download (vedi `piano-security.md`); il link di download è un endpoint autenticato o un token firmato a scadenza breve, mai un percorso di file.

---

## 5. Archiviazione dei file

### 5.1 Astrazione `IFileStorage` (Infrastructure)

Il documentale non può tenere i file in `bytea`: backup enormi, tabelle pesanti, memoria del server. Proposta in **Infrastructure_Dev** (servirà anche ad allegati ticket e note spese in futuro):

```
IFileStorage
  Task<string> SaveAsync(int idCompany, Stream content, string contentType)   -> chiave
  Task<Stream> OpenAsync(string key)
  Task DeleteAsync(string key)
  Task DeleteCompanyAsync(int idCompany)
```

Implementazioni, scelte da configurazione:
- **FileSystem** (predefinita): volume Docker dedicato (`nvxapp_files`, da aggiungere a `docker-compose*.yml` e ai backup), percorso `<idCompany>/<aaaa>/<mm>/<guid>`;
- **S3 compatibile** (MinIO in Docker o storage esterno) per le installazioni grandi;
- **Database** (`bytea` in una tabella `Doc_FileContent`) per installazioni minime/sviluppo.

Nel database restano solo i **metadati**: chiave, nome, tipo, dimensione, **hash SHA-256** (integrità, duplicati). Facoltativo: cifratura dei file a riposo con chiave per azienda (decisione §10).

### 5.2 Limiti
- dimensione massima per file e **quota per azienda** (spazio occupato mostrato all'admin; quota configurabile dal SuperUser/studio);
- tipi ammessi (lista bianca: pdf, immagini, office, testo, zip);
- antivirus facoltativo (ClamAV in Docker) prima della pubblicazione del documento.

---

## 6. Modello dati (schema `tenant_<id>_5`, prefisso `Doc_`)

### 6.1 Configurazione

| Tabella | Contenuto |
|---|---|
| `Doc_Cfg` | 1-1 con `Az_Anagrafica`: notifiche email, giorni di preavviso scadenze, presa visione obbligatoria predefinita |
| `Doc_Par_Tipo` | tipologia (Cedolino, CU, Contratto, F24, DURC, Visura, Certificato medico, Attestato formazione, Generico…): categoria (Personale / Azienda / Cliente), **anni di conservazione**, ha scadenza (sì/no), presa visione richiesta, riservato (solo admin abilitati), metadati attesi |
| `Doc_Par_TipoMetadato` | campi aggiuntivi per tipologia (es. Cedolino: mese/anno; Contratto: data inizio/fine) |
| `Doc_Cartella` | albero delle cartelle (padre, descrizione, ordine, di sistema sì/no) |
| `Doc_CartellaPermesso` | chi vede/carica/modifica: per ruolo, per utente o (se in Common) per reparto |

### 6.2 Dati

| Tabella | Contenuto |
|---|---|
| `Doc_Documento` | titolo, descrizione, `IdDoc_Par_Tipo`, `IdDoc_Cartella`, **riferimento**: `IdDip_Anagrafica` *o* `IdAz_Cliente` *o* nessuno (aziendale) – FK verso le tabelle comuni; data documento, competenza (mese/anno), **data scadenza**, data di eliminazione prevista (da conservazione), stato (Bozza / Pubblicato / Archiviato), origine (Studio / Azienda / Dipendente / Import), versione corrente |
| `Doc_Versione` | file di una versione: chiave storage, nome file, tipo, dimensione, hash, numero versione, autore, data; **testo estratto** (per la ricerca e l'AI) |
| `Doc_Metadato` | valori dei metadati della tipologia |
| `Doc_Tag`, `Doc_DocumentoTag` | etichette libere |
| `Doc_Destinatario` | a chi è indirizzato (utente, dipendente, ruolo): **presa visione** richiesta, data prima apertura, data conferma, data download |
| `Doc_Richiesta` | richiesta di documenti (da studio/admin a azienda o dipendente): oggetto, tipologia attesa, scadenza, stato (Aperta / Evasa / Scaduta / Annullata); i file caricati in risposta diventano `Doc_Documento` collegati |
| `Doc_Condivisione` | link esterno a scadenza (token casuale salvato come hash, scadenza, numero massimo di download, password facoltativa) per inviare documenti a chi non ha account (es. contatto di un cliente) |
| `Doc_Log` | **registro accessi**: chi ha visto/scaricato/modificato/eliminato cosa e quando (obbligatorio per dati del personale) |
| `Doc_Distribuzione` + `Doc_DistribuzioneRiga` | caricamento massivo (§7.2): file sorgente, stato, righe con pagina/i, CF/matricola trovati, dipendente abbinato, esito |

**Ricerca nel testo**: colonna `tsvector` (configurazione `italian`) su `Doc_Versione` con indice GIN, aggiornata al caricamento; ricerca combinata con filtri (tipo, periodo, dipendente, cliente, cartella, tag).

---

## 7. Funzioni

### 7.1 Archivio (azienda)
- navigazione per **cartelle** e per **fascicoli** (dipendente, cliente), elenco con `app-generic-list`, filtri, ricerca nel testo;
- caricamento (trascinamento di più file), anteprima PDF/immagini nel browser, **versioni** (nuova versione, storico, ripristino);
- spostamento, tag, scadenza, cestino (eliminazione logica con svuotamento dopo N giorni);
- **scadenzario**: documenti in scadenza/scaduti, notifica in app (SignalR) ed email ai responsabili.

### 7.2 Distribuzione massiva dei documenti del personale (studio)
Il caso tipico: lo studio carica **un unico PDF** con i cedolini del mese di tutti i dipendenti.
1. caricamento del file + tipologia (Cedolino) + competenza (mese/anno), per una o più aziende;
2. elaborazione in background (RabbitMQ, avanzamento con `long-job-notifier`): divisione per pagina/gruppo di pagine, estrazione del testo con `FileText`/PdfPig, ricerca del **codice fiscale** (regex) o della matricola;
3. abbinamento a `Dip_Anagrafica` dell'azienda (CF unico per azienda); per lo studio multi-azienda si cerca anche quale azienda;
4. **schermata di verifica**: abbinati, non abbinati, ambigui; correzione manuale; nulla viene pubblicato prima della conferma;
5. pubblicazione: un `Doc_Documento` per dipendente, destinatario con presa visione, notifica in app/email.
Stesso flusso per CU e altri documenti per dipendente; un tracciato diverso (es. un file per dipendente con il CF nel nome) si gestisce con regole di abbinamento sul nome file.

### 7.3 Dipendente
- "**I miei documenti**": elenco per tipologia/anno, nuovi da leggere evidenziati, presa visione con un clic (registrata con data e IP), download;
- risposta alle **richieste** (caricamento dal telefono, anche foto);
- nessun accesso ai documenti di altri.

### 7.4 Studio
- pagina dello studio con le proprie aziende: documenti inviati, **stato delle prese visione**, richieste aperte per azienda;
- invio di comunicazioni/circolari a più aziende in una volta.

### 7.5 Admin
- configurazione (§6.1), permessi delle cartelle, regole di conservazione, quota usata;
- **registro accessi** consultabile ed esportabile;
- report: documenti non letti, richieste scadute, scadenze del mese.

### 7.6 AI (provider e chat già presenti)
- **classificazione** automatica del tipo e proposta dei metadati (date, CF, periodo di competenza, scadenza) dal testo estratto, sempre da confermare;
- handler chat `ChatAiHandler/DocumentManagement`: "trova il contratto di Rossi", "documenti in scadenza", "apri l'ultimo cedolino";
- **domande sul contenuto** dei documenti (ricerca nel testo + passaggio dei brani al modello), rispettando i permessi di chi chiede: l'AI riceve solo documenti che l'utente può vedere;
- i documenti del personale riservati **non** si mandano all'AI salvo configurazione esplicita dell'azienda (dati sensibili, GDPR).

---

## 8. Integrazioni con gli altri applicativi (facoltative, via interfacce in Infrastructure)

- **Presenze**: dalla scheda dipendente delle presenze, sezione "Documenti" (se documentale attivo). Allegati di note spese e giustificativi (certificati medici) archiviabili nel fascicolo del dipendente: le presenze chiamano `IDocumentArchiver` se registrato e attivo, altrimenti continuano a usare le proprie tabelle allegati.
- **CRM**: fascicolo del cliente nella scheda cliente; offerte PDF archiviate automaticamente (decisione in `ANALISI_CRM.md` §9 punto 6).
- **Ticket**: nessuna dipendenza (gli allegati dei ticket restano nel loro flusso).

---

## 9. Aspetti trasversali e piano di lavoro

**Trasversali**
- multilingua (`DocumentManagementMessages.resx/.en.resx`, cartella i18n client), menu/rotte/site-name/showcase dedicati con `forApplication(..., ApplicationType.DocumentManagement)`;
- `DocumentManagementDbContext` registrato con `AddApplicationDbContext`, design-time factory, cartelle `DocumentManagement` in entità/repository/servizi/mapper/controller;
- **cancellazione fisica**: `DocumentManagementDataDeleter` – conteggi per l'anteprima, eliminazione dei **file** nello storage (`DeleteCompanyAsync`) oltre allo schema; per l'utente rimosso **non** cancella i documenti del personale (§2.1 punto 2), toglie solo accessi e destinatari;
- **conservazione**: job notturno che segnala (non elimina da solo, salvo conferma configurata) i documenti oltre gli anni di conservazione;
- **GDPR**: registro accessi, documenti riservati, minimizzazione verso l'AI, esportazione dei documenti di un dipendente su richiesta;
- pagine con `BasePageConfirmCancelComponent` / `BaseDialogConfirmCancelComponent`, elenchi con `app-generic-list`.

**Piano**
- **Fase 0 – Infrastructure_Dev**: quella di `ANALISI_CRM.md` §8 (schema Common, enum, spostamento anagrafiche) **più**: modifiche a `Dip_Anagrafica` (§2.1), `IFileStorage` con le tre implementazioni e il volume Docker, interfacce di integrazione (`IDocumentArchiver`).
- **Fase 0 bis – AttendanceTracking_Dev**: adeguamento al dipendente senza utente e al nuovo comportamento del deleter.
- **Fase 1 – DocumentManagement_Dev** (creato da Infrastructure): contesto, configurazione e wizard di attivazione (tipologie e cartelle predefinite); archivio con versioni, ricerca, anteprima; documenti del personale con destinatari e presa visione; "I miei documenti"; registro accessi.
- **Fase 2**: distribuzione massiva con abbinamento CF; richieste di documenti; scadenzario e notifiche; area studio multi-azienda; link esterni.
- **Fase 3**: AI (classificazione, chat, domande sul contenuto), integrazioni con presenze e CRM, OCR, firma elettronica tramite provider.

---

## 10. Decisioni da prendere

1. **Storage predefinito**: file system su volume Docker (consigliato) / S3-MinIO / database.
2. **Cifratura** dei file a riposo per azienda: sì/no (consigliata almeno per i documenti del personale).
3. **`Dip_Anagrafica.IdAspNetUsers` facoltativo** e conservazione del dipendente dopo la rimozione dell'utente (§2.1): confermare, perché cambia anche le presenze.
4. **Chi carica i documenti del personale**: solo lo studio, o anche l'azienda (CompanyAdmin abilitato)?
5. **CompanyAdmin e buste paga**: per impostazione predefinita vede i cedolini dei colleghi o no? (consigliato: no, abilitazione esplicita).
6. **Quote di spazio**: chi le stabilisce (SuperUser, dealer, studio) e se bloccano il caricamento o avvisano soltanto.
7. **Eliminazione a fine conservazione**: automatica o solo segnalata.
8. **Contenuti all'AI**: quali tipologie possono essere inviate al provider (consigliato: escludere per default quelle riservate).
9. **Sedi/reparti in Common** (vedi `ANALISI_CRM.md` §9 punto 4): servono per permessi e distribuzione per reparto.
