import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../Utility/infrastructure/auth.service';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { map, Observable } from 'rxjs';
import {
  ActivityStatistics_GetInModel,
  ActivityStatistics_GetOutModel
} from './Models/activity-statistics-model';

@Injectable({
  providedIn: 'root'
})
export class ActivityStatisticsService {

  constructor(
    private http: HttpClient,
    private authService: AuthService
  ) { }

  ActivityStatisticsGet(model: GenericRequest<ActivityStatistics_GetInModel>): Observable<GenericResult<ActivityStatistics_GetOutModel>> {
    return this.http.post<GenericResult<ActivityStatistics_GetOutModel>>(
      environment.remoteData.apiUri + 'ActivityStatistics/ActivityStatisticsGet', model
    ).pipe(map(r => r));
  }

}
