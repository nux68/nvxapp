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

  private static readonly ALL_ERROR_DETAILS =
    GG_ResultStato.Err_TimbratureMancanti | GG_ResultStato.Err_2 | GG_ResultStato.Err_3 |
    GG_ResultStato.Err_4 | GG_ResultStato.Err_5 | GG_ResultStato.Err_6 |
    GG_ResultStato.Err_7 | GG_ResultStato.Err_8 | GG_ResultStato.Err_9 |
    GG_ResultStato.Err_10 | GG_ResultStato.Err_11 | GG_ResultStato.Err_12 |
    GG_ResultStato.Err_13 | GG_ResultStato.Err_14 | GG_ResultStato.Err_15 |
    GG_ResultStato.Err_16 | GG_ResultStato.Err_17 | GG_ResultStato.Err_18 |
    GG_ResultStato.Err_19 | GG_ResultStato.Err_20;

  private static readonly ALL_WARNING_DETAILS =
    GG_ResultStato.Warning_1 | GG_ResultStato.Warning_2;
  // Warning_3 → Warning_20 vanno aggiunti se usi BigInt


  // ────────────────────────────────────────────────────────────────
  // SetState
  // ────────────────────────────────────────────────────────────────
  static SetState(stato: GG_ResultStato, state: GG_ResultStato): GG_ResultStato {
    stato &= ~GG_ResultStato.STATE_MASK;
    stato |= state;
    return stato;
  }

  // ────────────────────────────────────────────────────────────────
  // AddDetail
  // ────────────────────────────────────────────────────────────────
  static AddDetail(stato: GG_ResultStato, detail: GG_ResultStato): GG_ResultStato {
    stato |= detail;

    if (this.IsErrorDetail(detail)) {
      stato |= GG_ResultStato.Err;
      stato &= ~GG_ResultStato.OK;
      stato &= ~GG_ResultStato.Init;
    }

    if (this.IsWarningDetail(detail)) {
      stato |= GG_ResultStato.Warning;
      stato &= ~GG_ResultStato.OK;
      stato &= ~GG_ResultStato.Init;
    }

    return stato;
  }

  // ────────────────────────────────────────────────────────────────
  // RemoveDetail
  // ────────────────────────────────────────────────────────────────
  static RemoveDetail(stato: GG_ResultStato, detail: GG_ResultStato): GG_ResultStato {
    stato &= ~detail;

    if (this.IsErrorDetail(detail)) {
      if (!this.HasAnyErrorDetail(stato))
        stato &= ~GG_ResultStato.Err;
    }

    if (this.IsWarningDetail(detail)) {
      if (!this.HasAnyWarningDetail(stato))
        stato &= ~GG_ResultStato.Warning;
    }

    if (!this.HasAnyErrorDetail(stato) && !this.HasAnyWarningDetail(stato)) {
      stato &= ~GG_ResultStato.STATE_MASK;
      stato |= GG_ResultStato.OK;
    }

    return stato;
  }

  // ────────────────────────────────────────────────────────────────
  static HasDetail(stato: GG_ResultStato, detail: GG_ResultStato): boolean {
    return (stato & detail) !== 0;
  }

  static GetState(stato: GG_ResultStato): GG_ResultStato {
    return stato & GG_ResultStato.STATE_MASK;
  }

  static Combine(...sources: GG_ResultStato[]): GG_ResultStato {
    let result = 0;

    for (const s of sources)
      result |= s & (this.ALL_ERROR_DETAILS | this.ALL_WARNING_DETAILS);

    if (this.HasAnyErrorDetail(result)) {
      result |= GG_ResultStato.Err;
      result &= ~GG_ResultStato.OK;
      result &= ~GG_ResultStato.Init;
    }
    else if (this.HasAnyWarningDetail(result)) {
      result |= GG_ResultStato.Warning;
      result &= ~GG_ResultStato.OK;
      result &= ~GG_ResultStato.Init;
    }
    else {
      result |= GG_ResultStato.OK;
    }

    return result;
  }

  static ToLong(stato: GG_ResultStato): number {
    return stato;
  }

  // ── helper privati ──────────────────────────────────────────────

  private static IsErrorDetail(value: GG_ResultStato): boolean {
    return (value & this.ALL_ERROR_DETAILS) !== 0;
  }

  private static IsWarningDetail(value: GG_ResultStato): boolean {
    return (value & this.ALL_WARNING_DETAILS) !== 0;
  }

  private static HasAnyErrorDetail(stato: GG_ResultStato): boolean {
    return (stato & this.ALL_ERROR_DETAILS) !== 0;
  }

  private static HasAnyWarningDetail(stato: GG_ResultStato): boolean {
    return (stato & this.ALL_WARNING_DETAILS) !== 0;
  }
}


//const r = new Dip_GG_Result_Helper();

//r.setState(GG_ResultStato.Err);
//r.addDetail(GG_ResultStato.Err_1);
//r.addDetail(GG_ResultStato.Warning_2);

//console.log(r.toInt()); // esempio: 16 + 32 + 512 = 560

//console.log(r.getState()); // Err
//console.log(r.hasDetail(GG_ResultStato.Err_1)); // true
