import { ModelResult } from "../../../ModelsBase/model-result";
import { Par_OrarioModel } from "../../Par_Orario/Models/par-orario-model";
import { Par_OrarioIntervalloHHModel } from "../../Par_OrarioIntervalloHH/Models/par-orario-intervallo-hh-model";


export class Dip_ProfiloOrarioModel {
  id: number;
  idDip_RapportoLavoro: number;
  idPar_ProfiloOrario: number;
  numGiornoPartenzaCiclo: number;
  dal: string;
  al: string;

  constructor() {
    this.id = 0;
    this.idDip_RapportoLavoro = 0;
    this.idPar_ProfiloOrario = 0;
    this.numGiornoPartenzaCiclo = 0;
    this.dal = "";
    this.al = "";
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

export class Dip_ProfiloOrario_Get_Profile_4Calculation_InModel {
  usersId: string[] = [];
  dal: string = "";
  al: string = "";
}

// Una riga orario del giorno, corrispondente a una riga Par_ProfiloOrarioGG.
// zOrder 1 = orario base (sempre presente), zOrder > 1 = override condizionale.
export class Dip_ProfiloOrario_DaySlot_GG {
  zOrder: number = 0;
  idPar_Orario: number = 0;
}

// Un record per ogni combinazione dipendente × giorno con tutte le righe orario ordinate per zOrder
export class Dip_ProfiloOrario_DaySlot {
  idAspNetUsers: string = "";
  idDip_RapportoLavoro: number = 0;
  data: string = "";                // DateTime serializzato come stringa ISO
  idPar_ProfiloOrario: number = 0;
  orari: Dip_ProfiloOrario_DaySlot_GG[] = [];
}

export class Dip_ProfiloOrario_Get_Profile_4Calculation_OutModel extends ModelResult {
  daySlots: Dip_ProfiloOrario_DaySlot[] = [];
  parOrario: Par_OrarioModel[] = [];
  par_OrarioIntervalloHH: Par_OrarioIntervalloHHModel[] = [];
}

