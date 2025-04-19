import { ModelResult } from "../../../ModelsBase/model-result";
import { RolesModel } from "./user-roles-model";


export class UserListModel {
  constructor(
    public descrizione: string | null = "",
    public idAspNetUsers: string = "",
    public roleId: string = "",
  ) { }
}
export class UserListInModel {
  
}
export class UserListOutModel extends ModelResult {

  public userList: UserListModel[] ;

}



export class UserEditModel {
  constructor(
    public idAspNetUsers: string = "",
    public descrizione: string | null = "",
    public mail: string | null = null,
    public pw: string | null = null,
    public roleId: string = "",
  ) { }
}
export class UserGetInModel {
  public id: string;
}
export class UserGetOutModel extends ModelResult {

  public userEdit: UserEditModel;

}
export class UserPutInModel  {

  public userEdit: UserEditModel;

}
export class UserPutOutModel extends ModelResult {

  public userEdit: UserEditModel;

}
