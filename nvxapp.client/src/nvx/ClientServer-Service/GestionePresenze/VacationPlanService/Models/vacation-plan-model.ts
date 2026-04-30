import { ModelResult } from "../../../ModelsBase/model-result";
import { Dip_GG_GiustificativiModel } from "../../Dip_GG_Giustificativi/Models/dip-gg-giustificativi-model";
import { Dip_GG_RichiestaModel } from "../../Dip_GG_Richiesta/Models/dip-gg-richiesta-model";

export class VacationPlanModel {
  public year: number             = 0;
  public month: number            = 0;
  public selectedUserId: string[] = [];
}

export class VacationPlan_DaySlot {
  public idAspNetUsers: string = '';
  public dip_GG_Richieste: Dip_GG_RichiestaModel[] = [];
  public dip_GG_Giustificativi: Dip_GG_GiustificativiModel[] = [];
}

export class VacationPlan_GetInModel {
  public vacationPlan: VacationPlanModel = new VacationPlanModel();
}

export class VacationPlan_GetOutModel extends ModelResult {
  public daySlots: VacationPlan_DaySlot[] = [];
}
