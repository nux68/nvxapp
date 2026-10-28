import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { map, Observable } from 'rxjs';
import {
  DipendentiImport_EstraiInModel,
  DipendentiImport_EstraiOutModel
} from './Models/dipendenti-import-model';

@Injectable({
  providedIn: 'root'
})
export class DipendentiImportService {

  constructor(private http: HttpClient) { }

  /** dipendenti letti dal file dal LLM (non salva nulla) */
  DipendentiImportEstrai(model: GenericRequest<DipendentiImport_EstraiInModel>): Observable<GenericResult<DipendentiImport_EstraiOutModel>> {
    return this.http.post<GenericResult<DipendentiImport_EstraiOutModel>>(
      environment.remoteData.apiUri + 'DipendentiImport/DipendentiImportEstrai', model
    ).pipe(map(r => r));
  }

}
