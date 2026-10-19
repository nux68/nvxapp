import { ModelResult } from "../../../ModelsBase/model-result";


// registrazione di una nuova azienda dallo showcase (prima del login): nasce nel ramo DEMO
export class CompanyRegisterInModel {
  public ragioneSociale: string | null = null;
  public mail: string | null = null;
  public pw: string | null = null;
  public accettaCondizioni: boolean = false;
}

// utente creato per la nuova azienda, da usare per il primo accesso
export class CompanyRegisterUserModel {
  public userName: string;
  public roles: string[] = [];
}

export class CompanyRegisterOutModel extends ModelResult {
  public idCompany: number;
  public descrizione: string;
  public users: CompanyRegisterUserModel[] = [];
}
