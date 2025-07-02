import { ModelResult } from "../../../ModelsBase/model-result";
import { CheckObjOn_Id_Number, CheckObjOn_Id_Text } from "../../../ModelsBase/check-obj";
import { Az_SubCommessaUser4EditModel } from "../../Az_SubCommessaUser/Models/az-subcommessa-user-model";
import { Az_SubCommessaAttivita4EditModel } from "../../Az_SubCommessaAttivita/Models/az-subcommessa-attivita-model";

export class Az_SubCommessaModel {
  public id: number = 0;
  public idAz_Commessa: number = 0;
  public descrizione?: string;
  public default: boolean = false;
}

export class Az_SubCommessa_GetAll_InModel {}

export class Az_SubCommessa_GetAll_OutModel extends ModelResult {
  public az_SubCommessa: Az_SubCommessaModel[] = [];
}

// Modello per l'edit (incorpora tutti i dati che compongono la sub commessa)
export class Az_SubCommessa_4EditModel extends Az_SubCommessaModel {
  public az_SubCommessaUser: Az_SubCommessaUser4EditModel[] = [];
  public az_SubCommessaAttivita: Az_SubCommessaAttivita4EditModel[] = [];
  public az_SubCommessaSediReparto: CheckObjOn_Id_Number[] = [];
}

export class Az_SubCommessa_GetAll_4Edit_InModel {
  public id: number = 0; // id della subcommessa
}

export class Az_SubCommessa_GetAll_4Edit_OutModel extends ModelResult {
  public az_SubCommessa: Az_SubCommessa_4EditModel[] = [];
}


