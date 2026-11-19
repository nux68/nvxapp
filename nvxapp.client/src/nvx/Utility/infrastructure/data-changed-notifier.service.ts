import { Injectable } from '@angular/core';
import { Observable, Subject, filter } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApplicationType } from '../../ClientServer-Service/Infrastructure/Account/Models/user-load-model';
import { SignalrService } from './signalr.service';

/** payload dell'evento SignalR "DataChangedNotifier" (server: DataChangeNotifier) */
export interface DataChangedNotifierData {
  /** applicativo dei dati modificati (ApplicationType); null = dati comuni */
  application: ApplicationType | null;
  /** nome del comando eseguito (es. "ClockNow") */
  command: string;
}

/*
 Dati modificati da un comando eseguito fuori dalle pagine: oggi i comandi di un assistente AI esterno collegato
 tramite MCP (ANALISI_INFRASTRUCTURE_MCP.md, MCP-26). Il server manda l'evento SignalR "DataChangedNotifier" agli
 utenti dell'azienda; l'evento non porta dati, dice solo quale applicativo e' cambiato.

 Uso nelle pagine che mostrano dati modificabili da un comando:
   this.sub = this.dataChangedNotifierService.of(ApplicationType.AttendanceTracking).subscribe(() => this.load());
 (sottoscrizione in ionViewWillEnter, unsubscribe in ionViewWillLeave, come per RequestNotifierService.changed$).
 Le pagine delle richieste non devono fare nulla: RequestNotifierService ascolta lo stesso evento.
*/
@Injectable({
  providedIn: 'root'
})
export class DataChangedNotifierService {

  private readonly _changed = new Subject<DataChangedNotifierData>();
  public readonly changed$: Observable<DataChangedNotifierData> = this._changed.asObservable();

  constructor(signalrService: SignalrService) {
    if (environment.signalR.useSignalR) {
      // l'evento resta collegato anche alle connessioni successive (SignalrService.on)
      signalrService.on('DataChangedNotifier').subscribe((data: DataChangedNotifierData) => this._changed.next(data));
    }
  }

  /** modifiche ai dati di un applicativo (o ai dati comuni, che valgono per tutti) */
  public of(application: ApplicationType): Observable<DataChangedNotifierData> {
    return this.changed$.pipe(filter(x => x.application === null || x.application === undefined || x.application === application));
  }
}
