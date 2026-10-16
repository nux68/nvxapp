import { ModelResult } from "../../../ModelsBase/model-result";

// Verifica, prima di un inserimento, se username o email sono già usati da un altro utente
export class UserCheckInModel {
  public userName: string | null = null;
  public mail: string | null = null;
  /** utente da escludere dal controllo (modifica di un utente esistente), vuoto per un nuovo utente */
  public idAspNetUsers: string | null = null;
}

export class UserCheckOutModel extends ModelResult {
  public userNameExists: boolean = false;
  public mailExists: boolean = false;
}
