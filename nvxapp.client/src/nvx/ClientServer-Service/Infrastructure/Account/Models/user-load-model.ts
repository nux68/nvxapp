import { ModelResult } from "../../../ModelsBase/model-result";
import { RolesModel } from "./user-roles-model";


export class UserLoadInModel {
  public id: string | null;
}

export class UserLoadOutModel extends ModelResult {

  public userData: UserDataModel | null;
  public token: string | null;
}

export class UserDataModel  {

  public id: string | null;

  public userName: string | null;

  public roles: RolesModel[] = [];

  // applicativi attivi per l'azienda dell'utente (valori di ApplicationType)
  public activeApplications: ApplicationType[] = [];

}

// Applicativi (stessi valori dell'enum ApplicationType del server)
export enum ApplicationType {
  Moke = 1,
  AttendanceTracking = 2,
}
