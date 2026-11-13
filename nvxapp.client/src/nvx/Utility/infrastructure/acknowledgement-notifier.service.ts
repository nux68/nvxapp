import { Injectable } from '@angular/core';
import { Observable, Subject } from 'rxjs';
import { environment } from '../../../environments/environment';
import { SignalrService } from './signalr.service';

/*
 Prese visione confermate (evento SignalR "AcknowledgementNotifier", ANALISI_INFRASTRUCTURE_NUOVI_MODULI.md INF-SVC.6):
 il server avvisa gli amministratori dell'azienda e il destinatario quando una presa visione viene confermata (pagina
 "Prese visione" o firma del documentale). Le pagine che ne mostrano lo stato si iscrivono a changed$ e si ricaricano
 se la fonte e' la loro (es. accordi di lavoro agile: "at.accordo-sw").
*/
@Injectable({
  providedIn: 'root'
})
export class AcknowledgementNotifierService {

  private readonly _changed = new Subject<AcknowledgementNotifierData>();
  /** presa visione appena confermata */
  public readonly changed$: Observable<AcknowledgementNotifierData> = this._changed.asObservable();

  constructor(signalrService: SignalrService) {
    if (environment.signalR.useSignalR) {
      // l'evento resta collegato anche alle connessioni successive (SignalrService.on)
      signalrService.on('AcknowledgementNotifier').subscribe((data: AcknowledgementNotifierData) => this._changed.next(data));
    }
  }
}

export class AcknowledgementNotifierData {
  public idCompany = 0;
  public application = 0;
  public fonte = '';
  public idRiferimento: number | null = null;
  public idAspNetUsers = '';
  public dataConferma: string | null = null;
}
