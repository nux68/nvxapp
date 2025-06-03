import { ModelResult } from "../../../ModelsBase/model-result";

export class Par_AttivitaModel {
  id: number = 0;
  idAz_Anagrafica: number = 0;
  descrizione: string = '';
  public backgroundColor!: string;
  public textColor!: string;
  public default: boolean;
}

export class Par_Attivita_GetAll_InModel {}

export class Par_Attivita_GetAll_OutModel extends ModelResult {
  public par_Attivita: Par_AttivitaModel[] = [];
}

export class Par_AttivitaGetInModel { id: number = 0; }
export class Par_AttivitaGetOutModel { par_Attivita: Par_AttivitaModel = new Par_AttivitaModel(); }
export class Par_AttivitaPutInModel { par_Attivita: Par_AttivitaModel = new Par_AttivitaModel(); }
export class Par_AttivitaPutOutModel { par_Attivita: Par_AttivitaModel = new Par_AttivitaModel(); }
