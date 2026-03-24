import { ModelResult } from "../../../ModelsBase/model-result";


export class Par_GiustificativiModel {

  public id!: number; // not Nullable
  public idAz_Anagrafica!: number; // not Nullable
  public descrizione!: string;
  public codice!: string;

  public backgroundColor!: string;
  public textColor!: string;
  public tipoInput!: JustTipoInput;
  public idCausale?: number; 
  public segno!: SignWithNeutral;

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


