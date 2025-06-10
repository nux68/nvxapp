import { CheckObjOn_Id_Number } from "../../../ModelsBase/check-obj";
import { ModelResult } from "../../../ModelsBase/model-result";

export class Az_SediModel {
  public id!: number;
  public idAz_Anagrafica!: string;
  public descrizione: string;
  public default: boolean;
}

export class Az_Sedi_GetAll_InModel {}

export class Az_Sedi_GetAll_OutModel extends ModelResult {
  public az_Sedi: Az_SediModel[];
}


export class Az_SediGetInModel {
  public id!: number;
}
export class Az_SediGetOutModel extends ModelResult {
  public az_Sedi!: Az_SediModel;
  public az_SediAttivita: CheckObjOn_Id_Number[];
}


export class Az_SediPutInModel {
  public az_Sedi!: Az_SediModel;
  public az_SediAttivita: CheckObjOn_Id_Number[];
}
export class Az_SediPutOutModel extends ModelResult {
  public az_Sedi!: Az_SediModel;
  public az_SediAttivita: CheckObjOn_Id_Number[];
}


export class Az_SediDeleteInModel {
  public id!: number;
}
export class Az_SediDeleteOutModel extends ModelResult {
  public az_Sedi!: Az_SediModel;
}



