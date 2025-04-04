import { ModelResult } from "../../ModelsBase/model-result";
import { RolesModel } from "./user-roles-model";


export class UserCompanyListModel {


  public descrizione: string | null;

  public idAspNetUsers: string;

}
export class UserCompanyListInModel {
  
}
export class UserCompanyListOutModel extends ModelResult {

  public userCompanyList: UserCompanyListModel[] ;

}



export class UserCompanyEditModel {


  public descrizione: string | null;

  public idAspNetUsers: string;



}
export class UserCompanyGetInModel {
  public id: string;
}
export class UserCompanyGetOutModel extends ModelResult {

  public UserCompanyEdit: UserCompanyEditModel;

}
export class UserCompanyPutInModel  {

  public UserCompanyEdit: UserCompanyEditModel;

}
export class UserCompanyPutOutModel extends ModelResult {

  public UserCompanyEdit: UserCompanyEditModel;

}
