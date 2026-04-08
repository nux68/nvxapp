import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { map, Observable } from 'rxjs';
import { AuthService } from '../../../Utility/infrastructure/auth.service';
import { Dip_GG_Result_GetAll_InModel, Dip_GG_Result_GetAll_OutModel, Dip_GG_Result_Get_4Calculation_InModel, Dip_GG_Result_Get_4Calculation_OutModel, GG_ResultStato } from './Models/dip-gg-result-model';


@Injectable({
  providedIn: 'root'
})
export class DipGGResultService {

  constructor(private http: HttpClient,
    private authService: AuthService
  ) { }

  GetAll(model: GenericRequest<Dip_GG_Result_GetAll_InModel>): Observable<GenericResult<Dip_GG_Result_GetAll_OutModel>> {

    return this.http.post<GenericResult<Dip_GG_Result_GetAll_OutModel>>(environment.remoteData.apiUri + 'Dip_GG_Result/GetAll', model)
      .pipe(
        map(r => {
          return r;
        })
      );

  }

  Dip_GG_Result_Get_4Calculation_OutModel(model: GenericRequest<Dip_GG_Result_Get_4Calculation_InModel>): Observable<GenericResult<Dip_GG_Result_Get_4Calculation_OutModel>> {

    return this.http.post<GenericResult<Dip_GG_Result_Get_4Calculation_OutModel>>(environment.remoteData.apiUri + 'Dip_GG_Result/Dip_GG_Result_Get_4Calculation', model)
      .pipe(
        map(r => {
          return r;
        })
      );

  }

}


export class Dip_GG_Result_Helper {

  public stato: GG_ResultStato = GG_ResultStato.Init;

  // Imposta lo stato principale (mutuamente esclusivo)
  public setState(state: GG_ResultStato): void {
    this.stato &= ~GG_ResultStato.STATE_MASK; // pulisce gli stati
    this.stato |= state;                      // imposta il nuovo
  }

  // Aggiunge un dettaglio (warning/errore)
  public addDetail(detail: GG_ResultStato): void {
    this.stato |= detail;
  }

  // Rimuove un dettaglio
  public removeDetail(detail: GG_ResultStato): void {
    this.stato &= ~detail;
  }

  // Controlla un dettaglio
  public hasDetail(detail: GG_ResultStato): boolean {
    return (this.stato & detail) !== 0;
  }

  // Legge lo stato principale
  public getState(): GG_ResultStato {
    return this.stato & GG_ResultStato.STATE_MASK;
  }

  // Cast a integer
  public toInt(): number {
    return this.stato;
  }
}


//const r = new Dip_GG_Result_Helper();

//r.setState(GG_ResultStato.Err);
//r.addDetail(GG_ResultStato.Err_1);
//r.addDetail(GG_ResultStato.Warning_2);

//console.log(r.toInt()); // esempio: 16 + 32 + 512 = 560

//console.log(r.getState()); // Err
//console.log(r.hasDetail(GG_ResultStato.Err_1)); // true
