import { ModelResult } from "../../../ModelsBase/model-result";

export class Par_AttivitaCompetenzaModel {
  public id!: number;
  public idPar_Attivita!: number;
  public descrizione: string;
}

export class Par_AttivitaCompetenza_GetAll_InModel {}

export class Par_AttivitaCompetenza_GetAll_OutModel extends ModelResult {
  public par_AttivitaCompetenza: Par_AttivitaCompetenzaModel[] = [];
}
