import { ModelResult } from "../../../ModelsBase/model-result";
import { StatoRichiesta } from "../../Dip_GG_Richiesta/Models/dip-gg-richiesta-model";



export class Dip_GG_ResultModel {

  public id: number;
  public idDip_RapportoLavoro!: number;
  public data!: string;
  public hH_Teo: string | null;
  public hH_Lav: string | null;
  public stato: GG_ResultStato;

}
export enum GG_ResultStato {
  Init = 1 << 0,   // 1
  OK = 1 << 1,   // 2
  Locked = 1 << 2,   // 4
  Warning = 1 << 3,   // 8
  Err = 1 << 4,   // 16

  // Error details (5 → 24)
  Err_1 = 1 << 5,
  Err_2 = 1 << 6,
  Err_3 = 1 << 7,
  Err_4 = 1 << 8,
  Err_5 = 1 << 9,
  Err_6 = 1 << 10,
  Err_7 = 1 << 11,
  Err_8 = 1 << 12,
  Err_9 = 1 << 13,
  Err_10 = 1 << 14,
  Err_11 = 1 << 15,
  Err_12 = 1 << 16,
  Err_13 = 1 << 17,
  Err_14 = 1 << 18,
  Err_15 = 1 << 19,
  Err_16 = 1 << 20,
  Err_17 = 1 << 21,
  Err_18 = 1 << 22,
  Err_19 = 1 << 23,
  Err_20 = 1 << 24,

  // Warning details (25 → 44)
  Warning_1 = 1 << 25,
  Warning_2 = 1 << 26,
  Warning_3 = 1 << 27,
  Warning_4 = 1 << 28,
  Warning_5 = 1 << 29,
  Warning_6 = 1 << 30,
  Warning_7 = 1 << 31,

  // Oltre 31 serve Number, non bitwise (che tronca a 32 bit)
  Warning_8 = 1 * 2 ** 32,
  Warning_9 = 1 * 2 ** 33,
  Warning_10 = 1 * 2 ** 34,
  Warning_11 = 1 * 2 ** 35,
  Warning_12 = 1 * 2 ** 36,
  Warning_13 = 1 * 2 ** 37,
  Warning_14 = 1 * 2 ** 38,
  Warning_15 = 1 * 2 ** 39,
  Warning_16 = 1 * 2 ** 40,
  Warning_17 = 1 * 2 ** 41,
  Warning_18 = 1 * 2 ** 42,
  Warning_19 = 1 * 2 ** 43,
  Warning_20 = 1 * 2 ** 44,

  STATE_MASK = Init | OK | Locked | Warning | Err
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













