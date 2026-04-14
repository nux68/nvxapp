import { ModelResult } from "../../../ModelsBase/model-result";
import { StatoRichiesta } from "../../Dip_GG_Richiesta/Models/dip-gg-richiesta-model";



export class Dip_GG_Giustificativi_GetAll_InModel {  
  public idAspNetUsers?:string
  public year: number;
  public month: number;
}

export class Dip_GG_Giustificativi_GetAll_OutModel extends ModelResult {

  public dip_GG_Giustificativi: Dip_GG_GiustificativiModel[];

}

export class Dip_GG_GiustificativiModel  {

  public id: number; // Required
  public idDip_RapportoLavoro: number; // Required
  public data: Date;
  public idJustificationType: number;
  public inputType: JustificationInputType;

  public hours?: string;
  public from?: string;

  public idPar_Giustificativi: number; // Required
  public richiestaStato: StatoRichiesta;
  public idDip_GG_Richiesta?: number; // Opzionale (può essere null)

}

export enum JustificationInputType {
  Manual,
  AllDay,
  IntegrateDay
}


export class Dip_GG_Giustificativi_Get_4Calculation_InModel {
  public usersId: string[] = [];
  public dal!: string;
  public al!: string;
}
export class Dip_GG_Giustificativi_Get_4Calculation_OutModel extends ModelResult {
  public dip_GG_Giustificativi: Dip_GG_GiustificativiModel[] = [];
}


export class Dip_GG_Giustificativi_DeleteInModel {
  public excludeRicalc: boolean;
  public id: number;
}
export class Dip_GG_Giustificativi_DeleteOutModel extends ModelResult {

}

export class Dip_GG_GiustificativiPutInModel {
  public excludeRicalc: boolean;
  public dip_GG_Giustificativi: Dip_GG_GiustificativiModel;
}
export class Dip_GG_GiustificativiPutOutModel extends ModelResult {
  public dip_GG_Giustificativi: Dip_GG_GiustificativiModel;
}

export class Dip_GG_GiustificativiGetInModel {
  public id: number;

  public data?: string;
  public idDip_RapportoLavoro!: number;
}
export class Dip_GG_GiustificativiGetOutModel extends ModelResult {
  public dip_GG_Giustificativi: Dip_GG_GiustificativiModel;
}
