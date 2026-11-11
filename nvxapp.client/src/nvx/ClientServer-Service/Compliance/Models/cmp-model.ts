import { ModelResult } from '../../ModelsBase/model-result';

export class CmpEmptyInModel { }

export class CmpPermessiModel { public powerAdmin = false; public admin = false; }

// ---------------------------------------------------------------- configurazione
export class CmpCfgModel {
  public giorniPreavviso = 30;
  public notificheAttive = true;
  public note: string | null = null;
}

export class CmpCfgGetOutModel extends ModelResult { public permessi = new CmpPermessiModel(); public cfg = new CmpCfgModel(); }
export class CmpCfgPutInModel { public cfg = new CmpCfgModel(); }
export class CmpCfgPutOutModel extends ModelResult { public cfg = new CmpCfgModel(); }
