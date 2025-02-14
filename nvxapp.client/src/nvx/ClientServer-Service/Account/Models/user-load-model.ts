import { ModelResult } from "../../ModelsBase/model-result";
import { RolesModel } from "./user-roles-model";


export class UserLoadInModel {
  public id: string | null;
}

export class UserLoadOutModel extends ModelResult {

  public userData: UserDataModel | null;

}

export class UserDataModel  {

  public id: string | null;

  public userName: string | null;

  public roles: RolesModel[] = [];

}

