import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { Dip_RapportoAutovetture_GetAll4User_InModel, Dip_RapportoAutovetture_GetAll4User_OutModel } from './Models/dip-rapporto-autovetture-model';

// le autovetture si modificano dall'anagrafica dipendente (Dip_AnagraficaPut); qui solo la lettura per la nota spese
@Injectable({
  providedIn: 'root'
})
export class DipRapportoAutovettureService {

  constructor(private http: HttpClient) { }

  GetAll4User(model: GenericRequest<Dip_RapportoAutovetture_GetAll4User_InModel>): Observable<GenericResult<Dip_RapportoAutovetture_GetAll4User_OutModel>> {
    return this.http.post<GenericResult<Dip_RapportoAutovetture_GetAll4User_OutModel>>(environment.remoteData.apiUri + 'Dip_RapportoAutovetture/GetAll4User', model);
  }
}
