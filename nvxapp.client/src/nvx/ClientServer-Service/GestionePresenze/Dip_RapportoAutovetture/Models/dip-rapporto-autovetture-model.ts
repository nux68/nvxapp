import { ModelResult } from "../../../ModelsBase/model-result";

// autovettura del rapporto di lavoro per le spese di rimborso chilometrico
export class Dip_RapportoAutovettureModel {
  id: number = 0;
  idDip_RapportoLavoro: number = 0;
  descrizione: string = '';
  targa: string | null = null;
  rimborsoKM: number = 0;
}

// autovetture del rapporto di lavoro dell'utente corrente (nota spese)
export class Dip_RapportoAutovetture_GetAll4User_InModel {
}
export class Dip_RapportoAutovetture_GetAll4User_OutModel extends ModelResult {
  public dip_RapportoAutovetture: Dip_RapportoAutovettureModel[] = [];
}

/** "Descrizione (TARGA)" */
export function autovetturaTesto(descrizione: string | null | undefined, targa: string | null | undefined): string {
  return targa ? `${descrizione ?? ''} (${targa})` : (descrizione ?? '');
}
