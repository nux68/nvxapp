import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import {
  Az_SediRepartoTurno_GetInModel, Az_SediRepartoTurno_GetOutModel, Az_SediRepartoTurno_PutInModel, Az_SediRepartoTurno_PutOutModel,
  Dip_TurnoScambio_AnnullaInModel, Dip_TurnoScambio_AnnullaOutModel, Dip_TurnoScambio_ProponiInModel, Dip_TurnoScambio_ProponiOutModel,
  Dip_TurnoScambio_RispondiInModel, Dip_TurnoScambio_RispondiOutModel, Dip_Turno_GetAll4UserInModel, Dip_Turno_GetAll4UserOutModel,
  Dip_Turno_PianificazioneInModel, Dip_Turno_PianificazioneOutModel, Dip_Turno_PutInModel, Dip_Turno_PutOutModel,
  Par_Turno_DeleteInModel, Par_Turno_DeleteOutModel, Par_Turno_GetAllInModel, Par_Turno_GetAllOutModel, Par_Turno_GetInModel,
  Par_Turno_GetOutModel, Par_Turno_PutInModel, Par_Turno_PutOutModel
} from './Models/turni-model';

// Turni e pianificazione (AT-TURNI): modelli di turno, coperture dei reparti, pianificazione, turni del dipendente, scambi
@Injectable({
  providedIn: 'root'
})
export class TurniService {

  constructor(private http: HttpClient) { }

  private post<I, O>(url: string, model: GenericRequest<I>): Observable<GenericResult<O>> {
    return this.http.post<GenericResult<O>>(environment.remoteData.apiUri + url, model);
  }

  GetAll(model: GenericRequest<Par_Turno_GetAllInModel>) { return this.post<Par_Turno_GetAllInModel, Par_Turno_GetAllOutModel>('Par_Turno/GetAll', model); }
  Par_TurnoGet(model: GenericRequest<Par_Turno_GetInModel>) { return this.post<Par_Turno_GetInModel, Par_Turno_GetOutModel>('Par_Turno/Par_TurnoGet', model); }
  Par_TurnoPut(model: GenericRequest<Par_Turno_PutInModel>) { return this.post<Par_Turno_PutInModel, Par_Turno_PutOutModel>('Par_Turno/Par_TurnoPut', model); }
  Par_TurnoDelete(model: GenericRequest<Par_Turno_DeleteInModel>) { return this.post<Par_Turno_DeleteInModel, Par_Turno_DeleteOutModel>('Par_Turno/Par_TurnoDelete', model); }
  Coperture4Reparto(model: GenericRequest<Az_SediRepartoTurno_GetInModel>) { return this.post<Az_SediRepartoTurno_GetInModel, Az_SediRepartoTurno_GetOutModel>('Par_Turno/Coperture4Reparto', model); }
  Coperture4RepartoPut(model: GenericRequest<Az_SediRepartoTurno_PutInModel>) { return this.post<Az_SediRepartoTurno_PutInModel, Az_SediRepartoTurno_PutOutModel>('Par_Turno/Coperture4RepartoPut', model); }

  Pianificazione(model: GenericRequest<Dip_Turno_PianificazioneInModel>) { return this.post<Dip_Turno_PianificazioneInModel, Dip_Turno_PianificazioneOutModel>('Dip_Turno/Pianificazione', model); }
  Put(model: GenericRequest<Dip_Turno_PutInModel>) { return this.post<Dip_Turno_PutInModel, Dip_Turno_PutOutModel>('Dip_Turno/Put', model); }
  GetAll4User(model: GenericRequest<Dip_Turno_GetAll4UserInModel>) { return this.post<Dip_Turno_GetAll4UserInModel, Dip_Turno_GetAll4UserOutModel>('Dip_Turno/GetAll4User', model); }

  Proponi(model: GenericRequest<Dip_TurnoScambio_ProponiInModel>) { return this.post<Dip_TurnoScambio_ProponiInModel, Dip_TurnoScambio_ProponiOutModel>('Dip_TurnoScambio/Proponi', model); }
  Rispondi(model: GenericRequest<Dip_TurnoScambio_RispondiInModel>) { return this.post<Dip_TurnoScambio_RispondiInModel, Dip_TurnoScambio_RispondiOutModel>('Dip_TurnoScambio/Rispondi', model); }
  Annulla(model: GenericRequest<Dip_TurnoScambio_AnnullaInModel>) { return this.post<Dip_TurnoScambio_AnnullaInModel, Dip_TurnoScambio_AnnullaOutModel>('Dip_TurnoScambio/Annulla', model); }
}
