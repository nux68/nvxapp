import { GenericResult } from "../../../ModelsBase/generic-result";
import { ModelResult } from "../../../ModelsBase/model-result";

export class Az_ClienteModel {
  public id!: number;
  public idAz_Anagrafica!: number;
  public descrizione: string = ''; // ragione sociale
  public default: boolean;

  public partitaIva: string | null = null;
  public codiceFiscale: string | null = null;

  public indirizzo: string | null = null;
  public cap: string | null = null;
  public citta: string | null = null;
  public provincia: string | null = null;
  public nazione: string | null = 'IT';

  public telefono: string | null = null;
  public email: string | null = null;
  public pec: string | null = null;
  public referente: string | null = null;

  public codiceDestinatario: string | null = null;

  public note: string | null = null;
}

export class Az_Cliente_GetAll_InModel {}

export class Az_Cliente_GetAll_OutModel extends ModelResult {
  public az_Cliente: Az_ClienteModel[] = [];
}

export class Az_ClienteGetInModel {
  id: number = 0;
}
export class Az_ClienteGetOutModel extends ModelResult {
  az_Cliente!: Az_ClienteModel;
}

export class Az_ClientePutInModel {
  az_Cliente!: Az_ClienteModel;
}
export class Az_ClientePutOutModel extends ModelResult {
  az_Cliente!: Az_ClienteModel;
}

export class Az_ClienteDeleteInModel {
  public id: number = 0;
}
export class Az_ClienteDeleteOutModel extends ModelResult {
  public az_Cliente: Az_ClienteModel = new Az_ClienteModel();
}
