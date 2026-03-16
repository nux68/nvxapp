import { ModelResult } from "../../../ModelsBase/model-result";
import { Dip_AnagraficaModel } from '../../Dip_Anagrafica/Models/dip-anagrafica-model';
import { Dip_RapportoLavoroModel } from '../../Dip_RapportoLavoro/Models/dip-rapporto-lavoro-model';
import { Dip_GG_CausaliModel } from '../../Dip_GG_Causali/Models/dip-gg-causali-model';
import { Dip_GG_GiustificativiModel } from '../../Dip_GG_Giustificativi/Models/dip-gg-giustificativi-model';
import { Dip_GG_RichiestaModel } from '../../Dip_GG_Richiesta/Models/dip-gg-richiesta-model';
import { Dip_GG_TimbraturaModel } from '../../Dip_GG_Timbratura/Models/dip-gg-timbratura-model';
import { Par_OrarioIntervalloHHModel } from '../../Par_OrarioIntervalloHH/Models/par-orario-intervallo-hh-model';
import { Par_OrarioModel } from '../../Par_Orario/Models/par-orario-model';
import { Par_ProfiloOrarioGGModel } from '../../Par_ProfiloOrarioGG/Models/par-profilo-orario-gg-model';
import { Par_ProfiloOrarioModel } from '../../Par_ProfiloOrario/Models/par-profilo-orario-model';

// ─── Calculate ────────────────────────────────────────────────────────────────

export class TimeSheet_CalculateModel {
  year: number;
  month: number;
  selectedUserId: string[] | null = null;
  dal: string;
  al: string;
  approva_Richieste_Timbrature: boolean;
  approva_Richieste_Giustificativo: boolean;
  genera_Timbrature_Mancanti: boolean;
}

export class TimeSheet_CalculateInModel {
  timeSheet_Calculate: TimeSheet_CalculateModel;
  constructor() {
    this.timeSheet_Calculate = new TimeSheet_CalculateModel();
  }
}

export class TimeSheet_CalculateOutModel extends ModelResult {
  timeSheet_Calculate: TimeSheet_CalculateModel;
}

// ─── DaySlot helpers ──────────────────────────────────────────────────────────

export class Dip_ProfiloOrario_DaySlot_GG {
  zOrder: number;
  idPar_Orario: number;
}

export class Dip_ProfiloOrario_DaySlot {
  idAspNetUsers: string;
  idDip_RapportoLavoro: number;
  data: string;               // ISO date string
  idPar_ProfiloOrario: number;
  orari: Dip_ProfiloOrario_DaySlot_GG[] = [];
}

// ─── Get_OrariSchema_4User ────────────────────────────────────────────────────

export class OrariSchema_4User_InModel {
  usersId: string[] = [];
  dal: string;
  al: string;
}

export class OrariSchema_4User_OutModel extends ModelResult {
  dip_Anagrafica: Dip_AnagraficaModel[] = [];
  dip_RapportoLavoro: Dip_RapportoLavoroModel[] = [];
  daySlots: Dip_ProfiloOrario_DaySlot[] = [];
  par_ProfiloOrario: Par_ProfiloOrarioModel[] = [];
  par_ProfiloOrarioGG: Par_ProfiloOrarioGGModel[] = [];
  parOrario: Par_OrarioModel[] = [];
  par_OrarioIntervalloHH: Par_OrarioIntervalloHHModel[] = [];
}

// ─── Dip_GG_AllData ───────────────────────────────────────────────────────────

export class Dip_GG_AllData_InModel {
  usersId: string[] = [];
  dal: string;
  al: string;
}

export class Dip_GG_AllData_OutModel extends ModelResult {
  dip_GG_Timbratura: Dip_GG_TimbraturaModel[] = [];
  dip_GG_Causali: Dip_GG_CausaliModel[] = [];
  dip_GG_Giustificativi: Dip_GG_GiustificativiModel[] = [];
  dip_GG_Richiesta: Dip_GG_RichiestaModel[] = [];
}

// ─── Get_Timesheet_AllData ────────────────────────────────────────────────────

export class Timesheet_AllData_InModel {
  usersId: string[] | null = null;
  dal: string;
  al: string;
}

export class Timesheet_AllData_OutModel extends ModelResult {
  dip_GG_AllData_OutModel: Dip_GG_AllData_OutModel = new Dip_GG_AllData_OutModel();
  orariSchema_4User_OutModel: OrariSchema_4User_OutModel = new OrariSchema_4User_OutModel();
}




