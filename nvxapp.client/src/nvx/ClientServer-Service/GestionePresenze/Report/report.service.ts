import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { Report_InModel, Report_OutModel } from './Models/report-model';

@Injectable({
  providedIn: 'root'
})
export class ReportService {

  constructor(private http: HttpClient) { }

  // report PDF delle pagine HR: avvia il job, il file arriva con la notifica di fine job
  ReportExportPdf(model: GenericRequest<Report_InModel>): Observable<GenericResult<Report_OutModel>> {
    return this.http.post<GenericResult<Report_OutModel>>(environment.remoteData.apiUri + 'Report/ReportExportPdf', model);
  }
}
