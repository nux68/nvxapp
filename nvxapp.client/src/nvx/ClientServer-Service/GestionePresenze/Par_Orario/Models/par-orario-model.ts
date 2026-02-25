import { ModelResult } from "../../../ModelsBase/model-result";
import { Par_OrarioIntervalloHHModel } from "../../Par_OrarioIntervalloHH/Models/par-orario-intervallo-hh-model";

export class Par_OrarioModel {
  id: number;
  idAz_Anagrafica: number;
  codice: string;
  descrizione: string;
  numeroCoppie: number;
  sogliaHHStrao: number;
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
  par_OrarioIntervalloHH: Par_OrarioIntervalloHHModel[];
  constructor() {
    super();
    this.par_Orario = new Par_OrarioModel();
    this.par_OrarioIntervalloHH = [];
  }
}

export class Par_Orario_PutInModel {
  par_Orario: Par_OrarioModel;
  par_OrarioIntervalloHH: Par_OrarioIntervalloHHModel[];
  constructor() {
    this.par_Orario = new Par_OrarioModel();
    this.par_OrarioIntervalloHH = [];
  }
}
export class Par_Orario_PutOutModel extends ModelResult {
  par_Orario: Par_OrarioModel;
  par_OrarioIntervalloHH: Par_OrarioIntervalloHHModel[];
  constructor() {
    super();
    this.par_Orario = new Par_OrarioModel();
    this.par_OrarioIntervalloHH = [];
  }
}

export class Par_Orario_DeleteInModel {
  id: number;
}
export class Par_Orario_DeleteOutModel extends ModelResult { }

