import { ApplicationType } from './user-load-model';
import { ModelResult } from "../../../ModelsBase/model-result";
import { RolesModel } from "./user-roles-model";


export class FinancialAdvisorListModel {
  /** collegamento disabilitato: la riga si vede solo con "Mostra anche i disabilitati" */
  public disabled: boolean = false;

  constructor(
    public idFinancialAdvisor: number = 0,
    public descrizione: string | null = "",
    public idAspNetUsers: string = "",
    public mainUser: boolean = false
  ) { }
}
export class FinancialAdvisorListInModel {
  
}
export class FinancialAdvisorListOutModel extends ModelResult {

  public financialAdvisorList: FinancialAdvisorListModel[] ;

}


// applicativo abilitato per lo studio (funzioni lato studio; FinancialAdvisorApplicationModel del server)
export class FinancialAdvisorApplicationModel {
  public idFinancialAdvisor: number = 0;
  public applicationType: ApplicationType = ApplicationType.Moke;
  public descrizione: string = '';
  public active: boolean = false;
  public activationDate: string | null = null;
  public deactivationDate: string | null = null;
}

export class FinancialAdvisorEditModel {
  /** collegamento disabilitato: la riga si vede solo con "Mostra anche i disabilitati" */
  public disabled: boolean = false;
  /** applicativi abilitati dello studio */
  public applications: FinancialAdvisorApplicationModel[] = [];
  /** applicativi visibili ma non modificabili (li cambiano solo il SuperUser e il DealerPowerAdmin del centro) */
  public applicationsReadOnly: boolean = true;

  constructor(
    public idFinancialAdvisor: number = 0,
    public descrizione: string | null = "",
    public idAspNetUsers: string = "",
    public mainUser: boolean = false,
    public mail: string | null = null,
    public pw: string | null = null

  ) { }
}

export class FinancialAdvisorGetInModel {
  public id: number;
}
export class FinancialAdvisorGetOutModel extends ModelResult {

  public financialAdvisorEdit: FinancialAdvisorEditModel;

}
export class FinancialAdvisorPutInModel  {

  public financialAdvisorEdit: FinancialAdvisorEditModel;

}
export class FinancialAdvisorPutOutModel extends ModelResult {

  public financialAdvisorEdit: FinancialAdvisorEditModel;

}



