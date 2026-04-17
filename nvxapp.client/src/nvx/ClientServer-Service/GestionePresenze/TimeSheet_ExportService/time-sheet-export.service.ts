import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../Utility/infrastructure/auth.service';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { map, Observable } from 'rxjs';
import {
  TimeSheet_ExportInModel,
  TimeSheet_ExportOutModel
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

}
