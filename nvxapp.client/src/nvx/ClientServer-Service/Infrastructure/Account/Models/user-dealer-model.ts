import { ModelResult } from "../../../ModelsBase/model-result";
import { RolesModel } from "./user-roles-model";


export class UserDealerListModel {
  constructor(
    public descrizione: string | null = "",
    public idAspNetUsers: string = "",
    public idUserDealer: number = 0,
    public mainUser: boolean = false,
    public roleId: string = "",
  ) { }
}
export class UserDealerListInModel {
  
}
export class UserDealerListOutModel extends ModelResult {

  public userDealerList: UserDealerListModel[] ;

}



export class UserDealerEditModel {
  constructor(
    public descrizione: string | null = "",
    public idUserDealer: number = 0,
    public mainUser: boolean = false,

    public mail: string | null = null,
    public pw: string | null = null,
    public roleId: string = "",
  ) { }
}

export class UserDealerGetInModel {
  public id: number;
}
export class UserDealerGetOutModel extends ModelResult {

  public userDealerEdit: UserDealerEditModel;

}
export class UserDealerPutInModel  {

  public userDealerEdit: UserDealerEditModel;

}
export class UserDealerPutOutModel extends ModelResult {

  public userDealerEdit: UserDealerEditModel;

}
