import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../Utility/infrastructure/auth.service';
import { Observable, map } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { Dip_GG_AllData_InModel, Dip_GG_AllData_OutModel, OrariSchema_4User_InModel, OrariSchema_4User_OutModel, Timesheet_AllData_InModel, Timesheet_AllData_OutModel, TimeSheet_CalculateInModel, TimeSheet_CalculateOutModel } from './Models/time-sheet-engine-model';


@Injectable({
  providedIn: 'root'
})
export class TimeSheetEngineService {

  constructor(private http: HttpClient,
    private authService: AuthService
  ) { }

  Calculate(model: GenericRequest<TimeSheet_CalculateInModel>): Observable<GenericResult<TimeSheet_CalculateOutModel>> {
    return this.http.post<GenericResult<TimeSheet_CalculateOutModel>>(environment.remoteData.apiUri + 'TimeSheet_Engine/Calculate', model)
      .pipe(
        map(r => {
          return r;
        })
      );
  }

  Get_OrariSchema_4User(model: GenericRequest<OrariSchema_4User_InModel>): Observable<GenericResult<OrariSchema_4User_OutModel>> {
    return this.http.post<GenericResult<OrariSchema_4User_OutModel>>(environment.remoteData.apiUri + 'TimeSheet_Engine/Get_OrariSchema_4User', model)
      .pipe(
        map(r => {
          return r;
        })
      );
  }

  Dip_GG_AllData_AllData(model: GenericRequest<Dip_GG_AllData_InModel>): Observable<GenericResult<Dip_GG_AllData_OutModel>> {
    return this.http.post<GenericResult<Dip_GG_AllData_OutModel>>(environment.remoteData.apiUri + 'TimeSheet_Engine/Dip_GG_AllData_AllData', model)
      .pipe(
        map(r => {
          return r;
        })
      );
  }

  Get_Timesheet_AllData(model: GenericRequest<Timesheet_AllData_InModel>): Observable<GenericResult<Timesheet_AllData_OutModel>> {
    return this.http.post<GenericResult<Timesheet_AllData_OutModel>>(environment.remoteData.apiUri + 'TimeSheet_Engine/Get_Timesheet_AllData', model)
      .pipe(
        map(r => {
          return r;
        })
      );
  }


}
