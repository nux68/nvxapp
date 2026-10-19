import { ModelResult } from "../../../ModelsBase/model-result";

// tipo di spesa delle note spese (parametro dell'azienda)
export class Par_TipoSpesaModel {
  id: number = 0;
  idAz_Anagrafica: number = 0;
  codice: string = '';
  descrizione: string = '';
}

export class Par_TipoSpesaInModel {
}
export class Par_TipoSpesaOutModel extends ModelResult {
  public par_TipoSpesa: Par_TipoSpesaModel[] = [];
}

export class Par_TipoSpesaGetInModel {
  public id: number = 0;
}
export class Par_TipoSpesaGetOutModel extends ModelResult {
  public par_TipoSpesa: Par_TipoSpesaModel = new Par_TipoSpesaModel();
}

export class Par_TipoSpesaPutInModel {
  public par_TipoSpesa: Par_TipoSpesaModel = new Par_TipoSpesaModel();
}
export class Par_TipoSpesaPutOutModel extends ModelResult {
  public par_TipoSpesa: Par_TipoSpesaModel = new Par_TipoSpesaModel();
}

export class Par_TipoSpesa_DeleteInModel {
  public id: number = 0;
}
export class Par_TipoSpesa_DeleteOutModel extends ModelResult {
}
