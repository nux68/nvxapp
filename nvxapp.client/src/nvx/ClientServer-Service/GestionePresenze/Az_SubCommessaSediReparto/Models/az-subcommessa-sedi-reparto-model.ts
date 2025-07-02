import { ModelResult } from "../../../ModelsBase/model-result";
import { CheckObjOn_Id_Number, CheckObjOn_Id_Text, ICheckObj } from "../../../ModelsBase/check-obj";



export class Az_SubCommessaSediRepartoModel {
  id: number = 0;
  idAz_SubCommessa: number = 0;
  idAz_SediReparto: number = 0;
}

export class Az_SubCommessaSediRepartoModel4EditModel extends Az_SubCommessaSediRepartoModel implements ICheckObj<number> {
  checked: boolean = false;
}
