import { ModelResult } from "../../../ModelsBase/model-result";


export class Az_SediRepartoUserModel {
  public id!: number;

  public idAspNetUsers: string;
  public idAz_Reparto!: number;

  public enabledToApproval: number;
  public approvalZOrder!: number;

  public dataDal?: Date;
  public dataAl?: Date;

}

export class Az_SediRepartoUser_GetAll_InModel {

  public idAz_Reparto!: number;

}

export class Az_SediRepartoUser_GetAll_OutModel extends ModelResult {

  public az_RepartoUser: Az_SediRepartoUserModel[];

}


export class Az_SediRepartoUser_GetAll_Period_InModel {

  public idAz_Reparto!: number;
  public dataDal?: Date;
  public dataAl?: Date;

}
