import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import {
  Dip_Anomalie_GetInModel, Dip_Anomalie_GetOutModel, Dip_Anomalie_SpiegaOutModel,
  Dip_PrevisioneAssenze_GetInModel, Dip_PrevisioneAssenze_GetOutModel,
  Widget_GiornateDaSistemareGetInModel, Widget_GiornateDaSistemareGetOutModel,
  Widget_PrevisioneAssenzeGetInModel, Widget_PrevisioneAssenzeGetOutModel
} from './Models/dip-anomalie-model';

// anomalie delle timbrature, giornate da sistemare e previsione delle assenze (ANALISI_ATTENDANCETRACKING_ANOMALIE_AI.md)
@Injectable({
  providedIn: 'root'
})
export class DipAnomalieService {

  constructor(private http: HttpClient) { }

  AnomalieGet(model: GenericRequest<Dip_Anomalie_GetInModel>): Observable<GenericResult<Dip_Anomalie_GetOutModel>> {
    return this.http.post<GenericResult<Dip_Anomalie_GetOutModel>>(environment.remoteData.apiUri + 'Dip_Anomalie/AnomalieGet', model);
  }

  AnomalieSpiega(model: GenericRequest<Dip_Anomalie_GetInModel>): Observable<GenericResult<Dip_Anomalie_SpiegaOutModel>> {
    return this.http.post<GenericResult<Dip_Anomalie_SpiegaOutModel>>(environment.remoteData.apiUri + 'Dip_Anomalie/AnomalieSpiega', model);
  }

  PrevisioneAssenzeGet(model: GenericRequest<Dip_PrevisioneAssenze_GetInModel>): Observable<GenericResult<Dip_PrevisioneAssenze_GetOutModel>> {
    return this.http.post<GenericResult<Dip_PrevisioneAssenze_GetOutModel>>(environment.remoteData.apiUri + 'Dip_Anomalie/PrevisioneAssenzeGet', model);
  }

  GiornateDaSistemareWidget(model: GenericRequest<Widget_GiornateDaSistemareGetInModel>): Observable<GenericResult<Widget_GiornateDaSistemareGetOutModel>> {
    return this.http.post<GenericResult<Widget_GiornateDaSistemareGetOutModel>>(environment.remoteData.apiUri + 'Dip_Anomalie/Widget_GiornateDaSistemareGet', model);
  }

  PrevisioneAssenzeWidget(model: GenericRequest<Widget_PrevisioneAssenzeGetInModel>): Observable<GenericResult<Widget_PrevisioneAssenzeGetOutModel>> {
    return this.http.post<GenericResult<Widget_PrevisioneAssenzeGetOutModel>>(environment.remoteData.apiUri + 'Dip_Anomalie/Widget_PrevisioneAssenzeGet', model);
  }
}
