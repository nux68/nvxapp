import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { PasswordRecoveryRequestInModel, PasswordRecoveryRequestOutModel, PasswordRecoveryResetInModel, PasswordRecoveryResetOutModel } from './Models/password-recovery-model';


// Recupero dell'accesso con password dimenticata (API anonime): link per email, poi nuova password
@Injectable({
  providedIn: 'root'
})
export class PasswordRecoveryService {

  constructor(private http: HttpClient) { }

  PasswordRecoveryRequest(model: GenericRequest<PasswordRecoveryRequestInModel>): Observable<GenericResult<PasswordRecoveryRequestOutModel>> {
    return this.http.post<GenericResult<PasswordRecoveryRequestOutModel>>(environment.remoteData.apiUri + 'PasswordRecovery/PasswordRecoveryRequest', model);
  }

  PasswordRecoveryReset(model: GenericRequest<PasswordRecoveryResetInModel>): Observable<GenericResult<PasswordRecoveryResetOutModel>> {
    return this.http.post<GenericResult<PasswordRecoveryResetOutModel>>(environment.remoteData.apiUri + 'PasswordRecovery/PasswordRecoveryReset', model);
  }
}
