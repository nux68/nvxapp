import { ModelResult } from "../../../ModelsBase/model-result";

export class Par_OrarioModel {
  id: number;
  idAz_Anagrafica: number;
  codice: string;
  descrizione: string;
}

export class Par_Orario_GetAllInModel { }
export class Par_Orario_GetAllOutModel extends ModelResult {
  par_Orario: Par_OrarioModel[];
  constructor() {
    super();
    this.par_Orario = [];
  }
}

export class Par_Orario_GetInModel {
  id: number;
}
export class Par_Orario_GetOutModel extends ModelResult {
  par_Orario: Par_OrarioModel;
  constructor() {
    super();
    this.par_Orario = new Par_OrarioModel();
  }
}

export class Par_Orario_PutInModel {
  par_Orario: Par_OrarioModel;
  constructor() {
    this.par_Orario = new Par_OrarioModel();
  }
}
export class Par_Orario_PutOutModel extends ModelResult {
  par_Orario: Par_OrarioModel;
  constructor() {
    super();
    this.par_Orario = new Par_OrarioModel();
  }
}

export class Par_Orario_DeleteInModel {
  id: number;
}
export class Par_Orario_DeleteOutModel extends ModelResult { }

