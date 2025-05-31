import { ModelResult } from "../../../ModelsBase/model-result";

export class Az_AttivitaCompetenzaModel {
  public id!: number;
  public idAz_Attivita!: number;
  public descrizione: string;
}

export class Az_AttivitaCompetenza_GetAll_InModel {}

export class Az_AttivitaCompetenza_GetAll_OutModel extends ModelResult {
  public az_AttivitaCompetenza: Az_AttivitaCompetenzaModel[] = [];
}
