import { ModelResult } from "../../../ModelsBase/model-result";

export class ChatAIInModel {
  public request:   string = '';
  public sessionId: string = '';  // identifica la sessione conversazionale sul server
}

export class ChatAIOutModel extends ModelResult {
  public responce:     string   = '';
  public sessionId:    string   = '';    // restituito dal server, da riusare al turno successivo
  public responseType: string   = '';    // "question" | "confirmation" | "result" | "error"
  public suggestions:  string[] = [];    // chip/bottoni opzionali suggeriti dal server
}
