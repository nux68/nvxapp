import { ModelResult } from "../../../ModelsBase/model-result";
import { Par_OrarioIntervalloHHModel } from "../../Par_OrarioIntervalloHH/Models/par-orario-intervallo-hh-model";
import { Par_ProfiloOrarioGGModel } from "../../Par_ProfiloOrarioGG/Models/par-profilo-orario-gg-model";

export class Par_ProfiloOrarioModel {
  id: number;
  idAz_Anagrafica: number;
  codice: string;
  descrizione: string;
  numGiorniCiclo: number;
  tipoProfilo: number; // 0= settimanale , 1= ciclico

  constructor() {
    this.id = 0;
    this.idAz_Anagrafica = 0;
    this.codice = '';
    this.descrizione = '';
    this.numGiorniCiclo = 0;
    this.tipoProfilo = 0;
  }

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
  par_ProfiloOrarioGG: Par_ProfiloOrarioGGModel[];
  par_OrarioIntervalloHH: Par_OrarioIntervalloHHModel[];
  constructor() {
    super();
    this.par_ProfiloOrario = new Par_ProfiloOrarioModel();
    this.par_ProfiloOrarioGG = [];
  }
}

export class Par_ProfiloOrario_PutInModel {
  par_ProfiloOrario: Par_ProfiloOrarioModel;
  par_ProfiloOrarioGG: Par_ProfiloOrarioGGModel[];
  constructor() {
    this.par_ProfiloOrario = new Par_ProfiloOrarioModel();
    this.par_ProfiloOrarioGG = [];
  }
}
export class Par_ProfiloOrario_PutOutModel extends ModelResult {
  par_ProfiloOrario: Par_ProfiloOrarioModel;
  par_ProfiloOrarioGG: Par_ProfiloOrarioGGModel[];
  constructor() {
    super();
    this.par_ProfiloOrario = new Par_ProfiloOrarioModel();
    this.par_ProfiloOrarioGG = [];
  }
}

export class Par_ProfiloOrario_DeleteInModel {
  id: number;
}
export class Par_ProfiloOrario_DeleteOutModel extends ModelResult { }



