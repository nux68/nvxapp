import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../Utility/infrastructure/auth.service';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { map, Observable } from 'rxjs';
import { Dip_Competenza_GetAll_InModel, Dip_Competenza_GetAll_OutModel } from './Models/dip-competenza-model';

@Injectable({
  providedIn: 'root'
})
export class DipCompetenzaService {
  constructor(private http: HttpClient, private authService: AuthService) { }

  GetAll(model: GenericRequest<Dip_Competenza_GetAll_InModel>): Observable<GenericResult<Dip_Competenza_GetAll_OutModel>> {
    return this.http.post<GenericResult<Dip_Competenza_GetAll_OutModel>>(environment.remoteData.apiUri + 'Dip_Competenza/GetAll', model)
      .pipe(map(r => r));
  }
}
