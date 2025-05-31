import { ModelResult } from "../../../ModelsBase/model-result";

export class Az_CommessaModel {
  public id!: number;
  public idAz_Anagrafica!: number;
  public descrizione: string;
  public idAz_Cliente?: number; // Relazione con cliente
}

export class Az_Commessa_GetAll_InModel {}

export class Az_Commessa_GetAll_OutModel extends ModelResult {
  public az_Commessa: Az_CommessaModel[] = [];
}
