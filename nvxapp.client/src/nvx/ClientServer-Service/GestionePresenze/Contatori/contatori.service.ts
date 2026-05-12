import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../Utility/infrastructure/auth.service';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { map, Observable } from 'rxjs';
import {
  Contatori_Calcolo_InModel,
  Contatori_Calcolo_OutModel,
  Contatori_Anno_InModel,
  Contatori_Anno_OutModel,
  Contatori_Riporto_Delete_InModel,
  Contatori_Riporto_Delete_OutModel,
  Contatori_Riporto_GetAll_InModel,
  Contatori_Riporto_GetAll_OutModel,
  Contatori_Riporto_Upsert_InModel,
  Contatori_Riporto_Upsert_OutModel
} from './Models/contatori-model';

@Injectable({
  providedIn: 'root'
})
export class ContatoriService {

  private readonly baseUrl = environment.remoteData.apiUri + 'Contatori/';

  constructor(
    private http: HttpClient,
    private authService: AuthService
  ) { }

  // ── Calcolo ────────────────────────────────────────────────────────────────

  CalcolaContatori(model: GenericRequest<Contatori_Calcolo_InModel>): Observable<GenericResult<Contatori_Calcolo_OutModel>> {
    return this.http.post<GenericResult<Contatori_Calcolo_OutModel>>(
      this.baseUrl + 'CalcolaContatori', model
    ).pipe(map(r => r));
  }

  // ── Riporto (Mese 0) ───────────────────────────────────────────────────────

  Riporto_GetAll(model: GenericRequest<Contatori_Riporto_GetAll_InModel>): Observable<GenericResult<Contatori_Riporto_GetAll_OutModel>> {
    return this.http.post<GenericResult<Contatori_Riporto_GetAll_OutModel>>(
      this.baseUrl + 'Riporto_GetAll', model
    ).pipe(map(r => r));
  }

  Riporto_Upsert(model: GenericRequest<Contatori_Riporto_Upsert_InModel>): Observable<GenericResult<Contatori_Riporto_Upsert_OutModel>> {
    return this.http.post<GenericResult<Contatori_Riporto_Upsert_OutModel>>(
      this.baseUrl + 'Riporto_Upsert', model
    ).pipe(map(r => r));
  }

  Riporto_Delete(model: GenericRequest<Contatori_Riporto_Delete_InModel>): Observable<GenericResult<Contatori_Riporto_Delete_OutModel>> {
    return this.http.post<GenericResult<Contatori_Riporto_Delete_OutModel>>(
      this.baseUrl + 'Riporto_Delete', model
    ).pipe(map(r => r));
  }

  CalcolaContatori_Anno(model: GenericRequest<Contatori_Anno_InModel>): Observable<GenericResult<Contatori_Anno_OutModel>> {
    return this.http.post<GenericResult<Contatori_Anno_OutModel>>(
      this.baseUrl + 'CalcolaContatori_Anno', model
    ).pipe(map(r => r));
  }

}
