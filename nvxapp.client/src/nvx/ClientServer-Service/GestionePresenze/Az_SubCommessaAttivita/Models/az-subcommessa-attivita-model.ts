import { ModelResult } from "../../../ModelsBase/model-result";
import { ICheckObj } from "../../../ModelsBase/check-obj";

export class Az_SubCommessaAttivitaModel {
  id: number = 0;
  idAz_SubCommessa: number = 0;
  idPar_Attivita: number = 0;
  default: boolean = false;
}

export class Az_SubCommessaAttivita4EditModel extends Az_SubCommessaAttivitaModel implements ICheckObj<number> {
  checked: boolean = false;
}

export class Az_SubCommessaAttivita_GetAll_InModel {}

export class Az_SubCommessaAttivita_GetAll_OutModel extends ModelResult {
  az_SubCommessaAttivita: Az_SubCommessaAttivitaModel[] = [];
}

export class Az_SubCommessaAttivita_Get4SubCommessa_InModel {
  idAz_SubCommessa: number = 0;
}

export class Az_SubCommessaAttivita_Get4SubCommessa_OutModel extends ModelResult {
  az_SubCommessaAttivita: Az_SubCommessaAttivita4EditModel[] = [];
}

export class Az_SubCommessaAttivita_Put4SubCommessa_InModel {
  idAz_SubCommessa: number = 0;
  az_SubCommessaAttivita: Az_SubCommessaAttivita4EditModel[] = [];
}

export class Az_SubCommessaAttivita_Put4SubCommessa_OutModel extends ModelResult {
  az_SubCommessaAttivita: Az_SubCommessaAttivita4EditModel[] = [];
}

export class Az_SubCommessaAttivita_4FullListModel {
  commessa_Id: number = 0;
  commessa_IdAz_Cliente: number = 0;
  commessa_Decrizione: string = '';
  commessa_Default: boolean = false;

  subCommessa_Id: number = 0;
  subCommessa_Decrizione: string = '';
  subCommessa_Default: boolean = false;

  subCommessaAttivita_Id: number = 0;
  subCommessaAttivita_Decrizione: string = '';
  subCommessaAttivita_Default: boolean = false;
  subCommessaAttivita_IdPar_Attivita: number = 0;
}

export class Az_SubCommessaAttivita_GetAll_4FullList_InModel {}

export class Az_SubCommessaAttivita_GetAll_4FullList_OutModel extends ModelResult {
  az_SubCommessaAttivita: Az_SubCommessaAttivita_4FullListModel[] = [];
}
