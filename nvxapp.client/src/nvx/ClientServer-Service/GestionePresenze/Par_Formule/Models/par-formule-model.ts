import { ModelResult } from "../../../ModelsBase/model-result";

/** fase del calcolo in cui gira la formula (stessi valori dell'enum FormulaFase del server) */
export enum FormulaFase {
  Giorno = 1,
  Finale = 2
}
export const FORMULA_FASE_TESTO: { [f in FormulaFase]: string } = {
  [FormulaFase.Giorno]: 'attendance.formule.faseGiorno',
  [FormulaFase.Finale]: 'attendance.formule.faseFinale',
};

/** tipo di riga del log delle formule (FormulaLogTipo del server) */
export enum FormulaLogTipo {
  Info = 0,
  Warning = 1,
  Errore = 2,
  Debug = 3
}

// formula personalizzata del calcolo giornata (script JavaScript eseguito sul server)
export class Par_FormuleModel {
  id: number = 0;
  idAz_Anagrafica: number = 0;
  codice: string = '';
  titolo: string = '';
  descrizione: string = '';
  testo: string = '';
}

export class FormulaErrore {
  riga: number = 0;
  colonna: number = 0;
  messaggio: string = '';
}

export class FormulaMetodo {
  categoria: string = '';
  tipo: string = '';
  testo: string = '';
  descrizione: string = '';
  inEvidenza: boolean = false;
}

export class FormulaProvaCausale {
  codice: string = '';
  minuti: number = 0;
  importo: number | null = null;
}
export class FormulaLogRiga {
  idPar_Formule: number | null = null;
  tipo: FormulaLogTipo = FormulaLogTipo.Info;
  messaggio: string = '';
}
export class FormulaProvaRisultato {
  eseguita: boolean = false;
  causaliPrima: FormulaProvaCausale[] = [];
  causaliDopo: FormulaProvaCausale[] = [];
  log: FormulaLogRiga[] = [];
  warning: boolean = false;
  anomalia: boolean = false;
}

export class Par_FormuleInModel { }
export class Par_FormuleOutModel extends ModelResult {
  public par_Formule: Par_FormuleModel[] = [];
}

export class Par_FormuleGetInModel {
  public id: number = 0;
}
export class Par_FormuleGetOutModel extends ModelResult {
  public par_Formule: Par_FormuleModel = new Par_FormuleModel();
}

export class Par_FormulePutInModel {
  public par_Formule: Par_FormuleModel = new Par_FormuleModel();
}
export class Par_FormulePutOutModel extends ModelResult {
  public par_Formule: Par_FormuleModel = new Par_FormuleModel();
  public errori: FormulaErrore[] = [];
}

export class Par_Formule_DeleteInModel {
  public id: number = 0;
}
export class Par_Formule_DeleteOutModel extends ModelResult { }

export class Par_Formule_DuplicaInModel {
  public id: number = 0;
  public codice: string = '';
  public titolo: string = '';
}
export class Par_Formule_DuplicaOutModel extends ModelResult {
  public par_Formule: Par_FormuleModel = new Par_FormuleModel();
}

export class Par_Formule_CtrlSintassiInModel {
  public testo: string = '';
}
export class Par_Formule_CtrlSintassiOutModel extends ModelResult {
  public errori: FormulaErrore[] = [];
}

export class Par_Formule_MetodiInModel { }
export class Par_Formule_MetodiOutModel extends ModelResult {
  public metodi: FormulaMetodo[] = [];
  public template: string = '';
}

export class Par_Formule_RapportoProvaModel {
  idDip_RapportoLavoro: number = 0;
  descrizione: string = '';
}
export class Par_Formule_RapportiProvaInModel { }
export class Par_Formule_RapportiProvaOutModel extends ModelResult {
  public rapporti: Par_Formule_RapportoProvaModel[] = [];
}

export class Par_Formule_ProvaInModel {
  public testo: string = '';
  public idDip_RapportoLavoro: number = 0;
  public giorno: string = '';
  public fase: FormulaFase = FormulaFase.Finale;
}
export class Par_Formule_ProvaOutModel extends ModelResult {
  public errori: FormulaErrore[] = [];
  public risultato: FormulaProvaRisultato = new FormulaProvaRisultato();
}

export class Par_Formule_LogInModel {
  public idPar_Formule: number | null = null;
  public dal: string = '';
  public al: string = '';
}
export class Par_Formule_LogRigaModel {
  giorno: string = '';
  dipendente: string = '';
  formula: string = '';
  tipo: FormulaLogTipo = FormulaLogTipo.Info;
  messaggio: string = '';
  data: string = '';
}
export class Par_Formule_LogOutModel extends ModelResult {
  public log: Par_Formule_LogRigaModel[] = [];
}

export class Par_Formule_GeneraAIInModel {
  public richiesta: string = '';
  public testoAttuale: string = '';
}
export class Par_Formule_GeneraAIOutModel extends ModelResult {
  public testo: string = '';
  public spiegazione: string = '';
  public errori: FormulaErrore[] = [];
}
