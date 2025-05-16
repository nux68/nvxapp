import { ModelResult } from "../../../ModelsBase/model-result";


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




export class Dip_GG_Richiesta_GetAll4User_InModel {
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
  public dip_GG_Richiesta: Dip_GG_RichiestaModel;
  constructor() {
    this.dip_GG_Richiesta = new Dip_GG_RichiestaModel(); 
  }
}

export class Dip_GG_Richiesta_Send_OutModel extends ModelResult {

  
  constructor() {
    super();
  }

}



export class Dip_GG_Richiesta_SetState_InModel {

  richiestaStato!: StatoRichiesta;
  IdDip_GG_Richiesta: number[] = [];

  constructor() {
    
  }
}

export class Dip_GG_Richiesta_SetState_OutModel extends ModelResult {

  //public dip_GG_Richiesta: Dip_GG_RichiestaModel;
  constructor() {
    super();
    //this.dip_GG_Richiesta = new Dip_GG_RichiestaModel();
  }

}
