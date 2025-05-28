import { Dip_GG_GiustificativiModel } from "../../ClientServer-Service/GestionePresenze/Dip_GG_Giustificativi/Models/dip-gg-giustificativi-model";
import { Dip_GG_RichiestaModel } from "../../ClientServer-Service/GestionePresenze/Dip_GG_Richiesta/Models/dip-gg-richiesta-model";
import { Dip_GG_TimbraturaModel } from "../../ClientServer-Service/GestionePresenze/Dip_GG_Timbratura/Models/dip-gg-timbratura-model";




export interface DayRecord {
  date: Date;
  dip_GG_Timbratura: Dip_GG_TimbraturaModel[];
  dip_GG_Giustificativi: Dip_GG_GiustificativiModel[];
}


export interface MonthData {
  year: number;
  month: number;  // 0-11 (gennaio = 0)
  days: { [key: number]: DayRecord };  // Mappa giorno -> record

  dip_GG_Richiesta: Dip_GG_RichiestaModel[];
}


export interface TimeSheetRemoteData {
  dip_GG_Timbratura: Dip_GG_TimbraturaModel[];
  dip_GG_Giustificativi: Dip_GG_GiustificativiModel[];
  dip_GG_Richiesta: Dip_GG_RichiestaModel[];
}



