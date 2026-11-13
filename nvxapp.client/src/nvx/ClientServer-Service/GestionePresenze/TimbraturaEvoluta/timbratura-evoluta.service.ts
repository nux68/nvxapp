import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import {
  TimbraturaEvoluta_ConfigGetInModel, TimbraturaEvoluta_ConfigGetOutModel, TimbraturaEvoluta_ConfigPutInModel, TimbraturaEvoluta_ConfigPutOutModel,
  TimbraturaEvoluta_KioskRegistraInModel, TimbraturaEvoluta_KioskRegistraOutModel, TimbraturaEvoluta_KioskStampInModel, TimbraturaEvoluta_KioskStampOutModel,
  TimbraturaEvoluta_PinGetAllInModel, TimbraturaEvoluta_PinGetAllOutModel, TimbraturaEvoluta_PinPutInModel, TimbraturaEvoluta_PinPutOutModel,
  TimbraturaEvoluta_QrCodeInModel, TimbraturaEvoluta_QrCodeOutModel, TimbraturaEvoluta_SediInModel, TimbraturaEvoluta_SediOutModel,
  TimbraturaEvoluta_StampInModel, TimbraturaEvoluta_StampOutModel, TimbraturaEvoluta_TerminaleSbloccaInModel, TimbraturaEvoluta_TerminaleSbloccaOutModel
} from './Models/timbratura-evoluta-model';

// Timbratura evoluta (AT-TIMB): configurazione delle sedi, QR della postazione, timbratura con QR/posizione/offline, kiosk, PIN
@Injectable({
  providedIn: 'root'
})
export class TimbraturaEvolutaService {

  constructor(private http: HttpClient) { }

  private post<I, O>(azione: string, model: GenericRequest<I>): Observable<GenericResult<O>> {
    return this.http.post<GenericResult<O>>(environment.remoteData.apiUri + 'TimbraturaEvoluta/' + azione, model);
  }

  Sedi(m: GenericRequest<TimbraturaEvoluta_SediInModel>) { return this.post<TimbraturaEvoluta_SediInModel, TimbraturaEvoluta_SediOutModel>('Sedi', m); }
  ConfigGet(m: GenericRequest<TimbraturaEvoluta_ConfigGetInModel>) { return this.post<TimbraturaEvoluta_ConfigGetInModel, TimbraturaEvoluta_ConfigGetOutModel>('ConfigGet', m); }
  ConfigPut(m: GenericRequest<TimbraturaEvoluta_ConfigPutInModel>) { return this.post<TimbraturaEvoluta_ConfigPutInModel, TimbraturaEvoluta_ConfigPutOutModel>('ConfigPut', m); }
  QrCode(m: GenericRequest<TimbraturaEvoluta_QrCodeInModel>) { return this.post<TimbraturaEvoluta_QrCodeInModel, TimbraturaEvoluta_QrCodeOutModel>('QrCode', m); }
  Stamp(m: GenericRequest<TimbraturaEvoluta_StampInModel>) { return this.post<TimbraturaEvoluta_StampInModel, TimbraturaEvoluta_StampOutModel>('Stamp', m); }
  KioskRegistra(m: GenericRequest<TimbraturaEvoluta_KioskRegistraInModel>) { return this.post<TimbraturaEvoluta_KioskRegistraInModel, TimbraturaEvoluta_KioskRegistraOutModel>('KioskRegistra', m); }
  KioskStamp(m: GenericRequest<TimbraturaEvoluta_KioskStampInModel>) { return this.post<TimbraturaEvoluta_KioskStampInModel, TimbraturaEvoluta_KioskStampOutModel>('KioskStamp', m); }
  TerminaleSblocca(m: GenericRequest<TimbraturaEvoluta_TerminaleSbloccaInModel>) { return this.post<TimbraturaEvoluta_TerminaleSbloccaInModel, TimbraturaEvoluta_TerminaleSbloccaOutModel>('TerminaleSblocca', m); }
  PinGetAll(m: GenericRequest<TimbraturaEvoluta_PinGetAllInModel>) { return this.post<TimbraturaEvoluta_PinGetAllInModel, TimbraturaEvoluta_PinGetAllOutModel>('PinGetAll', m); }
  PinPut(m: GenericRequest<TimbraturaEvoluta_PinPutInModel>) { return this.post<TimbraturaEvoluta_PinPutInModel, TimbraturaEvoluta_PinPutOutModel>('PinPut', m); }
}
