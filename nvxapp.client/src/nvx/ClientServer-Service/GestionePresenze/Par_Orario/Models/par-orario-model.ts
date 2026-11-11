import { ModelResult } from "../../../ModelsBase/model-result";
import { Par_OrarioIntervalloHHModel } from "../../Par_OrarioIntervalloHH/Models/par-orario-intervallo-hh-model";

export class Par_OrarioModel {
  id: number;
  idAz_Anagrafica: number;
  codice: string;
  descrizione: string;
  numeroCoppie: number;
  idCausale_HH_Lav_MonteOre: number | null;
  timbratureTipo: OrarioTimbratureTipo;
  hh_Teo_MonteOre: string | null;

  idAz_SubCommessaAttivita_MonteOre: number | null;
}

export enum OrarioTimbratureTipo {
  IntervalloOrario,
  MonteOre,
  MonteOreValore
}

/** Pausa dell'orario (ANALISI_ATTENDANCETRACKING_PAUSE.md) */
export enum PausaTipo {
  Pranzo = 0,
  Breve = 1
}

/** Rilevata: la pausa va timbrata, il calcolo controlla e avvisa; Automatica: timbratura facoltativa, il calcolo detrae la parte mancante */
export enum PausaModalita {
  Rilevata = 0,
  Automatica = 1
}

export class Par_OrarioPausaModel {
  id: number = 0;
  idPar_Orario: number = 0;
  numPausa: number = 1;
  tipo: PausaTipo = PausaTipo.Pranzo;
  descrizione: string | null = null;
  modalita: PausaModalita = PausaModalita.Rilevata;
  durataMin: number = 60;
  durataMaxMin: number | null = null;
  sogliaMin: number = 360;
  finestraDalle: string | null = null;   // "HH:mm:ss", vuota = dagli intervalli di riconoscimento
  finestraAlle: string | null = null;
  retribuitaMin: number = 0;
  idCausale_Pausa: number | null = null;
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
  par_OrarioPausa: Par_OrarioPausaModel[];
  constructor() {
    super();
    this.par_Orario = new Par_OrarioModel();
    this.par_OrarioIntervalloHH = [];
    this.par_OrarioPausa = [];
  }
}

export class Par_Orario_PutInModel {
  par_Orario: Par_OrarioModel;
  par_OrarioIntervalloHH: Par_OrarioIntervalloHHModel[];
  par_OrarioPausa: Par_OrarioPausaModel[];
  constructor() {
    this.par_Orario = new Par_OrarioModel();
    this.par_OrarioIntervalloHH = [];
    this.par_OrarioPausa = [];
  }
}
export class Par_Orario_PutOutModel extends ModelResult {
  par_Orario: Par_OrarioModel;
  par_OrarioIntervalloHH: Par_OrarioIntervalloHHModel[];
  par_OrarioPausa: Par_OrarioPausaModel[];
  constructor() {
    super();
    this.par_Orario = new Par_OrarioModel();
    this.par_OrarioIntervalloHH = [];
    this.par_OrarioPausa = [];
  }
}

export class Par_Orario_DeleteInModel {
  id: number;
}
export class Par_Orario_DeleteOutModel extends ModelResult { }

