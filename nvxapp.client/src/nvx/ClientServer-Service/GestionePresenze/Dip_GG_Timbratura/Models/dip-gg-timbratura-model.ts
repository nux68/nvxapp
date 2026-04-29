import { ModelResult } from "../../../ModelsBase/model-result";
import { StatoRichiesta } from "../../Dip_GG_Richiesta/Models/dip-gg-richiesta-model";
import { Par_AttivitaModel } from "../../Par_Attivita/Models/par-attivita-model";



export class Dip_GG_TimbraturaModel {

  public id: number;
  public idDip_RapportoLavoro!: number;

  public timbratura!: Date;
  public timbraturaOriginale!: Date;
  public timbraturaArrotondata?: Date;
  public giornoCompetenza!: Date;

  public timbraturaTipo!: TipoTimbratura;

  public richiestaStato!: StatoRichiesta;
  public idDip_GG_Richiesta?: number;
  public idAz_SubCommessaAttivita: number;

}
export enum TipoTimbratura {
  Entrata,
  Uscita,
  SenzaVerso,
  Attivita
}



export class Dip_GG_Timbratura_GetAll_InModel {
  public idAspNetUsers?: string
  public year: number;
  public month: number;
}
export class Dip_GG_Timbratura_GetAll_OutModel extends ModelResult {
  public dip_GG_Timbratura: Dip_GG_TimbraturaModel[];
}


export class Dip_GG_Timbratura_Stamp_InModel {
  public dateStamp!: string;
  public excludeRicalc: boolean;
  public idAz_SubCommessaAttivita: number;
}
export class Dip_GG_Timbratura_Stamp_OutModel extends ModelResult {
}


export class Dip_GG_Timbratura_Get_4Calculation_InModel {
  public usersId: string[] = [];
  public dal!: string; // ISO 8601 locale, es. "2026-03-01T00:00:00"
  public al!: string;  // ISO 8601 locale, es. "2026-03-31T23:59:59"
}
export class Dip_GG_Timbratura_Get_4Calculation_OutModel extends ModelResult {
  public dip_GG_Timbratura: Dip_GG_TimbraturaModel[] = [];
}


export class Dip_GG_TimbraturaGetInModel {
  public id: number;

  public data?: string;
  public idDip_RapportoLavoro!: number;
}
export class Dip_GG_TimbraturaGetOutModel extends ModelResult {
  public dip_GG_Timbratura: Dip_GG_TimbraturaModel;
}


export class Dip_GG_TimbraturaPutInModel {
  public excludeRicalc: boolean;
  public dip_GG_Timbratura: Dip_GG_TimbraturaModel;
}
export class Dip_GG_TimbraturaPutOutModel extends ModelResult {
  public dip_GG_Timbratura: Dip_GG_TimbraturaModel;
}



export class Dip_GG_Timbratura_DeleteInModel {
  public excludeRicalc: boolean;
  public id: number;
}
export class Dip_GG_Timbratura_DeleteOutModel extends ModelResult {

}

export class Dip_GG_Timbratura_StampPrepare_InModel {
  public idAspNetUsers?: string;
}
export class Dip_GG_Timbratura_StampPrepare_OutModel extends ModelResult {
  public idAspNetUsers: string = '';
  public currentDate!: Date;
  public idPar_Attivita: number | null = null;
  public par_Attivita: Par_AttivitaModel[] = [];
}







