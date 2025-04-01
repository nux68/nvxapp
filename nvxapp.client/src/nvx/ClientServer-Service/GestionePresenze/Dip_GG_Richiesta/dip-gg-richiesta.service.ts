import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../Utility/auth.service';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { map, Observable } from 'rxjs';
import { Dip_GG_RichiestaInModel, Dip_GG_RichiestaOutModel } from './Models/dip-gg-richiesta-model';

@Injectable({
  providedIn: 'root'
})
export class DipGGRichiestaService {

  constructor(private http: HttpClient,
    private authService: AuthService
  ) { }

  GetAll(model: GenericRequest<Dip_GG_RichiestaInModel>): Observable<GenericResult<Dip_GG_RichiestaOutModel>> {

    return this.http.post<GenericResult<Dip_GG_RichiestaOutModel>>(environment.remoteData.apiUri + 'Dip_GG_Richiesta/GetAll', model)
      .pipe(
        map(r => {
          return r;
        }
        )
      );

  }

}
