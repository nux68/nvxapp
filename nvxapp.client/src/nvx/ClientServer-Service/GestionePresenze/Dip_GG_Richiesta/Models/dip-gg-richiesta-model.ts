import { ModelResult } from "../../../ModelsBase/model-result";


export class Dip_GG_RichiestaModel {

  id!: number;
  idDip_RapportoLavoro!: number;

  data!: string;
  dataA!: string;

  richiestaTipo!: TipoRichiesta;
  richiestaStato!: StatoRichiesta;

  // Campo per oggetto JSON
  dati?: string;

  // Campo per oggetto JSON
  cronologiaApprovazione?: string;

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
  NotaSpesa
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
  public dip_GG_RichiestaModel: Dip_GG_RichiestaModel;
  constructor() {
    this.dip_GG_RichiestaModel = new Dip_GG_RichiestaModel(); 
  }
}

export class Dip_GG_Richiesta_Send_OutModel extends ModelResult {

  public dip_GG_RichiestaModel: Dip_GG_RichiestaModel;
  constructor() {
    super();
    this.dip_GG_RichiestaModel = new Dip_GG_RichiestaModel();
  }

}
