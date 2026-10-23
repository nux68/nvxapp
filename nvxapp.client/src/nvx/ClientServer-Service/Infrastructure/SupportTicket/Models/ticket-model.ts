import { ModelResult } from "../../../ModelsBase/model-result";
import { ApplicationType } from "../../Account/Models/user-load-model";

// Livelli dei ticket (stessi valori dell'enum TicketLevel del server): un livello "sopra" ha valore piu' basso
export enum TicketLevel {
  SuperUser = 0,
  Dev = 1,
  Dealer = 2,
  FinancialAdvisor = 3,
  Company = 4,
  User = 5,
}

// Stato del ticket (enum TicketState del server)
export enum TicketState {
  Aperto = 0,
  InCarico = 1,
  InAttesaInfo = 2,
  Risolto = 3,
  Chiuso = 4,
}

// Passaggi della cronologia (enum TicketAction del server)
export enum TicketAction {
  Apertura = 0,
  Assegnazione = 1,
  Inoltro = 2,
  RichiestaInfo = 3,
  RispostaInfo = 4,
  Nota = 5,
  Risoluzione = 6,
  RisoluzioneAI = 7,
  Riapertura = 8,
  Chiusura = 9,
  Riclassificazione = 10,
  Spostamento = 11,
}

// Viste dell'elenco
export enum TicketView {
  InCarico = 0,
  Aperti = 1,
  Altri = 2,
}

export class TicketListModel {
  public id: number = 0;
  public idTicketType: number = 0;
  public tipoDescrizione: string = '';
  public oggetto: string = '';
  public state: TicketState = TicketState.Aperto;
  public openerLevel: TicketLevel = TicketLevel.User;
  public idAspNetUsersOpener: string = '';
  public openerUserName: string = '';
  public currentLevel: TicketLevel = TicketLevel.User;
  public currentIdAspNetUsers: string | null = null;
  public currentUserName: string | null = null;
  public creationDate: string | null = null;
  public modifiedDate: string | null = null;
  public version: number = 0;
}

// allegato: file (base64) valorizzato solo in invio e nel download
export class TicketAttachModel {
  public id: number = 0;
  public fileName: string = '';
  public contentType: string = '';
  public size: number = 0;
  public file: string | null = null;
}

export class TicketCronologyModel {
  public id: number = 0;
  public level: TicketLevel = TicketLevel.User;
  public userId: string | null = null;
  public userName: string | null = null;
  public idAspNetUsersAuthor: string | null = null;
  public authorName: string | null = null;
  public action: TicketAction = TicketAction.Apertura;
  public state: TicketState = TicketState.Aperto;
  public text: string | null = null;
  public returnLevel: TicketLevel | null = null;
  public returnUserName: string | null = null;
  public idTicketType: number | null = null;
  public tipoDescrizione: string | null = null;
  public creationDate: string | null = null;
  public ticketAttach: TicketAttachModel[] = [];
}

export class TicketUserModel {
  public idAspNetUsers: string = '';
  public userName: string = '';
}

// destinatario di una richiesta di informazioni
export class TicketInfoTargetModel {
  public level: TicketLevel = TicketLevel.User;
  public isOpener: boolean = false;
  public idAspNetUsers: string | null = null;
  public userName: string | null = null;
}

export class TicketTypeOptionModel {
  public id: number = 0;
  public applicationType: ApplicationType | null = null;
  public descrizione: string = '';
}

export class TicketOpenerRoleModel {
  public level: TicketLevel = TicketLevel.User;
  public ticketType: TicketTypeOptionModel[] = [];
}

// azioni consentite all'utente corrente
export class TicketActionsModel {
  public assegna: boolean = false;
  public inoltra: boolean = false;
  public richiediInfo: boolean = false;
  public rispondi: boolean = false;
  public nota: boolean = false;
  public risolvi: boolean = false;
  public riapri: boolean = false;
  public chiudi: boolean = false;
  public riclassifica: boolean = false;
  public inoltraLevel: TicketLevel | null = null;
  public assegnatario: TicketUserModel[] = [];
  public infoTarget: TicketInfoTargetModel[] = [];
  public ticketType: TicketTypeOptionModel[] = [];
}

export class TicketDetailModel extends TicketListModel {
  public text: string = '';
  public ticketCronology: TicketCronologyModel[] = [];
  public actions: TicketActionsModel = new TicketActionsModel();
}


export class TicketListInModel {
  public view: TicketView = TicketView.InCarico;
  public state: TicketState | null = null;
}

export class TicketListOutModel extends ModelResult {
  public ticket: TicketListModel[] = [];
}


export class TicketCountInModel {
}

export class TicketCountOutModel extends ModelResult {
  public inCarico: number = 0;
}


export class TicketGetInModel {
  public id: number = 0;
}

export class TicketGetOutModel extends ModelResult {
  public ticket: TicketDetailModel = new TicketDetailModel();
}


export class TicketOpenInfoInModel {
}

export class TicketOpenInfoOutModel extends ModelResult {
  public openerRole: TicketOpenerRoleModel[] = [];
  public assegnatario: TicketUserModel[] = [];
}


export class TicketOpenModel {
  public openerLevel: TicketLevel | null = null;
  public idTicketType: number | null = null;
  public oggetto: string = '';
  public text: string = '';
  public idAspNetUsersAssignee: string | null = null;
  public ticketAttach: TicketAttachModel[] = [];
}

export class TicketOpenInModel {
  public ticket: TicketOpenModel = new TicketOpenModel();
}

export class TicketOpenOutModel extends ModelResult {
  public id: number = 0;
}


// passaggio sul ticket (vedi TicketActionInModel del server)
export class TicketActionInModel {
  public id: number = 0;
  public version: number = 0;
  public action: TicketAction = TicketAction.Nota;
  public text: string | null = null;
  public idAspNetUsers: string | null = null;
  public infoLevel: TicketLevel | null = null;
  public infoToOpener: boolean = false;
  public infoToLevel: boolean = false;
  public idTicketType: number | null = null;
  public ticketAttach: TicketAttachModel[] = [];
}

export class TicketActionOutModel extends ModelResult {
  public ticket: TicketDetailModel = new TicketDetailModel();
}


export class TicketAttachGetInModel {
  public id: number = 0;
}

export class TicketAttachGetOutModel extends ModelResult {
  public ticketAttach: TicketAttachModel = new TicketAttachModel();
}


// notifica push (evento SignalR "TicketNotifier")
export class TicketNotifierData {
  public idTicket: number = 0;
  public oggetto: string = '';
  public action: TicketAction = TicketAction.Apertura;
  public state: TicketState = TicketState.Aperto;
}
