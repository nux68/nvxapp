import { ModelResult } from "../../../ModelsBase/model-result";
import { RolesModel } from "./user-roles-model";


export class UserLoadInModel {
  public id: string | null;
}

export class UserLoadOutModel extends ModelResult {

  public userData: UserDataModel | null;
  public token: string | null;
}

export class UserDataModel  {

  public id: string | null;

  public userName: string | null;

  public roles: RolesModel[] = [];

  // applicativi attivi per l'azienda dell'utente (valori di ApplicationType)
  public activeApplications: ApplicationType[] = [];
  // applicativi abilitati per lo studio dell'utente (funzioni lato studio)
  public financialAdvisorApplications: ApplicationType[] = [];

  // voci di menu nascoste ai ruoli dell'azienda (configurate dal CompanyPowerAdmin)
  public menuRestrictions: MenuRestrictionModel[] = [];

}

// voce di menu (url) nascosta a un ruolo dell'azienda
export class MenuRestrictionModel {
  public url: string = '';
  public role: string = '';
}

// Applicativi (stessi valori dell'enum ApplicationType del server).
// Common (0) non e' un applicativo da attivare: sono i dati comuni dell'azienda (clienti...), sempre presenti.
export enum ApplicationType {
  Common = 0,
  Moke = 1,
  AttendanceTracking = 2,
  Crm = 3,
  DocumentManagement = 4,
}
