import { ModelResult } from "../../../ModelsBase/model-result";
import { Par_ExportCau_CausaliModel } from "../../Par_ExportCau_Causali/Models/par-export-cau-causali-model";

export class Par_ExportCauModel {
  id: number;
  idAz_Anagrafica: number;
  codice: string;
  descrizione: string;
  tipoFile: Par_Export_TipoFile;
}

export enum Par_Export_TipoFile {
  CSV = 0,
  TXT = 1
}

export class Par_ExportCau_GetAll_InModel { }
export class Par_ExportCau_GetAll_OutModel extends ModelResult {
  par_ExportCau: Par_ExportCauModel[];
  constructor() {
    super();
    this.par_ExportCau = [];
  }
}

export class Par_ExportCau_Get_InModel {
  id: number = 0;
}
export class Par_ExportCau_Get_OutModel extends ModelResult {
  par_ExportCau: Par_ExportCauModel;
  par_ExportCau_Causali: Par_ExportCau_CausaliModel[];
  constructor() {
    super();
    this.par_ExportCau = new Par_ExportCauModel();
    this.par_ExportCau_Causali = [];
  }
}

export class Par_ExportCau_Put_InModel {
  par_ExportCau: Par_ExportCauModel;
  par_ExportCau_Causali: Par_ExportCau_CausaliModel[];
  constructor() {
    this.par_ExportCau = new Par_ExportCauModel();
    this.par_ExportCau_Causali = [];
  }
}
export class Par_ExportCau_Put_OutModel extends ModelResult {
  par_ExportCau: Par_ExportCauModel;
  par_ExportCau_Causali: Par_ExportCau_CausaliModel[];
  constructor() {
    super();
    this.par_ExportCau = new Par_ExportCauModel();
    this.par_ExportCau_Causali = [];
  }
}

export class Par_ExportCau_Delete_InModel {
  id: number = 0;
}
export class Par_ExportCau_Delete_OutModel extends ModelResult { }
