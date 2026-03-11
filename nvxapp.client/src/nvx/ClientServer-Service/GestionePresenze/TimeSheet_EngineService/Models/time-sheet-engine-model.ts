import { ModelResult } from "../../../ModelsBase/model-result";

export class TimeSheet_CalculateModel {
  year: number;
  month: number;
  selectedUserId: string[] | null = null
  dal: string
  al: string;
  approva_Richieste_Timbrature: boolean
  approva_Richieste_Giustificativo: boolean
  genera_Timbrature_Mancanti: boolean
}


export class TimeSheet_CalculateInModel {
  timeSheet_Calculate: TimeSheet_CalculateModel;
  constructor() {
    this.timeSheet_Calculate = new TimeSheet_CalculateModel();
  }
}
export class TimeSheet_CalculateOutModel extends ModelResult {
  timeSheet_Calculate: TimeSheet_CalculateModel;
  
}


