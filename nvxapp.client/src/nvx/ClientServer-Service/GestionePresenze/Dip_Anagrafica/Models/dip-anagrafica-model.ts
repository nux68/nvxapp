import { RoleCode } from "../../../Infrastructure/Account/Models/user-roles-model";
import { ModelResult } from "../../../ModelsBase/model-result";
import { Dip_ProfiloOrarioModel } from "../../Dip_ProfiloOrario/Models/dip-profilo-orario-model";
import { Dip_RapportoLavoroModel } from "../../Dip_RapportoLavoro/Models/dip-rapporto-lavoro-model";


export class Dip_AnagraficaModel {

  public id!: number;
  public idAspNetUsers: string;
  public userName: string;
  
  public cognome!: string;
  public nome!: string;

  public dip_RapportoLavoro: Dip_RapportoLavoroModel[];

  public roleCode: RoleCode[];

  constructor() {
    this.idAspNetUsers = '';
    this.userName = '';
    this.dip_RapportoLavoro = [];
    this.roleCode = [];
  }

}

export class Dip_Anagrafica_GetAll_InModel {
  
}

export class Dip_Anagrafica_GetAll_OutModel extends ModelResult {

 
  public dip_Anagrafica: Dip_AnagraficaModel[];

}



export class Dip_Anagrafica4EditModel extends Dip_AnagraficaModel {
  
  public roles: string[] = [];
  public idUserCompany: number = 0;
  public descrizione: string;
  public roleId: string;

  public mainUser: boolean;
  public mail: string;
  public pw: string;

  public dip_ProfiloOrario: Dip_ProfiloOrarioModel[];


  constructor() {
    super();
    this.descrizione = '';
    this.roleId = '';

    this.mail = '';
    this.pw = '';
    this.mainUser = false;

    this.dip_ProfiloOrario = [];
  }

}

export class Dip_Anagrafica_Get_InModel {
  public id: string;
}
export class Dip_Anagrafica_Get_OutModel {
  public dip_Anagrafica: Dip_Anagrafica4EditModel;
}
export class Dip_Anagrafica_Put_InModel {
  public id: string;
  public dip_Anagrafica: Dip_Anagrafica4EditModel;
}
export class Dip_Anagrafica_Put_OutModel {
  public id: string;
  public dip_Anagrafica: Dip_Anagrafica4EditModel;
}
