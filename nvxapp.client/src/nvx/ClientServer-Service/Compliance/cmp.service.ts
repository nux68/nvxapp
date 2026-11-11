import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { GenericRequest } from '../ModelsBase/generic-request';
import { GenericResult } from '../ModelsBase/generic-result';
import {
  CmpCfgGetOutModel, CmpCfgPutInModel, CmpCfgPutOutModel, CmpDeleteInModel, CmpDeleteOutModel, CmpEmptyInModel, CmpGdprExportOutModel, CmpListInModel,
  CmpListOutModel, CmpLookupOutModel, CmpMineOutModel, CmpPutInModel, CmpPutOutModel, CmpRiga, CmpStudioAziendaModel
} from './Models/cmp-model';

/** controller del server: sicurezza sul lavoro (CMP-81) e registro GDPR (CMP-GDPR, anche lato studio) */
export type CmpApi = 'Cmp_Sicurezza' | 'Cmp_Gdpr';

/*
 Compliance (ANALISI_COMPLIANCE_APPLICATIVO.md): configurazione, sicurezza sul lavoro e registro GDPR.
 Le sezioni hanno tutte la stessa forma (Cmp_<Sezione>List / Put / Delete): vedi cmp-sezioni.ts.
*/
@Injectable({
  providedIn: 'root'
})
export class CmpService {

  constructor(private http: HttpClient) { }

  private post<I, O>(api: string, action: string, model: GenericRequest<I>): Observable<GenericResult<O>> {
    return this.http.post<GenericResult<O>>(environment.remoteData.apiUri + api + '/' + action, model);
  }

  // configurazione
  CfgGet(m: GenericRequest<CmpEmptyInModel>) { return this.post<CmpEmptyInModel, CmpCfgGetOutModel>('Cmp_Config', 'Cmp_CfgGet', m); }
  CfgPut(m: GenericRequest<CmpCfgPutInModel>) { return this.post<CmpCfgPutInModel, CmpCfgPutOutModel>('Cmp_Config', 'Cmp_CfgPut', m); }

  // sezioni (elenco, salvataggio, cancellazione)
  List(api: CmpApi, sezione: string, m: GenericRequest<CmpListInModel>) { return this.post<CmpListInModel, CmpListOutModel<CmpRiga>>(api, 'Cmp_' + sezione + 'List', m); }
  Put(api: CmpApi, sezione: string, m: GenericRequest<CmpPutInModel>) { return this.post<CmpPutInModel, CmpPutOutModel<CmpRiga>>(api, 'Cmp_' + sezione + 'Put', m); }
  Delete(api: CmpApi, sezione: string, m: GenericRequest<CmpDeleteInModel>) { return this.post<CmpDeleteInModel, CmpDeleteOutModel>(api, 'Cmp_' + sezione + 'Delete', m); }

  // sicurezza sul lavoro: tabelle di scelta, situazione della formazione, dati del dipendente
  Lookup(m: GenericRequest<CmpListInModel>) { return this.post<CmpListInModel, CmpLookupOutModel>('Cmp_Sicurezza', 'Cmp_Lookup', m); }
  FormazioneStato(m: GenericRequest<CmpListInModel>) { return this.post<CmpListInModel, CmpListOutModel<CmpRiga>>('Cmp_Sicurezza', 'Cmp_FormazioneStato', m); }
  Mine(m: GenericRequest<CmpListInModel>) { return this.post<CmpListInModel, CmpMineOutModel>('Cmp_Sicurezza', 'Cmp_Mine', m); }

  // registro GDPR: export (job) e funzioni lato studio
  GdprExport(m: GenericRequest<CmpListInModel>) { return this.post<CmpListInModel, CmpGdprExportOutModel>('Cmp_Gdpr', 'Cmp_GdprExport', m); }
  StudioAziende(m: GenericRequest<CmpListInModel>) { return this.post<CmpListInModel, CmpListOutModel<CmpStudioAziendaModel>>('Cmp_Gdpr', 'Cmp_StudioAziende', m); }
  StudioScadenze(m: GenericRequest<CmpListInModel>) { return this.post<CmpListInModel, CmpListOutModel<CmpRiga>>('Cmp_Gdpr', 'Cmp_StudioScadenze', m); }
}
