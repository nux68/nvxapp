import { ModelResult } from "../../../ModelsBase/model-result";
import { StatoRichiesta } from "../../Dip_GG_Richiesta/Models/dip-gg-richiesta-model";



export class Dip_GG_ResultModel {

  public id: number;
  public idDip_RapportoLavoro!: number;
  public data!: Date;
  public hH_Teo: string | null;
  public hH_Lav: string | null;
  public stato: GG_ResultStato;

}
export enum GG_ResultStato {
  Init,
  Err,
  OK,
  Locked
}



export class Dip_GG_Result_GetAll_InModel {
  public idAspNetUsers?: string
  public year: number;
  public month: number;
}
export class Dip_GG_Result_GetAll_OutModel extends ModelResult {
  public dip_GG_Result: Dip_GG_ResultModel[];
}





export class Dip_GG_Result_Get_4Calculation_InModel {
  public usersId: string[] = [];
  public dal!: string; // ISO 8601 locale, es. "2026-03-01T00:00:00"
  public al!: string;  // ISO 8601 locale, es. "2026-03-31T23:59:59"
}
export class Dip_GG_Result_Get_4Calculation_OutModel extends ModelResult {
  public dip_GG_Result: Dip_GG_ResultModel[] = [];
}













