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
