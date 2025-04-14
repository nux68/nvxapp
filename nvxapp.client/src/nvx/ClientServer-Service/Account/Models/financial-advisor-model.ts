import { ModelResult } from "../../ModelsBase/model-result";
import { RolesModel } from "./user-roles-model";


export class FinancialAdvisorListModel {
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


export class FinancialAdvisorEditModel {
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



