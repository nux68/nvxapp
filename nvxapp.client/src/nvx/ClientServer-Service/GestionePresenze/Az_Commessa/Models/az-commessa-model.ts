import { CheckObjOn_Id_Number, CheckObjOn_Id_Text } from "../../../ModelsBase/check-obj";
import { ModelResult } from "../../../ModelsBase/model-result";
import { CheckObjOn_Id_Text_4ApprovalZorder } from "../../Az_SediReparto/Models/az-sedi-reparto-model";
import { Az_SubCommessa_4EditModel } from "../../Az_SubCommessa/Models/az-subcommessa-model";

export class Az_CommessaModel {
  public id!: number;
  public idAz_Anagrafica!: number;
  public descrizione: string;
  public idAz_Cliente!: number; 
  public default: boolean;
  public data!: string;
  public dataA!: string;
}

export class Az_Commessa_GetAll_InModel {}
export class Az_Commessa_GetAll_OutModel extends ModelResult {
  public az_Commessa: Az_CommessaModel[] = [];
}


export class Az_CommessaDeleteInModel {
  public id: number = 0;
}
export class Az_CommessaDeleteOutModel extends ModelResult {
  public az_Commessa!: Az_CommessaModel;
}




export class Az_CommessaGetInModel {
  public id: number = 0;
}
export class Az_CommessaGetOutModel extends ModelResult {
  public az_Commessa!: Az_CommessaModel;
  public az_SubCommessa: Az_SubCommessa_4EditModel[] = [];
  //public selected_User: CheckObjOn_Id_Text[];
  //public selected_Az_SediReparto: CheckObjOn_Id_Number[];
}

export class Az_CommessaPutInModel {
  public az_Commessa: Az_CommessaModel = new Az_CommessaModel();
  public az_SubCommessa: Az_SubCommessa_4EditModel[] = [];

  //public selected_User: CheckObjOn_Id_Text[];
  //public selected_Az_SediReparto: CheckObjOn_Id_Number[];
}
export class Az_CommessaPutOutModel extends ModelResult {
  az_Commessa!: Az_CommessaModel;
  public az_SubCommessa: Az_SubCommessa_4EditModel[] = [];
  //public selected_User: CheckObjOn_Id_Text[];
  //public selected_Az_SediReparto: CheckObjOn_Id_Number[];
}
