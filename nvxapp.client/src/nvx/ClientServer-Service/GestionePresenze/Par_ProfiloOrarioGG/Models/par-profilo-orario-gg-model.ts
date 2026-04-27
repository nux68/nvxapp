import { ModelResult } from "../../../ModelsBase/model-result";

export class Par_ProfiloOrarioGGModel {
  id: number;
  idPar_ProfiloOrario: number;
  numGiorno: number;
  zOrder: number;
  idPar_Orario: number;
  //idAz_SubCommessaAttivita: number;
}

// Modelli per la gestione dell'edit
export class Par_ProfiloOrarioGG_Get_4Edit_InModel {
  id: number = 0; // id del profilo
}
export class Par_ProfiloOrarioGG_Get_4Edit_OutModel extends ModelResult {
  par_ProfiloOrarioGG: Par_ProfiloOrarioGGModel[] = [];
}

export class Par_ProfiloOrarioGG_Put_4Edit_InModel {
  id: number = 0; // id del profilo
  par_ProfiloOrarioGG: Par_ProfiloOrarioGGModel[] = [];
}
export class Par_ProfiloOrarioGG_Put_4Edit_OutModel extends ModelResult {
  id: number = 0; // id del profilo
  par_ProfiloOrarioGG: Par_ProfiloOrarioGGModel[] = [];
}

export class Par_ProfiloOrarioGG_Arrange_NumDay_InModel {
  id: number = 0; // id del profilo
  numGiorniCiclo: number = 0;

  numGiorno_Incrementa: number = 0;

  par_ProfiloOrarioGG: Par_ProfiloOrarioGGModel[] = [];
}
export class Par_ProfiloOrarioGG_Arrange_NumDay_OutModel extends ModelResult {
  id: number = 0; // id del profilo
  par_ProfiloOrarioGG: Par_ProfiloOrarioGGModel[] = [];
}
