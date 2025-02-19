import { ModelResult } from "../../ModelsBase/model-result";
import { RolesModel } from "./user-roles-model";


export class FinancialAdvisorListInModel {
  
}

export class FinancialAdvisorListOutModel extends ModelResult {

  public financialAdvisorList: FinancialAdvisorListModel[] ;

}

export class FinancialAdvisorListModel  {

  public idFinancialAdvisor: number;

  public descrizione: string | null;

  public idAspNetUsers: string ;

}

