import { ModelResult } from "../../../ModelsBase/model-result";


export class Az_CfgModel {

  public id!: number;
  public idAz_Anagrafica!: number;
  public approvazioneTipo: TipoApprovazione;

}

export enum TipoApprovazione {
  SigleAdmin,
  AllAdmin,
  AllAdminHierarchy,
}


export class Az_Cfg_GetAll_InModel {
  
}
export class Az_Cfg_GetAll_OutModel extends ModelResult {

  public az_Cfg: Az_CfgModel;

}



export class Az_Cfg_Get_InModel {
  //public id: number;
}
export class Az_Cfg_Get_OutModel extends ModelResult {

  public az_Cfg: Az_CfgModel;

}
export class Az_Cfg_Put_InModel {

  public az_Cfg: Az_CfgModel;

}
export class Az_Cfg_Put_OutModel extends ModelResult {

  public az_Cfg: Az_CfgModel;

}
