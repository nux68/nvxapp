import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../Utility/auth.service';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { map, Observable } from 'rxjs';
import { Az_RepartoAttivitaInModel, Az_RepartoAttivitaOutModel } from './Models/az-reparto-attivita-model';

@Injectable({
  providedIn: 'root'
})
export class AzRepartoAttivitaService {

  constructor(private http: HttpClient,
    private authService: AuthService
  ) { }

  GetAll(model: GenericRequest<Az_RepartoAttivitaInModel>): Observable<GenericResult<Az_RepartoAttivitaOutModel>> {

    return this.http.post<GenericResult<Az_RepartoAttivitaOutModel>>(environment.remoteData.apiUri + 'Az_RepartoAttivita/GetAll', model)
      .pipe(
        map(r => {
          return r;
        }
        )
      );

  }

}
