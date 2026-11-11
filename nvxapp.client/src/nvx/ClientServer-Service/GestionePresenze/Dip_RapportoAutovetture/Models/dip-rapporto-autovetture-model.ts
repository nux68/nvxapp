import { ModelResult } from "../../../ModelsBase/model-result";

// autovettura del dipendente per le spese di rimborso chilometrico (tabella comune, AT-COM: prima era del rapporto di lavoro)
export class Dip_RapportoAutovettureModel {
  id: number = 0;
  idDip_Anagrafica: number = 0;
  descrizione: string = '';
  targa: string | null = null;
  rimborsoKM: number = 0;
}

// autovetture del dipendente corrente (nota spese)
export class Dip_RapportoAutovetture_GetAll4User_InModel {
}
export class Dip_RapportoAutovetture_GetAll4User_OutModel extends ModelResult {
  public dip_RapportoAutovetture: Dip_RapportoAutovettureModel[] = [];
}

/** "Descrizione (TARGA)" */
export function autovetturaTesto(descrizione: string | null | undefined, targa: string | null | undefined): string {
  return targa ? `${descrizione ?? ''} (${targa})` : (descrizione ?? '');
}
