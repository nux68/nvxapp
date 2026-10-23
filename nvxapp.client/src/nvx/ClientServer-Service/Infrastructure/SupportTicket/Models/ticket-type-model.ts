import { ModelResult } from "../../../ModelsBase/model-result";
import { ApplicationType } from "../../Account/Models/user-load-model";
import { TicketLevel } from "./ticket-model";

export class TicketTypeListModel {
  public id: number = 0;
  public applicationType: ApplicationType | null = null;
  public codice: string = '';
  public descrizione: string = '';
  public aiResolution: boolean = false;
  public ordine: number = 0;
  public active: boolean = true;
  public level: TicketLevel[] = [];
}

export class TicketTypeEditModel {
  public id: number = 0;
  public applicationType: ApplicationType | null = null;
  public codice: string = '';
  public descrizione: string = '';
  public aiResolution: boolean = false;
  public ordine: number = 0;
  public active: boolean = true;
  public level: TicketLevel[] = [];
}


export class TicketTypeListInModel {
}

export class TicketTypeListOutModel extends ModelResult {
  public ticketType: TicketTypeListModel[] = [];
}


export class TicketTypeGetInModel {
  public id: number = 0;
}

export class TicketTypeGetOutModel extends ModelResult {
  public ticketType: TicketTypeEditModel = new TicketTypeEditModel();
}


export class TicketTypePutInModel {
  public ticketType: TicketTypeEditModel = new TicketTypeEditModel();
}

export class TicketTypePutOutModel extends ModelResult {
  public ticketType: TicketTypeEditModel = new TicketTypeEditModel();
}
