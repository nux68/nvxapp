import { CheckObjOn_Id_Text } from "../../../ModelsBase/check-obj";
import { ModelResult } from "../../../ModelsBase/model-result";


export class Az_SediRepartoModel {

  public id!: number;
  public idAz_Sedi!: number; 
  public descrizione!: string;
  public idAz_SediReparto?: number; 

}


export class Az_SediReparto_GetAll_InModel {
  
}
export class Az_SediReparto_GetAll_OutModel extends ModelResult {

  public az_SediReparto: Az_SediRepartoModel[];

}


export class Az_SediReparto_Get4User_InModel {

}
export class Az_SediReparto_Get4User_OutModel extends ModelResult {

  public az_SediReparto: Az_SediRepartoModel[];

}



export class Az_SediRepartoGetInModel {
  public id: number;
}
export class Az_SediRepartoGetOutModel extends ModelResult {
  public az_SediReparto: Az_SediRepartoModel;
  public selectedAdmin: CheckObjOn_Id_Text_4ApprovalZorder[];
  public selectedUser: CheckObjOn_Id_Text_4ApprovalZorder[];
}

export class CheckObjOn_Id_Text_4ApprovalZorder extends CheckObjOn_Id_Text {
  public enabledToApproval!: boolean;
  public approvalZOrder!: number;
}

export class Az_SediRepartoPutInModel {
  public az_SediReparto: Az_SediRepartoModel;
  public selectedAdmin: CheckObjOn_Id_Text_4ApprovalZorder[];
  public selectedUser: CheckObjOn_Id_Text_4ApprovalZorder[];
}
export class Az_SediRepartoPutOutModel extends ModelResult {
  public az_SediReparto: Az_SediRepartoModel;
  public selectedAdmin: CheckObjOn_Id_Text_4ApprovalZorder[];
  public selectedUser: CheckObjOn_Id_Text_4ApprovalZorder[];
}

