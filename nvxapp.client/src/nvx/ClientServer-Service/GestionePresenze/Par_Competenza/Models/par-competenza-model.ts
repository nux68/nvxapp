import { ModelResult } from "../../../ModelsBase/model-result";

export class Par_CompetenzaModel {
  public id!: number;
  public idAz_Anagrafica!: number;
  public descrizione: string;
  public default: boolean;
}

export class Par_Competenza_GetAll_InModel {}

export class Par_Competenza_GetAll_OutModel extends ModelResult {
  public par_Competenza: Par_CompetenzaModel[] = [];
}


export class Par_CompetenzaGetInModel {
  public id: number = 0;
}

export class Par_CompetenzaGetOutModel extends ModelResult {
  public par_Competenza: Par_CompetenzaModel = new Par_CompetenzaModel();
}


export class Par_CompetenzaPutInModel extends ModelResult {
  public par_Competenza: Par_CompetenzaModel = new Par_CompetenzaModel();
}

export class Par_CompetenzaPutOutModel extends ModelResult {
  public par_Competenza: Par_CompetenzaModel = new Par_CompetenzaModel();
}


export class Par_CompetenzaDeleteInModel {
  public id: number = 0;
}

export class Par_CompetenzaDeleteOutModel extends ModelResult {
  public par_Competenza: Par_CompetenzaModel = new Par_CompetenzaModel();
}
