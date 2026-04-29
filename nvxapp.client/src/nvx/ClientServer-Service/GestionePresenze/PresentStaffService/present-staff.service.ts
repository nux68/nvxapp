import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../Utility/infrastructure/auth.service';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { map, Observable } from 'rxjs';
import {
  PresentStaff_GetInModel,
  PresentStaff_GetOutModel
} from './Models/present-staff-model';

@Injectable({
  providedIn: 'root'
})
export class PresentStaffService {

  constructor(
    private http: HttpClient,
    private authService: AuthService
  ) { }

  PresentStaffGet(model: GenericRequest<PresentStaff_GetInModel>): Observable<GenericResult<PresentStaff_GetOutModel>> {
    return this.http.post<GenericResult<PresentStaff_GetOutModel>>(
      environment.remoteData.apiUri + 'PresentStaff/PresentStaffGet', model
    ).pipe(map(r => r));
  }

}
