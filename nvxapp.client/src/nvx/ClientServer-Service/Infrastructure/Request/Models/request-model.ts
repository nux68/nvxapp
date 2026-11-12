import { ModelResult } from "../../../ModelsBase/model-result";


// richieste da approvare comuni a tutti gli applicativi (ANALISI_INFRASTRUCTURE_RICHIESTE.md)

/** DaApprovare: per l'amministratore quelle che puo' approvare adesso, per il richiedente quelle in attesa */
export enum RequestFilter {
  DaApprovare = 0,
  Tutte = 1,
  Approvate = 2,
}

/** stato comune (colori e filtri); il testo preciso arriva dal modulo in statoTesto */
export enum RequestState {
  InAttesa = 0,
  InApprovazione = 1,
  Approvata = 2,
  Rifiutata = 3,
  Annullata = 4,
}

export class RequestItemModel {
  /** "<prefisso>:<id>" */
  public key = '';
  public id = '';
  /** valore di ApplicationType */
  public application = 0;
  public tipo = '';
  public tipoTesto = '';
  public richiedente: string | null = null;
  public dal = '';
  public al: string | null = null;
  /** dal/al sono solo date */
  public soloData = false;
  public dettaglio: string | null = null;
  public stato: RequestState = RequestState.InAttesa;
  public statoTesto = '';
  public canApprove = false;
  public canReject = false;
  public canCancel = false;
  public url: string | null = null;
  public urlState: any = null;
  /** azione del client fornita dal ramo del modulo (iRequestUiProvider) */
  public extra: string | null = null;
  public dataRichiesta: string | null = null;
}

export class RequestCountInModel {
}

export class RequestCountOutModel extends ModelResult {
  public daApprovare = 0;
  public inAttesa = 0;
}

export class RequestListInModel {
  /** true = "Richieste da approvare" (amministratori dell'azienda) */
  public admin = false;
  public filtro: RequestFilter = RequestFilter.DaApprovare;
}

export class RequestListOutModel extends ModelResult {
  public richieste: RequestItemModel[] = [];
  public applicativi: number[] = [];
  public daApprovare = 0;
  public inAttesa = 0;
}

export class RequestActionInModel {
  public key = '';
  public motivo: string | null = null;
}

export class RequestActionOutModel extends ModelResult {
  public daApprovare = 0;
  public inAttesa = 0;
}

/** payload dell'evento SignalR "RequestNotifier" */
export interface RequestNotifierData {
  key: string | null;
}
