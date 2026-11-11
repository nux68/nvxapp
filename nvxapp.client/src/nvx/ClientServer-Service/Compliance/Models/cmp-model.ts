import { ModelResult } from '../../ModelsBase/model-result';

export class CmpEmptyInModel { }

export class CmpPermessiModel { public powerAdmin = false; public admin = false; }

// ---------------------------------------------------------------- configurazione
export class CmpCfgModel {
  public giorniPreavviso = 30;
  public notificheAttive = true;
  public note: string | null = null;
  /** CMP-81.2: utenti abilitati ai dati sanitari delle visite */
  public utentiDatiSanitari: string[] = [];
}

export class CmpUtenteModel { public id = ''; public nome = ''; }

export class CmpCfgGetOutModel extends ModelResult { public permessi = new CmpPermessiModel(); public cfg = new CmpCfgModel(); public utenti: CmpUtenteModel[] = []; }
export class CmpCfgPutInModel { public cfg = new CmpCfgModel(); }
export class CmpCfgPutOutModel extends ModelResult { public cfg = new CmpCfgModel(); }

// ---------------------------------------------------------------- aree (CMP-81, CMP-GDPR): elenco, salvataggio, cancellazione
// idCompany serve solo allo studio (azienda scelta); per gli utenti dell'azienda il server lo ignora
export class CmpListInModel { public idCompany: number | null = null; }
export class CmpDeleteInModel { public id = 0; public idCompany: number | null = null; }
export class CmpPutInModel { public item: any = {}; public idCompany: number | null = null; }
export class CmpListOutModel<T> extends ModelResult { public items: T[] = []; }
export class CmpPutOutModel<T> extends ModelResult { public item!: T; }
export class CmpDeleteOutModel extends ModelResult { }

export class CmpVoceModel { public id = 0; public nome = ''; public attivo = true; }

export class CmpPermessiAreeModel { public powerAdmin = false; public admin = false; public sanitario = false; public studio = false; }

export class CmpLookupOutModel extends ModelResult {
  public permessi = new CmpPermessiAreeModel();
  public dipendenti: CmpVoceModel[] = [];
  public mansioni: CmpVoceModel[] = [];
  public dpi: CmpVoceModel[] = [];
  public sedi: CmpVoceModel[] = [];
  public giorniPreavviso = 30;
}

/** riga generica di una sezione (i campi dipendono dalla sezione, vedi cmp-sezioni.ts) */
export interface CmpRiga {
  id: any;
  scaduta?: boolean;
  inScadenza?: boolean;
  rinnovata?: boolean;
  [key: string]: any;
}

export class CmpMineOutModel extends ModelResult {
  public dipendente = false;
  public formazione: CmpRiga[] = [];
  public visite: CmpRiga[] = [];
  public consegne: CmpRiga[] = [];
  public nomine: CmpRiga[] = [];
}

export class CmpGdprExportOutModel extends ModelResult { public jobId = ''; }

export class CmpStudioAziendaModel { public idCompany = 0; public descrizione = ''; public scadenzeAperte = 0; public scadenzeScadute = 0; }
