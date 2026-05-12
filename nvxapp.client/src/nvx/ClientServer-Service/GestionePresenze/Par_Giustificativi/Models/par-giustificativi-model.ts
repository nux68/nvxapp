import { ModelResult } from "../../../ModelsBase/model-result";


export class Par_GiustificativiModel {

  public id!: number;
  public idAz_Anagrafica!: number;
  public descrizione!: string;
  public codice!: string;

  public backgroundColor!: string;
  public textColor!: string;
  public tipoInput!: JustTipoInput;
  public idCausale?: number;
  public segno!: SignWithNeutral;
  public visualizzaInPianoFerie!: boolean;
  public tipoContatore: TipoContatore = TipoContatore.NoContatore;

}

export enum SignWithNeutral {
  Down = -1,
  Neutral = 0,
  Up = 1
}

export enum JustTipoInput {
  InteraGiornate,
  Intervallo,
  Tutti
}

export enum TipoContatore {
  NoContatore        = 0,
  Contatore          = 1,
  ContatoreConAvviso = 2,
  ContatoreConBlocco = 3
}


export class Par_GiustificativiInModel {
  
}
export class Par_GiustificativiOutModel extends ModelResult {

  public par_Giustificativi: Par_GiustificativiModel[];

}



export class Par_GiustificativiGetInModel {
  public id: number;
}
export class Par_GiustificativiGetOutModel extends ModelResult {
  public par_Giustificativi: Par_GiustificativiModel;
}
export class Par_GiustificativiPutInModel {
  public par_Giustificativi: Par_GiustificativiModel;
}
export class Par_GiustificativiPutOutModel extends ModelResult {
  public par_Giustificativi: Par_GiustificativiModel;
}

export class Par_Giustificativi_DeleteInModel {
  id: number;
}
export class Par_Giustificativi_DeleteOutModel extends ModelResult {
}


