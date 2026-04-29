import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../Utility/infrastructure/auth.service';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { map, Observable } from 'rxjs';
import {
  VacationPlan_GetInModel,
  VacationPlan_GetOutModel
} from './Models/vacation-plan-model';

@Injectable({
  providedIn: 'root'
})
export class VacationPlanService {

  constructor(
    private http: HttpClient,
    private authService: AuthService
  ) { }

  VacationPlanGet(model: GenericRequest<VacationPlan_GetInModel>): Observable<GenericResult<VacationPlan_GetOutModel>> {
    return this.http.post<GenericResult<VacationPlan_GetOutModel>>(
      environment.remoteData.apiUri + 'VacationPlan/VacationPlanGet', model
    ).pipe(map(r => r));
  }

}
