import { ModelResult } from "../../../ModelsBase/model-result";


export class Az_SediRepartoModel {

  public id!: number;
  public idAz_Sedi!: number; 
  public descrizione!: string;

  public idAz_SediReparto?: number; 

}


export class Az_SediRepartoInModel {
  
}
export class Az_SediRepartoOutModel extends ModelResult {

  public az_SediReparto: Az_SediRepartoModel[];

}

export class Az_SediRepartoGetInModel {
  public id: number;
}
export class Az_SediRepartoGetOutModel extends ModelResult {
  public az_SediReparto: Az_SediRepartoModel;
}
export class Az_SediRepartoPutInModel {
  public az_SediReparto: Az_SediRepartoModel;
}
export class Az_SediRepartoPutOutModel extends ModelResult {
  public az_SediReparto: Az_SediRepartoModel;
}

