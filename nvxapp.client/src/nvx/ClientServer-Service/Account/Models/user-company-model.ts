import { ModelResult } from "../../ModelsBase/model-result";
import { RolesModel } from "./user-roles-model";


export class UserCompanyListModel {
  constructor(
    public descrizione: string | null = "",
    public idAspNetUsers: string = "",
    public idUserCompany: number = 0
  ) { }
}
export class UserCompanyListInModel {
  
}
export class UserCompanyListOutModel extends ModelResult {

  public userCompanyList: UserCompanyListModel[] ;

}



export class UserCompanyEditModel {
  constructor(
    public descrizione: string | null = "Descrizione non disponibile",
    public idUserCompany: number = 0
  ) { }
}

export class UserCompanyGetInModel {
  public id: number;
}
export class UserCompanyGetOutModel extends ModelResult {

  public userCompanyEdit: UserCompanyEditModel;

}
export class UserCompanyPutInModel  {

  public userCompanyEdit: UserCompanyEditModel;

}
export class UserCompanyPutOutModel extends ModelResult {

  public userCompanyEdit: UserCompanyEditModel;

}
