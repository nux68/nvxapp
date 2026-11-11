import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { Par_ProfiloFormuleInModel, Par_ProfiloFormuleOutModel, Par_ProfiloFormuleGetInModel, Par_ProfiloFormuleGetOutModel, Par_ProfiloFormulePutInModel, Par_ProfiloFormulePutOutModel, Par_ProfiloFormule_DeleteInModel, Par_ProfiloFormule_DeleteOutModel, Par_ProfiloFormule_DuplicaInModel, Par_ProfiloFormule_DuplicaOutModel } from './Models/par-profilo-formule-model';

// formule personalizzate del calcolo giornata (CompanyPowerAdmin, piano Pro)
@Injectable({
  providedIn: 'root'
})
export class ParProfiloFormuleService {

  constructor(private http: HttpClient) { }

  GetAll(model: GenericRequest<Par_ProfiloFormuleInModel>): Observable<GenericResult<Par_ProfiloFormuleOutModel>> {
    return this.http.post<GenericResult<Par_ProfiloFormuleOutModel>>(environment.remoteData.apiUri + 'Par_ProfiloFormule/GetAll', model);
  }

  Par_ProfiloFormuleGet(model: GenericRequest<Par_ProfiloFormuleGetInModel>): Observable<GenericResult<Par_ProfiloFormuleGetOutModel>> {
    return this.http.post<GenericResult<Par_ProfiloFormuleGetOutModel>>(environment.remoteData.apiUri + 'Par_ProfiloFormule/Par_ProfiloFormuleGet', model);
  }

  Par_ProfiloFormulePut(model: GenericRequest<Par_ProfiloFormulePutInModel>): Observable<GenericResult<Par_ProfiloFormulePutOutModel>> {
    return this.http.post<GenericResult<Par_ProfiloFormulePutOutModel>>(environment.remoteData.apiUri + 'Par_ProfiloFormule/Par_ProfiloFormulePut', model);
  }

  Par_ProfiloFormuleDelete(model: GenericRequest<Par_ProfiloFormule_DeleteInModel>): Observable<GenericResult<Par_ProfiloFormule_DeleteOutModel>> {
    return this.http.post<GenericResult<Par_ProfiloFormule_DeleteOutModel>>(environment.remoteData.apiUri + 'Par_ProfiloFormule/Par_ProfiloFormuleDelete', model);
  }

  Par_ProfiloFormuleDuplica(model: GenericRequest<Par_ProfiloFormule_DuplicaInModel>): Observable<GenericResult<Par_ProfiloFormule_DuplicaOutModel>> {
    return this.http.post<GenericResult<Par_ProfiloFormule_DuplicaOutModel>>(environment.remoteData.apiUri + 'Par_ProfiloFormule/Par_ProfiloFormuleDuplica', model);
  }
}
