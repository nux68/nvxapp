import { ModelResult } from "../../../ModelsBase/model-result";

export class Az_CompetenzaModel {
  public id!: number;
  public idAz_Anagrafica!: number;
  public descrizione: string;
}

export class Az_Competenza_GetAll_InModel {}

export class Az_Competenza_GetAll_OutModel extends ModelResult {
  public az_Competenza: Az_CompetenzaModel[] = [];
}


export class Az_CompetenzaGetInModel {
  public id: number = 0;
}

export class Az_CompetenzaGetOutModel extends ModelResult {
  public az_Competenza: Az_CompetenzaModel = new Az_CompetenzaModel();
}


export class Az_CompetenzaPutInModel extends ModelResult {
  public az_Competenza: Az_CompetenzaModel = new Az_CompetenzaModel();
}

export class Az_CompetenzaPutOutModel extends ModelResult {
  public az_Competenza: Az_CompetenzaModel = new Az_CompetenzaModel();
}
