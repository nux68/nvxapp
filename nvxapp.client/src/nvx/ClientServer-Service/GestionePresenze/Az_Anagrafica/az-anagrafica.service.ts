import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../Utility/auth.service';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { map, Observable } from 'rxjs';
import { Az_AnagraficaInModel, Az_AnagraficaOutModel } from './Models/az-anagrafica-model';


@Injectable({
  providedIn: 'root'
})
export class AzAnagraficaService {

  constructor(private http: HttpClient,
              private authService: AuthService
  ) { }

  GetAll(model: GenericRequest<Az_AnagraficaInModel>): Observable<GenericResult<Az_AnagraficaOutModel>> {

    return this.http.post<GenericResult<Az_AnagraficaOutModel>>(environment.remoteData.apiUri + 'Az_Anagrafica/GetAll', model)
      .pipe(
        map(r => {
                  return r;
                 }
           )
      );

  }

}
