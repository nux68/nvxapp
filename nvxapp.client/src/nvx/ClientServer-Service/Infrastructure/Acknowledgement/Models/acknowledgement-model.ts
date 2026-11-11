import { ModelResult } from "../../../ModelsBase/model-result";


// prese visione dell'utente corrente (ANALISI_INFRASTRUCTURE_NUOVI_MODULI.md, INF-SVC.6)
export class AcknowledgementModel {
  public id = 0;
  public idCompany = 0;
  public azienda: string | null = null;
  public application = 0;
  public titolo = '';
  public testo: string | null = null;
  /** pagina del client da aprire e stato (JSON) da passarle */
  public url: string | null = null;
  public urlState: string | null = null;
  public canale = '';
  /** si conferma in questa pagina (canale base); gli altri canali si confermano aprendo url (es. firma del documento) */
  public confermabileQui = false;
  public dataRichiesta: string | null = null;
  public dataConferma: string | null = null;
}


export class AcknowledgementListInModel {
  /** true = solo quelle da confermare */
  public daConfermare = true;
}

export class AcknowledgementListOutModel extends ModelResult {
  public acknowledgement: AcknowledgementModel[] = [];
  public daConfermare = 0;
}


export class AcknowledgementConfirmInModel {
  public id = 0;
}

export class AcknowledgementConfirmOutModel extends ModelResult {
  public acknowledgement: AcknowledgementModel = new AcknowledgementModel();
  public daConfermare = 0;
}
