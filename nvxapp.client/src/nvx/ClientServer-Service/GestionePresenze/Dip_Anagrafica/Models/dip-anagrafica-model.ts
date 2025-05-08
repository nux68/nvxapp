import { ModelResult } from "../../../ModelsBase/model-result";
import { Dip_RapportoLavoroModel } from "../../Dip_RapportoLavoro/Models/dip-rapporto-lavoro-model";


export class Dip_AnagraficaModel {

  public id!: number;
  public idAspNetUsers: string;
  public userName: string;
  
  public cognome!: string;
  public nome!: string;

  public dip_RapportoLavoro: Dip_RapportoLavoroModel[];

  public roles: string[];

}

export class Dip_Anagrafica_GetAll_InModel {
  
}

export class Dip_Anagrafica_GetAll_OutModel extends ModelResult {

 
  public dip_Anagrafica: Dip_AnagraficaModel[];

}



