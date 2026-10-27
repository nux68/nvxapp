import { ModelResult } from "../../../ModelsBase/model-result";

/** comportamento del tipo di spesa: generica (importo libero) o rimborso km (importo = km x rimborso dell'autovettura) */
export enum TipoSpesaTipo {
  Generica,
  RimborsoKm
}

/** chiavi di traduzione dei valori di TipoSpesaTipo */
export const TIPO_SPESA_TIPO_TESTO: { [tipo in TipoSpesaTipo]: string } = {
  [TipoSpesaTipo.Generica]: 'attendance.pages.tipoSpesaEdit.tipoGenerica',
  [TipoSpesaTipo.RimborsoKm]: 'attendance.pages.tipoSpesaEdit.tipoRimborsoKm',
};

// tipo di spesa delle note spese (parametro dell'azienda)
export class Par_TipoSpesaModel {
  id: number = 0;
  idAz_Anagrafica: number = 0;
  codice: string = '';
  descrizione: string = '';
  tipo: TipoSpesaTipo = TipoSpesaTipo.Generica;
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
