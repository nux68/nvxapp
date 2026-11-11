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

  // licenze dell'azienda (o dello studio) dell'utente (ANALISI_INFRASTRUCTURE_LICENZE.md §8.1.1, §8.1.2)
  public license: UserLicenseModel | null = null;

}

// stato delle licenze per menu e pulsanti; il server ricontrolla sempre
export class UserLicenseModel {
  // false = sistema licenze non attivo: tutto come Pro, utenti illimitati
  public enabled: boolean = false;
  public function: UserLicenseFunctionModel[] = [];
  public activeUsers: number = 0;
  // limite utenti del soggetto; null = illimitati
  public userLimit: number | null = null;
  public canAddUser: boolean = true;
  // limite clienti dell'azienda (licenza CRM); null = illimitati o CRM non coperto
  public clientLimit: number | null = null;
  public canAddClient: boolean = true;
}

export class UserLicenseFunctionModel {
  public application!: ApplicationType;
  // null = nessuna licenza valida
  public livello: LivelloLicenza | null = null;
  public stato: StatoLicenza = StatoLicenza.NotSubscribed;
  public maxUtenti: number | null = null;
  public maxClienti: number | null = null;
  public maxSpazioMB: number | null = null;
  public dataScadenza: string | null = null;
}

// livelli di licenza (stessi valori dell'enum LivelloLicenza del server)
export enum LivelloLicenza {
  Free = 1,
  Standard = 2,
  Pro = 3,
}

export enum StatoLicenza {
  NotSubscribed = 0,
  Licensed = 1,
  Grace = 2,
  Expired = 3,
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
