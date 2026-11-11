# Presenze: smart working / lavoro agile (AT-SW)

Data: 11/11/2026. Ramo di lavoro `AttendanceTracking_Dev_f4` (worktree `wt_at-f4`), da `AttendanceTracking_Dev`.
Area `AT-SW` di `ANALISI_INFRASTRUCTURE_NUOVI_MODULI.md` (§3.1, §10.2).

## 1. Obiettivo

1. **Accordo individuale** di lavoro agile (L. 81/2017): periodo, giorni massimi a settimana, giorni della settimana consentiti; firma o presa visione del dipendente tramite `IAcknowledgementService` (INF-SVC.6). Con il Documentale attivo il canale di firma (DM-FIRMA) genera il **verbale di accordo** perché la fonte contiene `accordo`.
2. **Calendario del lavoro agile**: il dipendente vede i giorni consentiti e quelli richiesti/approvati, richiede un giorno, l'approvazione segue il flusso delle richieste; il limite settimanale e i giorni consentiti si controllano alla richiesta.
3. **Giustificativo dedicato** nel calcolo della giornata.

## 2. Situazione di partenza

- I giustificativi (`Par_Giustificativi`) hanno già flag che cambiano il calcolo (`BancaOre`, `AutorizzaStraordinario`, `AnnullaPausa`); la richiesta di un giustificativo (`Dip_GG_Richiesta` di tipo `Giustificativo`) crea un `Dip_GG_Giustificativi` per ogni giorno lavorativo e passa dagli approvatori del reparto.
- `GeneraCausali` del motore accumula le ore del giustificativo sulla sua causale; il segno decide l'effetto sul lavorabile (− assenza, + aumento, neutro nessuno).
- `IAcknowledgementService.RequestAsync` (Infrastructure) crea la presa visione per utente e oggetto e sceglie il canale (conferma semplice o firma del Documentale).

## 3. Modello dati (migration `AttendanceTracking_SmartWorking`)

| Tabella | Campi | Note |
|---|---|---|
| `Dip_AccordoSW` | `IdDip_RapportoLavoro`, `Dal`, `Al`(date), `MaxGiorniSettimana`, `GiorniConsentiti`(7), `Note`(500)?, `DataRichiestaFirma`?, `DataFirma`? | Accordo del rapporto di lavoro. `GiorniConsentiti` = maschera lun..dom (`1111100`). Periodi dello stesso rapporto non sovrapposti |
| `Par_Giustificativi.LavoroAgile` | bool | Nuovo flag: giustificativo del lavoro agile (uno per azienda) |

Presa visione: `Fonte = "at.accordo-sw"`, `IdRiferimento = Id` dell'accordo, destinatario il dipendente, testo con le condizioni dell'accordo. `DataFirma` si aggiorna leggendo lo stato (`StatusAsync`) quando si legge l'accordo.

## 4. Regole

- **Giustificativo predefinito**: causale `SW` "Lavoro agile" e giustificativo `SW` "Lavoro agile" (giornata intera, segno **neutro**, `VisualizzaInPianoFerie`, nessun contatore, `LavoroAgile = true`). Creati con i parametri delle nuove aziende e, per quelle esistenti, al primo uso (accordi o calendario).
- **Richiesta di un giorno** = richiesta di giustificativo con il giustificativo `LavoroAgile` (pagina del calendario o pagina "Richiesta giustificativo"). In `Dip_GG_RichiestaService.Send` si controlla per ogni giorno: accordo valido quel giorno e **firmato** (presa visione confermata), giorno della settimana consentito, giorni di lavoro agile della settimana (lun-dom: richiesti in attesa + approvati + nuovi) ≤ `MaxGiorniSettimana`. Se non va, la richiesta è rifiutata con il motivo. Vale anche per l'HR che inserisce dalla richiesta (`FromHR`); l'inserimento diretto del giustificativo dall'HR (cartellino) non è controllato (forzatura).
- **Calcolo della giornata** (AT-SW.3): nessuna modifica del motore. Il giustificativo neutro mette le ore del giorno sulla causale `SW` (marcatore per l'export paghe) e non cambia il dovuto: il dipendente timbra da remoto (app) come in sede e le ore lavorate restano sulle causali ordinarie. Chi non vuole le timbrature nei giorni agili può dare al giustificativo il segno "−" (le ore diventano giustificate). Nei giorni agili approvati il geofence non si applica (AT-TIMB).

## 5. API e client

Server `Dip_AccordoSW`: `GetAll` (HR, anche per dipendente), `Dip_AccordoSWGet`, `Dip_AccordoSWPut` (salva e chiede la presa visione), `Dip_AccordoSWDelete` (ritira la presa visione non confermata), `Calendario` (dipendente o HR: accordi, giorni consentiti, giorni agili del mese con stato, conteggio per settimana, giustificativo da usare).
Client: `/accordiswlist`, `/accordoswedit` (CompanyAdmin/CompanyPowerAdmin), `/lavoroagile` (dipendente: calendario del mese, richiesta del giorno), flag "Lavoro agile" nella modifica del giustificativo.

## 6. Decisioni

| # | Decisione | Esito |
|---|---|---|
| S1 | Causale o giustificativo | **Scelta provvisoria**: giustificativo dedicato con flag `LavoroAgile` e causale `SW`, segno neutro |
| S2 | Richiesta del giorno | **Scelta provvisoria**: riuso della richiesta di giustificativo (stessi approvatori, stato, notifiche, revoca) con il controllo dell'accordo in `Send` |
| S3 | Accordo non ancora firmato | **Scelta provvisoria**: i giorni agili non si possono richiedere finché la presa visione non è confermata |
| S4 | Settimana del limite | Lunedì-domenica; contano i giorni in attesa e approvati |
| S5 | Accordi sovrapposti | Non ammessi per lo stesso rapporto |
| S6 | Più giustificativi con `LavoroAgile` | **Scelta provvisoria**: si usa il primo (Id più basso) per il calendario; il controllo vale per tutti |

## 7. Checklist

Legenda: ✅ svolto · ⏳ da fare · 🔄 in corso

- ✅ `AT-SW.0` Analisi (questo documento)
- ✅ `AT-SW.1` Accordi individuali: `Dip_AccordoSW`, API, presa visione `at.accordo-sw` (verbale di accordo con il Documentale), pagine `/accordiswlist` e `/accordoswedit`
- ✅ `AT-SW.2` Calendario del lavoro agile: API `Calendario`, controllo di accordo, giorni consentiti e limite settimanale nella richiesta; pagina `/lavoroagile`
- ✅ `AT-SW.3` Giustificativo `SW` (flag `LavoroAgile`, causale `SW`) nei parametri di default e al primo uso; geofence escluso nei giorni agili; flag nella pagina del giustificativo
- ✅ `AT-SW.4` Prova su DB usa e getta (accordo con presa visione, giorno oltre il limite rifiutato, giorno non consentito rifiutato, calcolo con causale `SW`) — 32 controlli API: permessi, presa visione `at.accordo-sw` creata (canale base, il Documentale non è nel ramo), accordo sovrapposto e giorni massimi non validi rifiutati, giustificativo SW creato al primo uso, richiesta con accordo non firmato rifiutata, conferma della presa visione e data di firma, giorno consentito accettato, giorno non consentito / oltre il limite / doppio / senza accordo rifiutati, altri giustificativi invariati, approvazione e calendario (stati e conteggio settimanale), calcolo: causale SW 8 ore con dovuto invariato, geofence non applicato nel giorno agile approvato, nuova presa visione dopo la modifica delle condizioni. Regressione su un'azienda nuova (13 controlli): giustificativo SW nei parametri di default, calcolo senza turni con gli stessi risultati di prima (8 ore, ORD1)
