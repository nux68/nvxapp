import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class RefresherService {

  private Dip_GG_Richiesta_refreshSubject = new BehaviorSubject<void>(undefined);

  // Observable a cui i componenti possono iscriversi
  public Dip_GG_Richiesta_refresh$: Observable<void> = this.Dip_GG_Richiesta_refreshSubject.asObservable();

  constructor() { }

  // Chiamare questo metodo per notificare il refresh
  public Dip_GG_Richiesta_triggerRefresh(): void {
    this.Dip_GG_Richiesta_refreshSubject.next();
  }
}
