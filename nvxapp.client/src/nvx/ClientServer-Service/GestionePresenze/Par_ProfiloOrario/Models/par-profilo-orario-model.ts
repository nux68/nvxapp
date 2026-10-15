import { ModelResult } from "../../../ModelsBase/model-result";
import { Par_OrarioIntervalloHHModel } from "../../Par_OrarioIntervalloHH/Models/par-orario-intervallo-hh-model";
import { Par_ProfiloOrarioGGModel } from "../../Par_ProfiloOrarioGG/Models/par-profilo-orario-gg-model";

export class Par_ProfiloOrarioModel {
  id: number;
  idAz_Anagrafica: number;
  codice: string;
  descrizione: string;
  numGiorniCiclo: number;
  tipoProfilo: TipoProfilo; 
  idPar_Orario_Festivo: number

  // straordinario / lavoro supplementare
  eccedenzeAttive: boolean;
  tipoConteggio: StraoTipoConteggio;
  straoSogliaHHFullTime: string | null;
  sogliaSettimanaleFT: number;
  supplLimitePerc: number;
  eccArrotondamento: TimeRoundInterval;
  eccFranchigiaMin: number;
  assenzeContanoSoglia: boolean;
  straoSoloAutorizzato: boolean;      // ore in piu' riconosciute solo fino alle ore autorizzate
  domenicaFestiva: boolean;
  notturnoDalle: string;
  notturnoAlle: string;

  idCausale_Lavoro_Strao: number;
  idCausale_Lavoro_StraoNott: number | null;
  idCausale_Lavoro_StraoFest: number | null;
  idCausale_Lavoro_StraoFestNott: number | null;
  idCausale_Lavoro_Suppl: number;
  idCausale_Lavoro_SupplEcc: number | null;
  idCausale_Lavoro_EccOrd: number | null;
  idCausale_BancaOre: number | null;
  idGiustificativo_Assenza_Ingiust: number;

  constructor() {
    this.id = 0;
    this.idAz_Anagrafica = 0;
    this.codice = '';
    this.descrizione = '';
    this.numGiorniCiclo = 0;
    this.tipoProfilo = 0;

    this.eccedenzeAttive = true;
    this.tipoConteggio = StraoTipoConteggio.Giornaliero;
    this.straoSogliaHHFullTime = '08:00:00';
    this.sogliaSettimanaleFT = 40;
    this.supplLimitePerc = 25;
    this.eccArrotondamento = TimeRoundInterval.Min15;
    this.eccFranchigiaMin = 0;
    this.assenzeContanoSoglia = false;
    this.straoSoloAutorizzato = false;
    this.domenicaFestiva = true;
    this.notturnoDalle = '22:00:00';
    this.notturnoAlle = '06:00:00';
    this.idCausale_Lavoro_Strao = 0;
    this.idCausale_Lavoro_StraoNott = null;
    this.idCausale_Lavoro_StraoFest = null;
    this.idCausale_Lavoro_StraoFestNott = null;
    this.idCausale_Lavoro_Suppl = 0;
    this.idCausale_Lavoro_SupplEcc = null;
    this.idCausale_Lavoro_EccOrd = null;
    this.idCausale_BancaOre = null;
    this.idGiustificativo_Assenza_Ingiust = 0;
  }

}



export enum TipoProfilo {
  Settimanale,
  Ciclico
}

export enum StraoTipoConteggio {
  Giornaliero,
  Settimanale,
  Mensile
}

// blocchi di arrotondamento (minuti), come TimeRoundInterval del server
export enum TimeRoundInterval {
  Min1 = 1,
  Min5 = 5,
  Min15 = 15,
  Min30 = 30,
  Min60 = 60
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
  //par_OrarioIntervalloHH: Par_OrarioIntervalloHHModel[];
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



