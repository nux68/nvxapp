import { ModelResult } from "../../../ModelsBase/model-result";
import { Dip_GG_TimbraturaModel } from "../../Dip_GG_Timbratura/Models/dip-gg-timbratura-model";

export class PresentStaffModel {
  //public year: number             = 0;
  //public month: number            = 0;
  public selectedUserId: string[] = [];
}

export class PresentStaff_GetInModel {
  public presentStaff: PresentStaffModel = new PresentStaffModel();
}

export class PresentStaff_DaySlot {
  public idAspNetUsers: string = '';
  public isPresent: boolean;
  public dip_GG_Timbratura?: Dip_GG_TimbraturaModel;
}

export class PresentStaff_GetOutModel extends ModelResult {
  public daySlots: PresentStaff_DaySlot[] = [];
}
