import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { CompanyRegisterInModel, CompanyRegisterOutModel } from './Models/company-register-model';


// Registrazione di una nuova azienda dallo showcase (API anonima)
@Injectable({
  providedIn: 'root'
})
export class RegistrationService {

  constructor(private http: HttpClient) { }

  CompanyRegister(model: GenericRequest<CompanyRegisterInModel>): Observable<GenericResult<CompanyRegisterOutModel>> {
    return this.http.post<GenericResult<CompanyRegisterOutModel>>(environment.remoteData.apiUri + 'Registration/CompanyRegister', model);
  }
}
