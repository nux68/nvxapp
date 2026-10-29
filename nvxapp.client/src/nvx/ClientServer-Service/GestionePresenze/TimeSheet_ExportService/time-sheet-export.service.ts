import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../Utility/infrastructure/auth.service';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { map, Observable } from 'rxjs';
import {
  TimeSheet_ExportInModel,
  TimeSheet_ExportOutModel,
  TimeSheet_ExportPdf_4User_InModel,
  TimeSheet_ExportPdf_4User_OutModel,
  TimeSheet_ExportPdf_4Admin_InModel,
  TimeSheet_ExportPdf_4Admin_OutModel,
  TimeSheet_Export_Anteprima_InModel,
  TimeSheet_Export_Anteprima_OutModel
} from './Models/time-sheet-export-model';

@Injectable({
  providedIn: 'root'
})
export class TimeSheetExportService {

  constructor(
    private http: HttpClient,
    private authService: AuthService
  ) { }

  Export(model: GenericRequest<TimeSheet_ExportInModel>): Observable<GenericResult<TimeSheet_ExportOutModel>> {
    return this.http.post<GenericResult<TimeSheet_ExportOutModel>>(
      environment.remoteData.apiUri + 'TimeSheet_Export/Export', model
    ).pipe(map(r => r));
  }

  // cartellino PDF del mese dell'utente collegato: avvia il job, il file arriva con la notifica di fine job
  ExportPdf_4User(model: GenericRequest<TimeSheet_ExportPdf_4User_InModel>): Observable<GenericResult<TimeSheet_ExportPdf_4User_OutModel>> {
    return this.http.post<GenericResult<TimeSheet_ExportPdf_4User_OutModel>>(
      environment.remoteData.apiUri + 'TimeSheet_Export/ExportPdf_4User', model
    ).pipe(map(r => r));
  }

  // cartellini PDF del mese dei dipendenti scelti dall'admin, in un unico file
  ExportPdf_4Admin(model: GenericRequest<TimeSheet_ExportPdf_4Admin_InModel>): Observable<GenericResult<TimeSheet_ExportPdf_4Admin_OutModel>> {
    return this.http.post<GenericResult<TimeSheet_ExportPdf_4Admin_OutModel>>(
      environment.remoteData.apiUri + 'TimeSheet_Export/ExportPdf_4Admin', model
    ).pipe(map(r => r));
  }

  // anteprima del file del modello di export (puo' chiedere la ricetta all'AI: anche decine di secondi)
  Export_Anteprima(model: GenericRequest<TimeSheet_Export_Anteprima_InModel>): Observable<GenericResult<TimeSheet_Export_Anteprima_OutModel>> {
    return this.http.post<GenericResult<TimeSheet_Export_Anteprima_OutModel>>(
      environment.remoteData.apiUri + 'TimeSheet_Export/Export_Anteprima', model
    );
  }

}
