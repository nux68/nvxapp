import { ModelResult } from '../../ModelsBase/model-result';

export class AstEmptyInModel { }

export class AstPermessiModel { public powerAdmin = false; public admin = false; }

// ---------------------------------------------------------------- configurazione
export class AstCfgModel {
  public giorniPreavviso = 30;
  public notificheAttive = true;
  public note: string | null = null;
}

export class AstCfgGetOutModel extends ModelResult { public permessi = new AstPermessiModel(); public cfg = new AstCfgModel(); }
export class AstCfgPutInModel { public cfg = new AstCfgModel(); }
export class AstCfgPutOutModel extends ModelResult { public cfg = new AstCfgModel(); }
