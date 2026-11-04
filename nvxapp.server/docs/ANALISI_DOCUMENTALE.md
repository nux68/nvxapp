# Analisi – Applicativo Documentale (gestione documentale)

Branch: **DocumentManagement_Dev** (riceve i merge da Infrastructure, si mergia in DocumentManagement).
Il modello delle **tabelle condivise** (schema "dati comuni dell'azienda", `Common`) è descritto in `ANALISI_CRM.md` §2 e va realizzato in **Infrastructure_Dev** prima di entrambi gli applicativi. Qui si riprendono solo i punti specifici del documentale.

Stato: analisi completa, **tutte le decisioni prese** (§10, 03-04/11/2026). Lavori fatti e da fare: §11.

---

## 1. Situazione di partenza

- **Gerarchia**: Dealer → FinancialAdvisor (studio) → Company (azienda) → utenti (`UserDealer`, `UserFinancialAdvisor`, `UserCompany`). Il documentale è il punto d'incontro naturale **studio ↔ azienda ↔ dipendente**.
- **File oggi**: sempre nel database come `byte[]` (`TicketAttach.File`, `Document.File`, `Dip_GG_NotaSpesa_Allegato.Contenuto`, `Par_ExportCau_Docs.Contenuto`). Va bene per pochi allegati, non per un archivio (cedolini mensili × dipendenti × anni).
- **Estrazione testo**: `Helpers/FileText.Estrai` (PdfPig per PDF, docx, odt, xlsx, html, testo). Le scansioni non hanno testo (niente OCR).
- **Tabella `public.Document`**: è la documentazione per le risposte AI ai ticket (`ANALISI_DOCUMENTAZIONE_AI.md`), **non** c'entra con il documentale. Le tabelle nuove usano il prefisso `Doc_` per non collidere (il controllo dei nomi all'avvio lo bloccherebbe).
- **Dipendenti**: `Dip_Anagrafica` (presenze) con `IdAspNetUsers` **obbligatorio**, Cognome, Nome, Matricola, CodiceFiscale (unici per azienda). Decisa la sua uscita dalle presenze verso Common (§2.1).
- **Schema Common** (dati comuni dell'azienda, `tenant_<id>_0`): predisposto il 03/11/2026 con `Az_Anagrafica`, `Az_Cliente`, `Az_ClienteContatto` (`ANALISI_CRM.md` §10).
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
| `Az_Sedi`, `Az_SediReparto` | **restano nelle presenze** *(deciso)*: nel documentale permessi e distribuzione si basano su cartelle, ruoli e utenti, non su sedi/reparti |

### 2.1 Modifiche a `Dip_Anagrafica` richieste dal documentale

*Deciso il 04/11/2026: `Dip_Anagrafica` si sposta in Common* (rivista la scelta del 03/11 di lasciarla nelle presenze: senza, un'azienda con il solo documentale non avrebbe a chi intestare cedolini e CU). Le modifiche che questo comporta:

1. **Dipendente senza account.** Un'azienda che usa solo il documentale archivia i cedolini anche di chi non accede all'applicazione (o non ha ancora ricevuto l'invito). → `IdAspNetUsers` **facoltativo** nella tabella comune. Le presenze lavorano solo sui dipendenti con utente (filtro nei servizi e nel wizard presenze); il controllo "un utente = un dipendente per azienda" resta (indice unico parziale `WHERE IdAspNetUsers IS NOT NULL`).
2. **Conservazione dopo l'uscita.** I documenti del personale vanno conservati per anni (obblighi di legge) anche dopo la cessazione o la rimozione dell'utente. → con il documentale attivo, togliere l'utente dall'azienda **non** cancella `Dip_Anagrafica`: si scollega l'utente (`IdAspNetUsers = null`) e si valorizza `DataCessazione`. Cambia il comportamento del deleter presenze: cancella i propri dati (rapporti, timbrature, …) ma la riga anagrafica appartiene a Common e la cancella il deleter Common solo se nessun applicativo la usa più (`ICommonEntityUsage`, già predisposta in Infrastructure).
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
| CompanyPowerAdmin | configurazione (tipologie, cartelle, permessi, conservazione), archivio aziendale; documenti del personale **solo se abilitato esplicitamente** |
| CompanyAdmin | archivio aziendale secondo i permessi; documenti del personale **solo se abilitato esplicitamente** |
| User (dipendente) | **solo i propri** documenti personali + cartelle condivise a cui ha accesso; presa visione, caricamento su richiesta |
| Dealer / SuperUser | nessun accesso ai contenuti (solo impersonando, come oggi) |

**Documenti del personale** *(deciso)*: caricarli e vederli (cedolini e buste paga dei colleghi compresi) è consentito a CompanyPowerAdmin e CompanyAdmin **solo con un'abilitazione esplicita per utente** (nuova tabella `Doc_UtenteAbilitazione` nello schema dell'azienda: IdAspNetUsers, CaricaPersonale, VedePersonale; la gestisce il CompanyPowerAdmin abilitato o chi attiva il documentale, S2). Senza abilitazione l'admin non vede i documenti del personale, nemmeno nell'elenco. Il dipendente vede sempre i propri.

Accesso dello studio a più aziende: il servizio lavora azienda per azienda con `TenantScope.Use(idCompany)` (già usato da job e inizializzatori), dopo aver verificato che l'azienda appartenga allo studio dell'utente. Nessun accesso diretto agli schemi di altre aziende senza questo controllo.

I permessi si verificano **sempre lato server** su ogni lettura/download (vedi `piano-security.md`); il link di download è un endpoint autenticato o un token firmato a scadenza breve, mai un percorso di file.

### 4.1 Attivazione del documentale: studio e aziende

**Oggi**
- L'attivazione esiste **solo per azienda**: `public.CompanyApplication` (una riga per azienda/applicativo, attivo o disattivato) e lo schema `tenant_<IdAzienda>_<App>`.
- Lo studio (`FinancialAdvisor`) non ha applicativi né dati propri negli schemi tenant: vede le aziende con `Company.IdFinancialAdvisor` = studio.
- `CompanyApplicationPut` **non controlla chi attiva** (nessun controllo di ruolo nel servizio): va chiuso comunque (vedi `piano-security.md`).
- `TenantSchemaAccessor` rifiuta l'accesso ai dati di un applicativo non attivo per l'azienda: è già il blocco "naturale" per le aziende, non per lo studio.

Nel documentale lo studio è il protagonista (carica i documenti del personale, invia le comunicazioni, chiede documenti), quindi servono regole anche al suo livello.

**Regole** (decise dall'utente il 03/11/2026: S2, S3, S6, l'estensione di S2 a tutti gli applicativi e l'archivio dello studio fuori dalla prima versione)

| # | Regola |
|---|---|
| S1 | **Abilitazione dello studio.** Nuova tabella `public.FinancialAdvisorApplication` (IdFinancialAdvisor, ApplicationType, Active, ActivationDate, DeactivationDate; una riga per studio/applicativo), stesso schema di `CompanyApplication`. Lo studio abilitato ha le funzioni "lato studio" del documentale (area studio §7.4, distribuzione massiva §7.2, richieste). L'abilitazione **non crea schemi**: i dati restano nelle aziende. |
| S2 | **Chi attiva** *(deciso)*: il documentale, sia per lo **studio** sia per l'**azienda**, lo attivano e disattivano solo il **SuperUser** e il **DealerPowerAdmin** del centro (Dealer) a cui appartengono lo studio o l'azienda (non il DealerAdmin). Né lo studio né l'azienda si attivano da soli (niente FinancialAdvisorPowerAdmin né CompanyPowerAdmin). Vale per **tutti gli applicativi**, non solo per il documentale. Il controllo va nel servizio, lato server: oggi `CompanyApplicationPut` non controlla nessun ruolo. |
| S3 | **Azienda indipendente dallo studio** *(deciso)*: un'azienda può avere il documentale attivo anche se il suo studio non lo ha (o lo ha disattivato). In quel caso l'azienda lo usa da sola: carica lei i documenti dei dipendenti (CompanyAdmin abilitato) o lo usa per i propri documenti; mancano solo le funzioni lato studio. |
| S4 | **Lo studio lavora solo sulle aziende con il documentale attivo.** Area studio, distribuzione massiva e invio a più aziende elencano solo le aziende dello studio con il documentale attivo; un'azienda senza documentale compare come "non attivo" e non riceve documenti (lo garantisce anche `TenantSchemaAccessor`, perché il servizio lavora con `TenantScope.Use(idCompany)`). Le funzioni lato studio servono **entrambe** le attivazioni: studio abilitato **e** azienda attiva. |
| S5 | **Attivazione in blocco.** Chi abilita lo studio (S2) può attivare nello stesso passaggio il documentale su tutte o parte delle sue aziende, e farlo proporre attivo sulle aziende **nuove** dello studio (come `ActivateOnNewCompany` delle presenze). Vale per qualsiasi applicativo. |
| S6 | **Studio disabilitato** *(deciso)*: si bloccano le funzioni lato studio; le aziende **restano attive** e continuano a usare il documentale da sole (S3). I documenti caricati dallo studio restano nelle aziende. |
| S7 | **Azienda che cambia studio** (`CompanyMove`): il vecchio studio perde l'accesso subito (già oggi, filtro su `IdFinancialAdvisor`); i documenti restano dell'azienda, con l'origine "Studio" e il nome del vecchio studio nel registro. Il documentale dell'azienda resta attivo; le funzioni lato studio dipendono dall'abilitazione del nuovo studio. |
| S8 | **Azienda che disattiva il documentale** (sempre dal centro o dal SuperUser): accesso bloccato e dati conservati; lo studio non vede più quell'azienda nell'area studio (S4). |
| S9 | **Utenti dello studio:** FinancialAdvisorPowerAdmin e FinancialAdvisorAdmin, con studio abilitato. Il PowerAdmin dello studio può limitare le funzioni ai propri utenti (es. solo alcuni caricano i cedolini), con la stessa logica di `CompanyMenuRestriction` portata a livello di studio. |
| S10 | **Menu e dati utente:** `UserLoad` restituisce anche gli applicativi abilitati dello studio dell'utente (oggi `ActiveApplications` vale solo per l'azienda corrente); il client mostra il menu "Documentale" dello studio con `forApplication` sui ruoli dello studio. |
| S11 | **Spazio a disposizione** (§5.3): per azienda, fissato da chi attiva (S2); per lo studio solo come somma informativa delle sue aziende. |

**Archivio dello studio** *(deciso: fuori dalla prima versione)*. Lo studio non ha uno schema proprio: una comunicazione inviata a più aziende diventa **una copia per ogni azienda** (ogni azienda possiede i suoi documenti; cancellare o spostare un'azienda non tocca le altre). Modelli e bozze delle comunicazioni dello studio, se servono, vanno in una piccola tabella di `public` legata a `IdFinancialAdvisor` (come `Document` per la documentazione AI), senza documenti dei dipendenti. Un archivio dei documenti dello studio stesso (che richiederebbe un tipo di schema tenant per studio) si valuterà in seguito.

**Estensione a tutti gli applicativi** *(decisa)*. La regola S2 vale per tutti gli applicativi e si implementa in Infrastructure: `CompanyApplicationPut` (oggi senza controllo di ruolo) e la nuova attivazione per studio accettano solo il SuperUser e il DealerPowerAdmin del centro dell'azienda/studio; la pagina di attivazione mostra le scelte in sola lettura agli altri ruoli. `FinancialAdvisorApplication` vale per ogni applicativo con funzioni lato studio (es. presenze: export paghe dello studio). Lo studio **non** fa da tetto alle sue aziende (S3).

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
- **FileSystem** (predefinita, *decisa*: su disco): volume Docker dedicato (`nvxapp_files`, da aggiungere a `docker-compose*.yml` e ai backup), percorso `<idCompany>/<aaaa>/<mm>/<guid>`;
- **S3 compatibile** (MinIO in Docker o storage esterno) per le installazioni grandi;
- **Database** (`bytea` in una tabella `Doc_FileContent`) per installazioni minime/sviluppo.

Nel database restano solo i **metadati**: chiave, nome, tipo, dimensione, **hash SHA-256** (integrità, duplicati). Cifratura dei file a riposo: **per ora no** *(deciso)*; l'astrazione `IFileStorage` permette di aggiungerla in seguito senza toccare il documentale.

### 5.2 Limiti
- dimensione massima per file e **spazio a disposizione per azienda** (§5.3);
- tipi ammessi (lista bianca: pdf, immagini, office, testo, zip);
- antivirus facoltativo (ClamAV in Docker) prima della pubblicazione del documento.

### 5.3 Spazio a disposizione: configurazione e gestione

**Cosa si misura.** Lo spazio occupato da un'azienda è la somma delle dimensioni di **tutte le versioni** dei suoi file nello storage (`Doc_Versione.Dimensione`), compresi i documenti nel **cestino** finché non viene svuotato. Non contano i metadati e il testo estratto (stanno nel database). Lo spazio è **solo per azienda** (nessuno spazio condiviso a livello di studio) e conta **solo i file del documentale**: i file degli altri applicativi, anche quando useranno `IFileStorage`, non entrano nel conteggio *(deciso il 03/11/2026)*.

**Configurazione (tabelle in `public`, Infrastructure)** *(decisa il 03/11/2026: pacchetti predefiniti, nessuna quota libera)*

| Tabella / chiave | Contenuto |
|---|---|
| `AppSetting` `Storage_MaxFileMB`, `Storage_WarningPercent` | valori predefiniti di tutta l'installazione: **dimensione massima del file** e **soglia di avviso**: **50 MB** e **80%** (decisi il 04/11/2026) |
| `StoragePlan` | **pacchetti predefiniti** di spazio (Descrizione, QuotaMB, MaxFileMB facoltativa, Ordine, Active, **Predefinito**), es. "Base 1 GB", "Plus 5 GB", "Pro 20 GB". Lo spazio di un'azienda è **sempre** quello del suo pacchetto: non si scrive una quota a mano. Un solo pacchetto è il predefinito, assegnato all'attivazione se non se ne sceglie un altro. Un pacchetto usato da aziende non si elimina, si disattiva (non più assegnabile) |
| `CompanyStorage` | una riga per azienda: `IdStoragePlan`, `MaxFileMB` (null = quella del pacchetto o predefinita), `WarningPercent` (null = predefinita), `UsedBytes`, `ReservedBytes`, `LastRecalc`, data dell'ultimo avviso inviato |
| `CompanyStorageHistory` | fotografia mensile per azienda (anno, mese, pacchetto, QuotaMB, UsedBytes, numero file): andamento e base per la fatturazione del centro |

**Chi configura.** Come per l'attivazione (S2): **SuperUser** e **DealerPowerAdmin** del centro dell'azienda. All'attivazione del documentale l'azienda riceve il pacchetto predefinito (o quello scelto nello stesso passaggio); cambiare pacchetto è l'unico modo per cambiare lo spazio. I pacchetti e i valori predefiniti li gestisce solo il SuperUser. L'azienda e lo studio vedono lo spazio in sola lettura.

**Controllo al caricamento (senza superare la quota con caricamenti contemporanei)**
1. prima di salvare il file si **prenota** lo spazio con un'unica istruzione atomica su `CompanyStorage`: `UPDATE ... SET ReservedBytes = ReservedBytes + @size WHERE UsedBytes + ReservedBytes + @size <= limite RETURNING ...` (nessuna riga = spazio insufficiente);
2. salvataggio nello storage e della `Doc_Versione` nello schema dell'azienda;
3. conferma: `ReservedBytes -= @size`, `UsedBytes += @size`; in caso di errore la prenotazione si annulla;
4. eliminazione definitiva (cestino svuotato, versione eliminata): `UsedBytes -= dimensione`.

Lo schema dell'azienda e `public` sono contesti diversi: per questo prenotazione e conferma, e un **ricalcolo notturno** (Hangfire, già presente) che riallinea `UsedBytes` con la somma reale delle versioni (e azzera prenotazioni rimaste appese), segnalando nel log le differenze.

**Soglie e oltre quota** *(deciso il 03/11/2026: oltre quota si bloccano i caricamenti)*
- al superamento della soglia di avviso (80% predefinito) e al 100%: notifica in app (SignalR) ed email al CompanyPowerAdmin dell'azienda e al DealerPowerAdmin del centro, al massimo una volta per soglia finché lo spazio non scende;
- oltre quota **si bloccano i caricamenti**: nessun nuovo file né nuova versione, con un messaggio che indica spazio usato e disponibile e chi contattare; restano sempre possibili consultazione, download, presa visione, spostamento ed **eliminazione** (per liberare spazio);
- **distribuzione massiva** (§7.2) e **invio a più aziende**: lo spazio necessario si controlla **prima** dell'elaborazione, azienda per azienda; le aziende senza spazio sufficiente vengono indicate nella schermata di verifica e non ricevono documenti finché non si libera spazio o si passa a un pacchetto più grande;
- passaggio a un pacchetto più piccolo dello spazio già usato: consentito con avviso, l'azienda risulta "oltre quota" e non carica finché non scende sotto.

**Liberare spazio**
- svuotamento del **cestino** (automatico dopo N giorni, configurabile in `Doc_Cfg`, o manuale);
- **versioni**: impostazione per azienda "conserva solo le ultime N versioni" (le più vecchie si eliminano, tranne quelle di documenti con presa visione obbligatoria, che restano come prova);
- elenco dei **file più grandi** e dei documenti oltre la conservazione (§9) per decidere cosa eliminare;
- disattivare il documentale **non** libera spazio (i dati restano, S8) e lo spazio continua a essere conteggiato; lo libera solo la cancellazione dell'azienda (il deleter elimina i file e la riga `CompanyStorage`).

**Pagine**
- **Azienda** (CompanyPowerAdmin, CompanyAdmin): barra spazio usato/disponibile, ripartizione per tipologia, anno e cartella, file più grandi, cestino; sola lettura della quota.
- **Centro / SuperUser**: elenco aziende con pacchetto, usato, quota, percentuale, filtri "oltre soglia" e "oltre quota"; cambio di pacchetto, dimensione massima del file e soglia di avviso dell'azienda; andamento mensile ed esportazione Excel (fatturazione).
- **Studio**: riepilogo in sola lettura dello spazio delle proprie aziende (S11).
- **SuperUser**: pacchetti (elenco e modifica) e valori predefiniti (dimensione massima del file, soglia di avviso); spazio totale dell'installazione rispetto al disco/bucket disponibile.

---

## 6. Modello dati (schema `tenant_<id>_4`, prefisso `Doc_`)

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
- configurazione (§6.1), permessi delle cartelle, regole di conservazione, spazio usato e disponibile (§5.3, sola lettura della quota);
- **registro accessi** consultabile ed esportabile;
- report: documenti non letti, richieste scadute, scadenze del mese.

### 7.6 AI (provider e chat già presenti)
- **classificazione** automatica del tipo e proposta dei metadati (date, CF, periodo di competenza, scadenza) dal testo estratto, sempre da confermare;
- handler chat `ChatAiHandler/DocumentManagement`: "trova il contratto di Rossi", "documenti in scadenza", "apri l'ultimo cedolino";
- **domande sul contenuto** dei documenti (ricerca nel testo + passaggio dei brani al modello), rispettando i permessi di chi chiede: l'AI riceve solo documenti che l'utente può vedere;
- *Deciso il 04/11/2026*: **per ora nessun documento viene inviato all'AI** (né classificazione, né domande sul contenuto). Le funzioni sopra restano per una fase successiva, quando si deciderà quali tipologie ammettere; l'handler chat può già cercare per titolo, tipologia e metadati senza leggere i contenuti.

---

## 8. Integrazioni con gli altri applicativi (facoltative, via interfacce in Infrastructure)

- **Presenze**: dalla scheda dipendente delle presenze, sezione "Documenti" (se documentale attivo). Allegati di note spese e giustificativi (certificati medici) archiviabili nel fascicolo del dipendente: le presenze chiamano `IDocumentArchiver` se registrato e attivo, altrimenti continuano a usare le proprie tabelle allegati.
- **CRM**: fascicolo del cliente (documenti del documentale con `IdAz_Cliente`) mostrato in lettura nella scheda cliente del CRM. Gli allegati e le offerte del CRM **restano nel CRM** (`Crm_Allegato`) e non si archiviano nel documentale (deciso il 04/11/2026, `ANALISI_CRM.md` §9 punto 6).
- **Ticket**: nessuna dipendenza (gli allegati dei ticket restano nel loro flusso).

---

## 9. Aspetti trasversali e piano di lavoro

**Trasversali**
- multilingua (`DocumentManagementMessages.resx/.en.resx`, cartella i18n client), menu/rotte/site-name/showcase dedicati con `forApplication(..., ApplicationType.DocumentManagement)`;
- `DocumentManagementDbContext` registrato con `AddApplicationDbContext`, design-time factory, cartelle `DocumentManagement` in entità/repository/servizi/mapper/controller;
- **cancellazione fisica**: `DocumentManagementDataDeleter` – conteggi per l'anteprima, eliminazione dei **file** nello storage (`DeleteCompanyAsync`) oltre allo schema; per l'utente rimosso **non** cancella i documenti del personale (§2.1 punto 2), toglie solo accessi e destinatari;
- **conservazione** *(deciso)*: job notturno che **solo segnala** i documenti oltre gli anni di conservazione; non elimina mai in automatico, l'eliminazione è sempre manuale;
- **GDPR**: registro accessi, documenti del personale visibili solo con abilitazione esplicita, nessun contenuto inviato all'AI (§7.6), esportazione dei documenti di un dipendente su richiesta;
- pagine con `BasePageConfirmCancelComponent` / `BaseDialogConfirmCancelComponent`, elenchi con `app-generic-list`.

**Piano**: fasi e lavori nella checklist al §11.

---

## 10. Decisioni

1. **Storage**: su disco (FileSystem, volume Docker) – deciso.
2. **Cifratura** dei file a riposo: per ora no – deciso.
3. **`Dip_Anagrafica` in Common** (§2.1): deciso il 04/11/2026, con `IdAspNetUsers` facoltativo e conservazione del dipendente dopo la rimozione dell'utente; cambia anche le presenze.
4. **Chi carica i documenti del personale**: lo studio e, nell'azienda, CompanyPowerAdmin e CompanyAdmin **solo con abilitazione esplicita** (§4) – deciso.
5. **Chi vede i documenti del personale dei colleghi**: CompanyPowerAdmin e CompanyAdmin **solo con abilitazione esplicita** (§4) – deciso.
6. **Spazio a disposizione** (§5.3) – deciso il 03/11/2026: configurano SuperUser e DealerPowerAdmin (S2); oltre quota **si bloccano i caricamenti**; valori predefiniti per **dimensione massima del file** e **soglia di avviso**; spazio solo da **pacchetti predefiniti** (`StoragePlan`); **non** si contano i file degli altri applicativi; spazio **solo per azienda**. Pacchetti iniziali *(decisi)*: **1, 5 e 20 GB**. Valori predefiniti *(decisi il 04/11/2026)*: **50 MB** per file e avviso all'**80%**.
7. **Fine conservazione**: i documenti vengono solo segnalati, mai eliminati in automatico – deciso.
8. **Contenuti all'AI**: per ora nessuno – deciso (§7.6).
9. **Sedi/reparti**: restano nelle presenze – deciso.
10. **Abilitazione dello studio** (§4.1) – tutto deciso il 03/11/2026: azienda indipendente dallo studio (S3); attivazione di studio e azienda solo dal SuperUser o dal **DealerPowerAdmin** del centro (S2), regola estesa a **tutti gli applicativi**; aziende attive anche con lo studio disabilitato (S6); **niente archivio proprio dello studio** nella prima versione.

---

## 11. Lavori fatti e da fare

Legenda: [x] fatto · [ ] da fare.

### Fase 0 – Prerequisiti comuni (Infrastructure_Dev, poi merge fino ad AttendanceTracking)

**Dati comuni (schema Common)** – checklist completa in `ANALISI_CRM.md` §10, Fase 0. In sintesi:
- [x] schema Common (`Common = 0`) con `Az_Anagrafica`, `Az_Cliente`, `Az_ClienteContatto`, API comuni, `ICommonEntityUsage`, deleter, migration azzerate e database ricreato (03/11/2026)
- [ ] `ApplicationType`: `Crm = 3`, `DocumentManagement = 4` (C# e TypeScript)
- [ ] `Az_Cliente`: `Default` → `Az_Cfg.IdAz_ClienteDefault`, nuovi `Attivo` e `Tipo`
- [ ] pagine Clienti e menu **Anagrafiche** nell'infrastruttura, contatti nella pagina cliente

**Dipendenti in Common** (§2.1)
- [ ] `Dip_Anagrafica` spostata in Common (entità, repository, servizi di scrittura comuni)
- [ ] `IdAspNetUsers` facoltativo con indice unico parziale (`WHERE IdAspNetUsers IS NOT NULL`); nuovi campi `Email`, `DataCessazione`, `Attivo`
- [ ] presenze (AttendanceTracking_Dev): `Dip_Anagrafica` mappata esclusa, lavoro solo sui dipendenti con utente, scritture tramite i servizi comuni
- [ ] deleter: togliendo l'utente dall'azienda le presenze cancellano i propri dati; la riga del dipendente resta (utente scollegato, `DataCessazione`) e la elimina il deleter Common solo se nessun applicativo la usa (`ICommonEntityUsage`)
- [ ] invito di un dipendente senza account (creazione dell'utente e collegamento alla riga esistente)
- [ ] nuove migration Common e presenze (o nuovo azzeramento, da decidere al momento)

**Attivazione degli applicativi** (§4.1)
- [ ] `CompanyApplicationPut` accettato solo da SuperUser e DealerPowerAdmin del centro dell'azienda (oggi nessun controllo); pagina in sola lettura per gli altri ruoli
- [ ] `public.FinancialAdvisorApplication` con servizio, API e pagina di abilitazione nello studio (SuperUser e DealerPowerAdmin)
- [ ] attivazione in blocco sulle aziende dello studio e proposta sulle aziende nuove (S5)
- [ ] `UserLoad` con gli applicativi abilitati dello studio; menu dello studio con `forApplication` (S10)
- [ ] restrizioni delle funzioni agli utenti dello studio (S9)

**Archiviazione su disco** (§5.1)
- [ ] `IFileStorage` con implementazione **FileSystem** (`<idCompany>/<aaaa>/<mm>/<guid>`), hash SHA-256
- [ ] volume Docker `nvxapp_files` in `docker-compose*.yml` e nelle istruzioni di backup
- [ ] lista bianca dei tipi di file e dimensione massima del file
- [ ] (facoltativo, dopo) implementazioni S3/MinIO e database; cifratura (decisa: per ora no)

**Spazio a disposizione** (§5.3)
- [ ] `AppSetting` `Storage_MaxFileMB`, `Storage_WarningPercent` = 50 MB e 80%
- [ ] `StoragePlan` con i pacchetti iniziali **1, 5 e 20 GB** e il pacchetto predefinito
- [ ] `CompanyStorage` (pacchetto, spazio usato e prenotato) e `CompanyStorageHistory` (fotografia mensile)
- [ ] prenotazione atomica al caricamento, conferma o annullamento, blocco oltre quota
- [ ] ricalcolo notturno (Hangfire) e fotografia mensile
- [ ] avvisi alla soglia e al 100% (SignalR + email a CompanyPowerAdmin e DealerPowerAdmin)
- [ ] pagine: spazio dell'azienda, elenco aziende per centro/SuperUser con cambio di pacchetto ed export Excel, riepilogo per lo studio, pacchetti e valori predefiniti per il SuperUser

### Fase 1 – Documentale di base (DocumentManagement_Dev, creato da Infrastructure dopo la Fase 0)
- [ ] creazione del ramo DocumentManagement_Dev da Infrastructure
- [ ] `DocumentManagementDbContext` (`ApplicationType.DocumentManagement`), design-time factory, installer, migration iniziale
- [ ] configurazione (§6.1: `Doc_Cfg`, tipologie, metadati, cartelle, permessi delle cartelle) e wizard di attivazione con tipologie e cartelle predefinite multilingua
- [ ] abilitazioni per utente ai documenti del personale (`Doc_UtenteAbilitazione`: carica / vede) per CompanyPowerAdmin e CompanyAdmin
- [ ] archivio: cartelle, fascicoli (dipendente, cliente), elenco, filtri, caricamento multiplo, anteprima PDF/immagini
- [ ] versioni (nuova, storico, ripristino), cestino con svuotamento
- [ ] ricerca nel testo (`tsvector` italiano + indice GIN) con il testo estratto da `FileText`
- [ ] documenti del personale con destinatari e **presa visione** (`Doc_Destinatario`)
- [ ] "I miei documenti" per il dipendente
- [ ] registro accessi (`Doc_Log`), consultabile ed esportabile
- [ ] controllo dello spazio a ogni caricamento (§5.3)
- [ ] `DocumentManagementDataDeleter` (file nello storage compresi; i documenti del personale restano all'uscita del dipendente) e `ICommonEntityUsage` del documentale (documenti di un cliente o di un dipendente)
- [ ] menu, rotte, site-name, showcase, multilingua (`DocumentManagementMessages`, i18n)
- [ ] handler chat che cerca per titolo, tipologia e metadati, **senza leggere i contenuti** (§7.6)

### Fase 2 – Studio e flussi
- [ ] area studio (§7.4): aziende dello studio con documentale attivo, documenti inviati, prese visione, richieste
- [ ] distribuzione massiva (§7.2): divisione del PDF, abbinamento per codice fiscale/matricola, schermata di verifica, pubblicazione, lavoro in background con avanzamento
- [ ] invio di comunicazioni a più aziende (una copia per azienda)
- [ ] richieste di documenti (`Doc_Richiesta`) con scadenza e caricamento in risposta, anche da telefono
- [ ] scadenzario con notifiche in app ed email
- [ ] link esterni a scadenza (`Doc_Condivisione`)
- [ ] segnalazione dei documenti oltre la conservazione (job notturno, solo segnalazione)
- [ ] report: documenti non letti, richieste scadute, scadenze del mese

### Fase 3 – Integrazioni ed estensioni
- [ ] presenze: sezione "Documenti" nella scheda dipendente; archiviazione facoltativa degli allegati di note spese e giustificativi (`IDocumentArchiver`)
- [ ] CRM: fascicolo del cliente in lettura nella scheda cliente
- [ ] AI sui contenuti (classificazione, domande sui documenti), quando si deciderà quali tipologie ammettere (oggi: nessuna)
- [ ] OCR delle scansioni
- [ ] firma elettronica tramite provider
- [ ] archivio proprio dello studio (escluso dalla prima versione)
