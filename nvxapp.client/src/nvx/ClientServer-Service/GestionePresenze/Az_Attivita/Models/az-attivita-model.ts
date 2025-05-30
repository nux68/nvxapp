import { ModelResult } from "../../../ModelsBase/model-result";

export class Az_AttivitaModel {
  public id!: number;
  public idAz_Anagrafica!: number;
  public descrizione: string;
}

export class Az_Attivita_GetAll_InModel {}

export class Az_Attivita_GetAll_OutModel extends ModelResult {
  public az_Attivita: Az_AttivitaModel[] = [];
}
