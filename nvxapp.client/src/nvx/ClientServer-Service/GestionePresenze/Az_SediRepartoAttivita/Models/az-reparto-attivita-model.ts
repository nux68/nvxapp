import { ModelResult } from "../../../ModelsBase/model-result";

export class Az_RepartoAttivitaModel {
  public id!: number;
  public idAz_SediReparto!: number;
  public idAz_SediAttivita!: number;
}

export class Az_RepartoAttivitaInModel {}

export class Az_RepartoAttivitaOutModel extends ModelResult {
  public az_SediRepartoAttivita: Az_RepartoAttivitaModel[] = [];
}

