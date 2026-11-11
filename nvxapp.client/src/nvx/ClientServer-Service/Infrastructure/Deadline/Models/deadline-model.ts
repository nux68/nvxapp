import { ModelResult } from "../../../ModelsBase/model-result";


// scadenzario dell'azienda corrente (ANALISI_INFRASTRUCTURE_NUOVI_MODULI.md, INF-SVC.1-3)
export enum DeadlineState {
  Aperta = 0,
  Chiusa = 1
}

export class DeadlineModel {
  public id = 0;
  public application = 0;
  public fonte = '';
  public idRiferimento: number | null = null;
  public descrizione = '';
  public note: string | null = null;
  public dataScadenza = '';
  public giorniPreavviso = 30;
  public idAspNetUsersResponsabile: string | null = null;
  public responsabile: string | null = null;
  /** pagina del client da aprire e stato (JSON) da passarle */
  public url: string | null = null;
  public urlState: string | null = null;
  public stato: DeadlineState = DeadlineState.Aperta;
  public dataChiusura: string | null = null;
  /** inserita a mano (non da un modulo): si puo' modificare e cancellare */
  public manuale = false;
  /** aperta e con data passata / entro il preavviso */
  public scaduta = false;
  public inScadenza = false;
}

export class DeadlineUserModel {
  public id = '';
  public nome = '';
}


export class DeadlineListInModel {
  /** null = tutte, 0 = aperte, 1 = chiuse */
  public stato: DeadlineState | null = DeadlineState.Aperta;
  /** solo le scadenze entro n giorni da oggi (comprese quelle scadute); null = tutte */
  public entroGiorni: number | null = null;
  public testo: string | null = null;
  /** solo quelle di cui l'utente e' responsabile (per gli amministratori; gli utenti vedono sempre solo le proprie) */
  public soloMie = false;
}

export class DeadlineListOutModel extends ModelResult {
  public deadline: DeadlineModel[] = [];
  /** l'utente vede e gestisce tutte le scadenze dell'azienda */
  public amministratore = false;
}


export class DeadlineGetInModel {
  /** 0 = nuova scadenza manuale */
  public id = 0;
}

export class DeadlineGetOutModel extends ModelResult {
  public deadline: DeadlineModel = new DeadlineModel();
  /** utenti dell'azienda selezionabili come responsabili */
  public utenti: DeadlineUserModel[] = [];
}


// scadenza inserita a mano (id = 0 nuova)
export class DeadlinePutInModel {
  public id = 0;
  public descrizione = '';
  public note: string | null = null;
  public dataScadenza = '';
  public giorniPreavviso = 30;
  public idAspNetUsersResponsabile: string | null = null;
}

export class DeadlinePutOutModel extends ModelResult {
  public deadline: DeadlineModel = new DeadlineModel();
}


// chiude (chiudi = true) o riapre una scadenza
export class DeadlineCloseInModel {
  public id = 0;
  public chiudi = true;
}

export class DeadlineCloseOutModel extends ModelResult {
  public deadline: DeadlineModel = new DeadlineModel();
}


export class DeadlineDeleteInModel {
  public id = 0;
}

export class DeadlineDeleteOutModel extends ModelResult {
}


// widget "In scadenza"
export class DeadlineSummaryInModel {
  public giorni = 30;
  public massimo = 5;
}

export class DeadlineSummaryOutModel extends ModelResult {
  public scadute = 0;
  public inScadenza = 0;
  public prossime: DeadlineModel[] = [];
}
