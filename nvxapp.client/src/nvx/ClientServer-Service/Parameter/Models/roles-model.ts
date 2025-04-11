import { RoleCode } from "../../Account/Models/user-roles-model";
import { ModelResult } from "../../ModelsBase/model-result";



export class RolesModel {
  constructor(
    public id: string = "",
    public code: RoleCode = RoleCode.User,
    public name: string = "",
    public normalizedName: string = "",
    
  ) { }
}
export class RolesListInModel {
  
}
export class RolesListOutModel extends ModelResult {

  public roles: RolesModel[] ;

}



