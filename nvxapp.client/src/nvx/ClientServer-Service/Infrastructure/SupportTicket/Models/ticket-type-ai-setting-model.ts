import { ModelResult } from "../../../ModelsBase/model-result";
import { ApplicationType } from "../../Account/Models/user-load-model";
import { TicketLevel } from "./ticket-model";

// isDefault: SuperUser, valori di default del livello; altrimenti personalizzazione del nodo dell'utente
export class TicketTypeAiSettingScopeModel {
  public level: TicketLevel = TicketLevel.Company;
  public isDefault: boolean = false;
  public canEdit: boolean = false;
}

// custom = il nodo ha valori propri (false = usa i default del SuperUser)
export class TicketTypeAiSettingModel {
  public idTicketType: number = 0;
  public codice: string = '';
  public descrizione: string = '';
  public applicationType: ApplicationType | null = null;
  public custom: boolean = false;
  public aiEnabled: boolean = false;
  public aiMaxAttempts: number = 1;
  public defaultAiEnabled: boolean = false;
  public defaultAiMaxAttempts: number = 1;
  // la risposta AI usa anche la documentazione dei livelli sopra (false: solo quella del nodo)
  public useParentDocuments: boolean = true;
  public defaultUseParentDocuments: boolean = true;
}


export class TicketTypeAiSettingScopeInModel {
}

export class TicketTypeAiSettingScopeOutModel extends ModelResult {
  public scope: TicketTypeAiSettingScopeModel[] = [];
  public maxAttempts: number = 10;
}


export class TicketTypeAiSettingListInModel {
  public level: TicketLevel = TicketLevel.Company;
}

export class TicketTypeAiSettingListOutModel extends ModelResult {
  public ticketTypeAiSetting: TicketTypeAiSettingModel[] = [];
}


export class TicketTypeAiSettingPutInModel {
  public level: TicketLevel = TicketLevel.Company;
  public ticketTypeAiSetting: TicketTypeAiSettingModel[] = [];
}

export class TicketTypeAiSettingPutOutModel extends ModelResult {
  public ticketTypeAiSetting: TicketTypeAiSettingModel[] = [];
}
