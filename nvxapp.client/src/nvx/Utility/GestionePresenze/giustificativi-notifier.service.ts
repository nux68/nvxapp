import { Injectable } from '@angular/core';
import { Observable, Subject } from 'rxjs';
import { environment } from '../../../environments/environment';
import { GiustificativiNotifierData } from '../../ClientServer-Service/GestionePresenze/Dip_GG_Giustificativi/Models/dip-gg-giustificativi-model';
import { SignalrService } from '../infrastructure/signalr.service';

/*
 Evento SignalR "GiustificativiNotifier": un giustificativo e' stato inserito, modificato o cancellato
 (es. nel Calendario HR o dalla chat). changed$: le pagine aperte con quel mese (es. piano ferie) si ricaricano.
*/
@Injectable({
  providedIn: 'root'
})
export class GiustificativiNotifierService {

  private readonly _changed = new Subject<GiustificativiNotifierData>();
  public readonly changed$: Observable<GiustificativiNotifierData> = this._changed.asObservable();

  constructor(signalrService: SignalrService) {
    if (environment.signalR.useSignalR) {
      // l'evento resta collegato anche alle connessioni successive (SignalrService.on)
      signalrService.on('GiustificativiNotifier').subscribe((data: GiustificativiNotifierData) => this._changed.next(data));
    }
  }
}
