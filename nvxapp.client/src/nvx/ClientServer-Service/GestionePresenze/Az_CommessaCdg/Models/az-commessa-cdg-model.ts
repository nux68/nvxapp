import { ModelResult } from "../../../ModelsBase/model-result";

// controllo di gestione sulle commesse (ANALISI_ATTENDANCETRACKING_CONTROLLO_GESTIONE.md)

/** costo orario di un dipendente impostato nella pagina (non salvato sul server) */
export class CdgCostoDipendente {
  public idAspNetUsers = '';
  public costoOrario = 0;
}

/** ricavo di una sottocommessa impostato nella pagina (non salvato sul server) */
export class CdgRicavo {
  public idAz_SubCommessa = 0;
  public ricavo = 0;
}

export class CdgParametri {
  public year = 0;
  public month = 0;
  public yearAl = 0;
  public monthAl = 0;
  public costoOrarioPredefinito: number | null = null;
  public costiDipendenti: CdgCostoDipendente[] = [];
  public ricavi: CdgRicavo[] = [];
  public idsCommessa: number[] = [];
}

export class Az_CommessaCdg_GetInModel {
  public parametri: CdgParametri = new CdgParametri();
}

export class CdgValori {
  public minuti = 0;
  public costoPersonale = 0;
  public noteSpese = 0;
  public costo = 0;
  public ricavo: number | null = null;
  public margine: number | null = null;
  public marginePerc: number | null = null;
}

export class CdgRigaDipendente {
  public idAspNetUsers = '';
  public nomeDipendente = '';
  public minuti = 0;
  public costoOrario = 0;
  public costoPersonale = 0;
  public noteSpese = 0;
}

export class CdgSubCommessaModel extends CdgValori {
  public idAz_SubCommessa = 0;
  public descrizione = '';
  public dipendenti: CdgRigaDipendente[] = [];
}

export class CdgCommessaModel extends CdgValori {
  public idAz_Commessa = 0;
  public descrizione = '';
  public cliente = '';
  public subCommesse: CdgSubCommessaModel[] = [];
}

export class CdgDipendenteModel {
  public idAspNetUsers = '';
  public nomeDipendente = '';
  public minuti = 0;
  public noteSpese = 0;
  public costoOrario = 0;
  public daPredefinito = false;
}

export class CdgRisultato {
  public dal = '';
  public al = '';
  public costoOrarioPredefinito = 0;
  public commesse: CdgCommessaModel[] = [];
  public totale: CdgValori = new CdgValori();
  public dipendenti: CdgDipendenteModel[] = [];
  public giornateEscluse = 0;
  public noteSpeseNonAttribuite = 0;
  public dipendentiSenzaCosto = 0;
}

export class Az_CommessaCdg_GetOutModel extends ModelResult {
  public risultato: CdgRisultato;
}

export class Widget_OreCommesseGetInModel {
  public costoOrarioPredefinito: number | null = null;
}

export class Widget_OreCommesseGetOutModel extends ModelResult {
  public year = 0;
  public month = 0;
  public minutiTotali = 0;
  public costoTotale: number | null = null;
  public commesse: CdgCommessaModel[] = [];
}
