import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map, Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { Dip_GG_NotaSpesa_Allegato_Get_InModel, Dip_GG_NotaSpesa_Allegato_Get_OutModel, Dip_GG_NotaSpesa_AllegatoModel, Dip_GG_NotaSpesa_Get4Richiesta_InModel, Dip_GG_NotaSpesa_Get4Richiesta_OutModel, Dip_GG_NotaSpesa_Search_InModel, Dip_GG_NotaSpesa_Search_OutModel, Dip_GG_NotaSpesaInModel, Dip_GG_NotaSpesaOutModel } from './Models/dip-gg-nota-spesa-model';

@Injectable({
  providedIn: 'root'
})
export class DipGGNotaSpesaService {

  constructor(private http: HttpClient) { }

  GetAll(model: GenericRequest<Dip_GG_NotaSpesaInModel>): Observable<GenericResult<Dip_GG_NotaSpesaOutModel>> {
    return this.http.post<GenericResult<Dip_GG_NotaSpesaOutModel>>(environment.remoteData.apiUri + 'Dip_GG_NotaSpesa/GetAll', model);
  }

  /** spese (con l'elenco degli allegati) di una richiesta di nota spese */
  Get4Richiesta(model: GenericRequest<Dip_GG_NotaSpesa_Get4Richiesta_InModel>): Observable<GenericResult<Dip_GG_NotaSpesa_Get4Richiesta_OutModel>> {
    return this.http.post<GenericResult<Dip_GG_NotaSpesa_Get4Richiesta_OutModel>>(environment.remoteData.apiUri + 'Dip_GG_NotaSpesa/Get4Richiesta', model);
  }

  /** ricerca delle spese (CompanyPowerAdmin) */
  Search(model: GenericRequest<Dip_GG_NotaSpesa_Search_InModel>): Observable<GenericResult<Dip_GG_NotaSpesa_Search_OutModel>> {
    return this.http.post<GenericResult<Dip_GG_NotaSpesa_Search_OutModel>>(environment.remoteData.apiUri + 'Dip_GG_NotaSpesa/Search', model);
  }

  AllegatoGet(model: GenericRequest<Dip_GG_NotaSpesa_Allegato_Get_InModel>): Observable<GenericResult<Dip_GG_NotaSpesa_Allegato_Get_OutModel>> {
    return this.http.post<GenericResult<Dip_GG_NotaSpesa_Allegato_Get_OutModel>>(environment.remoteData.apiUri + 'Dip_GG_NotaSpesa/AllegatoGet', model);
  }

  /** Apre l'allegato in una nuova scheda (PDF e immagini); se il browser blocca la scheda lo scarica. */
  ApriAllegato(allegato: Dip_GG_NotaSpesa_AllegatoModel): void {
    // la scheda si apre subito (nel gesto dell'utente), il contenuto arriva dopo
    const scheda = window.open('', '_blank');
    const request = new GenericRequest<Dip_GG_NotaSpesa_Allegato_Get_InModel>(Dip_GG_NotaSpesa_Allegato_Get_InModel);
    request.data.id = allegato.id;
    this.AllegatoGet(request).pipe(map(res => res.success && res.data ? res.data : null)).subscribe(file => {
      if (!file) { scheda?.close(); return; }
      const bytes = Uint8Array.from(atob(file.contenuto), c => c.charCodeAt(0));
      const url = URL.createObjectURL(new Blob([bytes], { type: file.contentType }));
      if (scheda) {
        scheda.location.href = url;
      } else {
        const a = document.createElement('a');
        a.href = url;
        a.download = file.nomeFile;
        a.click();
      }
      setTimeout(() => URL.revokeObjectURL(url), 60000);
    });
  }
}
