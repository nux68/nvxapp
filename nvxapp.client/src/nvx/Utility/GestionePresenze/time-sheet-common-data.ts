import { Dip_GG_CausaliModel } from "../../ClientServer-Service/GestionePresenze/Dip_GG_Causali/Models/dip-gg-causali-model";
import { Dip_GG_GiustificativiModel } from "../../ClientServer-Service/GestionePresenze/Dip_GG_Giustificativi/Models/dip-gg-giustificativi-model";
import { Dip_GG_ResultModel } from "../../ClientServer-Service/GestionePresenze/Dip_GG_Result/Models/dip-gg-result-model";
import { Dip_GG_RichiestaModel } from "../../ClientServer-Service/GestionePresenze/Dip_GG_Richiesta/Models/dip-gg-richiesta-model";
import { Dip_GG_TimbraturaModel } from "../../ClientServer-Service/GestionePresenze/Dip_GG_Timbratura/Models/dip-gg-timbratura-model";
import { Dip_ProfiloOrario_DaySlot } from "../../ClientServer-Service/GestionePresenze/TimeSheet_EngineService/Models/time-sheet-engine-model";




export interface DayRecord {
  date: Date;
  dip_GG_Timbratura: Dip_GG_TimbraturaModel[];
  dip_GG_Giustificativi: Dip_GG_GiustificativiModel[];
  dip_GG_Result: Dip_GG_ResultModel;
  dip_GG_Causali: Dip_GG_CausaliModel[];
  idDip_RapportoLavoro: number;
}


export interface MonthData {
  year: number;
  month: number;  // 0-11 (gennaio = 0)
  days: { [key: number]: DayRecord };  // Mappa giorno -> record

  dip_GG_Richiesta: Dip_GG_RichiestaModel[];
  daySlot: Dip_ProfiloOrario_DaySlot[];
}


export interface TimeSheetRemoteData {
  dip_GG_Timbratura: Dip_GG_TimbraturaModel[];
  dip_GG_Giustificativi: Dip_GG_GiustificativiModel[];
  dip_GG_Richiesta: Dip_GG_RichiestaModel[];
  dip_GG_Result: Dip_GG_ResultModel[];
  dip_GG_Causali: Dip_GG_CausaliModel[];
  daySlot: Dip_ProfiloOrario_DaySlot[];
}



