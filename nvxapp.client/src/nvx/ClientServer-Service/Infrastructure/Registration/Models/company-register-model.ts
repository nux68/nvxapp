import { ModelResult } from "../../../ModelsBase/model-result";


// registrazione di una nuova azienda dallo showcase (prima del login): il server salva la richiesta
// e invia all'email indicata il link di conferma; l'azienda nasce (nel ramo DEMO) solo alla conferma
export class CompanyRegisterInModel {
  public ragioneSociale: string | null = null;
  public mail: string | null = null;
  public pw: string | null = null;
  public accettaCondizioni: boolean = false;
  // nomi scelti per gli amministratori (vuoti = ragione sociale + "_PowerAdmin" / "_Admin")
  public powerAdminUserName: string | null = null;
  public adminUserName: string | null = null;
}

export class CompanyRegisterOutModel extends ModelResult {
  // indirizzo a cui e' stato inviato il link e sua validita' in ore
  public mail: string;
  public scadenzaOre: number;
}


// verifica, mentre si compila la pagina, dei nomi degli amministratori che si creeranno
export class CompanyRegisterUserNamesCheckInModel {
  public ragioneSociale: string | null = null;
  public powerAdminUserName: string | null = null;
  public adminUserName: string | null = null;
}

// errore per ciascun nome (vuoto = valido e libero)
export class CompanyRegisterUserNamesCheckOutModel extends ModelResult {
  public powerAdminUserNameError: string;
  public adminUserNameError: string;
}


// conferma dal link ricevuto (pagina /confirm-email): crea l'azienda con i suoi utenti
export class CompanyRegisterConfirmInModel {
  public token: string | null = null;
}

// utente creato per la nuova azienda, da usare per il primo accesso
export class CompanyRegisterUserModel {
  public userName: string;
  public roles: string[] = [];
}

export class CompanyRegisterConfirmOutModel extends ModelResult {
  public idCompany: number;
  public descrizione: string;
  public users: CompanyRegisterUserModel[] = [];
}
