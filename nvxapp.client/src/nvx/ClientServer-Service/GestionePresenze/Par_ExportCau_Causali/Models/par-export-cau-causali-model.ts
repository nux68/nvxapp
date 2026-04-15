import { ModelResult } from "../../../ModelsBase/model-result";

export class Par_ExportCau_CausaliModel {
  id: number;
  idPar_ExportCau: number;
  idCausale: number;
  codice: string;
  tipoElaborazione: Par_Export_TipoElaborazione;
  tipoUnita: Par_Export_TipoUnita;
}

export enum Par_Export_TipoElaborazione {
  Gionaliera = 0,
  Mensile    = 1
}

export enum Par_Export_TipoUnita {
  Ore     = 0,
  Giorni  = 1,
  Importo = 2
}

export class Par_ExportCau_Causali_Get_InModel {
  id: number = 0; // id del Par_ExportCau padre
}
export class Par_ExportCau_Causali_Get_OutModel extends ModelResult {
  par_ExportCau_Causali: Par_ExportCau_CausaliModel[] = [];
}

export class Par_ExportCau_Causali_Put_InModel {
  idPar_ExportCau: number = 0;
  par_ExportCau_Causali: Par_ExportCau_CausaliModel[] = [];
}
export class Par_ExportCau_Causali_Put_OutModel extends ModelResult {
  par_ExportCau_Causali: Par_ExportCau_CausaliModel[] = [];
}
