// Sedi, reparti e competenze dell'azienda (tabelle comuni, schema Common) in sola lettura,
// per i moduli che non sono le presenze: disponibili con qualsiasi applicativo attivo
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { map, Observable } from 'rxjs';
import { CommonOrganizzazione_SediGetAll_InModel, CommonOrganizzazione_SediGetAll_OutModel, CommonOrganizzazione_CompetenzeGetAll_InModel, CommonOrganizzazione_CompetenzeGetAll_OutModel } from './Models/common-organizzazione-model';

@Injectable({
  providedIn: 'root'
})
export class CommonOrganizzazioneService {
  constructor(private http: HttpClient) { }

  // sedi con l'albero dei reparti
  SediGetAll(model: GenericRequest<CommonOrganizzazione_SediGetAll_InModel>): Observable<GenericResult<CommonOrganizzazione_SediGetAll_OutModel>> {
    return this.http.post<GenericResult<CommonOrganizzazione_SediGetAll_OutModel>>(environment.remoteData.apiUri + 'CommonOrganizzazione/SediGetAll', model)
      .pipe(map(r => r));
  }

  // competenze definite dall'azienda
  CompetenzeGetAll(model: GenericRequest<CommonOrganizzazione_CompetenzeGetAll_InModel>): Observable<GenericResult<CommonOrganizzazione_CompetenzeGetAll_OutModel>> {
    return this.http.post<GenericResult<CommonOrganizzazione_CompetenzeGetAll_OutModel>>(environment.remoteData.apiUri + 'CommonOrganizzazione/CompetenzeGetAll', model)
      .pipe(map(r => r));
  }
}
