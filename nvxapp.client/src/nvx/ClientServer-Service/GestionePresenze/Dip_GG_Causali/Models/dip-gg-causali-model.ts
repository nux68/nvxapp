import { ModelResult } from "../../../ModelsBase/model-result";


export class Dip_GG_CausaliModel {

  public id: number;
  public idDip_RapportoLavoro: number;
  public data!: string;
  public idPar_Causali: number;
  public valore: string | null;

}


export class Dip_GG_Causali_GetAll_InModel {
  
}
export class Dip_GG_Causali_GetAll_OutModel extends ModelResult {

  public dip_GG_Causali: Dip_GG_CausaliModel[];

}


export class Dip_GG_Causali_Get_4Calculation_InModel {
  public usersId: string[] = [];
  public dal!: string; // ISO 8601 locale, es. "2026-03-01T00:00:00"
  public al!: string;  // ISO 8601 locale, es. "2026-03-31T23:59:59"
}
export class Dip_GG_Causali_Get_4Calculation_OutModel extends ModelResult {
  public dip_GG_Causali: Dip_GG_CausaliModel[];
}


export class Dip_GG_Causali_DeleteInModel {
  public id: number;
}
export class Dip_GG_Causali_DeleteOutModel extends ModelResult {
  
}

export class Dip_GG_CausaliPutInModel {
  public dip_GG_Causali: Dip_GG_CausaliModel;
}
export class Dip_GG_CausaliPutOutModel extends ModelResult {
  public dip_GG_Causali: Dip_GG_CausaliModel;
}
