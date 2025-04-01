import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../Utility/auth.service';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { map, Observable } from 'rxjs';
import { Az_RepartoInModel, Az_RepartoOutModel } from './Models/az-reparto-model';

@Injectable({
  providedIn: 'root'
})
export class AzRepartoService {

  constructor(private http: HttpClient,
    private authService: AuthService
  ) { }

  GetAll(model: GenericRequest<Az_RepartoInModel>): Observable<GenericResult<Az_RepartoOutModel>> {

    return this.http.post<GenericResult<Az_RepartoOutModel>>(environment.remoteData.apiUri + 'Az_Reparto/GetAll', model)
      .pipe(
        map(r => {
          return r;
        }
        )
      );

  }

}
