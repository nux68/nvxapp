import { ModelResult } from "../../../ModelsBase/model-result";

// persona di riferimento di un cliente (tabella comune Az_ClienteContatto); al massimo un contatto principale
export class Az_ClienteContattoModel {
  public id = 0;
  public idAz_Cliente = 0;
  public cognome: string | null = null;
  public nome: string | null = null;
  public ruolo: string | null = null;
  public telefono: string | null = null;
  public cellulare: string | null = null;
  public email: string | null = null;
  public principale = false;
  public note: string | null = null;
}

export class Az_ClienteContatto_GetAll_InModel { public idAz_Cliente = 0; }
export class Az_ClienteContatto_GetAll_OutModel extends ModelResult { public az_ClienteContatto: Az_ClienteContattoModel[] = []; }
export class Az_ClienteContattoPutInModel { public az_ClienteContatto: Az_ClienteContattoModel = new Az_ClienteContattoModel(); }
export class Az_ClienteContattoPutOutModel extends ModelResult { public az_ClienteContatto: Az_ClienteContattoModel = new Az_ClienteContattoModel(); }
export class Az_ClienteContattoDeleteInModel { public id = 0; }
export class Az_ClienteContattoDeleteOutModel extends ModelResult { public az_ClienteContatto: Az_ClienteContattoModel = new Az_ClienteContattoModel(); }
