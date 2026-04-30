import { ModelResult } from "../../../ModelsBase/model-result";

export class PresentStaffModel {
  public year: number             = 0;
  public month: number            = 0;
  public selectedUserId: string[] = [];
}

export class PresentStaff_GetInModel {
  public presentStaff: PresentStaffModel = new PresentStaffModel();
}

export class PresentStaff_DaySlot {
  public idAspNetUsers: string = '';
  public isPresent: boolean;
}

export class PresentStaff_GetOutModel extends ModelResult {
  public daySlots: PresentStaff_DaySlot[] = [];
}
