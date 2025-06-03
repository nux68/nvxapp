import { ModelResult } from "../../../ModelsBase/model-result";



export class Az_SediModel {
  public id!: number;
  public idAz_Anagrafica!: string;
  public descrizione: string;
  public default: boolean;

}

export class Az_Sedi_GetAll_InModel {
  
}


export class Az_Sedi_GetAll_OutModel extends ModelResult {

  public az_Sedi: Az_SediModel[];

}



