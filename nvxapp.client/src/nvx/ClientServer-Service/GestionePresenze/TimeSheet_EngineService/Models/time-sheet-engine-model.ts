import { ModelResult } from "../../../ModelsBase/model-result";

export class TimeSheet_CalculateModel {
  id: number;
  numCoppia: number;
}


export class TimeSheet_CalculateInModel {
  timeSheet_Calculate: TimeSheet_CalculateModel; 
}
export class TimeSheet_CalculateOutModel extends ModelResult {
  timeSheet_Calculate: TimeSheet_CalculateModel;
}


