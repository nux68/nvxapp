import { Injectable } from '@angular/core';
import { iRequestUiProvider, RequestNewItem, RequestUiAction } from './request-ui.model';

/*
 Azioni e pagine di invio delle richieste di Risorse aziendali nelle pagine comuni delle richieste
 (ANALISI_INFRASTRUCTURE_RICHIESTE.md). In Infrastructure e' vuoto: lo valorizza il ramo Assets.
*/
@Injectable({
  providedIn: 'root'
})
export class RequestUiAssetsService implements iRequestUiProvider {
  public get Actions(): RequestUiAction[] { return []; }
  public get NewRequests(): RequestNewItem[] { return []; }
}
