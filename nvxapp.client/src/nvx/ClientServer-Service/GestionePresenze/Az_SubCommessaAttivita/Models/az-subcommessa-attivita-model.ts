import { ModelResult } from "../../../ModelsBase/model-result";

export class Az_SubCommessaAttivitaModel {
  public id!: number;
  public idAz_SubCommessa!: number;
  public idAz_SediAttivita!: number;
  public default: boolean;
}

export class Az_SubCommessaAttivita_GetAll_InModel {}

export class Az_SubCommessaAttivita_GetAll_OutModel extends ModelResult {
  public az_SubCommessaAttivita: Az_SubCommessaAttivitaModel[] = [];
}
