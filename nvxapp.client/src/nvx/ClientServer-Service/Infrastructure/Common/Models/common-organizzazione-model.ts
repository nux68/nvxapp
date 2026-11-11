import { ModelResult } from "../../../ModelsBase/model-result";

// sede dell'azienda (tabella comune Az_Sedi) con i reparti di primo livello
export class Az_SediCommonModel {
  public id!: number;
  public idAz_Anagrafica!: number;
  public descrizione: string = '';
  public default: boolean = false;
  // reparti senza padre; i sottoreparti stanno in az_SediReparto_Sub
  public az_SediReparto: Az_SediRepartoCommonModel[] = [];
}

// reparto di una sede (tabella comune Az_SediReparto), ad albero
export class Az_SediRepartoCommonModel {
  public id!: number;
  public idAz_Sedi!: number;
  public idAz_SediReparto: number | null = null; // reparto padre
  public descrizione: string = '';
  public default: boolean = false;
  public az_SediReparto_Sub: Az_SediRepartoCommonModel[] = [];
}

// competenza definita dall'azienda (tabella comune Par_Competenza)
export class Par_CompetenzaCommonModel {
  public id!: number;
  public idAz_Anagrafica!: number;
  public descrizione: string = '';
  public default: boolean = false;
}

export class CommonOrganizzazione_SediGetAll_InModel {}

export class CommonOrganizzazione_SediGetAll_OutModel extends ModelResult {
  public az_Sedi: Az_SediCommonModel[] = [];
}

export class CommonOrganizzazione_CompetenzeGetAll_InModel {}

export class CommonOrganizzazione_CompetenzeGetAll_OutModel extends ModelResult {
  public par_Competenza: Par_CompetenzaCommonModel[] = [];
}
