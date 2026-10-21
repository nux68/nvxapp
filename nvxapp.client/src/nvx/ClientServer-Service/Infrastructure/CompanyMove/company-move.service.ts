import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { CompanyMovePutInModel, CompanyMovePutOutModel, CompanyMoveTreeInModel, CompanyMoveTreeOutModel } from './Models/company-move-model';


// Spostamento di un'azienda sotto un altro studio (pagina "Sposta azienda", SuperUser)
@Injectable({
  providedIn: 'root'
})
export class CompanyMoveService {

  constructor(private http: HttpClient) { }

  CompanyMoveTree(model: GenericRequest<CompanyMoveTreeInModel>): Observable<GenericResult<CompanyMoveTreeOutModel>> {
    return this.http.post<GenericResult<CompanyMoveTreeOutModel>>(environment.remoteData.apiUri + 'CompanyMove/CompanyMoveTree', model);
  }

  CompanyMovePut(model: GenericRequest<CompanyMovePutInModel>): Observable<GenericResult<CompanyMovePutOutModel>> {
    return this.http.post<GenericResult<CompanyMovePutOutModel>>(environment.remoteData.apiUri + 'CompanyMove/CompanyMovePut', model);
  }
}
