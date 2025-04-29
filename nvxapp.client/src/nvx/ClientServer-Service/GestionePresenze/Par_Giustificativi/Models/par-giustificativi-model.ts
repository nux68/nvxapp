import { ModelResult } from "../../../ModelsBase/model-result";



export class Par_GiustificativiInModel {
  
}

export class Par_GiustificativiOutModel extends ModelResult {

  public par_Giustificativi: Par_GiustificativiModel[];

}

export class Par_GiustificativiModel  {

  public id!: number; // not Nullable
  public idAz_Anagrafica!: number; // not Nullable
  public descrizione!: string;
  public codice!: string;

  public backgroundColor!: string;
  public textColor!: string;

}

