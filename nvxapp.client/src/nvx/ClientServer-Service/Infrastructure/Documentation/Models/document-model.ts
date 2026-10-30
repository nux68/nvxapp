import { ModelResult } from "../../../ModelsBase/model-result";
import { ApplicationType } from "../../Account/Models/user-load-model";
import { TicketLevel, TicketTypeOptionModel } from "../../SupportTicket/Models/ticket-model";

// uso del documento (enum DocumentSection del server)
export enum DocumentSection {
  TicketAi = 0,
}

// livello gestito dall'utente, con i tipi di richiesta proponibili
export class DocumentScopeModel {
  public level: TicketLevel = TicketLevel.SuperUser;
  public canEdit: boolean = false;
  public ticketType: TicketTypeOptionModel[] = [];
}

export class DocumentListModel {
  public id: number = 0;
  public titolo: string = '';
  public descrizione: string | null = null;
  public fileName: string | null = null;
  public applicationType: ApplicationType | null = null;
  public includeSubLevels: boolean = false;
  public ordine: number = 0;
  public active: boolean = true;
  public contenutoLength: number = 0;
  public idTicketType: number[] = [];
  public modifiedDate: string | null = null;
}

// file (base64) solo quando se ne carica uno nuovo; removeFile toglie quello salvato
export class DocumentEditModel {
  public id: number = 0;
  public section: DocumentSection = DocumentSection.TicketAi;
  public level: TicketLevel = TicketLevel.SuperUser;
  public titolo: string = '';
  public descrizione: string | null = null;
  public contenuto: string = '';
  public fileName: string | null = null;
  public contentType: string | null = null;
  public size: number = 0;
  public file: string | null = null;
  public removeFile: boolean = false;
  public applicationType: ApplicationType | null = null;
  public includeSubLevels: boolean = false;
  public ordine: number = 0;
  public active: boolean = true;
  public idTicketType: number[] = [];
  public canEdit: boolean = true;
}


export class DocumentScopeInModel {
  public section: DocumentSection = DocumentSection.TicketAi;
}

export class DocumentScopeOutModel extends ModelResult {
  public scope: DocumentScopeModel[] = [];
}


export class DocumentListInModel {
  public section: DocumentSection = DocumentSection.TicketAi;
  public level: TicketLevel = TicketLevel.SuperUser;
}

export class DocumentListOutModel extends ModelResult {
  public document: DocumentListModel[] = [];
}


export class DocumentGetInModel {
  public id: number = 0;
}

export class DocumentGetOutModel extends ModelResult {
  public document: DocumentEditModel = new DocumentEditModel();
}


export class DocumentPutInModel {
  public document: DocumentEditModel = new DocumentEditModel();
}

export class DocumentPutOutModel extends ModelResult {
  public document: DocumentEditModel = new DocumentEditModel();
}


export class DocumentDeleteInModel {
  public id: number = 0;
}

export class DocumentDeleteOutModel extends ModelResult {
}


export class DocumentFileTextInModel {
  public fileName: string = '';
  public file: string | null = null;
}

export class DocumentFileTextOutModel extends ModelResult {
  public text: string = '';
}


export class DocumentFileGetInModel {
  public id: number = 0;
}

export class DocumentFileGetOutModel extends ModelResult {
  public fileName: string = '';
  public contentType: string = '';
  public file: string | null = null;
}
