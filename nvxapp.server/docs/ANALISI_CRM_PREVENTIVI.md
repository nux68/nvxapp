# Analisi: preventivi e offerte del CRM (CRM-PREV)

> Analisi dell'11/11/2026, branch `CRM_Dev` (lavoro su `CRM_Dev_f4`, worktree `wt_crm-f4`).
> Area `CRM-PREV` di `ANALISI_INFRASTRUCTURE_NUOVI_MODULI.md` (§3.3, §10.2: punti `CRM-PREV.1`–`CRM-PREV.3`).
> Modello generale del CRM: `ANALISI_CRM_APPLICATIVO.md` (§5.2 `Crm_Offerta`, §6.1 Offerte).
>
> **Obiettivo**: completare i preventivi del CRM (che nel CRM si chiamano **offerte**: stessa cosa, stesse tabelle)
> con il ciclo di vita completo – stati con la scadenza della validità, scadenzario, PDF prodotto con il meccanismo
> dei job, invio per email registrato come attività, esito che aggiorna l'opportunità della pipeline – e predisporre
> il passaggio futuro a un ordine dell'Amministrazione.

Legenda: ✅ svolto · ⏳ da fare · 🔍 da valutare.

## 1. Situazione di partenza

Con la Fase 1 del CRM (04/11/2026) esistono già:

- `Crm_Offerta` + `Crm_OffertaRiga`: numero/anno (numerazione per anno con prefisso in `Crm_Cfg`), data, validità
  (`ValidaFinoAl`, predefinita `Crm_Cfg.OffertaGiorniValidita`), cliente, contatto, opportunità, oggetto, note,
  righe con prodotto, descrizione, quantità, UM, prezzo, sconto %, aliquota IVA, importo; totali calcolati dal server.
- Stati `Bozza`, `Inviata`, `Accettata`, `Rifiutata` (scelti a mano nella pagina).
- Listino: `Crm_Par_Prodotto` (codice, descrizione, prezzo, UM, IVA) nelle Tabelle del CRM; scegliendo un prodotto
  la riga si compila.
- PDF con MigraDoc (`CrmOffertaPdf`) **scaricato direttamente** (`GET Crm_OffertaPdf`): non rispetta la regola del
  progetto "export solo tramite job".
- Invio per email con il PDF allegato (`IEmailService`): segna l'offerta come inviata ma **non lascia traccia** tra
  le attività del cliente.
- Nessun legame automatico con la pipeline, nessuna scadenza nello scadenzario, nessuno stato "scaduta".

## 2. Modello dati (migration incrementale `Crm_PreventiviAssistenza` del contesto Crm, comune alle due aree)

| Modifica | Dettaglio |
|---|---|
| `Crm_StatoOfferta.Scaduta = 4` | nuovo stato: validità passata senza esito (i valori 0–3 non cambiano) |
| `Crm_Offerta.DataEsito` (`DateTime?`) | data in cui l'offerta è stata accettata, rifiutata o è scaduta; si svuota tornando a bozza/inviata |
| `Crm_Offerta.IdOrdineAmministrazione` (`int?`) | **predisposizione** dell'ordine dell'Amministrazione (applicativo 8, più avanti): ID senza FK, come `IdCommessaPresenze` delle opportunità (regola §2.6 di `ANALISI_CRM_APPLICATIVO.md`). Nessuna logica oggi: si legge e si mostra soltanto |
| `Crm_Par_Prodotto.TipoProdotto` (`Crm_TipoProdotto`: `Articolo = 0`, `Servizio = 1`) | listino "articoli/servizi": distingue le voci nel listino e nelle scelte della riga |

Nessuna nuova tabella: il listino resta `Crm_Par_Prodotto` (un solo listino per azienda, prezzo unico, *scelta
provvisoria*: niente listini multipli per cliente o per periodo).

## 3. Ciclo di vita

```
Bozza ──(Invia per email / stato "Inviata")──► Inviata ──► Accettata ─► (opportunità vinta)
  ▲                                              │  ├──────► Rifiutata
  └──────────────(torna in bozza)────────────────┘  └─(validità passata)─► Scaduta
```

- **Numerazione**: invariata (per anno, assegnata al primo salvataggio).
- **Righe e totali**: invariati e calcolati solo dal server. Importo riga = `round(q × prezzo × (1 − sconto/100), 2)`;
  IVA = somma per riga di `round(importo × aliquota/100, 2)`; totale = imponibile + IVA (arrotondamento "away from zero").
- **Scaduta**: un'offerta *Inviata* con `ValidaFinoAl` < oggi passa a *Scaduta*. Lo fa il servizio in background del
  CRM (`CrmReminderService`, un controllo all'ora per ogni azienda con il CRM attivo) e, per l'azienda corrente,
  anche l'apertura dell'elenco o della scheda delle offerte (così lo stato mostrato è sempre aggiornato).
  Un'offerta scaduta si può riportare a *Inviata* (cambiando la validità) o chiudere con l'esito.
- **Esito** (Accettata / Rifiutata / Scaduta): imposta `DataEsito`.

### 3.1 Scadenzario (`IDeadlineRegistry`)

- Fonte **`crm.preventivo`**, applicativo `Crm`, riferimento = Id dell'offerta, data = `ValidaFinoAl`,
  descrizione "Offerta N/AAAA – Cliente", link `/crmofferedit` con `{ id }`.
- Si registra quando l'offerta è **Inviata** con una validità (*scelta provvisoria*: una bozza non è ancora nelle mani
  del cliente, quindi non ha una scadenza da seguire). Si aggiorna a ogni salvataggio (cambio di validità = scadenza
  riaperta, come da regola dello scadenzario).
- Preavviso: **7 giorni** (*scelta provvisoria*, fisso; in futuro eventualmente in `Crm_Cfg`).
- Responsabile: responsabile dell'opportunità collegata, altrimenti responsabile del cliente, altrimenti nessuno
  (avvisati gli amministratori dell'azienda, regola dello scadenzario).
- **Chiusa** all'esito (Accettata, Rifiutata, Scaduta); **tolta** se l'offerta torna in bozza o viene cancellata.

## 4. PDF e invio

- **PDF solo tramite job** (regola del progetto): `POST api/Crm_Offerta/Crm_OffertaPdfJob { id }` controlla i permessi,
  prepara i dati e i testi nella lingua della richiesta e restituisce subito il `JobId`; il job (`RunInBackground`)
  scrive il PDF con MigraDoc (`CrmOffertaPdf`), lo registra con `NVXSystem.SalvaFileJob` e lo notifica con il link
  di download (`ILongJobNotifier`, barra dei job del client), come gli export dei pagamenti e i manuali.
  Il vecchio `GET Crm_OffertaPdf` viene tolto.
- **Invio per email** (`POST Crm_OffertaInvia { id, email, messaggio }`): destinatario predefinito = email del contatto
  dell'offerta (altrimenti del cliente); il PDF allegato si genera in memoria (è un allegato, non un export scaricato
  dall'utente: *scelta provvisoria* di tenere l'invio sincrono, così l'esito – e l'eventuale errore SMTP – arriva subito
  all'utente). Se l'email non parte: messaggio chiaro, nessun cambio di stato, nessuna attività.
- **Registrazione dell'invio come attività del CRM**: attività *Fatta* del tipo con icona `mail` (il tipo "Email"
  predefinito; se l'azienda l'ha tolto, il primo tipo attivo), oggetto "Invio offerta N/AAAA", esito con il
  destinatario, assegnatario = chi invia, collegata a cliente, contatto e opportunità dell'offerta. Compare
  nell'agenda (fatte) e nelle schede di cliente e opportunità.
- Dopo l'invio una bozza diventa *Inviata* (`DataInvio` aggiornata a ogni invio) e si registra la scadenza.

## 5. Collegamento con la pipeline

Coerente con `ANALISI_CRM_APPLICATIVO.md` (§11.2: "Opportunità → Offerta → Vinta / Persa"):

- **Offerta accettata → opportunità vinta**: se l'offerta è collegata a un'opportunità ancora in una fase *aperta*, questa
  passa alla prima fase di tipo *Vinta* (per ordine), con probabilità 100, data di chiusura oggi e **valore =
  imponibile dell'offerta accettata**; il passaggio finisce nello storico delle fasi (`Crm_OpportunitaStorico`), base dei
  report. Se l'opportunità è già chiusa (vinta o persa) non si tocca.
- **Offerta rifiutata o scaduta**: l'opportunità **non** cambia (*scelta provvisoria*: una trattativa può avere più
  offerte, la perdita si decide dalla pipeline con il motivo).
- Se l'azienda non ha nessuna fase *Vinta* attiva, l'offerta si accetta comunque e l'opportunità resta com'è.

## 6. Ordine (Amministrazione, più avanti)

Solo predisposizione: campo `IdOrdineAmministrazione` (vuoto). Quando esisterà l'Amministrazione, un'interfaccia in
Infrastructure (es. `IOrderCreator`, regola §2.6) creerà l'ordine da un'offerta *Accettata* e ne salverà l'ID qui.
Nessun nuovo stato oggi: "ordinata" si dedurrà da `IdOrdineAmministrazione` valorizzato.

## 7. Ruoli

Invariati rispetto alle offerte (`ANALISI_CRM_APPLICATIVO.md` §4): CompanyPowerAdmin e CompanyAdmin vedono e gestiscono
tutte le offerte; il commerciale (User) quelle dei clienti che vede (regola di visibilità). Il listino si modifica dalle
Tabelle del CRM (solo CompanyPowerAdmin). PDF e invio: chi vede l'offerta.

## 8. Client

- Pagina offerta: stato *Scaduta* tra gli stati, data esito, pulsante **PDF** che avvia il job (avviso "PDF in
  preparazione: lo trovi nella barra dei job"), invio invariato (destinatario proposto dal contatto).
- Elenco offerte: azione PDF con il job.
- Tabelle del CRM → Prodotti: tipo (Articolo/Servizio); nella riga dell'offerta il prodotto mostra il tipo.
- Lingue it/en.

## 9. Decisioni (scelte provvisorie da confermare)

| # | Tema | Scelta provvisoria |
|---|---|---|
| P1 | Nome | "Offerta" nelle pagine (esistente); preventivo = offerta |
| P2 | Listino | uno per azienda (`Crm_Par_Prodotto`) con tipo Articolo/Servizio, niente listini per cliente |
| P3 | Scadenza nello scadenzario | solo per le offerte *Inviate*, preavviso 7 giorni |
| P4 | Invio email | sincrono (PDF allegato generato in memoria), errori mostrati subito |
| P5 | Esito negativo | rifiutata/scaduta non cambia l'opportunità |
| P6 | Valore dell'opportunità vinta | imponibile dell'offerta accettata |
| P7 | Ordine | solo il campo `IdOrdineAmministrazione` |

## 10. Checklist

Stato (11/11/2026): **fatto su `CRM_Dev_f4`, da mergiare in CRM_Dev**.

- [x] `CRM-PREV.1` Modello: stato `Scaduta`, `DataEsito`, `IdOrdineAmministrazione`, tipo del prodotto; migration `Crm_PreventiviAssistenza`
- [x] `CRM-PREV.1` Scadenza automatica (servizio in background + apertura di elenco/scheda) e scadenzario `crm.preventivo` (registrata, chiusa all'esito, tolta in bozza/cancellazione) – `CrmOfferteCiclo`
- [x] `CRM-PREV.2` PDF tramite job (`Crm_OffertaPdfJob`), tolto il download diretto
- [x] `CRM-PREV.2` Invio email al contatto con PDF allegato, errore gestito, invio registrato come attività
- [x] `CRM-PREV.3` Offerta accettata → opportunità vinta (fase, valore, storico)
- [x] Client: stati, PDF con job, tipo del prodotto, i18n it/en
- [x] Manuale del CRM aggiornato (it/en)
- [x] Build server (`dotnet build`) e client (`ng build --configuration development`)
- [x] Prova su DB usa e getta (`nvx_test_crmf4`, porta 5334) l'11/11/2026: **76 controlli, tutti ok** (script `test_crmf4.py`): azienda, CRM attivato dal SuperUser, utenti PowerAdmin/Admin/User, listino con tipo, cliente e contatto, opportunità; preventivo con 4 righe e totali controllati a mano (720,00 + 99,99 + 190,00 + 20,17 = imponibile 1030,16; IVA 158,40 + 22,00 + 0 + 2,02 = 182,42; totale 1212,58), numerazione; job del PDF completato con il file registrato e scaricabile solo da chi l'ha lanciato; invio con SMTP spento (errore chiaro, resta in bozza, nessuna attività) e con SMTP finto (email al contatto con il PDF allegato, attività registrata, scadenza aperta con il responsabile dell'opportunità); accettata → scadenza chiusa e opportunità vinta (fase, probabilità, chiusura, valore, storico); inviata con validità passata → scaduta e scadenza chiusa; bozza → scadenza tolta; permessi del commerciale; cliente usato non cancellabile.
- [ ] Prova nel browser (pagine offerta e listino)
- [ ] Il passaggio automatico a "Scaduta" del servizio in background (una volta all'ora) non è stato provato a tempo: la stessa funzione è provata tramite l'apertura dell'elenco
- [ ] 🔍 Decisioni P1–P7 da confermare con l'utente
