import { ModelResult } from "../../../ModelsBase/model-result";

export class VacationPlanModel {
  public year: number             = 0;
  public month: number            = 0;
  public selectedUserId: string[] = [];
}

export class VacationPlan_DaySlot {
  public idAspNetUsers: string = '';
  public isPresent: boolean    = false;
}

export class VacationPlan_GetInModel {
  public vacationPlan: VacationPlanModel = new VacationPlanModel();
}

export class VacationPlan_GetOutModel extends ModelResult {
  public daySlots: VacationPlan_DaySlot[] = [];
}
