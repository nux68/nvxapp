import { Injectable } from '@angular/core';
import { iRequestUiProvider, RequestNewItem, RequestUiAction } from './request-ui.model';
import { ApplicationType } from '../../ClientServer-Service/Infrastructure/Account/Models/user-load-model';

/*
 Azioni e pagine di invio delle richieste di Risorse aziendali nelle pagine comuni delle richieste
 (ANALISI_INFRASTRUCTURE_RICHIESTE.md). In Infrastructure e' vuoto: lo valorizza il ramo Assets.
 Le prenotazioni da approvare non hanno azioni proprie (si aprono con l'url della richiesta, /astbookings);
 "Nuova prenotazione" apre la pagina di modifica, che carica da sola risorse e limiti.
*/
@Injectable({
  providedIn: 'root'
})
export class RequestUiAssetsService implements iRequestUiProvider {
  public get Actions(): RequestUiAction[] { return []; }
  public get NewRequests(): RequestNewItem[] {
    return [
      { title: 'ast.requests.newBooking', url: '/astbookingedit', roles: ['CompanyPowerAdmin', 'CompanyAdmin', 'User'], application: ApplicationType.Assets },
    ];
  }
}
