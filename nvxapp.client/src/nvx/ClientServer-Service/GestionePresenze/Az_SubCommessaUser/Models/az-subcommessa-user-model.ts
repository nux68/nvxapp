import { ModelResult } from "../../../ModelsBase/model-result";
import { CheckObjOn_Id_Number, CheckObjOn_Id_Text, ICheckObj } from "../../../ModelsBase/check-obj";



export class Az_SubCommessaUserModel {
  id: number = 0;
  idAz_SubCommessa: number = 0;
  idAspNetUsers: string = '';
}

export class Az_SubCommessaUser4EditModel extends Az_SubCommessaUserModel implements ICheckObj<number> {
  checked: boolean = false;
}
