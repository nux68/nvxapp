# Presenze: timbratura evoluta (AT-TIMB)

Data: 11/11/2026. Ramo di lavoro `AttendanceTracking_Dev_f4` (worktree `wt_at-f4`), da `AttendanceTracking_Dev`.
Area `AT-TIMB` di `ANALISI_INFRASTRUCTURE_NUOVI_MODULI.md` (§3.1, §10.2).

## 1. Obiettivo

1. **QR per sede**: una pagina "postazione" mostra un QR firmato che cambia ogni pochi secondi; il dipendente lo legge con il telefono e timbra. NFC come estensione.
2. **Geofencing**: coordinate e raggio della sede; la posizione inviata dal telefono si confronta con la sede e l'esito resta sulla timbratura; regola configurabile (segnala o blocca).
3. **Kiosk**: un dispositivo condiviso a schermo intero in sede, il dipendente si identifica con PIN o badge.
4. **Offline (PWA)**: senza rete la timbratura resta in una coda locale con l'ora del dispositivo e parte alla riconnessione; il server accetta le timbrature differite entro un limite e le marca.
5. **Sicurezza**: nessuna timbratura per conto di altri senza permesso, QR non riutilizzabile.

## 2. Situazione di partenza

- `Dip_GG_TimbraturaService.Stamp`: timbra l'utente corrente con data e ora **mandate dal client** (la pagina `/timeclockuser` permette di cambiarle), su un'attività consentita, poi ricalcola il giorno.
- `Dip_GG_TimbraturaPut`: inserimento/modifica (HR) con mappatura completa modello → entità: campi aggiunti a `Dip_GG_Timbratura` e non presenti nel modello verrebbero azzerati a ogni modifica e nel salvataggio del motore.
- Le sedi (`Az_Sedi`) sono nello schema **Common** e non si modificano da questo ramo.
- Il client non ha un service worker (`@angular/service-worker` non installato) né librerie QR; sul server è disponibile nella cache NuGet `QRCoder` 1.6.0.

## 3. Modello dati (contesto `AttendanceTrackingDbContext`, migration `AttendanceTracking_Timbratura`)

| Tabella | Campi | Note |
|---|---|---|
| `Az_SediTimbratura` | `IdAz_Sedi` (univoco), `Latitudine`?, `Longitudine`?, `RaggioMetri`, `RegolaGeofence` (Nessuna, Segnala, Blocca), `QrAttivo`, `QrObbligatorio`, `QrSegreto`(64), `QrDurataSecondi`, `KioskAttivo`, `KioskToken`(64)?, `DifferitaMaxOre` | Configurazione della timbratura per sede, **tabella Presenze collegata alla sede comune** (FK Cascade su `Az_Sedi`), Common non cambia |
| `Dip_GG_TimbraturaInfo` | `IdDip_GG_Timbratura` (univoco), `IdDip_RapportoLavoro`, `IdAz_Sedi`?, `Origine` (App, Qr, Kiosk), `Differita`, `DataDispositivo`?, `DataRicezione`, `Latitudine`?, `Longitudine`?, `PrecisioneMetri`?, `DistanzaMetri`?, `EsitoGeofence` (NonRichiesto, Dentro, Fuori, PosizioneMancante, LavoroAgile), `QrSlot`?, `IdAspNetUsers_Operatore`? | Esito registrato **sulla timbratura** in una tabella 1:1 (FK Cascade): non si perde nelle modifiche dell'HR. Indice univoco filtrato (`IdAz_Sedi`, `QrSlot`, `IdDip_RapportoLavoro`) = anti-replay |
| `Dip_TimbraturaPin` | `IdDip_Anagrafica` (univoco), `IdAz_Anagrafica`, `PinHash`(100)?, `Badge`(50)? | Credenziali del kiosk. PIN come HMAC-SHA256 (chiave del server + azienda), indici univoci azienda+PIN e azienda+badge |

## 4. Regole

### 4.1 Timbratura evoluta (`TimbraturaEvoluta/Stamp`)
Dati dal client: attività, codice QR (facoltativo), posizione (lat, lon, precisione; facoltativa), `Offline` e data/ora del dispositivo, sede (facoltativa).
1. **Sede**: quella del QR; altrimenti quella indicata; altrimenti la sede del primo reparto del dipendente (`Az_SediRepartoUser.UserInDepartment`). Sede senza configurazione = nessun controllo (come la timbratura di oggi, ma con l'ora del server).
2. **Ora**: online vale **l'ora del server**. Offline (`Offline = true`) vale l'ora del dispositivo, accettata se non nel futuro (tolleranza 5 minuti) e non più vecchia di `DifferitaMaxOre` (0 = differite non accettate); la timbratura è marcata `Differita` con data del dispositivo e data di ricezione.
3. **QR**: codice `<idSede>.<slot>.<firma>` con `slot = secondi Unix / QrDurataSecondi` e `firma = HMAC-SHA256(QrSegreto, idSede.slot)` (base64url, 16 byte). Valido se la firma torna e lo slot è quello dell'ora della timbratura o il precedente. Lo stesso codice usato una seconda volta dallo stesso dipendente è rifiutato (anti-replay); più colleghi possono usare lo stesso codice esposto. Con `QrObbligatorio` la timbratura senza QR è rifiutata.
4. **Geofence**: con coordinate e regola ≠ Nessuna si calcola la distanza (formula dell'emisenoverso). Dentro se distanza ≤ raggio + precisione (massimo 100 m di tolleranza). Fuori o posizione mancante: con `Segnala` la timbratura si salva con l'esito; con `Blocca` è rifiutata. Nei giorni di **lavoro agile approvato** (AT-SW) il geofence non si applica (esito `LavoroAgile`).
5. Si salva la timbratura con `Dip_GG_TimbraturaPut` (stesse regole: attività proposta, giorno nel rapporto) e poi `Dip_GG_TimbraturaInfo`; ricalcolo del giorno.

### 4.2 Postazione QR (`TimbraturaEvoluta/QrCode`)
Solo CompanyAdmin/CompanyPowerAdmin: restituisce il codice corrente della sede, la scadenza e l'immagine PNG (QRCoder) dell'indirizzo `<origine del client>/timbraturaqr?c=<codice>`. Il telefono lo legge con la fotocamera (o con il lettore della pagina, `BarcodeDetector` dove disponibile) e apre la pagina `/timbraturaqr`, che legge la posizione e timbra.

### 4.3 Kiosk (`TimbraturaEvoluta/KioskRegistra`, `KioskStamp`)
- Il kiosk è un dispositivo con un utente **CompanyAdmin/CompanyPowerAdmin** collegato (permesso di timbrare per altri) e **registrato** sulla sede: `KioskRegistra` genera un `KioskToken` casuale (32 byte) salvato nel dispositivo; registrare di nuovo invalida il precedente.
- `KioskStamp` (sede, token, PIN o badge): token e `KioskAttivo` verificati, dipendente dell'azienda trovato da PIN (HMAC) o badge, ora del server, `Origine = Kiosk`, operatore = utente del dispositivo. Dopo 5 errori in un minuto per dispositivo il kiosk resta bloccato per un minuto.
- PIN e badge li imposta **solo l'HR** (`TimbraturaEvoluta/PinPut`): il PIN è unico nell'azienda e, se lo scegliesse il dipendente, il controllo di unicità gli rivelerebbe i PIN dei colleghi.

### 4.4 Offline
Coda nel `localStorage` del client (`TimbraturaOfflineService`): se la chiamata fallisce per rete (o il browser è offline) la timbratura resta in coda con `dataDispositivo`; all'evento `online`, all'avvio della pagina e con "Invia ora" si inviano in ordine con `Offline = true`. Le risposte di rifiuto (oltre il limite) tolgono la timbratura dalla coda e restano visibili come scartate.

## 5. Impatti sul calcolo
Nessuno: la timbratura evoluta crea una normale `Dip_GG_Timbratura` (stato Diretta). Le informazioni aggiuntive sono solo in `Dip_GG_TimbraturaInfo` e nell'elenco del giorno (`TimbraturaEvoluta/Info4Giorno`).

## 6. Decisioni

| # | Decisione | Esito |
|---|---|---|
| B1 | Dove salvare coordinate e regole della sede senza toccare Common | **Scelta provvisoria**: `Az_SediTimbratura` nelle Presenze, 1:1 con `Az_Sedi` |
| B2 | Dove registrare l'esito | **Scelta provvisoria**: tabella 1:1 `Dip_GG_TimbraturaInfo` (non colonne di `Dip_GG_Timbratura`, che il modello mappato azzererebbe) |
| B3 | QR | **Scelta provvisoria**: HMAC per sede, finestra = slot corrente + precedente (durata default 30 s), anti-replay per dipendente; immagine generata dal server con `QRCoder` 1.6.0 (nuovo pacchetto del servizio) |
| B4 | NFC | **Estensione documentata, non implementata**: un tag NFC statico con l'indirizzo `/timbraturaqr?n=<codice tag>` si leggerebbe con la Web NFC (`NDEFReader`, solo Chrome Android) o aprendo l'URL; un tag statico non ruota e quindi non è anti-replay (va unito al geofence) |
| B5 | Kiosk | **Scelta provvisoria**: dispositivo con un amministratore dell'azienda collegato + token del dispositivo per sede; il dipendente si identifica con PIN (4-8 cifre) o badge |
| B6 | Ora della timbratura evoluta | **Scelta provvisoria**: ora del server online, ora del dispositivo solo per le differite entro `DifferitaMaxOre` (default 24) |
| B7 | PWA offline | **Scelta provvisoria**: coda in `localStorage` e invio alla riconnessione. Il caricamento dell'app senza rete richiede un service worker (`@angular/service-worker`, non installato): estensione |
| B8 | Timbratura semplice esistente (`/timeclockuser`) | Invariata |
| B9 | Geofence nei giorni di lavoro agile | Non si applica se il giorno ha un giustificativo di lavoro agile approvato (AT-SW) |

## 7. Checklist

Legenda: ✅ svolto · ⏳ da fare · 🔄 in corso

- ✅ `AT-TIMB.0` Analisi (questo documento)
- ✅ `AT-TIMB.1` QR per sede: configurazione della sede, codice firmato e rotante, immagine, anti-replay; pagine `/timbraturapostazione` e `/timbraturaqr`; NFC documentato
- ✅ `AT-TIMB.2` Geofencing: coordinate, raggio e regola per sede, esito registrato su `Dip_GG_TimbraturaInfo`, blocca o segnala
- ✅ `AT-TIMB.3` Kiosk: registrazione del dispositivo, PIN/badge, `KioskStamp`, pagina `/timbraturakiosk` a schermo intero, limite dei tentativi
- ✅ `AT-TIMB.4` Offline: coda locale, invio alla riconnessione, differite accettate entro il limite e marcate
- ✅ `AT-TIMB.5` Prova su DB usa e getta (QR valido/riusato, fuori dal geofence, differita accettata/rifiutata, kiosk con PIN) — 41 controlli API: configurazione (permessi, coordinate obbligatorie con il geofence), QR con immagine, QR valido, riusato dallo stesso dipendente (rifiutato), usato da un collega (accettato), firma falsa e QR scaduto rifiutati, fuori area con "segnala" (accettata, esito e distanza registrati) e con "blocca" (rifiutata, anche senza posizione), tolleranza della precisione, QR obbligatorio, differita accettata con l'ora del dispositivo e marcata, oltre il limite / nel futuro / sede senza differite rifiutate, PIN solo dall'HR e salvato come hash, kiosk con token sbagliato, utente non amministratore, PIN errato (rifiutati), PIN e badge corretti, blocco dopo 5 errori, nuova registrazione che invalida il token
