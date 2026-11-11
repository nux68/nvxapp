# Analisi – Firma digitale ed elettronica nel Documentale

Branch: **DocumentManagement_Dev** (riceve i merge da Infrastructure, si mergia in DocumentManagement).
Riferimenti: `ANALISI_DOCUMENTMANAGEMENT_APPLICATIVO.md` (modello `Doc_*`, storage, ruoli, §11 Fase 3 "firma elettronica tramite provider"), `piano-security.md` (autorizzazione lato server).

Stato: **analisi, decisioni da prendere** (§12). Realizzata (11/11/2026) la **richiesta di firma FES con un clic** (§7.1, §8.2): parametro sulla tipologia, scelta a ogni invio, firma del destinatario con registrazione delle prove. Il resto è ancora da fare.

> Nota: i riferimenti normativi sono un inquadramento tecnico, non un parere legale. Prima di attivare la FEA (§2.3) serve una verifica con un consulente.

---

## 1. Situazione di partenza

**Già presente nel documentale (Fase 1, 04/11/2026)**
- `Doc_Documento` → `Doc_Versione` (una riga per versione: `StorageKey`, `NomeFile`, `ContentType`, `Dimensione`, **`Hash` SHA-256**, autore, note). Ogni nuova versione azzera la presa visione.
- `Doc_Destinatario`: destinatari (dipendente `IdDip_Anagrafica` o utente `IdAspNetUsers`) con **presa visione** (prima apertura, data conferma, download).
- `Doc_Log`: registro accessi (utente, data, IP, dettaglio).
- `IFileStorage` su disco (`<idCompany>/<aaaa>/<mm>/<guid>`), con quota e prenotazione atomica dello spazio (`CompanyStorage`).
- Estrazione testo `Helpers/FileText.Estrai` (PdfPig): **non** legge i file `.p7m`.
- Dipendenti anche senza account (`Dip_Anagrafica` in Common con `Email`), clienti e contatti (`Az_Cliente`, `Az_ClienteContatto`).
- Infrastruttura: RabbitMQ (lavori lunghi), SignalR (notifiche), MailKit (email), `BackgroundService` di manutenzione, Hangfire facoltativo, Caddy come reverse proxy HTTPS.

**Librerie già referenziate che servono alla firma**
| Pacchetto | Uso per la firma |
|---|---|
| `System.Security.Cryptography.Pkcs` 10 | oggi referenziato solo per una dipendenza di MailKit, non usato nel codice; serve per leggere i `.p7m` (CAdES): `SignedCms`, estrazione del contenuto, elenco firmatari e certificati |
| `PDFsharp-MigraDoc` 6.2 | oggi usato solo per il manuale utente in PDF; apposizione di firme PDF con un firmatario esterno (`IDigitalSigner`): serve solo se si firma "in casa" con un certificato remoto (§4.4) |
| `PdfPig` | lettura dei PDF (testo, dizionari delle firme per un primo riconoscimento) |

Nessuna traccia nel codice di firme, certificati, OTP/2FA (Identity 2FA non usato) o SMS. Il caricamento accetta già `.p7m` e `.xml` tra le estensioni ammesse. Client: Angular 22 + Ionic 9 (Capacitor), Transloco, nessuna libreria di componenti.

**Mancano**: un visualizzatore PDF nel client (l'anteprima è ancora "da fare" in `ANALISI_DOCUMENTMANAGEMENT_APPLICATIVO.md` §11), un servizio di **verifica** delle firme con le liste di fiducia europee, qualunque integrazione con un prestatore di servizi fiduciari (QTSP), OTP via SMS.

---

## 2. Inquadramento: quale "firma"

### 2.1 Norme di riferimento
- **eIDAS** – Reg. UE 910/2014, aggiornato dal Reg. UE 2024/1183 (eIDAS 2);
- **CAD** – D.Lgs. 82/2005, artt. 20, 21, 24 (valore probatorio dei documenti informatici firmati);
- **DPCM 22/02/2013** – regole tecniche su firme avanzate, qualificate e digitali (artt. 55-61 per la FEA);
- **Linee guida AgID** sulla formazione, gestione e conservazione dei documenti informatici e sulla sottoscrizione elettronica ex art. 20 CAD (firma con SPID).

### 2.2 Livelli

| Livello | Cos'è | Valore | Chi la eroga |
|---|---|---|---|
| **FES** – firma elettronica semplice | qualunque dato che collega il documento a una persona: clic di accettazione, OTP via email/SMS | liberamente valutabile in giudizio | l'applicazione stessa |
| **FEA** – firma elettronica avanzata | collegata univocamente al firmatario, sotto il suo controllo esclusivo, rileva modifiche successive (OTP su cellulare verificato, grafometrica, firma con SPID) | forma scritta (art. 20 CAD), salvo gli atti dell'art. 1350 c.c. nn. 1-12 | un **soggetto erogatore** con gli obblighi del DPCM (identificazione, adesione scritta, conservazione 20 anni, assicurazione se la eroga a terzi) – oppure un provider che la offre "chiavi in mano" |
| **FEQ / firma digitale** | certificato qualificato emesso da un QTSP, chiave in dispositivo sicuro (smart card, token, HSM remoto) | forma scritta per tutti gli atti, equivale alla firma autografa in tutta l'UE | solo un **QTSP** (InfoCert, Aruba, Namirial, Intesi Group, …) |
| **Sigillo elettronico (qualificato)** | "firma" di una persona giuridica (lo studio, l'azienda), apponibile in automatico | garantisce origine e integrità del documento, non esprime volontà di una persona | QTSP, su HSM remoto |

### 2.3 Cosa significa per noi
- **FES**: si può fare interamente in casa (estensione della presa visione).
- **FEA in casa**: tecnicamente semplice (OTP), ma rende l'azienda o lo studio "soggetto erogatore" con obblighi legali e organizzativi. **Sconsigliata**: meglio la FEA offerta dal provider, che si assume quegli obblighi.
- **FEQ e sigillo**: solo tramite QTSP. Il certificato può essere quello **del firmatario** (ne possiede già uno, es. il consulente dello studio) oppure un certificato **"one-shot"** emesso al momento dal provider dopo il riconoscimento (SPID, CIE, video), adatto a dipendenti e clienti che non ne hanno uno.

---

## 3. Casi d'uso nell'applicativo

| # | Caso | Chi firma | Livello adatto |
|---|---|---|---|
| U1 | Contratto di assunzione, lettera di trasformazione, accordo smart working | dipendente (+ azienda) | FEA o FEQ one-shot |
| U2 | Accettazione di regolamenti, informativa privacy, consegna DPI, ricevuta di documenti | dipendente | FES (OTP) – oggi la presa visione è un clic |
| U3 | Mandati, deleghe (F24, cassetto fiscale), lettere d'incarico studio ↔ azienda | legale rappresentante dell'azienda, studio | FEQ (firma digitale) |
| U4 | Documenti inviati dallo studio all'azienda/dipendenti (cedolini, CU, prospetti) | nessuna persona: **sigillo** dello studio | sigillo elettronico (facoltativo) |
| U5 | Contratti/offerte al cliente nel fascicolo cliente | contatto del cliente (esterno, senza account) | FEA o FEQ one-shot |
| U6 | File **già firmati** caricati (`.p7m`, PDF firmati da DURC, visure, contratti) | – | **verifica** e visualizzazione |
| U7 | Firma "esterna" con il proprio dispositivo (Dike, ArubaSign, Firma4NG) | chiunque abbia smart card/token | FEQ, fuori dall'applicazione |

U6 e U7 sono gli unici che non richiedono contratti con un provider e coprono già molto dell'uso quotidiano di uno studio.

---

## 4. Opzioni tecniche

### 4.1 Opzione A – Verifica e gestione dei file firmati (nessun provider)
- riconoscimento al caricamento: `.p7m` (CAdES), PDF con dizionario `/Sig` (PAdES), `.xml` firmati (XAdES), `.tsd`/`.m7m` (con marca temporale);
- **estrazione del contenuto** dei `.p7m` con `SignedCms` (anche annidati: `.pdf.p7m.p7m`) per **anteprima, download del file "in chiaro" e ricerca nel testo** (oggi `FileText` non li legge);
- **verifica**: integrità, validità del certificato alla data di firma, revoca (OCSP/CRL), certificato qualificato, catena verso le **liste di fiducia UE (LOTL)**. Due strade:
  - **EU DSS** (Digital Signature Service della Commissione europea, open source LGPL) come **container Docker** accanto all'applicazione, chiamato via REST (`/services/rest/validation/validateSignature`): gestisce PAdES/CAdES/XAdES/ASiC e le liste di fiducia, restituisce un rapporto (semplice, dettagliato, ETSI). **Consigliata**.
  - solo .NET (`SignedCms.CheckSignature` + catena X509): verifica l'integrità ma **non** sa se il certificato è qualificato senza scaricare e interpretare le liste di fiducia; insufficiente come verifica "legale".
- **firma esterna (U7)**: "Scarica per firmare" → l'utente firma con il suo software → "Carica versione firmata": nuova `Doc_Versione` verificata, con il controllo che il contenuto firmato corrisponda alla versione scaricata (hash del contenuto estratto dal `.p7m`, o della prima revisione del PDF per il PAdES).

Costo: basso. Nessun contratto. Utile subito.

### 4.2 Opzione B – FES con OTP in casa
Estende la presa visione: il destinatario conferma "Ho letto e accetto" inserendo un codice ricevuto via **email** (subito, MailKit) o **SMS** (richiede un gateway SMS, es. Skebby, Aruba SMS, Twilio).
- registrazione di prova: hash della versione, data/ora, IP, user agent, canale e destinatario dell'OTP, identità dell'utente autenticato;
- **attestato** PDF generato (PDFsharp) con questi dati, archiviato come allegato del documento, e – se disponibile – chiuso con il **sigillo** (§4.3) per renderlo non modificabile;
- OTP: 6 cifre, validità 5-10 minuti, salvato solo come hash, massimo 5 tentativi, limite di invii per ora.

Costo: basso (email), medio con SMS. Valore legale: FES. Copre U2.

### 4.3 Opzione C – Provider QTSP con flusso di firma gestito ("envelope")
L'applicazione invia al provider il PDF, i firmatari e le posizioni delle firme; il provider identifica i firmatari, raccoglie le firme (FES/FEA/FEQ one-shot/firma con SPID, secondo il contratto), restituisce il PDF firmato e l'**audit trail** e avvisa con un **webhook**.
- la parte legale (identificazione, certificati, conservazione delle prove FEA) è del provider;
- firmatari esterni senza account (U5) e dipendenti senza account (U1) firmano da un link ricevuto via email/SMS;
- la pagina di firma può essere quella del provider (redirect) o incorporata (iframe/URL di firma), se il provider lo consente;
- lo stesso provider fornisce di solito il **sigillo elettronico** (U4) via API, per firmare in automatico.

Costo: medio. Contratto con costo per firma/busta. Copre U1, U3, U4, U5.

### 4.4 Opzione D – Firma remota con il certificato del firmatario (API CSC)
Per chi possiede già una firma remota (tipico per il consulente dello studio): l'applicazione calcola l'hash del PDF, il firmatario autorizza con le proprie credenziali + OTP, il provider restituisce la firma che l'applicazione inserisce nel PDF (PAdES, con `PDFsharp` `IDigitalSigner` o una libreria dedicata). Lo standard **Cloud Signature Consortium (CSC API)** è supportato da più QTSP e permette di non legarsi a uno solo.
- il PDF non lascia il server (si invia solo l'hash);
- più complessa: gestione del livello PAdES (B-T con marca temporale, B-LTA per la validità nel tempo), campi firma, firme multiple.

Costo: alto. Copre U3 con certificati propri. Da valutare dopo C.

### 4.5 Fuori ambito
- **firma con smart card dal browser**: il browser non accede ai dispositivi senza un software locale; si copre con la firma esterna (§4.1);
- **firma grafometrica** (tablet): hardware dedicato e obblighi FEA; eventualmente tramite il provider;
- **conservazione a norma**: i documenti firmati sono i primi candidati, ma resta un'integrazione separata con un conservatore accreditato (`ANALISI_DOCUMENTMANAGEMENT_APPLICATIVO.md` §3).

### 4.6 Raccomandazione
Le interfacce generiche (`ISignatureValidator`, `ISignatureProvider`, `IOtpService`, tabelle in `public`) vanno sviluppate in **Infrastructure_Dev** e arrivano nel documentale con il merge; nel ramo DocumentManagement_Dev restano tabelle `Doc_*`, servizi, API e pagine del documentale.

1. **Fase F1**: Opzione **A** (verifica con EU DSS, estrazione `.p7m`, firma esterna) + Opzione **B** solo via email.
2. **Fase F2**: Opzione **C** con **un** provider, dietro un'interfaccia `ISignatureProvider` che permetta di aggiungerne altri; sigillo dello studio/azienda.
3. **Fase F3** (su richiesta): Opzione D (CSC), SMS per la FES, firma con SPID se non inclusa nel provider.

---

## 5. Architettura

```
                        ┌─────────────── nvxapp.server ───────────────┐
 Angular ── API ──▶ Doc_FirmaService ──▶ ISignatureProvider ──▶ QTSP (REST)
   │                    │      │               ▲                   │
   │                    │      └─▶ ISignatureValidator ──▶ EU DSS (container)
   │                    ▼                      │                   │
   │              IFileStorage            Webhook endpoint ◀───────┘
   │              (versioni firmate)      (AllowAnonymous + HMAC)
   │                    ▲                      │
   └── SignalR ◀── RabbitMQ: download PDF firmato, verifica, notifiche
```

### 5.1 Interfacce (Infrastructure, riusabili da CRM e presenze)

```
ISignatureValidator                       // EU DSS o implementazione .NET minima
  Task<SignatureReport> ValidateAsync(Stream signed, Stream? detachedContent)
  Task<Stream> ExtractContentAsync(Stream p7m)          // CAdES → file originale

ISignatureProvider                        // un'implementazione per provider
  string Code { get; }                                  // "infocert", "namirial", …
  SignatureCapabilities Capabilities { get; }           // FES/FEA/FEQ, sigillo, embedded, sequenziale
  Task<EnvelopeResult> CreateEnvelopeAsync(EnvelopeRequest r)   // PDF + firmatari + campi
  Task<EnvelopeStatus> GetStatusAsync(string envelopeId)
  Task<string> GetSigningUrlAsync(string envelopeId, string signerId)
  Task CancelAsync(string envelopeId, string reason)
  Task<SignedResult> DownloadAsync(string envelopeId)    // PDF firmato + audit trail
  Task<WebhookEvent?> ParseWebhookAsync(HttpRequest req) // verifica la firma HMAC
  Task<Stream> SealAsync(Stream pdf, SealOptions o)      // sigillo (se supportato)

IOtpService                               // FES (Opzione B)
  Task SendAsync(OtpPurpose p, string channel, string to)
  Task<bool> VerifyAsync(OtpPurpose p, string to, string code)
```

Registrazione da configurazione (come `IFileStorage`): `Signature:Provider`, `Signature:Validator`, credenziali da variabili d'ambiente / secret Docker, mai nel database in chiaro né nel repository.

### 5.2 Webhook e multi-tenant
Il webhook arriva **senza** contesto di utente né di azienda. Serve una tabella in `public`:

| `public.SignatureEnvelope` | |
|---|---|
| `Provider`, `EnvelopeId` (unici) | identificativo presso il provider |
| `IdCompany`, `IdDoc_Firma` | dove si trova la richiesta |
| `CreatedAt`, `LastEventAt`, `Status` | stato sintetico |

Flusso: verifica HMAC/firma del webhook → ricerca della busta → evento salvato (idempotente per id evento) → messaggio RabbitMQ → il consumer lavora con `TenantScope.Use(idCompany)`. Un `BackgroundService` interroga ogni N minuti le buste aperte da più di X ore (webhook persi) e chiude le scadute.

L'endpoint webhook è l'unico `[AllowAnonymous]` del modulo: va limitato a POST, dimensione massima, rate limit, e deve rispondere 200 in fretta (lavoro vero in coda). Serve un URL pubblico raggiungibile dal provider (Caddy in produzione; in sviluppo un tunnel o il solo polling).

---

## 6. Modello dati (schema `tenant_<id>_4`, prefisso `Doc_`)

### 6.1 Nuove tabelle

| Tabella | Contenuto |
|---|---|
| `Doc_Firma` | richiesta di firma: `IdDoc_Documento`, **`IdDoc_Versione` da firmare** (+ `HashOriginale`), `Livello` (FES/FEA/FEQ/Sigillo/Esterna), `Provider`, `EnvelopeId`, `Stato` (Bozza, Inviata, InCorso, Completata, Rifiutata, Scaduta, Annullata, Errore), `Sequenziale`, `DataScadenza`, `Messaggio`, `IdAspNetUsersRichiedente`, `IdDoc_VersioneFirmata` (risultato), `StorageKeyAuditTrail`, `Errore` |
| `Doc_FirmaFirmatario` | `IdDoc_Firma`, `Ordine`, **chi**: `IdAspNetUsers` *o* `IdDip_Anagrafica` *o* `IdAz_ClienteContatto` *o* esterno; snapshot di Nome, Cognome, Email, Cellulare, CodiceFiscale al momento dell'invio; `Ruolo` (Firmatario, Approvatore, Copia); `Stato`; `DataFirma`; `MotivoRifiuto`; `ProviderSignerId`; posizione (pagina, x, y, larghezza, altezza) o nome del campo firma |
| `Doc_FirmaEvento` | storico eventi (inviata, aperta, firmata, rifiutata, scaduta, errore, webhook ricevuto): data, firmatario, `IdEventoProvider` (idempotenza), dettaglio. Complementa `Doc_Log` |
| (riuso) `Doc_Condivisione` | già prevista nel documentale (link esterno con token salvato come hash e scadenza): per la FES con OTP di un firmatario esterno senza account, se non si passa dal provider |
| `Doc_FirmaOtp` | (Opzione B) `IdDoc_Firma`/`IdDoc_Destinatario`, canale, destinazione, `HashCodice`, scadenza, tentativi, `DataVerifica`, IP, user agent |
| `Doc_VerificaFirma` | esito della verifica di una `Doc_Versione` firmata: `IdDoc_Versione`, `Formato` (CAdES/PAdES/XAdES/ASiC), `Esito` (Valida, NonValida, Indeterminata), `DataVerifica`, firmatari (JSON: nome, CF, emittente, data firma, qualificato, marca temporale), `StorageKeyRapporto` (rapporto DSS completo nello storage) |

### 6.2 Modifiche a tabelle esistenti

| Tabella | Modifica |
|---|---|
| `Doc_Versione` | `Firmata` (bool), `FormatoFirma`, `IdDoc_VersioneOrigine` (la versione non firmata da cui deriva), `StorageKeyContenuto` (contenuto estratto da un `.p7m`, per anteprima e ricerca) |
| `Doc_Documento` | `StatoFirma` sintetico (Nessuna, InAttesa, Firmato, Parziale, Rifiutato) per filtri ed elenco (con il provider) |
| `Doc_Par_Tipo` | **`FirmaRichiesta`** (fatto: valore *proposto* all'invio, solo categoria Personale, §7.1); in seguito `LivelloFirmaPredefinito`, `SigilloAutomatico` (es. Contratto → FEA; Cedolino → sigillo) |
| `Doc_Documento` | **`FirmaRichiesta`** (fatto): richiesta decisa all'invio del singolo documento, modificabile anche dopo (§7.1) |
| `Doc_Destinatario` | **fatto**: `FirmaRichiesta`, `DataFirma`, `FirmaVersione`, `FirmaHash` (SHA-256 della versione), `FirmaIp`, `FirmaUserAgent`, `IdAspNetUsersFirma` |
| `Doc_Cfg` | provider abilitato per l'azienda, livelli consentiti, scadenza predefinita delle richieste, promemoria (giorni) |
| `Doc_LogAzione` (enum) | fatti: `Firma` (9, dettaglio "FES sha256:<hash>"), `RichiestaFirma` (10, richiesta aggiunta/tolta dopo l'invio); in seguito RifiutoFirma, AnnullamentoFirma, VerificaFirma |
| `Doc_UtenteAbilitazione` | `RichiedeFirma` (il CompanyAdmin può inviare richieste di firma solo se abilitato; il CompanyPowerAdmin sempre, come per i documenti del personale) |

### 6.3 Tabelle in `public` (Infrastructure)

| Tabella | Contenuto |
|---|---|
| `SignatureEnvelope` | indice per il webhook (§5.2) |
| `SignatureProviderConfig` | provider dell'installazione, ambiente (test/produzione), opzioni; **credenziali fuori dal DB** |
| `CompanySignatureUsage` | consumi per azienda e mese (buste, firme per livello, sigilli, SMS): base per la fatturazione del centro, come `CompanyStorageHistory` |
| `SignaturePlan` *(se deciso)* | pacchetti di firme per azienda (es. 50/200/1000 firme/anno), con lo stesso schema di `StoragePlan` |

---

## 7. Regole

### 7.1 Richiesta di firma: tipologia e singolo invio (decisione dell'11/11/2026)
- La **tipologia** ha il parametro `FirmaRichiesta`: è solo il **valore proposto** (nessun obbligo). Ha senso solo per la categoria Personale, l'unica con destinatari; per le altre categorie il server lo salva spento. Nelle tipologie predefinite è acceso solo per "Contratto di lavoro".
- **All'invio** (caricamento singolo o multiplo) la richiesta è un interruttore già impostato dalla tipologia: chi invia la può **togliere o aggiungere**. Nell'API `FirmaRichiesta = null` significa "quella della tipologia".
- **Dopo l'invio** la richiesta si può ancora togliere o aggiungere modificando il documento (registro: azione `RichiestaFirma`). Togliendola, le firme già fatte restano come prova.
- Cambiare il parametro della tipologia non tocca i documenti già caricati.
- Il livello oggi è solo FES; quando ci sarà il provider la stessa scelta all'invio porterà anche il livello (`LivelloFirmaPredefinito` sulla tipologia, modificabile all'invio).

### 7.2 Regole generali

**Integrità**
- R1. Si firma **una versione precisa**: `Doc_Firma.IdDoc_Versione` + `HashOriginale`. Mentre una richiesta è aperta il documento è **bloccato**: niente nuova versione né spostamento nel cestino; per cambiarlo si annulla la richiesta (annullamento anche presso il provider).
- R2. Al rientro del PDF firmato: nuova `Doc_Versione` (`Firmata = true`, `IdDoc_VersioneOrigine`), hash calcolato, spazio prenotato e confermato come per ogni caricamento (§5.3 documentale); verifica automatica (`Doc_VerificaFirma`); controllo che la prima revisione del PDF firmato coincida con l'originale.
- R3. Una versione firmata non si sovrascrive: una nuova versione successiva **azzera lo stato di firma** (come la presa visione) e la versione firmata resta nello storico. La potatura delle versioni (`VersioniDaConservare`) **non elimina mai** versioni firmate né i loro audit trail.
- R4. Il cestino di un documento firmato richiede conferma esplicita; lo svuotamento automatico salta i documenti firmati con conservazione non scaduta.

**Formati**
- R5. Si invia in firma solo **PDF** (meglio PDF/A); i file Office vanno convertiti prima. Conversione facoltativa con un container **Gotenberg** (LibreOffice) o lasciata al provider se la offre. Per i non-PDF si accetta solo la firma esterna CAdES (`.p7m`).
- R6. Livello minimo PAdES **B-T** (con marca temporale); **B-LTA** per i documenti con conservazione lunga, se il provider lo supporta.

**Accessi** (sempre lato server, `piano-security.md`)
- R7. Richiedono firme: CompanyPowerAdmin, CompanyAdmin con `RichiedeFirma`, utenti dello studio abilitato sulle aziende con documentale attivo (S4). Per i documenti del personale valgono anche le abilitazioni esistenti (`VedePersonale`/`CaricaPersonale`).
- R8. Il firmatario vede il documento **solo** in quanto firmatario (diventa destinatario con `Doc_Destinatario`); firmare vale anche come presa visione.
- R9. Dealer e SuperUser non vedono contenuti né richieste (solo consumi aggregati in `CompanySignatureUsage`).

**Privacy**
- R10. Il provider è **responsabile del trattamento** (art. 28 GDPR): DPA firmato, dati in UE. Al provider vanno solo il PDF e i dati minimi dei firmatari. Con l'Opzione D va solo l'hash.
- R11. Snapshot dei dati del firmatario in `Doc_FirmaFirmatario` (prova di chi era il firmatario al momento della firma) e registro in `Doc_Log` / `Doc_FirmaEvento`.

**Cancellazione fisica**
- R12. `DocumentManagementDataDeleter`: elimina anche richieste, eventi, OTP, rapporti di verifica e audit trail dallo storage; **annulla presso il provider** le buste aperte; elimina le righe di `public.SignatureEnvelope` dell'azienda. Rimozione di un utente/dipendente: le firme fatte restano (sono prova), le richieste aperte a suo nome vengono annullate.

---

## 8. Flussi

### 8.1 Richiesta di firma (Opzione C)
1. Dal documento: "**Richiedi firma**" → dialog: livello, firmatari (utenti, dipendenti, contatti clienti, esterni), ordine (in sequenza / in parallelo), scadenza, messaggio.
2. Posizionamento delle firme sull'anteprima PDF (trascinamento di un riquadro per firmatario), oppure posizione automatica in fondo all'ultima pagina, oppure campi firma già presenti nel PDF.
3. Server: controlli (R1, R5, R7), consumo disponibile, `Doc_Firma` in stato Bozza → `CreateEnvelopeAsync` → `Inviata`, riga in `public.SignatureEnvelope`, documento bloccato.
4. Il provider invia email/SMS ai firmatari; per gli utenti dell'app compare anche "**Da firmare**" (badge nel menu, sezione in "I miei documenti") con il pulsante che apre la pagina di firma.
5. Webhook per ogni evento → `Doc_FirmaEvento`, stato del firmatario, notifica SignalR al richiedente.
6. Busta completata → consumer RabbitMQ: download PDF firmato e audit trail → nuova versione (R2) → verifica → stato `Completata`, sblocco → email/SignalR a richiedente e firmatari (con il documento firmato o il link).
7. Rifiuto / scadenza / annullamento → stato corrispondente, sblocco, notifica con il motivo.
8. Promemoria automatici ai firmatari in ritardo (se il provider non li gestisce).

### 8.2 FES (Opzione B)
**Realizzato (11/11/2026), senza OTP:**
1. Documento con `FirmaRichiesta` → email al dipendente con il testo "da firmare" (`NotifyTextSign`), colonna **Firma** = "Da firmare" in "I miei documenti".
2. Il dipendente **scarica** la versione corrente (obbligatorio: il server controlla nel registro il download di quella versione da parte di chi firma, altrimenti `SignDownloadFirst`).
3. **Firma** → conferma ("dichiari di averlo letto e di accettarlo…") → `Doc_DocumentoFirma { Id, Versione }`: se nel frattempo è arrivata una nuova versione la firma si rifiuta (`SignVersionChanged`).
4. Registrazione su `Doc_Destinatario` (data, versione, hash, IP, user agent, utente), presa visione impostata se mancava, registro `Firma` con l'hash.
5. L'amministratore vede firma, versione, hash e IP tra i Destinatari. Una nuova versione azzera la firma; i documenti con firma richiesta o firmati almeno una volta non vengono mai potati (`VersioniDaConservare`).

**Da fare:** OTP via email (`IOtpService` in Infrastructure, `Doc_FirmaOtp`) per legare la firma al possesso della casella, e attestato PDF (eventualmente sigillato) archiviato con il documento.

### 8.3 File firmato caricato (Opzione A)
Caricamento → riconoscimento del formato → (`.p7m`) estrazione del contenuto per anteprima e testo → verifica DSS in background → badge "**Firmato – valido / non valido / da verificare**" e pannello con firmatari, date, emittenti, marca temporale, rapporto scaricabile. Pulsante "Verifica di nuovo" (la validità di un certificato si valuta alla data della firma, ma la revoca può essere scoperta dopo).

### 8.4 Sigillo automatico (U4)
Tipologie con `SigilloAutomatico`: alla pubblicazione (anche nella distribuzione massiva dei cedolini, §7.2 documentale) il PDF viene sigillato prima di essere salvato. Lavoro in background con avanzamento (`long-job-notifier`) per i lotti; il costo per sigillo va nel conteggio dei consumi.

---

## 9. Client (Angular)

| Elemento | Note |
|---|---|
| **Visualizzatore PDF** | prerequisito (anche per l'anteprima già prevista): `pdfjs-dist` / `ngx-extended-pdf-viewer`; con un livello per disegnare i riquadri delle firme |
| Dialog "Richiedi firma" | firmatari con ricerca su utenti, `Dip_Anagrafica`, `Az_ClienteContatto`; ordine; livello; scadenza |
| Elenco "Firme" | `app-generic-list`: richieste per stato, firmatari, scadenza, azioni (promemoria, annulla, scarica audit trail) |
| "Da firmare" | per l'utente/dipendente, con badge nel menu; apre la pagina di firma (redirect o iframe) o il flusso OTP |
| Pannello verifica | esito, firmatari, rapporto DSS |
| Archivio | colonna/filtro "Stato firma", icona sui documenti firmati, download "originale firmato" / "contenuto in chiaro" per i `.p7m` |
| Configurazione | in `Doc_Cfg` e `Doc_Par_Tipo`; abilitazione `RichiedeFirma` nella pagina Abilitazioni |
| SuperUser / centro | provider, consumi per azienda ed export (come lo spazio) |

Multilingua: chiavi in `DocumentManagementMessages.resx/.en.resx` e i18n `document-management`.

---

## 10. Docker e configurazione

- nuovo servizio **`dss`** (immagine EU DSS / demo webapp, o immagine costruita dai sorgenti) in `docker-compose*.yml`, rete interna, nessuna porta pubblica; cache delle liste di fiducia su volume; healthcheck. Richiede ~1 GB di RAM (Java).
- facoltativo **`gotenberg`** per la conversione in PDF.
- `appsettings`: sezione `Signature` (`Provider`, `Validator`, `DssUrl`, `WebhookPath`, `EnvelopeTimeoutDays`, `PollingMinutes`); segreti vuoti in `appsettings.json` e valorizzati con le variabili d'ambiente `NVXAPP_Signature__ApiKey`, `NVXAPP_Signature__WebhookSecret` (come gli altri, `docs/imposta-variabili-nvxapp.ps1`, `DockerVps.env`), mai nel repository.
- **memoria**: `docker-compose.prod.yml` limita oggi il VPS a 2 GB; il container DSS (Java) ne chiede circa 1. Va previsto più spazio sul VPS, oppure DSS su un'altra macchina, oppure la verifica del provider (decisione §12.8).
- aggiornare `ANALISI_INFRASTRUCTURE_DOCKER.md` (servizi, backup del volume DSS non necessario: si ricostruisce).

---

## 11. Rischi e punti di attenzione

| Rischio | Mitigazione |
|---|---|
| Dipendenza da un provider (API, prezzi) | `ISignatureProvider`, dati di firma nel nostro DB, PDF firmati e audit trail sempre scaricati e archiviati da noi |
| Webhook persi o duplicati | idempotenza per id evento + polling di recupero |
| Documento modificato durante la firma | blocco della versione (R1) e confronto degli hash (R2) |
| Firme che "scadono" (certificato scaduto/revocato) | marca temporale (B-T/B-LTA), conservazione a norma in futuro |
| Costi non controllati | consumi per azienda, eventuali pacchetti, blocco come per lo spazio |
| FEA in casa con obblighi legali | non farla: FEA solo tramite provider |
| URL pubblico per il webhook in sviluppo | polling come alternativa, ambiente sandbox del provider |
| DSS pesante (Java, ~1 GB, limite attuale VPS 2 GB) | servizio facoltativo: senza DSS si mostra solo la verifica di integrità ("Firmato – non verificato") |

---

## 12. Decisioni da prendere

1. **Casi d'uso prioritari** (§3): quali tra U1-U7 servono nella prima versione?
2. **Livelli**: basta FES + verifica (F1) all'inizio, o serve subito FEA/FEQ (F2)? *(11/11/2026: avviata la FES con un clic; richiesta proposta dalla tipologia e decisa a ogni invio, §7.1)*
3. **Provider**: quale QTSP (InfoCert, Aruba, Namirial, Intesi Group, altri)? Criteri: API REST documentata e sandbox, FEA e FEQ one-shot con riconoscimento SPID/CIE, firma incorporabile, sigillo, webhook, CSC API, prezzo per firma, dati in Italia/UE.
4. **Contratto**: uno per installazione (chi gestisce l'applicazione rivende le firme alle aziende) o uno per azienda/studio (ognuno le proprie credenziali)?
5. **Fatturazione**: solo conteggio dei consumi (`CompanySignatureUsage`) o anche **pacchetti** con blocco al raggiungimento (`SignaturePlan`)? Chi li assegna (SuperUser/DealerPowerAdmin come per lo spazio)?
6. **Sigillo** dello studio/azienda sui documenti pubblicati: sì/no, e di chi (studio o azienda)?
7. **OTP via SMS** per la FES: sì (serve un gateway) o solo email?
8. **Verifica**: container EU DSS (consigliato) o servizio di verifica del provider?
9. **Lo studio** firma documenti per conto delle aziende (es. lettere d'incarico) con certificato proprio (Opzione D) già nella F2?

---

## 13. Piano di lavoro

Legenda: [ ] da fare. Stime indicative in giornate/uomo.

### Fase F0 – Prerequisiti
- [ ] visualizzatore PDF nel client (anteprima, già in Fase 1 documentale) – 2 gg
- [ ] `Doc_Versione`: campi `Firmata`, `FormatoFirma`, `IdDoc_VersioneOrigine`, `StorageKeyContenuto`; migration – 0,5 gg

### Fase F1 – Verifica, firma esterna, FES via email (nessun provider) – ~10 gg
- [ ] Infrastructure: `ISignatureValidator`, implementazione .NET (integrità `SignedCms`, estrazione `.p7m` annidati) e implementazione **EU DSS** (REST); servizio `dss` in docker-compose
- [ ] riconoscimento dei file firmati al caricamento, estrazione del contenuto per anteprima e **ricerca nel testo** (`FileText` sul contenuto estratto)
- [ ] `Doc_VerificaFirma`, verifica in background (RabbitMQ), pannello verifica e badge nell'archivio, "Verifica di nuovo"
- [ ] firma esterna: "Scarica per firmare" / "Carica versione firmata" con confronto del contenuto
- [x] `Doc_Par_Tipo.FirmaRichiesta` (valore proposto) e `Doc_Documento.FirmaRichiesta` (scelta all'invio, modificabile dopo) – 11/11/2026
- [x] flusso "Scarica e firma" FES con un clic: `Doc_DocumentoFirma`, prove su `Doc_Destinatario`, registro, nuova versione che azzera la firma, potatura esclusa, pagine Tipologia / Documento / I miei documenti, manuale – 11/11/2026
- [ ] Infrastructure: `IOtpService` (email), `Doc_FirmaOtp`, OTP nel flusso di firma, attestato PDF
- [ ] `LivelloFirmaPredefinito`, regole R1-R4 complete (blocco del documento durante la richiesta), deleter (R12)
- [ ] prove API e browser (dipendente, CompanyPowerAdmin)

### Fase F2 – Provider QTSP – ~15-20 gg (dipende dal provider)
- [ ] scelta del provider e contratto sandbox (§12.3-4)
- [ ] Infrastructure: `ISignatureProvider`, `public.SignatureEnvelope`, `SignatureProviderConfig`, `CompanySignatureUsage`, webhook con verifica HMAC, polling di recupero
- [ ] implementazione del primo provider
- [ ] `Doc_Firma`, `Doc_FirmaFirmatario`, `Doc_FirmaEvento`; servizio, API, consumer RabbitMQ (download, nuova versione, verifica, notifiche)
- [ ] client: dialog "Richiedi firma" con posizionamento sull'anteprima, elenco Firme, "Da firmare", notifiche SignalR
- [ ] `Doc_UtenteAbilitazione.RichiedeFirma`, controlli R7-R9
- [ ] sigillo automatico per tipologia (se deciso), anche nella distribuzione massiva
- [ ] pagine consumi per SuperUser/centro ed export Excel; pacchetti (se decisi)
- [ ] manuali utente (`Manuals/documents*.md`) e multilingua

### Fase F3 – Estensioni (su richiesta)
- [ ] firma remota con certificato del firmatario via **CSC API** (Opzione D), PAdES B-T/B-LTA
- [ ] OTP via SMS
- [ ] secondo provider
- [ ] integrazione CRM (offerte firmate dai clienti) e presenze (contratti nel fascicolo del dipendente) tramite le interfacce di Infrastructure
- [ ] invio dei documenti firmati in conservazione a norma
