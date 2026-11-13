import { Injectable } from '@angular/core';
import { HttpClient, HttpContext } from '@angular/common/http';
import { NVX_SENZA_DIALOG_ERRORI } from '../../../http/http-interceptor';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import {
  TimbraturaEvoluta_ConfigGetInModel, TimbraturaEvoluta_ConfigGetOutModel, TimbraturaEvoluta_ConfigPutInModel, TimbraturaEvoluta_ConfigPutOutModel,
  TimbraturaEvoluta_KioskRegistraInModel, TimbraturaEvoluta_KioskRegistraOutModel, TimbraturaEvoluta_KioskStampInModel, TimbraturaEvoluta_KioskStampOutModel,
  TimbraturaEvoluta_KioskIdentificaInModel, TimbraturaEvoluta_KioskIdentificaOutModel, TimbraturaEvoluta_OpzioniInModel, TimbraturaEvoluta_OpzioniOutModel,
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

  // senzaDialogErrori: la pagina mostra da se' l'esito negativo (kiosk e terminali a schermo intero, dove il dialog comune
  // degli errori alla chiusura tornerebbe indietro)
  private post<I, O>(azione: string, model: GenericRequest<I>, senzaDialogErrori = false): Observable<GenericResult<O>> {
    return this.http.post<GenericResult<O>>(environment.remoteData.apiUri + 'TimbraturaEvoluta/' + azione, model,
      { context: new HttpContext().set(NVX_SENZA_DIALOG_ERRORI, senzaDialogErrori) });
  }

  Sedi(m: GenericRequest<TimbraturaEvoluta_SediInModel>, senzaDialogErrori = false) { return this.post<TimbraturaEvoluta_SediInModel, TimbraturaEvoluta_SediOutModel>('Sedi', m, senzaDialogErrori); }
  ConfigGet(m: GenericRequest<TimbraturaEvoluta_ConfigGetInModel>, senzaDialogErrori = false) { return this.post<TimbraturaEvoluta_ConfigGetInModel, TimbraturaEvoluta_ConfigGetOutModel>('ConfigGet', m, senzaDialogErrori); }
  ConfigPut(m: GenericRequest<TimbraturaEvoluta_ConfigPutInModel>, senzaDialogErrori = false) { return this.post<TimbraturaEvoluta_ConfigPutInModel, TimbraturaEvoluta_ConfigPutOutModel>('ConfigPut', m, senzaDialogErrori); }
  QrCode(m: GenericRequest<TimbraturaEvoluta_QrCodeInModel>, senzaDialogErrori = false) { return this.post<TimbraturaEvoluta_QrCodeInModel, TimbraturaEvoluta_QrCodeOutModel>('QrCode', m, senzaDialogErrori); }
  Stamp(m: GenericRequest<TimbraturaEvoluta_StampInModel>, senzaDialogErrori = false) { return this.post<TimbraturaEvoluta_StampInModel, TimbraturaEvoluta_StampOutModel>('Stamp', m, senzaDialogErrori); }
  KioskRegistra(m: GenericRequest<TimbraturaEvoluta_KioskRegistraInModel>, senzaDialogErrori = false) { return this.post<TimbraturaEvoluta_KioskRegistraInModel, TimbraturaEvoluta_KioskRegistraOutModel>('KioskRegistra', m, senzaDialogErrori); }
  Opzioni(m: GenericRequest<TimbraturaEvoluta_OpzioniInModel>, senzaDialogErrori = false) { return this.post<TimbraturaEvoluta_OpzioniInModel, TimbraturaEvoluta_OpzioniOutModel>('Opzioni', m, senzaDialogErrori); }
  KioskIdentifica(m: GenericRequest<TimbraturaEvoluta_KioskIdentificaInModel>, senzaDialogErrori = false) { return this.post<TimbraturaEvoluta_KioskIdentificaInModel, TimbraturaEvoluta_KioskIdentificaOutModel>('KioskIdentifica', m, senzaDialogErrori); }
  KioskStamp(m: GenericRequest<TimbraturaEvoluta_KioskStampInModel>, senzaDialogErrori = false) { return this.post<TimbraturaEvoluta_KioskStampInModel, TimbraturaEvoluta_KioskStampOutModel>('KioskStamp', m, senzaDialogErrori); }
  TerminaleSblocca(m: GenericRequest<TimbraturaEvoluta_TerminaleSbloccaInModel>, senzaDialogErrori = false) { return this.post<TimbraturaEvoluta_TerminaleSbloccaInModel, TimbraturaEvoluta_TerminaleSbloccaOutModel>('TerminaleSblocca', m, senzaDialogErrori); }
  PinGetAll(m: GenericRequest<TimbraturaEvoluta_PinGetAllInModel>, senzaDialogErrori = false) { return this.post<TimbraturaEvoluta_PinGetAllInModel, TimbraturaEvoluta_PinGetAllOutModel>('PinGetAll', m, senzaDialogErrori); }
  PinPut(m: GenericRequest<TimbraturaEvoluta_PinPutInModel>, senzaDialogErrori = false) { return this.post<TimbraturaEvoluta_PinPutInModel, TimbraturaEvoluta_PinPutOutModel>('PinPut', m, senzaDialogErrori); }
}
