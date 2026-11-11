# Presenze: gestione delle pause (pranzo e pause brevi)

Analisi di come il programma tratta oggi le pause, quali casi d'uso servono e come estendere modello dati, motore di calcolo e interfaccia per gestire:

- la **pausa pranzo timbrata**, con controllo di durata minima, durata massima e finestra;
- la **pausa pranzo automatica**, cioè la detrazione della pausa quando il dipendente non la timbra;
- le **pause brevi** (caffè, uscite intermedie) timbrate dentro una coppia dell'orario, con eventuale quota retribuita.

Il documento segue lo stesso schema di `GestionePresenze_Straordinario.md` (`nvxapp.server/Note/`). Prima versione: commit `958040c2` (solo pausa pranzo, in `Note/GestionePresenze_PausaPranzo.md`); questa revisione la sposta tra le analisi `ANALISI_ATTENDANCETRACKING_*`, aggiorna i riferimenti al codice dopo l'introduzione delle formule personalizzate e aggiunge le pause brevi.

**Stato: implementata** nel branch `AttendanceTracking_Dev_pause` (vedi §12). Le scelte fatte sulle domande aperte sono in §11.

Riferimenti nel codice (branch `AttendanceTracking_Dev`, revisione `b964eaf6`):

- `nvxapp.server.service/ClientServer-Service/GestionePresenze/TimeSheet_EngineService/TimeSheet_EngineService.cs`: il motore. Metodi coinvolti: `AggiornaCartellinoMese` (1044), `CalcolaGiorno` (1094), `CalcolaOreTeoriche` (1425), `CalcolaOreLavorate` (1475), `AssegnaVersoTimbrature` (1532), `AccoppiaTimbratureFuoriOrario` (1706), `GeneraTimbratureMancanti` (1720), `GeneraCausali` (2090), `Sovrapposizione` (2423), `OreNotturne` (2450), `Get_Timbrature_Giorno` (2467). Aggancio delle formule: fase Giorno in `CalcolaGiorno` (1139-1146), fase Finale nella fase di periodo (997-1010), dati per le formule in `DatiFormule` (1278).
- `nvxapp.server.service/ClientServer-Service/GestionePresenze/_utility/GestionePresenzeOreTeoriche.cs:86-99`: un secondo calcolo delle ore teoriche, usato da contatori e piano ferie.
- `nvxapp.server.service/ClientServer-Service/GestionePresenze/_utility/GestionePresenzeParametriDefault.cs:120-165`: l'orario di default `DIURNO8H` e il profilo `DAY7H8`.
- `nvxapp.server.service/ClientServer-Service/GestionePresenze/FormuleEngine/`: formule personalizzate (`FormuleRunner.cs:51-52` usa `Warning_2` ed `Err_4`; `FormulaContext.cs` espone `x.Result`, `x.Orario`, `x.Timbrate`).
- `nvxapp.server.data/Entities/Tenant/GestionePresenze/`: le entità `Par_Orario`, `Par_OrarioIntervalloHH`, `Dip_GG_Timbratura` (`TipoTimbratura`: `Entrata`, `Uscita`, `SenzaVerso`), `Dip_GG_Result` (`GG_ResultStato`) e `Dip_Periodo`.
- `nvxapp.server.data/Infrastructure/AttendanceTrackingDbContext.cs`: la configurazione EF (`Par_Orario` riga 557, `Par_OrarioIntervalloHH` riga 644).
- Client: `ClientServer-Service/GestionePresenze/Dip_GG_Result/dip-ggresult.service.ts:56-58` (maschera degli avvisi, oggi solo `Warning_1 | Warning_2`) e `Utility/GestionePresenze/time-sheet.service.ts:1030-1033` (testi degli avvisi).

---

## 1. Situazione attuale

### 1.1 La pausa esiste solo come "buco" tra due coppie

Nel modulo presenze non c'è nessun concetto esplicito di pausa: niente entità, campi, enum, parametri, servizi, testi i18n o elementi di interfaccia per pausa, pranzo, mensa o buono pasto (verificato su tutto il codice server e client).

La pausa pranzo è rappresentata **implicitamente** come l'intervallo tra due coppie dell'orario (`Par_OrarioIntervalloHH`). L'orario di default `DIURNO8H` ha le coppie 09:00-13:00 e 14:00-18:00, con tolleranza ±15' e arrotondamento a 15'. Il buco 13:00-14:00 è di fatto la pausa pranzo:

| Calcolo | Come lo fa oggi | Effetto sulla pausa |
|---|---|---|
| Ore teoriche (`CalcolaOreTeoriche`) | Somma delle durate delle coppie | Il buco non è mai teorico, quindi 8h |
| Ore lavorate (`CalcolaOreLavorate`, `GeneraCausali`) | Timbrature con verso ordinate per ora arrotondata, prese **a due a due per posizione** (0-1, 2-3, ...); una coppia che non è E poi U viene scartata | Il tempo tra l'uscita delle 13 e l'entrata delle 14 non conta |
| Accoppiamento (`AssegnaVersoTimbrature`) | Una timbratura attesa per ogni `Dalle`/`Alle` con `*_Use_4_Match = true`, cercata nella sua tolleranza; il verso dipende dalla posizione della timbratura attesa (pari = E, dispari = U). Le timbrature rimaste libere vanno ad `AccoppiaTimbratureFuoriOrario`, che le accoppia E/U in ordine cronologico come **lavoro fuori orario** | Servono **4 timbrature** |

### 1.2 Cosa succede nei casi reali

| Caso | Timbrature | Esito oggi |
|---|---|---|
| Pausa timbrata regolare | 08:58, 13:02, 13:57, 18:03 | Corretto: 8h lavorate |
| Pausa breve timbrata (20 min) | 09:00, 13:00, 13:20, 18:00 | 8h40 lavorate. I 40 minuti in più diventano eccedenza o straordinario. Nessun controllo di pausa minima |
| Pausa lunga timbrata (90 min) | 09:00, 13:00, 14:30, 18:00 | 7h30 lavorate, 30 minuti mancanti (`Err_2`). Corretto |
| **Pausa non timbrata** | 09:00, 18:00 | Le timbrature sono meno di quelle attese, quindi `Err_TimbratureMancanti`. Con l'opzione "Genera timbrature mancanti" il motore **inventa** le timbrature 13:00 e 14:00 |
| **Pausa caffè timbrata dentro la mattina** | 09:00, 10:30, 10:45, 13:00, 14:00, 18:00 | **Errato: 4h lavorate.** Le 4 timbrature attese si abbinano a 09, 13, 14, 18; 10:30 e 10:45 restano libere e `AccoppiaTimbratureFuoriOrario` le marca E/U. In ordine la sequenza è E, E, U, U, E, U: le prime due coppie per posizione (E-E, U-U) vengono scartate e resta solo 14-18. Ci si aspetterebbe 7h45 (o 8h con 15' retribuiti) |
| Orario continuato a coppia unica (08:30-17:30) | 08:30, 17:30 | 9h lavorate su 9h teoriche. Nessuna pausa e nessuna detrazione |
| Orari a monte ore (`MonteOre`, `MonteOreValore`) | valori orari | La pausa non è applicabile |

### 1.3 Limiti

1. Non c'è **detrazione automatica**: chi non timbra la pausa non ottiene un risultato corretto senza correzioni manuali o timbrature generate.
2. Non c'è il controllo di **pausa minima**. Il D.Lgs. 66/2003, art. 8, prevede una pausa quando l'orario giornaliero supera le 6 ore. Durata e modalità sono fissate dal CCNL, con un minimo di 10 minuti in mancanza di regole contrattuali. Oggi una pausa insufficiente non viene segnalata e il tempo in più diventa eccedenza.
3. Non c'è il concetto di **finestra della pausa** (per esempio tra le 12:00 e le 14:30) né di pausa flessibile.
4. Non c'è la **pausa retribuita** (per esempio mensa interna retribuita o 10 minuti di pausa contrattuale pagati).
5. In `Dip_GG_Result` e `Dip_Periodo` non c'è nessun dato sulla pausa. L'HR non vede né la pausa fatta né quella detratta.
6. Le ore teoriche si calcolano in **due punti** (motore e `GestionePresenzeOreTeoriche`) con la stessa regola duplicata.
7. **Difetto**: le uscite intermedie dentro una coppia (pausa caffè, uscita breve) rompono l'accoppiamento per posizione e fanno perdere intere ore lavorate (§1.2). È un errore di calcolo che esiste già oggi, indipendente dalla nuova funzione.
8. Le formule personalizzate non hanno dati sulla pausa: chi oggi volesse simularla dovrebbe ricostruirla da `x.Timbrate`, e comunque una formula modifica i minuti delle causali ma non può spostare i segmenti lavorati (notturno, fasce).

---

## 2. Casi d'uso da coprire

| # | Caso | Esempio | Regola richiesta |
|---|---|---|---|
| U1 | Pausa timbrata con durata minima | Orario 09-13 / 14-18, pausa minima 60' | Se la pausa timbrata è di 30', detrarre 30' ("integrazione pausa") oppure solo segnalare |
| U2 | **Pausa automatica su orario a due coppie** | Il dipendente timbra solo 09:00 e 18:00 | Le 2 timbrature sono accettate senza errore e si detrae 1h |
| U3 | **Pausa automatica su orario continuato** | Coppia unica 08:30-17:30, pausa 60' | Teorico 8h. Lavorato = presenza meno 60' se si superano le 6h |
| U4 | Pausa flessibile in finestra | Pausa tra 12:00 e 14:30, durata 30'-90' | Conta come pausa solo il buco che cade nella finestra |
| U5 | Pausa automatica solo oltre soglia | Mezza giornata 08:00-13:00 (permesso pomeridiano) | Nessuna detrazione se il lavorato non supera la soglia (es. 6h) |
| U6 | Pausa retribuita, in tutto o in parte | Mensa con 30' pagati su 60' | Si detraggono solo i minuti non retribuiti |
| U7 | Turni notturni | Turno 22:00-06:00 con pausa 02:00-02:30 | Finestra a cavallo della mezzanotte, coerente con `GiornoCompetenza` |
| U8 | (fase 2) Buono pasto | Buono pasto se lavorato ≥ 6h con pausa | Genera una causale o un giustificativo a importo |
| **U9** | **Pausa breve timbrata** | 09:00, 10:30, 10:45, 13:00, 14:00, 18:00 | Le due timbrature intermedie sono un'uscita e un rientro: 15' non lavorati (oggi il giorno vale 4h, §1.2) |
| **U10** | **Pausa breve retribuita** | CCNL con 10' di pausa pagata al giorno | Le uscite brevi fino a 10' complessivi restano lavorate; oltre, si detraggono |

### 2.1 Il dipendente deve timbrare la pausa?

Dipende da come l'azienda configura l'orario: è la **modalità** della pausa pranzo a deciderlo.

| | **Rilevata** (la pausa va timbrata) | **Automatica** (timbratura facoltativa) |
|---|---|---|
| Per chi è | Aziende che vogliono sapere quando il dipendente esce e rientra | Aziende dove il dipendente timbra solo entrata e uscita |
| 4 timbrature (09:00, 13:00, 14:00, 18:00) | 8h, pausa rilevata 60' | 8h, pausa rilevata 60', nessuna detrazione |
| 2 timbrature (09:00, 18:00) | **Errore "timbrature mancanti"**, come oggi | **8h**: il calcolo toglie 60' |
| Pausa timbrata di 30' (minimo 60') | 8h30 con **avviso "pausa insufficiente"**: decide l'HR | 8h: il calcolo **toglie i 30' mancanti** |
| Pausa timbrata di 90' | 7h30 (30' mancanti) e avviso se oltre il massimo | uguale |

- **Rilevata**: il dipendente **deve** timbrare la pausa; il calcolo controlla e segnala, non corregge.
- **Automatica**: il dipendente **può** timbrarla. Se non la timbra, o la fa troppo corta, il calcolo completa la pausa fino al minimo. Se la timbra regolarmente non toglie nulla in più. Per i giorni in cui ha lavorato durante la pausa l'HR inserisce il giustificativo **"Pausa non fruita"**.
- Le **pause brevi** (caffè, uscite) vanno sempre timbrate: senza timbratura il calcolo non può sapere che il dipendente è uscito. Si configura solo quanta parte resta pagata.
- **Orario continuato** (coppia unica): con Automatica la pausa si toglie se non è timbrata; con Rilevata, se manca, c'è solo l'avviso (l'orario non prevede timbrature a metà giornata).

Impostazione consigliata per i nuovi orari: **Automatica**, la più semplice per il dipendente e comunque rispettosa del minimo di legge; Rilevata dove l'azienda vuole controllare gli orari effettivi della pausa.

### 2.2 Come la gestiscono altri applicativi

Ricerca su prodotti internazionali (UKG/Kronos, Factorial, Personio, Deputy, Jibble, SAP) e su manuali e regolamenti italiani (RiPreSa, e-Time, regolamenti aziendali). Per Zucchetti e Inaz non è disponibile documentazione pubblica sulle regole di pausa.

| Modello | Dove | Come funziona |
|---|---|---|
| Detrazione automatica oltre soglia | UKG/Kronos, Jibble, Kenjo, Clockodo | Durata + soglia di ore lavorate; UKG "Cancel if break taken" non detrae se la pausa è timbrata; l'HR può annullare la detrazione sul singolo giorno |
| Integrazione fino al minimo | Personio, Factorial, Personizer | Se la pausa timbrata è più corta del minimo si detrae la differenza; in Germania minimi a scaglioni (30' tra 6 e 9h, 45' oltre 9h) |
| Pausa pianificata nell'orario/turno | SAP, Deputy, Factorial | SAP: schema pause per orario con quota retribuita e non retribuita, a orario fisso o dopo N ore; Deputy: pausa pianificata scritta nel foglio ore, giorno segnalato se non fatta, attestazione del dipendente |
| Tasto "Pausa" sul timbratore | Jibble, Deputy | Timbratura dedicata di inizio e fine pausa |
| Fascia mensa timbrata | RiPreSa, e-Time, regolamenti italiani | Fascia mensa con flessibilità, minimo con detrazione della differenza, soglia minima di presenza, massimo ed eccesso di pausa |

La soluzione adottata è allineata soprattutto al modello italiano e a "integrazione fino al minimo": una sola regola `dovuta - rilevata` copre pausa non timbrata e pausa troppo corta, con soglia, quota retribuita (come SAP), finestra dagli intervalli di riconoscimento (la "fascia mensa con flessibilità"), minimo e massimo con avviso. Scelta diversa: niente tasto "Pausa" sul timbratore, il verso si ricava dagli intervalli (§3.3).

Dagli altri prodotti si è ripreso subito l'**annullamento della detrazione sul giorno** (giustificativo "Pausa non fruita", come UKG). Restano per la fase 2: soglie a scaglioni, pausa "dopo N ore dall'entrata" (SAP, Deputy), conferma del dipendente (Deputy).

Fonti: [UKG – Bonus and Deductions](https://hlp02.gss.mykronos.com/help/HCM/oxy_ex-2/_shared/common_topics/bonus_and_deductions.html), [UKG – Meals and Breaks Rule](https://library.ukg.com/Prebuilt/newgen/WorkforceMgmt/Content/WFMMealsAndBreaksRule-c_x122502.html), [Factorial – automatic corrections](https://help.factorialhr.com/en_US/automatic-corrections-for-manual-breaks), [Personio](https://support.personio.de/hc/en-us/articles/115000671865), [Deputy – Break Planning](https://www.deputy.com/blog/break-planning-the-better-way-to-manage-breaks), [Jibble – automatic deductions](https://www.jibble.io/help/calculating-breaks-and-automatic-deductions), [SAP – Break Schedules](https://help.sap.com/doc/ca44e153a217424de10000000a174cb4/3.6/en-US/9b38e153038a424de10000000a174cb4.html), [RiPreSa – Pausa Mensa](https://personale.asptrapani.it/areas/help/ripresa/orari/parametriPausaMensa.htm).

---

## 3. Scelte di progetto

### 3.1 Dove configurare la pausa: nell'**orario** (proposta)

| Opzione | Pro | Contro |
|---|---|---|
| **A. Orario (`Par_Orario`)**, proposta | Un profilo contiene giorni con e senza pausa (sabato a mezza giornata, part-time 4h). La pausa dipende dalla forma della giornata | La configurazione va ripetuta su ogni orario (mitigato dai default di seed e dalla copia orario) |
| B. Profilo orario (`Par_ProfiloOrario`) | Stesso schema dei parametri dello straordinario | Non distingue i giorni del profilo. Bisognerebbe comunque escludere gli orari corti |
| C. Azienda (`Az_Cfg`) con override per orario | Un default unico per azienda | Due livelli da gestire. Il precedente `Par_Arrotondamenti` è rimasto non collegato al motore |
| D. Solo formule personalizzate | Nessuna modifica al modello | Solo piano Pro; la formula non sposta i segmenti (notturno, fasce); ogni azienda riscrive la stessa regola |

La proposta è la A, con una tabella figlia **`Par_OrarioPausa`** (da 0 a n righe per orario) sul modello di `Par_OrarioIntervalloHH`, invece di molte colonne su `Par_Orario`. La tabella figlia regge sia il pranzo sia le pause brevi (una riga ciascuno) e i turni lunghi con due pause (U7). La prima versione dell'interfaccia gestisce al massimo una riga per tipo.

### 3.2 Non generare timbrature fittizie

La pausa automatica **non crea record in `Dip_GG_Timbratura`**. È un calcolo che il motore ripete a ogni ricalcolo.

L'alternativa, cioè generare timbrature come fa `GeneraTimbratureMancanti`, è scartata per tre motivi:
- sporca il dato originale del dipendente;
- se i parametri della pausa cambiano, le timbrature generate restano;
- confonde l'HR, che non distingue più una timbratura reale da una fittizia.

### 3.3 Interpretare la sequenza con gli intervalli di riconoscimento (prerequisito)

Prima di qualsiasi logica di pausa va corretto il difetto della §1.2 (U9). Lo strumento c'è già: ogni coppia dell'orario ha gli **intervalli di riconoscimento** `Dalle_Limite_SX/DX` e `Alle_Limite_SX/DX` (seed `DIURNO8H`: ±15', `GestionePresenzeParametriDefault.cs:139-146`).

**Come li usa oggi il motore.** Solo per **agganciare** una timbratura all'ora attesa: nel primo passaggio la timbratura va alla prima ora attesa libera il cui intervallo la contiene (`AssegnaVersoTimbrature`, 1626-1628), nel secondo all'ora attesa più vicina, anche fuori intervallo (1658-1663). Lo stesso schema c'è in `GeneraTimbratureMancanti` (1784). Le timbrature che restano fuori diventano lavoro fuori orario **senza guardare dove cadono**.

**Proposta.** Usare gli intervalli per dividere la giornata in **zone** e interpretare l'intera sequenza:

```
 prima    [Dalle1]   LAVORO 1   [Alle1]   PAUSA 1-2   [Dalle2]   LAVORO 2   [Alle2]    dopo
 ------|  SX....DX |----------| SX....DX |-----------| SX....DX |----------| SX....DX |------
fuori      ancora     uscite      ancora     pausa       ancora     uscite      ancora    fuori
orario     E          intermedie  U          pranzo      E          intermedie  U         orario
```

Algoritmo in `AssegnaVersoTimbrature`, in quest'ordine:

1. **Pulizia delle doppie timbrature.** Due timbrature a distanza inferiore a una soglia (es. 2') nello stesso intervallo valgono come una: la seconda viene ignorata nel calcolo, non cancellata, e il giorno riceve un avviso.
2. **Abbinamento ordinato alle ancore.** Le ore attese (`Dalle`/`Alle` con `*_Use_4_Match`) sono le ancore, in ordine cronologico. L'abbinamento rispetta l'ordine: se una timbratura va a un'ancora, quelle successive possono andare solo alle ancore successive. Si massimizza il numero di ancore abbinate dentro il loro intervallo e, a parità, si minimizza la distanza dall'ora attesa. Con poche timbrature al giorno basta una programmazione dinamica O(timbrature × ancore). Così funzionano anche gli intervalli larghi e sovrapposti di un pranzo flessibile (es. `Alle1` 12:00-14:30 e `Dalle2` 12:30-15:00), che con il "primo intervallo libero" di oggi sono ambigui.
3. **Classificazione per zona delle timbrature non abbinate:**
   - zona **lavoro** k (tra l'ancora E e l'ancora U della coppia k) → **uscita intermedia**: a coppie, la prima `Uscita` e la seconda `Entrata`, senza arrotondamento. Sono le pause brevi (U9, U10);
   - zona **pausa** tra la coppia k e la k+1 → se `Alle(k)` o `Dalle(k+1)` non hanno trovato una timbratura nel loro intervallo, la timbratura ne prende il posto **fuori intervallo**, con un avviso (uscita anticipata o rientro in ritardo), invece del "più vicino" alla cieca di oggi. Altrimenti è un'ulteriore uscita e rientro durante la pausa, senza effetto sul lavorato;
   - zona **prima** o **dopo** l'orario → lavoro fuori orario, come oggi (`AccoppiaTimbratureFuoriOrario`).
4. **Ancore mancanti.** Se `Alle(k)` e `Dalle(k+1)` restano entrambe vuote e l'orario ha una pausa `Automatica`, sono facoltative (§6.2). In ogni altro caso: `Err_TimbratureMancanti`, come oggi.
5. **Controllo finale.** La sequenza cronologica dei versi deve alternare E/U. Se non è così (uscita senza rientro dentro una fascia), il giorno va in anomalia invece di perdere ore in silenzio.

| Timbrature (`DIURNO8H`, ±15') | Oggi | Con l'interpretazione per zone |
|---|---|---|
| 09:00, 10:30, 10:45, 13:00, 14:00, 18:00 | 4h (§1.2) | 10:30/10:45 in zona lavoro 1 → U/E: **7h45**, pausa breve 15' |
| 08:58, 08:59, 13:02, 13:57, 18:03 | 08:59 diventa fuori orario e va in coppia con un'altra timbratura libera, se c'è | 08:59 è doppia e viene ignorata: 8h, avviso |
| 09:00, 11:30, 14:00, 18:00 (uscita anticipata senza giustificativo) | 11:30 agganciata a `Alle1` come "più vicina" | 11:30 in zona lavoro 1 con `Alle1` vuota → `Alle1` fuori intervallo: 6h30, avviso esplicito |
| 09:00, 18:00 con pausa automatica | `Err_TimbratureMancanti` | `Alle1`/`Dalle2` vuote e facoltative → detrazione automatica (§3.4) |
| 09:00, 13:00, 14:00, 18:00, 20:00, 22:00 | 8h + 2h fuori orario | invariato (zona dopo) |
| 09:00, 10:30, 13:00, 14:00, 18:00 | risultato incoerente | anomalia esplicita (dispari in zona lavoro 1) |

Con questa regola i cicli di `CalcolaOreLavorate` e `GeneraCausali`, che leggono le coppie per posizione, ricevono una sequenza E/U già corretta e non vanno toccati. La correzione è **indipendente** dalla nuova funzione e può essere rilasciata da sola.

**Le zone danno anche la finestra della pausa.** Se `Par_OrarioPausa` non indica `FinestraDalle`/`FinestraAlle`, la finestra predefinita del pranzo tra le coppie k e k+1 va da `Alle_Limite_SX(k)` a `Dalle_Limite_DX(k+1)` (in `DIURNO8H` 12:45-14:15). Per una pausa flessibile basta allargare gli intervalli di riconoscimento, senza un secondo parametro da tenere allineato.

**Validazione degli intervalli (oggi assente).** Né il servizio `Par_OrarioIntervalloHHService` né il dialog `edit-par-orario-dettaglio-orario-intervallo-hhdialog` controllano la coerenza dei limiti (il client li rende solo obbligatori e li sposta insieme all'ora). Per l'interpretazione per zone servono:
- `Limite_SX <= ora <= Limite_DX` per ogni ancora;
- intervalli **in ordine**: l'inizio di ogni intervallo non precede l'inizio del precedente. La sovrapposizione è ammessa, perché la gestisce l'abbinamento ordinato;
- una zona lavoro non vuota: `Dalle_Limite_DX(k) < Alle_Limite_SX(k)`.

### 3.4 Una sola formula per pausa timbrata e automatica

Per ogni giorno e per ogni pausa configurata:

```
PausaRilevata  = Pranzo: buco tra l'ancora Alle(k) e l'ancora Dalle(k+1) (zona pausa, §3.3), ristretto alla finestra
                         (finestra predefinita: Alle_Limite_SX(k) .. Dalle_Limite_DX(k+1));
                         su orario a coppia unica: le uscite intermedie che cadono nella finestra
                 Breve:  somma delle uscite intermedie in zona lavoro non attribuite al pranzo
LavoratoLordo  = somma delle coppie E/U (come oggi, dopo la correzione §3.3)
PausaDovuta    = LavoratoLordo > SogliaMin ? DurataMin : 0
Detrazione     = Modalita == Automatica
                   ? max(0, PausaDovuta - PausaRilevata - RetribuitaMin)
                   : 0
Riaccredito    = min(PausaRilevata, RetribuitaMin)      // minuti di pausa timbrata pagati (U6, U10)
Lavorato       = LavoratoLordo - Detrazione + Riaccredito
```

Con questa formula i casi della §2 si risolvono tutti nello stesso modo:

| Caso | Rilevata | Dovuta | Detrazione | Riaccredito | Lavorato |
|---|---|---|---|---|---|
| 09:00-18:00, nessuna pausa timbrata (U2) | 0' | 60' | 60' | 0' | 8h |
| 09:00-13:00 / 13:30-18:00 (U1) | 30' | 60' | 30' | 0' | 8h |
| 09:00-13:00 / 14:30-18:00 | 90' | 60' | 0' | 0' | 7h30 (30' mancanti, come oggi) |
| 08:00-13:00, permesso pomeridiano (U5) | 0' | 0' (5h < 6h) | 0' | 0' | 5h |
| Continuato 08:30-17:30 con 30' retribuiti (U6) | 0' | 60' | 30' | 0' | 8h30 (teorico 8h30, §6.1) |
| Pausa caffè 10:30-10:45, pausa breve con 10' retribuiti (U9, U10) | 15' | — | 0' | 10' | 7h55 |

Con la modalità **Rilevata** la detrazione è sempre 0. Se però la pausa dovuta supera quella rilevata, il giorno riceve l'avviso di pausa insufficiente. Il riaccredito si applica in entrambe le modalità (da confermare, domanda aperta 4).

### 3.5 Dove collocare i minuti detratti

Le ore lavorate vengono attribuite alle causali per sovrapposizione con le coppie (`GeneraCausali` → `Sovrapposizione`), e le ore notturne per fascia (`OreNotturne`). Per questo la detrazione deve cadere in un punto preciso della giornata.

**Regola proposta:** la pausa virtuale inizia a `FinestraDalle`. Senza finestra inizia alla `Alle` della coppia che precede il buco. Su un orario a coppia unica, senza finestra, cade a metà della presenza. I minuti `[inizio, inizio + Detrazione]` vengono tolti dal segmento E/U che li contiene. Se il segmento è più corto, la parte restante passa al segmento successivo. Il riaccredito si aggiunge invece al segmento che precede l'uscita.

### 3.6 Stati del giorno

Gli slot di avviso proposti nella prima versione vanno rivisti, perché nel frattempo le formule hanno occupato `Warning_2`:

| Slot | Uso attuale | Uso proposto |
|---|---|---|
| `Warning_1` | ore in più (motore, riga 2334) | — |
| `Warning_2` | avviso delle formule (`FormuleRunner.StatoWarningFormula`) | — |
| `Warning_3` | libero | **pausa insufficiente o non fruita** |
| `Warning_4` | libero | **pausa oltre la durata massima** |
| `Warning_5` | libero | **timbratura doppia ignorata o ancora fuori intervallo** (§3.3) |
| `Err_4` | anomalia delle formule | — |
| `Err_5` | libero (definito, mai impostato dal motore) | **sequenza non interpretabile**: uscita senza rientro in zona lavoro (§3.3, passo 5) |

`Warning_5` (`1 << 29`) sta ancora nei 31 bit gestiti dal client.

`Warning_3` (`1 << 27`) e `Warning_4` (`1 << 28`) stanno ancora nei 31 bit gestiti dagli operatori bitwise di TypeScript, quindi sul client basta aggiungerli alla maschera `ALL_WARNING_DETAILS` senza passare a `BigInt`.

---

## 4. Modello dati

### 4.1 Nuova entità `Par_OrarioPausa` (schema tenant presenze)

| Campo | Tipo | Note |
|---|---|---|
| `Id`, campi `BaseEntity` | | |
| `IdPar_Orario` | int, FK | cascade come `Par_OrarioIntervalloHH` |
| `NumPausa` | int | ordine (1 = pranzo) |
| `Tipo` | `PausaTipo` | `Pranzo` = buco tra/entro le coppie nella finestra; `Breve` = uscite intermedie dentro le coppie |
| `Descrizione` | string(50) | "Pausa pranzo", "Pausa caffè" |
| `Modalita` | `PausaModalita` | `Rilevata` = solo controllo e avviso; `Automatica` = detrae la differenza (solo `Pranzo`) |
| `DurataMin` | int | durata minima o standard in minuti (es. 60); per `Breve` può essere 0 |
| `DurataMaxMin` | int? | oltre questa durata: avviso `Warning_4` (informativo; i minuti in più sono già ore mancanti) |
| `SogliaMin` | int | la pausa è dovuta se il lavorato lordo supera questi minuti (default 360 = 6h) |
| `FinestraDalle` | TimeOnly? | inizio della finestra in cui la pausa è riconosciuta e collocata. Vuota: `Alle_Limite_SX` della coppia che precede la pausa (§3.3) |
| `FinestraAlle` | TimeOnly? | fine della finestra. Vuota: `Dalle_Limite_DX` della coppia che segue. Se `FinestraAlle < FinestraDalle`, la finestra finisce il giorno dopo (turni notturni). Obbligatoria su orario a coppia unica |
| `RetribuitaMin` | int | minuti della pausa che restano pagati (default 0) |
| `IdCausale_Pausa` | int? | facoltativa: causale informativa per i minuti detratti, se servono nell'export paghe |

Enum `PausaTipo { Pranzo = 0, Breve = 1 }` e `PausaModalita { Rilevata = 0, Automatica = 1 }`. Se un orario non ha righe di pausa, il comportamento resta **esattamente quello di oggi** (a parte la correzione §3.3).

Vincoli di validazione nel servizio (messaggi localizzati in `AttendanceTrackingMessages`):
- `DurataMin >= 0`, e `> 0` per `Tipo = Pranzo`;
- `RetribuitaMin <= DurataMin` per `Pranzo`;
- `DurataMaxMin`, se valorizzata, `>= DurataMin`;
- `Modalita = Automatica` ammessa solo per `Tipo = Pranzo`;
- la pausa è ammessa solo se `Par_Orario.TimbratureTipo == IntervalloOrario`;
- al massimo una riga `Pranzo` per orario nella prima versione.

### 4.2 Risultati

| Entità | Nuovi campi | Uso |
|---|---|---|
| `Dip_GG_Result` | `int Min_PausaRilevata`, `int Min_PausaDetratta`, `int Min_PausaRetribuita` | Dettaglio del giorno nel cartellino |
| `Dip_GG_Result.Stato` | `Warning_3`, `Warning_4` (§3.6) | Avvisi |
| `Dip_Periodo` | `int Min_PausaDetratta` | Totale mensile in `AggiornaCartellinoMese` |

### 4.3 Migrazione e seed

- Nuova migrazione in `nvxapp.server.data/Migrations/AttendanceTracking/`, con una tabella e sei colonne.
- Accesso solo tramite repository (`Par_OrarioPausaRepository` in `Repositories/Tenant/GestionePresenze/`), come da `nvxapp.server.service/CLAUDE.md`.
- `GestionePresenzeParametriDefault`:
  - `DIURNO8H` riceve una pausa `Pranzo`/`Rilevata` di 60', finestra vuota (quindi dagli intervalli: 12:45-14:15) e soglia 6h. Il calcolo non cambia, si aggiungono solo gli avvisi.
  - Nuovo orario `CONT8H` (il codice orario ha al massimo 10 caratteri): coppia unica 08:30-17:30 con pausa `Pranzo`/`Automatica` di 60', finestra 12:30-14:30, soglia 6h. È il caso tipico della pausa automatica.
  - Nuovo giustificativo `PNF` "Pausa non fruita" (`AnnullaPausa`, segno neutro, senza causale né contatore).
- I dati esistenti senza righe di pausa mantengono il comportamento attuale.

---

## 5. Modifiche al motore: correzione preliminare (U9)

In `AssegnaVersoTimbrature` (1532), `AccoppiaTimbratureFuoriOrario` (1706) e nell'abbinamento di `GeneraTimbratureMancanti` (1784): applicare l'interpretazione per zone della §3.3. La logica va in una funzione pura `GestionePresenzeSequenza.Interpreta(ancore, timbrature) → (versi, arrotondamenti, avvisi)`, usata da entrambi i metodi e coperta dai casi della tabella §3.3.

- 09:00, 10:30, 10:45, 13:00, 14:00, 18:00 su `DIURNO8H` → 7h45, causali della mattina 3h45;
- 09:00, 13:00, 14:00, 18:00, 20:00, 22:00 (rientro serale) → invariato: 8h + 2h fuori orario;
- 09:00, 10:30, 13:00, 14:00, 18:00 (uscita senza rientro, dispari) → anomalia esplicita invece del risultato attuale;
- doppia timbratura e ancora fuori intervallo → avviso (slot da assegnare, §3.6).

Nuovi controlli di coerenza degli intervalli in `Par_OrarioIntervalloHHService` (messaggi it/en) e nel dialog `edit-par-orario-dettaglio-orario-intervallo-hhdialog` (§3.3, ultimo paragrafo).

---

## 6. Modifiche al motore: pause

Tutte in `TimeSheet_EngineService.cs` salvo dove indicato.

### 6.1 Ore teoriche (U3)

`CalcolaOreTeoriche` e `GestionePresenzeOreTeoriche` sottraggono `DurataMin - RetribuitaMin` per ogni pausa `Pranzo` **che cade dentro una coppia**, nella misura della sovrapposizione tra la finestra (o la pausa collocata) e la coppia:

- coppia unica 08:30-17:30 con pausa di 60' → teorico 8h;
- orario a due coppie con la pausa nel buco → la sovrapposizione è 0, quindi il teorico resta 8h come oggi.

Le pause `Breve` non toccano il teorico. Per non duplicare ancora la regola, il calcolo va spostato in un unico helper statico in `GestionePresenzeOreTeoriche`, chiamato anche dal motore.

### 6.2 Accoppiamento: timbrature interne facoltative (U2)

In `AssegnaVersoTimbrature`, se l'orario ha una pausa `Automatica` e il buco tra la coppia k e la coppia k+1 cade nella finestra, le timbrature attese `Alle(k)` e `Dalle(k+1)` diventano **facoltative**:

- non concorrono al conteggio che produce `Err_TimbratureMancanti` (riga 1595);
- se ci sono solo 2 timbrature, la prima si abbina a `Dalle(1)` e l'ultima a `Alle(n)`, con le loro tolleranze e i loro arrotondamenti;
- se le timbrature interne ci sono, si abbinano come oggi e la pausa diventa "rilevata".

### 6.3 Niente timbrature generate per la pausa

`GeneraTimbratureMancanti` (1720) non deve generare `Alle(k)` e `Dalle(k+1)` quando sono facoltative per la regola §6.2. Altrimenti il motore inventerebbe 13:00 e 14:00, la pausa risulterebbe "rilevata" e il dato verrebbe sporcato.

### 6.4 Nuovo passo `ApplicaPausa` e segmenti lavorati unici

Oggi `CalcolaOreLavorate` e `GeneraCausali` (ciclo sulle coppie E/U, riga 2213) scorrono le coppie **ognuno per conto proprio**. Va introdotto un metodo unico:

```csharp
List<(DateTime Dalle, DateTime Alle)> SegmentiLavorati(allData, idRapporto, giorno, out PausaEsito esito)
```

Il metodo:
1. costruisce i segmenti dalle coppie E/U arrotondate (la logica attuale, dopo la correzione §5);
2. per ogni `Par_OrarioPausa` dell'orario base calcola `PausaRilevata`, `PausaDovuta`, `Detrazione` e `Riaccredito` (§3.4);
3. applica detrazione e riaccredito nella posizione della §3.5;
4. restituisce i segmenti netti e l'esito (minuti rilevati, detratti, retribuiti, avvisi).

`CalcolaOreLavorate` diventa la somma dei segmenti. `GeneraCausali` usa gli stessi segmenti per sovrapposizione con le coppie, fascia notturna, ore ordinarie ed eccedenze. In questo modo **straordinario, notturno e supplementare restano coerenti** senza toccare `GestionePresenzeEccedenze`. In `CalcolaGiorno` l'esito viene scritto in `Dip_GG_Result.Min_Pausa*` e nei flag `Warning_3` e `Warning_4`, **prima** della fase Giorno delle formule (1139), così le formule vedono il dato già netto.

La logica di `ApplicaPausa` va scritta come funzione pura (input: segmenti e parametri; output: segmenti netti ed esito), come `GestionePresenzeEccedenze`, in `_utility/GestionePresenzePause.cs`. Così si può coprire con test unitari quando verrà creato il progetto xUnit previsto da `piano-security.md`.

### 6.5 Ricalcolo

Una modifica a orario o profilo **non** innesca il ricalcolo (commento in testa al motore). Va bene anche per la pausa: dopo una modifica dei parametri l'HR rilancia il calcolo del periodo, come per gli altri parametri. In alternativa, il salvataggio dell'orario potrebbe proporre il ricalcolo dei giorni non bloccati (`Locked`): non implementato.

---

## 7. Integrazione con le altre funzioni

| Funzione | Impatto | Proposta |
|---|---|---|
| **Formule personalizzate** (`FormuleEngine/`) | Le formule non conoscono la pausa | `FormulaDatiGiorno` riceve le pause dell'orario; `x.Result` espone `PausaRilevata`, `PausaDetratta`, `PausaRetribuita`; `x.Orario` espone le pause configurate. Aggiornare `FormuleCatalogo` (documentazione per l'AI e per l'editor) e l'analisi `ANALISI_ATTENDANCETRACKING_FORMULE.md` |
| **Personale presente / widget "presenti oggi"** (`PresentStaffService`, `Dip_WidgetService`) | Presente = ultima timbratura di oggi in entrata: chi è in pausa timbrata risulta assente | Fase 2: stato "In pausa" se l'ultima timbratura è un'uscita dentro la finestra di una pausa. Con la pausa automatica non cambia nulla |
| **Statistiche attività** (`ActivityStatisticsService`) | Calcola le ore per attività dalle timbrature: non vede la detrazione automatica | Domanda aperta 7: lasciare le ore di presenza o applicare la detrazione all'attività del segmento che la contiene |
| **Timbratura da chat AI / comandi vocali** (`RegisterClockingHandler`) | Registra timbrature `SenzaVerso`: compatibile | Nessuna modifica. Facoltativo: frasi "inizio pausa" / "fine pausa" come sinonimi di timbratura |
| **Export paghe / export AI** | I minuti detratti non hanno causale | Se `IdCausale_Pausa` è valorizzata, i minuti detratti finiscono in quella causale (informativa) e quindi nell'export; aggiornare i metadati di `TimeSheet_ExportService` |
| **Licenze / piani** | — | Domanda aperta 8: funzione nel piano base o nel Pro (le formule sono Pro) |

---

## 8. API e client

### 8.1 Server

- `Par_OrarioPausa`: entità e repository. Le pause viaggiano con l'orario, come le coppie: `Par_Orario_Get` le restituisce e `Par_Orario_Put` le valida e le salva (cancella quelle tolte dal client). Nessun controller separato.
- `Par_OrarioGet` e `Get_OrariSchema_4User` caricano anche le pause, che servono al motore in `OrariSchema_4User_OutModel`.
- Registrazione DI automatica (AutoRegisterDi), nessuna modifica agli installer.
- I modelli di `Dip_GG_Result` e `Dip_Periodo` espongono i nuovi campi.
- Messaggi di validazione in it/en (`AttendanceTrackingMessages`).

### 8.2 Client

| Dove | Modifica |
|---|---|
| `orari-edit-page` | Nuova scheda **"Pause"**, visibile solo per orari `IntervalloOrario`: una sezione Pranzo (modalità, durata, soglia, finestra, retribuita, causale) e una sezione Pause brevi (retribuita, durata massima) |
| Modelli TS | `Par_OrarioPausaModel`, `PausaTipo`, `PausaModalita` in `par-orario-model.ts`; campi nuovi in `dip-gg-result`, `dip-periodo` (anche il giorno del drill-down) e `par-giustificativi` |
| `dip-ggresult.service.ts:56-58` | `ALL_WARNING_DETAILS` include `Warning_3`, `Warning_4` e `Warning_5` |
| `time-sheet.service.ts` | Testi di `Warning_3/4/5` ed `Err_5` al posto di `avvertimentoN`/`erroreN`; nuovo `pausaGiorno()` |
| Cartellino (`time-sheet-*-page`) | Nel popup dello stato del giorno: riga "Pausa: timbrata 0:30 · detratta 0:30 · pagata 0:10"; avvisi con il testo specifico |
| `quadratura-mensile-page`, `cartellino-quadratura` | Nota "pausa detratta" nei totali mensili; voce "Pausa detratta" con drill-down sui giorni |
| `justification-edit-page` | Interruttore "Pausa non fruita" (`annullaPausa`) |
| `time-clock-user-page` | Nessuna modifica: la timbratura resta `SenzaVerso`. Facoltativo: un avviso del tipo "la pausa pranzo è detratta automaticamente" |
| `formule-edit-page` | Nessuna modifica: il catalogo si genera dagli attributi `[FormulaApi]` dei nuovi campi |
| i18n | Nuove chiavi in `assets/i18n/attendance-tracking/it.json` e `en.json` |
| Manuale | Nuovo paragrafo in `Manuals/attendance.guide.it.md` e `.en.md` (e `attendance.it/en.md`) |

---

## 9. Fase 2: buono pasto (U8)

È fuori dal perimetro iniziale, ma il modello lo prevede.

- Si aggiungono a `Par_OrarioPausa` (o al profilo) i campi `BuonoPasto_SogliaMin` e `IdCausale_BuonoPasto`, quest'ultima a importo.
- Dopo `ApplicaPausa`, se `Lavorato >= soglia` e c'è una pausa rilevata o detratta, il motore scrive la causale a importo come oggi i giustificativi a importo (`GeneraCausali`, righe 2132-2135).
- In alternativa, per le aziende Pro, il buono pasto si può già ottenere con una formula della fase Giorno usando `x.Result.PausaRilevata`/`PausaDetratta`.

---

## 10. Piano di lavoro

| Fase | Contenuto | File principali |
|---|---|---|
| 0. Sequenza | Interpretazione delle timbrature per zone con gli intervalli di riconoscimento (§3.3, §5) e validazione degli intervalli, rilasciabile da sola | `TimeSheet_EngineService.cs`, nuovo `GestionePresenzeSequenza.cs`, `Par_OrarioIntervalloHHService.cs`, dialog coppie |
| 1. Dati | Entità `Par_OrarioPausa`, enum, campi in `Dip_GG_Result`/`Dip_Periodo`, configurazione EF, migrazione, seed | `Entities/Tenant/GestionePresenze/`, `AttendanceTrackingDbContext.cs`, `GestionePresenzeParametriDefault.cs` |
| 2. Servizi e API | Repository, servizio, controller, caricamento in `Get_OrariSchema_4User` | `Repositories/Tenant/GestionePresenze/`, `ClientServer-Service/GestionePresenze/`, `Controllers/GestionePresenze/` |
| 3. Motore | Helper unico delle ore teoriche, timbrature interne facoltative, `GeneraTimbratureMancanti`, `SegmentiLavorati`/`ApplicaPausa`, flag e totali | `TimeSheet_EngineService.cs`, `GestionePresenzeOreTeoriche.cs`, nuovo `GestionePresenzePause.cs` |
| 4. Integrazioni | Formule, export, (fase 2) presenti e statistiche | `FormuleEngine/`, `TimeSheet_ExportService` |
| 5. Client | Scheda Pause, cartellino, quadratura, avvisi, i18n, manuali | `pages/GestionePresenze/`, `Utility/GestionePresenze/time-sheet.service.ts` |
| 6. Verifica | Scenari della tabella §3.4 più U4, U7, U9, poi ricalcolo di un mese con orari senza pausa per confermare che **i risultati non cambiano** (salvo i giorni con uscite intermedie, che vanno confrontati a mano) | DB di prova (vedi note di test API in locale) |

---

## 11. Decisioni

Le domande aperte sono state chiuse con le proposte dell'analisi (da rivedere se l'uso reale lo richiede):

| # | Domanda | Scelta adottata |
|---|---|---|
| 1 | Livello di configurazione | Solo l'orario (`Par_OrarioPausa`), nessun default aziendale |
| 2 | Collocazione della pausa automatica | Dall'inizio della finestra (§3.5) |
| 3 | Soglia su lordo o netto | Lavorato **lordo**: la detrazione può portare sotto la soglia (6h15 → 5h15) |
| 4 | Riaccredito della pausa retribuita | Sì, in entrambe le modalità, fino a `RetribuitaMin` |
| 5 | Terza modalità "Rilevata con integrazione" | No: `Automatica` copre già l'integrazione |
| 6 | Buono pasto | Fase 2 (oggi ottenibile con una formula della fase Giorno, §9) |
| 7 | Statistiche attività | Invariate: restano le ore di presenza |
| 8 | Piano di licenza | Nessun vincolo: funzione del piano base |
| 9 | Correzione uscite intermedie | Inclusa: i giorni con uscite intermedie cambiano al prossimo ricalcolo |
| 10 | Doppie timbrature | Soglia fissa di 2' (`GestionePresenzeSequenza.MinutiDoppia`) |
| 11 | Ancora fuori intervallo | Avviso `Warning_5`, non blocca la giornata |
| 12 | Intervalli già salvati incoerenti | Segnalati al salvataggio dell'orario, nessuna migrazione dati |
| 13 | Annullare la detrazione sul giorno (dal confronto §2.2) | Giustificativo con `AnnullaPausa` (seed `PNF` "Pausa non fruita") |
| 14 | Scaglioni, pausa dopo N ore, conferma del dipendente | Fase 2 |

---

## 12. Implementazione

Branch `AttendanceTracking_Dev_pause` (worktree `wt_at_pause`), da `AttendanceTracking_Dev`.

| Livello | File |
|---|---|
| Dati | `Entities/Tenant/GestionePresenze/Par_OrarioPausa.cs` (con `PausaTipo`, `PausaModalita`); campi in `Par_Orario`, `Dip_GG_Result`, `Dip_Periodo`, `Par_Giustificativi`; `AttendanceTrackingDbContext`; `Par_OrarioPausaRepository`; migrazione `20261111153221_AttendanceTracking_Pause` |
| Funzioni pure | `_utility/GestionePresenzeSequenza.cs` (interpretazione per zone), `_utility/GestionePresenzePause.cs` (pausa e teorico), `_utility/GestionePresenzeOrarioValidazione.cs` (controlli al salvataggio) |
| Servizi | `Par_OrarioService` (Get/Put con le pause e validazione), `Par_OrarioIntervalloHHService` (validazione intervalli), `Par_CausaliService` (causale della pausa "in uso"), `Par_GiustificativiService`, `Dip_PeriodoService`, `GestionePresenzeOreTeoriche`, `GestionePresenzeParametriDefault` |
| Motore | `TimeSheet_EngineService`: `AssegnaVersoTimbrature` e `GeneraTimbratureMancanti` con `GestionePresenzeSequenza`, `AncoreFacoltative`, `SegmentiLavorati` (base di `CalcolaOreLavorate` e `GeneraCausali`), pausa in `CalcolaOreTeoriche`, `Min_Pausa*` e avvisi in `GeneraCausali`, totale in `AggiornaCartellinoMese`; rimossi `AccoppiaTimbratureFuoriOrario` e `Timbratura_From_Coppia` |
| Formule | `FormulaContext`: `x.Result.Min_PausaRilevata/Detratta/Retribuita`, `x.Orario.PausaMin`, `x.Orario.PausaModalita` |
| Messaggi | `AttendanceTrackingMessages(.en).resx`: 13 messaggi di validazione |
| Client | vedi §8.2 |

Differenze rispetto all'analisi:

- niente controller `Par_OrarioPausa`: le pause si salvano con l'orario (§8.1);
- l'abbinamento usa solo le timbrature valide (dirette o approvate): quelle in attesa di approvazione non occupano più un'ora attesa;
- con timbrature mancanti la giornata resta senza versi (prima restava il verso del calcolo precedente);
- anche le timbrature fuori orario dispari (prima ignorate in silenzio) danno `Err_5`;
- i segmenti lavorati si accoppiano in ordine di timbratura e non per posizione fissa, quindi un arrotondamento che inverte due timbrature vicine non fa più perdere la coppia.

Limite noto, fuori perimetro: le ore teoriche di una coppia a cavallo della mezzanotte valgono 0 (comportamento preesistente di `CalcolaOreTeoriche`); la pausa del turno notturno si detrae correttamente dal lavorato.

Verifiche:

- programma di prova usa e getta sulle funzioni pure: 24 controlli superati (casi della tabella §3.3 e §3.4, pausa breve retribuita, pranzo flessibile con intervalli sovrapposti, turno notturno con pausa 02:00-02:30, orario continuato con 30' pagati, validazioni, finestra predefinita 12:45-14:15);
- build del server (0 errori, nessun nuovo avviso) e del client (`ng build`, 0 errori).
- prova end-to-end delle API su database usa e getta (`nvxapp_pausetest`, poi eliminato): migrazioni e seed (`DIURNO8H` con pausa rilevata, `CONT8H` automatica, `PNF`), creazione azienda, salvataggio orario in automatica, validazione della finestra incompleta, calcolo di tre giorni:
  - 09:00, 10:30, 10:45, 13:00, 14:00, 18:00 con pausa da timbrare: versi E U E U E U, 7h45, pausa rilevata 60, ore inferiori (`Err_2`) per i 15' di caffè;
  - 09:00, 18:00 con pausa automatica: 8h, 60' detratti, nessun errore di timbrature mancanti;
  - 09:00, 13:00, 13:30, 18:00 con pausa automatica: 8h, 30' rilevati e 30' detratti, avviso `Warning_5` (rientro fuori dal suo intervallo 13:45-14:15).

Osservazione fuori perimetro: inserendo timbrature in rapida sequenza, i ricalcoli in parallelo possono fallire con violazione di `IX_Dip_Periodo_IdDip_RapportoLavoro_Anno_Mese` (cartellino mensile creato due volte); il giorno resta da ricalcolare. Comportamento preesistente.

---

## 13. Checklist

### Analisi
- [x] Verifica dello stato attuale nel codice (nessuna gestione esplicita delle pause)
- [x] Casi d'uso pausa pranzo (U1-U8)
- [x] Verifica del difetto sulle uscite intermedie e casi d'uso pause brevi (U9-U10)
- [x] Riferimenti al codice aggiornati dopo le formule personalizzate; slot di stato rivisti (`Warning_3`/`Warning_4`)
- [x] Interpretazione della sequenza con gli intervalli di riconoscimento (zone, abbinamento ordinato, finestra pausa predefinita, §3.3)
- [x] Verifica dell'assenza di validazione degli intervalli (server e client); slot `Warning_5`/`Err_5` verificati liberi
- [x] "Il dipendente deve timbrare la pausa?" (§2.1) e confronto con altri applicativi (§2.2)
- [x] Decisioni sulle domande aperte (§11)

### Fase 0: interpretazione della sequenza per zone
- [x] Funzione pura `GestionePresenzeSequenza.Interpreta` (doppie, abbinamento ordinato, zone, ancore mancanti, controllo alternanza)
- [x] Uso in `AssegnaVersoTimbrature` e `GeneraTimbratureMancanti` (sostituisce `AccoppiaTimbratureFuoriOrario`)
- [x] Avviso `Warning_5` (doppia / ancora fuori intervallo) ed errore `Err_5` (sequenza non interpretabile), testi client
- [x] Validazione degli intervalli in `Par_OrarioIntervalloHHService` e `Par_OrarioService`
- [ ] Validazione degli intervalli anche nel dialog delle coppie (client): oggi l'errore arriva dal server al salvataggio
- [x] Verifica dei casi della tabella §3.3 (programma di prova)

### Fase 1: dati
- [x] Entità `Par_OrarioPausa` ed enum `PausaTipo`, `PausaModalita`
- [x] Campi `Min_Pausa*` in `Dip_GG_Result`, `Min_PausaDetratta` in `Dip_Periodo`, `AnnullaPausa` in `Par_Giustificativi`
- [x] Configurazione EF in `AttendanceTrackingDbContext` e migrazione
- [x] Seed: pausa su `DIURNO8H`, nuovo orario `CONT8H`, giustificativo `PNF`

### Fase 2: servizi e API
- [x] Repository; Get/Put delle pause con l'orario, validazioni e messaggi it/en
- [x] Caricamento pause in `Par_OrarioGet` e `Get_OrariSchema_4User`
- [x] Registrazione DI (automatica)
- [x] Nuovi campi nei modelli `Dip_GG_Result`, `Dip_Periodo`, `Dip_Periodo_Giorno`, `Par_Giustificativi`

### Fase 3: motore
- [x] Pausa nelle ore teoriche (motore + `GestionePresenzeOreTeoriche`, stessa funzione `VariazioneTeorico`)
- [x] Timbrature interne facoltative con pausa automatica (§6.2)
- [x] `GeneraTimbratureMancanti` non genera le timbrature della pausa (§6.3)
- [x] `GestionePresenzePause.Applica` (funzione pura) e `SegmentiLavorati`
- [x] `CalcolaOreLavorate` e `GeneraCausali` sui segmenti netti
- [x] Esito in `Dip_GG_Result` (`Min_Pausa*`, `Warning_3`, `Warning_4`) e totale in `AggiornaCartellinoMese`
- [x] Giustificativo "Pausa non fruita" (`AnnullaPausa`)

### Fase 4: integrazioni
- [x] Formule: dati pausa in `FormulaDatiGiorno`, `x.Result`, `x.Orario` (catalogo dagli attributi)
- [x] Export: minuti detratti sulla causale `IdCausale_Pausa` (esportata come le altre causali)
- [ ] (fase 2) Voce "Pausa detratta" nel riepilogo del cartellino PDF/export
- [ ] (fase 2) Stato "In pausa" in personale presente e widget
- [ ] (fase 2) Buono pasto
- [ ] (fase 2) Soglie a scaglioni, pausa "dopo N ore", conferma del dipendente

### Fase 5: client
- [x] Modelli TS (`Par_OrarioPausaModel`, campi pausa, `annullaPausa`)
- [x] Scheda "Pause" in `orari-edit-page`
- [x] Interruttore "Pausa non fruita" in `justification-edit-page`
- [x] Maschera avvisi (`Warning_3`, `Warning_4`, `Warning_5`) e testi in `time-sheet.service.ts`
- [x] Dettaglio pausa nel popup del giorno del cartellino
- [x] "Pausa detratta" nella quadratura mensile e nel riepilogo del cartellino
- [x] Chiavi i18n it/en
- [x] Manuali `attendance.guide.it/en.md`

### Fase 6: verifica
- [x] Build server e client
- [x] Scenari della tabella §3.4, U4 (finestra), U7 (notturno), U9/U10 (pause brevi) sulle funzioni pure
- [x] Prova end-to-end delle API su database usa e getta (migrazione, seed, salvataggio orario, calcolo giorno)
- [ ] Ricalcolo di un mese reale con orari senza pausa: risultati invariati salvo i giorni con uscite intermedie
- [ ] Catena dei merge (AttendanceTracking_Dev → AttendanceTracking → AllFunction_Dev → AllFunction), previa conferma
