import { ModelResult } from "../../../ModelsBase/model-result";

export class Dip_CompetenzaModel {
  public id!: number;
  public idDip_Anagrafica!: number;
  public idAz_Competenza!: number;
}

export class Dip_Competenza_GetAll_InModel {}

export class Dip_Competenza_GetAll_OutModel extends ModelResult {
  public dip_Competenza: Dip_CompetenzaModel[] = [];
}
