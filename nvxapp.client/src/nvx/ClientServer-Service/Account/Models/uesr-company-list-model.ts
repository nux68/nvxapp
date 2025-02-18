import { ModelResult } from "../../ModelsBase/model-result";
import { RolesModel } from "./user-roles-model";


export class UserCompanyListInModel {
  
}

export class UserCompanyListOutModel extends ModelResult {

  public userCompanyList: UserCompanyListModel[] ;

}

export class UserCompanyListModel  {


  public descrizione: string | null;

  public idAspNetUsers: string ;

}

