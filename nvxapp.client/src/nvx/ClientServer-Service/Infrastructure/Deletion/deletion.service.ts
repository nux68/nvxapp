import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { DeletionExecuteInModel, DeletionExecuteOutModel, DeletionPreviewInModel, DeletionPreviewOutModel } from './Models/deletion-model';


// Cancellazione fisica di centri, studi, aziende e utenti (anteprima ed esecuzione; permessi controllati dal server)
@Injectable({
  providedIn: 'root'
})
export class DeletionService {

  constructor(private http: HttpClient) { }

  DeletionPreview(model: GenericRequest<DeletionPreviewInModel>): Observable<GenericResult<DeletionPreviewOutModel>> {
    return this.http.post<GenericResult<DeletionPreviewOutModel>>(environment.remoteData.apiUri + 'Deletion/DeletionPreview', model);
  }

  DeletionExecute(model: GenericRequest<DeletionExecuteInModel>): Observable<GenericResult<DeletionExecuteOutModel>> {
    return this.http.post<GenericResult<DeletionExecuteOutModel>>(environment.remoteData.apiUri + 'Deletion/DeletionExecute', model);
  }
}
