import { ModelResult } from "../../../ModelsBase/model-result";

export class Dip_Rapporto_Giustificativi_MaturazioneModel {
  public id:                   number = 0;
  public idDip_RapportoLavoro: number = 0;
  public idPar_Giustificativi: number = 0;
  /** Ore maturate al mese, formato "HHH:MM" (può superare 23h). */
  public oreMaturazione:       string = '00:00';
}

export class Dip_Rapporto_Giustificativi_Maturazione_GetAll_InModel {
  public idDip_RapportoLavoro: number = 0;
}

export class Dip_Rapporto_Giustificativi_Maturazione_GetAll_OutModel extends ModelResult {
  public maturazioni: Dip_Rapporto_Giustificativi_MaturazioneModel[] = [];
}

export class Dip_Rapporto_Giustificativi_Maturazione_Upsert_InModel {
  public maturazione: Dip_Rapporto_Giustificativi_MaturazioneModel = new Dip_Rapporto_Giustificativi_MaturazioneModel();
}

export class Dip_Rapporto_Giustificativi_Maturazione_Upsert_OutModel extends ModelResult {
  public maturazione: Dip_Rapporto_Giustificativi_MaturazioneModel | null = null;
}

export class Dip_Rapporto_Giustificativi_Maturazione_Delete_InModel {
  public id: number = 0;
}

export class Dip_Rapporto_Giustificativi_Maturazione_Delete_OutModel extends ModelResult {}
