import { ModelResult } from "../../ModelsBase/model-result";




export class LoginInModel {
  public userName: string | null;
  public password: string | null;
}

export class LoginOutModel extends ModelResult {

  public id: string | null;
  public token: string|null;
  //public refreshToken: string | null;

}


