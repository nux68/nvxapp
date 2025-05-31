import { ModelResult } from "../../../ModelsBase/model-result";

export class Az_AttivitaModel {
  id: number = 0;
  idAz_Anagrafica: number = 0;
  descrizione: string = '';
}

export class Az_Attivita_GetAll_InModel {}

export class Az_Attivita_GetAll_OutModel extends ModelResult {
  public az_Attivita: Az_AttivitaModel[] = [];
}

export class Az_AttivitaGetInModel { id: number = 0; }
export class Az_AttivitaGetOutModel { az_Attivita: Az_AttivitaModel = new Az_AttivitaModel(); }
export class Az_AttivitaPutInModel { az_Attivita: Az_AttivitaModel = new Az_AttivitaModel(); }
export class Az_AttivitaPutOutModel { az_Attivita: Az_AttivitaModel = new Az_AttivitaModel(); }
