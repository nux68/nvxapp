import { GenericResult } from "../../../ModelsBase/generic-result";
import { ModelResult } from "../../../ModelsBase/model-result";

export class Az_ClienteModel {
  public id!: number;
  public idAz_Anagrafica!: number;
  public descrizione: string = '';
}

export class Az_Cliente_GetAll_InModel {}

export class Az_Cliente_GetAll_OutModel extends ModelResult {
  public az_Cliente: Az_ClienteModel[] = [];
}

export class Az_ClienteGetInModel {
  id: number = 0;
}
export class Az_ClienteGetOutModel extends GenericResult<Az_ClienteModel> {
  az_Cliente!: Az_ClienteModel;
}
export class Az_ClientePutInModel {
  az_Cliente!: Az_ClienteModel;
}
export class Az_ClientePutOutModel extends GenericResult<Az_ClienteModel> {
  az_Cliente!: Az_ClienteModel;
}
