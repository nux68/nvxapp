import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { ModelResult } from '../../ModelsBase/model-result';

// menu dell'utente per il manuale: sezioni (titoli tradotti) e voci che vede adesso
export class UserManualMenuItem { public titolo = ''; public url = ''; }
export class UserManualMenuSection { public titolo = ''; public voci: UserManualMenuItem[] = []; }

export class UserManualGenerateInModel { public code = ''; public siteName: string | null = null; public menu: UserManualMenuSection[] = []; }
export class UserManualGenerateOutModel extends ModelResult { public jobId = ''; }

// Manuali utente in PDF: il server li prepara con un job, il download compare nella barra dei job della pagina
@Injectable({
  providedIn: 'root'
})
export class UserManualService {
  constructor(private http: HttpClient) { }

  Generate(model: GenericRequest<UserManualGenerateInModel>): Observable<GenericResult<UserManualGenerateOutModel>> {
    return this.http.post<GenericResult<UserManualGenerateOutModel>>(environment.remoteData.apiUri + 'UserManual/UserManualGenerate', model);
  }
}
