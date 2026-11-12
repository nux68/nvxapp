/*
 Ciclo unico delle richieste (ANALISI_INFRASTRUCTURE_RICHIESTE.md; server: RichiestaRegole), uguale per tutti i tipi
 (presenze, note spese, prenotazioni, ...):
    1. richiesta -> il richiedente la ritira                    -> Cancellata                          (fine)
    2. richiesta -> l'approvatore la rifiuta                      -> Rifiutata                           (fine)
    3. approvata -> annullamento chiesto -> approvato            -> Annullata (annullamento approvato)  (fine)
    4. approvata -> annullamento chiesto -> respinto             -> Approvata (annullamento respinto)   (fine)
    5. approvata -> annullamento chiesto -> ritirato             -> Approvata (annullamento ritirato)   (fine)
 L'annullamento si chiede una volta sola. Testi in infrastructure/i18n "common.requestState" e "common.requestAction".
*/

/** stessi valori di StatoRichiesta (Dip_GG_Richiesta, server) */
export enum StatoRichiestaComune {
  Diretta = 0,
  Immessa = 1,
  Cancellata = 2,
  Rifiutata = 3,
  ApprovazioneInCorso = 4,
  ParzialmenteApprovata = 5,
  Approvata = 6,
}

export interface StatoRichiestaUi {
  /** chiave di traduzione del testo dello stato */
  chiave: string;
  /** colore ionic del badge */
  colore: string;
  icona: string;
}

const IN_ATTESA = [StatoRichiestaComune.Immessa, StatoRichiestaComune.ApprovazioneInCorso, StatoRichiestaComune.ParzialmenteApprovata];

type Stato = StatoRichiestaComune | number | null | undefined;

export function richiestaInAttesa(stato: Stato): boolean {
  return stato != null && IN_ATTESA.includes(stato);
}

/** annullamento chiesto e ancora da decidere: il richiedente lo puo' ritirare (la pratica si chiude, resta approvata) */
export function annullamentoInAttesa(revoca: Stato): boolean {
  return richiestaInAttesa(revoca);
}

/** il richiedente puo' chiedere l'annullamento: approvata e annullamento mai chiesto */
export function puoChiedereAnnullamento(richiesta: Stato, revoca: Stato): boolean {
  return richiesta === StatoRichiestaComune.Approvata && revoca == null;
}

/** il richiedente puo' agire: ritirare la richiesta in attesa, chiedere l'annullamento o ritirare quello in attesa */
export function puoAnnullareRichiesta(richiesta: Stato, revoca: Stato): boolean {
  return (revoca == null && richiestaInAttesa(richiesta)) || puoChiedereAnnullamento(richiesta, revoca) || annullamentoInAttesa(revoca);
}

/** testo, colore e icona dello stato: la fase dell'annullamento prevale */
export function statoRichiestaUi(richiesta: Stato, revoca: Stato): StatoRichiestaUi {
  const k = (s: string) => 'common.requestState.' + s;
  if (revoca != null) {
    switch (revoca) {
      case StatoRichiestaComune.Approvata: return { chiave: k('revoked'), colore: 'dark', icona: 'trash-outline' };
      case StatoRichiestaComune.Rifiutata: return { chiave: k('revokeRejected'), colore: 'success', icona: 'checkmark-circle-outline' };
      case StatoRichiestaComune.Cancellata: return { chiave: k('revokeWithdrawn'), colore: 'success', icona: 'checkmark-circle-outline' };
      default: return { chiave: k('revokePending'), colore: 'warning', icona: 'time-outline' };
    }
  }
  switch (richiesta) {
    case StatoRichiestaComune.Diretta: return { chiave: k('direct'), colore: 'success', icona: 'checkmark-circle' };
    case StatoRichiestaComune.Immessa: return { chiave: k('pending'), colore: 'medium', icona: 'time-outline' };
    case StatoRichiestaComune.ApprovazioneInCorso:
    case StatoRichiestaComune.ParzialmenteApprovata: return { chiave: k('inProgress'), colore: 'medium', icona: 'hourglass-outline' };
    case StatoRichiestaComune.Rifiutata: return { chiave: k('rejected'), colore: 'danger', icona: 'close-circle-outline' };
    case StatoRichiestaComune.Cancellata: return { chiave: k('cancelled'), colore: 'dark', icona: 'trash-outline' };
    case StatoRichiestaComune.Approvata: return { chiave: k('approved'), colore: 'success', icona: 'checkmark-circle-outline' };
    default: return { chiave: '', colore: 'medium', icona: 'help-circle-outline' };
  }
}
