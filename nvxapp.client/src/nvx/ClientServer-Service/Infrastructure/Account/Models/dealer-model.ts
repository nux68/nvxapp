import { ModelResult } from "../../../ModelsBase/model-result";
import { RolesModel } from "./user-roles-model";


export class DealerListModel {

  constructor(
    public idDealer: number = 0,
    public descrizione: string | null = "",
    public idAspNetUsers: string = "",
    public mainUser: boolean = false,
  ) { }

}
export class DealerListInModel {
  
}
export class DealerListOutModel extends ModelResult {

  public dealerList: DealerListModel[] ;

}




export class DealerEditModel {

  constructor(
    public idDealer: number = 0,
    public descrizione: string | null = "",
    public idAspNetUsers: string = "",
    public mainUser: boolean = false,
    public mail: string | null = null,
    public pw: string | null = null
  ) { }

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



