# Presenze: turni e pianificazione (AT-TURNI)

Data: 11/11/2026. Ramo di lavoro `AttendanceTracking_Dev_f4` (worktree `wt_at-f4`), da `AttendanceTracking_Dev`.
Area `AT-TURNI` di `ANALISI_INFRASTRUCTURE_NUOVI_MODULI.md` (§3.1, §10.2).

## 1. Obiettivo

Gestire il lavoro a turni nelle Presenze:

1. **modelli di turno** (mattina, pomeriggio, notte, riposo...) collegati a un orario esistente (`Par_Orario`);
2. **turni abilitati per reparto** (reparti e sedi comuni: `Az_Sedi`/`Az_SediReparto`, schema Common) con la **copertura minima** di persone per turno;
3. una pagina di **pianificazione** per reparto (vista settimana o mese) che segnala le coperture insufficienti e i turni assegnati in giorni di ferie o assenze approvate;
4. lo **scambio di turni** tra colleghi, accettato dal collega e approvato con il flusso delle richieste esistente;
5. il **confronto** tra il turno previsto e le timbrature nel calcolo della giornata: il turno pianificato, se c'è, sostituisce l'orario del profilo per quel giorno.

## 2. Situazione di partenza

- L'orario di un giorno si ricava dal profilo orario del rapporto (`Dip_ProfiloOrario` → `Par_ProfiloOrario` → `Par_ProfiloOrarioGG` → `Par_Orario`), settimanale o ciclico. Il motore costruisce un `DaySlot` per dipendente e giorno in `TimeSheet_EngineService.Get_OrariSchema_4User` (regione "DaySlots") e tutti i passi del calcolo (ore teoriche, verso delle timbrature, causali, pause, eccedenze) leggono l'orario dal `DaySlot` (`Orari.OrderBy(ZOrder).First()`).
- Un secondo calcolo delle ore teoriche (`_utility/GestionePresenzeOreTeoriche`) serve a contatori, piano ferie e ore dei giustificativi a giornata intera.
- Il profilo ciclico copre già i turni "a rotazione fissa", ma non la pianificazione libera, le coperture e gli scambi.
- Le richieste (`Dip_GG_Richiesta`) hanno un flusso di approvazione per reparto (`Az_SediRepartoUser.EnabledToApproval`, `Az_Cfg.ApprovazioneTipo`) usato per timbrature, giustificativi e note spese.

## 3. Modello dati (contesto `AttendanceTrackingDbContext`, migration `AttendanceTracking_Turni`)

| Tabella | Campi | Note |
|---|---|---|
| `Par_Turno` | `IdAz_Anagrafica`, `Codice`(10), `Descrizione`(50), `IdPar_Orario`?, `Colore`(7)? | Modello di turno. `IdPar_Orario` null = **riposo** (vale l'orario festivo del profilo: 0 ore). FK su `Par_Orario` Restrict |
| `Az_SediRepartoTurno` | `IdAz_SediReparto`, `IdPar_Turno`, `MinPersone`, `GiorniSettimana`(7) | Turno usato nel reparto e copertura minima. `GiorniSettimana` = maschera lun..dom (`1111100` = solo feriali) dei giorni in cui vale la copertura. Indice univoco reparto+turno. FK sul reparto comune (Cascade) |
| `Dip_Turno` | `IdDip_RapportoLavoro`, `Data`(date), `IdPar_Turno`, `IdAz_SediReparto`?, `Note`(200)? | Turno pianificato. **Un turno per dipendente e giorno** (indice univoco rapporto+data) |
| `Dip_TurnoScambio` | `IdDip_Turno`, `IdDip_Turno_Collega`?, `IdDip_RapportoLavoro`, `IdDip_RapportoLavoro_Collega`, `Stato`, `IdDip_GG_Richiesta`?, `Note`(200)?, `DataRisposta`? | Proposta di scambio. Senza `IdDip_Turno_Collega` è una **cessione** (il collega prende il turno) |

`Dip_GG_Richiesta.TipoRichiesta` riceve il valore `ScambioTurno = 4` (nessuna modifica di schema: l'enum è un intero).
Le tabelle comuni (`Az_SediReparto`) restano escluse dalle migration; le nuove FK puntano alle tabelle comuni come quelle di AT-COM.

## 4. Pianificazione e coperture

- Dipendenti del reparto: `Az_SediRepartoUser` con `UserInDepartment` (stesso criterio delle approvazioni), con il rapporto di lavoro.
- Per ogni giorno e turno abilitato: **assegnati** = dipendenti con quel turno nel reparto, **esclusi** quelli con un'assenza approvata a giornata intera (giustificativo con segno "−", stato Diretta o Approvata, `AllDay` o ore ≥ teoriche). Copertura **insufficiente** se assegnati < `MinPersone` nei giorni della maschera.
- **Conflitti** segnalati: turno assegnato in un giorno di assenza approvata; assenza in attesa di approvazione (avviso).
- **Consuntivo**: per i giorni già calcolati la pagina mostra ore teoriche/lavorate e lo stato del giorno (`Dip_GG_Result`), cioè il confronto previsto/timbrato.
- Salvataggio: insert/update/delete dei `Dip_Turno` della griglia e ricalcolo asincrono dei giorni modificati (`ITimeSheet_EngineService_OnlyCalculate`).

## 5. Scambio turni

1. Il dipendente propone lo scambio di un suo turno con quello di un collega dello stesso reparto (o la cessione a un collega libero quel giorno): `Dip_TurnoScambio` in stato `InAttesaCollega`, notifica al collega.
2. Il collega accetta o rifiuta. Se accetta si crea una `Dip_GG_Richiesta` di tipo `ScambioTurno` **del richiedente** (`Dip_GG_RichiestaService.Send`): stessi approvatori, stessi stati, stessa pagina "Richieste" e stesse notifiche. `Dati` contiene `{ idDip_TurnoScambio, testo }`.
3. Approvazione: `SetState` alla prima transizione verso `Approvata` chiama `IDip_TurnoScambioHandler.ApplicaAsync` che scambia i turni (stesso giorno: si scambiano turno e reparto; giorni diversi: si scambiano i proprietari, purché nessuno dei due abbia già un turno nel giorno che riceve) e ricalcola i giorni dei due dipendenti. Rifiuto/annullamento: stato `Rifiutato`/`Annullato`. **Revoca approvata**: lo scambio si annulla (turni rimessi come prima).
4. Il controllo dei conflitti si ripete all'approvazione (i turni possono essere cambiati nel frattempo): se lo scambio non è più possibile l'approvazione dà un messaggio e lo scambio resta in approvazione.

## 6. Impatti sul calcolo (AT-TURNI.4)

Integrazione **minima** in due punti, documentata nel codice:

- `TimeSheet_EngineService.Get_OrariSchema_4User`, regione DaySlots: dopo l'orario del profilo (e del festivo) si cercano i `Dip_Turno` dei rapporti nel periodo. Se il giorno ha un turno pianificato, `Orari` diventa **solo** l'orario del turno (`ZOrder` 1); turno di riposo = orario festivo del profilo. Il `DaySlot` riceve `IdDip_Turno` e `IdPar_Turno`. Tutto il resto del motore (tolleranze, verso, pause, causali delle coppie, eccedenze, formule) lavora sull'orario del turno senza modifiche: il confronto tra turno previsto e timbrature è quello già fatto per l'orario (ore mancanti = `Err_2`, ore in più = `Warning_1`, pausa...).
- `GestionePresenzeOreTeoriche.GetOreTeoriche`: stessa sostituzione, così contatori, piano ferie e ore dei giustificativi a giornata intera vedono il turno.

Senza turni pianificati nulla cambia (stesso `DaySlot`, stesse ore). Il turno vale solo nei giorni coperti da un profilo orario (senza profilo non c'è `DaySlot`).

## 7. API e client

Server (`Controllers/GestionePresenze`, servizi `ClientServer-Service/GestionePresenze`):
- `Par_Turno`: `GetAll`, `Par_TurnoGet`, `Par_TurnoPut`, `Par_TurnoDelete` (CompanyPowerAdmin; un turno usato non si cancella).
- `Az_SediRepartoTurno`: `GetAll4Reparto`, `Put4Reparto` (elenco completo del reparto).
- `Dip_Turno`: `Pianificazione` (griglia, coperture, assenze, conflitti, consuntivo), `Put` (modifiche della griglia), `GetAll4User` (miei turni e turni dei colleghi del reparto) — pianificazione per CompanyAdmin/CompanyPowerAdmin.
- `Dip_TurnoScambio`: `Proponi`, `Rispondi`, `GetAll4User`, `Annulla`.

Client:
- `/turnolist`, `/turnoedit` (Parametri, CompanyPowerAdmin), `/turnocoperture` (turni e coperture del reparto).
- `/pianificazioneturni` (CompanyAdmin e CompanyPowerAdmin): reparto, settimana/mese, griglia dipendenti × giorni, riga delle coperture evidenziata, assenze, consuntivo; clic sulla cella = scelta del turno.
- `/mieiturni` (dipendente): turni dei prossimi giorni, proposte inviate e ricevute, "Proponi scambio", accetta/rifiuta.
- `TipoRichiesta.ScambioTurno` nei testi delle richieste (pipe e dettaglio).

## 8. Decisioni

| # | Decisione | Esito |
|---|---|---|
| T1 | Fascia della copertura minima | **Scelta provvisoria**: la fascia coincide con il **modello di turno** (mattina/pomeriggio/notte), non con fasce orarie libere |
| T2 | Turni per dipendente e giorno | **Scelta provvisoria**: uno solo (indice univoco). Gli spezzati si fanno con un orario a più coppie |
| T3 | Turno e festività | **Scelta provvisoria**: il turno pianificato prevale anche nei festivi (il giorno resta festivo per la qualifica dello straordinario) |
| T4 | Turno di riposo | **Scelta provvisoria**: turno senza orario = orario festivo del profilo (0 ore) |
| T5 | Scambio: chi approva | **Scelta provvisoria**: accettazione del collega + approvatori del reparto del richiedente (flusso delle richieste esistente, `TipoRichiesta.ScambioTurno`) |
| T6 | Assenze che riducono la copertura | **Scelta provvisoria**: solo quelle approvate (o dirette) a giornata intera con segno "−"; quelle in attesa sono un avviso |
| T7 | Turno senza profilo orario | Ignorato dal calcolo (serve il profilo per qualifiche e festivo) |

## 9. Checklist

Legenda: ✅ svolto · ⏳ da fare · 🔄 in corso

- ✅ `AT-TURNI.0` Analisi (questo documento)
- ✅ `AT-TURNI.1` Modello dei turni: `Par_Turno`, `Az_SediRepartoTurno`, `Dip_Turno`, `Dip_TurnoScambio`, migration `AttendanceTracking_Turni`, repository, servizi e API dei parametri (turni, coperture del reparto); client `/turnolist`, `/turnoedit`, `/turnocoperture`
- ✅ `AT-TURNI.2` Pianificazione: API `Dip_Turno/Pianificazione` e `Dip_Turno/Put` (coperture, assenze approvate, conflitti, consuntivo, ricalcolo); client `/pianificazioneturni` (settimana/mese)
- ✅ `AT-TURNI.3` Scambio turni: proposta, risposta del collega, richiesta `ScambioTurno` con gli approvatori esistenti, applicazione all'approvazione e annullamento alla revoca; client `/mieiturni` e testi delle richieste
- ✅ `AT-TURNI.4` Turno pianificato nel calcolo della giornata (`Get_OrariSchema_4User`, `GestionePresenzeOreTeoriche`), senza cambiare i calcoli senza turni
- ✅ `AT-TURNI.5` Prova su DB usa e getta (copertura insufficiente, scambio approvato, calcolo con turno, calcolo senza turni invariato) — 49 controlli API: modelli e coperture, griglia con coperture insufficienti e ferie approvata (conflitto), scambio fra giorni diversi approvato e poi revocato (turni ripristinati), scambio rifiutato dal collega, scambio nello stesso giorno, calcolo: riposo pianificato = 0 ore, sabato con turno = 8 ore teoriche e lavorate (ORD1), dipendente senza turni con il profilo invariato
