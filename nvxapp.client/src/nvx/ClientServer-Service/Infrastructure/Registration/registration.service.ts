import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { CompanyRegisterConfirmInModel, CompanyRegisterConfirmOutModel, CompanyRegisterInModel, CompanyRegisterOutModel } from './Models/company-register-model';


// Registrazione di una nuova azienda dallo showcase (API anonime): richiesta con invio del link
// di conferma per email, poi conferma dal link
@Injectable({
  providedIn: 'root'
})
export class RegistrationService {

  constructor(private http: HttpClient) { }

  CompanyRegister(model: GenericRequest<CompanyRegisterInModel>): Observable<GenericResult<CompanyRegisterOutModel>> {
    return this.http.post<GenericResult<CompanyRegisterOutModel>>(environment.remoteData.apiUri + 'Registration/CompanyRegister', model);
  }

  CompanyRegisterConfirm(model: GenericRequest<CompanyRegisterConfirmInModel>): Observable<GenericResult<CompanyRegisterConfirmOutModel>> {
    return this.http.post<GenericResult<CompanyRegisterConfirmOutModel>>(environment.remoteData.apiUri + 'Registration/CompanyRegisterConfirm', model);
  }
}
