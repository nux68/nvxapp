import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { GenericRequest } from '../ModelsBase/generic-request';
import { GenericResult } from '../ModelsBase/generic-result';
import { AstCfgGetOutModel, AstCfgPutInModel, AstCfgPutOutModel, AstEmptyInModel } from './Models/ast-model';
import * as F from './Models/ast-flotta-model';

/*
 Risorse aziendali (ANALISI_ASSETS_APPLICATIVO.md): configurazione, flotta (AST-FLO), beni (AST-BENI), prenotazioni (AST-PREN).
 I permessi li controlla il server in ogni servizio.
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

  // flotta
  VeicoloList(m: GenericRequest<F.AstVeicoloListInModel>) { return this.post<F.AstVeicoloListInModel, F.AstVeicoloListOutModel>('Ast_Flotta', 'Ast_VeicoloList', m); }
  VeicoloGet(m: GenericRequest<F.AstIdInModel>) { return this.post<F.AstIdInModel, F.AstVeicoloGetOutModel>('Ast_Flotta', 'Ast_VeicoloGet', m); }
  VeicoloPut(m: GenericRequest<F.AstVeicoloPutInModel>) { return this.post<F.AstVeicoloPutInModel, F.AstVeicoloPutOutModel>('Ast_Flotta', 'Ast_VeicoloPut', m); }
  VeicoloDelete(m: GenericRequest<F.AstIdInModel>) { return this.post<F.AstIdInModel, F.AstDeleteOutModel>('Ast_Flotta', 'Ast_VeicoloDelete', m); }
  AssegnazionePut(m: GenericRequest<F.AstAssegnazionePutInModel>) { return this.post<F.AstAssegnazionePutInModel, F.AstAssegnazionePutOutModel>('Ast_Flotta', 'Ast_AssegnazionePut', m); }
  AssegnazioneDelete(m: GenericRequest<F.AstIdInModel>) { return this.post<F.AstIdInModel, F.AstDeleteOutModel>('Ast_Flotta', 'Ast_AssegnazioneDelete', m); }
  VeicoloScadenzaPut(m: GenericRequest<F.AstVeicoloScadenzaPutInModel>) { return this.post<F.AstVeicoloScadenzaPutInModel, F.AstVeicoloScadenzaPutOutModel>('Ast_Flotta', 'Ast_VeicoloScadenzaPut', m); }
  VeicoloScadenzaDelete(m: GenericRequest<F.AstIdInModel>) { return this.post<F.AstIdInModel, F.AstDeleteOutModel>('Ast_Flotta', 'Ast_VeicoloScadenzaDelete', m); }
  VeicoloCostoPut(m: GenericRequest<F.AstVeicoloCostoPutInModel>) { return this.post<F.AstVeicoloCostoPutInModel, F.AstVeicoloCostoPutOutModel>('Ast_Flotta', 'Ast_VeicoloCostoPut', m); }
  VeicoloCostoDelete(m: GenericRequest<F.AstIdInModel>) { return this.post<F.AstIdInModel, F.AstDeleteOutModel>('Ast_Flotta', 'Ast_VeicoloCostoDelete', m); }
  MultaPut(m: GenericRequest<F.AstMultaPutInModel>) { return this.post<F.AstMultaPutInModel, F.AstMultaPutOutModel>('Ast_Flotta', 'Ast_MultaPut', m); }
  MultaDelete(m: GenericRequest<F.AstIdInModel>) { return this.post<F.AstIdInModel, F.AstDeleteOutModel>('Ast_Flotta', 'Ast_MultaDelete', m); }
  MultaNotifica(m: GenericRequest<F.AstIdInModel>) { return this.post<F.AstIdInModel, F.AstMultaPutOutModel>('Ast_Flotta', 'Ast_MultaNotifica', m); }
  MieiVeicoliGet(m: GenericRequest<AstEmptyInModel>) { return this.post<AstEmptyInModel, F.AstMieiVeicoliOutModel>('Ast_Flotta', 'Ast_MieiVeicoliGet', m); }
}
