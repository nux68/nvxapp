import { ModelResult } from "../../../ModelsBase/model-result";
import { RolesModel } from "./user-roles-model";


export class UserFinancialAdvisorListModel {
  constructor(
    public descrizione: string | null = "",
    public idAspNetUsers: string = "",
    public idUserFinancialAdvisor: number = 0,
    public mainUser: boolean = false,
    public roleId: string = "",
  ) { }
}
export class UserFinancialAdvisorListInModel {
  
}
export class UserFinancialAdvisorListOutModel extends ModelResult {

  public userFinancialAdvisorList: UserFinancialAdvisorListModel[] ;

}



export class UserFinancialAdvisorEditModel {
  constructor(
    public descrizione: string | null = "",
    public idUserFinancialAdvisor: number = 0,
    public mainUser: boolean = false,

    public mail: string | null = null,
    public pw: string | null = null,
    public roleId: string = "",
  ) { }
}

export class UserFinancialAdvisorGetInModel {
  public id: number;
}
export class UserFinancialAdvisorGetOutModel extends ModelResult {

  public userFinancialAdvisorEdit: UserFinancialAdvisorEditModel;

}
export class UserFinancialAdvisorPutInModel  {

  public userFinancialAdvisorEdit: UserFinancialAdvisorEditModel;

}
export class UserFinancialAdvisorPutOutModel extends ModelResult {

  public userFinancialAdvisorEdit: UserFinancialAdvisorEditModel;

}
