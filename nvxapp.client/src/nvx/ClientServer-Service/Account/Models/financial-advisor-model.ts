import { ModelResult } from "../../ModelsBase/model-result";
import { RolesModel } from "./user-roles-model";


export class FinancialAdvisorListModel {

  public idFinancialAdvisor: number;

  public descrizione: string | null;

  public idAspNetUsers: string;

  public mainUser: boolean;

}
export class FinancialAdvisorListInModel {
  
}
export class FinancialAdvisorListOutModel extends ModelResult {

  public financialAdvisorList: FinancialAdvisorListModel[] ;

}


export class FinancialAdvisorEditModel {

  public idFinancialAdvisor: number;

  public descrizione: string | null;

  public idAspNetUsers: string;

  public mainUser: boolean;

}
export class FinancialAdvisorGetInModel {
  public id: number;
}
export class FinancialAdvisorGetOutModel extends ModelResult {

  public FinancialAdvisorEdit: FinancialAdvisorEditModel;

}
export class FinancialAdvisorPutInModel  {

  public FinancialAdvisorEdit: FinancialAdvisorEditModel;

}
export class FinancialAdvisorPutOutModel extends ModelResult {

  public FinancialAdvisorEdit: FinancialAdvisorEditModel;

}



