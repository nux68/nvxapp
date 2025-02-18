import { ModelResult } from "../../ModelsBase/model-result";
import { RolesModel } from "./user-roles-model";


export class DealerListInModel {
  
}

export class DealerListOutModel extends ModelResult {

  public dealerList: DealerListModel[] ;

}

export class DealerListModel  {

  public idDealer: number;

  public descrizione: string | null;

  public idAspNetUsers: string ;

}

