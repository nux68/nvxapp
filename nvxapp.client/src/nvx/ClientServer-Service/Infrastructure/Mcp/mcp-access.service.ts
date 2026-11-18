import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import {
  McpCallLogListInModel, McpCallLogListOutModel,
  McpCompanySettingGetInModel, McpCompanySettingGetOutModel, McpCompanySettingPutInModel, McpCompanySettingPutOutModel,
  McpTokenCreateInModel, McpTokenCreateOutModel, McpTokenListInModel, McpTokenListOutModel, McpTokenRevokeInModel, McpTokenRevokeOutModel
} from './Models/mcp-access-model';


// Accesso di un'AI esterna tramite MCP (nvxapp.server/docs/ANALISI_INFRASTRUCTURE_MCP.md)
@Injectable({
  providedIn: 'root'
})
export class McpAccessService {

  constructor(private http: HttpClient) { }

  McpTokenList(model: GenericRequest<McpTokenListInModel>): Observable<GenericResult<McpTokenListOutModel>> {
    return this.http.post<GenericResult<McpTokenListOutModel>>(environment.remoteData.apiUri + 'McpAccess/McpTokenList', model);
  }

  McpTokenCreate(model: GenericRequest<McpTokenCreateInModel>): Observable<GenericResult<McpTokenCreateOutModel>> {
    return this.http.post<GenericResult<McpTokenCreateOutModel>>(environment.remoteData.apiUri + 'McpAccess/McpTokenCreate', model);
  }

  McpTokenRevoke(model: GenericRequest<McpTokenRevokeInModel>): Observable<GenericResult<McpTokenRevokeOutModel>> {
    return this.http.post<GenericResult<McpTokenRevokeOutModel>>(environment.remoteData.apiUri + 'McpAccess/McpTokenRevoke', model);
  }

  McpCompanySettingGet(model: GenericRequest<McpCompanySettingGetInModel>): Observable<GenericResult<McpCompanySettingGetOutModel>> {
    return this.http.post<GenericResult<McpCompanySettingGetOutModel>>(environment.remoteData.apiUri + 'McpAccess/McpCompanySettingGet', model);
  }

  McpCompanySettingPut(model: GenericRequest<McpCompanySettingPutInModel>): Observable<GenericResult<McpCompanySettingPutOutModel>> {
    return this.http.post<GenericResult<McpCompanySettingPutOutModel>>(environment.remoteData.apiUri + 'McpAccess/McpCompanySettingPut', model);
  }

  McpCallLogList(model: GenericRequest<McpCallLogListInModel>): Observable<GenericResult<McpCallLogListOutModel>> {
    return this.http.post<GenericResult<McpCallLogListOutModel>>(environment.remoteData.apiUri + 'McpAccess/McpCallLogList', model);
  }
}
