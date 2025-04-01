import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../Utility/auth.service';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { map, Observable } from 'rxjs';
import { Dip_GG_NotaSpesaInModel, Dip_GG_NotaSpesaOutModel } from './Models/dip-gg-nota-spesa-model';

@Injectable({
  providedIn: 'root'
})
export class DipGGNotaSpesaService {

  constructor(private http: HttpClient,
    private authService: AuthService
  ) { }

  GetAll(model: GenericRequest<Dip_GG_NotaSpesaInModel>): Observable<GenericResult<Dip_GG_NotaSpesaOutModel>> {

    return this.http.post<GenericResult<Dip_GG_NotaSpesaOutModel>>(environment.remoteData.apiUri + 'Dip_GG_NotaSpesa/GetAll', model)
      .pipe(
        map(r => {
          return r;
        }
        )
      );

  }

}
