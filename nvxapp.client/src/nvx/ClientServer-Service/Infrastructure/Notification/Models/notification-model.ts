import { ModelResult } from "../../../ModelsBase/model-result";


// notifiche dell'utente corrente (ANALISI_INFRASTRUCTURE_NUOVI_MODULI.md, INF-SVC.4)
export class NotificationModel {
  public id = 0;
  public categoria = '';
  public titolo = '';
  public messaggio: string | null = null;
  /** pagina del client da aprire e stato (JSON) da passarle */
  public url: string | null = null;
  public urlState: string | null = null;
  public letta = false;
  public data: string | null = null;
}


export class NotificationListInModel {
  public soloNonLette = false;
  /** pagina di 50 notifiche, dalla piu' recente (da 0) */
  public pagina = 0;
}

export class NotificationListOutModel extends ModelResult {
  public notification: NotificationModel[] = [];
  public nonLette = 0;
  public altreDisponibili = false;
}


export class NotificationCountInModel {
}

export class NotificationCountOutModel extends ModelResult {
  public nonLette = 0;
}


// segna come lette le notifiche indicate (tutte = tutte quelle dell'utente)
export class NotificationReadInModel {
  public id: number[] = [];
  public tutte = false;
}

export class NotificationReadOutModel extends ModelResult {
  public nonLette = 0;
}


// cancella le notifiche indicate (lette = tutte quelle gia' lette)
export class NotificationDeleteInModel {
  public id: number[] = [];
  public lette = false;
}

export class NotificationDeleteOutModel extends ModelResult {
  public nonLette = 0;
}


// preferenze dell'utente per categoria: nell'app e per email
export class NotificationPreferenceModel {
  public categoria = '';
  public titolo = '';
  public app = true;
  public email = false;
  /** la categoria si puo' ricevere per email */
  public emailAllowed = true;
}

export class NotificationPreferenceGetInModel {
}

export class NotificationPreferenceGetOutModel extends ModelResult {
  public preference: NotificationPreferenceModel[] = [];
  /** l'invio delle email e' configurato sul server */
  public emailConfigured = false;
}

export class NotificationPreferencePutInModel {
  public preference: NotificationPreferenceModel[] = [];
}

export class NotificationPreferencePutOutModel extends ModelResult {
  public preference: NotificationPreferenceModel[] = [];
  public emailConfigured = false;
}


// evento SignalR "NotificationNew": nuova notifica per l'utente collegato
export class NotificationNewData {
  public id = 0;
  public categoria = '';
}
