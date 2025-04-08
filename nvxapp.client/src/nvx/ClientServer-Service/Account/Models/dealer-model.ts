import { ModelResult } from "../../ModelsBase/model-result";
import { RolesModel } from "./user-roles-model";


export class DealerListModel {

  public idDealer: number;

  public descrizione: string | null;

  public idAspNetUsers: string;

  public mainUser: boolean;

}
export class DealerListInModel {
  
}
export class DealerListOutModel extends ModelResult {

  public dealerList: DealerListModel[] ;

}




export class DealerEditModel {

  public idDealer: number;

  public descrizione: string | null;

  public idAspNetUsers: string;

  public mainUser: boolean;

}
export class DealerGetInModel {
  public id: number;
}
export class DealerGetOutModel extends ModelResult {

  public dealerEdit: DealerEditModel;

}
export class DealerPutInModel  {

  public dealerEdit: DealerEditModel;

}
export class DealerPutOutModel extends ModelResult {

  public dealerEdit: DealerEditModel;

}



