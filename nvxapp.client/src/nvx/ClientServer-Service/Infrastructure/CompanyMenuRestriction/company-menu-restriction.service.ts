import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { CompanyMenuRestrictionListInModel, CompanyMenuRestrictionListOutModel, CompanyMenuRestrictionPutInModel, CompanyMenuRestrictionPutOutModel } from './Models/company-menu-restriction-model';


// Voci di menu nascoste ai ruoli CompanyAdmin e User dell'azienda corrente (pagina "Menu utenti")
@Injectable({
  providedIn: 'root'
})
export class CompanyMenuRestrictionService {

  constructor(private http: HttpClient) { }

  CompanyMenuRestrictionList(model: GenericRequest<CompanyMenuRestrictionListInModel>): Observable<GenericResult<CompanyMenuRestrictionListOutModel>> {
    return this.http.post<GenericResult<CompanyMenuRestrictionListOutModel>>(environment.remoteData.apiUri + 'CompanyMenuRestriction/CompanyMenuRestrictionList', model);
  }

  CompanyMenuRestrictionPut(model: GenericRequest<CompanyMenuRestrictionPutInModel>): Observable<GenericResult<CompanyMenuRestrictionPutOutModel>> {
    return this.http.post<GenericResult<CompanyMenuRestrictionPutOutModel>>(environment.remoteData.apiUri + 'CompanyMenuRestriction/CompanyMenuRestrictionPut', model);
  }
}
