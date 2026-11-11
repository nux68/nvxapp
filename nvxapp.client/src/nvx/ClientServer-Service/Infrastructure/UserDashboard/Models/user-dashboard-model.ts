import { ModelResult } from "../../../ModelsBase/model-result";


// dashboard dell'utente corrente nell'azienda corrente (ANALISI_INFRASTRUCTURE_WIDGET.md)
export class UserDashboardGetInModel {
}

export class UserDashboardGetOutModel extends ModelResult {
  /** false = mai configurata: valgono i widget predefiniti */
  public configurata = false;
  /** chiavi dei widget nell'ordine di visualizzazione */
  public widgets: string[] = [];
  /** l'utente puo' configurare la propria dashboard (User, CompanyAdmin, CompanyPowerAdmin in un'azienda) */
  public configurabile = false;
}


// sostituisce l'elenco dei widget dell'utente corrente
export class UserDashboardPutInModel {
  public widgets: string[] = [];
}

export class UserDashboardPutOutModel extends ModelResult {
  public configurata = false;
  public widgets: string[] = [];
  public configurabile = false;
}
