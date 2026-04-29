import { ModelResult } from "../../../ModelsBase/model-result";

import { ModelResult } from "../../../ModelsBase/model-result";

export class VacationPlanModel {
  public year: number             = 0;
  public month: number            = 0;
  public dal: string              = '';
  public al: string               = '';
  public selectedUserId: string[] = [];
}

export class VacationPlan_GetInModel {
  public vacationPlan: VacationPlanModel = new VacationPlanModel();
}

export class VacationPlan_GetOutModel extends ModelResult {
  public vacationPlan: VacationPlanModel[] = [];
}
