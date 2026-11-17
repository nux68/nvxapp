import { ModelResult } from "../../../ModelsBase/model-result";

export class ChatAIInModel {
  public request:   string = '';
  public sessionId: string = '';  // identifica la sessione conversazionale sul server
  public currentRoute: string = '';  // pagina in cui si trova l'utente (help contestuale)
  // voci del menu che l'utente vede, gia' tradotte: "vai a tabelle" trova la pagina anche dal titolo della voce
  public menuPages: MenuPageModel[] = [];
}

// Voce del menu dell'utente per la navigazione della chat
export class MenuPageModel {
  public title:   string = '';  // titolo della voce nella lingua dell'utente ("Tabelle")
  public route:   string = '';  // url della pagina ("/crmparlist")
  public section: string = '';  // sezione del menu (applicativo, es. "CRM"), per distinguere voci con lo stesso titolo
}

// Azione eseguita dal client senza navigare (es. cambio tema o lingua)
export class ClientActionPayload {
  public action: string = '';  // "theme" | "language"
  public value:  string = '';  // theme: "dark" | "light" | "toggle"; language: "it" | "en"
}

// Payload di navigazione restituito quando responseType === "navigate"
// o come campo navigate su un ActionResultItem
export class NavigatePayload {
  public route:   string                     = '';
  public params:  Record<string, string>     = {};
  // Stato passato al router Angular (es. { id: 42 } per aprire un record specifico)
  public state:   Record<string, any>        = {};
}

// Risultato di una singola azione nel piano multi-azione
export class ActionResultItem {
  public intent:   string           = '';
  public success:  boolean          = false;
  public message:  string           = '';
  public navigate: NavigatePayload | null = null;
  public clientAction: ClientActionPayload | null = null;
}

export class ChatAIOutModel extends ModelResult {
  public responce:      string            = '';
  public sessionId:     string            = '';
  // "question" | "confirmation" | "result" | "error" | "navigate" | "partial_error"
  public responseType:  string            = '';
  public suggestions:   string[]          = [];
  // Payload di navigazione per l'azione Navigate (ultima del piano)
  public navigate:      NavigatePayload | null = null;
  // Risultati per singola azione del piano
  public actionResults: ActionResultItem[] = [];
  // Solo con responseType === "confirmation": true se il piano contiene solo navigazioni
  // (il vocale puo' confermare da solo); con scritture di dati conferma l'utente
  public autoConfirm:   boolean           = false;
  // Testo breve da pronunciare (risposta vocale); vuoto = usare responce
  public speechText:    string            = '';
  // Azione da eseguire nel client (es. cambio tema o lingua)
  public clientAction:  ClientActionPayload | null = null;
}
