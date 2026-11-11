import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { GenericRequest } from '../ModelsBase/generic-request';
import { GenericResult } from '../ModelsBase/generic-result';
import { AstCfgGetOutModel, AstCfgPutInModel, AstCfgPutOutModel, AstEmptyInModel } from './Models/ast-model';

/*
 Risorse aziendali (ANALISI_ASSETS_APPLICATIVO.md): per ora la sola configurazione del modulo.
*/
@Injectable({
  providedIn: 'root'
})
export class AstService {

  constructor(private http: HttpClient) { }

  private post<I, O>(api: string, action: string, model: GenericRequest<I>): Observable<GenericResult<O>> {
    return this.http.post<GenericResult<O>>(environment.remoteData.apiUri + api + '/' + action, model);
  }

  // configurazione
  CfgGet(m: GenericRequest<AstEmptyInModel>) { return this.post<AstEmptyInModel, AstCfgGetOutModel>('Ast_Config', 'Ast_CfgGet', m); }
  CfgPut(m: GenericRequest<AstCfgPutInModel>) { return this.post<AstCfgPutInModel, AstCfgPutOutModel>('Ast_Config', 'Ast_CfgPut', m); }
}
