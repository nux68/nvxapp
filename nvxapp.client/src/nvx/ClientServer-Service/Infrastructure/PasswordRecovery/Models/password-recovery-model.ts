import { ModelResult } from "../../../ModelsBase/model-result";


// richiesta del link per reimpostare la password (pagina /forgot-password, prima del login):
// la risposta e' sempre la stessa, che l'utente esista o no
export class PasswordRecoveryRequestInModel {
  public userName: string | null = null;
}

export class PasswordRecoveryRequestOutModel extends ModelResult {
  public scadenzaOre: number;
}


// nuova password dal link ricevuto (pagina /reset-password)
export class PasswordRecoveryResetInModel {
  public userId: string | null = null;
  public token: string | null = null;
  public pw: string | null = null;
}

export class PasswordRecoveryResetOutModel extends ModelResult {
  public userName: string;
}


// cambio della password conoscendo quella attuale (pagina /change-password, dal login)
export class PasswordChangeInModel {
  public userName: string | null = null;
  public pw: string | null = null;
  public newPw: string | null = null;
}

export class PasswordChangeOutModel extends ModelResult {
  public userName: string;
}
