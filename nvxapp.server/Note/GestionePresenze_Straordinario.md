# Presenze: straordinario e lavoro supplementare

Analisi di come il programma calcola le ore lavorate in più rispetto all'orario.
Spiega cosa sono lo straordinario e il supplementare, quali parametri li governano, come si svolge il calcolo, cosa significa "provvisorio" e come l'HR valida il mese.

Riferimenti nel codice:

- `nvxapp.server.service/ClientServer-Service/GestionePresenze/_utility/GestionePresenzeEccedenze.cs`: la qualifica delle ore, cioè la logica pura, senza database.
- `nvxapp.server.service/ClientServer-Service/GestionePresenze/TimeSheet_EngineService/TimeSheet_EngineService.cs`: il motore di calcolo (metodi `CalcolaRapporto`, `CalcolaGiorno`, `GeneraCausali`).
- `nvxapp.server.service/ClientServer-Service/GestionePresenze/Dip_PeriodoService/Dip_PeriodoService.cs`: il cartellino mensile, cioè approvazione e ore riconosciute.

---

## 1. Concetti

| Tipo di ora | Chi | Quando |
|---|---|---|
| **Ordinaria** | tutti | ore lavorate entro le ore *lavorabili* del giorno |
| **Supplementare** | solo part-time | ore oltre l'orario contrattuale ridotto ma entro la soglia full-time (es. dalla 21ª alla 40ª ora settimanale) |
| **Straordinario** | full-time e part-time | ore oltre la soglia full-time del periodo di conteggio |
| **Ore eccedenti non straordinarie** | full-time | ore oltre il lavorabile che non superano la soglia: per esempio 2 ore in più nel giorno in cui si sono prese 4 ore di ferie |

Le **ore lavorabili** di un giorno sono le ore teoriche dell'orario, meno i giustificativi che riducono il dovuto (ferie, permessi, malattia, assenze; segno `-`), più quelli che lo aumentano (segno `+`).

Le **ore in più** di un giorno sono le ore lavorate meno le ore lavorabili. Se il risultato è negativo, sono **ore mancanti**.

---

## 2. Dove si configura

### 2.1 Profilo orario, scheda "Straordinario"

| Parametro | Significato | Default |
|---|---|---|
| Calcola straordinario e supplementare | Se è spento, le ore in più restano solo segnalate: niente qualifica e niente causali | sì |
| Periodo di conteggio | Giornaliero, Settimanale (lunedì-domenica) o Mensile: il periodo su cui si decide se un'ora è straordinario o supplementare (vedi §4) | Giornaliero |
| Soglia giornaliera full-time | Soglia dello straordinario per i part-time nel conteggio giornaliero. Per i full-time è un tetto al teorico del giorno | 8:00 |
| Soglia settimanale full-time (ore) | Soglia dello straordinario nel conteggio settimanale. È anche la base del riproporzionamento del part-time nel conteggio mensile | 40 |
| Limite supplementare (%) | Percentuale delle ore settimanali contrattuali entro cui il supplementare è "standard"; oltre è "oltre il limite" (D.Lgs 81/2015: 25%) | 25 |
| Ore in più riconosciute a blocchi di | Arrotondamento per difetto delle ore in più del giorno (1, 5, 15, 30, 60 minuti) | 15 |
| Franchigia giornaliera | Sotto questi minuti le ore in più del giorno non si riconoscono | 0 |
| Ferie e permessi contano per la soglia | Se attivo, le assenze giustificate contano come lavorate per raggiungere la soglia dello straordinario | no |
| Domenica festiva | La domenica conta come giorno festivo per lo straordinario | sì |
| Notturno dalle / alle | Fascia notturna | 22:00 - 06:00 |
| Causali | Una causale per ogni tipo di ora (vedi §6). Quelle facoltative, se vuote, ricadono sulla causale base | |

Nella scheda "Generale" resta l'**orario festivo**: è l'orario che vale nei giorni del calendario festività (di solito RIPOSO, 0 ore teoriche).

### 2.2 Scheda dipendente, rapporto di lavoro, sezione "Contratto"

| Campo | Uso |
|---|---|
| Tipo contratto | Full-time o Part-time: decide se il resto delle ore in più diventa supplementare (part-time) o ore eccedenti (full-time) |
| Ore settimanali contrattuali | Solo part-time: base del limite % del supplementare e del riproporzionamento mensile. Se è vuoto si usano le ore teoriche della settimana |
| CCNL | Informativo |
| Straordinario e supplementare | **Pagamento in busta paga** oppure **Accredito in banca ore** |

### 2.3 Calendario festività (menu Parametri, "Festività")

Ogni festività ha un tipo:

- **Ogni anno, stesso giorno**: per esempio il 25/12. L'anno della data non conta.
- **Solo nella data indicata**: per esempio un ponte aziendale o un patrono spostato.
- **Lunedì dell'Angelo**: è calcolato ogni anno dalla data di Pasqua.

Alla creazione dell'azienda vengono caricate le festività nazionali.

In un giorno festivo:

- il giorno usa l'**orario festivo** del profilo, quindi normalmente 0 ore teoriche;
- le ore lavorate quel giorno sono ore in più e, se diventano straordinario, si qualificano come **straordinario festivo**;
- la domenica conta come festiva se il profilo lo prevede.

Le stesse festività valgono anche nel calcolo delle ore teoriche delle richieste. Per esempio, le ferie non vengono scalate in un giorno festivo.

### 2.4 Giustificativo "Recupero banca ore"

È un giustificativo con contatore e con l'opzione "Recupero banca ore" attiva.

- Il **maturato** del contatore sono le ore accreditate in banca ore nei mesi **approvati**.
- Il **goduto** sono i recuperi effettivamente presi.

---

## 3. Come funziona il calcolo

Il calcolo parte come sempre dal Calendario HR ("Calcola", "Calcola tutto il mese" e simili) e gira in background. Lavora in due fasi.

### 3.1 Fase giornaliera (ogni giorno)

1. **Timbrature.** Le timbrature si abbinano alle coppie dell'orario, con le tolleranze e gli arrotondamenti dell'orario.
   - Le timbrature **oltre il numero di coppie** (per esempio un rientro serale 21-23) e quelle dei **giorni senza coppie** (riposo, festivo) si accoppiano in ordine cronologico (entrata, uscita) e contano come lavoro fuori orario, senza arrotondamento.
   - Una coppia può superare la mezzanotte: si usa la data completa.
2. **Ore lavorate e ore lavorabili.** Si calcolano le ore lavorate e quelle lavorabili (teoriche più o meno i giustificativi).
3. **Ore ordinarie.** Sono il minimo tra lavorate e lavorabili e vanno sulle causali delle fasce dell'orario.
   - Le ore ordinarie fatte fuori dalle fasce (per esempio entrata in ritardo recuperata la sera) vanno sulla causale della prima fascia.
4. **Ore in più del giorno** = lavorate - lavorabili.
   - Se sono positive si arrotondano **per difetto** al blocco del profilo e si azzerano se restano sotto la **franchigia**.
   - Se sono negative restano come **ore mancanti**: nel conteggio settimanale o mensile servono a compensare.
5. **Parte notturna.** Sono le ore lavorate fuori dalle coppie dell'orario che cadono nella fascia notturna (al massimo quanto le ore in più).
6. **Giorno festivo.** Il giorno è festivo se è nel calendario festività, oppure se è domenica e il profilo considera festiva la domenica.
7. **Stato del giorno.** Le ore in più segnano il giorno con un **avviso** (`Warning_1`), non con un errore: sono una situazione normale, che il calcolo qualifica. Le ore mancanti restano un errore (`Err_2`).

Le ore in più sono quindi le ore lavorate **fuori** dall'orario: prima dell'inizio, dopo la fine, nei rientri, nei giorni di riposo.

### 3.2 Fase di periodo (qualifica)

Per ogni periodo di conteggio toccato dal ricalcolo (vedi §4) si sommano i giorni del periodo:

```
Ore in più del periodo  = max(0, somma delle ore in più e mancanti dei giorni)
X (ore utili per soglia) = ore lavorate del periodo (+ assenze, se "Ferie e permessi contano per la soglia")
Soglia full-time        = vedi tabella sotto
Straordinario           = X - Soglia, limitato tra 0 e le ore in più del periodo
Resto                   = ore in più - straordinario
                          part-time -> lavoro supplementare
                          full-time -> ore eccedenti non straordinarie
```

**Soglia full-time del periodo**

| Conteggio | Full-time | Part-time |
|---|---|---|
| Giornaliero | teorico del giorno (non oltre la soglia giornaliera del profilo) | soglia giornaliera FT (es. 8h) |
| Settimanale | teorico della settimana (non oltre la soglia settimanale, es. 40h) | soglia settimanale FT (es. 40h) |
| Mensile | teorico del mese | teorico del mese × soglia settimanale FT / ore settimanali contrattuali |

Per il full-time la soglia segue il teorico. Per questo:

- un sabato di riposo lavorato (teorico 0) è tutto straordinario;
- in una settimana con un festivo la soglia scende di conseguenza.

**Attribuzione ai giorni**

Le ore qualificate si attribuiscono ai giorni **partendo dall'ultimo** con ore in più del periodo, perché le ore oltre la soglia sono le ultime lavorate: prima lo straordinario, poi il resto.

- Le ore in più dei primi giorni che non ricevono una qualifica sono **compensate** dalle ore mancanti dello stesso periodo.
- Così ogni ora qualificata ha un giorno di provenienza (dettaglio "causa-effetto" nel riepilogo).

**Notturno e festivo**

Nel giorno, lo straordinario è:

- **notturno** fino alla parte notturna delle ore in più;
- **festivo** (o festivo notturno) se il giorno è festivo.

**Limite del supplementare**

Il supplementare entro la percentuale delle ore settimanali contrattuali è "standard", oltre è "oltre il limite". Il limite si consuma in ordine cronologico nella settimana lunedì-domenica, qualunque sia il periodo di conteggio.

**Tetto dell'HR**

Se l'HR ha fissato le **ore riconosciute** del mese (vedi §5), le ore qualificate oltre il tetto si tolgono partendo dagli ultimi giorni e diventano **non riconosciute**.

### 3.3 Causali

Per ogni giorno il motore scrive:

- le causali dei **giustificativi**;
- le **ordinarie** sulle causali delle fasce;
- le causali delle **ore qualificate**:

| Ore | Causale (profilo) | Se vuota |
|---|---|---|
| Straordinario diurno | Straordinario | obbligatoria |
| Straordinario notturno | Straordinario notturno | Straordinario |
| Straordinario festivo | Straordinario festivo | Straordinario |
| Straordinario festivo notturno | Straordinario festivo notturno | festivo, poi notturno, poi Straordinario |
| Supplementare entro il limite | Supplementare | obbligatoria |
| Supplementare oltre il limite | Supplementare oltre il limite | Supplementare |
| Ore eccedenti (full-time) | Ore eccedenti non straordinarie | Straordinario |

Se il dipendente ha la destinazione **banca ore**, tutte le ore qualificate vanno sulla causale **Accredito banca ore** del profilo.

Le causali sono quelle lette dall'export paghe: per esportarle vanno aggiunte al modello di export.

Seed di default (profilo DAY7H8):

- causali STRAO, STRNOT, STRFES, STRFN, SUPPL, SUPEC, ECCOR, BOACC;
- conteggio giornaliero;
- orario festivo RIPOSO.

---

## 4. Periodo di conteggio e "provvisorio"

Il periodo di conteggio decide **quando** un'ora in più diventa straordinario o supplementare:

- **Giornaliero.** Ogni giorno si chiude da solo: le ore in più di un giorno non compensano le ore mancanti di un altro.
- **Settimanale** (lunedì-domenica). Le ore in più e mancanti della settimana si compensano tra loro. Lo straordinario c'è solo se nella settimana si supera la soglia (es. 40h).
- **Mensile.** La compensazione avviene sull'intero mese.

**Finché il periodo non è finito, la qualifica non è definitiva.** Un'ora in più di lunedì potrebbe essere compensata da un'ora in meno di giovedì. Per questo, mentre il periodo è in corso:

- nel giorno le ore in più restano **provvisorie**, "in attesa di quadratura";
- non viene scritta nessuna causale di straordinario o supplementare;
- il riepilogo del mese le mostra nella riga "in attesa di quadratura".

Un periodo è "in corso" se il suo ultimo giorno non è ancora passato: oggi compreso. Nel conteggio giornaliero è provvisorio solo il giorno di oggi.

Quando il periodo è finito, il ricalcolo qualifica le ore e scrive le causali. Il calcolo va rilanciato dopo la fine del periodo: per esempio "Calcola tutto il mese" a inizio mese successivo.

**Settimana a cavallo di due mesi.** Con il conteggio settimanale la settimana si qualifica tutta insieme. Le ore restano sui giorni in cui sono state fatte, quindi anche nel mese di quei giorni. Per questo il mese si considera concluso solo dopo la domenica della sua ultima settimana.

### Cosa si vede

- **Calendario HR**, colonna "In più":
  - `+1:30` in verde con la qualifica (es. "Strao 1:00 · Strao nott. 0:30");
  - in grigio corsivo "in attesa di quadratura" se provvisorio;
  - in rosso le ore mancanti.
- **Calendario del dipendente**: in ogni giorno le ore lavorate e le ore in più, con la stessa logica.
- **Riepilogo del mese** (in cima ai due calendari):
  - ore teoriche e lavorate;
  - ordinarie e assenze;
  - ore in più (provvisorie e compensate);
  - supplementare e straordinario nelle loro varianti;
  - ore eccedenti, non riconosciute e saldo.
  - Cliccando una voce si vedono i giorni che l'hanno generata.

---

## 5. Cartellino mensile: stati e validazione

Ogni rapporto di lavoro ha un cartellino per mese (tabella `Dip_Periodo`) con i totali del mese, aggiornato a ogni ricalcolo.

| Stato | Quando | Cosa si può fare |
|---|---|---|
| **Provvisorio** | il mese (o la sua ultima settimana, con il conteggio settimanale) è in corso, oppure ci sono ore ancora in attesa di quadratura | si lavora normalmente |
| **Chiuso, in attesa di approvazione** | il periodo è finito ed è stato ricalcolato | l'HR verifica, eventualmente limita le ore riconosciute, approva |
| **Approvato** | dopo l'approvazione HR | giorni bloccati: niente nuovi movimenti, il ricalcolo li salta; banca ore accreditata |

Azioni HR, dal riepilogo nel Calendario HR o dalla pagina "Quadratura mensile":

- **Ore in più riconosciute.** Tetto mensile in h:mm:
  - vuoto = tutte;
  - `0` = rifiuta tutte (pulsanti "Riconosci tutte" e "Rifiuta tutte").
  - Il salvataggio rilancia il calcolo del mese: le ore oltre il tetto si tolgono dagli ultimi giorni e diventano "non riconosciute".
- **Approva.** Solo per i cartellini chiusi. I giorni del mese vengono bloccati (stato `Locked`) e ogni inserimento o modifica di timbrature, giustificativi, causali o richieste in quel mese viene rifiutato.
- **Riapri.** Il mese torna modificabile e ricalcolabile.

Nella pagina **Quadratura mensile** ci sono i dipendenti del reparto con i totali del mese. Si possono approvare più cartellini insieme e aprire il Calendario HR del singolo.

**Banca ore.** Per i dipendenti con destinazione banca ore, le ore qualificate di un mese approvato diventano maturato del giustificativo "Recupero banca ore" nei contatori.

---

## 6. Straordinario solo se autorizzato

Con il parametro del profilo **"Ore in più solo se autorizzate"** le ore in più si riconoscono solo fino alle ore autorizzate del giorno.

**Come si autorizza**

L'autorizzazione è un giustificativo con l'opzione **"Autorizza ore in più"**: il seed crea **AUTS - Autorizzazione straordinario**.
- Segno neutro e nessuna causale: non cambia le ore dovute e non scrive causali.
- Il dipendente lo chiede dalla pagina di richiesta giustificativo, indicando le ore. Non c'è "giornata intera", e il limite è 16 ore al giorno.
- La richiesta vale anche per **sabato, domenica e festivi**, perché lì lo straordinario è tutto fuori orario. Gli altri giustificativi invece saltano i giorni con 0 ore teoriche.
- Segue il flusso di approvazione delle richieste (approvatori del reparto, stati, revoca). L'HR può anche inserirlo direttamente dal Calendario HR; in quel caso è già approvato.
- Il calcolo con l'opzione "Approva richieste giustificativo" **non approva mai** in automatico le autorizzazioni.

**Effetto nel calcolo** (fase giornaliera, dopo arrotondamento e franchigia)

```
Ore in più riconosciute = min(ore in più del giorno, ore autorizzate approvate)
Non autorizzate         = ore in più - riconosciute     (nessuna causale, non entrano nella soglia)
di cui da autorizzare   = parte coperta da richieste di autorizzazione ancora in attesa
```

Le ore riconosciute seguono poi la qualifica normale: periodo, soglia, notturno e festivo, supplementare, banca ore, tetto HR.

- Finché nel mese ci sono autorizzazioni in attesa, il cartellino resta **provvisorio**.
- Nella colonna "In più" e nel riepilogo compaiono le voci "non autorizzate" e "in attesa di autorizzazione", con il dettaglio dei giorni.

**Esempio**

Orario 9-13 / 14-18, il dipendente lavora fino alle 20 (2 ore in più).

| Situazione | Risultato |
|---|---|
| nessuna autorizzazione | 2h non autorizzate, causali solo ORD1 |
| autorizzate 1h (approvate) | 1h straordinario (STRAO), 1h non autorizzata |
| richiesta di 2h in attesa | 2h non autorizzate, di cui 2h da autorizzare; mese provvisorio |
| sabato, 4h lavorate, autorizzate 3h | 3h straordinario, 1h non autorizzata |

---

## 7. Esempi (verificati con prove automatiche)

Full-time, orario 9-13 / 14-18 (8h), sabato e domenica di riposo.

**Conteggio giornaliero**

| Giorno | Timbrature | Risultato |
|---|---|---|
| lun | 9-13, 14-19 | +1:00 straordinario |
| mar | 9-13, 14-18, 21-23 | +2:00: 1:00 straordinario, 1:00 straordinario notturno (22-23) |
| mer | 9-13, 14-17 | -1:00 ore mancanti (non compensa nulla) |
| sab | 9-13 | +4:00 straordinario (teorico 0) |
| dom | 9-11 | +2:00 straordinario festivo |

**Conteggio settimanale, stessa settimana senza sabato e domenica**

Teorico 40h. Lavorate: lunedì +1, martedì +2, mercoledì -1, giovedì e venerdì 8h.

- Ore in più della settimana = 3 - 1 = 2.
- Le 2 ore qualificate vanno sull'ultimo giorno con ore in più (martedì): 1:00 diurno e 1:00 notturno.
- Il lunedì risulta compensato.

**Ferie di mezza giornata (full-time, giornaliero)**

Teorico 8, ferie 4, lavorate 6. Ore in più 2:

- senza "ferie in soglia": 6 < 8, quindi 2 ore eccedenti non straordinarie;
- con "ferie in soglia": 6 + 4 = 10 > 8, quindi 2 ore di straordinario.

**Part-time 20h settimanali (4h al giorno), settimanale**

Lavorate 44,5 ore:

- ore in più 24,5;
- straordinario 4,5 (oltre le 40);
- supplementare 20, di cui 5 entro il limite del 25% di 20h e 15 oltre il limite.

**Part-time verticale, giorno di riposo lavorato 5h (giornaliero)**

5 ore di supplementare: la soglia è quella full-time di 8h.

**Tetto HR**

Il mese ha 3 ore qualificate (lunedì 1h, martedì 2h). Con riconosciute 1:00, le 2 ore di martedì diventano non riconosciute.

---

## 8. Limiti noti e punti da decidere

- **Autorizzazione.** Si autorizzano ore per giorno, non una fascia oraria. Se si autorizza meno di quanto lavorato, le ore riconosciute sono le prime della qualifica: la parte notturna conta per prima. Il tipo di richiesta `ApprovazioneStraordinario` resta inutilizzato.
- **Banca ore.** Con la banca ore si accredita l'intera ora; non c'è la divisione "base in banca ore, maggiorazione in busta paga".
- **Lavoro ordinario.** Il lavoro ordinario notturno o festivo (dentro l'orario) non ha una sua maggiorazione: si qualificano solo le ore in più.
- **Timbrature anomale.** Le timbrature oltre le coppie dell'orario si accoppiano in ordine cronologico. Una timbratura doppia dentro una coppia dell'orario può falsare l'abbinamento.
- **Cambio di profilo.** Se il profilo cambia a metà periodo, la qualifica del periodo usa i parametri del profilo in vigore nel giorno che ha fatto partire il periodo.
- **Ruoli.** Le API del cartellino (approva, riapri, ore riconosciute), come le altre API delle presenze, non controllano il ruolo lato server (vedi `piano-security.md`).
- **Formato LUL.** Non c'è ancora il formato LUL: l'export resta quello delle causali.
