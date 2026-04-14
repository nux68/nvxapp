import { ModelResult } from "../../../ModelsBase/model-result";
import { Dip_GG_GiustificativiModel } from "../../Dip_GG_Giustificativi/Models/dip-gg-giustificativi-model";
import { Dip_GG_TimbraturaModel } from "../../Dip_GG_Timbratura/Models/dip-gg-timbratura-model";

/************************************************************/
/******************** MODEL DATA ****************************/
/************************************************************/

export class Dip_GG_RichiestaModel {

  id!: number;
  idDip_RapportoLavoro!: number;

  data!: string;
  dataA!: string;

  richiestaTipo!: TipoRichiesta;
  
  // Campo per oggetto JSON
  dati?: string;  // contiene oggetti di tipo  Dip_GG_Richiesta_Body_Timbratura,Dip_GG_Richiesta_Body_Giustificativo, Dip_GG_Richiesta_Body_NotaSpesa

  richiestaStato!: StatoRichiesta;
  richiestaApprovazioneData: Dip_GG_Richiesta_Stato_Cronology[];

  revocaStato?: StatoRichiesta;
  revocaApprovazioneData: Dip_GG_Richiesta_Stato_Cronology[];

}
export class Dip_GG_Richiesta_Stato_Cronology {
  idAspNetUsers!: string;
  richiestaStato!: StatoRichiesta;
  data!: Date;
}
export class Dip_GG_Richiesta_Body_Timbratura {
  hhmm!: string;
}
export class Dip_GG_Richiesta_Body_Giustificativo {
  hhmm!: string;
  allDay!: boolean;
  idPar_Giustificativi!: number;
}
export class Dip_GG_Richiesta_Body_NotaSpesa {
}


export enum TipoRichiesta {
  Timbratura,
  Giustificativo,
  NotaSpesa,
  ApprovazioneStraordinario
}
export enum StatoRichiesta {
  Diretta,

  Immessa,
  Cancellata,
  Rifiutata,
  ApprovazioneInCorso,
  ParzialmenteApprovata,

  Approvata
}


/************************************************************/
/******************** DATI INPUT X API **********************/
/************************************************************/

export class Dip_GG_Richiesta_GetAll4User_InModel {
  public idAspNetUsers?: string
  public year: number;
  public month: number;

  constructor() {}
}
export class Dip_GG_Richiesta_GetAll4User_OutModel extends ModelResult {

  public dip_GG_Richiesta: Dip_GG_RichiestaModel[];

}


export class Dip_GG_Richiesta_GetAll4Admin_InModel {
  public year: number;
  public month: number;

  constructor() { }
}
export class Dip_GG_Richiesta_GetAll4Admin_OutModel extends ModelResult {

  public dip_GG_Richiesta: Dip_GG_RichiestaModel[];

}


export class Dip_GG_Richiesta_Send_InModel {
  public excludeRicalc: boolean;
  public idAspNetUsers?: string
  public dip_GG_Richiesta: Dip_GG_RichiestaModel;
  public fromHR: boolean;
  constructor() {
    this.dip_GG_Richiesta = new Dip_GG_RichiestaModel(); 
  }
}
export class Dip_GG_Richiesta_Send_OutModel extends ModelResult {

  public dip_GG_Richiesta: Dip_GG_RichiestaModel[];
  public dip_GG_Timbratura: Dip_GG_TimbraturaModel[];
  public dip_GG_Giustificativi: Dip_GG_GiustificativiModel[];

  
  constructor() {
    super();
  }

}


export class Dip_GG_Richiesta_SetState_InModel {

  public excludeRicalc: boolean;
  public richiestaStato!: StatoRichiesta;
  public idDip_GG_Richiesta: number[] = [];
  public fromHR: boolean;

  constructor() {
    
  }
}
export class Dip_GG_Richiesta_SetState_OutModel extends ModelResult {

  public dip_GG_Richiesta: Dip_GG_RichiestaModel[] = [];
  public dip_GG_Timbratura: Dip_GG_TimbraturaModel[] = [];
  public dip_GG_Giustificativi: Dip_GG_GiustificativiModel[] = [];


  constructor() {
    super();
    //this.dip_GG_Richiesta = new Dip_GG_RichiestaModel();
  }

}


export class Dip_GG_Richiesta_Get_4Calculation_InModel {
  public usersId: string[] = [];
  public dal!: string;
  public al!: string;
}
export class Dip_GG_Richiesta_Get_4Calculation_OutModel extends ModelResult {
  public dip_GG_Richiesta: Dip_GG_RichiestaModel[] = [];
}
