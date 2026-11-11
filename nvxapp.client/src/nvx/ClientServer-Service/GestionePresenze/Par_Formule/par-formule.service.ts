import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { Par_FormuleInModel, Par_FormuleOutModel, Par_FormuleGetInModel, Par_FormuleGetOutModel, Par_FormulePutInModel, Par_FormulePutOutModel, Par_Formule_DeleteInModel, Par_Formule_DeleteOutModel, Par_Formule_DuplicaInModel, Par_Formule_DuplicaOutModel, Par_Formule_CtrlSintassiInModel, Par_Formule_CtrlSintassiOutModel, Par_Formule_MetodiInModel, Par_Formule_MetodiOutModel, Par_Formule_RapportiProvaInModel, Par_Formule_RapportiProvaOutModel, Par_Formule_ProvaInModel, Par_Formule_ProvaOutModel, Par_Formule_LogInModel, Par_Formule_LogOutModel, Par_Formule_GeneraAIInModel, Par_Formule_GeneraAIOutModel } from './Models/par-formule-model';

// formule personalizzate del calcolo giornata (CompanyPowerAdmin, piano Pro)
@Injectable({
  providedIn: 'root'
})
export class ParFormuleService {

  constructor(private http: HttpClient) { }

  GetAll(model: GenericRequest<Par_FormuleInModel>): Observable<GenericResult<Par_FormuleOutModel>> {
    return this.http.post<GenericResult<Par_FormuleOutModel>>(environment.remoteData.apiUri + 'Par_Formule/GetAll', model);
  }

  Par_FormuleGet(model: GenericRequest<Par_FormuleGetInModel>): Observable<GenericResult<Par_FormuleGetOutModel>> {
    return this.http.post<GenericResult<Par_FormuleGetOutModel>>(environment.remoteData.apiUri + 'Par_Formule/Par_FormuleGet', model);
  }

  Par_FormulePut(model: GenericRequest<Par_FormulePutInModel>): Observable<GenericResult<Par_FormulePutOutModel>> {
    return this.http.post<GenericResult<Par_FormulePutOutModel>>(environment.remoteData.apiUri + 'Par_Formule/Par_FormulePut', model);
  }

  Par_FormuleDelete(model: GenericRequest<Par_Formule_DeleteInModel>): Observable<GenericResult<Par_Formule_DeleteOutModel>> {
    return this.http.post<GenericResult<Par_Formule_DeleteOutModel>>(environment.remoteData.apiUri + 'Par_Formule/Par_FormuleDelete', model);
  }

  Par_FormuleDuplica(model: GenericRequest<Par_Formule_DuplicaInModel>): Observable<GenericResult<Par_Formule_DuplicaOutModel>> {
    return this.http.post<GenericResult<Par_Formule_DuplicaOutModel>>(environment.remoteData.apiUri + 'Par_Formule/Par_FormuleDuplica', model);
  }

  CtrlSintassi(model: GenericRequest<Par_Formule_CtrlSintassiInModel>): Observable<GenericResult<Par_Formule_CtrlSintassiOutModel>> {
    return this.http.post<GenericResult<Par_Formule_CtrlSintassiOutModel>>(environment.remoteData.apiUri + 'Par_Formule/CtrlSintassi', model);
  }

  Metodi(model: GenericRequest<Par_Formule_MetodiInModel>): Observable<GenericResult<Par_Formule_MetodiOutModel>> {
    return this.http.post<GenericResult<Par_Formule_MetodiOutModel>>(environment.remoteData.apiUri + 'Par_Formule/Metodi', model);
  }

  RapportiProva(model: GenericRequest<Par_Formule_RapportiProvaInModel>): Observable<GenericResult<Par_Formule_RapportiProvaOutModel>> {
    return this.http.post<GenericResult<Par_Formule_RapportiProvaOutModel>>(environment.remoteData.apiUri + 'Par_Formule/RapportiProva', model);
  }

  Prova(model: GenericRequest<Par_Formule_ProvaInModel>): Observable<GenericResult<Par_Formule_ProvaOutModel>> {
    return this.http.post<GenericResult<Par_Formule_ProvaOutModel>>(environment.remoteData.apiUri + 'Par_Formule/Prova', model);
  }

  Log(model: GenericRequest<Par_Formule_LogInModel>): Observable<GenericResult<Par_Formule_LogOutModel>> {
    return this.http.post<GenericResult<Par_Formule_LogOutModel>>(environment.remoteData.apiUri + 'Par_Formule/Log', model);
  }

  GeneraAI(model: GenericRequest<Par_Formule_GeneraAIInModel>): Observable<GenericResult<Par_Formule_GeneraAIOutModel>> {
    return this.http.post<GenericResult<Par_Formule_GeneraAIOutModel>>(environment.remoteData.apiUri + 'Par_Formule/GeneraAI', model);
  }
}
