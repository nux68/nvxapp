import { ModelResult } from "../../../ModelsBase/model-result";

export class ActivityStatisticsModel {
  public year: number             = 0;
  public month: number            = 0;
  //public dal: string              = '';
  //public al: string               = '';
  public selectedUserId: string[] = [];
}

export class ActivityStatistics_GetInModel {
  public activityStatistics: ActivityStatisticsModel = new ActivityStatisticsModel();
}

export class ActivityStatistics_GetOutModel extends ModelResult {
  public activityStatistics: ActivityStatisticsModel[] = [];
}
