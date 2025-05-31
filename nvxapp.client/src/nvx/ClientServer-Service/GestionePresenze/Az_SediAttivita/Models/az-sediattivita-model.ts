import { ModelResult } from "../../../ModelsBase/model-result";

export class Az_SediAttivitaModel {
  public id!: number;
  public idAz_Sedi!: number;
  public descrizione: string;
}

export class Az_SediAttivita_GetAll_InModel {}

export class Az_SediAttivita_GetAll_OutModel extends ModelResult {
  public az_SediAttivita: Az_SediAttivitaModel[] = [];
}
