import { ModelResult } from "../../../ModelsBase/model-result";

export enum TipoFestivita {
  DataRicorrente,   // stesso giorno e mese ogni anno
  DataSingola,      // solo nella data indicata
  Pasquetta         // lunedi' dopo Pasqua, calcolato ogni anno
}

export class Par_FestivitaModel {
  public id: number = 0;
  public idAz_Anagrafica: number = 0;
  public descrizione: string = '';
  public tipoFestivita: TipoFestivita = TipoFestivita.DataRicorrente;
  public data: string | null = null; // "YYYY-MM-DDT00:00:00"
}

export class Par_Festivita_GetAllInModel { }
export class Par_Festivita_GetAllOutModel extends ModelResult {
  public par_Festivita: Par_FestivitaModel[] = [];
}

export class Par_Festivita_GetInModel {
  public id: number;
}
export class Par_Festivita_GetOutModel extends ModelResult {
  public par_Festivita: Par_FestivitaModel;
}

export class Par_Festivita_PutInModel {
  public par_Festivita: Par_FestivitaModel;
}
export class Par_Festivita_PutOutModel extends ModelResult {
  public par_Festivita: Par_FestivitaModel;
}

export class Par_Festivita_DeleteInModel {
  public id: number;
}
export class Par_Festivita_DeleteOutModel extends ModelResult { }
