import { ModelResult } from '../../ModelsBase/model-result';

export class HrEmptyInModel { }

export class HrPermessiModel { public powerAdmin = false; public admin = false; }

// ---------------------------------------------------------------- configurazione
export class HrCfgModel {
  public giorniPreavviso = 30;
  public notificheAttive = true;
  public note: string | null = null;
}

export class HrCfgGetOutModel extends ModelResult { public permessi = new HrPermessiModel(); public cfg = new HrCfgModel(); }
export class HrCfgPutInModel { public cfg = new HrCfgModel(); }
export class HrCfgPutOutModel extends ModelResult { public cfg = new HrCfgModel(); }
