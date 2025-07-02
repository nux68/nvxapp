import { ModelResult } from "../../../ModelsBase/model-result";
import { ICheckObj } from "../../../ModelsBase/check-obj";

export class Az_SubCommessaAttivitaModel {
  id: number = 0;
  idAz_SubCommessa: number = 0;
  idAz_SediAttivita: number = 0;
  default: boolean = false;
}

export class Az_SubCommessaAttivita4EditModel extends Az_SubCommessaAttivitaModel implements ICheckObj<number> {
  checked: boolean = false;
}

//export class Az_SubCommessaAttivita_GetAll_InModel {}

//export class Az_SubCommessaAttivita_GetAll_OutModel extends ModelResult {
//  az_SubCommessaAttivita: Az_SubCommessaAttivitaModel[] = [];
//}

//export class Az_SubCommessaAttivita_Get4SubCommessa_InModel {
//  idAz_SubCommessa: number = 0;
//}

//export class Az_SubCommessaAttivita_Get4SubCommessa_OutModel extends ModelResult {
//  az_SubCommessaAttivita: Az_SubCommessaAttivita4EditModel[] = [];
//}

//export class Az_SubCommessaAttivita_Put4SubCommessa_InModel {
//  idAz_SubCommessa: number = 0;
//  az_SubCommessaAttivita: Az_SubCommessaAttivita4EditModel[] = [];
//}

//export class Az_SubCommessaAttivita_Put4SubCommessa_OutModel extends ModelResult {
//  az_SubCommessaAttivita: Az_SubCommessaAttivita4EditModel[] = [];
//}
