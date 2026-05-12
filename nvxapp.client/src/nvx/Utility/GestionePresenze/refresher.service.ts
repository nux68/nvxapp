import { Injectable } from '@angular/core';
import { Observable, Subject } from 'rxjs';
import { AuthService } from '../infrastructure/auth.service';
import { SharedParameterGestionePresenzeService } from '../../shared/shared-parameter-gestione-presenze.service';

@Injectable({
  providedIn: 'root'
})
export class RefresherService {

  // Subject (non BehaviorSubject): non emette il valore iniziale alle nuove subscription,
  // evitando che i componenti chiamino LoadData() al solo fatto di iscriversi.
  private SharedParameterGestionePresenze_refreshSubject = new Subject<void>();
  private Dip_GG_Richiesta_refreshSubject = new Subject<void>();

  public Dip_GG_Richiesta_refresh$: Observable<void> = this.Dip_GG_Richiesta_refreshSubject.asObservable();
  public SharedParameterGestionePresenze_refresh$: Observable<void> = this.SharedParameterGestionePresenze_refreshSubject.asObservable();

  constructor(private authService: AuthService,
              private sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService) { }

  public Dip_GG_Richiesta_triggerRefresh(): void {
    this.Dip_GG_Richiesta_refreshSubject.next();
  }

  public SharedParameterGestionePresenze_triggerRefresh(): void {
    this.sharedParameterGestionePresenzeService.IsLoad = false;
    this.authService.forceRolesEmission();
    this.SharedParameterGestionePresenze_refreshSubject.next();
  }

}
