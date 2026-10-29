import { ModelResult } from "../../../ModelsBase/model-result";

export class TimeSheet_ExportModel {
  year: number     = 0;
  month: number    = 0;
  selectedUserId: string[] = [];
  dal: string      = '';
  al: string       = '';
  idPar_ExportCau: number = 0;
}

export class TimeSheet_ExportInModel {
  timeSheet_Export: TimeSheet_ExportModel;
  constructor() {
    this.timeSheet_Export = new TimeSheet_ExportModel();
  }
}

export class TimeSheet_ExportOutModel extends ModelResult {
  timeSheet_Export: TimeSheet_ExportModel;
  constructor() {
    super();
    this.timeSheet_Export = new TimeSheet_ExportModel();
  }
}

// cartellino PDF dell'utente collegato: solo il mese, l'utente lo decide il server
export class TimeSheet_ExportPdf_4User_Model {
  year: number  = 0;
  month: number = 0;   // 1-12
}

export class TimeSheet_ExportPdf_4User_InModel {
  timeSheet_ExportPdf: TimeSheet_ExportPdf_4User_Model;
  constructor() {
    this.timeSheet_ExportPdf = new TimeSheet_ExportPdf_4User_Model();
  }
}

export class TimeSheet_ExportPdf_4User_OutModel extends ModelResult {
  jobId: string | null = null;
}

// cartellini PDF dei dipendenti scelti dall'admin (il server tiene solo quelli che l'admin puo' vedere)
export class TimeSheet_ExportPdf_4Admin_Model {
  year: number  = 0;
  month: number = 0;   // 1-12
  selectedUserId: string[] = [];
}

export class TimeSheet_ExportPdf_4Admin_InModel {
  timeSheet_ExportPdf: TimeSheet_ExportPdf_4Admin_Model;
  constructor() {
    this.timeSheet_ExportPdf = new TimeSheet_ExportPdf_4Admin_Model();
  }
}

export class TimeSheet_ExportPdf_4Admin_OutModel extends ModelResult {
  jobId: string | null = null;
}

// anteprima del file del modello di export su un dipendente (pagina del modello)
export class TimeSheet_Export_Anteprima_InModel {
  idPar_ExportCau: number = 0;
  dal: string = '';
  al: string = '';
  idAspNetUsers: string | null = null;
  /** chiede una ricetta nuova all'AI anche se documenti e note non sono cambiati */
  rigenera: boolean = false;
}

export class TimeSheet_Export_Anteprima_OutModel extends ModelResult {
  /** prime righe del file, o il JSON dei dati senza documentazione */
  anteprima: string = '';
  estensione: string = '';
  /** ricetta usata (JSON); null senza documentazione */
  ricetta: string | null = null;
  avvisi: string[] = [];
}
