import { ModelResult } from "../../../ModelsBase/model-result";

export class Par_ProfiloOrarioModel {
  id: number;
  idAz_Anagrafica: number;
  codice: string;
  descrizione: string;
  numGiorniCiclo: number;
}

export class Par_ProfiloOrario_GetAllInModel { }
export class Par_ProfiloOrario_GetAllOutModel extends ModelResult {
  par_ProfiloOrario: Par_ProfiloOrarioModel[];
  constructor() {
    super();
    this.par_ProfiloOrario = [];
  }
}

export class Par_ProfiloOrario_GetInModel {
  id: number;
}
export class Par_ProfiloOrario_GetOutModel extends ModelResult {
  par_ProfiloOrario: Par_ProfiloOrarioModel;
  constructor() {
    super();
    this.par_ProfiloOrario = new Par_ProfiloOrarioModel();
  }
}

export class Par_ProfiloOrario_PutInModel {
  par_ProfiloOrario: Par_ProfiloOrarioModel;
  constructor() {
    this.par_ProfiloOrario = new Par_ProfiloOrarioModel();
  }
}
export class Par_ProfiloOrario_PutOutModel extends ModelResult {
  par_ProfiloOrario: Par_ProfiloOrarioModel;
  constructor() {
    super();
    this.par_ProfiloOrario = new Par_ProfiloOrarioModel();
  }
}

export class Par_ProfiloOrario_DeleteInModel {
  id: number;
}
export class Par_ProfiloOrario_DeleteOutModel extends ModelResult { }



