import { Dip_AnagraficaModel } from "../../../GestionePresenze/Dip_Anagrafica/Models/dip-anagrafica-model";
import { Dip_ProfiloOrarioModel } from "../../../GestionePresenze/Dip_ProfiloOrario/Models/dip-profilo-orario-model";
import { Dip_RapportoLavoroModel } from "../../../GestionePresenze/Dip_RapportoLavoro/Models/dip-rapporto-lavoro-model";
import { ModelResult } from "../../../ModelsBase/model-result";
import { RolesModel } from "./user-roles-model";


export class UserCompanyListModel {
  constructor(
    public descrizione: string | null = "",
    public idAspNetUsers: string = "",
    public idUserCompany: number = 0,
    public mainUser: boolean = false,
    
    public roles: string[] = []
  ) { }
}
export class UserCompanyListInModel {

  constructor(
    public filteredRoles: string[] = []
  ) { }

}
export class UserCompanyListOutModel extends ModelResult {

  public userCompanyList: UserCompanyListModel[] ;

}


 
  


export class UserCompanyEditModel {
  constructor(
    public descrizione: string | null = "",
    public idUserCompany: number = 0,
    public idAspNetUsers: string = "",
    public mainUser: boolean = false,

    public mail: string | null = null,
    public pw: string | null = null,
    
    public roles: string[] = []

    
  ) { }
}

export class UserCompanyGetInModel {
  public id: number;
}
export class UserCompanyGetOutModel extends ModelResult {

  public userCompanyEdit: UserCompanyEditModel;

}
export class UserCompanyPutInModel  {

  public userCompanyEdit: UserCompanyEditModel;

}
export class UserCompanyPutOutModel extends ModelResult {

  public userCompanyEdit: UserCompanyEditModel;

}
