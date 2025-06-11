import { CheckObjOn_Id_Number, CheckObjOn_Id_Text } from "../../../ModelsBase/check-obj";
import { ModelResult } from "../../../ModelsBase/model-result";


export class Az_SediRepartoModel {

  public id!: number;
  public idAz_Sedi!: number; 
  public descrizione!: string;
  public idAz_SediReparto?: number; 
  public default: boolean;
}


export class Az_SediReparto_GetAll_InModel {
  
}
export class Az_SediReparto_GetAll_OutModel extends ModelResult {

  public az_SediReparto: Az_SediRepartoModel[];

}


export class Az_SediReparto_Get4User_InModel {
  idAspNetUsers!: string;
}
export class Az_SediReparto_Get4User_OutModel extends ModelResult {

  public az_SediReparto: Az_SediRepartoModel[];

}


export class Az_SediReparto_Get4Admin_InModel {
  idAspNetUsers!: string;
}
export class Az_SediReparto_Get4Admin_OutModel extends ModelResult {

  public az_SediReparto: Az_SediRepartoModel[];

}


export class Az_SediReparto_Get4AdminApproval_InModel {
  idAspNetUsers!: string;
}
export class Az_SediReparto_Get4AdminApproval_OutModel extends ModelResult {

  public az_SediReparto: Az_SediRepartoModel[];

}


export class Az_SediRepartoGetInModel {
  public id: number;
}
//export class Az_SediRepartoGetOutModel extends ModelResult {
//  public az_SediReparto: Az_SediRepartoModel;
//  public selectedAdmin: CheckObjOn_Id_Text_4ApprovalZorder[];
//  public selectedUser: CheckObjOn_Id_Text_4ApprovalZorder[];
//  public az_SediRepartoAttivita: CheckObjOn_Id_Number[];

//}

export class Az_SediRepartoGetOutModel extends ModelResult {
  public az_SediReparto: Az_SediRepartoModel;
  public selectedAdmin: CheckObjOn_Id_Text_4ApprovalZorder[];
  public selectedUser: CheckObjOn_Id_Text_4ApprovalZorder[];
  public az_SediRepartoAttivita: CheckObjOn_Id_Number[];

  constructor() {
    super();
    this.az_SediReparto = new Az_SediRepartoModel();
    this.selectedAdmin = [];
    this.selectedUser = [];
    this.az_SediRepartoAttivita = [];
  }
}


export class CheckObjOn_Id_Text_4ApprovalZorder extends CheckObjOn_Id_Text {
  public enabledToApproval!: boolean;
  public approvalZOrder!: number;
}

export class Az_SediRepartoPutInModel {
  public az_SediReparto: Az_SediRepartoModel;
  public selectedAdmin: CheckObjOn_Id_Text_4ApprovalZorder[];
  public selectedUser: CheckObjOn_Id_Text_4ApprovalZorder[];
  public az_SediRepartoAttivita: CheckObjOn_Id_Number[];
}
export class Az_SediRepartoPutOutModel extends ModelResult {
  public az_SediReparto: Az_SediRepartoModel;
  public selectedAdmin: CheckObjOn_Id_Text_4ApprovalZorder[];
  public selectedUser: CheckObjOn_Id_Text_4ApprovalZorder[];
  public az_SediRepartoAttivita: CheckObjOn_Id_Number[];
}

export class Az_SediRepartoDeleteInModel {
  public id: number;
  constructor() {
    this.id = 0;
  }
}

export class Az_SediRepartoDeleteOutModel extends ModelResult {
  public az_SediReparto: Az_SediRepartoModel;
}

