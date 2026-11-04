import { Injectable } from '@angular/core';
import { Observable, Subject } from 'rxjs';

/*
 Avvisi di modifica dei dati comuni dell'azienda (schema Common), per gli applicativi che ne tengono una copia
 (es. le presenze tengono i clienti in cache per le scelte): le pagine dell'infrastruttura avvisano dopo ogni
 salvataggio o cancellazione, gli applicativi si iscrivono e ricaricano.
*/
@Injectable({
  providedIn: 'root'
})
export class CommonDataEventsService {

  private readonly _clienti = new Subject<void>();

  /** clienti (o loro contatti) modificati */
  public get ClientiModificati$(): Observable<void> {
    return this._clienti.asObservable();
  }

  public ClientiModificati(): void {
    this._clienti.next();
  }
}
