import { ModelResult } from "../../../ModelsBase/model-result";

export class Az_SubCommessaModel {
  public id!: number;
  public idAz_Commessa!: number;
  public descrizione: string;
}

export class Az_SubCommessa_GetAll_InModel {}

export class Az_SubCommessa_GetAll_OutModel extends ModelResult {
  public az_SubCommessa: Az_SubCommessaModel[] = [];
}
