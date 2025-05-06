import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../Utility/infrastructure/auth.service';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { map, Observable } from 'rxjs';
import { Az_SediRepartoInModel, Az_SediRepartoOutModel } from './Models/az-sedi-reparto-model';

@Injectable({
  providedIn: 'root'
})
export class AzSediRepartoService {

  constructor(private http: HttpClient,
    private authService: AuthService
  ) { }

  GetAll(model: GenericRequest<Az_SediRepartoInModel>): Observable<GenericResult<Az_SediRepartoOutModel>> {

    return this.http.post<GenericResult<Az_SediRepartoOutModel>>(environment.remoteData.apiUri + 'Az_SediReparto/GetAll', model)
      .pipe(
        map(r => {
          return r;
        }
        )
      );

  }

}
