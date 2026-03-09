import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../Utility/infrastructure/auth.service';
import { Observable, map } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { TimeSheet_CalculateInModel, TimeSheet_CalculateOutModel } from './Models/time-sheet-engine-model';


@Injectable({
  providedIn: 'root'
})
export class TimeSheetEngineService {

  constructor(private http: HttpClient,
    private authService: AuthService
  ) {
  }

  Calculate(model: GenericRequest<TimeSheet_CalculateInModel>): Observable<GenericResult<TimeSheet_CalculateOutModel>> {

    return this.http.post<GenericResult<TimeSheet_CalculateOutModel>>(environment.remoteData.apiUri + 'TimeSheet_Engine/Calculate', model)
      .pipe(
        map(r => {
          return r;
        }
        )
      );

  }



}
