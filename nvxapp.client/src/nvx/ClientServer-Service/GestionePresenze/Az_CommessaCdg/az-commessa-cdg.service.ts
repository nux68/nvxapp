import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { Az_CommessaCdg_GetInModel, Az_CommessaCdg_GetOutModel, CdgParametri, Widget_OreCommesseGetInModel, Widget_OreCommesseGetOutModel } from './Models/az-commessa-cdg-model';

/*
 Controllo di gestione sulle commesse (ANALISI_ATTENDANCETRACKING_CONTROLLO_GESTIONE.md).
 Costi orari e ricavi non si salvano sul server (scelta provvisoria CDG1-2): il browser li ricorda (localStorage)
 e li invia a ogni richiesta; li legge anche il widget "Ore sulle commesse".
*/
@Injectable({
  providedIn: 'root'
})
export class AzCommessaCdgService {

  private static readonly ChiaveParametri = 'nvx.attendance.cdg.parametri';

  constructor(private http: HttpClient) { }

  CdgGet(model: GenericRequest<Az_CommessaCdg_GetInModel>): Observable<GenericResult<Az_CommessaCdg_GetOutModel>> {
    return this.http.post<GenericResult<Az_CommessaCdg_GetOutModel>>(environment.remoteData.apiUri + 'Az_CommessaCdg/CdgGet', model);
  }

  OreCommesseWidget(model: GenericRequest<Widget_OreCommesseGetInModel>): Observable<GenericResult<Widget_OreCommesseGetOutModel>> {
    return this.http.post<GenericResult<Widget_OreCommesseGetOutModel>>(environment.remoteData.apiUri + 'Az_CommessaCdg/Widget_OreCommesseGet', model);
  }

  /** costi e ricavi ricordati dal browser (null se non ci sono o lo storage non e' disponibile) */
  ParametriSalvati(): Partial<CdgParametri> | null {
    try {
      const testo = localStorage.getItem(AzCommessaCdgService.ChiaveParametri);
      return testo ? JSON.parse(testo) : null;
    } catch {
      return null;
    }
  }

  SalvaParametri(p: CdgParametri): void {
    try {
      localStorage.setItem(AzCommessaCdgService.ChiaveParametri, JSON.stringify({
        costoOrarioPredefinito: p.costoOrarioPredefinito,
        costiDipendenti: p.costiDipendenti,
        ricavi: p.ricavi
      }));
    } catch {
      // storage non disponibile: i valori valgono solo per la pagina aperta
    }
  }
}
