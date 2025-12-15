import { ModelResult } from "../../../ModelsBase/model-result";



export class Dip_ProfiloOrarioModel {
  id: number;
  idDip_RapportoLavoro: number;
  idPar_ProfiloOrario: number;
  numGiornoPartenzaCiclo: number;

  constructor() {
    this.id = 0;
    this.idDip_RapportoLavoro = 0;
    this.idPar_ProfiloOrario = 0;
    this.numGiornoPartenzaCiclo = 0;
  }

}

export class Dip_ProfiloOrario_Get_InModel {
  id: number; // = IdDip_RapportoLavoro
}
export class Dip_ProfiloOrario_Get_OutModel extends ModelResult {
  dip_ProfiloOrario: Dip_ProfiloOrarioModel[] = [];
}

export class Dip_ProfiloOrario_Put_InModel {
  id: number; // = IdDip_RapportoLavoro
  dip_ProfiloOrario: Dip_ProfiloOrarioModel[] = [];
}
export class Dip_ProfiloOrario_Put_OutModel extends ModelResult {
  id: number; // = IdDip_RapportoLavoro
  dip_ProfiloOrario: Dip_ProfiloOrarioModel[] = [];
}

