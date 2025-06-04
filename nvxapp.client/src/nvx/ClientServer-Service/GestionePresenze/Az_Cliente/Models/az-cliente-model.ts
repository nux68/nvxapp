import { GenericResult } from "../../../ModelsBase/generic-result";
import { ModelResult } from "../../../ModelsBase/model-result";

export class Az_ClienteModel {
  public id!: number;
  public idAz_Anagrafica!: number;
  public descrizione: string = '';
  public default: boolean;
}

export class Az_Cliente_GetAll_InModel {}

export class Az_Cliente_GetAll_OutModel extends ModelResult {
  public az_Cliente: Az_ClienteModel[] = [];
}

export class Az_ClienteGetInModel {
  id: number = 0;
}
export class Az_ClienteGetOutModel extends ModelResult {
  az_Cliente!: Az_ClienteModel;
}

export class Az_ClientePutInModel {
  az_Cliente!: Az_ClienteModel;
}
export class Az_ClientePutOutModel extends ModelResult {
  az_Cliente!: Az_ClienteModel;
}

export class Az_ClienteDeleteInModel {
  public id: number = 0;
}
export class Az_ClienteDeleteOutModel extends ModelResult {
  public az_Cliente: Az_ClienteModel = new Az_ClienteModel();
}
