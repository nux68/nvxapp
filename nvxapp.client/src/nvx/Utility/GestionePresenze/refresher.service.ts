import { Injectable } from '@angular/core';
import { firstValueFrom, Observable, Subject } from 'rxjs';
import { ParTable, SharedParameterGestionePresenzeService } from '../../shared/shared-parameter-gestione-presenze.service';

@Injectable({
  providedIn: 'root'
})
export class RefresherService {

  // Subject (non BehaviorSubject): non emette il valore iniziale alle nuove subscription,
  // evitando che i componenti chiamino LoadData() al solo fatto di iscriversi.
  private SharedParameterGestionePresenze_refreshSubject = new Subject<ParTable[]>();
  private Dip_GG_Richiesta_refreshSubject = new Subject<void>();

  public Dip_GG_Richiesta_refresh$: Observable<void> = this.Dip_GG_Richiesta_refreshSubject.asObservable();
  /** tabelle della cache appena ricaricate */
  public SharedParameterGestionePresenze_refresh$: Observable<ParTable[]> = this.SharedParameterGestionePresenze_refreshSubject.asObservable();

  constructor(private sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService) { }

  public Dip_GG_Richiesta_triggerRefresh(): void {
    this.Dip_GG_Richiesta_refreshSubject.next();
  }

  /**
   * Dopo un salvataggio / una cancellazione: ricarica nella cache solo le tabelle modificate
   * (e quelle che ne dipendono), in background e senza il riquadro di caricamento.
   * Le liste che leggono dalla cache si aggiornano da sole.
   */
  public SharedParameterGestionePresenze_triggerRefresh(...tables: ParTable[]): Promise<void> {
    return firstValueFrom(this.sharedParameterGestionePresenzeService.Reload(...tables))
      .then(() => this.SharedParameterGestionePresenze_refreshSubject.next(tables));
  }

}
