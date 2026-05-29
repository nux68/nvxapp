import { ModelResult } from "../../../ModelsBase/model-result";

export class ChatAIInModel {
  public request:   string = '';
  public sessionId: string = '';  // identifica la sessione conversazionale sul server
}

// Payload di navigazione restituito quando responseType === "navigate"
// o come campo navigate su un ActionResultItem
export class NavigatePayload {
  public route:   string                     = '';
  public params:  Record<string, string>     = {};
}

// Risultato di una singola azione nel piano multi-azione
export class ActionResultItem {
  public intent:   string           = '';
  public success:  boolean          = false;
  public message:  string           = '';
  public navigate: NavigatePayload | null = null;
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
}
