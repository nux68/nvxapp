import { ModelResult } from "../../ModelsBase/model-result";
import { RolesModel } from "./user-roles-model";


export class CompanyListInModel {
  
}

export class CompanyListOutModel extends ModelResult {

  public companyList: CompanyListModel[] ;

}

export class CompanyListModel  {

  public idCompany: number;

  public descrizione: string | null;

  public idAspNetUsers: string ;

}

