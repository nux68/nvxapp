import { ModelResult } from "../../ModelsBase/model-result";
import { RolesModel } from "./user-roles-model";


export class CompanyListModel {

  public idCompany: number;

  public descrizione: string | null;

  public idAspNetUsers: string;

  public mainUser: boolean;

}
export class CompanyListInModel {
  
}
export class CompanyListOutModel extends ModelResult {

  public companyList: CompanyListModel[] ;

}



export class CompanyEditModel {

  public idCompany: number;

  public descrizione: string | null;

  public idAspNetUsers: string;

  public mainUser: boolean;

}
export class CompanyGetInModel {
  public id: number;
}
export class CompanyGetOutModel extends ModelResult {

  public CompanyEdit: CompanyEditModel;

}
export class CompanyPutInModel  {

  public CompanyEdit: CompanyEditModel;

}
export class CompanyPutOutModel extends ModelResult {

  public CompanyEdit: CompanyEditModel;

}




