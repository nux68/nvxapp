# Presenze: gestione della pausa pranzo (timbrata e automatica)

Analisi di come il programma tratta oggi la pausa pranzo, quali casi d'uso servono e come estendere modello dati, motore di calcolo e interfaccia per gestire:

- la **pausa timbrata**, con controllo di durata minima, durata massima e finestra;
- la **pausa automatica**, cioè la detrazione della pausa quando il dipendente non la timbra.

Il documento segue lo stesso schema di `GestionePresenze_Straordinario.md`.

Riferimenti nel codice (branch `AttendanceTracking_Dev`):

- `nvxapp.server.service/ClientServer-Service/GestionePresenze/TimeSheet_EngineService/TimeSheet_EngineService.cs`: il motore. I metodi coinvolti sono `CalcolaGiorno` (1066), `CalcolaOreTeoriche` (1304), `CalcolaOreLavorate` (1354), `AssegnaVersoTimbrature` (1411), `GeneraTimbratureMancanti` (1599) e `GeneraCausali` (1969).
- `nvxapp.server.service/ClientServer-Service/GestionePresenze/_utility/GestionePresenzeOreTeoriche.cs:86-99`: un secondo calcolo delle ore teoriche, usato da contatori e piano ferie.
- `nvxapp.server.service/ClientServer-Service/GestionePresenze/_utility/GestionePresenzeParametriDefault.cs:120-150`: l'orario di default `DIURNO8H`.
- `nvxapp.server.data/Entities/Tenant/GestionePresenze/`: le entità `Par_Orario`, `Par_OrarioIntervalloHH`, `Dip_GG_Timbratura`, `Dip_GG_Result` e `Dip_Periodo`.
- `nvxapp.server.data/Infrastructure/AttendanceTrackingDbContext.cs`: la configurazione EF (`Par_Orario` → `Par_OrarioIntervalloHH` alle righe 497-589).

---

## 1. Situazione attuale

### 1.1 La pausa esiste solo come "buco" tra due coppie

Nel modulo presenze non c'è nessun concetto esplicito di pausa: niente entità, campi, enum, parametri, servizi, testi i18n o elementi di interfaccia per pausa, pranzo, mensa o buono pasto. Le uniche occorrenze di "pranzo" sono esempi di nota spese nel manuale (`Manuals/attendance.guide.it.md:227,239`).

La pausa è rappresentata **implicitamente** come l'intervallo tra due coppie dell'orario (`Par_OrarioIntervalloHH`). L'orario di default `DIURNO8H` ha le coppie 09:00-13:00 e 14:00-18:00. Il buco 13:00-14:00 è di fatto la pausa pranzo:

| Calcolo | Come lo fa oggi | Effetto sulla pausa |
|---|---|---|
| Ore teoriche (`CalcolaOreTeoriche`) | Somma delle durate delle coppie | Il buco non è mai teorico, quindi 8h |
| Ore lavorate (`CalcolaOreLavorate`, `GeneraCausali`) | Somma delle coppie Entrata/Uscita timbrate | Il tempo tra l'uscita delle 13 e l'entrata delle 14 non conta |
| Accoppiamento (`AssegnaVersoTimbrature`) | Si aspetta una timbratura per ogni `Dalle`/`Alle` con `*_Use_4_Match = true` | Servono **4 timbrature** |

### 1.2 Cosa succede nei casi reali

| Caso | Timbrature | Esito oggi |
|---|---|---|
| Pausa timbrata regolare | 08:58, 13:02, 13:57, 18:03 | Corretto: 8h lavorate |
| Pausa breve timbrata (20 min) | 09:00, 13:00, 13:20, 18:00 | 8h40 lavorate. I 40 minuti in più diventano eccedenza o straordinario. Nessun controllo di pausa minima |
| Pausa lunga timbrata (90 min) | 09:00, 13:00, 14:30, 18:00 | 7h30 lavorate, 30 minuti mancanti (`Err_2`). Corretto |
| **Pausa non timbrata** | 09:00, 18:00 | Le timbrature sono meno di quelle attese, quindi `Err_TimbratureMancanti`. Con l'opzione "Genera timbrature mancanti" il motore **inventa** le timbrature 13:00 e 14:00 |
| Orario continuato a coppia unica (08:30-17:30) | 08:30, 17:30 | 9h lavorate su 9h teoriche. Nessuna pausa e nessuna detrazione |
| Orari a monte ore (`MonteOre`, `MonteOreValore`) | valori orari | La pausa non è applicabile |

### 1.3 Limiti

1. Non c'è **detrazione automatica**: chi non timbra la pausa non ottiene un risultato corretto senza correzioni manuali o timbrature generate.
2. Non c'è il controllo di **pausa minima**. Il D.Lgs. 66/2003, art. 8, prevede una pausa quando l'orario giornaliero supera le 6 ore. Durata e modalità sono fissate dal CCNL, con un minimo di 10 minuti in mancanza di regole contrattuali. Oggi una pausa insufficiente non viene segnalata e il tempo in più diventa eccedenza.
3. Non c'è il concetto di **finestra della pausa** (per esempio tra le 12:00 e le 14:30) né di pausa flessibile.
4. Non c'è la **pausa retribuita** (per esempio mensa interna retribuita o 10 minuti di pausa contrattuale pagati).
5. In `Dip_GG_Result` e `Dip_Periodo` non c'è nessun dato sulla pausa. L'HR non vede né la pausa fatta né quella detratta.
6. Le ore teoriche si calcolano in **due punti** (motore e `GestionePresenzeOreTeoriche`) con la stessa regola duplicata.

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

---

## 3. Scelte di progetto

### 3.1 Dove configurare la pausa: nell'**orario** (proposta)

| Opzione | Pro | Contro |
|---|---|---|
| **A. Orario (`Par_Orario`)**, proposta | Un profilo contiene giorni con e senza pausa (sabato a mezza giornata, part-time 4h). La pausa dipende dalla forma della giornata | La configurazione va ripetuta su ogni orario (mitigato dai default di seed) |
| B. Profilo orario (`Par_ProfiloOrario`) | Stesso schema dei parametri dello straordinario | Non distingue i giorni del profilo. Bisognerebbe comunque escludere gli orari corti |
| C. Azienda (`Az_Cfg`) con override per orario | Un default unico per azienda | Due livelli da gestire. Il precedente `Par_Arrotondamenti` è rimasto non collegato al motore |

La proposta è la A, con una tabella figlia **`Par_OrarioPausa`** (da 0 a n righe per orario) sul modello di `Par_OrarioIntervalloHH`, invece di molte colonne su `Par_Orario`. Oggi serve una sola pausa (il pranzo), ma la tabella figlia regge senza modifiche i turni lunghi con due pause (U7). La prima versione dell'interfaccia gestisce una sola riga.

### 3.2 Non generare timbrature fittizie

La pausa automatica **non crea record in `Dip_GG_Timbratura`**. È un calcolo che il motore ripete a ogni ricalcolo.

L'alternativa, cioè generare timbrature come fa `GeneraTimbratureMancanti`, è scartata per tre motivi:
- sporca il dato originale del dipendente;
- se i parametri della pausa cambiano, le timbrature generate restano;
- confonde l'HR, che non distingue più una timbratura reale da una fittizia.

### 3.3 Una sola formula per pausa timbrata e automatica

Per ogni giorno e per ogni pausa configurata:

```
PausaRilevata  = somma dei buchi tra un'Uscita e l'Entrata successiva che cadono nella finestra della pausa
                 (senza finestra: il buco più lungo tra le coppie timbrate)
LavoratoLordo  = somma delle coppie E/U (come oggi)
PausaDovuta    = LavoratoLordo > SogliaMin ? DurataMin : 0
Detrazione     = Modalita == Automatica
                   ? max(0, PausaDovuta - PausaRilevata - RetribuitaMin)
                   : 0
Lavorato       = LavoratoLordo - Detrazione
```

Con questa formula i casi della §2 si risolvono tutti nello stesso modo:

| Caso | Rilevata | Dovuta | Detrazione | Lavorato |
|---|---|---|---|---|
| 09:00-18:00, nessuna pausa timbrata (U2) | 0' | 60' | 60' | 8h |
| 09:00-13:00 / 13:30-18:00 (U1) | 30' | 60' | 30' | 8h |
| 09:00-13:00 / 14:30-18:00 | 90' | 60' | 0' | 7h30 (30' mancanti, come oggi) |
| 08:00-13:00, permesso pomeridiano (U5) | 0' | 0' (5h < 6h) | 0' | 5h |
| Continuato 08:30-17:30 con 30' retribuiti (U6) | 0' | 60' | 30' | 8h30 (teorico 8h30, §5.1) |

Con la modalità **Rilevata** la detrazione è sempre 0. Se però la pausa dovuta supera quella rilevata, il giorno riceve l'avviso di pausa insufficiente.

### 3.4 Dove collocare i minuti detratti

Le ore lavorate vengono attribuite alle causali per sovrapposizione con le coppie (`GeneraCausali`, `Sovrapposizione` 2302), e le ore notturne per fascia (`OreNotturne` 2329). Per questo la detrazione deve cadere in un punto preciso della giornata.

**Regola proposta:** la pausa virtuale inizia a `FinestraDalle`. Senza finestra inizia alla `Alle` della coppia che precede il buco. Su un orario a coppia unica, senza finestra, cade a metà della presenza. I minuti `[inizio, inizio + Detrazione]` vengono tolti dal segmento E/U che li contiene. Se il segmento è più corto, la parte restante passa al segmento successivo.

---

## 4. Modello dati

### 4.1 Nuova entità `Par_OrarioPausa` (schema tenant presenze)

| Campo | Tipo | Note |
|---|---|---|
| `Id`, campi `BaseEntity` | | |
| `IdPar_Orario` | int, FK | cascade come `Par_OrarioIntervalloHH` |
| `NumPausa` | int | ordine (1 = pranzo) |
| `Descrizione` | string(50) | "Pausa pranzo" |
| `Modalita` | `PausaModalita` | `Rilevata` = solo controllo e avviso; `Automatica` = detrae la differenza |
| `DurataMin` | int | durata minima o standard in minuti (es. 60) |
| `DurataMaxMin` | int? | oltre questa durata: avviso `Warning_3` (informativo; i minuti in più sono già ore mancanti) |
| `SogliaMin` | int | la pausa è dovuta se il lavorato lordo supera questi minuti (default 360 = 6h) |
| `FinestraDalle` | TimeOnly? | inizio della finestra in cui la pausa è riconosciuta e collocata |
| `FinestraAlle` | TimeOnly? | fine della finestra. Se `FinestraAlle < FinestraDalle`, la finestra finisce il giorno dopo (turni notturni) |
| `RetribuitaMin` | int | minuti della pausa che restano pagati (default 0) |
| `IdCausale_Pausa` | int? | facoltativa: causale informativa per i minuti detratti, se servono nell'export paghe |

Enum `PausaModalita { Rilevata = 0, Automatica = 1 }`. Se un orario non ha righe di pausa, il comportamento resta **esattamente quello di oggi**.

Vincoli di validazione nel servizio:
- `DurataMin > 0`;
- `RetribuitaMin <= DurataMin`;
- `DurataMaxMin`, se valorizzata, `>= DurataMin`;
- la pausa è ammessa solo se `Par_Orario.TimbratureTipo == IntervalloOrario`.

### 4.2 Risultati

| Entità | Nuovi campi | Uso |
|---|---|---|
| `Dip_GG_Result` | `int Min_PausaRilevata`, `int Min_PausaDetratta` | Dettaglio del giorno nel cartellino |
| `Dip_GG_Result.Stato` | `Warning_2` = pausa insufficiente o non fruita; `Warning_3` = pausa oltre la durata massima | Gli slot sono già presenti e liberi (nel motore sono usati solo `Err_2` e `Warning_1`) |
| `Dip_Periodo` | `int Min_PausaDetratta` | Totale mensile in `AggiornaCartellinoMese` |

### 4.3 Migrazione e seed

- Nuova migrazione in `nvxapp.server.data/Migrations/AttendanceTracking/`, con una tabella e cinque colonne.
- `GestionePresenzeParametriDefault`:
  - `DIURNO8H` riceve una pausa `Rilevata` di 60', finestra 12:00-15:00 e soglia 6h. Il calcolo non cambia, si aggiungono solo gli avvisi.
  - Nuovo orario `CONTINUATO8H`: coppia unica 08:30-17:30 con pausa `Automatica` di 60', finestra 12:30-14:30, soglia 6h. È il caso tipico della pausa automatica.
- I dati esistenti senza righe di pausa mantengono il comportamento attuale.

---

## 5. Modifiche al motore

Tutte in `TimeSheet_EngineService.cs` salvo dove indicato.

### 5.1 Ore teoriche (U3)

`CalcolaOreTeoriche` e `GestionePresenzeOreTeoriche` sottraggono `DurataMin - RetribuitaMin` per ogni pausa **non retribuita che cade dentro una coppia**, nella misura della sovrapposizione tra la finestra (o la pausa collocata) e la coppia:

- coppia unica 08:30-17:30 con pausa di 60' → teorico 8h;
- orario a due coppie con la pausa nel buco → la sovrapposizione è 0, quindi il teorico resta 8h come oggi.

Per non duplicare ancora la regola, il calcolo va spostato in un unico helper statico in `GestionePresenzeOreTeoriche`, chiamato anche dal motore.

### 5.2 Accoppiamento: timbrature interne facoltative (U2)

In `AssegnaVersoTimbrature`, se l'orario ha una pausa `Automatica` e il buco tra la coppia k e la coppia k+1 cade nella finestra, le timbrature attese `Alle(k)` e `Dalle(k+1)` diventano **facoltative**:

- non concorrono al conteggio che produce `Err_TimbratureMancanti`;
- se ci sono solo 2 timbrature, la prima si abbina a `Dalle(1)` e l'ultima a `Alle(n)`, con le loro tolleranze e i loro arrotondamenti;
- se le timbrature interne ci sono, si abbinano come oggi e la pausa diventa "rilevata".

### 5.3 Niente timbrature generate per la pausa

`GeneraTimbratureMancanti` non deve generare `Alle(k)` e `Dalle(k+1)` quando sono facoltative per la regola §5.2. Altrimenti il motore inventerebbe 13:00 e 14:00, la pausa risulterebbe "rilevata" e il dato verrebbe sporcato.

### 5.4 Nuovo passo `ApplicaPausa` e segmenti lavorati unici

Oggi `CalcolaOreLavorate` e `GeneraCausali` (parte B, 2057-2137) scorrono le coppie E/U **ognuno per conto proprio**. Va introdotto un metodo unico:

```csharp
List<(DateTime Dalle, DateTime Alle)> SegmentiLavorati(allData, idRapporto, giorno, out PausaEsito esito)
```

Il metodo:
1. costruisce i segmenti dalle coppie E/U arrotondate (la logica attuale);
2. per ogni `Par_OrarioPausa` dell'orario base calcola `PausaRilevata`, `PausaDovuta` e `Detrazione` (§3.3);
3. sottrae la detrazione nella posizione della §3.4;
4. restituisce i segmenti netti e l'esito (minuti rilevati, minuti detratti, avvisi).

`CalcolaOreLavorate` diventa la somma dei segmenti. `GeneraCausali` usa gli stessi segmenti per sovrapposizione con le coppie, fascia notturna, ore ordinarie ed eccedenze. In questo modo **straordinario, notturno e supplementare restano coerenti** senza toccare `GestionePresenzeEccedenze`. In `CalcolaGiorno` l'esito viene scritto in `Dip_GG_Result.Min_Pausa*` e nei flag `Warning_2` e `Warning_3`.

### 5.5 Pausa retribuita (U6)

I minuti `RetribuitaMin` sono già esclusi dalla detrazione. Nella modalità `Rilevata`, se la pausa timbrata comprende minuti retribuiti, il motore li riaggiunge come segmento lavorato alla causale della coppia precedente. **Da confermare**: vedi la domanda aperta 4.

### 5.6 Ricalcolo

Una modifica a orario o profilo **non** innesca il ricalcolo (commento a riga 54 del motore). Va bene anche per la pausa: dopo una modifica dei parametri l'HR rilancia il calcolo del periodo, come per gli altri parametri. In alternativa, `Par_OrarioPausaService` può proporre il ricalcolo dei giorni non bloccati (`Locked`).

---

## 6. API e client

### 6.1 Server

- `Par_OrarioPausa`: repository, servizio e controller `Par_OrarioPausaController` (`GetAll`, `Get`, `Put`, `Delete`), sul modello di `Par_OrarioIntervalloHHController`.
- `Par_OrarioGet` e `Get_OrariSchema_4User` caricano anche le pause, che servono al motore in `OrariSchema_4User_OutModel`.
- Registrazione DI in `nvxapp.server/Utility/Installers4AttendanceTracking.cs`.
- I modelli di `Dip_GG_Result` e `Dip_Periodo` espongono i nuovi campi.

### 6.2 Client

| Dove | Modifica |
|---|---|
| `orari-edit-page` | Nuova scheda **"Pausa"**, visibile solo per orari `IntervalloOrario`, con modalità, durata, soglia, finestra, retribuita e causale |
| Modelli TS | `par-orario-pausa-model.ts`, campi nuovi in `dip-gg-result` e `dip-periodo` |
| Cartellino (`time-sheet-*-page`, `time-sheet.service.ts:542-576`) | Nel popup del giorno: "Pausa 1:00 (automatica)" o "Pausa 0:30 timbrata, 0:30 detratti"; icona di avviso per `Warning_2`/`Warning_3` |
| `cartellino-quadratura` | Riga "Pausa detratta" nei totali mensili |
| `time-clock-user-page` | Nessuna modifica: la timbratura resta `SenzaVerso`. Facoltativo: un avviso del tipo "la pausa pranzo è detratta automaticamente" |
| i18n | Nuove chiavi in `assets/i18n/attendance-tracking/it.json` e `en.json` |
| Manuale | Nuovo paragrafo in `Manuals/attendance.guide.it.md` e `.en.md` |

---

## 7. Fase 2: buono pasto (U8)

È fuori dal perimetro iniziale, ma il modello lo prevede.

- Si aggiungono a `Par_OrarioPausa` (o al profilo) i campi `BuonoPasto_SogliaMin` e `IdCausale_BuonoPasto`, quest'ultima a importo.
- Dopo `ApplicaPausa`, se `Lavorato >= soglia` e c'è una pausa rilevata o detratta, il motore scrive la causale a importo in `Dip_GG_Causali`, come oggi i giustificativi a importo (2010-2016).

---

## 8. Piano di lavoro

| Fase | Contenuto | File principali |
|---|---|---|
| 1. Dati | Entità `Par_OrarioPausa`, enum, campi in `Dip_GG_Result`/`Dip_Periodo`, configurazione EF, migrazione, seed | `Entities/Tenant/GestionePresenze/`, `AttendanceTrackingDbContext.cs`, `GestionePresenzeParametriDefault.cs` |
| 2. Servizi e API | Repository, servizio, controller, caricamento in `Get_OrariSchema_4User` | `Repositories/Tenant/GestionePresenze/`, `ClientServer-Service/GestionePresenze/`, `Controllers/GestionePresenze/` |
| 3. Motore | Helper unico delle ore teoriche, timbrature interne facoltative, `GeneraTimbratureMancanti`, `SegmentiLavorati`/`ApplicaPausa`, flag e totali | `TimeSheet_EngineService.cs`, `GestionePresenzeOreTeoriche.cs` |
| 4. Client | Scheda Pausa, cartellino, quadratura, i18n | `pages/GestionePresenze/`, `Utility/GestionePresenze/time-sheet.service.ts` |
| 5. Verifica | Scenari della tabella §3.3 più U4 e U7 su dati di prova, poi ricalcolo di un mese con orari senza pausa per confermare che **i risultati non cambiano** | |

La logica di `ApplicaPausa` va scritta come funzione pura (input: segmenti e parametri; output: segmenti netti ed esito), come `GestionePresenzeEccedenze`. Così si può coprire con test unitari quando verrà creato il progetto xUnit previsto da `piano-security.md`.

---

## 9. Domande aperte

1. **Livello di configurazione**: la proposta è l'orario (§3.1). Serve anche un default per azienda?
2. **Collocazione della pausa automatica**: inizio finestra (proposta) o metà della presenza? Conta solo per attribuire le causali nei giorni con causali diverse tra mattina e pomeriggio, o con lavoro notturno.
3. **Soglia**: la soglia va confrontata con il lavorato lordo (proposta) o con quello netto? Per esempio, con 6h15 di presenza e 60' di pausa il lavorato scende a 5h15. Va accettato o la detrazione va limitata in modo da non scendere sotto la soglia?
4. **Pausa retribuita in modalità Rilevata**: i minuti retribuiti della pausa timbrata vanno riaccreditati come lavorato?
5. **Pausa insufficiente in modalità Rilevata**: basta l'avviso, o serve anche una terza modalità "Rilevata con integrazione" (detrae la differenza solo se la pausa è stata timbrata)? Con la formula della §3.3, `Automatica` copre già entrambi i casi.
6. **Buono pasto**: va incluso nella prima fase?
